using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace GooseGameAP
{
    [BepInPlugin("com.archipelago.goosegame", "Goose Game Archipelago", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        public static Plugin Instance;
        
        public const long BASE_ID = 119000000;
        
        private Harmony harmony;
        private bool showUI = false;
        private bool hasInitializedGates = false;
        public bool HasInitializedGates => hasInitializedGates;
        
        // Component managers
        public UIManager UI { get; private set; }
        public ArchipelagoClient Client { get; private set; }
        public GateManager GateManager { get; private set; }
        public TrapManager TrapManager { get; private set; }
        public ItemTracker ItemTracker { get; private set; }
        public InteractionTracker InteractionTracker { get; private set; }
        public PositionTracker PositionTracker { get; private set; }
        public GooseColourManager GooseColour { get; private set; }
        public NPCManager NPCManager { get; private set; }
        public PropManager PropManager { get; private set; }
        public LocationHighlightManager LocationHighlight { get; private set; }
        
        // Area access flags (Hub is always accessible - starting area)
        public bool HasGardenAccess { get; set; } = false;
        public bool HasHighStreetAccess { get; set; } = false;
        public bool HasBackGardensAccess { get; set; } = false;
        public bool HasPubAccess { get; set; } = false;
        public bool HasModelVillageAccess { get; set; } = false;
        public bool HasGoldenBell { get; set; } = false;

        // Extra area access flags for teleporting
        public bool HasEnteredBackGardens { get; set; } = false;
        public bool HasEnteredPub { get; set; } = false;
        
        // NPC Soul flags
        public bool HasGroundskeeperSoul { get; set; } = false;
        public bool HasBoySoul { get; set; } = false;
        public bool HasTVShopOwnerSoul { get; set; } = false;
        public bool HasMarketLadySoul { get; set; } = false;
        public bool HasTidyNeighbourSoul { get; set; } = false;
        public bool HasMessyNeighbourSoul { get; set; } = false;
        public bool HasBurlyManSoul { get; set; } = false;
        public bool HasOldManSoul { get; set; } = false;
        public bool HasPubLadySoul { get; set; } = false;
        public bool HasFancyLadiesSoul { get; set; } = false;
        public bool HasCookSoul { get; set; } = false;
        
        // Soul settings from slot data
        public bool NPCSoulsEnabled => Client?.NPCSoulsEnabled ?? true;
        public bool PropSoulsEnabled => Client?.PropSoulsEnabled ?? true;
        public bool NewTasksEnabled => Client?.NewTasksEnabled ?? true;
        
        // Buff tracking
        public bool IsSilent => TrapManager?.IsSilent ?? false;
        public int MegaHonkCount => TrapManager?.MegaHonkCount ?? 0;
        
        // DeathLink
        public bool DeathLinkEnabled => Client?.DeathLinkEnabled ?? false;
        private bool deathLinkPending = false;
        
        // Location tracking
        private HashSet<long> checkedLocations = new HashSet<long>();
        public int CheckedLocationCount => checkedLocations.Count;
        
        public bool IsConnected => Client?.IsConnected ?? false;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            Log.LogInfo("Goose Game AP v1.0.0");
            
            // Initialize components
            UI = new UIManager();
            TrapManager = new TrapManager(this);
            GateManager = new GateManager(this);
            ItemTracker = new ItemTracker(this);
            InteractionTracker = new InteractionTracker(this);
            PositionTracker = new PositionTracker();
            GooseColour = new GooseColourManager(this);
            NPCManager = new NPCManager(this);
            PropManager = new PropManager(this);
            LocationHighlight = new LocationHighlightManager(this);
            Client = new ArchipelagoClient(this);
            
            harmony = new Harmony("com.archipelago.goosegame");
            harmony.PatchAll();
        }

        private void Update()
        {
            // Reset state when player returns to menu
            if (hasInitializedGates && (GameManager.instance == null || 
                GameManager.instance.allGeese == null || GameManager.instance.allGeese.Count == 0))
            {
                hasInitializedGates = false;
                GateManager?.ResetFinale();
                NPCManager?.Reset();
                PropManager?.Reset();
                LocationHighlight?.Reset();
                SwitchSystemPatches.ResetSandcastleTracking();
            }
            
            // Initialize gates once when game scene loads
            if (!hasInitializedGates && GameManager.instance != null && 
                GameManager.instance.allGeese != null && GameManager.instance.allGeese.Count > 0)
            {
                hasInitializedGates = true;
                
                GooseColour?.RefreshRenderers();
                PositionTracker?.ScanAndCacheItems();
                
                StartCoroutine(DelayedGateInit());
            }
            
            // Update NPCManager
            NPCManager?.Update();
            
            // Update PropManager
            PropManager?.Update();
            
            // Toggle UI
            if (Input.GetKeyDown(KeyCode.F1))
            {
                showUI = !showUI;
            }
            
            // Debug keys
            HandleDebugKeys();
            
            // Process AP messages
            Client?.ProcessQueuedMessages();
            
            // Handle gate sync timing
            HandleGateSyncTiming();
            
            // Check for finale trigger (goose holding golden bell in pub area)
            CheckFinaleStart();
            
            // Handle DeathLink
            if (deathLinkPending)
            {
                if (GameManager.instance != null && GameManager.instance.allGeese != null && GameManager.instance.allGeese.Count > 0)
                {
                    deathLinkPending = false;
                    TrapManager.ForceDropItems();
                    GateManager?.TeleportGooseToWell();
                    UI?.ShowNotification("DeathLink! Another player died!");
                }
            }
            
            // Update traps
            TrapManager?.Update();
            
            // Update goose color
            GooseColour?.Update();
            
            // Update notification timer
            UI?.UpdateNotification(Time.deltaTime);
            
            // Track pickups/drags
            ItemTracker?.Update();
            
            // Update location highlights (visual feedback for unchecked items)
            LocationHighlight?.Update();
        }
        
        private void LateUpdate()
        {
        }
        
        private void HandleDebugKeys()
        {
            // F2 key: Toggle soul tracker overlay
            if (Input.GetKeyDown(KeyCode.F2))
            {
                UI?.ToggleSoulTracker();
            }
            
            // F3 key: Toggle server log overlay
            if (Input.GetKeyDown(KeyCode.F3))
            {
                UI?.ToggleServerLog();
            }
            
            // F4 key: Toggle server log overlay
            if (Input.GetKeyDown(KeyCode.F4))
            {
                UI?.ToggleNewTasksTracker();
            }
            
            // F5 key: Toggle location highlighting (sparkles on unchecked items)
            if (Input.GetKeyDown(KeyCode.F5))
            {
                LocationHighlight?.Toggle();
                string state = LocationHighlight?.HighlightingEnabled == true ? "ON" : "OFF";
                UI?.ShowNotification($"Location highlighting: {state}");
            }
            
            // F6 key: Cycle highlight color (Shift+F6 for backwards)
            if (Input.GetKeyDown(KeyCode.F6))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    LocationHighlight?.CycleColorBack();
                }
                else
                {
                    LocationHighlight?.CycleColor();
                }
                UI?.ShowNotification($"Highlight color: {LocationHighlight?.CurrentColorName}");
            }


            // G key: Use a stored Goose Day
            if (Input.GetKeyDown(KeyCode.G) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
            {
                TrapManager?.UseGooseDay(15f);
            }
            
            // H key: Warp to Hub (only if not holding anything)
            if (Input.GetKeyDown(KeyCode.H) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
            {
                WarpToHub();
            }

            // 0 key: Warp to Start (only if not holding anything)
            if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
            {
                WarpToStart();
            }

            // Number keys 1-5: Warp to areas (if unlocked and not holding anything)
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                WarpToArea("Garden", HasGardenAccess, GateManager.GardenPosition);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                if (PropManager.HasReceivedSoul("Chalk") && !checkedLocations.Contains(119000016))
                {
                    int numTasks = 0;
                    if (checkedLocations.Contains(119000010)) numTasks++;
                    if (checkedLocations.Contains(119000011)) numTasks++;
                    if (checkedLocations.Contains(119000012)) numTasks++;
                    if (checkedLocations.Contains(119000013)) numTasks++;
                    if (checkedLocations.Contains(119000014)) numTasks++;
                    if (checkedLocations.Contains(119000015)) numTasks++;
                    
                    if (numTasks >= 5)
                    {
                        WarpToArea("High Street", HasHighStreetAccess, GateManager.HighStreetTemporaryPosition);
                    }
                    else
                    {
                        WarpToArea("High Street", HasHighStreetAccess, GateManager.HighStreetPosition);
                    }
                }
                else
                {
                    WarpToArea("High Street", HasHighStreetAccess, GateManager.HighStreetPosition);
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                if (!HasEnteredBackGardens)
                {
                    WarpToArea("Back Gardens", HasBackGardensAccess, GateManager.BackGardensPosition);
                }
                else
                {
                    WarpToArea("Back Gardens", HasBackGardensAccess, GateManager.BackGardensUpdatedPosition);
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                if (!HasEnteredPub)
                {
                    WarpToArea("Pub", HasPubAccess, GateManager.PubPosition);
                }
                else
                {
                    WarpToArea("Pub", HasPubAccess, GateManager.PubUpdatedPosition);
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
            {
                WarpToArea("Model Village", HasModelVillageAccess, GateManager.ModelVillagePosition);
            }
            
            // C key: Cycle goose color
            if (Input.GetKeyDown(KeyCode.C) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
            {
                GooseColour?.CycleColour();
                UI?.ShowNotification($"Goose Colour: {GooseColour?.CurrentColourName}");
            }
            
            // Ctrl+C: Reset goose color to default
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.C))
            {
                GooseColour?.ResetToDefault();
                UI?.ShowNotification("Goose color reset");
            }
            
            // F9: Manual gate sync from access flags
            if (Input.GetKeyDown(KeyCode.F9))
            {
                LoadAccessFlags();
                ClearHubBlockers();
                GateManager?.SyncGatesFromAccessFlags();
                UI?.ShowNotification("Gates re-synced!");
            }
            
            // F10: Log current goose position
            if (Input.GetKeyDown(KeyCode.F10))
            {
                if (GameManager.instance?.allGeese != null)
                {
                    foreach (var goose in GameManager.instance.allGeese)
                    {
                        if (goose != null && goose.isActiveAndEnabled)
                        {
                            Vector3 pos = goose.transform.position;
                            string posStr = $"({pos.x:F1}f, {pos.y:F1}f, {pos.z:F1}f)";
                            Log.LogInfo($"[DEBUG] Goose position: {posStr}");
                            UI?.ShowNotification($"Pos: {posStr}");
                            break;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Warp to a specific area if unlocked and not holding anything
        /// </summary>
        private void WarpToArea(string areaName, bool hasAccess, Vector3 position)
        {
            if (!hasAccess)
            {
                UI?.ShowNotification($"{areaName} is not unlocked!");
                return;
            }
            
            if (!CanWarp(out string reason))
            {
                UI?.ShowNotification(reason);
                return;
            }
            
            GateManager?.TeleportGoose(position);
            UI?.ShowNotification($"Warped to {areaName}!");
        }
        
        /// <summary>
        /// Check if the goose can warp (not holding or dragging anything)
        /// </summary>
        private bool CanWarp(out string reason)
        {
            reason = "";
            
            if (GameManager.instance?.allGeese == null || GameManager.instance.allGeese.Count == 0)
            {
                reason = "No goose found!";
                return false;
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
                reason = "No active goose found!";
                return false;
            }
            
            // Check if holding something in beak
            var holder = goose.GetComponent<Holder>();
            if (holder != null && holder.holding != null)
            {
                reason = "Can't warp while holding something!";
                return false;
            }
            
            // Check if dragging something
            bool isDragging = false;
            
            var dragger = goose.GetComponent<Dragger>();
            if (dragger != null)
            {
                var activeField = dragger.GetType().GetField("active",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                
                if (activeField != null)
                {
                    isDragging = (bool)activeField.GetValue(dragger);
                }
            }
            
            if (!isDragging)
            {
                foreach (var comp in goose.GetComponents<Component>())
                {
                    if (comp.GetType().Name == "Goose")
                    {
                        var draggerField = comp.GetType().GetField("dragger",
                            System.Reflection.BindingFlags.Public |
                            System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Instance);
                        
                        if (draggerField != null)
                        {
                            var draggerObj = draggerField.GetValue(comp);
                            if (draggerObj != null)
                            {
                                var activeField = draggerObj.GetType().GetField("active",
                                    System.Reflection.BindingFlags.Public |
                                    System.Reflection.BindingFlags.NonPublic |
                                    System.Reflection.BindingFlags.Instance);
                                
                                if (activeField != null)
                                {
                                    isDragging = (bool)activeField.GetValue(draggerObj);
                                }
                            }
                        }
                        break;
                    }
                }
            }
            
            if (isDragging)
            {
                reason = "Can't warp while dragging something!";
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Check if the finale should start (goose holding golden bell enters pub area)
        /// </summary>
        private void CheckFinaleStart()
        {
            // Skip if finale already active or GateManager not ready
            if (GateManager == null || GateManager.FinaleActive) return;
            
            try
            {
                if (GameManager.instance?.allGeese == null) return;
                
                foreach (var goose in GameManager.instance.allGeese)
                {
                    if (goose == null || !goose.isActiveAndEnabled) continue;
                    
                    // Check if goose is holding the golden bell
                    var holder = goose.GetComponent<Holder>();
                    if (holder == null || holder.holding == null) continue;
                    
                    string heldName = holder.holding.name.ToLower();
                    if (!heldName.Contains("goldenbell") && !heldName.Contains("golden") && !heldName.Contains("bell")) continue;
                    
                    // Goose is holding the bell - check position
                    // Pub area is roughly around x: -20 to -40, z: 0 to 20
                    Vector3 pos = goose.transform.position;
                    
                    // If goose with bell is in pub area (coming back from model village)
                    // The pub entrance from model village is around x: -25, z: 10
                    if (pos.x < -15f && pos.z > -5f && pos.z < 25f)
                    {
                        Log.LogInfo("[FINALE] Golden bell detected in pub area - triggering finale!");
                        GateManager.OnFinaleStart();
                        UI?.ShowNotification("FINALE! The chase begins!");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[FINALE] CheckFinaleStart error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Warp the goose to the Hub (well position) - only works if not holding/dragging anything
        /// </summary>
        public void WarpToHub()
        {
            if (!CanWarp(out string reason))
            {
                UI?.ShowNotification(reason);
                return;
            }
            
            // All clear - teleport to hub
            GateManager?.TeleportGooseToWell();
            UI?.ShowNotification("Warped to Hub!");
        }
        
        /// <summary>
        /// Warp the goose to the Start (around the tree stump) - only works if not holding/dragging anything
        /// </summary>
        public void WarpToStart()
        {
            if (!CanWarp(out string reason))
            {
                UI?.ShowNotification(reason);
                return;
            }
            
            // All clear - teleport to start
            GateManager?.TeleportGooseToStart();
            UI?.ShowNotification("Warped to Start!");
        }
        
        private void HandleGateSyncTiming()
        {
            if (Client == null) return;
            
            if (Client.PendingGateSync && GameManager.instance != null && 
                GameManager.instance.allGeese != null && GameManager.instance.allGeese.Count > 0)
            {
                Client.GateSyncTimer += Time.deltaTime;
                if (Client.GateSyncTimer >= 2.0f)
                {
                    Client.GateSyncAttempts++;
                    Client.GateSyncTimer = 0f;
                    
                    ClearHubBlockers();
                    GateManager?.SyncGatesFromAccessFlags();
                    
                    if (Client.GateSyncAttempts >= ArchipelagoClient.MAX_GATE_SYNC_ATTEMPTS)
                    {
                        Client.PendingGateSync = false;
                    }
                }
            }
            
            if (Client.WaitingForReceivedItems && Client.IsConnected)
            {
                Client.ReceivedItemsTimeout += Time.deltaTime;
                if (Client.ReceivedItemsTimeout >= 5.0f)
                {
                    Log.LogWarning("ReceivedItems timeout - syncing gates with current access flags");
                    Client.WaitingForReceivedItems = false;
                    Client.ReceivedItemsTimeout = 0f;
                    Client.PendingGateSync = true;
                    Client.GateSyncTimer = 0f;
                }
            }
        }

        private void OnGUI()
        {
            // Draw soul tracker overlay (always if visible, even when main UI is hidden)
            UI?.DrawSoulTracker(this);
            
            // Draw server log overlay (always if visible, even when main UI is hidden)
            UI?.DrawServerLog(this);
            
            // Draw extra task list overlay (always if visible, even when main UI is hidden)
            UI?.DrawNewTasksTracker(this);
            
            if (!showUI) return;
            UI?.DrawUI(this);
        }
        

        // === PUBLIC API FOR OTHER COMPONENTS ===
        
        public void Connect()
        {
            string savedSlot = PlayerPrefs.GetString("AP_SlotName", "");
            string savedServer = PlayerPrefs.GetString("AP_ServerAddress", "");
            
            Log.LogInfo($"[AP] Connect called. Saved: {savedSlot}@{savedServer}, Current: {UI.SlotName}@{UI.ServerAddress}");
            
            bool slotMatches = !string.IsNullOrEmpty(savedSlot) && 
                               savedSlot == UI.SlotName && 
                               savedServer == UI.ServerAddress;
            
            if (slotMatches)
            {
                Log.LogInfo($"[AP] Reconnecting to same slot ({UI.SlotName}@{UI.ServerAddress}). Loading saved state.");
                LoadAccessFlags();
                Client?.LoadSavedSoulSettings();
                PropManager?.LoadSouls();
                GateManager?.SyncGatesFromAccessFlags();
                NPCManager?.RefreshNPCStates();
                PropManager?.RefreshPropStates();
            }
            else
            {
                Log.LogInfo($"[AP] New/different slot. Clearing ALL saved state.");
                ResetAllAccess();
                PropManager?.ClearAllSouls();
                PropManager?.ClearSavedSouls();
                GateManager?.SyncGatesFromAccessFlags();
                NPCManager?.RefreshNPCStates();
                PropManager?.RefreshPropStates();
            }
            
            PlayerPrefs.SetString("AP_SlotName", UI.SlotName);
            PlayerPrefs.SetString("AP_ServerAddress", UI.ServerAddress);
            PlayerPrefs.Save();
            Log.LogInfo($"[AP] Saved new slot info: {UI.SlotName}@{UI.ServerAddress}");
            
            Client?.Connect(UI.ServerAddress, UI.ServerPort, UI.SlotName, UI.Password, DeathLinkEnabled);
        }
        
        /// <summary>
        /// Called by ArchipelagoClient when connection is complete and slot_data is parsed
        /// </summary>
        public void OnConnectionComplete()
        {
            Log.LogInfo($"[AP] OnConnectionComplete - NPCSouls={NPCSoulsEnabled}, PropSouls={PropSoulsEnabled}");
            
            // Refresh NPC and Prop states now that we know the settings
            NPCManager?.RefreshNPCStates();
            PropManager?.RefreshPropStates();
        }
        
        public void Disconnect()
        {
            Client?.Disconnect();
        }
        
        public void SendLocationCheck(long locationId)
        {
            if (!checkedLocations.Contains(locationId))
            {
                checkedLocations.Add(locationId);
                Log.LogInfo($"[LOC-PLUGIN] Sending location check: {locationId} = {LocationMappings.GetLocationName(locationId)}");
                Client?.SendLocationCheck(locationId);

                CheckSendGoalLocations(locationId);
            }
        }

        public void CheckSendGoalLocations(long locationId)
        {
            long offset = locationId - BASE_ID;

            switch (offset)
            {
                case 1: case 2: case 3: case 4: case 5: case 6: //All Garden Tasks Complete
                    if (checkedLocations.Contains(1 + BASE_ID) && checkedLocations.Contains(2 + BASE_ID)
                        && checkedLocations.Contains(3 + BASE_ID) && checkedLocations.Contains(4 + BASE_ID) && checkedLocations.Contains(5 + BASE_ID)
                        && checkedLocations.Contains(6 + BASE_ID) && checkedLocations.Contains(7 + BASE_ID))
                    {
                        SendLocationCheck(81 + BASE_ID);
                    }
                    break;
                case 7: //All Garden Tasks Complete, and Four Final Tasks
                    if (checkedLocations.Contains(1 + BASE_ID) && checkedLocations.Contains(2 + BASE_ID)
                        && checkedLocations.Contains(3 + BASE_ID) && checkedLocations.Contains(4 + BASE_ID) && checkedLocations.Contains(5 + BASE_ID)
                        && checkedLocations.Contains(6 + BASE_ID) && checkedLocations.Contains(7 + BASE_ID))
                    {
                        SendLocationCheck(81 + BASE_ID);
                    }
                    if (checkedLocations.Contains(7 + BASE_ID) && checkedLocations.Contains(16 + BASE_ID)
                        && checkedLocations.Contains(26 + BASE_ID) && checkedLocations.Contains(37 + BASE_ID))
                    {
                        SendLocationCheck(94 + BASE_ID);
                    }
                    break;
                case 10: case 11: case 12: case 13: case 14: case 15: // All High Street Tasks Complete
                    if (checkedLocations.Contains(10 + BASE_ID) && checkedLocations.Contains(11 + BASE_ID)
                        && checkedLocations.Contains(12 + BASE_ID) && checkedLocations.Contains(13 + BASE_ID) && checkedLocations.Contains(14 + BASE_ID)
                        && checkedLocations.Contains(15 + BASE_ID) && checkedLocations.Contains(16 + BASE_ID))
                    {
                        SendLocationCheck(82 + BASE_ID);
                    }
                    break;
                case 16: // All High Street Tasks Complete, and Four Final Tasks
                    if (checkedLocations.Contains(10 + BASE_ID) && checkedLocations.Contains(11 + BASE_ID)
                        && checkedLocations.Contains(12 + BASE_ID) && checkedLocations.Contains(13 + BASE_ID) && checkedLocations.Contains(14 + BASE_ID)
                        && checkedLocations.Contains(15 + BASE_ID) && checkedLocations.Contains(16 + BASE_ID))
                    {
                        SendLocationCheck(82 + BASE_ID);
                    }
                    if (checkedLocations.Contains(7 + BASE_ID) && checkedLocations.Contains(16 + BASE_ID)
                        && checkedLocations.Contains(26 + BASE_ID) && checkedLocations.Contains(37 + BASE_ID))
                    {
                        SendLocationCheck(94 + BASE_ID);
                    }
                    break;
                case 20: case 21: case 22: case 23: case 24: case 25: // All Back Gardens Tasks Complete
                    HasEnteredBackGardens = true;
                    if (checkedLocations.Contains(20 + BASE_ID) && checkedLocations.Contains(21 + BASE_ID)
                        && checkedLocations.Contains(22 + BASE_ID) && checkedLocations.Contains(23 + BASE_ID) && checkedLocations.Contains(24 + BASE_ID)
                        && checkedLocations.Contains(25 + BASE_ID) && checkedLocations.Contains(26 + BASE_ID))
                    {
                        SendLocationCheck(83 + BASE_ID);
                    }
                    break;
                case 26: // All Back Gardens Tasks Complete, and Four Final Tasks
                    HasEnteredBackGardens = true;
                    if (checkedLocations.Contains(20 + BASE_ID) && checkedLocations.Contains(21 + BASE_ID)
                        && checkedLocations.Contains(22 + BASE_ID) && checkedLocations.Contains(23 + BASE_ID) && checkedLocations.Contains(24 + BASE_ID)
                        && checkedLocations.Contains(25 + BASE_ID) && checkedLocations.Contains(26 + BASE_ID))
                    {
                        SendLocationCheck(83 + BASE_ID);
                    }
                    if (checkedLocations.Contains(7 + BASE_ID) && checkedLocations.Contains(16 + BASE_ID)
                        && checkedLocations.Contains(26 + BASE_ID) && checkedLocations.Contains(37 + BASE_ID))
                    {
                        SendLocationCheck(94 + BASE_ID);
                    }
                    break;
                case 30: case 31: case 32: case 33: case 34: case 35: case 36: // All Pub Tasks Complete
                    HasEnteredPub = true;
                    if (checkedLocations.Contains(30 + BASE_ID) && checkedLocations.Contains(31 + BASE_ID) && checkedLocations.Contains(32 + BASE_ID)
                        && checkedLocations.Contains(33 + BASE_ID) && checkedLocations.Contains(34 + BASE_ID) && checkedLocations.Contains(35 + BASE_ID)
                        && checkedLocations.Contains(36 + BASE_ID) && checkedLocations.Contains(37 + BASE_ID))
                    {
                        SendLocationCheck(84 + BASE_ID);
                    }
                    break;
                case 37: // All Pub Tasks Complete, and Four Final Tasks
                    HasEnteredPub = true;
                    if (checkedLocations.Contains(30 + BASE_ID) && checkedLocations.Contains(31 + BASE_ID) && checkedLocations.Contains(32 + BASE_ID)
                        && checkedLocations.Contains(33 + BASE_ID) && checkedLocations.Contains(34 + BASE_ID) && checkedLocations.Contains(35 + BASE_ID)
                        && checkedLocations.Contains(36 + BASE_ID) && checkedLocations.Contains(37 + BASE_ID))
                    {
                        SendLocationCheck(84 + BASE_ID);
                    }
                    if (checkedLocations.Contains(7 + BASE_ID) && checkedLocations.Contains(16 + BASE_ID)
                        && checkedLocations.Contains(26 + BASE_ID) && checkedLocations.Contains(37 + BASE_ID))
                    {
                        SendLocationCheck(94 + BASE_ID);
                    }
                    break;
                case 40:
                    SendLocationCheck(93 + BASE_ID);
                    break;
                case 50: case 51: case 52: case 53: case 54: case 55: case 56: // All To Do (As Well) Tasks Complete
                case 60: case 61: case 62: case 63: case 64: case 65: case 66: case 67: // - should only be enabled if post-game goals are enabled
                    if (checkedLocations.Contains(50 + BASE_ID) && checkedLocations.Contains(51 + BASE_ID) && checkedLocations.Contains(52 + BASE_ID)
                        && checkedLocations.Contains(53 + BASE_ID) && checkedLocations.Contains(54 + BASE_ID) && checkedLocations.Contains(55 + BASE_ID)
                        && checkedLocations.Contains(56 + BASE_ID) && checkedLocations.Contains(60 + BASE_ID) && checkedLocations.Contains(61 + BASE_ID)
                        && checkedLocations.Contains(62 + BASE_ID) && checkedLocations.Contains(63 + BASE_ID) && checkedLocations.Contains(64 + BASE_ID)
                        && checkedLocations.Contains(65 + BASE_ID) && checkedLocations.Contains(66 + BASE_ID) && checkedLocations.Contains(67 + BASE_ID))
                    {
                        SendLocationCheck(85 + BASE_ID);
                    }
                    break;
                case 70: case 71: // All Speedrun Tasks Complete
                case 72: case 73: // - should only be enabled if speedrun goals are enabled
                    if (checkedLocations.Contains(70 + BASE_ID) && checkedLocations.Contains(71 + BASE_ID)
                        && checkedLocations.Contains(72 + BASE_ID) && checkedLocations.Contains(73 + BASE_ID))
                    {
                        SendLocationCheck(86 + BASE_ID);
                        SendLocationCheck(87 + BASE_ID);
                    }
                    break;
                case 81: case 82:
                case 83: case 84:
                    if (checkedLocations.Contains(81 + BASE_ID) && checkedLocations.Contains(82 + BASE_ID)
                        && checkedLocations.Contains(83 + BASE_ID) && checkedLocations.Contains(84 + BASE_ID))
                    {
                        SendLocationCheck(88 + BASE_ID);
                        SendLocationCheck(89 + BASE_ID);
                    }
                    break;
                case 85: case 86: case 88:
                    if (checkedLocations.Contains(85 + BASE_ID) && checkedLocations.Contains(86 + BASE_ID) && checkedLocations.Contains(88 + BASE_ID))
                    {
                        SendLocationCheck(90 + BASE_ID);
                        SendLocationCheck(91 + BASE_ID);
                        SendLocationCheck(92 + BASE_ID);
                    }
                    else if (checkedLocations.Contains(85 + BASE_ID) && checkedLocations.Contains(88 + BASE_ID))
                    {
                        SendLocationCheck(92 + BASE_ID);
                    }
                    break;

                // Break boards
                case 1502:
                    HasEnteredBackGardens = true;
                    break;

                // Model church first-time pecks
                case 1350:
                    SendLocationCheck(1390 + BASE_ID);
                    break;
                case 1369:
                    SendLocationCheck(1391 + BASE_ID);
                    break;
                
                // Dress up the bush with both ribbons
                case 9998: case 9999:
                    if (checkedLocations.Contains(9998 + BASE_ID) && checkedLocations.Contains(9999 + BASE_ID))
                    {
                        SendLocationCheck(1507 + BASE_ID);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Check if a location has been sent to AP server
        /// </summary>
        public bool IsLocationChecked(long locationId)
        {
            return checkedLocations.Contains(locationId);
        }
        
        /// <summary>
        /// Load checked locations received from AP server on connect
        /// </summary>
        public void LoadCheckedLocations(List<long> locationIds)
        {
            foreach (var id in locationIds)
            {
                checkedLocations.Add(id);
                CheckSendGoalLocations(id);
            }
            Log.LogInfo($"[AP] Loaded {locationIds.Count} checked locations from server");
        }
        
        public void SendGoalComplete()
        {
            Client?.SendGoalComplete();
        }
        
        public void TriggerDeathLink()
        {
            if (DeathLinkEnabled)
            {
                deathLinkPending = true;
                UI?.AddChatMessage("DeathLink received!");
            }
        }
        
        public void TeleportGooseToWell()
        {
            GateManager?.TeleportGooseToWell();
        }
        
        public void ProcessReceivedItem(long itemId)
        {
            long offset = itemId - BASE_ID;
            
            switch (offset)
            {
                case 100:
                    HasGardenAccess = true;
                    GateManager?.TriggerAreaUnlock("Garden");
                    GateManager?.OpenGatesForArea("Garden");
                    UI?.ShowNotification("Garden is now accessible!");
                    SaveAccessFlags();
                    break;
                case 101:
                    HasHighStreetAccess = true;
                    GateManager?.TriggerAreaUnlock("HighStreet");
                    GateManager?.OpenGatesForArea("HighStreet");
                    UI?.ShowNotification("High Street is now accessible!");
                    SaveAccessFlags();
                    break;
                case 102:
                    HasBackGardensAccess = true;
                    GateManager?.TriggerAreaUnlock("Backyards");
                    GateManager?.OpenGatesForArea("Backyards");
                    UI?.ShowNotification("Back Gardens are now accessible!");
                    SaveAccessFlags();
                    break;
                case 103:
                    HasPubAccess = true;
                    GateManager?.TriggerAreaUnlock("Pub");
                    GateManager?.OpenGatesForArea("Pub");
                    UI?.ShowNotification("The Pub is now accessible!");
                    SaveAccessFlags();
                    break;
                case 104:
                    HasModelVillageAccess = true;
                    GateManager?.TriggerAreaUnlock("Finale");
                    GateManager?.OpenGatesForArea("Finale");
                    UI?.ShowNotification("Model Village is now accessible!");
                    SaveAccessFlags();
                    break;
                
                // NPC Souls (120-129) - only process if NPC souls enabled
                case 120:
                    HasGroundskeeperSoul = true;
                    if (NPCSoulsEnabled) NPCManager?.EnableNPC("Groundskeeper");
                    UI?.ShowNotification("Groundskeeper Soul received!");
                    SaveAccessFlags();
                    break;
                case 121:
                    HasBoySoul = true;
                    if (NPCSoulsEnabled) NPCManager?.EnableNPC("Boy");
                    UI?.ShowNotification("Boy Soul received!");
                    SaveAccessFlags();
                    break;
                case 122:
                    HasTVShopOwnerSoul = true;
                    if (NPCSoulsEnabled) NPCManager?.EnableNPC("TVShopOwner");
                    UI?.ShowNotification("TV Shop Owner Soul received!");
                    SaveAccessFlags();
                    break;
                case 123:
                    HasMarketLadySoul = true;
                    if (NPCSoulsEnabled) NPCManager?.EnableNPC("MarketLady");
                    UI?.ShowNotification("Market Lady Soul received!");
                    SaveAccessFlags();
                    break;
                case 124:
                    HasTidyNeighbourSoul = true;
                    if (NPCSoulsEnabled) NPCManager?.EnableNPC("TidyNeighbour");
                    UI?.ShowNotification("Tidy Neighbour Soul received!");
                    SaveAccessFlags();
                    break;
                case 125:
                    HasMessyNeighbourSoul = true;
                    if (NPCSoulsEnabled) NPCManager?.EnableNPC("MessyNeighbour");
                    UI?.ShowNotification("Messy Neighbour Soul received!");
                    SaveAccessFlags();
                    break;
                case 126:
                    HasBurlyManSoul = true;
                    if (NPCSoulsEnabled) NPCManager?.EnableNPC("BurlyMan");
                    UI?.ShowNotification("Burly Man Soul received!");
                    SaveAccessFlags();
                    break;
                case 127:
                    HasOldManSoul = true;
                    if (NPCSoulsEnabled) NPCManager?.EnableNPC("OldMan");
                    UI?.ShowNotification("Old Man Soul received!");
                    SaveAccessFlags();
                    break;
                case 128:
                    HasPubLadySoul = true;
                    if (NPCSoulsEnabled) NPCManager?.EnableNPC("PubLady");
                    UI?.ShowNotification("Pub Lady Soul received!");
                    SaveAccessFlags();
                    break;
                case 129:
                    HasFancyLadiesSoul = true;
                    if (NPCSoulsEnabled) NPCManager?.EnableNPC("FancyLadies");
                    UI?.ShowNotification("Fancy Ladies Soul received!");
                    SaveAccessFlags();
                    break;
                case 130:
                    HasCookSoul = true;
                    if (NPCSoulsEnabled) NPCManager?.EnableNPC("Cook");
                    UI?.ShowNotification("Cook Soul received!");
                    SaveAccessFlags();
                    break;
                
                // Prop Souls (400-621) - handled by PropManager
                case 400: case 402: case 403: case 404: case 405: case 406: case 407: case 408: case 409:
                case 410: case 411: case 412: case 413: case 414: case 415: case 416: case 417: case 418: case 419:
                case 420: case 421: case 422: case 423: case 424: case 425:
                case 500: case 501: case 502: case 503: case 504: case 505: case 506: case 507: case 508: case 509:
                case 510: case 511: case 512: case 513: case 514: case 515: case 516: case 517: case 518: case 519:
                case 520: case 521: case 522: case 523: case 524: case 525: case 526: case 527: case 528: case 529:
                case 530: case 531: case 532: case 533: case 534: case 535: case 536: case 537: case 538: case 539:
                case 540: case 541: case 542: case 543:
                case 550: case 551: case 552: case 553: case 554: case 555: case 556: case 557: case 558: case 559:
                case 560: case 561: case 562: case 563: case 564: case 565: case 566: case 567: case 568: case 569:
                case 570: case 571: case 572: case 573: case 574:
                case 580: case 581: case 582: case 583: case 584: case 585: case 586: case 587: case 588: case 589:
                case 590: case 591: case 592: case 593: case 594: case 595: case 596: case 597: case 598: case 599:
                case 600: case 601:
                case 610: case 611: case 612: case 613: case 614: case 615: case 616: case 617: case 618: case 619: case 620:
                case 621: // Golden Bell Soul
                    string soulName = LocationMappings.GetItemName(itemId);
                    PropManager?.ReceiveSoul(soulName);
                    PropManager?.SaveSouls();
                    UI?.ShowNotification($"{soulName} received!");
                    break;

                case 401:  // Tomatoes
                    PropManager?.ReceiveSoul("Tomatoes");
                    PropManager?.SaveSouls();
                    UI?.ShowNotification($"Tomatoes received!");
                    
                    var checkAllSwitchSystems = FindObjectsOfType<SwitchSystem>();
                    foreach (var switchSys in checkAllSwitchSystems)
                    {
                        string switchSysParentName = switchSys?.transform?.parent?.gameObject?.name ?? "no parent";
                        Log.LogInfo($"[INTERACT DEBUG] Noting existence of SwitchSystem '{switchSys.name}' with parent '{switchSysParentName}'");
                    }

                    break;
                
                case 204: // Coin filler item
                    PropManager?.ReceiveCoin();
                    UI?.ShowNotification($"Coin received.");
                    break;
                    
                case 200:
                    if (TrapManager.MegaHonkCount < 3)
                    {
                        TrapManager.MegaHonkCount++;
                        TrapManager.SaveProgressiveItems();
                        string[] levelDesc = { "", "NPCs will notice!", "Heard from further away!", "NPCs will drop items in fear!" };
                        UI?.ShowNotification($"MEGA HONK Level {TrapManager.MegaHonkLevel}! {levelDesc[TrapManager.MegaHonkLevel]}");
                    }
                    else
                    {
                        UI?.ShowNotification("MEGA HONK already at max level!");
                    }
                    break;
                case 201:
                    if (TrapManager.SpeedyFeetCount < 10)
                    {
                        TrapManager.SpeedyFeetCount++;
                        TrapManager.SaveProgressiveItems();
                        int speedBonus = Math.Min(TrapManager.SpeedyFeetCount * 5, 50);
                        UI?.ShowNotification($"SPEEDY FEET! +{speedBonus}% speed ({TrapManager.SpeedyFeetCount}/10)");
                    }
                    else
                    {
                        UI?.ShowNotification("SPEEDY FEET already at max!");
                    }
                    break;
                case 202:
                    TrapManager.IsSilent = true;
                    TrapManager.SaveProgressiveItems();
                    UI?.ShowNotification("SILENT STEPS! NPCs can't hear your footsteps!");
                    break;
                case 203:
                    TrapManager?.ActivateGooseDay(15f);
                    break;
                    
                case 300:
                    TrapManager?.ActivateTired(15f);
                    break;
                case 301:
                    TrapManager?.ActivateConfused(15f);
                    break;
                case 302:
                    TrapManager?.ActivateButterfingers(10f);
                    break;
                case 303:
                    TrapManager?.ActivateSuspicious(10f);
                    break;
                    
                case 999:
                    HasGoldenBell = true;
                    UI?.ShowNotification("You received the Golden Bell!");
                    SaveAccessFlags();
                    break;
            }
        }
        
        public void OnGoalCompleted(string goalName)
        {
            long? locationId = LocationMappings.GetGoalLocationId(goalName);
            if (locationId.HasValue)
            {
                Log.LogInfo($"[LOC-PLUGIN] Sending goal location check: {goalName}");
                SendLocationCheck(locationId.Value);
            }
        }
        
        public void OnAreaBlocked(GoalListArea blockedArea)
        {
            string areaName = blockedArea.ToString();
            UI?.ShowNotification("You need " + areaName + " Access to enter!");
        }
        
        public void OnGooseShooed(string nameOfShooer)
        {
            if (DeathLinkEnabled) // TO DO: add option for deathlink every n shoos rather than every shoo
            {
                Client?.SendDeathLink(nameOfShooer);
            }
        }
        
        public void OnLaunch(Prop prop, PropLaunchEffect launcher)
        {
            Log.LogInfo("[OnLaunch] Launching: " + prop + " with: " + launcher);
            if (launcher.name == "launcher0")
            {
                Instance?.InteractionTracker?.OnInteraction("WellDrop");
                
                if (PropManager.CleanPropName(prop.name) == "exitletter" || PropManager.CleanPropName(prop.name) == "exitparcel" || PropManager.CleanPropName(prop.name) == "minimailpillarprop")
                {
                    Log.LogInfo("[OnLaunch] Prop launched successfully counts as mail");
                    Instance?.InteractionTracker?.OnInteraction("WellDrop2");
                }
            }
        }
        
        public bool CanEnterArea(GoalListArea area)
        {
            switch (area)
            {
                case GoalListArea.Garden: return HasGardenAccess;
                case GoalListArea.Shops: return HasHighStreetAccess;
                case GoalListArea.Backyards: return HasBackGardensAccess;
                case GoalListArea.Pub: return HasPubAccess;
                case GoalListArea.Finale: return HasModelVillageAccess;
                default: return true;
            }
        }
        
        public void ResetAllAccess()
        {
            Log.LogInfo("[AP] ResetAllAccess called - clearing all progress");
            
            HasGardenAccess = false;
            HasHighStreetAccess = false;
            HasBackGardensAccess = false;
            HasPubAccess = false;
            HasModelVillageAccess = false;
            HasGoldenBell = false;

            HasEnteredBackGardens = false;
            HasEnteredPub = false;
            
            HasGroundskeeperSoul = false;
            HasBoySoul = false;
            HasTVShopOwnerSoul = false;
            HasMarketLadySoul = false;
            HasTidyNeighbourSoul = false;
            HasMessyNeighbourSoul = false;
            HasBurlyManSoul = false;
            HasOldManSoul = false;
            HasPubLadySoul = false;
            HasFancyLadiesSoul = false;
            HasCookSoul = false;
            
            PropManager?.ClearAllSouls();
            PropManager?.ClearSavedSouls();
            
            Client?.ClearReceivedItems();
            UI?.ClearReceivedItems();
            checkedLocations.Clear();
            TrapManager?.ClearTraps();
            ClearSavedAccessFlags();
            
            Log.LogInfo("[AP] ResetAllAccess complete");
        }
        
        // === PERSISTENCE ===
        
        private void SaveAccessFlags()
        {
            if (UI != null && !string.IsNullOrEmpty(UI.SlotName))
            {
                PlayerPrefs.SetString("AP_SlotName", UI.SlotName);
                PlayerPrefs.SetString("AP_ServerAddress", UI.ServerAddress);
            }
            
            PlayerPrefs.SetInt("AP_Garden", HasGardenAccess ? 1 : 0);
            PlayerPrefs.SetInt("AP_HighStreet", HasHighStreetAccess ? 1 : 0);
            PlayerPrefs.SetInt("AP_BackGardens", HasBackGardensAccess ? 1 : 0);
            PlayerPrefs.SetInt("AP_Pub", HasPubAccess ? 1 : 0);
            PlayerPrefs.SetInt("AP_ModelVillage", HasModelVillageAccess ? 1 : 0);
            PlayerPrefs.SetInt("AP_GoldenBell", HasGoldenBell ? 1 : 0);

            PlayerPrefs.SetInt("AP_HasEntered_BackGardens", HasEnteredBackGardens ? 1 : 0);
            PlayerPrefs.SetInt("AP_HasEntered_Pub", HasEnteredPub ? 1 : 0);
            
            PlayerPrefs.SetInt("AP_GroundskeeperSoul", HasGroundskeeperSoul ? 1 : 0);
            PlayerPrefs.SetInt("AP_BoySoul", HasBoySoul ? 1 : 0);
            PlayerPrefs.SetInt("AP_TVShopOwnerSoul", HasTVShopOwnerSoul ? 1 : 0);
            PlayerPrefs.SetInt("AP_MarketLadySoul", HasMarketLadySoul ? 1 : 0);
            PlayerPrefs.SetInt("AP_TidyNeighbourSoul", HasTidyNeighbourSoul ? 1 : 0);
            PlayerPrefs.SetInt("AP_MessyNeighbourSoul", HasMessyNeighbourSoul ? 1 : 0);
            PlayerPrefs.SetInt("AP_BurlyManSoul", HasBurlyManSoul ? 1 : 0);
            PlayerPrefs.SetInt("AP_OldManSoul", HasOldManSoul ? 1 : 0);
            PlayerPrefs.SetInt("AP_PubLadySoul", HasPubLadySoul ? 1 : 0);
            PlayerPrefs.SetInt("AP_FancyLadiesSoul", HasFancyLadiesSoul ? 1 : 0);
            PlayerPrefs.SetInt("AP_CookSoul", HasCookSoul ? 1 : 0);
            
            PlayerPrefs.Save();
        }
        
        private void LoadAccessFlags()
        {
            if (UI != null && !string.IsNullOrEmpty(UI.SlotName))
            {
                string savedSlot = PlayerPrefs.GetString("AP_SlotName", "");
                string savedServer = PlayerPrefs.GetString("AP_ServerAddress", "");
                
                if (!string.IsNullOrEmpty(savedSlot) && 
                    (savedSlot != UI.SlotName || savedServer != UI.ServerAddress))
                {
                    Log.LogInfo($"[AP] Different slot detected. Clearing saved flags.");
                    ClearSavedAccessFlags();
                    ResetAllFlags();
                    return;
                }
            }
            
            if (PlayerPrefs.HasKey("AP_Garden"))
            {
                HasGardenAccess = PlayerPrefs.GetInt("AP_Garden") == 1;
                HasHighStreetAccess = PlayerPrefs.GetInt("AP_HighStreet") == 1;
                HasBackGardensAccess = PlayerPrefs.GetInt("AP_BackGardens") == 1;
                HasPubAccess = PlayerPrefs.GetInt("AP_Pub") == 1;
                HasModelVillageAccess = PlayerPrefs.GetInt("AP_ModelVillage") == 1;
                HasGoldenBell = PlayerPrefs.GetInt("AP_GoldenBell") == 1;

                HasEnteredBackGardens = PlayerPrefs.GetInt("AP_HasEntered_BackGardens") == 1;
                HasEnteredPub = PlayerPrefs.GetInt("AP_HasEntered_Pub") == 1;

                
                HasGroundskeeperSoul = PlayerPrefs.GetInt("AP_GroundskeeperSoul", 0) == 1;
                HasBoySoul = PlayerPrefs.GetInt("AP_BoySoul", 0) == 1;
                HasTVShopOwnerSoul = PlayerPrefs.GetInt("AP_TVShopOwnerSoul", 0) == 1;
                HasMarketLadySoul = PlayerPrefs.GetInt("AP_MarketLadySoul", 0) == 1;
                HasTidyNeighbourSoul = PlayerPrefs.GetInt("AP_TidyNeighbourSoul", 0) == 1;
                HasMessyNeighbourSoul = PlayerPrefs.GetInt("AP_MessyNeighbourSoul", 0) == 1;
                HasBurlyManSoul = PlayerPrefs.GetInt("AP_BurlyManSoul", 0) == 1;
                HasOldManSoul = PlayerPrefs.GetInt("AP_OldManSoul", 0) == 1;
                HasPubLadySoul = PlayerPrefs.GetInt("AP_PubLadySoul", 0) == 1;
                HasFancyLadiesSoul = PlayerPrefs.GetInt("AP_FancyLadiesSoul", 0) == 1;
                HasCookSoul = PlayerPrefs.GetInt("AP_CookSoul", 0) == 1;
            }
        }
        
        private void ResetAllFlags()
        {
            HasGardenAccess = false;
            HasHighStreetAccess = false;
            HasBackGardensAccess = false;
            HasPubAccess = false;
            HasModelVillageAccess = false;
            HasGoldenBell = false;

            HasEnteredBackGardens = false;
            HasEnteredPub = false;
            
            HasGroundskeeperSoul = false;
            HasBoySoul = false;
            HasTVShopOwnerSoul = false;
            HasMarketLadySoul = false;
            HasTidyNeighbourSoul = false;
            HasMessyNeighbourSoul = false;
            HasBurlyManSoul = false;
            HasOldManSoul = false;
            HasPubLadySoul = false;
            HasFancyLadiesSoul = false;
            HasCookSoul = false;
            
            if (TrapManager != null)
            {
                TrapManager.MegaHonkCount = 0;
                TrapManager.SpeedyFeetCount = 0;
                TrapManager.IsSilent = false;
            }
        }
        
        public void ReloadAccessFlags()
        {
            LoadAccessFlags();
        }
        
        private void ClearSavedAccessFlags()
        {
            PlayerPrefs.DeleteKey("AP_SlotName");
            PlayerPrefs.DeleteKey("AP_ServerAddress");
            
            PlayerPrefs.DeleteKey("AP_Garden");
            PlayerPrefs.DeleteKey("AP_HighStreet");
            PlayerPrefs.DeleteKey("AP_BackGardens");
            PlayerPrefs.DeleteKey("AP_Pub");
            PlayerPrefs.DeleteKey("AP_ModelVillage");
            PlayerPrefs.DeleteKey("AP_GoldenBell");
            
            PlayerPrefs.DeleteKey("AP_GroundskeeperSoul");
            PlayerPrefs.DeleteKey("AP_BoySoul");
            PlayerPrefs.DeleteKey("AP_TVShopOwnerSoul");
            PlayerPrefs.DeleteKey("AP_MarketLadySoul");
            PlayerPrefs.DeleteKey("AP_TidyNeighbourSoul");
            PlayerPrefs.DeleteKey("AP_MessyNeighbourSoul");
            PlayerPrefs.DeleteKey("AP_BurlyManSoul");
            PlayerPrefs.DeleteKey("AP_OldManSoul");
            PlayerPrefs.DeleteKey("AP_PubLadySoul");
            PlayerPrefs.DeleteKey("AP_FancyLadiesSoul");
            PlayerPrefs.DeleteKey("AP_CookSoul");
            
            PlayerPrefs.DeleteKey("AP_NPCSoulsEnabled");
            PlayerPrefs.DeleteKey("AP_PropSoulsEnabled");
            PlayerPrefs.DeleteKey("AP_NewTasksEnabled");
            
            PlayerPrefs.DeleteKey("AP_LastItemIndex");
            PlayerPrefs.DeleteKey("AP_SpeedyFeet");
            PlayerPrefs.DeleteKey("AP_MegaHonk");
            PlayerPrefs.DeleteKey("AP_GooseDays");
            PlayerPrefs.DeleteKey("AP_SilentSteps");
            PlayerPrefs.Save();
        }
        
        // === COROUTINES ===
        
        private IEnumerator DelayedGateInit()
        {
            yield return new WaitForSeconds(3f);
            
            for (int attempt = 0; attempt < 3; attempt++)
            {
                ClearHubBlockers();
                yield return new WaitForSeconds(1f);
            }
            
            GateManager?.SyncGatesFromAccessFlags();
        }
        
        private void ClearHubBlockers()
        {
            GateManager?.ClearHubBlockers();
            
            var allObjects = FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj == null) continue;
                
                if (obj.name.Contains("ExtraCollider"))
                {
                    var cols = obj.GetComponentsInChildren<Collider>();
                    foreach (var c in cols) c.enabled = false;
                    obj.SetActive(false);
                }
            }
        }
        
        private string GetGameObjectPath(GameObject obj)
        {
            if (obj == null) return "null";
            string path = obj.name;
            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }
        
        private void OnDestroy()
        {
            Client?.Disconnect();
            harmony?.UnpatchSelf();
        }
    }
}