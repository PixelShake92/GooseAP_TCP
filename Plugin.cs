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
        
        // Area access flags (Hub is always accessible - starting area)
        public bool HasGardenAccess { get; set; } = false;
        public bool HasHighStreetAccess { get; set; } = false;
        public bool HasBackGardensAccess { get; set; } = false;
        public bool HasPubAccess { get; set; } = false;
        public bool HasModelVillageAccess { get; set; } = false;
        public bool HasGoldenBell { get; set; } = false;
        
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
        
        // Buff tracking
        public bool IsSilent => TrapManager?.IsSilent ?? false;
        public int MegaHonkCount => TrapManager?.MegaHonkCount ?? 0;
        
        // DeathLink
        public bool DeathLinkEnabled { get; set; } = false;
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
            Client = new ArchipelagoClient(this);
            
            harmony = new Harmony("com.archipelago.goosegame");
            harmony.PatchAll();
            
            // Don't load access flags on startup - they'll be loaded/validated when connecting
            // This prevents old session data from affecting new multiworlds
        }

        private void Update()
        {
            // Reset state when player returns to menu (no goose exists)
            if (hasInitializedGates && (GameManager.instance == null || 
                GameManager.instance.allGeese == null || GameManager.instance.allGeese.Count == 0))
            {
                hasInitializedGates = false;
                GateManager?.ResetFinale();
                NPCManager?.Reset();
                PropManager?.Reset();
                SwitchSystemPatches.ResetSandcastleTracking();
            }
            
            // Initialize gates once when game scene loads
            if (!hasInitializedGates && GameManager.instance != null && 
                GameManager.instance.allGeese != null && GameManager.instance.allGeese.Count > 0)
            {
                hasInitializedGates = true;
                // Only initialize gates, don't teleport - let player start where they are
                
                // Refresh goose color renderers
                GooseColour?.RefreshRenderers();
                
                // Scan and rename duplicate items EARLY before player can pick them up
                PositionTracker?.ScanAndCacheItems();
                
                // DON'T initialize NPCs here - wait until connection is validated
                // NPCManager?.InitializeNPCs();
                
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
            
            // Handle DeathLink
            if (deathLinkPending)
            {
                deathLinkPending = false;
                GateManager?.TeleportGooseToWell();
                UI?.ShowNotification("DeathLink! Another player died!");
            }
            
            // Update traps
            TrapManager?.Update();
            
            // Update goose color (for rainbow mode)
            GooseColour?.Update();
            
            // Update notification timer
            UI?.UpdateNotification(Time.deltaTime);
            
            // Track pickups/drags
            ItemTracker?.Update();
        }
        
        private void LateUpdate()
        {
            // Confused feet now uses velocity inversion in MoverPatches
            // stickAim inversion didn't work - Mover reads input directly
        }
        
        private void HandleDebugKeys()
        {
            // G key: Use a stored Goose Day
            if (Input.GetKeyDown(KeyCode.G) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
            {
                TrapManager?.UseGooseDay(15f);
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
            
            // F9: Manual gate sync from access flags (recovery for disconnect issues)
            if (Input.GetKeyDown(KeyCode.F9))
            {
                LoadAccessFlags();
                ClearHubBlockers();
                GateManager?.SyncGatesFromAccessFlags();
                UI?.ShowNotification("Gates re-synced!");
            }
        }
        
        private void HandleGateSyncTiming()
        {
            if (Client == null) return;
            
            // Handle pending gate sync - now does multiple attempts
            if (Client.PendingGateSync && GameManager.instance != null && 
                GameManager.instance.allGeese != null && GameManager.instance.allGeese.Count > 0)
            {
                Client.GateSyncTimer += Time.deltaTime;
                if (Client.GateSyncTimer >= 2.0f)
                {
                    Client.GateSyncAttempts++;
                    Client.GateSyncTimer = 0f;
                    
                    // Clear hub blockers on each attempt
                    ClearHubBlockers();
                    
                    // Sync gates
                    GateManager?.SyncGatesFromAccessFlags();
                    
                    // Check if we've done all attempts
                    if (Client.GateSyncAttempts >= ArchipelagoClient.MAX_GATE_SYNC_ATTEMPTS)
                    {
                        Client.PendingGateSync = false;
                    }
                }
            }
            
            // Timeout for received items
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
            if (!showUI) return;
            UI?.DrawUI(this);
        }
        
        // === PUBLIC API FOR OTHER COMPONENTS ===
        
        public void Connect()
        {
            // Check if connecting to a different slot than last time
            string savedSlot = PlayerPrefs.GetString("AP_SlotName", "");
            string savedServer = PlayerPrefs.GetString("AP_ServerAddress", "");
            
            Log.LogInfo($"[AP] Connect called. Saved: {savedSlot}@{savedServer}, Current: {UI.SlotName}@{UI.ServerAddress}");
            
            bool slotMatches = !string.IsNullOrEmpty(savedSlot) && 
                               savedSlot == UI.SlotName && 
                               savedServer == UI.ServerAddress;
            
            if (slotMatches)
            {
                // Same slot - load saved flags
                Log.LogInfo($"[AP] Reconnecting to same slot ({UI.SlotName}@{UI.ServerAddress}). Loading saved state.");
                LoadAccessFlags();
                PropManager?.LoadSouls();
                GateManager?.SyncGatesFromAccessFlags();
                NPCManager?.RefreshNPCStates();
                PropManager?.RefreshPropStates();
            }
            else
            {
                // Different slot OR first time - clear everything and start fresh
                Log.LogInfo($"[AP] New/different slot. Clearing ALL saved state.");
                ResetAllAccess(); // This clears ALL flags including souls
                PropManager?.ClearAllSouls();
                PropManager?.ClearSavedSouls();
                GateManager?.SyncGatesFromAccessFlags();
                NPCManager?.RefreshNPCStates();
                PropManager?.RefreshPropStates();
            }
            
            // Save the new slot info IMMEDIATELY so we can detect changes next time
            PlayerPrefs.SetString("AP_SlotName", UI.SlotName);
            PlayerPrefs.SetString("AP_ServerAddress", UI.ServerAddress);
            PlayerPrefs.Save();
            Log.LogInfo($"[AP] Saved new slot info: {UI.SlotName}@{UI.ServerAddress}");
            
            Client?.Connect(UI.ServerAddress, UI.ServerPort, UI.SlotName, UI.Password, DeathLinkEnabled);
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
                Client?.SendLocationCheck(locationId);
            }
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
                
                // NPC Souls (120-129)
                case 120:
                    HasGroundskeeperSoul = true;
                    NPCManager?.EnableNPC("Groundskeeper");
                    UI?.ShowNotification("Groundskeeper Soul received!");
                    SaveAccessFlags();
                    break;
                case 121:
                    HasBoySoul = true;
                    NPCManager?.EnableNPC("Boy");
                    UI?.ShowNotification("Boy Soul received!");
                    SaveAccessFlags();
                    break;
                case 122:
                    HasTVShopOwnerSoul = true;
                    NPCManager?.EnableNPC("TVShopOwner");
                    UI?.ShowNotification("TV Shop Owner Soul received!");
                    SaveAccessFlags();
                    break;
                case 123:
                    HasMarketLadySoul = true;
                    NPCManager?.EnableNPC("MarketLady");
                    UI?.ShowNotification("Market Lady Soul received!");
                    SaveAccessFlags();
                    break;
                case 124:
                    HasTidyNeighbourSoul = true;
                    NPCManager?.EnableNPC("TidyNeighbour");
                    UI?.ShowNotification("Tidy Neighbour Soul received!");
                    SaveAccessFlags();
                    break;
                case 125:
                    HasMessyNeighbourSoul = true;
                    NPCManager?.EnableNPC("MessyNeighbour");
                    UI?.ShowNotification("Messy Neighbour Soul received!");
                    SaveAccessFlags();
                    break;
                case 126:
                    HasBurlyManSoul = true;
                    NPCManager?.EnableNPC("BurlyMan");
                    UI?.ShowNotification("Burly Man Soul received!");
                    SaveAccessFlags();
                    break;
                case 127:
                    HasOldManSoul = true;
                    NPCManager?.EnableNPC("OldMan");
                    UI?.ShowNotification("Old Man Soul received!");
                    SaveAccessFlags();
                    break;
                case 128:
                    HasPubLadySoul = true;
                    NPCManager?.EnableNPC("PubLady");
                    UI?.ShowNotification("Pub Lady Soul received!");
                    SaveAccessFlags();
                    break;
                case 129:
                    HasFancyLadiesSoul = true;
                    NPCManager?.EnableNPC("FancyLadies");
                    UI?.ShowNotification("Fancy Ladies Soul received!");
                    SaveAccessFlags();
                    break;
                case 130:
                    HasCookSoul = true;
                    NPCManager?.EnableNPC("Cook");
                    UI?.ShowNotification("Cook Soul received!");
                    SaveAccessFlags();
                    break;
                
                // Prop Souls (400-620) - handled by PropManager
                case 400: case 401: case 402: case 403: case 404: case 405: case 406: case 407: case 408: case 409:
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
                SendLocationCheck(locationId.Value);
            }
        }
        
        public void OnAreaBlocked(GoalListArea blockedArea)
        {
            string areaName = blockedArea.ToString();
            UI?.ShowNotification("You need " + areaName + " Access to enter!");
        }
        
        public void OnGooseShooed()
        {
            // Could trigger DeathLink here if enabled
        }
        
        public bool CanEnterArea(GoalListArea area)
        {
            switch (area)
            {
                case GoalListArea.Garden: return HasGardenAccess;  // Now requires access!
                case GoalListArea.Shops: return HasHighStreetAccess;
                case GoalListArea.Backyards: return HasBackGardensAccess;
                case GoalListArea.Pub: return HasPubAccess;
                case GoalListArea.Finale: return HasModelVillageAccess;
                default: return true;  // Hub is always accessible
            }
        }
        
        public void ResetAllAccess()
        {
            Log.LogInfo("[AP] ResetAllAccess called - clearing all progress");
            
            // Area access
            HasGardenAccess = false;
            HasHighStreetAccess = false;
            HasBackGardensAccess = false;
            HasPubAccess = false;
            HasModelVillageAccess = false;
            HasGoldenBell = false;
            
            // NPC Souls
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
            
            // Prop Souls
            PropManager?.ClearAllSouls();
            PropManager?.ClearSavedSouls();
            
            // Clear tracking
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
            // Save slot identifier to detect when switching multiworlds
            if (UI != null && !string.IsNullOrEmpty(UI.SlotName))
            {
                PlayerPrefs.SetString("AP_SlotName", UI.SlotName);
                PlayerPrefs.SetString("AP_ServerAddress", UI.ServerAddress);
            }
            
            // Area access flags
            PlayerPrefs.SetInt("AP_Garden", HasGardenAccess ? 1 : 0);
            PlayerPrefs.SetInt("AP_HighStreet", HasHighStreetAccess ? 1 : 0);
            PlayerPrefs.SetInt("AP_BackGardens", HasBackGardensAccess ? 1 : 0);
            PlayerPrefs.SetInt("AP_Pub", HasPubAccess ? 1 : 0);
            PlayerPrefs.SetInt("AP_ModelVillage", HasModelVillageAccess ? 1 : 0);
            PlayerPrefs.SetInt("AP_GoldenBell", HasGoldenBell ? 1 : 0);
            
            // NPC Soul flags
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
            // Check if saved data is for a different slot - if so, clear it
            if (UI != null && !string.IsNullOrEmpty(UI.SlotName))
            {
                string savedSlot = PlayerPrefs.GetString("AP_SlotName", "");
                string savedServer = PlayerPrefs.GetString("AP_ServerAddress", "");
                
                // If connecting to a different slot/server combo, clear saved data
                if (!string.IsNullOrEmpty(savedSlot) && 
                    (savedSlot != UI.SlotName || savedServer != UI.ServerAddress))
                {
                    Log.LogInfo($"[AP] Different slot detected (saved: {savedSlot}@{savedServer}, current: {UI.SlotName}@{UI.ServerAddress}). Clearing saved flags.");
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
                
                // NPC Souls
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
        
        /// <summary>
        /// Reset all in-memory flags to default values
        /// </summary>
        private void ResetAllFlags()
        {
            // Area access
            HasGardenAccess = false;
            HasHighStreetAccess = false;
            HasBackGardensAccess = false;
            HasPubAccess = false;
            HasModelVillageAccess = false;
            HasGoldenBell = false;
            
            // NPC Souls
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
            
            // Progressive items (via TrapManager)
            if (TrapManager != null)
            {
                TrapManager.MegaHonkCount = 0;
                TrapManager.SpeedyFeetCount = 0;
                TrapManager.IsSilent = false;
            }
        }
        
        /// <summary>
        /// Public method to reload access flags - called on reconnect
        /// </summary>
        public void ReloadAccessFlags()
        {
            LoadAccessFlags();
        }
        
        private void ClearSavedAccessFlags()
        {
            // Slot tracking
            PlayerPrefs.DeleteKey("AP_SlotName");
            PlayerPrefs.DeleteKey("AP_ServerAddress");
            
            // Area access
            PlayerPrefs.DeleteKey("AP_Garden");
            PlayerPrefs.DeleteKey("AP_HighStreet");
            PlayerPrefs.DeleteKey("AP_BackGardens");
            PlayerPrefs.DeleteKey("AP_Pub");
            PlayerPrefs.DeleteKey("AP_ModelVillage");
            PlayerPrefs.DeleteKey("AP_GoldenBell");
            
            // NPC Souls
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
            
            // Progressive items
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
            // Wait longer for game to fully initialize
            yield return new WaitForSeconds(3f);
            
            // Run clearing multiple times to make sure it sticks
            for (int attempt = 0; attempt < 3; attempt++)
            {
                ClearHubBlockers();
                yield return new WaitForSeconds(1f);
            }
            
            // Sync area gates based on current access flags
            GateManager?.SyncGatesFromAccessFlags();
        }
        
        private void ClearHubBlockers()
        {
            // Use GateManager's method which has the actual blocker paths
            GateManager?.ClearHubBlockers();
            
            // Also scan for any remaining GateExtraColliders and disable them
            var allObjects = FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj == null) continue;
                
                // Disable any GateExtraColliders anywhere
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