using System;
using BepInEx.Logging;
using UnityEngine;

namespace GooseGameAP
{
    /// <summary>
    /// Handles all trap effects: Butterbeak, Suspicious, Tired, Confused Feet
    /// Also handles buff effects: Speedy Feet, Goose Day
    /// </summary>
    public class TrapManager
    {
        private static ManualLogSource Log => Plugin.Log;
        private Plugin plugin;
        
        // Trap timers
        private float trapTimer = 0f;
        private float butterFingersTimer = 0f;
        private float suspiciousTimer = 0f;
        private float suspiciousHonkTimer = 0f;
        private float confusedTimer = 0f;
        
        // Buff timers
        private float gooseDayTimer = 0f;
        private float gooseDayCalmTimer = 0f;
        
        // Stored buffs
        private int storedGooseDays = 0;
        public const int MAX_GOOSE_DAYS = 3;
        
        // Trap states
        private bool isTired = false;
        private bool isConfused = false;
        private bool hasButterfingers = false;
        private bool isSuspicious = false;
        
        // Confused Feet - random direction rotation
        private float confusedAngle = 180f;  // Start with inverted (180 degrees)
        private float confusedShuffleTimer = 0f;
        private const float CONFUSED_SHUFFLE_INTERVAL = 5.0f;
        
        // Buff states
        private bool hasGooseDay = false;
        
        // Buff tracking
        public int SpeedyFeetCount { get; set; } = 0;
        public int MegaHonkCount { get; set; } = 0;
        public bool IsSilent { get; set; } = false;
        
        /// <summary>
        /// Mega Honk Level (1-3):
        /// Level 1: Draws attention - all NPCs react to honk
        /// Level 2: Increased distance - honk heard from further away
        /// Level 3: Scary honk - NPCs drop held items
        /// </summary>
        public int MegaHonkLevel => System.Math.Min(MegaHonkCount, 3);
        
        public bool HasButterfingers => hasButterfingers;
        public bool IsTired => isTired;
        public bool IsConfused => isConfused;
        public bool IsSuspicious => isSuspicious;
        public bool HasGooseDay => hasGooseDay;
        public int StoredGooseDays => storedGooseDays;
        
        public TrapManager(Plugin plugin)
        {
            this.plugin = plugin;
            LoadProgressiveItems();
        }
        
        /// <summary>
        /// Load progressive item counts from PlayerPrefs
        /// </summary>
        private void LoadProgressiveItems()
        {
            SpeedyFeetCount = Math.Min(PlayerPrefs.GetInt("AP_SpeedyFeet", 0), 10);  // Max 10
            MegaHonkCount = Math.Min(PlayerPrefs.GetInt("AP_MegaHonk", 0), 3);       // Max 3
            storedGooseDays = Math.Min(PlayerPrefs.GetInt("AP_GooseDays", 0), MAX_GOOSE_DAYS);  // Max 3
            IsSilent = PlayerPrefs.GetInt("AP_SilentSteps", 0) == 1;
            Plugin.Log?.LogInfo($"[LOAD] Progressive items: Speedy={SpeedyFeetCount}, Honk={MegaHonkCount}, GooseDays={storedGooseDays}, Silent={IsSilent}");
        }
        
        /// <summary>
        /// Save progressive item counts to PlayerPrefs
        /// </summary>
        public void SaveProgressiveItems()
        {
            // Clamp values before saving
            SpeedyFeetCount = Math.Min(SpeedyFeetCount, 10);
            MegaHonkCount = Math.Min(MegaHonkCount, 3);
            storedGooseDays = Math.Min(storedGooseDays, MAX_GOOSE_DAYS);
            
            PlayerPrefs.SetInt("AP_SpeedyFeet", SpeedyFeetCount);
            PlayerPrefs.SetInt("AP_MegaHonk", MegaHonkCount);
            PlayerPrefs.SetInt("AP_GooseDays", storedGooseDays);
            PlayerPrefs.SetInt("AP_SilentSteps", IsSilent ? 1 : 0);
            PlayerPrefs.Save();
            Plugin.Log?.LogInfo($"[SAVE] Progressive items: Speedy={SpeedyFeetCount}, Honk={MegaHonkCount}, GooseDays={storedGooseDays}, Silent={IsSilent}");
        }
        
        public float GetEffectiveSpeedMultiplier()
        {
            // Tired = 50% speed
            if (isTired)
                return 0.5f;
            
            // Confused Feet doesn't affect speed - it scrambles direction instead
                
            // Speedy Feet: 5% speed per item, capped at 50% bonus (10 items)
            float bonus = SpeedyFeetCount * 0.05f;
            if (bonus > 0.5f) bonus = 0.5f;
            return 1.0f + bonus;
        }
        
        public string GetActiveTrapText()
        {
            if (!isTired && !isConfused && !hasButterfingers && !isSuspicious && !hasGooseDay && storedGooseDays == 0 && SpeedyFeetCount == 0 && MegaHonkCount == 0 && !IsSilent)
                return null;
            
            string trapText = "";
            if (isTired) trapText += "TIRED(" + trapTimer.ToString("F0") + "s) ";
            if (isConfused) trapText += "CONFUSED(" + confusedTimer.ToString("F0") + "s|" + confusedAngle + "°) ";
            if (hasButterfingers) trapText += "BUTTERBEAK(" + butterFingersTimer.ToString("F0") + "s) ";;
            if (isSuspicious) trapText += "SUSPICIOUS(" + suspiciousTimer.ToString("F0") + "s) ";
            if (hasGooseDay) trapText += "GOOSE DAY(" + gooseDayTimer.ToString("F0") + "s) ";
            if (storedGooseDays > 0 && !hasGooseDay) trapText += "GOOSE DAY[G](" + storedGooseDays + ") ";
            if (SpeedyFeetCount > 0 && !isTired) 
            {
                int bonus = System.Math.Min(SpeedyFeetCount * 5, 50);
                trapText += "SPEEDY(+" + bonus + "%) ";
            }
            if (MegaHonkCount > 0)
            {
                string[] levelNames = { "", "LOUD", "LOUDER", "SCARY" };
                trapText += "HONK:" + levelNames[MegaHonkLevel] + " ";
            }
            if (IsSilent) trapText += "SILENT ";
            return trapText.Length > 0 ? trapText : null;
        }
        
        public void ActivateTired(float duration = 15f)
        {
            isTired = true;
            trapTimer = duration;
            plugin.UI.ShowNotification("TIRED GOOSE! Slowed for " + duration + "s!");
        }
        
        public void ActivateConfused(float duration = 15f)
        {
            isConfused = true;
            confusedTimer = duration;
            confusedShuffleTimer = CONFUSED_SHUFFLE_INTERVAL;  // First shuffle after 5s
            ShuffleConfusedAngle();  // Set initial random angle
            Log.LogInfo("[CONFUSED] Controls scrambled!");
            plugin.UI.ShowNotification("CONFUSED FEET! Controls scrambled for " + duration + "s!");
        }
        
        /// <summary>
        /// Randomize the confusion angle - picks angles that feel different from normal
        /// </summary>
        private void ShuffleConfusedAngle()
        {
            // Random angle between 90-270 degrees (avoids 0 which feels normal)
            int[] angles = { 90, 120, 150, 180, 210, 240, 270 };
            confusedAngle = angles[UnityEngine.Random.Range(0, angles.Length)];
            Log.LogInfo($"[CONFUSED] New direction: {confusedAngle} degrees");
            plugin.UI.ShowNotification($"DIRECTIONS SHIFTED!");
        }
        
        /// <summary>
        /// Get current confusion angle for input rotation
        /// </summary>
        public float GetConfusionAngle()
        {
            return isConfused ? confusedAngle : 0f;
        }
        
        public void ActivateButterfingers(float duration = 10f)
        {
            hasButterfingers = true;
            butterFingersTimer = duration;
            plugin.UI.ShowNotification("BUTTERBEAK! Can't hold items for " + duration + "s!");
            ForceDropItems();
        }
        
        public void ActivateSuspicious(float duration = 10f)
        {
            isSuspicious = true;
            suspiciousTimer = duration;
            suspiciousHonkTimer = 0.5f;
            plugin.UI.ShowNotification("SUSPICIOUS! Honking uncontrollably for " + duration + "s!");
        }
        
        public void ActivateGooseDay(float duration = 15f)
        {
            // Store Goose Days instead of using immediately (max 3)
            if (storedGooseDays < MAX_GOOSE_DAYS)
            {
                storedGooseDays++;
                SaveProgressiveItems();
                Log.LogInfo($"[GOOSE DAY] Stored! Now have {storedGooseDays}/{MAX_GOOSE_DAYS}");
                plugin.UI.ShowNotification($"Goose Day stored! ({storedGooseDays}/{MAX_GOOSE_DAYS}) - Press G to use");
            }
            else
            {
                Log.LogInfo("[GOOSE DAY] Already at max storage, using immediately");
                UseGooseDay(duration);
            }
        }
        
        /// <summary>
        /// Use a stored Goose Day buff
        /// </summary>
        public bool UseGooseDay(float duration = 15f)
        {
            // Check if already active
            if (hasGooseDay)
            {
                plugin.UI.ShowNotification("Goose Day already active!");
                return false;
            }
            
            // Check if we have any stored (or allow direct use for debug)
            if (storedGooseDays <= 0)
            {
                plugin.UI.ShowNotification("No Goose Days stored!");
                return false;
            }
            
            storedGooseDays--;
            SaveProgressiveItems();
            Log.LogInfo($"[GOOSE DAY] Using! {storedGooseDays} remaining");
            
            hasGooseDay = true;
            gooseDayTimer = duration;
            gooseDayCalmTimer = 0f;
            plugin.UI.ShowNotification($"A GOOSE DAY! NPCs will ignore you for {duration}s! ({storedGooseDays} left)");
            
            // Immediately calm all NPCs
            CalmAllNPCs();
            return true;
        }
        
        /// <summary>
        /// Force activate Goose Day (for debug, bypasses storage)
        /// </summary>
        public void ForceActivateGooseDay(float duration = 15f)
        {
            Log.LogInfo("[GOOSE DAY] Force activated (debug)!");
            hasGooseDay = true;
            gooseDayTimer = duration;
            gooseDayCalmTimer = 0f;
            plugin.UI.ShowNotification($"A GOOSE DAY! (DEBUG) NPCs will ignore you for {duration}s!");
            CalmAllNPCs();
        }
        
        public void ClearTraps()
        {
            bool hadTraps = isTired || isConfused || hasButterfingers || isSuspicious || hasGooseDay || SpeedyFeetCount > 0 || MegaHonkCount > 0 || IsSilent;
            isTired = false;
            isConfused = false;
            hasButterfingers = false;
            isSuspicious = false;
            hasGooseDay = false;
            butterFingersTimer = 0f;
            suspiciousTimer = 0f;
            gooseDayTimer = 0f;
            trapTimer = 0f;
            confusedTimer = 0f;
            confusedAngle = 0f;
            // Reset permanent upgrades too (for debug)
            SpeedyFeetCount = 0;
            MegaHonkCount = 0;
            IsSilent = false;
            // Note: Don't clear storedGooseDays - those are intentionally saved
            SaveProgressiveItems();
            
            if (hadTraps)
                plugin.UI.ShowNotification("All effects cleared!");
        }
        
        public void Update()
        {
            // Handle trap timers
            if (trapTimer > 0)
            {
                trapTimer -= Time.deltaTime;
                if (trapTimer <= 0)
                {
                    isTired = false;
                    plugin.UI.ShowNotification("Tiredness wore off!");
                }
            }
            
            // Handle confused feet - inverted controls
            if (confusedTimer > 0)
            {
                confusedTimer -= Time.deltaTime;
                confusedShuffleTimer -= Time.deltaTime;
                
                // Shuffle direction every 5 seconds
                if (confusedShuffleTimer <= 0 && confusedTimer > 0)
                {
                    ShuffleConfusedAngle();
                    confusedShuffleTimer = CONFUSED_SHUFFLE_INTERVAL;
                }
                
                if (confusedTimer <= 0)
                {
                    isConfused = false;
                    confusedAngle = 0f;
                    MoverPatches.ResetDiag();
                    GooseGetStickAimPatch.ResetLog();
                    plugin.UI.ShowNotification("Controls restored!");
                }
            }
            
            // Handle butterfingers - drop items and prevent pickup
            if (butterFingersTimer > 0)
            {
                butterFingersTimer -= Time.deltaTime;
                ForceDropItems();
                
                if (butterFingersTimer <= 0)
                {
                    hasButterfingers = false;
                    plugin.UI.ShowNotification("Butterbeak wore off!");
                }
            }
            
            // Handle suspicious goose - force honking
            if (suspiciousTimer > 0)
            {
                suspiciousTimer -= Time.deltaTime;
                suspiciousHonkTimer -= Time.deltaTime;
                
                // Honk every 0.3-0.8 seconds
                if (suspiciousHonkTimer <= 0)
                {
                    ForceHonk();
                    suspiciousHonkTimer = UnityEngine.Random.Range(0.3f, 0.8f);
                }
                
                if (suspiciousTimer <= 0)
                {
                    isSuspicious = false;
                    plugin.UI.ShowNotification("Suspicious behavior stopped!");
                }
            }
            
            // Handle Goose Day - NPCs can't see/hear goose (Harmony patches block detection)
            // Still periodically reset state in case they were already tracking
            if (gooseDayTimer > 0)
            {
                gooseDayTimer -= Time.deltaTime;
                gooseDayCalmTimer -= Time.deltaTime;
                
                // Calm NPCs every 1 second to reset anyone who was already tracking
                if (gooseDayCalmTimer <= 0)
                {
                    CalmAllNPCs();
                    gooseDayCalmTimer = 1.0f;
                }
                
                if (gooseDayTimer <= 0)
                {
                    hasGooseDay = false;
                    plugin.UI.ShowNotification("A Goose Day has ended - NPCs are aware of you again!");
                }
            }
        }
        
        private void ForceDropItems()
        {
            if (GameManager.instance == null || GameManager.instance.allGeese == null) return;
            
            foreach (var goose in GameManager.instance.allGeese)
            {
                if (goose != null && goose.isActiveAndEnabled)
                {
                    // Drop held items (beak)
                    var holder = goose.GetComponent<Holder>();
                    if (holder != null && holder.holding != null)
                    {
                        try
                        {
                            var dropMethod = holder.GetType().GetMethod("Drop", 
                                System.Reflection.BindingFlags.Public | 
                                System.Reflection.BindingFlags.Instance);
                            if (dropMethod != null)
                            {
                                dropMethod.Invoke(holder, null);
                                Log.LogInfo("[BUTTERBEAK] Forced drop (held)!");
                            }
                        }
                        catch { }
                    }
                    
                    // Drop dragged items
                    var dragger = goose.GetComponent<Dragger>();
                    if (dragger != null)
                    {
                        try
                        {
                            var activeField = dragger.GetType().GetField("active",
                                System.Reflection.BindingFlags.Public |
                                System.Reflection.BindingFlags.NonPublic |
                                System.Reflection.BindingFlags.Instance);
                            
                            bool isActive = activeField != null && (bool)activeField.GetValue(dragger);
                            
                            if (isActive)
                            {
                                // Try Drop method
                                var dropMethod = dragger.GetType().GetMethod("Drop",
                                    System.Reflection.BindingFlags.Public |
                                    System.Reflection.BindingFlags.NonPublic |
                                    System.Reflection.BindingFlags.Instance);
                                
                                if (dropMethod != null)
                                {
                                    dropMethod.Invoke(dragger, null);
                                    Log.LogInfo("[BUTTERBEAK] Forced drop (dragged)!");
                                }
                                else
                                {
                                    // Try Release method
                                    var releaseMethod = dragger.GetType().GetMethod("Release",
                                        System.Reflection.BindingFlags.Public |
                                        System.Reflection.BindingFlags.NonPublic |
                                        System.Reflection.BindingFlags.Instance);
                                    
                                    if (releaseMethod != null)
                                    {
                                        releaseMethod.Invoke(dragger, null);
                                        Log.LogInfo("[BUTTERBEAK] Forced release (dragged)!");
                                    }
                                    else if (activeField != null)
                                    {
                                        activeField.SetValue(dragger, false);
                                        Log.LogInfo("[BUTTERBEAK] Set drag active=false!");
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
        }
        
        public void ForceHonk()
        {
            try
            {
                if (GameManager.instance == null || GameManager.instance.allGeese == null) return;
                
                foreach (var goose in GameManager.instance.allGeese)
                {
                    if (goose == null || !goose.isActiveAndEnabled) continue;
                    
                    // Access the GooseHonker
                    var honker = goose.gooseHonker;
                    if (honker == null) continue;
                    
                    // Play honk sound (false = not muffled)
                    honker.PlayHonkSound(false);
                    
                    // Play honk particles (use reflection to avoid ParticleSystemModule dependency)
                    try
                    {
                        var particleField = honker.GetType().GetField("honkParticleSystem",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (particleField != null)
                        {
                            var particles = particleField.GetValue(honker);
                            if (particles != null)
                            {
                                var playMethod = particles.GetType().GetMethod("Play", 
                                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                                    null, Type.EmptyTypes, null);
                                playMethod?.Invoke(particles, null);
                            }
                        }
                    }
                    catch { }
                    
                    // Trigger honk animation
                    if (goose.gooseEngine != null)
                    {
                        goose.gooseEngine.SetTrigger("Honk");
                    }
                    
                    // Set honk state
                    honker.justQuacked = true;
                    honker.timeAtLastHonk = Time.time;
                    
                    // Notify all NPCs about the honk (this alerts them!)
                    if (GameCollections.Instance != null && GameCollections.Instance.allBrains != null)
                    {
                        foreach (var brain in GameCollections.Instance.allBrains)
                        {
                            if (brain != null && goose.honkHearAtDistance != null && 
                                goose.honkHearAtDistance.CanBeHeardBy(brain))
                            {
                                brain.knows.closestKnowsGoose.HearHonk(goose.honkHearAtDistance);
                            }
                        }
                    }
                    
                    // Trigger rumble (use reflection to avoid Rewired dependency)
                    try
                    {
                        var rumbleType = Type.GetType("RumbleManager, Assembly-CSharp");
                        if (rumbleType != null)
                        {
                            var playerField = goose.GetType().GetField("player",
                                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (playerField != null)
                            {
                                var player = playerField.GetValue(goose);
                                if (player != null)
                                {
                                    var triggerMethod = rumbleType.GetMethod("TriggerRumbleHonk",
                                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                                    triggerMethod?.Invoke(null, new object[] { player });
                                }
                            }
                        }
                    }
                    catch { }
                    
                    // Flash light bar (use reflection)
                    try
                    {
                        if (goose.gooseLightBarHelper != null)
                        {
                            var flashMethod = goose.gooseLightBarHelper.GetType().GetMethod("Flash",
                                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (flashMethod != null)
                            {
                                var color = honker.GetMyLightBarColor();
                                flashMethod.Invoke(goose.gooseLightBarHelper, new object[] { color });
                            }
                        }
                    }
                    catch { }
                    
                    Log.LogInfo("[SUSPICIOUS] Forced honk!");
                }
            }
            catch (Exception ex)
            {
                Log.LogError("ForceHonk error: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Calms all NPCs - forces them back to idle state and resets goose awareness
        /// </summary>
        private void CalmAllNPCs()
        {
            try
            {
                int calmedCount = 0;
                
                // Use the actual game's Brain system
                if (GameCollections.Instance != null && GameCollections.Instance.allBrains != null)
                {
                    foreach (var brain in GameCollections.Instance.allBrains)
                    {
                        if (brain == null) continue;
                        
                        // Reset the entire knows system
                        if (brain.knows != null)
                        {
                            TryResetKnowsSystem(brain.knows);
                        }
                        
                        // Force state machine back to idle
                        ForceToIdleState(brain);
                        calmedCount++;
                    }
                }
                
                if (calmedCount > 0)
                {
                    Log.LogDebug($"[GOOSE DAY] Calmed {calmedCount} NPC brains");
                }
            }
            catch (Exception ex)
            {
                Log.LogError("CalmAllNPCs error: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Reset the entire Knows system on a brain
        /// </summary>
        private void TryResetKnowsSystem(object knows)
        {
            if (knows == null) return;
            
            var flags = System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Instance;
            var knowsType = knows.GetType();
            
            try
            {
                // Reset all fields on the knows system
                foreach (var field in knowsType.GetFields(flags))
                {
                    try
                    {
                        var fieldValue = field.GetValue(knows);
                        if (fieldValue == null) continue;
                        
                        string fieldName = field.Name.ToLower();
                        
                        // If it's a goose-related sub-object, reset it thoroughly
                        if (fieldName.Contains("goose") || fieldName.Contains("closest"))
                        {
                            TryResetKnowsGoose(fieldValue);
                        }
                        // Reset primitive types
                        else if (field.FieldType == typeof(float))
                            field.SetValue(knows, 0f);
                        else if (field.FieldType == typeof(int))
                            field.SetValue(knows, 0);
                        else if (field.FieldType == typeof(bool))
                            field.SetValue(knows, false);
                    }
                    catch { }
                }
            }
            catch { }
        }
        
        /// <summary>
        /// Forces an NPC brain back to idle state by clearing all goose-related state desires
        /// </summary>
        private void ForceToIdleState(Brain brain)
        {
            try
            {
                var flags = System.Reflection.BindingFlags.Public | 
                            System.Reflection.BindingFlags.NonPublic | 
                            System.Reflection.BindingFlags.Instance;
                
                // Find the BrainStates field (could be 'states', 'brainStates', etc.)
                var brainType = brain.GetType();
                object states = null;
                
                foreach (var fieldName in new[] { "brainStates", "states", "stateManager", "humanStates" })
                {
                    var field = brainType.GetField(fieldName, flags);
                    if (field != null)
                    {
                        states = field.GetValue(brain);
                        if (states != null) break;
                    }
                }
                
                if (states == null) return;
                
                var statesType = states.GetType();
                
                // List of goose-related state fields to clear
                string[] gooseStateFields = {
                    "chaseState", "shooState", "herdState", "scaredState", "searchState",
                    "honkSuspiciousState", "defendState", "watchState", "cowardHideState", "backOffState",
                    "reactionFoundYou", "reactionGooseHasMyItem", "reactionThreaten", "reactionCantFindGoose",
                    "reactionHeardNoise", "reactionSuspiciousListen", "reactionTakenAback", "reactionStartled",
                    "reactionJustGotScared", "reactionPostScared", "reactionCloseSurprise", "reactionINeedToHerd",
                    "reactionLostWhileHerding", "reactionSnatchFromGoose", "reactionGooseInABox", "reactionTheftShock"
                };
                
                // Clear isDesired on all goose-related states
                foreach (var stateName in gooseStateFields)
                {
                    var stateField = statesType.GetField(stateName, flags);
                    if (stateField != null)
                    {
                        var state = stateField.GetValue(states);
                        if (state != null)
                        {
                            var isDesiredField = state.GetType().GetField("isDesired", flags);
                            if (isDesiredField != null)
                            {
                                isDesiredField.SetValue(state, false);
                            }
                        }
                    }
                }
                
                // Get idle state and make it desired
                var idleStateField = statesType.GetField("idleState", flags);
                if (idleStateField != null)
                {
                    var idleState = idleStateField.GetValue(states);
                    if (idleState != null)
                    {
                        var isDesiredField = idleState.GetType().GetField("isDesired", flags);
                        if (isDesiredField != null)
                        {
                            isDesiredField.SetValue(idleState, true);
                        }
                    }
                }
                
                // Check if current state is goose-related and force exit
                var currentStateField = brainType.GetField("currentState", flags);
                if (currentStateField != null)
                {
                    var currentState = currentStateField.GetValue(brain);
                    if (currentState != null)
                    {
                        string stateName = currentState.GetType().Name.ToLower();
                        if (stateName.Contains("chase") || stateName.Contains("shoo") || 
                            stateName.Contains("scared") || stateName.Contains("search") ||
                            stateName.Contains("honk") || stateName.Contains("defend") ||
                            stateName.Contains("reaction") || stateName.Contains("herd"))
                        {
                            // Try to exit current state
                            var exitMethod = currentState.GetType().GetMethod("Exit", flags);
                            try { exitMethod?.Invoke(currentState, null); } catch { }
                            
                            // Clear follow through
                            var followThroughField = brainType.GetField("followThrough", flags);
                            if (followThroughField != null)
                            {
                                var followThrough = followThroughField.GetValue(brain);
                                if (followThrough != null)
                                {
                                    var clearMethod = followThrough.GetType().GetMethod("Clear", flags);
                                    try { clearMethod?.Invoke(followThrough, null); } catch { }
                                }
                            }
                            
                            // Set to idle state
                            var idleState = idleStateField?.GetValue(states);
                            if (idleState != null)
                            {
                                currentStateField.SetValue(brain, idleState);
                                var enterMethod = idleState.GetType().GetMethod("Enter", flags);
                                try { enterMethod?.Invoke(idleState, null); } catch { }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogDebug($"ForceToIdleState error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Reset the KnowsGoose component - target the exact fields we know exist
        /// </summary>
        private void TryResetKnowsGoose(object knowsGoose)
        {
            if (knowsGoose == null) return;
            
            var flags = System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Instance;
            var knowsType = knowsGoose.GetType();
            
            try
            {
                // Call BecomeLocationUnknown FIRST - this is the game's proper way to make NPC forget
                var method = knowsType.GetMethod("BecomeLocationUnknown", flags);
                if (method != null && method.GetParameters().Length == 0)
                {
                    try { method.Invoke(knowsGoose, null); } catch { }
                }
                
                // Reset detection flags (but NOT assumedPosition - that causes them to walk to 0,0,0!)
                SetField(knowsType, knowsGoose, "isSeen", false, flags);
                SetField(knowsType, knowsGoose, "isSensed", false, flags);
                SetField(knowsType, knowsGoose, "isFirstSeen", false, flags);
                SetField(knowsType, knowsGoose, "isSeenAsDisguise", false, flags);
                SetField(knowsType, knowsGoose, "justHonked", false, flags);
                SetField(knowsType, knowsGoose, "isInShooZone", false, flags);
                SetField(knowsType, knowsGoose, "isInShooZoneHidingPlace", false, flags);
                SetField(knowsType, knowsGoose, "locationUnknown", true, flags);
                SetField(knowsType, knowsGoose, "assumedDistance", float.PositiveInfinity, flags);
                SetField(knowsType, knowsGoose, "heardHonk", null, flags);
                SetField(knowsType, knowsGoose, "heardFeet", null, flags);
                SetField(knowsType, knowsGoose, "shooZone", null, flags);
                SetField(knowsType, knowsGoose, "hasThisItemOfMine", null, flags);
                SetField(knowsType, knowsGoose, "rememberedInThisPose", null, flags);
            }
            catch { }
        }
        
        private void SetField(Type type, object obj, string fieldName, object value, System.Reflection.BindingFlags flags)
        {
            try
            {
                var field = type.GetField(fieldName, flags);
                if (field != null)
                {
                    field.SetValue(obj, value);
                }
            }
            catch { }
        }
    }
}