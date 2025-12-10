using System;
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
        [HarmonyPatch(typeof(SwitchEventManager), "TriggerEvent")]
        [HarmonyPrefix]
        static bool OnTriggerEvent(string id)
        {
            try
            {
                if (Plugin.Instance == null) return true;
                
                // Reset dragger cache on area transitions (helps after resets)
                if (id.StartsWith("enterArea") || id.StartsWith("resetArea") || id.Contains("Reset"))
                {
                    Plugin.Log.LogInfo("[EVENT] Area event detected: " + id + " - resetting dragger cache");
                    Plugin.Instance.ItemTracker?.ResetDraggerCache();
                }
                
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
    }

    [HarmonyPatch]
    public static class MoverPatches
    {
        /// <summary>
        /// Scale goose velocity after FixedUpdate applies it
        /// We patch FixedUpdate instead of Update because:
        /// - Update's currentSpeed calculation uses previous currentSpeed in Clamp bounds
        /// - Modifying currentSpeed causes compounding
        /// - FixedUpdate sets rb.velocity fresh from currentSpeed each time
        /// </summary>
        [HarmonyPatch(typeof(Mover), "FixedUpdate")]
        [HarmonyPostfix]
        static void ScaleVelocity(Mover __instance)
        {
            try
            {
                if (__instance == null) return;
                
                float multiplier = Plugin.Instance?.TrapManager?.GetEffectiveSpeedMultiplier() ?? 1.0f;
                if (System.Math.Abs(multiplier - 1.0f) < 0.001f) return;
                
                // Get rigidbody via reflection (it's private)
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var rbField = typeof(Mover).GetField("rb", flags);
                if (rbField == null) return;
                
                var rb = rbField.GetValue(__instance) as Rigidbody;
                if (rb == null) return;
                
                // Scale horizontal velocity only (preserve Y for gravity/jumping)
                Vector3 vel = rb.velocity;
                float y = vel.y;
                vel.x *= multiplier;
                vel.z *= multiplier;
                vel.y = y;
                rb.velocity = vel;
            }
            catch { }
        }
    }
    
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
    
    // NOTE: Butterfingers effect is handled by TrapManager forcing drops
    // The Holder.Grab and Dragger.Grab patches were removed because
    // those method signatures don't exist in this game version
}