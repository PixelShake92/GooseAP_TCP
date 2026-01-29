using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using BepInEx.Logging;
using UnityEngine;

namespace GooseGameAP
{
    public class ArchipelagoClient
    {
        private static ManualLogSource Log => Plugin.Log;
        private Plugin plugin;
        
        private TcpClient tcp;
        private StreamReader reader;
        private StreamWriter writer;
        private Thread receiveThread;
        private System.Diagnostics.Process proxyProcess;
        
        private const int LOCAL_PROXY_PORT = 38282;
        
        private Queue<string> messageQueue = new Queue<string>();
        private int lastProcessedIndex = -1;
        private int playerSlot = 0;
        private string slotName = "";
        private int slotNumber = 0;
        
        // DataPackage accumulation for large packages
        private StringBuilder dataPackageBuffer = new StringBuilder();
        private bool accumulatingDataPackage = false;
        
        // Name lookups from server
        private Dictionary<int, string> playerNames = new Dictionary<int, string>();
        private Dictionary<int, string> playerGames = new Dictionary<int, string>();
        
        // Datapackage - item and location names per game
        private Dictionary<string, Dictionary<long, string>> gameItemNames = new Dictionary<string, Dictionary<long, string>>();
        private Dictionary<string, Dictionary<long, string>> gameLocationNames = new Dictionary<string, Dictionary<long, string>>();
        private bool hasDatapackage = false;
        
        // Server log messages
        private List<ServerLogEntry> serverLog = new List<ServerLogEntry>();
        private const int MAX_LOG_ENTRIES = 100;
        
        public bool IsConnected { get; private set; } = false;
        
        // Slot data options
        public bool NPCSoulsEnabled { get; private set; } = true;
        public bool PropSoulsEnabled { get; private set; } = true;
        public bool NewTasksEnabled { get; private set; } = true;
        public bool DeathLinkEnabled { get; private set; } = false;
        
        // Gate sync timing
        public bool PendingGateSync { get; set; } = false;
        public float GateSyncTimer { get; set; } = 0f;
        public int GateSyncAttempts { get; set; } = 0;
        public const int MAX_GATE_SYNC_ATTEMPTS = 3;
        public bool WaitingForReceivedItems { get; set; } = false;
        public float ReceivedItemsTimeout { get; set; } = 0f;
        
        public IReadOnlyList<ServerLogEntry> ServerLog => serverLog;
        
        public ArchipelagoClient(Plugin plugin)
        {
            this.plugin = plugin;
        }
        
        public void Connect(string serverAddress, string serverPort, string slot, string password, bool deathLinkEnabled)
        {
            try
            {
                plugin.UI.Status = "Starting proxy...";
                slotName = slot;
                StopProxy();
                
                string gameDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string proxyPath = FindProxyPath(gameDir);
                
                if (proxyPath == null)
                {
                    plugin.UI.Status = "ERROR: APProxy.exe not found!";
                    Log.LogError("APProxy.exe not found. Searched near: " + gameDir);
                    plugin.UI.AddChatMessage("Place APProxy.exe in BepInEx/plugins/");
                    return;
                }
                
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = proxyPath,
                    Arguments = serverAddress + " " + serverPort + " " + LOCAL_PROXY_PORT,
                    WorkingDirectory = Path.GetDirectoryName(proxyPath),
                    UseShellExecute = true,
                    CreateNoWindow = false
                };
                
                proxyProcess = System.Diagnostics.Process.Start(startInfo);
                Thread.Sleep(1500);
                
                if (proxyProcess == null || proxyProcess.HasExited)
                {
                    plugin.UI.Status = "ERROR: Proxy failed to start!";
                    return;
                }
                
                plugin.UI.Status = "Connecting to proxy...";
                tcp = new TcpClient();
                
                int attempts = 0;
                while (attempts < 5)
                {
                    try
                    {
                        tcp.Connect("127.0.0.1", LOCAL_PROXY_PORT);
                        break;
                    }
                    catch
                    {
                        attempts++;
                        if (attempts >= 5) throw;
                        Thread.Sleep(500);
                    }
                }
                
                var stream = tcp.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8);
                writer.AutoFlush = true;

                plugin.UI.Status = "Waiting for server...";

                receiveThread = new Thread(() => ReceiveLoop(slot, password, deathLinkEnabled, serverAddress, serverPort));
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                plugin.UI.Status = "Error: " + ex.Message;
                Log.LogError("Connect error: " + ex);
            }
        }
        
        private string FindProxyPath(string gameDir)
        {
            string[] searchPaths = {
                Path.Combine(gameDir, "APProxy.exe"),
                Path.Combine(gameDir, "..", "APProxy.exe"),
                Path.Combine(gameDir, "..", "..", "APProxy", "APProxy.exe"),
                Path.Combine(gameDir, "..", "..", "..", "APProxy.exe")
            };
            
            foreach (var path in searchPaths)
            {
                string fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }
        
        public void Disconnect()
        {
            try
            {
                receiveThread?.Abort();
                reader?.Close();
                writer?.Close();
                tcp?.Close();
            }
            catch { }
            tcp = null;
            IsConnected = false;
            StopProxy();
            plugin.UI.Status = "Disconnected";
        }
        
        private void StopProxy()
        {
            try
            {
                if (proxyProcess != null && !proxyProcess.HasExited)
                {
                    proxyProcess.Kill();
                    proxyProcess.Dispose();
                }
            }
            catch { }
            proxyProcess = null;
        }
        
        public void SendPacket(string json)
        {
            try { writer?.WriteLine(json); }
            catch { }
        }
        
        public void SendLocationCheck(long locationId)
        {
            if (!IsConnected) return;
            string json = "[{\"cmd\":\"LocationChecks\",\"locations\":[" + locationId + "]}]";
            SendPacket(json);
            Log.LogInfo($"[LOC-APCLIENT] Sent location check: {locationId} = {LocationMappings.GetLocationName(locationId)}");
        }
        
        public void SendGoalComplete()
        {
            if (!IsConnected) return;
            string json = "[{\"cmd\":\"StatusUpdate\",\"status\":30}]";
            SendPacket(json);
            AddServerLog("[GOAL] Victory! Goal complete sent!", ServerLogType.Goal);
            plugin.UI.ShowNotification("Victory! Goal complete sent to Archipelago!");
        }

        public void SendDeathLink(string nameOfShooer)
        {
            if (!IsConnected) return;
            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
            string json = "[{\"cmd\":\"Bounce\",\"tags\":[\"DeathLink\"],\"data\":{\"time\":" + unixTime + ",\"source\":\"" + slotName + "\",\"cause\":\"Shooed by " + nameOfShooer + "!\"}}]";
            SendPacket(json);
            Log.LogInfo("[APCLIENT] Sent DeathLink");
        }
        
        public void ProcessQueuedMessages()
        {
            lock (messageQueue)
            {
                while (messageQueue.Count > 0)
                {
                    ProcessMessage(messageQueue.Dequeue());
                }
            }
        }
        
        public void ClearReceivedItems()
        {
            lastProcessedIndex = -1;
            PlayerPrefs.SetInt("AP_LastItemIndex", -1);
            PlayerPrefs.Save();
            plugin.UI.ClearReceivedItems();
        }
        
        public void ClearServerLog()
        {
            serverLog.Clear();
        }
        
        private void ReceiveLoop(string slot, string password, bool deathLinkEnabled, string serverAddress, string serverPort)
        {
            try
            {
                string roomInfo = reader.ReadLine();
                if (roomInfo == null) return;
                
                Log.LogInfo("[AP] Got RoomInfo");
                
                string tags = deathLinkEnabled ? "[\"DeathLink\"]" : "[]";
                string passStr = string.IsNullOrEmpty(password) ? "null" : "\"" + password + "\"";
                string connectPacket = "[{\"cmd\":\"Connect\",\"game\":\"Untitled Goose Game\",\"name\":\"" + slot + 
                    "\",\"uuid\":\"\",\"version\":{\"major\":0,\"minor\":5,\"build\":0,\"class\":\"Version\"},\"tags\":" + 
                    tags + ",\"password\":" + passStr + ",\"items_handling\":7}]";
                
                writer.WriteLine(connectPacket);
                Log.LogInfo($"[AP] Sent Connect packet. DeathLink={deathLinkEnabled}");
                
                while (tcp != null && tcp.Connected)
                {
                    string line = reader.ReadLine();
                    if (line == null) break;
                    
                    lock (messageQueue)
                    {
                        messageQueue.Enqueue(line);
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                Log.LogError("[AP] Receive error: " + ex);
            }
            
            IsConnected = false;
        }
        
        private void ProcessMessage(string data)
        {
            // Handle DataPackage accumulation - large packages come in multiple chunks
            if (data.Contains("\"cmd\":\"DataPackage\""))
            {
                // Start accumulating
                accumulatingDataPackage = true;
                dataPackageBuffer.Clear();
                dataPackageBuffer.Append(data);
                
                // Check if this chunk completes the DataPackage (ends with }])
                if (IsCompleteJson(dataPackageBuffer.ToString()))
                {
                    accumulatingDataPackage = false;
                    ParseDataPackage(dataPackageBuffer.ToString());
                    dataPackageBuffer.Clear();
                }
                else
                {
                    Log.LogInfo("[AP] DataPackage incomplete, waiting for more data...");
                }
                return;
            }
            
            // Continue accumulating if we're in the middle of a DataPackage
            if (accumulatingDataPackage)
            {
                dataPackageBuffer.Append(data);
                
                // Check if complete
                if (IsCompleteJson(dataPackageBuffer.ToString()))
                {
                    accumulatingDataPackage = false;
                    Log.LogInfo($"[AP] DataPackage complete, total size: {dataPackageBuffer.Length}");
                    ParseDataPackage(dataPackageBuffer.ToString());
                    dataPackageBuffer.Clear();
                }
                return;
            }
            
            if (data.Contains("\"cmd\":\"Connected\""))
            {
                IsConnected = true;
                plugin.UI.Status = "CONNECTED!";
                AddServerLog("[CONNECTED] Successfully connected to Archipelago!", ServerLogType.System);
                
                int idx = data.IndexOf("\"slot\":");
                if (idx > 0)
                {
                    int start = idx + 7;
                    int end = data.IndexOf(",", start);
                    if (end > start)
                    {
                        int.TryParse(data.Substring(start, end - start), out playerSlot);
                        slotNumber = playerSlot;
                    }
                }
                
                ParsePlayerNames(data);
                ParseSlotData(data);
                ParseCheckedLocations(data);  // Load already-checked locations from server
                
                lastProcessedIndex = PlayerPrefs.GetInt("AP_LastItemIndex", -1);
                Log.LogInfo($"[AP] Connected - lastProcessedIndex = {lastProcessedIndex}");
                
                GateSyncTimer = 0f;
                GateSyncAttempts = 0;
                PendingGateSync = true;
                WaitingForReceivedItems = true;
                ReceivedItemsTimeout = 0f;
                
                RequestDataPackage();
                SendPacket("[{\"cmd\":\"Sync\"}]");
                plugin.OnConnectionComplete();
            }
            else if (data.Contains("\"cmd\":\"ReceivedItems\""))
            {
                WaitingForReceivedItems = false;
                ParseReceivedItems(data);
                PendingGateSync = true;
                GateSyncTimer = 0f;
                GateSyncAttempts = 0;
            }
            else if (data.Contains("\"cmd\":\"PrintJSON\""))
            {
                ParsePrintJSON(data);
            }
            else if (data.Contains("\"cmd\":\"Bounced\"") && data.Contains("\"DeathLink\""))
            {
                if (plugin.DeathLinkEnabled)
                {
                    if (!data.Contains("\"source\":" + slotName))
                    {
                        AddServerLog("[DEATHLINK] Another player died!", ServerLogType.DeathLink);
                        plugin.TriggerDeathLink();
                    }
                }
            }
            else if (data.Contains("\"cmd\":\"ConnectionRefused\""))
            {
                plugin.UI.Status = "Connection Refused!";
                AddServerLog("[ERROR] Connection refused by server", ServerLogType.Error);
            }
        }
        
        private void RequestDataPackage()
        {
            string packet = "[{\"cmd\":\"GetDataPackage\"}]";
            SendPacket(packet);
            Log.LogInfo("[AP] Requested DataPackage (v4)");
        }
        
        private bool IsCompleteJson(string json)
        {
            // A complete JSON array should have balanced brackets
            // DataPackage format is: [{"cmd":"DataPackage",...}]
            if (string.IsNullOrEmpty(json)) return false;
            
            int bracketCount = 0;
            int braceCount = 0;
            bool inString = false;
            
            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                
                // Handle string escaping
                if (c == '"' && (i == 0 || json[i - 1] != '\\'))
                {
                    inString = !inString;
                    continue;
                }
                
                if (inString) continue;
                
                if (c == '[') bracketCount++;
                else if (c == ']') bracketCount--;
                else if (c == '{') braceCount++;
                else if (c == '}') braceCount--;
            }
            
            return bracketCount == 0 && braceCount == 0 && json.TrimEnd().EndsWith("]");
        }
        
        private void ParseDataPackage(string data)
        {
            try
            {
                Log.LogInfo("[AP] Parsing DataPackage...");
                Log.LogInfo($"[AP] DataPackage length: {data.Length}");
                
                int totalItems = 0;
                int totalLocations = 0;
                int gamesFound = 0;
                
                // Find each game by looking for "item_name_to_id" sections
                int searchPos = 0;
                while (true)
                {
                    int itemNameToIdIdx = data.IndexOf("\"item_name_to_id\":", searchPos);
                    if (itemNameToIdIdx < 0) break;
                    
                    // Find game name by looking backwards for the pattern "GameName":{ before item_name_to_id
                    string gameName = FindGameNameBefore(data, itemNameToIdIdx);
                    if (string.IsNullOrEmpty(gameName) || gameItemNames.ContainsKey(gameName))
                    {
                        searchPos = itemNameToIdIdx + 20;
                        continue;
                    }
                    
                    Log.LogInfo($"[AP] Found game: {gameName}");
                    gamesFound++;
                    
                    gameItemNames[gameName] = new Dictionary<long, string>();
                    gameLocationNames[gameName] = new Dictionary<long, string>();
                    
                    // Parse item_name_to_id
                    int itemBraceStart = data.IndexOf("{", itemNameToIdIdx);
                    if (itemBraceStart > 0)
                    {
                        int itemBraceEnd = FindMatchingBrace(data, itemBraceStart);
                        if (itemBraceEnd > itemBraceStart)
                        {
                            ParseNameIdSection(data, itemBraceStart + 1, itemBraceEnd, gameItemNames[gameName]);
                        }
                    }
                    
                    // Find and parse location_name_to_id for this game
                    int locNameToIdIdx = data.IndexOf("\"location_name_to_id\":", itemNameToIdIdx);
                    int nextGameIdx = data.IndexOf("\"item_name_to_id\":", itemNameToIdIdx + 20);
                    
                    if (locNameToIdIdx > 0 && (nextGameIdx < 0 || locNameToIdIdx < nextGameIdx))
                    {
                        int locBraceStart = data.IndexOf("{", locNameToIdIdx);
                        if (locBraceStart > 0)
                        {
                            int locBraceEnd = FindMatchingBrace(data, locBraceStart);
                            if (locBraceEnd > locBraceStart)
                            {
                                ParseNameIdSection(data, locBraceStart + 1, locBraceEnd, gameLocationNames[gameName]);
                            }
                        }
                    }
                    
                    totalItems += gameItemNames[gameName].Count;
                    totalLocations += gameLocationNames[gameName].Count;
                    Log.LogInfo($"[AP] {gameName}: {gameItemNames[gameName].Count} items, {gameLocationNames[gameName].Count} locations");
                    
                    searchPos = itemNameToIdIdx + 20;
                }
                
                hasDatapackage = true;
                Log.LogInfo($"[AP] DataPackage complete: {gamesFound} games, {totalItems} items, {totalLocations} locations");
                AddServerLog($"[INFO] Loaded {totalItems} items, {totalLocations} locations from {gamesFound} games", ServerLogType.System);
            }
            catch (Exception ex)
            {
                Log.LogError($"[AP] ParseDataPackage error: {ex}");
                hasDatapackage = true;
                AddServerLog("[INFO] DataPackage parse error", ServerLogType.Error);
            }
        }
        
        private string FindGameNameBefore(string data, int position)
        {
            // Look backwards from position to find "GameName":{
            // We need to find a pattern like ..."GameName":{"item_name_to_id":...
            int braceCount = 0;
            int searchEnd = position;
            
            // First find the opening brace of this game's object
            for (int i = position - 1; i >= 0; i--)
            {
                if (data[i] == '}') braceCount++;
                else if (data[i] == '{')
                {
                    if (braceCount == 0)
                    {
                        // Found the opening brace, now find the game name before it
                        int colonIdx = -1;
                        for (int j = i - 1; j >= 0; j--)
                        {
                            if (data[j] == ':')
                            {
                                colonIdx = j;
                                break;
                            }
                            if (!char.IsWhiteSpace(data[j])) break;
                        }
                        
                        if (colonIdx > 0)
                        {
                            // Find the quoted string before the colon
                            int quoteEnd = -1;
                            for (int j = colonIdx - 1; j >= 0; j--)
                            {
                                if (data[j] == '"')
                                {
                                    quoteEnd = j;
                                    break;
                                }
                                if (!char.IsWhiteSpace(data[j])) break;
                            }
                            
                            if (quoteEnd > 0)
                            {
                                int quoteStart = data.LastIndexOf('"', quoteEnd - 1);
                                if (quoteStart >= 0)
                                {
                                    return data.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
                                }
                            }
                        }
                        break;
                    }
                    braceCount--;
                }
            }
            return null;
        }
        
        private void ParseNameIdSection(string data, int start, int end, Dictionary<long, string> dict)
        {
            int pos = start;
            while (pos < end)
            {
                // Find opening quote
                int quoteStart = data.IndexOf('"', pos);
                if (quoteStart < 0 || quoteStart >= end) break;
                
                // Find closing quote
                int quoteEnd = quoteStart + 1;
                while (quoteEnd < end && quoteEnd < data.Length)
                {
                    if (data[quoteEnd] == '"' && data[quoteEnd - 1] != '\\')
                        break;
                    quoteEnd++;
                }
                if (quoteEnd >= end) break;
                
                string name = data.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
                
                // Find colon
                int colonIdx = data.IndexOf(':', quoteEnd);
                if (colonIdx < 0 || colonIdx >= end) break;
                
                // Find number
                int numStart = colonIdx + 1;
                while (numStart < end && char.IsWhiteSpace(data[numStart]))
                    numStart++;
                
                int numEnd = numStart;
                if (numEnd < end && data[numEnd] == '-')
                    numEnd++;
                while (numEnd < end && char.IsDigit(data[numEnd]))
                    numEnd++;
                
                if (numEnd > numStart)
                {
                    string numStr = data.Substring(numStart, numEnd - numStart);
                    if (long.TryParse(numStr, out long id))
                    {
                        dict[id] = name;
                    }
                }
                
                pos = numEnd;
            }
        }
        
        private int FindMatchingBrace(string data, int openBraceIdx)
        {
            int depth = 0;
            for (int i = openBraceIdx; i < data.Length; i++)
            {
                if (data[i] == '{') depth++;
                else if (data[i] == '}')
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
            return -1;
        }
        
        private int FindMatchingBracket(string data, int openBracketIdx)
        {
            int depth = 0;
            for (int i = openBracketIdx; i < data.Length; i++)
            {
                if (data[i] == '[') depth++;
                else if (data[i] == ']')
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
            return -1;
        }
        
        private void ParsePrintJSON(string data)
        {
            try
            {
                StringBuilder message = new StringBuilder();
                string msgType = "chat";
                
                if (data.Contains("\"type\":\"ItemSend\"")) msgType = "item";
                else if (data.Contains("\"type\":\"Hint\"")) msgType = "hint";
                else if (data.Contains("\"type\":\"Join\"")) msgType = "join";
                else if (data.Contains("\"type\":\"Part\"")) msgType = "leave";
                else if (data.Contains("\"type\":\"Goal\"")) msgType = "goal";
                
                int dataIdx = data.IndexOf("\"data\":");
                if (dataIdx < 0) return;
                
                int arrStart = data.IndexOf("[", dataIdx);
                if (arrStart < 0) return;
                
                int arrEnd = FindMatchingBracket(data, arrStart);
                if (arrEnd < 0) arrEnd = data.Length;
                
                int pos = arrStart + 1;
                while (pos < arrEnd)
                {
                    int objStart = data.IndexOf("{", pos);
                    if (objStart < 0 || objStart >= arrEnd) break;
                    
                    int objEnd = data.IndexOf("}", objStart);
                    if (objEnd < 0 || objEnd >= arrEnd) break;
                    
                    string part = data.Substring(objStart, objEnd - objStart + 1);
                    string partType = ExtractJsonString(part, "type");
                    string text = ExtractJsonString(part, "text");
                    
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (partType == "player_id")
                        {
                            if (int.TryParse(text, out int playerId))
                                text = GetPlayerName(playerId);
                        }
                        else if (partType == "item_id")
                        {
                            if (long.TryParse(text, out long itemId))
                            {
                                string playerField = ExtractJsonString(part, "player");
                                if (!string.IsNullOrEmpty(playerField) && int.TryParse(playerField, out int itemPlayer))
                                    text = GetItemNameForPlayer(itemId, itemPlayer);
                                else
                                    text = GetItemName(itemId);
                            }
                        }
                        else if (partType == "location_id")
                        {
                            if (long.TryParse(text, out long locId))
                            {
                                string playerField = ExtractJsonString(part, "player");
                                if (!string.IsNullOrEmpty(playerField) && int.TryParse(playerField, out int locPlayer))
                                    text = GetLocationNameForPlayer(locId, locPlayer);
                                else
                                    text = GetLocationName(locId);
                            }
                        }
                        
                        message.Append(text);
                    }
                    
                    pos = objEnd + 1;
                }
                
                string fullMessage = message.ToString().Trim();
                if (!string.IsNullOrEmpty(fullMessage))
                {
                    ServerLogType logType = ServerLogType.Chat;
                    switch (msgType)
                    {
                        case "item": logType = ServerLogType.ItemReceived; break;
                        case "hint": logType = ServerLogType.Hint; break;
                        case "join": logType = ServerLogType.PlayerJoin; break;
                        case "leave": logType = ServerLogType.PlayerLeave; break;
                        case "goal": logType = ServerLogType.Goal; break;
                    }
                    AddServerLog(fullMessage, logType);
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[AP] ParsePrintJSON error: {ex}");
            }
        }
        
        private string ExtractJsonString(string json, string key)
        {
            string searchKey = "\"" + key + "\":";
            int idx = json.IndexOf(searchKey);
            if (idx < 0) return null;
            
            int valueStart = idx + searchKey.Length;
            while (valueStart < json.Length && char.IsWhiteSpace(json[valueStart]))
                valueStart++;
            
            if (valueStart >= json.Length) return null;
            
            if (json[valueStart] == '"')
            {
                valueStart++;
                int valueEnd = json.IndexOf("\"", valueStart);
                if (valueEnd > valueStart)
                    return json.Substring(valueStart, valueEnd - valueStart);
            }
            else if (char.IsDigit(json[valueStart]) || json[valueStart] == '-')
            {
                int valueEnd = valueStart;
                if (valueEnd < json.Length && json[valueEnd] == '-')
                    valueEnd++;
                while (valueEnd < json.Length && char.IsDigit(json[valueEnd]))
                    valueEnd++;
                return json.Substring(valueStart, valueEnd - valueStart);
            }
            
            return null;
        }
        
        private void AddServerLog(string message, ServerLogType type)
        {
            serverLog.Add(new ServerLogEntry(message, type, DateTime.Now));
            while (serverLog.Count > MAX_LOG_ENTRIES)
                serverLog.RemoveAt(0);
            Log.LogInfo($"[AP-LOG] {message}");
        }
        
        public string GetPlayerName(int slot)
        {
            if (playerNames.TryGetValue(slot, out string name))
                return name;
            return $"Player {slot}";
        }
        
        public string GetPlayerGame(int slot)
        {
            if (playerGames.TryGetValue(slot, out string game))
                return game;
            return null;
        }
        
        public string GetItemNameForPlayer(long itemId, int playerSlot)
        {
            string game = GetPlayerGame(playerSlot);
            if (!string.IsNullOrEmpty(game) && gameItemNames.TryGetValue(game, out var items))
            {
                if (items.TryGetValue(itemId, out string name))
                    return name;
            }
            return GetItemName(itemId);
        }
        
        public string GetLocationNameForPlayer(long locationId, int playerSlot)
        {
            string game = GetPlayerGame(playerSlot);
            if (!string.IsNullOrEmpty(game) && gameLocationNames.TryGetValue(game, out var locs))
            {
                if (locs.TryGetValue(locationId, out string name))
                    return name;
            }
            return GetLocationName(locationId);
        }
        
        public string GetItemName(long itemId)
        {
            string localName = LocationMappings.GetItemName(itemId);
            if (!localName.StartsWith("Unknown"))
                return localName;
            
            foreach (var gameItems in gameItemNames.Values)
            {
                if (gameItems.TryGetValue(itemId, out string name))
                    return name;
            }
            
            return localName;
        }
        
        public string GetLocationName(long locationId)
        {
            string localName = LocationMappings.GetLocationName(locationId);
            if (!localName.StartsWith("Unknown"))
                return localName;
            
            foreach (var gameLocs in gameLocationNames.Values)
            {
                if (gameLocs.TryGetValue(locationId, out string name))
                    return name;
            }
            
            return localName;
        }
        
        private void ParseSlotData(string data)
        {
            NPCSoulsEnabled = true;
            PropSoulsEnabled = true;
            
            int slotDataIdx = data.IndexOf("\"slot_data\":");
            if (slotDataIdx < 0)
            {
                Log.LogInfo("[AP] No slot_data found, using defaults");
                return;
            }
            
            int npcSoulsIdx = data.IndexOf("\"include_npc_souls\":", slotDataIdx);
            if (npcSoulsIdx > 0)
            {
                int colonPos = npcSoulsIdx + 20;
                string valueArea = data.Substring(colonPos, Math.Min(10, data.Length - colonPos)).Trim();
                NPCSoulsEnabled = valueArea.StartsWith("true") || valueArea.StartsWith("1");
                Log.LogInfo($"[AP] Parsed include_npc_souls: {NPCSoulsEnabled}");
            }
            
            int propSoulsIdx = data.IndexOf("\"include_prop_souls\":", slotDataIdx);
            if (propSoulsIdx > 0)
            {
                int colonPos = propSoulsIdx + 21;
                string valueArea = data.Substring(colonPos, Math.Min(10, data.Length - colonPos)).Trim();
                PropSoulsEnabled = valueArea.StartsWith("true") || valueArea.StartsWith("1");
                Log.LogInfo($"[AP] Parsed include_prop_souls: {PropSoulsEnabled}");
            }
            
            int newTasksIdx = data.IndexOf("\"include_new_tasks\":", slotDataIdx);
            if (newTasksIdx > 0)
            {
                int colonPos = newTasksIdx + 20;
                string valueArea = data.Substring(colonPos, Math.Min(10, data.Length - colonPos)).Trim();
                NewTasksEnabled = valueArea.StartsWith("true") || valueArea.StartsWith("1");
                Log.LogInfo($"[AP] Parsed include_new_tasks: {NewTasksEnabled}");
            }
            
            int deathLinkIdx = data.IndexOf("\"death_link\":", slotDataIdx);
            if (deathLinkIdx > 0)
            {
                int colonPos = deathLinkIdx + 13;
                string valueArea = data.Substring(colonPos, Math.Min(10, data.Length - colonPos)).Trim();
                DeathLinkEnabled = valueArea.StartsWith("true") || valueArea.StartsWith("1");
                Log.LogInfo($"[AP] Parsed death_link: {DeathLinkEnabled}");
            }
            
            Log.LogInfo($"[AP] Slot data parsed: NPCSouls={NPCSoulsEnabled}, PropSouls={PropSoulsEnabled}, NewTasks={NewTasksEnabled}, DeathLink={DeathLinkEnabled}");
            
            PlayerPrefs.SetInt("AP_NPCSoulsEnabled", NPCSoulsEnabled ? 1 : 0);
            PlayerPrefs.SetInt("AP_PropSoulsEnabled", PropSoulsEnabled ? 1 : 0);
            PlayerPrefs.SetInt("AP_NewTasksEnabled", NewTasksEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        public void LoadSavedSoulSettings()
        {
            NPCSoulsEnabled = PlayerPrefs.GetInt("AP_NPCSoulsEnabled", 1) == 1;
            PropSoulsEnabled = PlayerPrefs.GetInt("AP_PropSoulsEnabled", 1) == 1;
            NewTasksEnabled = PlayerPrefs.GetInt("AP_NewTasksEnabled", 1) == 1;
            Log.LogInfo($"[AP] Loaded soul settings: NPCSouls={NPCSoulsEnabled}, PropSouls={PropSoulsEnabled}, NewTasks={NewTasksEnabled}");
        }
        
        /// <summary>
        /// Parse checked_locations from Connected packet to know what's already been sent to server
        /// Format: "checked_locations":[123456,123457,...]
        /// </summary>
        private void ParseCheckedLocations(string data)
        {
            int idx = data.IndexOf("\"checked_locations\":");
            if (idx < 0)
            {
                Log.LogInfo("[AP] No checked_locations in Connected packet");
                return;
            }
            
            int arrStart = data.IndexOf("[", idx);
            if (arrStart < 0) return;
            
            int arrEnd = data.IndexOf("]", arrStart);
            if (arrEnd < 0) return;
            
            string arrContent = data.Substring(arrStart + 1, arrEnd - arrStart - 1);
            if (string.IsNullOrEmpty(arrContent.Trim()))
            {
                Log.LogInfo("[AP] checked_locations is empty");
                return;
            }
            
            List<long> locationIds = new List<long>();
            string[] parts = arrContent.Split(',');
            foreach (string part in parts)
            {
                string trimmed = part.Trim();
                if (long.TryParse(trimmed, out long locationId))
                {
                    locationIds.Add(locationId);
                }
            }
            
            if (locationIds.Count > 0)
            {
                plugin.LoadCheckedLocations(locationIds);
            }
        }
        
        private void ParseReceivedItems(string data)
        {
            int startingIndex = 0;
            int indexPos = data.IndexOf("\"index\":");
            if (indexPos > 0)
            {
                int start = indexPos + 8;
                int end = data.IndexOf(",", start);
                if (end > start)
                {
                    string indexStr = data.Substring(start, end - start).Trim();
                    int.TryParse(indexStr, out startingIndex);
                }
            }
            
            Log.LogInfo($"[AP] ParseReceivedItems - startingIndex={startingIndex}, lastProcessedIndex={lastProcessedIndex}");
            
            int pos = 0;
            int currentIndex = startingIndex;
            int newItemsCount = 0;
            
            while ((pos = data.IndexOf("\"item\":", pos + 1)) > 0)
            {
                int start = pos + 7;
                int end = data.IndexOf(",", start);
                if (end < 0) end = data.IndexOf("}", start);
                
                if (end > start)
                {
                    string idStr = data.Substring(start, end - start).Trim();
                    if (long.TryParse(idStr, out long itemId))
                    {
                        string itemName = GetItemName(itemId);
                        
                        if (currentIndex > lastProcessedIndex)
                        {
                            plugin.UI.AddReceivedItem(itemName);
                            plugin.ProcessReceivedItem(itemId);
                            newItemsCount++;
                            
                            AddServerLog($"[RECV] {itemName}", ServerLogType.ItemReceived);
                            
                            lastProcessedIndex = currentIndex;
                            PlayerPrefs.SetInt("AP_LastItemIndex", lastProcessedIndex);
                            PlayerPrefs.Save();
                        }
                        
                        currentIndex++;
                    }
                }
            }
            
            Log.LogInfo($"[AP] ParseReceivedItems - processed {newItemsCount} new items, lastProcessedIndex now {lastProcessedIndex}");
        }
        
        private void ParsePlayerNames(string data)
        {
            playerNames.Clear();
            playerGames.Clear();
            
            int playersIdx = data.IndexOf("\"players\":");
            if (playersIdx < 0) return;
            
            int arrStart = data.IndexOf("[", playersIdx);
            if (arrStart < 0) return;
            
            int arrEnd = FindMatchingBracket(data, arrStart);
            if (arrEnd < 0) return;
            
            string playersArr = data.Substring(arrStart, arrEnd - arrStart + 1);
            
            int pos = 0;
            while (pos < playersArr.Length)
            {
                int objStart = playersArr.IndexOf("{", pos);
                if (objStart < 0) break;
                
                int objEnd = playersArr.IndexOf("}", objStart);
                if (objEnd < 0) break;
                
                string obj = playersArr.Substring(objStart, objEnd - objStart + 1);
                
                int slot = 0;
                int slotIdx = obj.IndexOf("\"slot\":");
                if (slotIdx >= 0)
                {
                    int start = slotIdx + 7;
                    int end = start;
                    while (end < obj.Length && (char.IsDigit(obj[end]) || obj[end] == '-'))
                        end++;
                    int.TryParse(obj.Substring(start, end - start), out slot);
                }
                
                string name = "";
                int aliasIdx = obj.IndexOf("\"alias\":\"");
                if (aliasIdx >= 0)
                {
                    int start = aliasIdx + 9;
                    int end = obj.IndexOf("\"", start);
                    if (end > start)
                        name = obj.Substring(start, end - start);
                }
                
                if (string.IsNullOrEmpty(name))
                {
                    int nameIdx = obj.IndexOf("\"name\":\"");
                    if (nameIdx >= 0)
                    {
                        int start = nameIdx + 8;
                        int end = obj.IndexOf("\"", start);
                        if (end > start)
                            name = obj.Substring(start, end - start);
                    }
                }
                
                string game = "";
                int gameIdx = obj.IndexOf("\"game\":\"");
                if (gameIdx >= 0)
                {
                    int start = gameIdx + 8;
                    int end = obj.IndexOf("\"", start);
                    if (end > start)
                        game = obj.Substring(start, end - start);
                }
                
                if (slot >= 0 && !string.IsNullOrEmpty(name))
                {
                    playerNames[slot] = name;
                    if (!string.IsNullOrEmpty(game))
                        playerGames[slot] = game;
                }
                
                pos = objEnd + 1;
            }
            
            // Also parse slot_info for game names (newer AP protocol)
            // Format: "slot_info":{"1":{"game":"GameName",...},"2":{"game":"GameName",...}}
            int slotInfoIdx = data.IndexOf("\"slot_info\":");
            if (slotInfoIdx >= 0)
            {
                int slotInfoStart = data.IndexOf("{", slotInfoIdx);
                if (slotInfoStart >= 0)
                {
                    int slotInfoEnd = FindMatchingBrace(data, slotInfoStart);
                    if (slotInfoEnd > slotInfoStart)
                    {
                        string slotInfoStr = data.Substring(slotInfoStart, slotInfoEnd - slotInfoStart + 1);
                        
                        // Parse each slot entry - look for pattern "N":{"game":"..."}
                        int searchPos = 1;
                        while (searchPos < slotInfoStr.Length)
                        {
                            // Find slot number (quoted)
                            int quoteStart = slotInfoStr.IndexOf("\"", searchPos);
                            if (quoteStart < 0) break;
                            
                            int quoteEnd = slotInfoStr.IndexOf("\"", quoteStart + 1);
                            if (quoteEnd < 0) break;
                            
                            string slotStr = slotInfoStr.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
                            
                            // Check if this looks like a slot number and is followed by :
                            if (!int.TryParse(slotStr, out int slot))
                            {
                                searchPos = quoteEnd + 1;
                                continue;
                            }
                            
                            int colonIdx = slotInfoStr.IndexOf(":", quoteEnd);
                            if (colonIdx < 0 || colonIdx > quoteEnd + 2)
                            {
                                searchPos = quoteEnd + 1;
                                continue;
                            }
                            
                            int braceStart = slotInfoStr.IndexOf("{", colonIdx);
                            if (braceStart < 0 || braceStart > colonIdx + 2)
                            {
                                searchPos = quoteEnd + 1;
                                continue;
                            }
                            
                            int braceEnd = FindMatchingBrace(slotInfoStr, braceStart);
                            if (braceEnd < 0)
                            {
                                searchPos = braceStart + 1;
                                continue;
                            }
                            
                            string slotObj = slotInfoStr.Substring(braceStart, braceEnd - braceStart + 1);
                            
                            // Find game name in this slot object
                            int gameIdx = slotObj.IndexOf("\"game\":\"");
                            if (gameIdx >= 0)
                            {
                                int gStart = gameIdx + 8;
                                int gEnd = slotObj.IndexOf("\"", gStart);
                                if (gEnd > gStart)
                                {
                                    string game = slotObj.Substring(gStart, gEnd - gStart);
                                    playerGames[slot] = game;
                                }
                            }
                            
                            searchPos = braceEnd + 1;
                        }
                    }
                }
            }
            
            Log.LogInfo($"[AP] Parsed {playerNames.Count} players, {playerGames.Count} games");
            foreach (var kvp in playerGames)
            {
                string pName = playerNames.ContainsKey(kvp.Key) ? playerNames[kvp.Key] : $"Slot {kvp.Key}";
                Log.LogInfo($"[AP] Player {kvp.Key} ({pName}) plays {kvp.Value}");
            }
        }
    }
    
    public enum ServerLogType
    {
        System, Chat, ItemReceived, LocationSent, Hint, PlayerJoin, PlayerLeave, Goal, DeathLink, Error
    }
    
    public class ServerLogEntry
    {
        public string Message { get; }
        public ServerLogType Type { get; }
        public DateTime Timestamp { get; }
        
        public ServerLogEntry(string message, ServerLogType type, DateTime timestamp)
        {
            Message = message;
            Type = type;
            Timestamp = timestamp;
        }
    }
}