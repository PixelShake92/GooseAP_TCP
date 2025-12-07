using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace GooseGameAP
{
    [BepInPlugin("com.archipelago.goosegame", "Goose Game Archipelago", "0.7.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        public static Plugin Instance;
        
        private TcpClient tcp;
        private StreamReader reader;
        private StreamWriter writer;
        private Thread receiveThread;
        private Harmony harmony;
        
        private bool showUI = false;
        private string serverAddress = "archipelago.gg";
        private string serverPort = "38281";
        private string slot = "Goosel";
        private string password = "";
        private string status = "Not connected";
        private bool connected = false;
        private bool hasInitializedGates = false;
        
        // Proxy process
        private System.Diagnostics.Process proxyProcess;
        private const int LOCAL_PROXY_PORT = 38282;
        
        private int playerSlot = 0;
        private Queue<string> messageQueue = new Queue<string>();
        private List<string> receivedItemNames = new List<string>();
        private List<string> chatMessages = new List<string>();
        private HashSet<long> checkedLocations = new HashSet<long>();
        private HashSet<long> receivedItemIds = new HashSet<long>();
        
        public const long BASE_ID = 119000000;
        
        // Area access flags
        public bool HasHighStreetAccess { get; private set; } = false;
        public bool HasBackGardensAccess { get; private set; } = false;
        public bool HasPubAccess { get; private set; } = false;
        public bool HasModelVillageAccess { get; private set; } = false;
        public bool HasGoldenBell { get; private set; } = false;
        
        // Buff tracking
        public float SpeedMultiplier { get; private set; } = 1.0f;
        public bool IsSilent { get; private set; } = false;
        public int MegaHonkCount { get; private set; } = 0;
        public int SpeedyFeetCount { get; private set; } = 0;
        
        // Trap state
        private float trapTimer = 0f;
        private bool isTired = false;
        private bool isClumsy = false;
        private bool hasButterfingers = false;
        private bool isSuspicious = false;
        
        // DeathLink
        public bool DeathLinkEnabled { get; set; } = false;
        private bool deathLinkPending = false;
        
        // Gate sync on reconnect
        private bool pendingGateSync = false;
        private float gateSyncTimer = 0f;

        // Gate snapshot system
        private class GateSnapshot
        {
            public int switchState;
            public Dictionary<string, bool> childActive = new Dictionary<string, bool>();
            public Dictionary<string, Vector3> childPos = new Dictionary<string, Vector3>();
            public Dictionary<string, Vector3> childRot = new Dictionary<string, Vector3>();
            public Dictionary<string, Vector3> childScale = new Dictionary<string, Vector3>();
            public Dictionary<string, string> components = new Dictionary<string, string>();
        }
        
        private Dictionary<string, GateSnapshot> beforeSnapshots = new Dictionary<string, GateSnapshot>();
        
        private static readonly string[] GatePathsToMonitor = new string[]
        {
            "gardenDynamic/GROUP_Hammering/gateTall",
            "gardenDynamic/GROUP_Hammering/gateTall/gateTallOpenSystem",
            "highStreetDynamic/GROUP_Garage/irongate",
            "highStreetDynamic/GROUP_Garage/irongate/GateSystem",
            "pubDynamic/GROUP_pubItems/PubGateSystem",
            "overworldStatic/GROUP_BackyardToPub/SluiceGateSystem",
            "pubDynamic/GROUP_BucketOnHead/PubToFinaleGateSystem",
            "gardenDynamic/GROUP_Gate/GardenGate",
            "gardenDynamic/GROUP_Gate/GardenGate/GateSystem"
        };

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            Log.LogInfo("Goose Game AP v0.9.4 - All Item Variants");
            
            harmony = new Harmony("com.archipelago.goosegame");
            harmony.PatchAll();
            Log.LogInfo("Harmony patches applied");
        }


        private void Update()
        {
            // Initialize gates and teleport to well once when the game scene is loaded
            if (!hasInitializedGates && GameManager.instance != null && GameManager.instance.allGeese != null && GameManager.instance.allGeese.Count > 0)
            {
                hasInitializedGates = true;
                Log.LogInfo("Game scene detected, starting initialization...");
                
                // Teleport player to the well hub as starting position
                TeleportGooseToWell();
                Log.LogInfo("Teleported to well hub");
                
                // Delay gate opening slightly to let scene fully initialize
                StartCoroutine(DelayedGateInit());
            }
            
            if (Input.GetKeyDown(KeyCode.F1))
            {
                showUI = !showUI;
                Log.LogInfo("F1 pressed - UI is now: " + (showUI ? "VISIBLE" : "HIDDEN"));
            }
            
            // Debug keys for testing
            if (Input.GetKeyDown(KeyCode.F2))
            {
                HasHighStreetAccess = true;
                OpenGatesForArea("HighStreet");
                ShowNotification("DEBUG: High Street unlocked!");
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                HasBackGardensAccess = true;
                OpenGatesForArea("Backyards");
                ShowNotification("DEBUG: Back Gardens unlocked!");
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                HasPubAccess = true;
                OpenGatesForArea("Pub");
                ShowNotification("DEBUG: Pub unlocked!");
            }
            if (Input.GetKeyDown(KeyCode.F5))
            {
                HasModelVillageAccess = true;
                OpenGatesForArea("Finale");
                ShowNotification("DEBUG: Model Village unlocked!");
            }
            if (Input.GetKeyDown(KeyCode.F6))
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    // Ctrl+F6: Find all colliders near goose
                    FindNearbyColliders();
                }
                else
                {
                    // Print current goose position for debugging
                    if (GameManager.instance != null && GameManager.instance.allGeese != null)
                    {
                        foreach (var goose in GameManager.instance.allGeese)
                        {
                            if (goose != null && goose.isActiveAndEnabled)
                            {
                                Vector3 pos = goose.transform.position;
                                string posStr = "Goose Position: X=" + pos.x.ToString("F2") + " Y=" + pos.y.ToString("F2") + " Z=" + pos.z.ToString("F2");
                                Log.LogInfo(posStr);
                                ShowNotification(posStr);
                            }
                        }
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.F7))
            {
                bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
                
                if (ctrl && shift)
                {
                    // Ctrl+Shift+F7: Dump all collected items to log
                    DumpCollectedItems();
                    ShowNotification("Dumped " + collectedItemsLog.Count + " items to log!");
                }
                else if (ctrl)
                {
                    // Ctrl+F7: Explore Goose class methods and held items
                    ExploreGooseClass();
                }
                else if (shift)
                {
                    // Shift+F7: Log currently held item
                    CheckHeldAndNearbyItems();
                }
                else if (alt)
                {
                    // Alt+F7: Explore Holder and Prop classes
                    ExploreHolderAndProp();
                }
                else
                {
                    // F7: Teleport to well
                    TeleportGooseToWell();
                    ShowNotification("Teleported to Well!");
                }
            }
            
            // === GATE DEBUG KEYS ===
            if (Input.GetKeyDown(KeyCode.F8))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    // Shift+F8: Search for sluice/pub blockers
                    SearchForSluiceBlockers();
                }
                else
                {
                    // F8: Log ALL components on gates
                    LogAllGateComponents();
                }
            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
                // Take BEFORE snapshot
                TakeBeforeSnapshot();
            }
            if (Input.GetKeyDown(KeyCode.F10))
            {
                // Take AFTER snapshot and compare
                TakeAfterSnapshotAndCompare();
            }
            if (Input.GetKeyDown(KeyCode.F11))
            {
                // Open all gates with multiple methods
                OpenAllGatesAggressive();
            }
            if (Input.GetKeyDown(KeyCode.F12))
            {
                // Teleport to test position (near finale gate)
                TeleportGoose(new Vector3(-2.38f, 1.37f, 21.24f));
                ShowNotification("Teleported to test position!");
            }
            
            lock (messageQueue)
            {
                while (messageQueue.Count > 0)
                    ProcessMessage(messageQueue.Dequeue());
            }
            
            // Handle trap timers
            if (trapTimer > 0)
            {
                trapTimer -= Time.deltaTime;
                if (trapTimer <= 0)
                {
                    ClearTraps();
                }
            }
            
            // Handle pending death link
            if (deathLinkPending)
            {
                deathLinkPending = false;
                ApplyDeathLink();
            }
            
            // Handle pending gate sync (for reconnection)
            if (pendingGateSync && GameManager.instance != null && GameManager.instance.allGeese != null && GameManager.instance.allGeese.Count > 0)
            {
                gateSyncTimer += Time.deltaTime;
                if (gateSyncTimer >= 2.0f) // Wait 2 seconds for scene to be fully ready
                {
                    pendingGateSync = false;
                    gateSyncTimer = 0f;
                    SyncGatesFromAccessFlags();
                }
            }
            
            // Automatic item pickup tracking
            TrackHeldItem();
        }
        
        // Item tracking state
        private Prop lastHeldProp = null;
        private HashSet<string> firstTimePickups = new HashSet<string>();
        
        private void TrackHeldItem()
        {
            if (GameManager.instance == null || GameManager.instance.allGeese == null) return;
            
            Goose goose = null;
            foreach (var g in GameManager.instance.allGeese)
            {
                if (g != null && g.isActiveAndEnabled)
                {
                    goose = g;
                    break;
                }
            }
            if (goose == null) return;
            
            var holder = goose.GetComponent<Holder>();
            if (holder == null) return;
            
            Prop currentProp = holder.holding;
            
            // Detect pickup (was null, now holding something)
            if (currentProp != null && lastHeldProp == null)
            {
                string itemName = currentProp.gameObject.name;
                string itemPath = GetGameObjectPath(currentProp.gameObject);
                string itemKey = itemName; // Use just name for first-time tracking
                
                Log.LogInfo("[AUTO] Picked up: " + itemName);
                
                // Check if this is first time picking up this item TYPE
                if (firstTimePickups.Add(itemKey))
                {
                    Log.LogInfo("[AUTO] >>> FIRST TIME PICKUP: " + itemName + " <<<");
                    OnFirstTimePickup(itemName, itemPath);
                }
            }
            // Detect drop (was holding, now null)
            else if (currentProp == null && lastHeldProp != null)
            {
                Log.LogInfo("[AUTO] Dropped: " + lastHeldProp.gameObject.name);
            }
            // Detect swap (holding different item)
            else if (currentProp != null && lastHeldProp != null && currentProp != lastHeldProp)
            {
                string itemName = currentProp.gameObject.name;
                string itemKey = itemName;
                
                Log.LogInfo("[AUTO] Swapped to: " + itemName);
                
                if (firstTimePickups.Add(itemKey))
                {
                    string itemPath = GetGameObjectPath(currentProp.gameObject);
                    Log.LogInfo("[AUTO] >>> FIRST TIME PICKUP: " + itemName + " <<<");
                    OnFirstTimePickup(itemName, itemPath);
                }
            }
            
            lastHeldProp = currentProp;
        }
        
        private void OnFirstTimePickup(string itemName, string itemPath)
        {
            // Show notification
            ShowNotification("First pickup: " + itemName);
            chatMessages.Add("Picked up: " + itemName);
            
            // TODO: Send AP location check based on item
            // For now, just log - we'll map items to locations next
            long? locationId = GetItemLocationId(itemName);
            if (locationId.HasValue && connected)
            {
                if (!checkedLocations.Contains(locationId.Value))
                {
                    checkedLocations.Add(locationId.Value);
                    string json = "[{\"cmd\":\"LocationChecks\",\"locations\":[" + locationId.Value + "]}]";
                    SendPacket(json);
                    Log.LogInfo("[AUTO] Sent location check for item: " + itemName + " (ID: " + locationId.Value + ")");
                }
            }
        }
        
        private long? GetItemLocationId(string itemName)
        {
            // Map item names to AP location IDs
            // Using BASE_ID + 1000 range for item pickups
            // NOTE: Parenthetical numbers like (1), (2) are UNIQUE items, do NOT strip them
            
            string lowerName = itemName.ToLower().Trim();
            Log.LogInfo("[ITEM LOOKUP] Raw: '" + itemName + "' | Lower: '" + lowerName + "'");
            
            switch (lowerName)
            {
                // Garden items (1001-1020)
                case "boot": return BASE_ID + 1001;
                case "radiosmall": return BASE_ID + 1002;
                case "trowel": return BASE_ID + 1003;
                case "keys": return BASE_ID + 1004;
                case "carrot": return BASE_ID + 1005;
                case "tulip": return BASE_ID + 1006;
                case "apple": return BASE_ID + 1007;
                case "jam": return BASE_ID + 1008;
                case "picnicmug": return BASE_ID + 1009;
                case "thermos (1)": return BASE_ID + 1010;
                case "sandwichr": return BASE_ID + 1011;
                case "sandwichl": return BASE_ID + 1012;
                case "forkgarden": return BASE_ID + 1013;
                case "strawhat": return BASE_ID + 1014;
                case "drinkcan": return BASE_ID + 1015;
                case "tennisball": return BASE_ID + 1016;
                case "gardenerhat": return BASE_ID + 1017;
                case "apple (1)": return BASE_ID + 1018;
                
                // High Street items (1021-1070)
                case "wimpglasses": return BASE_ID + 1021;
                case "hornrimmedglasses": return BASE_ID + 1022;
                case "redglasses": return BASE_ID + 1023;
                case "sunglasses": return BASE_ID + 1024;
                case "toiletpaper": return BASE_ID + 1025;
                case "toycar": return BASE_ID + 1026;
                case "hairbrush": return BASE_ID + 1027;
                case "toothbrush": return BASE_ID + 1028;
                case "stereoscope": return BASE_ID + 1029;
                case "dishwashbottle": return BASE_ID + 1030;
                case "canblue": return BASE_ID + 1031;
                case "canyellow": return BASE_ID + 1032;
                case "canorange": return BASE_ID + 1033;
                case "weedtool": return BASE_ID + 1034;
                case "lilyflower": return BASE_ID + 1035;
                case "orange": return BASE_ID + 1036;
                case "tomato (1)": return BASE_ID + 1037;
                case "carrotnogreen (1)": return BASE_ID + 1038;
                case "cucumber (1)": return BASE_ID + 1039;
                case "leek (1)": return BASE_ID + 1040;
                case "fusilage": return BASE_ID + 1041;
                case "pintbottle": return BASE_ID + 1042;
                case "spraybottle": return BASE_ID + 1043;
                case "walkietalkieb": return BASE_ID + 1044;
                case "walkietalkie": return BASE_ID + 1045;
                case "applecore": return BASE_ID + 1046;
                case "applecore (1)": return BASE_ID + 1058;
                case "dustbinlid": return BASE_ID + 1047;
                case "pintbottle (1)": return BASE_ID + 1048;
                case "coin": return BASE_ID + 1049;
                case "chalk": return BASE_ID + 1050;
                case "tomato (2)": return BASE_ID + 1051;
                case "orange (1)": return BASE_ID + 1052;
                case "orange (2)": return BASE_ID + 1053;
                case "carrotnogreen (3)": return BASE_ID + 1054;
                case "carrotnogreen (2)": return BASE_ID + 1057;
                case "cucumber (2)": return BASE_ID + 1055;
                case "leek (2)": return BASE_ID + 1056;
                case "leek (3)": return BASE_ID + 1059;
                case "tomato (3)": return BASE_ID + 1060;
                case "cucumber": return BASE_ID + 1061;
                
                // Back Gardens items (1071-1090)
                case "bowprop_b": return BASE_ID + 1071;
                case "dummyprop": return BASE_ID + 1072;
                case "cricketball": return BASE_ID + 1073;
                case "bustpipeprop": return BASE_ID + 1074;
                case "busthatprop": return BASE_ID + 1075;
                case "bustglassesprop": return BASE_ID + 1076;
                case "cleanslipperr": return BASE_ID + 1077;
                case "cleanslipperl": return BASE_ID + 1078;
                case "teacup": return BASE_ID + 1079;
                case "newspaper": return BASE_ID + 1080;
                case "socksplaceholder": return BASE_ID + 1081;
                case "socksplaceholder (1)": return BASE_ID + 1082;
                case "vaseprop": return BASE_ID + 1083;
                case "bowprop": return BASE_ID + 1084;
                case "potstack": return BASE_ID + 1085;
                case "soap": return BASE_ID + 1086;
                case "paintbrush": return BASE_ID + 1087;
                case "vasebroken01": return BASE_ID + 1088;
                case "vasebroken02": return BASE_ID + 1089;
                case "rightstrap": return BASE_ID + 1090;
                case "rightstrap (1)": return BASE_ID + 1091;
                case "rightstrap (2)": return BASE_ID + 1092;
                case "badmintonracket": return BASE_ID + 1093;
                
                // Pub items (1101-1130)
                case "fishingbobberprop": return BASE_ID + 1101;
                case "exitletterprop":
                case "exitletter": return BASE_ID + 1102;
                case "pubtomato": return BASE_ID + 1103;
                case "plate": return BASE_ID + 1104;
                case "plate (1)": return BASE_ID + 1105;
                case "plate (2)": return BASE_ID + 1106;
                case "quoitgreen (2)": return BASE_ID + 1107;
                case "quoitred (1)": return BASE_ID + 1108;
                case "fork": return BASE_ID + 1109;
                case "fork (1)": return BASE_ID + 1110;
                case "knife": return BASE_ID + 1111;
                case "knife (1)": return BASE_ID + 1112;
                case "cork": return BASE_ID + 1113;
                case "candlestick": return BASE_ID + 1114;
                case "flowerforvase": return BASE_ID + 1115;
                case "dart1": return BASE_ID + 1116;
                case "dart2": return BASE_ID + 1117;
                case "dart3": return BASE_ID + 1118;
                case "harmonica": return BASE_ID + 1119;
                case "pintglassprop": return BASE_ID + 1120;
                case "toyboat": return BASE_ID + 1121;
                case "woolyhat": return BASE_ID + 1122;
                case "peppergrinder": return BASE_ID + 1123;
                case "pubwomancloth": return BASE_ID + 1124;
                
                // Model Village items (1131-1150)
                case "miniperson variant - child": return BASE_ID + 1131;
                case "miniperson variant - jumpsuit": return BASE_ID + 1132;
                case "miniperson variant - gardener": return BASE_ID + 1133;
                case "minishovelprop": return BASE_ID + 1134;
                case "flowerpoppy": return BASE_ID + 1135;
                case "miniperson variant - old woman": return BASE_ID + 1136;
                case "miniphonedoorprop": return BASE_ID + 1137;
                case "minimailpillarprop": return BASE_ID + 1138;
                case "miniperson variant - postie": return BASE_ID + 1139;
                case "miniperson variant - vestman": return BASE_ID + 1140;
                case "miniperson": return BASE_ID + 1141;
                case "timberhandleprop": return BASE_ID + 1142;
                case "goldenbell": return BASE_ID + 1143;
                
                default: 
                    Log.LogWarning("[ITEM] Unknown item not mapped: " + itemName);
                    return null;
            }
        }

        private void OnGUI()
        {
            if (!showUI) return;

            GUI.Box(new Rect(10, 10, 450, 620), "Archipelago - Goose Game v0.7 (Gate Debug)");
            
            // Connection settings
            GUI.Label(new Rect(20, 40, 100, 20), "Server:");
            serverAddress = GUI.TextField(new Rect(120, 40, 200, 20), serverAddress);
            GUI.Label(new Rect(325, 40, 10, 20), ":");
            serverPort = GUI.TextField(new Rect(340, 40, 60, 20), serverPort);
            
            GUI.Label(new Rect(20, 65, 100, 20), "Slot Name:");
            slot = GUI.TextField(new Rect(120, 65, 280, 20), slot);
            
            GUI.Label(new Rect(20, 90, 100, 20), "Password:");
            password = GUI.PasswordField(new Rect(120, 90, 280, 20), password, '*');

            if (GUI.Button(new Rect(20, 120, 180, 30), connected ? "Connected!" : "Connect"))
                if (!connected) Connect();
            
            if (GUI.Button(new Rect(210, 120, 90, 30), "Disconnect"))
                Disconnect();
            
            if (GUI.Button(new Rect(310, 120, 90, 30), "Sync Gates"))
                SyncGatesFromAccessFlags();

            GUI.Label(new Rect(20, 158, 410, 20), status);
            
            int y = 183;
            
            // Area access display
            GUI.Label(new Rect(20, y, 380, 20), "=== Area Access ===");
            y += 22;
            
            GUI.Label(new Rect(20, y, 380, 20), "Garden: YES | High Street: " + (HasHighStreetAccess ? "YES" : "NO"));
            y += 20;
            GUI.Label(new Rect(20, y, 380, 20), "Back Gardens: " + (HasBackGardensAccess ? "YES" : "NO") + " | Pub: " + (HasPubAccess ? "YES" : "NO"));
            y += 20;
            GUI.Label(new Rect(20, y, 380, 20), "Model Village: " + (HasModelVillageAccess ? "YES" : "NO") + " | Golden Bell: " + (HasGoldenBell ? "YES" : "NO"));
            
            // Buffs display
            y += 25;
            GUI.Label(new Rect(20, y, 380, 20), "=== Buffs/Effects ===");
            y += 22;
            GUI.Label(new Rect(20, y, 380, 20), "Speed: " + (GetEffectiveSpeedMultiplier() * 100).ToString("F0") + "% | Silent: " + IsSilent + " | Mega Honks: " + MegaHonkCount);
            y += 20;
            
            // Active traps
            if (isTired || isClumsy || hasButterfingers || isSuspicious)
            {
                string trapText = "TRAP ACTIVE: ";
                if (isTired) trapText += "Tired ";
                if (isClumsy) trapText += "Clumsy ";
                if (hasButterfingers) trapText += "Butterfingers ";
                if (isSuspicious) trapText += "Suspicious ";
                trapText += "(" + trapTimer.ToString("F0") + "s)";
                GUI.Label(new Rect(20, y, 380, 20), trapText);
                y += 20;
            }
            
            // Stats
            y += 5;
            GUI.Label(new Rect(20, y, 380, 20), "Locations: " + checkedLocations.Count + " | Items: " + receivedItemNames.Count);
            
            // Recent items
            y += 25;
            GUI.Label(new Rect(20, y, 100, 20), "Recent Items:");
            y += 20;
            for (int i = Math.Max(0, receivedItemNames.Count - 4); i < receivedItemNames.Count; i++)
            {
                GUI.Label(new Rect(30, y, 370, 18), receivedItemNames[i]);
                y += 18;
            }
            
            // Chat messages
            y += 10;
            GUI.Label(new Rect(20, y, 100, 20), "Messages:");
            y += 20;
            for (int i = Math.Max(0, chatMessages.Count - 4); i < chatMessages.Count; i++)
            {
                GUI.Label(new Rect(30, y, 370, 18), chatMessages[i]);
                y += 18;
            }
            
            // Debug info
            y += 15;
            GUI.Label(new Rect(20, y, 420, 20), "=== Gate Debug Keys ===");
            y += 20;
            GUI.Label(new Rect(20, y, 420, 20), "F6=Pos Ctrl+F6=NearbyColliders F7=Well F8=Gates");
            y += 18;
            GUI.Label(new Rect(20, y, 420, 20), "Shift+F8=SearchBlockers F9=BEFORE F10=AFTER");
            y += 18;
            GUI.Label(new Rect(20, y, 420, 20), "F11=Force Open All F12=Teleport Test Pos");
        }

        public float GetEffectiveSpeedMultiplier()
        {
            if (isTired)
                return 0.5f;
            return 1.0f + (SpeedyFeetCount * 0.00f);
        }

        // ==================== GATE DEBUG FUNCTIONS ====================

        // F8 - Log ALL components on gates
        private void LogAllGateComponents()
        {
            Log.LogInfo("========== COMPREHENSIVE GATE COMPONENT ANALYSIS ==========");
            
            foreach (var path in GatePathsToMonitor)
            {
                var obj = GameObject.Find(path);
                if (obj == null) continue;
                
                Log.LogInfo("\n=== " + path + " ===");
                LogObjectComponents(obj, "");
            }
            
            ShowNotification("Gate components logged - check BepInEx log!");
        }
        
        private void LogObjectComponents(GameObject obj, string indent)
        {
            Log.LogInfo(indent + "[" + obj.name + "] Active:" + obj.activeSelf + " InHierarchy:" + obj.activeInHierarchy);
            Log.LogInfo(indent + "  LocalPos: " + obj.transform.localPosition);
            Log.LogInfo(indent + "  LocalRot: " + obj.transform.localEulerAngles);
            Log.LogInfo(indent + "  LocalScale: " + obj.transform.localScale);
            
            // Log all components
            var components = obj.GetComponents<Component>();
            foreach (var comp in components)
            {
                if (comp == null) continue;
                string typeName = comp.GetType().Name;
                Log.LogInfo(indent + "  [Component] " + typeName);
                
                // Special handling for interesting components
                if (typeName == "SwitchSystem")
                {
                    var sw = comp as SwitchSystem;
                    Log.LogInfo(indent + "    currentState: " + sw.currentState);
                    Log.LogInfo(indent + "    states.Length: " + (sw.states != null ? sw.states.Length : 0));
                    
                    // Log the states array content
                    if (sw.states != null)
                    {
                        for (int i = 0; i < sw.states.Length; i++)
                        {
                            Log.LogInfo(indent + "    state[" + i + "]: exists");
                        }
                    }
                }
                else if (typeName == "Animator")
                {
                    Log.LogInfo(indent + "    (Animator found - may control gate animation)");
                }
                else if (typeName == "Rigidbody")
                {
                    Log.LogInfo(indent + "    (Rigidbody found - physics body)");
                }
                else if (typeName.Contains("Joint"))
                {
                    Log.LogInfo(indent + "    (Joint - may control gate hinge)");
                }
                else if (typeName.Contains("Collider"))
                {
                    Log.LogInfo(indent + "    (Collider found)");
                }
                else if (typeName == "ToggleSwitchEffect")
                {
                    var toggle = comp as ToggleSwitchEffect;
                    if (toggle != null && toggle.entireObject != null)
                    {
                        Log.LogInfo(indent + "    target: " + toggle.entireObject.name + " active: " + toggle.entireObject.activeSelf);
                    }
                }
            }
            
            // Recurse to children (limit depth to 4)
            if (indent.Length < 16)
            {
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    LogObjectComponents(obj.transform.GetChild(i).gameObject, indent + "  ");
                }
            }
        }

        // F9 - Take BEFORE snapshot
        private void TakeBeforeSnapshot()
        {
            Log.LogInfo("========== TAKING BEFORE SNAPSHOT ==========");
            beforeSnapshots.Clear();
            
            foreach (var path in GatePathsToMonitor)
            {
                var obj = GameObject.Find(path);
                if (obj == null) continue;
                
                var snapshot = new GateSnapshot();
                
                // Get SwitchSystem state if present
                var sw = obj.GetComponent<SwitchSystem>();
                if (sw != null)
                    snapshot.switchState = sw.currentState;
                
                // Snapshot all descendants
                SnapshotRecursive(obj.transform, "", snapshot);
                
                beforeSnapshots[path] = snapshot;
                Log.LogInfo("Snapshot taken: " + path + " (state=" + snapshot.switchState + ", children=" + snapshot.childActive.Count + ")");
            }
            
            ShowNotification("BEFORE snapshot saved! Now open gate manually, then press F10");
        }
        
        private void SnapshotRecursive(Transform t, string prefix, GateSnapshot snapshot)
        {
            string key = string.IsNullOrEmpty(prefix) ? t.name : prefix + "/" + t.name;
            
            snapshot.childActive[key] = t.gameObject.activeInHierarchy;
            snapshot.childPos[key] = t.localPosition;
            snapshot.childRot[key] = t.localEulerAngles;
            snapshot.childScale[key] = t.localScale;
            
            // Get component list
            var comps = t.GetComponents<Component>();
            var compNames = new List<string>();
            foreach (var c in comps)
            {
                if (c != null)
                    compNames.Add(c.GetType().Name);
            }
            snapshot.components[key] = string.Join(",", compNames.ToArray());
            
            for (int i = 0; i < t.childCount; i++)
            {
                SnapshotRecursive(t.GetChild(i), key, snapshot);
            }
        }

        // F10 - Take AFTER snapshot and compare
        private void TakeAfterSnapshotAndCompare()
        {
            Log.LogInfo("========== COMPARING BEFORE/AFTER SNAPSHOTS ==========");
            
            if (beforeSnapshots.Count == 0)
            {
                ShowNotification("No BEFORE snapshot! Press F9 first.");
                return;
            }
            
            foreach (var path in GatePathsToMonitor)
            {
                var obj = GameObject.Find(path);
                if (obj == null) continue;
                if (!beforeSnapshots.ContainsKey(path)) continue;
                
                var before = beforeSnapshots[path];
                Log.LogInfo("\n=== CHANGES FOR: " + path + " ===");
                
                // Check SwitchSystem state
                var sw = obj.GetComponent<SwitchSystem>();
                if (sw != null && sw.currentState != before.switchState)
                {
                    Log.LogInfo("*** SWITCH STATE: " + before.switchState + " -> " + sw.currentState);
                }
                
                // Compare all children
                CompareRecursive(obj.transform, "", before);
            }
            
            ShowNotification("Comparison complete - check BepInEx log!");
        }
        
        private void CompareRecursive(Transform t, string prefix, GateSnapshot before)
        {
            string key = string.IsNullOrEmpty(prefix) ? t.name : prefix + "/" + t.name;
            
            // Check active state
            if (before.childActive.ContainsKey(key))
            {
                bool wasActive = before.childActive[key];
                bool isActive = t.gameObject.activeInHierarchy;
                if (wasActive != isActive)
                    Log.LogInfo("*** ACTIVE CHANGED: " + key + ": " + wasActive + " -> " + isActive);
            }
            else
            {
                Log.LogInfo("*** NEW OBJECT: " + key);
            }
            
            // Check position
            if (before.childPos.ContainsKey(key))
            {
                Vector3 oldPos = before.childPos[key];
                Vector3 newPos = t.localPosition;
                if (Vector3.Distance(oldPos, newPos) > 0.01f)
                    Log.LogInfo("*** POSITION CHANGED: " + key + ": " + oldPos + " -> " + newPos);
            }
            
            // Check rotation
            if (before.childRot.ContainsKey(key))
            {
                Vector3 oldRot = before.childRot[key];
                Vector3 newRot = t.localEulerAngles;
                if (Vector3.Distance(oldRot, newRot) > 1f)
                    Log.LogInfo("*** ROTATION CHANGED: " + key + ": " + oldRot + " -> " + newRot);
            }
            
            // Check scale
            if (before.childScale.ContainsKey(key))
            {
                Vector3 oldScale = before.childScale[key];
                Vector3 newScale = t.localScale;
                if (Vector3.Distance(oldScale, newScale) > 0.01f)
                    Log.LogInfo("*** SCALE CHANGED: " + key + ": " + oldScale + " -> " + newScale);
            }
            
            for (int i = 0; i < t.childCount; i++)
            {
                CompareRecursive(t.GetChild(i), key, before);
            }
        }

        // F11 - Aggressive gate opening - try everything
        private void OpenAllGatesAggressive()
        {
            Log.LogInfo("========== AGGRESSIVE GATE OPENING ==========");
            
            // Method 1: Set all save flags
            Log.LogInfo("Setting save flags...");
            SaveGameData.SetBoolValue("goalHammering", true, false);
            SaveGameData.SetBoolValue("goalGarage", true, false);
            SaveGameData.SetBoolValue("goalPrune", true, false);
            SaveGameData.SetBoolValue("goalBucket", true, false);
            
            // Method 2: Trigger switch events
            Log.LogInfo("Triggering switch events...");
            string[] events = { "unlockHighStreet", "unlockBackyards", "unlockPub", "unlockFinale",
                               "openHighStreet", "openBackyards", "openPub", "openFinale",
                               "enterAreaHighstreet", "enterAreaBackyards", "enterAreaPub", "enterAreaFinale" };
            foreach (var evt in events)
            {
                try { SwitchEventManager.TriggerEvent(evt); } catch { }
            }
            
            // Method 3: Open gates via SwitchSystem
            Log.LogInfo("Opening gates via SwitchSystem...");
            OpenAllGates();
            
            // Method 4: Skip animator triggers (requires additional Unity modules)
            Log.LogInfo("Skipping animator triggers (not available)...");
            
            // Method 5: Skip collider disabling (requires additional Unity modules)
            Log.LogInfo("Skipping collider disable (not available)...");
            
            // Method 6: Try to physically rotate gate objects
            Log.LogInfo("Trying physical rotation...");
            foreach (var path in GatePathsToMonitor)
            {
                var obj = GameObject.Find(path);
                if (obj == null) continue;
                
                // Find gate child
                Transform gateChild = obj.transform.Find("gate");
                if (gateChild == null)
                {
                    // Try finding any child with "gate" in name
                    foreach (Transform child in obj.transform)
                    {
                        if (child.name.ToLower().Contains("gate"))
                        {
                            gateChild = child;
                            break;
                        }
                    }
                }
                
                if (gateChild != null)
                {
                    Log.LogInfo("  Found gate child at: " + GetGameObjectPath(gateChild.gameObject));
                    Log.LogInfo("    Current rotation: " + gateChild.localEulerAngles);
                    
                    // Try different rotations
                    // Most gates rotate 90 degrees on Y axis when opening
                    gateChild.localEulerAngles = new Vector3(0, 90, 0);
                    Log.LogInfo("    Set rotation to: (0, 90, 0)");
                }
            }
            
            ShowNotification("Aggressive gate opening complete - check log!");
        }

        // ==================== CONNECTION ====================

        private void Connect()
        {
            try
            {
                status = "Starting proxy...";
                
                // Kill any existing proxy
                StopProxy();
                
                // Find and launch the proxy
                string gameDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string proxyPath = System.IO.Path.Combine(gameDir, "APProxy.exe");
                
                // Also check parent directories
                if (!System.IO.File.Exists(proxyPath))
                {
                    proxyPath = System.IO.Path.Combine(gameDir, "..", "APProxy.exe");
                }
                if (!System.IO.File.Exists(proxyPath))
                {
                    proxyPath = System.IO.Path.Combine(gameDir, "..", "..", "APProxy", "APProxy.exe");
                }
                if (!System.IO.File.Exists(proxyPath))
                {
                    // Try game root
                    string gameRoot = System.IO.Path.Combine(gameDir, "..", "..", "..");
                    proxyPath = System.IO.Path.Combine(gameRoot, "APProxy.exe");
                }
                
                // Normalize the path
                proxyPath = System.IO.Path.GetFullPath(proxyPath);
                
                if (!System.IO.File.Exists(proxyPath))
                {
                    status = "ERROR: APProxy.exe not found!";
                    Log.LogError("APProxy.exe not found. Searched near: " + gameDir);
                    chatMessages.Add("Place APProxy.exe in BepInEx/plugins/");
                    return;
                }
                
                Log.LogInfo("Launching proxy: " + proxyPath);
                Log.LogInfo("Connecting to: " + serverAddress + ":" + serverPort);
                
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = proxyPath,
                    Arguments = serverAddress + " " + serverPort + " " + LOCAL_PROXY_PORT,
                    WorkingDirectory = System.IO.Path.GetDirectoryName(proxyPath),
                    UseShellExecute = true,
                    CreateNoWindow = false
                };
                
                proxyProcess = System.Diagnostics.Process.Start(startInfo);
                
                System.Threading.Thread.Sleep(1500);
                
                if (proxyProcess == null)
                {
                    status = "ERROR: Failed to launch proxy process!";
                    Log.LogError("Process.Start returned null");
                    return;
                }
                
                if (proxyProcess.HasExited)
                {
                    status = "ERROR: Proxy exited immediately (code " + proxyProcess.ExitCode + ")";
                    Log.LogError("Proxy exited with code: " + proxyProcess.ExitCode);
                    chatMessages.Add("Is .NET 6 runtime installed?");
                    return;
                }
                
                status = "Connecting to proxy...";
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
                        System.Threading.Thread.Sleep(500);
                    }
                }
                
                var stream = tcp.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8);
                writer.AutoFlush = true;

                status = "Waiting for server...";

                receiveThread = new Thread(ReceiveLoop);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                status = "Error: " + ex.Message;
                Log.LogError("Connect error: " + ex);
                chatMessages.Add("Error: " + ex.Message);
            }
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

        private void SendPacket(string json)
        {
            try { writer.WriteLine(json); }
            catch { }
        }

        private void ReceiveLoop()
        {
            try
            {
                while (tcp != null && tcp.Connected)
                {
                    string line = reader.ReadLine();
                    if (line == null) break;
                    lock (messageQueue)
                        messageQueue.Enqueue(line);
                }
            }
            catch { }
            connected = false;
            status = "Disconnected";
        }

        private void ProcessMessage(string data)
        {
            if (data.Contains("\"cmd\":\"RoomInfo\""))
            {
                status = "Authenticating...";
                string tags = DeathLinkEnabled ? "\"DeathLink\"" : "";
                string pwField = string.IsNullOrEmpty(password) ? "null" : "\"" + password + "\"";
                string json = "[{\"cmd\":\"Connect\",\"password\":" + pwField + ",\"game\":\"Untitled Goose Game\",\"name\":\"" + slot + "\",\"uuid\":\"" + Guid.NewGuid().ToString() + "\",\"version\":{\"class\":\"Version\",\"major\":0,\"minor\":5,\"build\":1},\"items_handling\":7,\"tags\":[" + tags + "],\"slot_data\":true}]";
                SendPacket(json);
            }
            else if (data.Contains("\"cmd\":\"Connected\""))
            {
                connected = true;
                status = "CONNECTED to " + serverAddress + ":" + serverPort;
                chatMessages.Add("Connected to Archipelago!");
                
                int idx = data.IndexOf("\"slot\":");
                if (idx > 0)
                {
                    int start = idx + 7;
                    int end = data.IndexOf(",", start);
                    if (end > start)
                        int.TryParse(data.Substring(start, end - start), out playerSlot);
                }
                
                // On connect/reconnect, clear received items to force re-sync
                // AP will send all items we've ever received
                receivedItemIds.Clear();
                Log.LogInfo("Cleared receivedItemIds for fresh sync from AP");
                
                // Queue a delayed gate re-sync in case items arrive before scene is ready
                pendingGateSync = true;
            }
            else if (data.Contains("\"cmd\":\"ReceivedItems\""))
            {
                ParseReceivedItems(data);
            }
            else if (data.Contains("\"cmd\":\"Bounced\"") && data.Contains("\"DeathLink\""))
            {
                if (DeathLinkEnabled)
                {
                    deathLinkPending = true;
                    chatMessages.Add("DeathLink received!");
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
                        chatMessages.Add(msg);
                        if (chatMessages.Count > 20) chatMessages.RemoveAt(0);
                    }
                }
            }
            else if (data.Contains("\"cmd\":\"ConnectionRefused\""))
            {
                status = "Connection Refused!";
            }
        }

        private void ParseReceivedItems(string data)
        {
            int pos = 0;
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
                        if (!receivedItemIds.Contains(itemId))
                        {
                            receivedItemIds.Add(itemId);
                            string itemName = GetItemName(itemId);
                            receivedItemNames.Add(itemName);
                            chatMessages.Add("Got: " + itemName);
                            if (chatMessages.Count > 20) chatMessages.RemoveAt(0);
                            ProcessItem(itemId);
                        }
                    }
                }
            }
        }

        private string GetItemName(long itemId)
        {
            long offset = itemId - BASE_ID;
            switch (offset)
            {
                case 100: return "Garden Access";
                case 101: return "High Street Access";
                case 102: return "Back Gardens Access";
                case 103: return "Pub Access";
                case 104: return "Model Village Access";
                case 110: return "Progressive Area";
                case 200: return "Mega Honk";
                case 201: return "Speedy Feet";
                case 202: return "Silent Steps";
                case 203: return "A Nice Day";
                case 300: return "Tired Goose";
                case 301: return "Clumsy Feet";
                case 302: return "Butterfingers";
                case 303: return "Suspicious Goose";
                case 999: return "Golden Bell";
                default: return "Unknown Item (" + offset + ")";
            }
        }

        private void ProcessItem(long itemId)
        {
            long offset = itemId - BASE_ID;
            Log.LogInfo("Processing item offset: " + offset);
            
            switch (offset)
            {
                case 101:
                    HasHighStreetAccess = true;
                    TriggerAreaUnlock("HighStreet");
                    OpenGatesForArea("HighStreet");
                    ShowNotification("High Street is now accessible!");
                    break;
                case 102:
                    HasBackGardensAccess = true;
                    TriggerAreaUnlock("Backyards");
                    OpenGatesForArea("Backyards");
                    ShowNotification("Back Gardens are now accessible!");
                    break;
                case 103:
                    HasPubAccess = true;
                    TriggerAreaUnlock("Pub");
                    OpenGatesForArea("Pub");
                    ShowNotification("The Pub is now accessible!");
                    break;
                case 104:
                    HasModelVillageAccess = true;
                    TriggerAreaUnlock("Finale");
                    OpenGatesForArea("Finale");
                    ShowNotification("Model Village is now accessible!");
                    break;
                    
                case 200:
                    MegaHonkCount++;
                    ShowNotification("MEGA HONK power increased! (x" + MegaHonkCount + ")");
                    break;
                case 201:
                    SpeedyFeetCount++;
                    ShowNotification("Speedy Feet! (+" + (SpeedyFeetCount * 15) + "% speed)");
                    break;
                case 202:
                    IsSilent = true;
                    ShowNotification("Your steps are now silent!");
                    break;
                case 203:
                    ShowNotification("What a nice day to be a goose!");
                    break;
                    
                case 300:
                    isTired = true;
                    trapTimer = 60f;
                    ShowNotification("TRAP: You feel very tired... (60s)");
                    break;
                case 301:
                    isClumsy = true;
                    trapTimer = 60f;
                    ShowNotification("TRAP: Your feet are clumsy! (60s)");
                    break;
                case 302:
                    hasButterfingers = true;
                    trapTimer = 60f;
                    ShowNotification("TRAP: Butterfingers! You might drop items! (60s)");
                    break;
                case 303:
                    isSuspicious = true;
                    trapTimer = 30f;
                    ShowNotification("TRAP: NPCs are suspicious of you! (30s)");
                    break;
                    
                case 999:
                    HasGoldenBell = true;
                    ShowNotification("You received the Golden Bell!");
                    break;
            }
        }

        private void ClearTraps()
        {
            bool hadTraps = isTired || isClumsy || hasButterfingers || isSuspicious;
            isTired = false;
            isClumsy = false;
            hasButterfingers = false;
            isSuspicious = false;
            if (hadTraps)
                ShowNotification("Trap effects have worn off!");
        }
        
        public void SyncGatesFromAccessFlags()
        {
            Log.LogInfo("=== SYNCING GATES FROM ACCESS FLAGS ===");
            Log.LogInfo("  High Street: " + HasHighStreetAccess);
            Log.LogInfo("  Back Gardens: " + HasBackGardensAccess);
            Log.LogInfo("  Pub: " + HasPubAccess);
            Log.LogInfo("  Model Village: " + HasModelVillageAccess);
            
            if (HasHighStreetAccess)
            {
                OpenGatesForArea("HighStreet");
                Log.LogInfo("  Opened High Street gates");
            }
            if (HasBackGardensAccess)
            {
                OpenGatesForArea("Backyards");
                Log.LogInfo("  Opened Back Gardens gates");
            }
            if (HasPubAccess)
            {
                OpenGatesForArea("Pub");
                Log.LogInfo("  Opened Pub gates");
            }
            if (HasModelVillageAccess)
            {
                OpenGatesForArea("Finale");
                Log.LogInfo("  Opened Model Village gates");
            }
            
            // Always ensure hub paths are open
            var parkHubBlocker = GameObject.Find("highStreetDynamic/GROUP_Garage/irongate/GateSystem/GateExtraColliders/ParkHubGateExtraCollider");
            if (parkHubBlocker != null)
            {
                parkHubBlocker.SetActive(false);
                Log.LogInfo("  Disabled ParkHubGateExtraCollider");
            }
            
            ShowNotification("Gates synced from server!");
            Log.LogInfo("=== GATE SYNC COMPLETE ===");
        }

        private void ShowNotification(string message)
        {
            Log.LogInfo("[NOTIFICATION] " + message);
            chatMessages.Add(message);
            if (chatMessages.Count > 20) chatMessages.RemoveAt(0);
        }

        private void TriggerAreaUnlock(string areaName)
        {
            try
            {
                Log.LogInfo("Triggering area unlock for: " + areaName);
                
                string[] possibleEvents = new string[]
                {
                    "unlock" + areaName,
                    "open" + areaName,
                    "gate" + areaName,
                    areaName + "Unlocked",
                    areaName + "Open",
                    areaName + "Gate",
                    "openGateTo" + areaName,
                    "unlockGateTo" + areaName
                };
                
                foreach (string eventName in possibleEvents)
                {
                    Log.LogInfo("Trying switch event: " + eventName);
                    SwitchEventManager.TriggerEvent(eventName);
                }
                
                switch (areaName)
                {
                    case "HighStreet":
                        SaveGameData.SetBoolValue("goalHammering", true, false);
                        break;
                    case "Backyards":
                        SaveGameData.SetBoolValue("goalGarage", true, false);
                        break;
                    case "Pub":
                        SaveGameData.SetBoolValue("goalPrune", true, false);
                        break;
                    case "Finale":
                        SaveGameData.SetBoolValue("goalBucket", true, false);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.LogError("TriggerAreaUnlock error: " + ex.Message);
            }
        }

        public bool CanEnterArea(GoalListArea area)
        {
            switch (area)
            {
                case GoalListArea.Garden:
                    return true;
                case GoalListArea.Shops:
                    return HasHighStreetAccess;
                case GoalListArea.Backyards:
                    return HasBackGardensAccess;
                case GoalListArea.Pub:
                    return HasPubAccess;
                case GoalListArea.Finale:
                    return HasModelVillageAccess;
                default:
                    return true;
            }
        }

        public void OnAreaBlocked(GoalListArea blockedArea)
        {
            string areaName = blockedArea.ToString();
            ShowNotification("You need " + areaName + " Access to enter!");
        }

        public void OnGoalCompleted(string goalName)
        {
            if (!connected)
            {
                Log.LogWarning("Goal completed but not connected: " + goalName);
                return;
            }

            long? locId = GetLocationId(goalName);
            if (locId == null)
            {
                Log.LogWarning("Unknown goal: " + goalName);
                return;
            }

            if (checkedLocations.Contains(locId.Value))
            {
                Log.LogInfo("Already checked: " + goalName);
                return;
            }

            checkedLocations.Add(locId.Value);
            string json = "[{\"cmd\":\"LocationChecks\",\"locations\":[" + locId.Value + "]}]";
            SendPacket(json);
            Log.LogInfo("Sent location check: " + goalName + " (" + locId + ")");
            chatMessages.Add("Checked: " + goalName);
            if (chatMessages.Count > 20) chatMessages.RemoveAt(0);
        }

        private long? GetLocationId(string goalName)
        {
            switch (goalName)
            {
                case "goalGarden": return BASE_ID + 1;
                case "goalWet": return BASE_ID + 2;
                case "goalKeys": return BASE_ID + 3;
                case "goalHat": return BASE_ID + 4;
                case "goalRake": return BASE_ID + 5;
                case "goalPicnic": return BASE_ID + 6;
                case "goalHammering": return BASE_ID + 7;
                case "goalBroom": return BASE_ID + 10;
                case "goalPhonebooth": return BASE_ID + 11;
                case "goalWrongGlasses": return BASE_ID + 12;
                case "goalBuyBack": return BASE_ID + 13;
                case "goalGetInShop": return BASE_ID + 14;
                case "goalShopping": return BASE_ID + 15;
                case "goalGarage": return BASE_ID + 16;
                case "goalBreakVase": return BASE_ID + 20;
                case "goalDressStatue": return BASE_ID + 21;
                case "goalBell": return BASE_ID + 22;
                case "goalRibbon": return BASE_ID + 23;
                case "goalBarefoot": return BASE_ID + 24;
                case "goalWashing": return BASE_ID + 25;
                case "goalPrune": return BASE_ID + 26;
                case "goalIntoPub": return BASE_ID + 30;
                case "goalOldMan1": return BASE_ID + 31;
                case "goalBoat": return BASE_ID + 32;
                case "goalOldMan2": return BASE_ID + 33;
                case "goalFlower": return BASE_ID + 34;
                case "goalPintGlass": return BASE_ID + 35;
                case "goalSetTable": return BASE_ID + 36;
                case "goalBucket": return BASE_ID + 37;
                case "goalModelVillage": return BASE_ID + 40;
                case "goalStealBell": return BASE_ID + 41;
                case "goalFinale": return BASE_ID + 42;
                case "goalLockout": return BASE_ID + 50;
                case "goalCabbage": return BASE_ID + 51;
                case "goalPuddle": return BASE_ID + 52;
                case "goalScales": return BASE_ID + 53;
                case "goalUmbrella": return BASE_ID + 54;
                case "goalBuyBack2": return BASE_ID + 55;
                case "goalFlowers": return BASE_ID + 56;
                case "goalWimpGarage": return BASE_ID + 60;
                case "goalCatch": return BASE_ID + 61;
                case "goalThrownGoose": return BASE_ID + 62;
                case "goalBust2": return BASE_ID + 63;
                case "goalFootball": return BASE_ID + 64;
                case "goalBoatBridge": return BASE_ID + 65;
                case "goalPerformRibbon": return BASE_ID + 66;
                case "goalOldManHat": return BASE_ID + 67;
                case "goalSpeedyGarden": return BASE_ID + 70;
                case "goalSpeedyShops": return BASE_ID + 71;
                case "goalSpeedyBackyards": return BASE_ID + 72;
                case "goalSpeedyPub": return BASE_ID + 73;
                case "goal100": return BASE_ID + 80;
                default: return null;
            }
        }

        public void SendGoalComplete()
        {
            if (!connected) return;
            SendPacket("[{\"cmd\":\"StatusUpdate\",\"status\":30}]");
            Log.LogInfo("Sent goal complete!");
            chatMessages.Add("GOAL COMPLETE! You win!");
        }

        public void OnGooseShooed()
        {
            if (!connected || !DeathLinkEnabled) return;
            
            string json = "[{\"cmd\":\"Bounce\",\"tags\":[\"DeathLink\"],\"data\":{\"time\":" + 
                DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 
                ",\"cause\":\"" + slot + " was shooed!\",\"source\":\"" + slot + "\"}}]";
            SendPacket(json);
            Log.LogInfo("Sent DeathLink: Goose was shooed!");
            chatMessages.Add("DeathLink sent: You were shooed!");
        }
        
        // Item pickup tracking
        private HashSet<string> pickedUpItems = new HashSet<string>();
        
        public void OnItemPickedUp(string itemName, string itemPath)
        {
            // For now just log - we'll add location checks once we map items to locations
            Log.LogInfo("=== ITEM PICKED UP ===");
            Log.LogInfo("  Name: " + itemName);
            Log.LogInfo("  Path: " + itemPath);
            
            // Track unique pickups (use path to distinguish same-named items)
            string key = itemPath;
            if (pickedUpItems.Add(key))
            {
                Log.LogInfo("  First time picking up this specific item!");
                chatMessages.Add("Picked up: " + itemName);
                
                // TODO: Map item pickups to AP location checks
                // For example:
                // - "radio" in garden -> send location check
                // - "apple" -> send location check
                // etc.
            }
            else
            {
                Log.LogInfo("  Already picked up before");
            }
        }

        private void ApplyDeathLink()
        {
            Log.LogInfo("Applying DeathLink - teleporting goose back");
            TeleportGooseToWell();
            ShowNotification("DeathLink! Another player died!");
        }

        private void Disconnect()
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
            connected = false;
            StopProxy();
            status = "Disconnected";
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
            Disconnect();
            StopProxy();
        }

        // Well/Hub coordinates - UPDATE THESE after testing with F6!
        public static readonly Vector3 WellPosition = new Vector3(0f, 0f, 0f); // PLACEHOLDER - update with actual coords
        
        public void TeleportGooseToWell()
        {
            try
            {
                if (GameManager.instance != null && GameManager.instance.allGeese != null)
                {
                    foreach (var goose in GameManager.instance.allGeese)
                    {
                        if (goose != null && goose.isActiveAndEnabled)
                        {
                            if (goose.mover != null)
                            {
                                goose.mover.currentSpeed = 0f;
                            }
                            goose.transform.position = WellPosition;
                            Log.LogInfo("Teleported goose to well at: " + WellPosition);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError("TeleportGooseToWell error: " + ex.Message);
            }
        }
        
        public void TeleportGoose(Vector3 position)
        {
            try
            {
                if (GameManager.instance != null && GameManager.instance.allGeese != null)
                {
                    foreach (var goose in GameManager.instance.allGeese)
                    {
                        if (goose != null && goose.isActiveAndEnabled)
                        {
                            if (goose.mover != null)
                            {
                                goose.mover.currentSpeed = 0f;
                            }
                            goose.transform.position = position;
                            Log.LogInfo("Teleported goose to: " + position);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError("TeleportGoose error: " + ex.Message);
            }
        }

        private void LogAllSwitchSystems()
        {
            Log.LogInfo("=== LOGGING ALL SWITCH SYSTEMS ===");
            var allSwitches = UnityEngine.Object.FindObjectsOfType<SwitchSystem>();
            Log.LogInfo("Found " + allSwitches.Length + " SwitchSystems");
            
            foreach (var sw in allSwitches)
            {
                string path = GetGameObjectPath(sw.gameObject);
                Log.LogInfo("SwitchSystem: " + path + " | State: " + sw.currentState + " | States Count: " + (sw.states != null ? sw.states.Length.ToString() : "null"));
                
                var toggleEffects = sw.GetComponentsInChildren<ToggleSwitchEffect>(true);
                foreach (var toggle in toggleEffects)
                {
                    string targetName = toggle.entireObject != null ? toggle.entireObject.name : "null";
                    bool isActive = toggle.entireObject != null ? toggle.entireObject.activeSelf : false;
                    Log.LogInfo("  -> ToggleSwitchEffect: " + toggle.gameObject.name + " | Target: " + targetName + " | Active: " + isActive);
                }
            }
            
            ShowNotification("Logged " + allSwitches.Length + " SwitchSystems - check BepInEx log!");
        }
        
        private void ExploreGooseClass()
        {
            Log.LogInfo("=== EXPLORING GOOSE CLASS ===");
            
            if (GameManager.instance == null || GameManager.instance.allGeese == null)
            {
                Log.LogWarning("No GameManager or geese available!");
                return;
            }
            
            foreach (var goose in GameManager.instance.allGeese)
            {
                if (goose == null || !goose.isActiveAndEnabled) continue;
                
                var gooseType = goose.GetType();
                Log.LogInfo("Goose Type: " + gooseType.FullName);
                
                // Get all methods
                Log.LogInfo("--- METHODS ---");
                var methods = gooseType.GetMethods(System.Reflection.BindingFlags.Public | 
                                                    System.Reflection.BindingFlags.NonPublic | 
                                                    System.Reflection.BindingFlags.Instance);
                foreach (var method in methods)
                {
                    // Focus on interesting ones
                    string name = method.Name.ToLower();
                    if (name.Contains("grab") || name.Contains("pick") || name.Contains("hold") ||
                        name.Contains("drop") || name.Contains("item") || name.Contains("carry") ||
                        name.Contains("interact") || name.Contains("mouth") || name.Contains("beak"))
                    {
                        string paramStr = "";
                        foreach (var p in method.GetParameters())
                            paramStr += p.ParameterType.Name + " " + p.Name + ", ";
                        Log.LogInfo("  " + method.ReturnType.Name + " " + method.Name + "(" + paramStr.TrimEnd(',', ' ') + ")");
                    }
                }
                
                // Get all fields
                Log.LogInfo("--- FIELDS (holding/item related) ---");
                var fields = gooseType.GetFields(System.Reflection.BindingFlags.Public | 
                                                  System.Reflection.BindingFlags.NonPublic | 
                                                  System.Reflection.BindingFlags.Instance);
                foreach (var field in fields)
                {
                    string name = field.Name.ToLower();
                    if (name.Contains("grab") || name.Contains("pick") || name.Contains("hold") ||
                        name.Contains("item") || name.Contains("carry") || name.Contains("mouth") ||
                        name.Contains("beak") || name.Contains("held") || name.Contains("current"))
                    {
                        var val = field.GetValue(goose);
                        string valStr = val != null ? val.ToString() : "null";
                        Log.LogInfo("  " + field.FieldType.Name + " " + field.Name + " = " + valStr);
                    }
                }
                
                // Get all properties
                Log.LogInfo("--- PROPERTIES (holding/item related) ---");
                var props = gooseType.GetProperties(System.Reflection.BindingFlags.Public | 
                                                     System.Reflection.BindingFlags.NonPublic | 
                                                     System.Reflection.BindingFlags.Instance);
                foreach (var prop in props)
                {
                    string name = prop.Name.ToLower();
                    if (name.Contains("grab") || name.Contains("pick") || name.Contains("hold") ||
                        name.Contains("item") || name.Contains("carry") || name.Contains("mouth") ||
                        name.Contains("beak") || name.Contains("held") || name.Contains("current"))
                    {
                        try
                        {
                            var val = prop.GetValue(goose, null);
                            string valStr = val != null ? val.ToString() : "null";
                            Log.LogInfo("  " + prop.PropertyType.Name + " " + prop.Name + " = " + valStr);
                        }
                        catch
                        {
                            Log.LogInfo("  " + prop.PropertyType.Name + " " + prop.Name + " = <error reading>");
                        }
                    }
                }
                
                // Also explore components on goose
                Log.LogInfo("--- GOOSE COMPONENTS ---");
                var components = goose.GetComponents<Component>();
                foreach (var comp in components)
                {
                    Log.LogInfo("  " + comp.GetType().Name);
                }
                
                // Look for a mouth/beak child object that might hold items
                Log.LogInfo("--- GOOSE CHILD OBJECTS ---");
                foreach (Transform child in goose.transform)
                {
                    LogTransformRecursive(child, "  ", 3);
                }
                
                break; // Just first goose
            }
            
            // Also search for item-related types in the scene
            Log.LogInfo("--- SEARCHING FOR ITEM TYPES ---");
            var allBehaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
            HashSet<string> uniqueTypes = new HashSet<string>();
            foreach (var b in allBehaviours)
            {
                string typeName = b.GetType().Name.ToLower();
                if (typeName.Contains("item") || typeName.Contains("pick") || typeName.Contains("grab") ||
                    typeName.Contains("hold") || typeName.Contains("prop") || typeName.Contains("interact"))
                {
                    if (uniqueTypes.Add(b.GetType().FullName))
                    {
                        Log.LogInfo("  Found type: " + b.GetType().FullName);
                    }
                }
            }
            
            ShowNotification("Goose class explored - check log!");
        }
        
        private void CheckHeldAndNearbyItems()
        {
            Log.LogInfo("=== LOGGING HELD ITEM ===");
            
            if (GameManager.instance == null || GameManager.instance.allGeese == null)
            {
                Log.LogWarning("No GameManager or geese available!");
                return;
            }
            
            Goose goose = null;
            foreach (var g in GameManager.instance.allGeese)
            {
                if (g != null && g.isActiveAndEnabled)
                {
                    goose = g;
                    break;
                }
            }
            
            if (goose == null)
            {
                Log.LogWarning("No active goose found!");
                return;
            }
            
            // Get the Holder component
            var holder = goose.GetComponent<Holder>();
            if (holder == null)
            {
                Log.LogWarning("No Holder component on goose!");
                return;
            }
            
            // Use the discovered 'holding' property
            Prop heldProp = holder.holding;
            
            if (heldProp == null)
            {
                Log.LogInfo("NOT HOLDING ANYTHING");
                ShowNotification("Not holding anything!");
                return;
            }
            
            // Log detailed info about the held item
            string itemName = heldProp.gameObject.name;
            string itemPath = GetGameObjectPath(heldProp.gameObject);
            
            Log.LogInfo("========================================");
            Log.LogInfo("HELD ITEM: " + itemName);
            Log.LogInfo("PATH: " + itemPath);
            
            // Get the prop's original parent/home if available
            var propType = heldProp.GetType();
            var homeField = propType.GetField("home", System.Reflection.BindingFlags.Public | 
                                                       System.Reflection.BindingFlags.NonPublic | 
                                                       System.Reflection.BindingFlags.Instance);
            if (homeField != null)
            {
                var home = homeField.GetValue(heldProp);
                if (home != null)
                {
                    Log.LogInfo("HOME: " + home.ToString());
                }
            }
            
            // Log all components on the prop
            Log.LogInfo("COMPONENTS:");
            foreach (var comp in heldProp.GetComponents<Component>())
            {
                Log.LogInfo("  - " + comp.GetType().Name);
            }
            
            Log.LogInfo("========================================");
            
            // Add to our collected items list
            if (collectedItemsLog.Add(itemName + "|" + itemPath))
            {
                Log.LogInfo(">>> NEW UNIQUE ITEM LOGGED <<<");
                ShowNotification("Logged: " + itemName + " (NEW)");
            }
            else
            {
                ShowNotification("Logged: " + itemName + " (already recorded)");
            }
            
            // Also print current collection count
            Log.LogInfo("Total unique items logged: " + collectedItemsLog.Count);
        }
        
        // Storage for collected items during playthrough
        private HashSet<string> collectedItemsLog = new HashSet<string>();
        
        private void DumpCollectedItems()
        {
            Log.LogInfo("========================================");
            Log.LogInfo("=== ALL COLLECTED ITEMS ===");
            Log.LogInfo("========================================");
            foreach (var item in collectedItemsLog)
            {
                var parts = item.Split('|');
                Log.LogInfo("NAME: " + parts[0]);
                if (parts.Length > 1)
                    Log.LogInfo("PATH: " + parts[1]);
                Log.LogInfo("---");
            }
            Log.LogInfo("TOTAL: " + collectedItemsLog.Count + " unique items");
            Log.LogInfo("========================================");
        }
        
        private void ExploreHolderAndProp()
        {
            Log.LogInfo("=== EXPLORING HOLDER AND PROP CLASSES ===");
            
            // Find a Holder component
            var holders = UnityEngine.Object.FindObjectsOfType<Holder>();
            Log.LogInfo("Found " + holders.Length + " Holder components");
            
            if (holders.Length > 0)
            {
                var holder = holders[0];
                var holderType = holder.GetType();
                Log.LogInfo("Holder Type: " + holderType.FullName);
                
                // Get ALL methods
                Log.LogInfo("--- HOLDER METHODS ---");
                var methods = holderType.GetMethods(System.Reflection.BindingFlags.Public | 
                                                    System.Reflection.BindingFlags.NonPublic | 
                                                    System.Reflection.BindingFlags.Instance |
                                                    System.Reflection.BindingFlags.DeclaredOnly);
                foreach (var method in methods)
                {
                    string paramStr = "";
                    foreach (var p in method.GetParameters())
                        paramStr += p.ParameterType.Name + " " + p.Name + ", ";
                    Log.LogInfo("  " + method.ReturnType.Name + " " + method.Name + "(" + paramStr.TrimEnd(',', ' ') + ")");
                }
                
                // Get ALL fields
                Log.LogInfo("--- HOLDER FIELDS ---");
                var fields = holderType.GetFields(System.Reflection.BindingFlags.Public | 
                                                  System.Reflection.BindingFlags.NonPublic | 
                                                  System.Reflection.BindingFlags.Instance);
                foreach (var field in fields)
                {
                    try
                    {
                        var val = field.GetValue(holder);
                        string valStr = val != null ? val.ToString() : "null";
                        if (valStr.Length > 50) valStr = valStr.Substring(0, 50) + "...";
                        Log.LogInfo("  " + field.FieldType.Name + " " + field.Name + " = " + valStr);
                    }
                    catch
                    {
                        Log.LogInfo("  " + field.FieldType.Name + " " + field.Name + " = <error>");
                    }
                }
                
                // Get ALL properties
                Log.LogInfo("--- HOLDER PROPERTIES ---");
                var props = holderType.GetProperties(System.Reflection.BindingFlags.Public | 
                                                     System.Reflection.BindingFlags.NonPublic | 
                                                     System.Reflection.BindingFlags.Instance);
                foreach (var prop in props)
                {
                    try
                    {
                        var val = prop.GetValue(holder, null);
                        string valStr = val != null ? val.ToString() : "null";
                        Log.LogInfo("  " + prop.PropertyType.Name + " " + prop.Name + " = " + valStr);
                    }
                    catch
                    {
                        Log.LogInfo("  " + prop.PropertyType.Name + " " + prop.Name + " = <error>");
                    }
                }
            }
            
            // Find a Prop component
            var allProps = UnityEngine.Object.FindObjectsOfType<Prop>();
            Log.LogInfo("Found " + allProps.Length + " Prop components in scene");
            
            if (allProps.Length > 0)
            {
                var prop = allProps[0];
                var propType = prop.GetType();
                Log.LogInfo("Prop Type: " + propType.FullName);
                
                // Get ALL methods
                Log.LogInfo("--- PROP METHODS ---");
                var methods = propType.GetMethods(System.Reflection.BindingFlags.Public | 
                                                   System.Reflection.BindingFlags.NonPublic | 
                                                   System.Reflection.BindingFlags.Instance |
                                                   System.Reflection.BindingFlags.DeclaredOnly);
                foreach (var method in methods)
                {
                    string paramStr = "";
                    foreach (var p in method.GetParameters())
                        paramStr += p.ParameterType.Name + " " + p.Name + ", ";
                    Log.LogInfo("  " + method.ReturnType.Name + " " + method.Name + "(" + paramStr.TrimEnd(',', ' ') + ")");
                }
                
                // Get fields
                Log.LogInfo("--- PROP FIELDS ---");
                var fields = propType.GetFields(System.Reflection.BindingFlags.Public | 
                                                 System.Reflection.BindingFlags.NonPublic | 
                                                 System.Reflection.BindingFlags.Instance);
                foreach (var field in fields)
                {
                    Log.LogInfo("  " + field.FieldType.Name + " " + field.Name);
                }
                
                // List a few props with their names
                Log.LogInfo("--- SAMPLE PROPS IN SCENE ---");
                int count = 0;
                foreach (var p in allProps)
                {
                    if (count++ > 20) break;
                    Log.LogInfo("  " + p.gameObject.name + " - " + GetGameObjectPath(p.gameObject));
                }
            }
            
            ShowNotification("Holder/Prop explored - check log!");
        }
        
        private void LogTransformRecursive(Transform t, string indent, int maxDepth)
        {
            if (maxDepth <= 0) return;
            Log.LogInfo(indent + t.name);
            foreach (Transform child in t)
            {
                LogTransformRecursive(child, indent + "  ", maxDepth - 1);
            }
        }
        
        private void FindNearbyColliders()
        {
            Log.LogInfo("=== FINDING COLLIDERS NEAR GOOSE ===");
            
            Vector3 goosePos = Vector3.zero;
            if (GameManager.instance != null && GameManager.instance.allGeese != null)
            {
                foreach (var goose in GameManager.instance.allGeese)
                {
                    if (goose != null && goose.isActiveAndEnabled)
                    {
                        goosePos = goose.transform.position;
                        break;
                    }
                }
            }
            
            if (goosePos == Vector3.zero)
            {
                Log.LogWarning("Could not find goose position!");
                return;
            }
            
            Log.LogInfo("Goose position: " + goosePos);
            
            // Find ALL colliders in the scene
            var allColliders = UnityEngine.Object.FindObjectsOfType<Collider>();
            Log.LogInfo("Total colliders in scene: " + allColliders.Length);
            
            float searchRadius = 5f;
            Log.LogInfo("Searching within radius: " + searchRadius);
            
            int found = 0;
            foreach (var col in allColliders)
            {
                if (col == null || !col.enabled) continue;
                
                // Check distance to collider bounds
                float distance = Vector3.Distance(goosePos, col.bounds.center);
                if (distance > searchRadius) continue;
                
                // Also check closest point
                Vector3 closest = col.ClosestPoint(goosePos);
                float closestDist = Vector3.Distance(goosePos, closest);
                
                if (closestDist <= searchRadius)
                {
                    string path = GetGameObjectPath(col.gameObject);
                    bool isActive = col.gameObject.activeInHierarchy;
                    string info = "[" + (isActive ? "ACTIVE" : "INACTIVE") + "] " + 
                                  col.GetType().Name + " @ dist=" + closestDist.ToString("F2") + 
                                  " - " + path;
                    Log.LogInfo(info);
                    found++;
                }
            }
            
            Log.LogInfo("Found " + found + " colliders within " + searchRadius + " units");
            ShowNotification("Found " + found + " nearby colliders - check log!");
        }
        
        private void SearchForSluiceBlockers()
        {
            Log.LogInfo("=== SEARCHING FOR HUB/GATE/BLOCKER OBJECTS ===");
            
            // Search for anything with these terms in the name
            string[] searchTerms = { "Hub", "hub", "blocker", "Blocker", "barrier", "Barrier", "GateSystem" };
            
            var allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            Log.LogInfo("Searching " + allObjects.Length + " GameObjects...");
            
            int found = 0;
            foreach (var obj in allObjects)
            {
                string path = GetGameObjectPath(obj);
                string lowerPath = path.ToLower();
                
                // Check if path contains any search term
                bool matches = false;
                foreach (var term in searchTerms)
                {
                    if (path.Contains(term))
                    {
                        matches = true;
                        break;
                    }
                }
                
                // Also specifically look for GROUP_Hub children
                if (path.Contains("GROUP_Hub"))
                {
                    matches = true;
                }
                
                if (matches)
                {
                    string info = "[" + (obj.activeSelf ? "ACTIVE" : "INACTIVE") + "] " + path;
                    
                    // Check for colliders
                    var colliders = obj.GetComponents<Collider>();
                    if (colliders.Length > 0)
                    {
                        info += " [HAS " + colliders.Length + " COLLIDER(S)]";
                    }
                    
                    // Check for SwitchSystem
                    var sw = obj.GetComponent<SwitchSystem>();
                    if (sw != null)
                    {
                        info += " [SWITCH state=" + sw.currentState + "]";
                    }
                    
                    Log.LogInfo(info);
                    found++;
                }
            }
            
            Log.LogInfo("Found " + found + " matching objects");
            
            // Also search specifically for GROUP_Hub children
            Log.LogInfo("");
            Log.LogInfo("=== DIRECT SEARCH: GROUP_Hub ===");
            var group = GameObject.Find("overworldStatic/GROUP_Hub");
            if (group != null)
            {
                LogAllChildren(group.transform, "");
            }
            else
            {
                Log.LogInfo("Could not find GROUP_Hub");
            }
            
            ShowNotification("Search complete - check BepInEx log!");
        }
        
        private void LogAllChildren(Transform parent, string indent)
        {
            foreach (Transform child in parent)
            {
                string info = indent + child.name;
                info += " [" + (child.gameObject.activeSelf ? "ON" : "OFF") + "]";
                
                var colliders = child.GetComponents<Collider>();
                if (colliders.Length > 0)
                {
                    info += " [" + colliders.Length + " colliders]";
                }
                
                var sw = child.GetComponent<SwitchSystem>();
                if (sw != null)
                {
                    info += " [SWITCH=" + sw.currentState + "]";
                }
                
                Log.LogInfo(info);
                
                if (indent.Length < 8) // Limit depth
                {
                    LogAllChildren(child, indent + "  ");
                }
            }
        }

        private System.Collections.IEnumerator DelayedGateInit()
        {
            yield return new WaitForSeconds(1.0f);
            
            Log.LogInfo("Running delayed gate initialization...");
            
            if (HasHighStreetAccess) OpenGatesForArea("HighStreet");
            if (HasBackGardensAccess) OpenGatesForArea("Backyards");
            if (HasPubAccess) OpenGatesForArea("Pub");
            if (HasModelVillageAccess) OpenGatesForArea("Finale");
            
            // Garden wooden gate is NOT opened - it's the player's first puzzle!
            // But we DO need to disable the hub-to-garden invisible wall so players can approach the garden
            var parkHubBlocker = GameObject.Find("highStreetDynamic/GROUP_Garage/irongate/GateSystem/GateExtraColliders/ParkHubGateExtraCollider");
            if (parkHubBlocker != null)
            {
                parkHubBlocker.SetActive(false);
                Log.LogInfo("Disabled ParkHubGateExtraCollider (hub-to-garden path)");
            }
            
            Log.LogInfo("Initial gate setup complete");
        }
        
        private string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }
        
        private Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name || child.name.Contains(name))
                {
                    return child;
                }
                Transform found = FindChildRecursive(child, name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        private static readonly Dictionary<string, string[]> AreaGates = new Dictionary<string, string[]>
        {
            { "HighStreet", new[] { 
                "gardenDynamic/GROUP_Hammering/gateTall/gateTallOpenSystem",
                "overworldStatic/GROUP_Hub/HallToHubGateSystem/HallToHubGateMainSystem",  // Hub to High Street
                "overworldStatic/GROUP_Hub/HallToHubGateSystem/HallToHubGateLockSystem"   // Lock system
            }},
            { "Backyards", new[] {
                "highStreetDynamic/GROUP_Garage/irongate/GateSystem"
            }},
            { "Pub", new[] {
                "pubDynamic/GROUP_pubItems/PubGateSystem",
                "overworldStatic/GROUP_BackyardToPub/SluiceGateSystem",
                "overworldStatic/GROUP_Hub/PubToHubGateSystem"  // Alternative pub entrance
            }},
            { "Finale", new[] {
                "pubDynamic/GROUP_BucketOnHead/PubToFinaleGateSystem",
                "pubDynamic/GROUP_BucketOnHead/PubToFinaleGateSystem/gate",
                "pubDynamic/GROUP_BucketOnHead/PubToFinaleGateSystem/gate/gateMetal",
                "overworldStatic/GROUP_ParkToPub/FinaleToParkGateSystem"  // Park to finale
            }},
            { "Garden", new[] {
                "gardenDynamic/GROUP_Gate/GardenGate/GateSystem",
                "overworldStatic/GROUP_Hub/HubGateSystem/HubGateMainSystem",  // Hub to Garden
                "overworldStatic/GROUP_Hub/HubGateSystem/LockSystem"          // Lock system
            }}
        };

        public void OpenGatesForArea(string areaName)
        {
            if (!AreaGates.ContainsKey(areaName))
            {
                Log.LogWarning("No gates defined for area: " + areaName);
                return;
            }

            Log.LogInfo("Opening gates for area: " + areaName);
            
            // STEP 1: Set the save flag that the game uses to track goal completion
            // This is what F11 does and it triggers animations!
            switch (areaName)
            {
                case "HighStreet":
                    SaveGameData.SetBoolValue("goalHammering", true, false);
                    Log.LogInfo("  Set goalHammering = true");
                    break;
                case "Backyards":
                    SaveGameData.SetBoolValue("goalGarage", true, false);
                    Log.LogInfo("  Set goalGarage = true");
                    break;
                case "Pub":
                    SaveGameData.SetBoolValue("goalPrune", true, false);
                    Log.LogInfo("  Set goalPrune = true");
                    break;
                case "Finale":
                    SaveGameData.SetBoolValue("goalBucket", true, false);
                    Log.LogInfo("  Set goalBucket = true");
                    break;
            }
            
            // STEP 2: Trigger relevant switch events
            string[] eventsToTry = null;
            switch (areaName)
            {
                case "HighStreet":
                    eventsToTry = new[] { "unlockHighStreet", "openHighStreet", "goalHammering" };
                    break;
                case "Backyards":
                    eventsToTry = new[] { "unlockBackyards", "openBackyards", "goalGarage" };
                    break;
                case "Pub":
                    eventsToTry = new[] { "unlockPub", "openPub", "goalPrune" };
                    break;
                case "Finale":
                    eventsToTry = new[] { "unlockFinale", "openFinale", "goalBucket" };
                    break;
            }
            
            if (eventsToTry != null)
            {
                foreach (var evt in eventsToTry)
                {
                    try 
                    { 
                        SwitchEventManager.TriggerEvent(evt);
                        Log.LogInfo("  Triggered event: " + evt);
                    } 
                    catch { }
                }
            }
            
            // STEP 2.5: Disable area-specific invisible blockers
            DisableAreaBlockers(areaName);

            // STEP 3: Also directly manipulate the SwitchSystem and colliders
            foreach (string gatePath in AreaGates[areaName])
            {
                Log.LogInfo("  Processing gate path: " + gatePath);
                
                // Find the gate object
                GameObject gateObj = GameObject.Find(gatePath);
                if (gateObj == null)
                {
                    // Try finding by partial path
                    var allSwitches = UnityEngine.Object.FindObjectsOfType<SwitchSystem>();
                    foreach (var sw in allSwitches)
                    {
                        string path = GetGameObjectPath(sw.gameObject);
                        if (path.EndsWith(gatePath) || path == gatePath)
                        {
                            gateObj = sw.gameObject;
                            break;
                        }
                    }
                }
                
                if (gateObj == null)
                {
                    Log.LogWarning("  Could not find gate: " + gatePath);
                    continue;
                }
                
                Log.LogInfo("  Found gate object: " + GetGameObjectPath(gateObj));
                
                // Set switch state
                var switchSystem = gateObj.GetComponent<SwitchSystem>();
                if (switchSystem != null)
                {
                    // Disable auto-closer first
                    Transform autoCloser = switchSystem.transform.Find("autoCloser");
                    if (autoCloser != null)
                    {
                        autoCloser.gameObject.SetActive(false);
                        Log.LogInfo("    Disabled autoCloser");
                    }
                    
                    switchSystem.SetState(1, null);
                    Log.LogInfo("    Set switch state to 1");
                }
                
                // Disable invisible walls/colliders
                DisableGateColliders(gateObj, areaName, gatePath);
            }
        }
        
        private void DisableGateColliders(GameObject gateObj, string areaName, string gatePath)
        {
            // Disable all the invisible walls based on gate type
            string[] collidersToDisable;
            
            if (gatePath.Contains("gateTallOpenSystem"))
            {
                // Search from parent (gateTall)
                Transform gateTall = gateObj.transform.parent;
                if (gateTall != null)
                {
                    DisableChildRecursive(gateTall, "GateTallExtraColliders");
                    DisableChildRecursive(gateTall, "meshLinks");
                }
            }
            else if (gatePath.Contains("SluiceGateSystem"))
            {
                // Sluice gate - this one is tricky
                // The collider might be on the gate itself
                DisableChildRecursive(gateObj.transform, "meshLinks");
                DisableChildRecursive(gateObj.transform, "cowCatcher");
                
                // Disable BoxColliders on gate metal by disabling the object
                Transform gate = gateObj.transform.Find("gate");
                if (gate != null)
                {
                    Transform gateMetal = gate.Find("gateMetal");
                    if (gateMetal != null)
                    {
                        // Try disabling collider children
                        Transform cowCatcher = gateMetal.Find("cowCatcher");
                        if (cowCatcher != null)
                        {
                            cowCatcher.gameObject.SetActive(false);
                            Log.LogInfo("    Disabled cowCatcher on gateMetal");
                        }
                    }
                }
            }
            else if (gatePath.Contains("PubToFinaleGateSystem"))
            {
                // Pub to Model Village gate
                DisableChildRecursive(gateObj.transform, "meshLinks");
                
                Transform gate = gateObj.transform.Find("gate");
                if (gate != null)
                {
                    Transform gateMetal = gate.Find("gateMetal");
                    if (gateMetal != null)
                    {
                        // Disable the collider on gateMetal
                        var colliders = gateMetal.GetComponents<Collider>();
                        foreach (var col in colliders)
                        {
                            col.enabled = false;
                            Log.LogInfo("    Disabled collider on gateMetal");
                        }
                        // Also try disabling the entire gateMetal if needed
                        gateMetal.gameObject.SetActive(false);
                        Log.LogInfo("    Disabled gateMetal object");
                    }
                }
            }
            else
            {
                // Generic - disable common collider children
                collidersToDisable = new[] { 
                    "meshLinks", 
                    "GateExtraColliders", 
                    "GateTallExtraColliders",
                    "gateStaffOnly",
                    "cowCatcher",
                    "collider",
                    "extraBeakBlocker"
                };
                
                foreach (var name in collidersToDisable)
                {
                    DisableChildRecursive(gateObj.transform, name);
                }
                
                // Also check parent
                if (gateObj.transform.parent != null)
                {
                    foreach (var name in collidersToDisable)
                    {
                        DisableChildRecursive(gateObj.transform.parent, name);
                    }
                }
            }
        }
        
        private void DisableAreaBlockers(string areaName)
        {
            Log.LogInfo("  Disabling area-specific blockers for: " + areaName);
            
            // These are invisible walls that are NOT part of the gate systems
            // Found via Shift+F8 search
            
            switch (areaName)
            {
                case "Pub":
                    // The invisible wall at the sluice gate - tied to the lady's sign
                    var pubBlocker = GameObject.Find("backyardsDynamic/GROUP_GoalPruneItems/messysign/messySignOriginalHome/extraCollider/pubBridgeGateExtraCollider");
                    if (pubBlocker != null)
                    {
                        pubBlocker.SetActive(false);
                        Log.LogInfo("    Disabled pubBridgeGateExtraCollider");
                    }
                    
                    // Also try the extraCollider parent
                    var extraCollider = GameObject.Find("backyardsDynamic/GROUP_GoalPruneItems/messysign/messySignOriginalHome/extraCollider");
                    if (extraCollider != null)
                    {
                        extraCollider.SetActive(false);
                        Log.LogInfo("    Disabled messysign extraCollider parent");
                    }
                    
                    // Canal one-way gate
                    var canalGate = GameObject.Find("overworldStatic/GROUP_BackyardToPub/GateCanallOneWay");
                    if (canalGate != null)
                    {
                        canalGate.SetActive(false);
                        Log.LogInfo("    Disabled GateCanallOneWay");
                    }
                    break;
                    
                case "Finale":
                    // Extra finale collider near pub
                    var finaleCollider = GameObject.Find("pubStatic/GROUP_BrickWalls/brickwallWithRailShortCollider/extraColliderSection/FinaleParkGateExtraCollider");
                    if (finaleCollider != null)
                    {
                        finaleCollider.SetActive(false);
                        Log.LogInfo("    Disabled FinaleParkGateExtraCollider");
                    }
                    
                    // Toggleable finale walls on the bridge
                    var toggleWalls = GameObject.Find("overworldStatic/GROUP_BackyardToPub/WatermillAndSluiceGates/Platform&Footbridge/brickbridge/ToggleableFinaleWalls");
                    if (toggleWalls != null)
                    {
                        var sw = toggleWalls.GetComponent<SwitchSystem>();
                        if (sw != null)
                        {
                            sw.SetState(1, null);
                            Log.LogInfo("    Set ToggleableFinaleWalls to state 1");
                        }
                        
                        // Also directly disable wallColliders child
                        Transform wallColliders = toggleWalls.transform.Find("wallColliders");
                        if (wallColliders != null)
                        {
                            wallColliders.gameObject.SetActive(false);
                            Log.LogInfo("    Disabled ToggleableFinaleWalls/wallColliders");
                        }
                    }
                    break;
                    
                case "HighStreet":
                    // Hub to High Street gate - disable ALL colliders in the gate system
                    DisableAllCollidersInPath("overworldStatic/GROUP_Hub/HallToHubGateSystem");
                    
                    // The invisible wall blocking hub-to-highstreet is actually on the iron gate!
                    var alleyHubBlocker = GameObject.Find("highStreetDynamic/GROUP_Garage/irongate/GateSystem/GateExtraColliders/AlleyHubGateExtraCollider");
                    if (alleyHubBlocker != null)
                    {
                        alleyHubBlocker.SetActive(false);
                        Log.LogInfo("    Disabled AlleyHubGateExtraCollider");
                    }
                    break;
                    
                case "Backyards":
                    // Nothing extra needed currently
                    break;
                    
                case "Garden":
                    // Hub to Garden gate - disable ALL colliders in the gate system
                    DisableAllCollidersInPath("overworldStatic/GROUP_Hub/HubGateSystem");
                    
                    // There might be an extra collider on the iron gate blocking hub-to-garden too
                    var parkHubBlocker = GameObject.Find("highStreetDynamic/GROUP_Garage/irongate/GateSystem/GateExtraColliders/ParkHubGateExtraCollider");
                    if (parkHubBlocker != null)
                    {
                        parkHubBlocker.SetActive(false);
                        Log.LogInfo("    Disabled ParkHubGateExtraCollider");
                    }
                    break;
            }
        }
        
        private void DisableAllCollidersInPath(string path)
        {
            var obj = GameObject.Find(path);
            if (obj == null)
            {
                Log.LogWarning("    Could not find: " + path);
                return;
            }
            
            Log.LogInfo("    Disabling all colliders in: " + path);
            
            // Get all colliders in this object and all children
            var allColliders = obj.GetComponentsInChildren<Collider>(true);
            int count = 0;
            foreach (var col in allColliders)
            {
                if (col.enabled)
                {
                    col.enabled = false;
                    count++;
                }
            }
            Log.LogInfo("    Disabled " + count + " colliders");
            
            // Also set all SwitchSystems to state 1
            var allSwitches = obj.GetComponentsInChildren<SwitchSystem>(true);
            foreach (var sw in allSwitches)
            {
                if (sw.currentState != 1)
                {
                    sw.SetState(1, null);
                    Log.LogInfo("    Set " + sw.gameObject.name + " to state 1");
                }
            }
        }
        
        private void DisableChildRecursive(Transform parent, string childName)
        {
            Transform child = FindChildRecursive(parent, childName);
            if (child != null && child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                Log.LogInfo("    Disabled " + childName);
            }
        }
        
        private void OpenGateByType(GameObject gateObj, string areaName, string gatePath)
        {
            // Kept for potential manual rotation fallback
        }
        
        private void DisableChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                child = FindChildRecursive(parent, childName);
            }
            if (child != null)
            {
                child.gameObject.SetActive(false);
                Log.LogInfo("  Disabled " + childName);
            }
        }
        
        private void TriggerAnimatorEffect(SwitchSystem sw)
        {
            // Kept for potential future use
        }
        
        private void TriggerAnimator(Animator animator, string source)
        {
            // Kept for potential future use
        }

        public void OpenAllGates()
        {
            foreach (var area in AreaGates.Keys)
            {
                OpenGatesForArea(area);
            }
            ShowNotification("All gates opened!");
        }
    }

    // ==================== HARMONY PATCHES ====================

    [HarmonyPatch]
    public static class GoalPatches
    {
        [HarmonyPatch(typeof(Goal), "AwardGoalDuringGame")]
        [HarmonyPostfix]
        static void OnGoalAwarded(Goal __instance)
        {
            try
            {
                string goalName = __instance.goalInfo.goalName;
                if (!string.IsNullOrEmpty(goalName))
                {
                    Plugin.Log.LogInfo("Goal completed: " + goalName);
                    Plugin.Instance?.OnGoalCompleted(goalName);
                    
                    if (goalName == "goalFinale")
                    {
                        Plugin.Instance?.SendGoalComplete();
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("Goal patch error: " + ex.Message);
            }
        }
    }

    [HarmonyPatch]
    public static class AreaPatches
    {
        [HarmonyPatch(typeof(AreaTracker), "ChangeTo")]
        [HarmonyPrefix]
        static bool OnAreaChange(GoalListArea newArea, AreaTracker __instance)
        {
            try
            {
                if (Plugin.Instance == null) return true;
                
                if (!Plugin.Instance.CanEnterArea(newArea))
                {
                    Plugin.Log.LogInfo("Blocked entry to: " + newArea);
                    Plugin.Instance.OnAreaBlocked(newArea);
                    return false;
                }
                
                Plugin.Log.LogInfo("Allowed entry to: " + newArea);
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("Area patch error: " + ex.Message);
                return true;
            }
        }
    }

    [HarmonyPatch]
    public static class GoosePatches
    {
        [HarmonyPatch(typeof(Goose), "Shoo", typeof(GameObject))]
        [HarmonyPostfix]
        static void OnGooseShooed(Goose __instance, GameObject shooer)
        {
            try
            {
                Plugin.Log.LogInfo("Goose was shooed by: " + shooer?.name);
                Plugin.Instance?.OnGooseShooed();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("Shoo patch error: " + ex.Message);
            }
        }
    }

    [HarmonyPatch]
    public static class SwitchEventPatches
    {
        [HarmonyPatch(typeof(SwitchEventManager), "TriggerEvent")]
        [HarmonyPrefix]
        static bool OnTriggerEvent(string id)
        {
            try
            {
                if (Plugin.Instance == null) return true;
                
                switch (id)
                {
                    case "enterAreaHighstreet":
                        if (!Plugin.Instance.HasHighStreetAccess)
                        {
                            Plugin.Log.LogInfo("Blocked event: " + id);
                            Plugin.Instance.OnAreaBlocked(GoalListArea.Shops);
                            TeleportGooseToWell();
                            return false;
                        }
                        break;
                    case "enterAreaBackyards":
                        if (!Plugin.Instance.HasBackGardensAccess)
                        {
                            Plugin.Log.LogInfo("Blocked event: " + id);
                            Plugin.Instance.OnAreaBlocked(GoalListArea.Backyards);
                            TeleportGooseToWell();
                            return false;
                        }
                        break;
                    case "enterAreaPub":
                        if (!Plugin.Instance.HasPubAccess)
                        {
                            Plugin.Log.LogInfo("Blocked event: " + id);
                            Plugin.Instance.OnAreaBlocked(GoalListArea.Pub);
                            TeleportGooseToWell();
                            return false;
                        }
                        break;
                    case "enterAreaFinale":
                        if (!Plugin.Instance.HasModelVillageAccess)
                        {
                            Plugin.Log.LogInfo("Blocked event: " + id);
                            Plugin.Instance.OnAreaBlocked(GoalListArea.Finale);
                            TeleportGooseToWell();
                            return false;
                        }
                        break;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("SwitchEvent patch error: " + ex.Message);
                return true;
            }
        }
        
        static void TeleportGooseToWell()
        {
            Plugin.Instance?.TeleportGooseToWell();
        }
    }

    [HarmonyPatch]
    public static class MoverPatches
    {
        [HarmonyPatch(typeof(Mover), "Update")]
        [HarmonyPostfix]
        static void OnMoverUpdate(Mover __instance)
        {
            try
            {
                if (Plugin.Instance == null) return;
                
                float multiplier = Plugin.Instance.GetEffectiveSpeedMultiplier();
                if (Math.Abs(multiplier - 1.0f) > 0.01f)
                {
                    __instance.currentSpeed *= multiplier;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("Mover patch error: " + ex.Message);
            }
        }
    }
    
    // === ITEM PICKUP PATCHES ===
    // These are commented out until we discover the correct method signatures
    // Use Alt+F7 to explore Holder/Prop classes and find the right methods
    
    /*
    [HarmonyPatch]
    public static class HolderPatches
    {
        // Try patching Holder.Grab - this is likely the pickup method
        [HarmonyPatch(typeof(Holder), "Grab")]
        [HarmonyPostfix]
        static void OnGrab(Holder __instance, Prop prop)
        {
            try
            {
                if (prop == null) return;
                string propName = prop.gameObject.name;
                string propPath = GetPath(prop.gameObject);
                Plugin.Log.LogInfo("[HOLDER] Grabbed: " + propName + " (" + propPath + ")");
                Plugin.Instance?.OnItemPickedUp(propName, propPath);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("Holder.Grab patch error: " + ex.Message);
            }
        }
        
        // Try patching Holder.Drop
        [HarmonyPatch(typeof(Holder), "Drop")]
        [HarmonyPostfix]
        static void OnDrop(Holder __instance)
        {
            try
            {
                Plugin.Log.LogInfo("[HOLDER] Dropped item");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("Holder.Drop patch error: " + ex.Message);
            }
        }
        
        static string GetPath(GameObject obj)
        {
            string path = obj.name;
            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }
    }
    
    [HarmonyPatch]
    public static class PropPatches
    {
        // Try patching Prop methods that might fire on pickup
        [HarmonyPatch(typeof(Prop), "OnGrabbed")]
        [HarmonyPostfix]
        static void OnPropGrabbed(Prop __instance)
        {
            try
            {
                string propName = __instance.gameObject.name;
                Plugin.Log.LogInfo("[PROP] OnGrabbed: " + propName);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("Prop.OnGrabbed patch error: " + ex.Message);
            }
        }
        
        [HarmonyPatch(typeof(Prop), "OnDropped")]
        [HarmonyPostfix]
        static void OnPropDropped(Prop __instance)
        {
            try
            {
                string propName = __instance.gameObject.name;
                Plugin.Log.LogInfo("[PROP] OnDropped: " + propName);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("Prop.OnDropped patch error: " + ex.Message);
            }
        }
    }
    */
}