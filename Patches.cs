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
        [HarmonyPatch(typeof(Goose), "Shoo", typeof(UnityEngine.GameObject))]
        [HarmonyPostfix]
        static void OnGooseShooed(Goose __instance, UnityEngine.GameObject shooer)
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
                    seenEvents.Add(id);
                    Plugin.Log?.LogInfo($"[EVENT DISCOVERY] New event: {id}");
                }
                
                // Reset dragger cache on area transitions (helps after resets)
                if (id.StartsWith("enterArea") || id.StartsWith("resetArea") || id.Contains("Reset"))
                {
                    Plugin.Log.LogInfo("[EVENT] Area event detected: " + id + " - resetting caches");
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
                            Plugin.Log.LogInfo("Blocked event: " + id);
                            Plugin.Instance.OnAreaBlocked(GoalListArea.Garden);
                            Plugin.Instance.GateManager?.TeleportGooseToWell();
                            return false;
                        }
                        break;
                    case "enterAreaHighstreet":
                        if (!Plugin.Instance.HasHighStreetAccess)
                        {
                            Plugin.Log.LogInfo("Blocked event: " + id);
                            Plugin.Instance.OnAreaBlocked(GoalListArea.Shops);
                            Plugin.Instance.GateManager?.TeleportGooseToWell();
                            return false;
                        }
                        break;
                    case "enterAreaBackyards":
                        if (!Plugin.Instance.HasBackGardensAccess)
                        {
                            Plugin.Log.LogInfo("Blocked event: " + id);
                            Plugin.Instance.OnAreaBlocked(GoalListArea.Backyards);
                            Plugin.Instance.GateManager?.TeleportGooseToWell();
                            return false;
                        }
                        break;
                    case "enterAreaPub":
                        if (!Plugin.Instance.HasPubAccess)
                        {
                            Plugin.Log.LogInfo("Blocked event: " + id);
                            Plugin.Instance.OnAreaBlocked(GoalListArea.Pub);
                            Plugin.Instance.GateManager?.TeleportGooseToWell();
                            return false;
                        }
                        break;
                    case "enterAreaFinale":
                        if (!Plugin.Instance.HasModelVillageAccess)
                        {
                            Plugin.Log.LogInfo("Blocked event: " + id);
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
                    Plugin.Log?.LogInfo($"[INTERACT] Bike bell event detected: {id}");
                    Plugin.Instance?.InteractionTracker?.OnBikeBellRung();
                }
                
                // Windmill detection
                if (lower.Contains("windmill") || lower.Contains("pinwheel"))
                {
                    Plugin.Log?.LogInfo($"[INTERACT] Windmill event detected: {id}");
                    Plugin.Instance?.InteractionTracker?.OnWindmillSpun();
                }
                
                // Wind chimes detection
                if (lower.Contains("chime") || lower.Contains("windchime"))
                {
                    Plugin.Log?.LogInfo($"[INTERACT] Wind chimes event detected: {id}");
                    Plugin.Instance?.InteractionTracker?.OnWindChimesPlayed();
                }
                
                // Radio unplug detection
                if (lower.Contains("radio") && (lower.Contains("unplug") || lower.Contains("off") || lower.Contains("stop")))
                {
                    Plugin.Log?.LogInfo($"[INTERACT] Radio unplug event detected: {id}");
                    Plugin.Instance?.InteractionTracker?.OnRadioUnplugged();
                }
                
                // Board break detection (entrance to back gardens)
                if (lower.Contains("board") && (lower.Contains("break") || lower.Contains("smash") || lower.Contains("destroy")))
                {
                    Plugin.Log?.LogInfo($"[INTERACT] Board break event detected: {id}");
                    Plugin.Instance?.InteractionTracker?.OnBoardsBroken();
                }
                
                // Also check for generic fence/gate events that might be the boards
                if (lower.Contains("fence") && lower.Contains("break"))
                {
                    Plugin.Log?.LogInfo($"[INTERACT] Fence break event detected: {id}");
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
        private static HashSet<string> loggedSwitches = new HashSet<string>();
        
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
                
                // Log for discovery
                string key = $"peck_{objName}_{parentName}";
                if (!loggedSwitches.Contains(key))
                {
                    loggedSwitches.Add(key);
                    Plugin.Log?.LogInfo($"[SWITCH PECK] Object: {objName}, Parent: {parentName}");
                }
                
                CheckInteraction(__instance, objName, parentName);
                CheckLacesInteraction(__instance, objName, parentName);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[SWITCH PECK] Error: {ex.Message}");
            }
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
                
                // Log for discovery
                string key = $"setstate_{objName}_{parentName}";
                if (!loggedSwitches.Contains(key))
                {
                    loggedSwitches.Add(key);
                    Plugin.Log?.LogInfo($"[SWITCH SETSTATE] Object: {objName}, Parent: {parentName}");
                }
                
                CheckInteraction(__instance, objName, parentName);
                CheckLacesInteraction(__instance, objName, parentName);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[SWITCH SETSTATE] Error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Check if a switch interaction matches our tracked interactions
        /// </summary>
        private static void CheckInteraction(SwitchSystem instance, string objName, string parentName)
        {
            string objLower = objName.ToLower();
            string parentLower = parentName.ToLower();
            
            // === GARDEN ===
            
            // Bike bell - bikeBellSwitchSystem with parent "bike"
            if (objLower.Contains("bikebell") && parentLower == "bike")
            {
                Plugin.Log?.LogInfo($"[INTERACT] Bike Bell: {objName}");
                Plugin.Instance?.InteractionTracker?.OnInteraction("BikeBell");
            }
            
            // Garden tap/valve
            if (objLower == "valve" && parentLower == "gardendynamic")
            {
                Plugin.Log?.LogInfo($"[INTERACT] Garden Tap: {objName}");
                Plugin.Instance?.InteractionTracker?.OnInteraction("GardenTap");
            }
            
            // Sprinkler
            if (objLower.Contains("sprinkler") && parentLower == "sprinkler")
            {
                Plugin.Log?.LogInfo($"[INTERACT] Sprinkler: {objName}");
                Plugin.Instance?.InteractionTracker?.OnInteraction("Sprinkler");
            }
            
            // === HUB ===
            
            // Intro gate (first gate at game start)
            if (objLower == "shoosystem" && parentLower == "introgate")
            {
                Plugin.Log?.LogInfo($"[INTERACT] Intro Gate: {objName}");
                Plugin.Instance?.InteractionTracker?.OnInteraction("IntroGate");
            }
            
            // === HIGH STREET ===
            
            // Radio - bigRadioWithCable
            if (objLower.Contains("bigradio"))
            {
                Plugin.Log?.LogInfo($"[INTERACT] Radio: {objName}");
                Plugin.Instance?.InteractionTracker?.OnInteraction("UnplugRadio");
            }
            
            // Boards - brokenBit objects
            if (objLower.Contains("brokenbit") || parentLower.Contains("brokenbit"))
            {
                Plugin.Log?.LogInfo($"[INTERACT] Boards: {objName}");
                Plugin.Instance?.InteractionTracker?.OnInteraction("BreakBoards");
            }
            
            // Umbrellas on stands - parent starts with "umbrella" (handles umbrella_1, umbrella_2, umbrella_3)
            if (parentLower.StartsWith("umbrella"))
            {
                if (objLower == "umbrellaonstand1")
                {
                    Plugin.Log?.LogInfo($"[INTERACT] Umbrella Stand 1: {objName}, parent: {parentName}");
                    Plugin.Instance?.InteractionTracker?.OnInteraction("UmbrellaStand1");
                }
                else if (objLower == "umbrellaonstand2")
                {
                    Plugin.Log?.LogInfo($"[INTERACT] Umbrella Stand 2: {objName}, parent: {parentName}");
                    Plugin.Instance?.InteractionTracker?.OnInteraction("UmbrellaStand2");
                }
                else if (objLower == "umbrellaonstand3")
                {
                    Plugin.Log?.LogInfo($"[INTERACT] Umbrella Stand 3: {objName}, parent: {parentName}");
                    Plugin.Instance?.InteractionTracker?.OnInteraction("UmbrellaStand3");
                }
            }
            
            // === BACK GARDENS ===
            
            // Big Garden Bell - StrikerSwitchSystem/BellSwitchSystem with parent "bell"
            if (parentLower == "bell" && (objLower.Contains("striker") || objLower.Contains("bellswitch")))
            {
                Plugin.Log?.LogInfo($"[INTERACT] Garden Bell: {objName}");
                Plugin.Instance?.InteractionTracker?.OnInteraction("GardenBell");
            }
            
            // Wind chimes - parent is "chimeSwitches", distinguished by grandparent (chime-a through chime-g)
            if (parentLower.Contains("chime"))
            {
                // Get grandparent to distinguish between chimes
                string grandparent = instance.transform.parent?.parent?.name?.ToLower() ?? "";
                
                Plugin.Log?.LogInfo($"[INTERACT] Wind Chimes: {objName}, grandparent: {grandparent}");
                
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
                Plugin.Log?.LogInfo($"[INTERACT] Windmill: {objName}");
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
                
                Plugin.Log?.LogInfo($"[INTERACT] Flower: {objName}, grandparent: {grandparentName}");
                
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
                    Plugin.Log?.LogInfo($"[INTERACT] Unknown flower grandparent: {grandparentName}");
                    Plugin.Instance?.InteractionTracker?.OnInteraction("SpinFlower");
                }
            }
            
            // Trellis/fence to messy neighbour
            if (parentLower == "trellisblockersystem" && objLower.Contains("peck"))
            {
                Plugin.Log?.LogInfo($"[INTERACT] Trellis: {objName}");
                Plugin.Instance?.InteractionTracker?.OnInteraction("BreakTrellis");
            }
            
            // === PUB ===
            
            // Van doors
            if (objLower == "switchsystem" && parentLower == "doorleft")
            {
                Plugin.Log?.LogInfo($"[INTERACT] Van Door Left: {objName}");
                Plugin.Instance?.InteractionTracker?.OnInteraction("VanDoorLeft");
            }
            if (objLower == "switchsystem" && parentLower == "doorright")
            {
                Plugin.Log?.LogInfo($"[INTERACT] Van Door Right: {objName}");
                Plugin.Instance?.InteractionTracker?.OnInteraction("VanDoorRight");
            }
            
            // Pub sink tap
            if (objLower == "goosesswitch" && parentLower == "taphandlepositioner")
            {
                Plugin.Log?.LogInfo($"[INTERACT] Pub Tap: {objName}");
                Plugin.Instance?.InteractionTracker?.OnInteraction("PubTap");
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
                        Plugin.Log?.LogInfo($"[INTERACT] Wimp Laces Left: {objName}");
                        Plugin.Instance?.InteractionTracker?.OnInteraction("WimpLacesLeft");
                    }
                    else if (objLower.Contains("right"))
                    {
                        Plugin.Log?.LogInfo($"[INTERACT] Wimp Laces Right: {objName}");
                        Plugin.Instance?.InteractionTracker?.OnInteraction("WimpLacesRight");
                    }
                    return;
                }
                
                // Check for burly man / pub man
                if (allNames.Contains("burly") || allNames.Contains("pub") || allNames.Contains("publican"))
                {
                    if (objLower.Contains("left"))
                    {
                        Plugin.Log?.LogInfo($"[INTERACT] Burly Laces Left: {objName}");
                        Plugin.Instance?.InteractionTracker?.OnInteraction("BurlyLacesLeft");
                    }
                    else if (objLower.Contains("right"))
                    {
                        Plugin.Log?.LogInfo($"[INTERACT] Burly Laces Right: {objName}");
                        Plugin.Instance?.InteractionTracker?.OnInteraction("BurlyLacesRight");
                    }
                    return;
                }
                
                // Unknown NPC - log for discovery
                Plugin.Log?.LogInfo($"[INTERACT] Unknown Laces: {objName}, hierarchy: {allNames}");
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
                    Plugin.Log?.LogInfo($"[CONFUSED] Input rotated by {angle}°");
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
                    Plugin.Log?.LogInfo($"[MEGA HONK] {brain.gameObject.name} dropped item from fear!");
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogInfo($"[MEGA HONK] Error: {ex.Message}");
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
            catch (System.Exception ex)
            {
                Plugin.Log?.LogInfo($"[MEGA HONK] Prefix error: {ex.Message}");
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
            catch (System.Exception ex)
            {
                Plugin.Log?.LogInfo($"[MEGA HONK] Postfix error: {ex.Message}");
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
                    Plugin.Log?.LogInfo("[BUTTERBEAK] Blocked drag PickUp!");
                    return false; // Skip original - can't pick up
                }
            }
            catch { }
            return true;
        }
    }
}