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
    /// <summary>
    /// Handles all Archipelago server communication
    /// </summary>
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
        private int lastProcessedIndex = -1;  // Track last processed item index, not IDs
        private int playerSlot = 0;
        
        public bool IsConnected { get; private set; } = false;
        
        // Gate sync timing - now does multiple attempts
        public bool PendingGateSync { get; set; } = false;
        public float GateSyncTimer { get; set; } = 0f;
        public int GateSyncAttempts { get; set; } = 0;
        public const int MAX_GATE_SYNC_ATTEMPTS = 3;
        public bool WaitingForReceivedItems { get; set; } = false;
        public float ReceivedItemsTimeout { get; set; } = 0f;
        
        public ArchipelagoClient(Plugin plugin)
        {
            this.plugin = plugin;
        }
        
        public void Connect(string serverAddress, string serverPort, string slot, string password, bool deathLinkEnabled)
        {
            try
            {
                plugin.UI.Status = "Starting proxy...";
                
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
                
                Log.LogInfo("Launching proxy: " + proxyPath);
                Log.LogInfo("Connecting to: " + serverAddress + ":" + serverPort);
                
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
        }
        
        public void SendGoalComplete()
        {
            if (!IsConnected) return;
            string json = "[{\"cmd\":\"StatusUpdate\",\"status\":30}]";
            SendPacket(json);
            plugin.UI.ShowNotification("Victory! Goal complete sent to Archipelago!");
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
            Log.LogInfo("Reset lastProcessedIndex to -1");
        }
        
        private void ReceiveLoop(string slot, string password, bool deathLinkEnabled, string serverAddress, string serverPort)
        {
            try
            {
                while (tcp != null && tcp.Connected)
                {
                    string line = reader.ReadLine();
                    if (line == null) break;
                    
                    // Handle RoomInfo immediately to send Connect
                    if (line.Contains("\"cmd\":\"RoomInfo\""))
                    {
                        plugin.UI.Status = "Authenticating...";
                        string tags = deathLinkEnabled ? "\"DeathLink\"" : "";
                        string pwField = string.IsNullOrEmpty(password) ? "null" : "\"" + password + "\"";
                        string json = "[{\"cmd\":\"Connect\",\"password\":" + pwField + ",\"game\":\"Untitled Goose Game\",\"name\":\"" + slot + "\",\"uuid\":\"" + Guid.NewGuid().ToString() + "\",\"version\":{\"class\":\"Version\",\"major\":0,\"minor\":5,\"build\":1},\"items_handling\":7,\"tags\":[" + tags + "],\"slot_data\":true}]";
                        SendPacket(json);
                    }
                    else
                    {
                        lock (messageQueue)
                            messageQueue.Enqueue(line);
                    }
                }
            }
            catch { }
            IsConnected = false;
            plugin.UI.Status = "Disconnected";
        }
        
        private void ProcessMessage(string data)
        {
            if (data.Contains("\"cmd\":\"Connected\""))
            {
                IsConnected = true;
                plugin.UI.Status = "CONNECTED!";
                plugin.UI.AddChatMessage("Connected to Archipelago!");
                
                // Parse slot
                int idx = data.IndexOf("\"slot\":");
                if (idx > 0)
                {
                    int start = idx + 7;
                    int end = data.IndexOf(",", start);
                    if (end > start)
                        int.TryParse(data.Substring(start, end - start), out playerSlot);
                }
                
                // Reload access flags from disk first - in case they were saved but not in memory
                plugin.ReloadAccessFlags();
                
                // Load last processed index from persistence
                lastProcessedIndex = PlayerPrefs.GetInt("AP_LastItemIndex", -1);
                Log.LogInfo($"Loaded lastProcessedIndex: {lastProcessedIndex}");
                
                // IMMEDIATELY queue a gate sync using saved flags
                // This ensures gates work even if AP doesn't resend items
                GateSyncTimer = 0f;
                GateSyncAttempts = 0;
                PendingGateSync = true;  // Start syncing now with saved flags
                Log.LogInfo("Queued immediate gate sync from saved access flags");
                
                WaitingForReceivedItems = true;
                ReceivedItemsTimeout = 0f;
                
                // Request sync
                SendPacket("[{\"cmd\":\"Sync\"}]");
                Log.LogInfo("Sent Sync request to AP server");
            }
            else if (data.Contains("\"cmd\":\"ReceivedItems\""))
            {
                Log.LogInfo(">>> RECEIVED ITEMS MESSAGE FROM AP <<<");
                WaitingForReceivedItems = false;
                ParseReceivedItems(data);
                
                Log.LogInfo("Received items processed, queuing gate sync (3 attempts)...");
                PendingGateSync = true;
                GateSyncTimer = 0f;
                GateSyncAttempts = 0;
            }
            else if (data.Contains("\"cmd\":\"Bounced\"") && data.Contains("\"DeathLink\""))
            {
                if (plugin.DeathLinkEnabled)
                {
                    plugin.TriggerDeathLink();
                }
            }
            else if (data.Contains("\"cmd\":\"PrintJSON\""))
            {
                int idx = data.IndexOf("\"text\":\"");
                if (idx > 0)
                {
                    int start = idx + 8;
                    int end = data.IndexOf("\"", start);
                    if (end > start)
                    {
                        string msg = data.Substring(start, Math.Min(55, end - start));
                        plugin.UI.AddChatMessage(msg);
                    }
                }
            }
            else if (data.Contains("\"cmd\":\"ConnectionRefused\""))
            {
                plugin.UI.Status = "Connection Refused!";
            }
        }
        
        private void ParseReceivedItems(string data)
        {
            Log.LogInfo("=== PARSING RECEIVED ITEMS ===");
            Log.LogInfo($"Current lastProcessedIndex: {lastProcessedIndex}");
            
            // Parse the starting index from the message
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
            Log.LogInfo($"ReceivedItems starting index: {startingIndex}");
            
            int itemCount = 0;
            int pos = 0;
            int currentIndex = startingIndex;
            
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
                        itemCount++;
                        string itemName = LocationMappings.GetItemName(itemId);
                        
                        // Only process items we haven't seen yet (by index)
                        if (currentIndex > lastProcessedIndex)
                        {
                            Log.LogInfo($"[RECEIVED NEW] Index {currentIndex}: {itemName} (ID: {itemId})");
                            plugin.UI.AddReceivedItem(itemName);
                            plugin.UI.AddChatMessage("Got: " + itemName);
                            plugin.ProcessReceivedItem(itemId);
                            
                            // Update last processed index
                            lastProcessedIndex = currentIndex;
                            PlayerPrefs.SetInt("AP_LastItemIndex", lastProcessedIndex);
                            PlayerPrefs.Save();
                        }
                        else
                        {
                            Log.LogInfo($"[RECEIVED SKIP] Index {currentIndex}: {itemName} (already processed)");
                        }
                        
                        currentIndex++;
                    }
                }
            }
            Log.LogInfo($"=== RECEIVED {itemCount} ITEMS, lastProcessedIndex now {lastProcessedIndex} ===");
        }
    }
}