using System;
using System.Collections.Generic;
using HarmonyLib;
using BepInEx.Logging;
using UnityEngine;

namespace GooseGameAP
{
    /// <summary>
    /// Harmony patches for intercepting game events
    /// </summary>
    
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
                    Plugin.Instance.OnAreaBlocked(newArea);
                    return false;
                }
                
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
        [HarmonyPatch(typeof(Goose), "Shoo", typeof(UnityEngine.GameObject))]
        [HarmonyPostfix]
        static void OnGooseShooed(Goose __instance, UnityEngine.GameObject shooer)
        {
            try
            {
                Plugin.Log.LogInfo("Goose shooed");
                Plugin.Instance?.OnGooseShooed();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("Shoo patch error: " + ex.Message);
            }
        }
    }

    [HarmonyPatch]
    public static class LaunchPatches
    {
        [HarmonyPatch(typeof(PropLaunchZone), "Launch")]
        [HarmonyPostfix]
        static void OnLaunch(PropLaunchZone __instance, Prop prop, PropLaunchEffect launcher)
        {
            try
            {
                Plugin.Instance?.OnLaunch(prop, launcher);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("Launch patch error: " + ex.Message);
            }
        }
    }

    [HarmonyPatch]
    public static class SwitchEventPatches
    {
        // Track which events we've seen for discovery purposes
        private static HashSet<string> seenEvents = new HashSet<string>();
        
        [HarmonyPatch(typeof(SwitchEventManager), "TriggerEvent")]
        [HarmonyPrefix]
        static bool OnTriggerEvent(string id)
        {
            try
            {
                if (Plugin.Instance == null) return true;
                
                // Log new events for discovery (only once per event type)
                if (!seenEvents.Contains(id))
                {
                    Plugin.Log.LogInfo("[OnTriggerEvent] Adding new event: " + id);
                    seenEvents.Add(id);
                }
                
                // Reset dragger cache on area transitions (helps after resets)
                if (id.StartsWith("enterArea") || id.StartsWith("resetArea") || id.Contains("Reset"))
                {
                    Plugin.Instance.ItemTracker?.ResetDraggerCache();
                    Plugin.Instance.GooseColour?.RefreshRenderers();
                    
                    // Scan for duplicate items and cache their positions
                    Plugin.Instance.PositionTracker?.ScanAndCacheItems();
                }
                
                // Check for interaction events
                CheckInteractionEvents(id);
                
                switch (id)
                {
                    case "enterAreaGarden":
                        if (!Plugin.Instance.HasGardenAccess)
                        {
                            Plugin.Instance.OnAreaBlocked(GoalListArea.Garden);
                            Plugin.Instance.GateManager?.TeleportGooseToWell();
                            return false;
                        }
                        break;
                    case "enterAreaHighstreet":
                        if (!Plugin.Instance.HasHighStreetAccess)
                        {
                            Plugin.Instance.OnAreaBlocked(GoalListArea.Shops);
                            Plugin.Instance.GateManager?.TeleportGooseToWell();
                            return false;
                        }
                        break;
                    case "enterAreaBackyards":
                        if (!Plugin.Instance.HasBackGardensAccess)
                        {
                            Plugin.Instance.OnAreaBlocked(GoalListArea.Backyards);
                            Plugin.Instance.GateManager?.TeleportGooseToWell();
                            return false;
                        }
                        break;
                    case "enterAreaPub":
                        if (!Plugin.Instance.HasPubAccess)
                        {
                            Plugin.Instance.OnAreaBlocked(GoalListArea.Pub);
                            Plugin.Instance.GateManager?.TeleportGooseToWell();
                            return false;
                        }
                        break;
                    case "enterAreaFinale":
                        if (!Plugin.Instance.HasModelVillageAccess)
                        {
                            Plugin.Instance.OnAreaBlocked(GoalListArea.Finale);
                            Plugin.Instance.GateManager?.TeleportGooseToWell();
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
        
        /// <summary>
        /// Check for interaction-related events
        /// </summary>
        private static void CheckInteractionEvents(string id)
        {
            try
            {
                string lower = id.ToLower();
                
                // Bike bell detection
                if (lower.Contains("bell") && (lower.Contains("bike") || lower.Contains("bicycle") || lower.Contains("ring")))
                {
                    Plugin.Instance?.InteractionTracker?.OnBikeBellRung();
                }
                
                // Windmill detection
                if (lower.Contains("windmill") || lower.Contains("pinwheel"))
                {
                    Plugin.Instance?.InteractionTracker?.OnWindmillSpun();
                }
                
                // Wind chimes detection
                if (lower.Contains("chime") || lower.Contains("windchime"))
                {
                    Plugin.Instance?.InteractionTracker?.OnWindChimesPlayed();
                }
                
                // Radio unplug detection
                if (lower.Contains("radio") && (lower.Contains("unplug") || lower.Contains("off") || lower.Contains("stop")))
                {
                    Plugin.Instance?.InteractionTracker?.OnRadioUnplugged();
                }
                
                // Board break detection (entrance to back gardens)
                if (lower.Contains("board") && (lower.Contains("break") || lower.Contains("smash") || lower.Contains("destroy")))
                {
                    Plugin.Instance?.InteractionTracker?.OnBoardsBroken();
                }
                
                // Also check for generic fence/gate events that might be the boards
                if (lower.Contains("fence") && lower.Contains("break"))
                {
                    Plugin.Instance?.InteractionTracker?.OnBoardsBroken();
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[INTERACT] Event check error: {ex.Message}");
            }
        }
    }

    // NOTE: Interaction patches for SwitchSystem work but may have signature issues.
    // Using careful patch definitions.

    /// <summary>
    /// Patches for SwitchSystem - this handles all switch/interaction state changes
    /// </summary>
    [HarmonyPatch]
    public static class SwitchSystemPatches
    {
        // Model church peck tracking
        private static Dictionary<string, int> sandcastleStates = new Dictionary<string, int>();
        private const string DOORWAY_PATH = "DoorwayPeckzoneSequence/DoorwayPeckAtSystem";
        private const string TOWER_PATH = "TowerPeckzoneSequence/TowerPeckAtSystem";
        private const int DOORWAY_MAX = 19;
        private const int TOWER_MAX = 16;
        
        /// <summary>
        /// Reset model church tracking when game restarts
        /// </summary>
        public static void ResetSandcastleTracking()
        {
            sandcastleStates.Clear();
        }
        
        /// <summary>
        /// Log when SwitchSystem.Peck is called (toggle-style switches)
        /// </summary>
        [HarmonyPatch(typeof(SwitchSystem), "Peck")]
        [HarmonyPostfix]
        static void OnPeck(SwitchSystem __instance)
        {
            try
            {
                string objName = __instance?.gameObject?.name ?? "unknown";
                string parentName = __instance?.transform?.parent?.gameObject?.name ?? "no parent";
                
                CheckInteraction(__instance, objName, parentName);
                CheckLacesInteraction(__instance, objName, parentName);
            }
            catch { }
        }
        
        /// <summary>
        /// Log when SwitchSystem.SetState is called
        /// </summary>
        [HarmonyPatch(typeof(SwitchSystem), "SetState")]
        [HarmonyPostfix]
        static void OnSetState(SwitchSystem __instance)
        {
            try
            {
                string objName = __instance?.gameObject?.name ?? "unknown";
                string parentName = __instance?.transform?.parent?.gameObject?.name ?? "no parent";
                
                CheckInteraction(__instance, objName, parentName);
                CheckLacesInteraction(__instance, objName, parentName);
                CheckSandcastlePeck(__instance);
                CheckFinaleStart(__instance, objName);
            }
            catch { }
        }
        
        /// <summary>
        /// Check if this is a model church peck and send location checks
        /// </summary>
        private static void CheckSandcastlePeck(SwitchSystem instance)
        {
            if (Plugin.Instance == null || instance == null) return;
            
            string path = GetSwitchPath(instance);
            if (string.IsNullOrEmpty(path)) return;
            
            bool isDoorway = path.Contains(DOORWAY_PATH);
            bool isTower = path.Contains(TOWER_PATH);
            
            if (!isDoorway && !isTower) return;
            
            int state = instance.currentState;
            string sideKey = isDoorway ? "doorway" : "tower";
            int maxPecks = isDoorway ? DOORWAY_MAX : TOWER_MAX;
            
            int lastState = 0;
            if (sandcastleStates.ContainsKey(sideKey))
            {
                lastState = sandcastleStates[sideKey];
            }
            
            if (state > lastState)
            {
                for (int i = lastState + 1; i <= state && i <= maxPecks; i++)
                {
                    long? locationId = GetSandcastleLocationId(sideKey, i);
                    if (locationId.HasValue)
                    {
                        Plugin.Instance.SendLocationCheck(locationId.Value);
                    }
                }
                sandcastleStates[sideKey] = state;
            }
        }
        
        /// <summary>
        /// Check if finale has started (bell grabbed from model church)
        /// </summary>
        private static void CheckFinaleStart(SwitchSystem instance, string objName)
        {
            // Don't trigger during game initialization
            if (Plugin.Instance == null || !Plugin.Instance.HasInitializedGates) return;
            
            // The bell's finaleSystem activates when grabbed
            if (objName.ToLower() == "finalesystem" && instance.currentState == 1)
            {
                string path = GetSwitchPath(instance);
                if (path.Contains("goldenBell"))
                {
                    Plugin.Instance?.GateManager?.OnFinaleStart();
                }
            }
        }
        
        private static string GetSwitchPath(SwitchSystem sw)
        {
            if (sw == null || sw.gameObject == null) return null;
            
            string path = sw.gameObject.name;
            Transform parent = sw.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }
        
        private static long? GetSandcastleLocationId(string side, int peckNumber)
        {
            long BASE_ID = 119000000;
            
            if (side == "doorway" && peckNumber >= 1 && peckNumber <= DOORWAY_MAX)
            {
                return BASE_ID + 1350 + (peckNumber - 1);
            }
            else if (side == "tower" && peckNumber >= 1 && peckNumber <= TOWER_MAX)
            {
                return BASE_ID + 1369 + (peckNumber - 1);
            }
            return null;
        }
        
        public static bool listAllSwitchSystems = false;
        public static bool listAllJobs = false;
        /// <summary>
        /// Check if a switch interaction matches our tracked interactions
        /// </summary>
        private static void CheckInteraction(SwitchSystem instance, string objName, string parentName)
        {
            string objLower = objName.ToLower();
            string parentLower = parentName.ToLower();

            /*if (!listAllSwitchSystems)
            {
                listAllSwitchSystems = true;
                var checkAllSwitchSystems = UnityEngine.Object.FindObjectsOfType<SwitchSystem>();
                foreach (var switchSys in checkAllSwitchSystems)
                {
                    string switchSysParentName = switchSys?.transform?.parent?.gameObject?.name ?? "no parent";
                    Plugin.Log.LogInfo($"[INTERACT DEBUG] Noting existence of SwitchSystem '{switchSys.name}' with parent '{switchSysParentName}'");
                }
            }
            if (!listAllJobs)
            {
                listAllJobs = true;
                var checkAllJobs = UnityEngine.Object.FindObjectsOfType<Job>();
                foreach (var foundJob in checkAllJobs)
                {
                    if (foundJob == null) continue;

                    Plugin.Log.LogInfo($"[INTERACT DEBUG] Noting existence of Job '{foundJob.name}' with owner: '{foundJob.owner}'");
                }
            }*/
            
            if (parentLower != "goldenbell" && parentLower != "irongate" && objLower != "pubgatesystem" && objLower != "pubtohubgatesystem" && objLower != "sluicegatesystem" &&
                parentLower != "chimeswitches" && objLower != "gardenareatrigger" && parentLower != "teachingzoom" && objLower != "getinthegardendetector" && parentLower != "dartthrowjob" &&
                parentLower != "quoitjobs" && parentLower != "footrcomponents" && parentLower != "footlcomponents" && objLower != "pubtofinalegatesystem" && !objLower.Contains("towerpeck") &&
                !objLower.Contains("doorwaypeck") && parentLower != "van" && parentLower != "getinpubzone" && parentLower != "finalecameradata" && parentLower != "binskip_openable" &&
                objLower != "pubareatrigger" && objLower != "messybackyarddetector" && parentLower != "teachingmove" && parentLower != "teachingbend" && parentLower != "spawnbush" &&
                parentLower != "teachingbeak" && parentLower != "teachingpickupground" && parentLower != "introgate" && objLower != "backyardareatrigger" && 
                parentLower != "trellisblockersystem" && parentLower != "toe_lcomponents" && parentLower != "cleanslipperl" && parentLower != "toe_rcomponents" &&
                parentLower != "cleanslipperr" && parentLower != "gatetall" && parentLower != "halltohubgatesystem" && objLower != "highstreetareatrigger" && parentLower != "tvshopswitches" &&
                objLower != "finaleareatrigger" && parentLower != "soap" && objLower != "goalmodelvillagesystem" && parentLower != "pricinggun" && parentLower != "topiary" &&
                parentLower != "hubgatesystem" && objLower != "speedygoaltimersystem" && parentLower != "rake")
            {
                Plugin.Log.LogInfo($"[INTERACT] Detected interaction: '{objLower}' (parent: '{parentLower}')");
            }

            if (objLower == "slidetomatoboxsystem" && parentLower == "pubsignjobsandhomes")
            {
                var allProps = UnityEngine.Object.FindObjectsOfType<Prop>();
                var tomatoes = new List<Prop>();
                
                foreach (var prop in allProps)
                {
                    if (prop == null || prop.gameObject == null) continue;
                    
                    string name = prop.gameObject.name.ToLower().Trim();
                    
                    if (name.Contains("pubtomato"))
                    {
                        tomatoes.Add(prop);
                        
                        // TO DO: fix tomato bug
                    }
                }
            }
            
            // === GARDEN ===
            
            // Bike bell - bikeBellSwitchSystem with parent "bike"
            if (objLower.Contains("bikebell") && parentLower == "bike")
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("BikeBell");
            }
            
            // Garden tap/valve
            if (objLower == "valve" && parentLower == "gardendynamic")
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("GardenTap");
            }
            
            // Sprinkler
            if (objLower.Contains("sprinkler") && parentLower == "sprinkler")
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("Sprinkler");
            }
            
            // Short out the radio
            if (objLower.Contains("shortoutswitchsystem") && parentLower == "radiosmall")
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("ShortRadio");
            }
            
            // Lock the groundskeeper in the garden
            if (objLower.Contains("lockedinsystem") && parentLower == "extragardengoals")
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("LockedIn");
            }
            
            // === HUB ===
            
            // Intro gate (first gate at game start)
            if (objLower == "shoosystem" && parentLower == "introgate")
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("IntroGate");
            }
            
            // === HIGH STREET ===
            
            // Boop the football
            if (objLower == "goosepecktorollswitch" && parentLower == "football")
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("Football");
            }
            
            // Radio - bigRadioWithCable
            if (objLower.Contains("bigradio"))
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("UnplugRadio");
            }
            
            // Boards - brokenBit objects
            if (objLower.Contains("brokenbit") || parentLower.Contains("brokenbit"))
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("BreakBoards");
            }
            
            // Umbrellas on stands - parent starts with "umbrella" (handles umbrella_1, umbrella_2, umbrella_3)
            if (parentLower.StartsWith("umbrella"))
            {
                if (objLower == "umbrellaonstand1")
                {
                    Plugin.Instance?.InteractionTracker?.OnInteraction("UmbrellaStand1");
                }
                else if (objLower == "umbrellaonstand2")
                {
                    Plugin.Instance?.InteractionTracker?.OnInteraction("UmbrellaStand2");
                }
                else if (objLower == "umbrellaonstand3")
                {
                    Plugin.Instance?.InteractionTracker?.OnInteraction("UmbrellaStand3");
                }
            }

            if (objLower.Contains("garagedoorbangsystem") || parentLower.Contains("garagedoor"))
            {
                var allJobs = UnityEngine.Object.FindObjectsOfType<Job>();
                foreach (var foundJob in allJobs)
                {
                    if (foundJob == null) continue;
                    if (foundJob.name == "GarageTrappedJobNonShopkeeper")
                    {
                        if (foundJob.lastPerformedBy != null)
                        {
                            Plugin.Log.LogInfo($"[INTERACT DEBUG] '{foundJob.owner}' is trapped in the garage! Last brain: '{foundJob.lastPerformedBy.name}'");
                            if (foundJob.lastPerformedBy.name == "tvshop brain")
                            {
                                Plugin.Instance?.InteractionTracker?.OnInteraction("TrappedTVShopOwner");
                            }
                        }
                        else
                        {
                            Plugin.Log.LogInfo($"[INTERACT DEBUG] '{foundJob.owner}' is trapped in the garage! Cannot determine last brain");
                        }
                    }
                }
            }
            
            // === BACK GARDENS ===
            
            // Topiary - pecking
            if (parentLower == "topiary" && (objLower == "peckeffects" || objLower == "peckyswitch"))
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("Topiary");
            }
            // Topiary - fixing
            if (parentLower == "topiary" && (/*objLower == "normalsnipswitch" || objLower == "cosmeticsnipswitch" ||*/ objLower == "finalsnipswitch"))
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("Topiary 2");
            }
            
            // Pose as a duck
            if (parentLower == "duckandribbonstuff" && objLower == "gooseisinposeswitchsystem")// objLower == "enablegooseposeswitchsystem")
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("Pose as Duck");
            }
            
            // Big Garden Bell - StrikerSwitchSystem/BellSwitchSystem with parent "bell"
            if (parentLower == "bell" && (objLower.Contains("striker") || objLower.Contains("bellswitch")))
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("GardenBell");
            }
            
            // Wind chimes - parent is "chimeSwitches", distinguished by grandparent (chime-a through chime-g)
            if (parentLower.Contains("chime"))
            {
                // Get grandparent to distinguish between chimes
                string grandparent = instance.transform.parent?.parent?.name?.ToLower() ?? "";
                
                
                // Each chime is named by musical note (a-g)
                if (grandparent == "chime-a")
                    Plugin.Instance?.InteractionTracker?.OnInteraction("WindChimeA");
                else if (grandparent == "chime-b")
                    Plugin.Instance?.InteractionTracker?.OnInteraction("WindChimeB");
                else if (grandparent == "chime-c")
                    Plugin.Instance?.InteractionTracker?.OnInteraction("WindChimeC");
                else if (grandparent == "chime-d")
                    Plugin.Instance?.InteractionTracker?.OnInteraction("WindChimeD");
                else if (grandparent == "chime-e")
                    Plugin.Instance?.InteractionTracker?.OnInteraction("WindChimeE");
                else if (grandparent == "chime-f")
                    Plugin.Instance?.InteractionTracker?.OnInteraction("WindChimeF");
                else if (grandparent == "chime-g")
                    Plugin.Instance?.InteractionTracker?.OnInteraction("WindChimeG");
                else
                    Plugin.Instance?.InteractionTracker?.OnInteraction("WindChimes"); // Fallback
            }
            
            // Windmill - switchsystem with parent "Plane"
            if (parentLower == "plane" && objLower.Contains("switchsystem"))
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("Windmill");
            }
            
            // Spinny Flowers - switch with parent "Circle", distinguished by grandparent
            if (parentLower == "circle" && objLower == "switch")
            {
                // Get grandparent to distinguish between flowers
                string grandparentName = "";
                if (instance.transform.parent?.parent != null)
                {
                    grandparentName = instance.transform.parent.parent.name.ToLower();
                }
                
                
                // Distinguish by grandparent name
                // fakeflowerpointy = sunflower (pointy petals)
                // fakeflowerround = purple flower (round petals)
                if (grandparentName.Contains("pointy"))
                {
                    Plugin.Instance?.InteractionTracker?.OnInteraction("SpinSunflower");
                }
                else if (grandparentName.Contains("round"))
                {
                    Plugin.Instance?.InteractionTracker?.OnInteraction("SpinPurpleFlower");
                }
                else
                {
                    // Fallback - log grandparent for debugging
                    Plugin.Instance?.InteractionTracker?.OnInteraction("SpinFlower");
                }
            }
            
            // Trellis/fence to messy neighbour
            if (parentLower == "trellisblockersystem" && objLower.Contains("peck"))
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("BreakTrellis");
            }
            
            // Getting ribbons stuck on the bushes
            if (objLower == "jumptohomesystem" && parentLower == "ribbonastuff")
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("RibbonStuckA");
            }
            if (objLower == "ribbonbjumptohomesystem" && parentLower == "ribbonbstuff")
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("RibbonStuckB");
            }
            
            // Getting ribbons stuck on the bushes
            if (objLower == "frontdoorsystem" && parentLower == "housebricksingleblend")
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("InteriorRedecorating");
            }
            
            // === PUB ===
            
            // Van doors
            if (objLower == "switchsystem" && parentLower == "doorleft")
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("VanDoorLeft");
            }
            if (objLower == "switchsystem" && parentLower == "doorright")
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("VanDoorRight");
            }
            
            // Pub sink tap
            if (objLower == "goosesswitch" && parentLower == "taphandlepositioner")
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("PubTap");
            }
            
            // Trip the burly man
            if (objLower == "tripsystem" && parentLower == "pub man")
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("TripBurly");
            }
            
            // Break a pint glass
            if (objLower == "breaksystem" && parentLower == "pintglassprop")
            {
                Plugin.Instance?.InteractionTracker?.OnInteraction("BreakPintGlass");
            }

            // Perform at the pub with a harmonica
            if (objLower == "finalcheersystem" || parentLower == "goosestage")
            {
                var geese = UnityEngine.Object.FindObjectsOfType<Goose>();
                foreach (var goose in geese)
                {
                    if (goose == null) continue;
                    if (goose.holder == null) continue;
                    if (goose.holder.holding == null) continue;
                    Plugin.Log.LogInfo($"[INTERACT DEBUG] Goose is holding '{goose.holder.holding.name}'");
                    if (goose.holder.holding.name.Contains("harmonica"))
                    {
                        Plugin.Instance?.InteractionTracker?.OnInteraction("PerformHarmonica");
                    }
                }
            }
        }
        
        /// <summary>
        /// Check laces interactions - need grandparent to differentiate wimp vs burly man
        /// Called separately because we need the full instance for hierarchy traversal
        /// </summary>
        private static void CheckLacesInteraction(SwitchSystem instance, string objName, string parentName)
        {
            string objLower = objName.ToLower();
            string parentLower = parentName.ToLower();
            
            // Only check for laces switches
            if (!objLower.StartsWith("lacesswitch")) return;
            if (!parentLower.Contains("foot")) return;
            
            // Traverse up the hierarchy to find NPC name
            // Structure: LacesSwitchLeft -> footLComponents -> foot_L -> ... -> hips -> Bone -> skellington -> NPC
            // NPC name is at level 8-9 in the hierarchy
            try
            {
                var current = instance?.transform;
                string allNames = "";
                for (int i = 0; i < 12 && current != null; i++)
                {
                    string name = current.gameObject?.name ?? "";
                    allNames += name.ToLower() + "|";
                    current = current.parent;
                }
                
                // Check for wimp (High Street boy)
                if (allNames.Contains("wimp") || allNames.Contains("boy"))
                {
                    if (objLower.Contains("left"))
                    {
                        Plugin.Instance?.InteractionTracker?.OnInteraction("WimpLacesLeft");
                    }
                    else if (objLower.Contains("right"))
                    {
                        Plugin.Instance?.InteractionTracker?.OnInteraction("WimpLacesRight");
                    }
                    return;
                }
                
                // Check for burly man / pub man
                if (allNames.Contains("burly") || allNames.Contains("pub") || allNames.Contains("publican"))
                {
                    if (objLower.Contains("left"))
                    {
                        Plugin.Instance?.InteractionTracker?.OnInteraction("BurlyLacesLeft");
                    }
                    else if (objLower.Contains("right"))
                    {
                        Plugin.Instance?.InteractionTracker?.OnInteraction("BurlyLacesRight");
                    }
                    return;
                }
                
                // Unknown NPC - log for discovery
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[LACES] Error traversing hierarchy: {ex.Message}");
            }
        }
    }

    [HarmonyPatch]
    public static class MoverPatches
    {
        // Cache reflection fields
        private static System.Reflection.FieldInfo rbField = null;
        private static bool fieldsInitialized = false;
        
        private static void InitFields()
        {
            if (fieldsInitialized) return;
            
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
            rbField = typeof(Mover).GetField("rb", flags);
            fieldsInitialized = true;
        }
        
        /// <summary>
        /// Apply speed multiplier only
        /// </summary>
        [HarmonyPatch(typeof(Mover), "FixedUpdate")]
        [HarmonyPostfix]
        static void ApplySpeedMultiplier(Mover __instance)
        {
            try
            {
                if (__instance == null) return;
                
                InitFields();
                if (rbField == null) return;
                
                var trapManager = Plugin.Instance?.TrapManager;
                if (trapManager == null) return;
                
                float multiplier = trapManager.GetEffectiveSpeedMultiplier();
                
                if (System.Math.Abs(multiplier - 1.0f) < 0.001f) 
                    return;
                
                var rb = rbField.GetValue(__instance) as Rigidbody;
                if (rb == null) return;
                
                Vector3 vel = rb.velocity;
                float y = vel.y;
                vel.x *= multiplier;
                vel.z *= multiplier;
                vel.y = y;
                
                rb.velocity = vel;
            }
            catch { }
        }
        
        public static void ResetDiag() { }
    }
    
    /// <summary>
    /// Confused Feet - Patch GetStickAim to return rotated input
    /// This is what Mover calls to get movement direction!
    /// </summary>
    [HarmonyPatch(typeof(Goose), "GetStickAim")]
    public static class GooseGetStickAimPatch
    {
        private static float lastLogTime = 0f;
        
        [HarmonyPostfix]
        static void RotateStickAim(ref Vector3 __result)
        {
            try
            {
                var trapManager = Plugin.Instance?.TrapManager;
                if (trapManager == null || !trapManager.IsConfused) return;
                
                float angle = trapManager.GetConfusionAngle();
                if (System.Math.Abs(angle) < 0.01f) return;
                
                // Rotate the input by the confusion angle
                float rad = angle * Mathf.Deg2Rad;
                float cos = Mathf.Cos(rad);
                float sin = Mathf.Sin(rad);
                
                float newX = __result.x * cos - __result.z * sin;
                float newZ = __result.x * sin + __result.z * cos;
                
                __result.x = newX;
                __result.z = newZ;
                
                if (Time.time - lastLogTime > 2.0f && __result.sqrMagnitude > 0.01f)
                {
                    lastLogTime = Time.time;
                }
            }
            catch { }
        }
        
        public static void ResetLog() { lastLogTime = 0f; }
    }
    
    /// <summary>
    /// Note: Confused Feet uses velocity inversion in MoverPatches.
    /// stickAim inversion was tried but didn't affect movement direction.
    /// Velocity inversion works but causes sliding (no walk animation).
    /// </summary>
    
    /// <summary>
    /// Goose Day effect - blocks NPC perception of the goose
    /// </summary>
    [HarmonyPatch]
    public static class GooseDayPatches
    {
        /// <summary>
        /// Block visual detection of goose during Goose Day
        /// </summary>
        [HarmonyPatch(typeof(Senses), nameof(Senses.CanSeeGoose))]
        [HarmonyPrefix]
        static bool BlockCanSeeGoose(ref bool __result)
        {
            try
            {
                if (Plugin.Instance?.TrapManager?.HasGooseDay == true)
                {
                    __result = false;
                    return false; // Skip original method
                }
            }
            catch { }
            return true; // Run original method
        }
        
        /// <summary>
        /// Block hearing honks during Goose Day
        /// </summary>
        [HarmonyPatch(typeof(KnowsGoose), nameof(KnowsGoose.HearHonk))]
        [HarmonyPrefix]
        static bool BlockHearHonk()
        {
            try
            {
                if (Plugin.Instance?.TrapManager?.HasGooseDay == true)
                {
                    return false; // Skip original method - don't hear the honk
                }
            }
            catch { }
            return true;
        }
        
        /// <summary>
        /// Block hearing footsteps during Goose Day
        /// </summary>
        [HarmonyPatch(typeof(KnowsGoose), nameof(KnowsGoose.HearFeet))]
        [HarmonyPrefix]
        static bool BlockHearFeet()
        {
            try
            {
                // Block during Goose Day OR if Silent Steps is active
                if (Plugin.Instance?.TrapManager?.HasGooseDay == true ||
                    Plugin.Instance?.TrapManager?.IsSilent == true)
                {
                    return false; // Skip original method
                }
            }
            catch { }
            return true;
        }
        
        /// <summary>
        /// Block NPCs from knowing goose has their items during Goose Day
        /// This prevents them from chasing after items the goose is holding
        /// </summary>
        [HarmonyPatch(typeof(Goose), nameof(Goose.GooseHasItem))]
        [HarmonyPrefix]
        static bool BlockGooseHasItem(ref bool __result)
        {
            try
            {
                if (Plugin.Instance?.TrapManager?.HasGooseDay == true)
                {
                    __result = false; // Pretend goose doesn't have the item
                    return false; // Skip original method
                }
            }
            catch { }
            return true;
        }
    }
    
    /// <summary>
    /// Mega Honk effects - upgraded honking abilities
    /// Level 1: All NPCs react to honk (draws attention) - default behavior enhanced
    /// Level 2: Increased honk detection distance - always heard regardless of distance
    /// Level 3: Scary honk - NPCs drop held items
    /// </summary>
    [HarmonyPatch]
    public static class MegaHonkPatches
    {
        /// <summary>
        /// For Level 2+: Force NPCs to react to honk by setting justHonked flag
        /// </summary>
        [HarmonyPatch(typeof(KnowsGoose), nameof(KnowsGoose.HearHonk))]
        [HarmonyPrefix]
        static void ForceHearHonk(KnowsGoose __instance)
        {
            try
            {
                // Don't enhance if Goose Day is active (NPCs shouldn't hear at all)
                if (Plugin.Instance?.TrapManager?.HasGooseDay == true) return;
                
                int level = Plugin.Instance?.TrapManager?.MegaHonkLevel ?? 0;
                if (level >= 2 && __instance != null)
                {
                    // Level 2+: Ensure the NPC processes this honk
                    __instance.justHonked = true;
                }
            }
            catch { }
        }
        
        /// <summary>
        /// Enhance honk effects based on Mega Honk level
        /// Called after HearHonk to add extra effects
        /// </summary>
        [HarmonyPatch(typeof(KnowsGoose), nameof(KnowsGoose.HearHonk))]
        [HarmonyPostfix]
        static void EnhanceHonkEffect(KnowsGoose __instance)
        {
            try
            {
                // Don't enhance if Goose Day is active (NPCs shouldn't hear at all)
                if (Plugin.Instance?.TrapManager?.HasGooseDay == true) return;
                
                int level = Plugin.Instance?.TrapManager?.MegaHonkLevel ?? 0;
                if (level < 3) return; // Level 3 required for scare effect
                
                // Level 3: Make NPC drop held item
                // Need to get brain via reflection since it might not be directly accessible
                var flags = System.Reflection.BindingFlags.Public | 
                            System.Reflection.BindingFlags.NonPublic | 
                            System.Reflection.BindingFlags.Instance;
                
                var brainField = typeof(KnowsGoose).GetField("brain", flags);
                if (brainField == null) return;
                
                var brain = brainField.GetValue(__instance) as Brain;
                if (brain?.holder != null && brain.holder.holding != null)
                {
                    brain.holder.Drop();
                }
            }
            catch (System.Exception)
            {
            }
        }
        
        // Store original particle values to prevent compounding
        private static float? originalStartSize = null;
        private static float? originalLifetime = null;
        private static ParticleSystem.MinMaxGradient? originalStartColor = null;
        private static ParticleSystem cachedParticleSystem = null;
        
        /// <summary>
        /// Set particle color BEFORE honk plays (Prefix)
        /// </summary>
        [HarmonyPatch(typeof(GooseHonker), "PlayHonkSound")]
        [HarmonyPrefix]
        static void SetHonkColorBefore(GooseHonker __instance)
        {
            try
            {
                int level = Plugin.Instance?.TrapManager?.MegaHonkLevel ?? 0;
                
                var flags = System.Reflection.BindingFlags.Public | 
                            System.Reflection.BindingFlags.NonPublic | 
                            System.Reflection.BindingFlags.Instance;
                
                var particleField = typeof(GooseHonker).GetField("honkParticleSystem", flags);
                if (particleField == null) return;
                
                var particleSystem = particleField.GetValue(__instance) as ParticleSystem;
                if (particleSystem == null) return;
                
                var main = particleSystem.main;
                
                // Store original values if first time
                if (cachedParticleSystem != particleSystem || originalStartColor == null)
                {
                    cachedParticleSystem = particleSystem;
                    originalStartSize = main.startSizeMultiplier;
                    originalLifetime = main.startLifetimeMultiplier;
                    originalStartColor = main.startColor;
                }
                
                // Apply size multipliers
                float sizeMultiplier = 1.0f + (level * 0.5f);
                float lifetimeMultiplier = 1.0f + (level * 0.3f);
                
                if (originalStartSize.HasValue)
                    main.startSizeMultiplier = originalStartSize.Value * sizeMultiplier;
                
                if (originalLifetime.HasValue)
                    main.startLifetimeMultiplier = originalLifetime.Value * lifetimeMultiplier;
                
                // Set startColor (might not work but try anyway)
                if (level >= 1)
                {
                    Color targetColor = level >= 3 ? new Color(1f, 0.15f, 0.15f, 1f) :
                                        level >= 2 ? new Color(1f, 0.6f, 0.2f, 1f) :
                                                     new Color(1f, 0.9f, 0.3f, 1f);
                    main.startColor = new ParticleSystem.MinMaxGradient(targetColor);
                }
                else if (originalStartColor.HasValue)
                {
                    main.startColor = originalStartColor.Value;
                }
            }
            catch (System.Exception)
            {
            }
        }
        
        /// <summary>
        /// Modify particle colors AFTER emission and emit extra (Postfix)
        /// </summary>
        [HarmonyPatch(typeof(GooseHonker), "PlayHonkSound")]
        [HarmonyPostfix]
        static void EnhanceHonkVisuals(GooseHonker __instance)
        {
            try
            {
                int level = Plugin.Instance?.TrapManager?.MegaHonkLevel ?? 0;
                if (level <= 0) return;
                
                var flags = System.Reflection.BindingFlags.Public | 
                            System.Reflection.BindingFlags.NonPublic | 
                            System.Reflection.BindingFlags.Instance;
                
                var particleField = typeof(GooseHonker).GetField("honkParticleSystem", flags);
                if (particleField == null) return;
                
                var particleSystem = particleField.GetValue(__instance) as ParticleSystem;
                if (particleSystem == null) return;
                
                // Determine color
                Color32 targetColor = level >= 3 ? new Color32(255, 40, 40, 255) :   // Red
                                      level >= 2 ? new Color32(255, 150, 50, 255) :  // Orange
                                                   new Color32(255, 230, 75, 255);   // Yellow
                
                // Get all current particles and modify their colors
                int particleCount = particleSystem.particleCount;
                if (particleCount > 0)
                {
                    ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleCount];
                    particleSystem.GetParticles(particles);
                    
                    for (int i = 0; i < particleCount; i++)
                    {
                        particles[i].startColor = targetColor;
                    }
                    
                    particleSystem.SetParticles(particles, particleCount);
                }
                
                // Emit extra particles
                particleSystem.Emit(level * 5);
                
                // Color the extra particles too
                int newCount = particleSystem.particleCount;
                if (newCount > particleCount)
                {
                    ParticleSystem.Particle[] allParticles = new ParticleSystem.Particle[newCount];
                    particleSystem.GetParticles(allParticles);
                    
                    for (int i = particleCount; i < newCount; i++)
                    {
                        allParticles[i].startColor = targetColor;
                    }
                    
                    particleSystem.SetParticles(allParticles, newCount);
                }
            }
            catch (System.Exception)
            {
            }
        }
    }
    
    // NOTE: Butterbeak effect - beak pickups already blocked elsewhere
    // This patch blocks dragging items during Butterbeak
    
    /// <summary>
    /// Block starting to drag items during Butterbeak
    /// </summary>
    [HarmonyPatch(typeof(Dragger), "PickUp")]
    public static class DraggerButterbeakPatch
    {
        [HarmonyPrefix]
        static bool BlockPickUp()
        {
            try
            {
                if (Plugin.Instance?.TrapManager?.HasButterfingers == true)
                {
                    return false; // Skip original - can't pick up
                }
            }
            catch { }
            return true;
        }
    }
}