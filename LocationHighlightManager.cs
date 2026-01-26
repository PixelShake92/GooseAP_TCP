using System;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

namespace GooseGameAP
{
    /// <summary>
    /// Manages visual highlighting for unchecked pickup/drag/interaction locations.
    /// Items that haven't been sent to AP will have a pulsing golden shimmer.
    /// Once checked, the effect is removed.
    /// </summary>
    public class LocationHighlightManager
    {
        private static ManualLogSource Log => Plugin.Log;
        private Plugin plugin;
        
        // Configuration
        public bool HighlightingEnabled { get; set; } = true;
        public float PulseSpeed { get; set; } = 0.5f;  // Cycles per second (slower pulse)
        public float MinBrightness { get; set; } = 0.3f;  // Minimum tint intensity
        public float MaxBrightness { get; set; } = 1.0f;  // Maximum tint intensity
        
        // Highlight color options
        public static readonly Color GoldenGlow = new Color(1f, 0.85f, 0.4f, 1f);
        public static readonly Color MagicPurple = new Color(0.8f, 0.5f, 1f, 1f);
        public static readonly Color CoolBlue = new Color(0.4f, 0.8f, 1f, 1f);
        public static readonly Color FreshGreen = new Color(0.5f, 1f, 0.6f, 1f);
        public static readonly Color HotPink = new Color(1f, 0.4f, 0.7f, 1f);
        public static readonly Color FireOrange = new Color(1f, 0.5f, 0.2f, 1f);
        public static readonly Color IceCyan = new Color(0.3f, 1f, 1f, 1f);
        public static readonly Color LimeGreen = new Color(0.7f, 1f, 0.3f, 1f);
        public static readonly Color SunYellow = new Color(1f, 1f, 0.3f, 1f);
        public static readonly Color PureWhite = new Color(1f, 1f, 1f, 1f);
        
        private Color currentHighlightColor = GoldenGlow;
        private int currentColorIndex = 0;
        
        // Rainbow mode
        private bool rainbowMode = false;
        private float rainbowHue = 0f;
        public float RainbowSpeed { get; set; } = 0.03f;  // Full cycle every ~30 seconds
        
        // Simple struct to replace tuples (for .NET Framework compatibility)
        public struct ColorPreset
        {
            public string Name;
            public Color Color;
            public bool IsRainbow;
            
            public ColorPreset(string name, Color color, bool isRainbow = false)
            {
                Name = name;
                Color = color;
                IsRainbow = isRainbow;
            }
        }
        
        public static readonly List<ColorPreset> ColorPresets = new List<ColorPreset>
        {
            new ColorPreset("Rainbow", Color.white, true),  // Rainbow first - most visible!
            new ColorPreset("Golden Glow", GoldenGlow),
            new ColorPreset("Magic Purple", MagicPurple),
            new ColorPreset("Cool Blue", CoolBlue),
            new ColorPreset("Fresh Green", FreshGreen),
            new ColorPreset("Hot Pink", HotPink),
            new ColorPreset("Fire Orange", FireOrange),
            new ColorPreset("Ice Cyan", IceCyan),
            new ColorPreset("Lime Green", LimeGreen),
            new ColorPreset("Sun Yellow", SunYellow),
            new ColorPreset("Pure White", PureWhite)
        };
        
        // Custom color support
        private Color customColor = GoldenGlow;
        private bool useCustomColor = false;
        
        // Persistence keys
        private const string COLOR_INDEX_KEY = "AP_HighlightColorIndex";
        private const string CUSTOM_R_KEY = "AP_HighlightCustomR";
        private const string CUSTOM_G_KEY = "AP_HighlightCustomG";
        private const string CUSTOM_B_KEY = "AP_HighlightCustomB";
        private const string USE_CUSTOM_KEY = "AP_HighlightUseCustom";
        
        // Tracked prop data
        private class HighlightedProp
        {
            public GameObject gameObject;
            public Renderer[] renderers;
            public string itemKey;
            public bool isPickup;  // true = pickup, false = drag
            public bool needsRefresh;
        }
        
        // CRITICAL: Static dictionary to store original colors per mesh - NEVER cleared
        // This prevents environmental changes from corrupting our baseline
        private static Dictionary<Mesh, Color[]> originalVertexColorsByMesh = new Dictionary<Mesh, Color[]>();
        
        private Dictionary<int, HighlightedProp> highlightedProps = new Dictionary<int, HighlightedProp>();
        private float pulseTime = 0f;
        private bool hasScanned = false;
        private float rescanTimer = 0f;
        private const float RESCAN_INTERVAL = 5f;  // Rescan for new props every 5 seconds
        
        public string CurrentColorName => useCustomColor ? "Custom" : ColorPresets[currentColorIndex].Name;
        
        public Color CurrentColor => useCustomColor ? customColor : currentHighlightColor;
        
        public LocationHighlightManager(Plugin plugin)
        {
            this.plugin = plugin;
            LoadColorPreference();
        }
        
        /// <summary>
        /// Load saved color preference
        /// </summary>
        private void LoadColorPreference()
        {
            currentColorIndex = PlayerPrefs.GetInt(COLOR_INDEX_KEY, 0);
            if (currentColorIndex < 0 || currentColorIndex >= ColorPresets.Count)
                currentColorIndex = 0;
            
            var preset = ColorPresets[currentColorIndex];
            if (preset.IsRainbow)
            {
                rainbowMode = true;
            }
            else
            {
                rainbowMode = false;
                currentHighlightColor = preset.Color;
            }
            
            useCustomColor = PlayerPrefs.GetInt(USE_CUSTOM_KEY, 0) == 1;
            if (useCustomColor)
            {
                rainbowMode = false;  // Custom color overrides rainbow
                customColor = new Color(
                    PlayerPrefs.GetFloat(CUSTOM_R_KEY, 1f),
                    PlayerPrefs.GetFloat(CUSTOM_G_KEY, 0.85f),
                    PlayerPrefs.GetFloat(CUSTOM_B_KEY, 0.4f),
                    1f
                );
            }
        }
        
        /// <summary>
        /// Save current color preference
        /// </summary>
        private void SaveColorPreference()
        {
            PlayerPrefs.SetInt(COLOR_INDEX_KEY, currentColorIndex);
            PlayerPrefs.SetInt(USE_CUSTOM_KEY, useCustomColor ? 1 : 0);
            if (useCustomColor)
            {
                PlayerPrefs.SetFloat(CUSTOM_R_KEY, customColor.r);
                PlayerPrefs.SetFloat(CUSTOM_G_KEY, customColor.g);
                PlayerPrefs.SetFloat(CUSTOM_B_KEY, customColor.b);
            }
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Cycle to the next highlight color preset
        /// </summary>
        public void CycleColor()
        {
            useCustomColor = false;
            currentColorIndex = (currentColorIndex + 1) % ColorPresets.Count;
            
            var preset = ColorPresets[currentColorIndex];
            if (preset.IsRainbow)
            {
                rainbowMode = true;
            }
            else
            {
                rainbowMode = false;
                currentHighlightColor = preset.Color;
            }
            
            SaveColorPreference();
            Log?.LogInfo($"[HIGHLIGHT] Color changed to: {CurrentColorName}");
        }
        
        /// <summary>
        /// Cycle to previous highlight color preset
        /// </summary>
        public void CycleColorBack()
        {
            useCustomColor = false;
            currentColorIndex--;
            if (currentColorIndex < 0) currentColorIndex = ColorPresets.Count - 1;
            
            var preset = ColorPresets[currentColorIndex];
            if (preset.IsRainbow)
            {
                rainbowMode = true;
            }
            else
            {
                rainbowMode = false;
                currentHighlightColor = preset.Color;
            }
            
            SaveColorPreference();
            Log?.LogInfo($"[HIGHLIGHT] Color changed to: {CurrentColorName}");
        }
        
        /// <summary>
        /// Set a custom RGB color (0-255 values)
        /// </summary>
        public void SetCustomColor(int r, int g, int b)
        {
            customColor = new Color(r / 255f, g / 255f, b / 255f, 1f);
            useCustomColor = true;
            rainbowMode = false;
            SaveColorPreference();
            Log?.LogInfo($"[HIGHLIGHT] Custom color set: RGB({r}, {g}, {b})");
        }
        
        /// <summary>
        /// Set a custom color directly
        /// </summary>
        public void SetCustomColor(Color color)
        {
            customColor = new Color(color.r, color.g, color.b, 1f);
            useCustomColor = true;
            rainbowMode = false;
            SaveColorPreference();
            Log?.LogInfo($"[HIGHLIGHT] Custom color set");
        }
        
        /// <summary>
        /// Main update loop - call from Plugin.Update()
        /// </summary>
        public void Update()
        {
            if (!HighlightingEnabled) return;
            if (GameManager.instance == null) return;
            
            // Update pulse animation
            pulseTime += Time.deltaTime * PulseSpeed;
            float pulse = (Mathf.Sin(pulseTime * Mathf.PI * 2f) + 1f) * 0.5f;  // 0 to 1
            float brightness = Mathf.Lerp(MinBrightness, MaxBrightness, pulse);
            
            // Update rainbow hue if in rainbow mode
            if (rainbowMode)
            {
                rainbowHue += Time.deltaTime * RainbowSpeed;
                if (rainbowHue > 1f) rainbowHue -= 1f;
            }
            
            // Periodic rescan for new props (items can spawn/move)
            rescanTimer += Time.deltaTime;
            if (!hasScanned || rescanTimer >= RESCAN_INTERVAL)
            {
                rescanTimer = 0f;
                ScanForProps();
                hasScanned = true;
            }
            
            // Update highlight colors on all tracked props
            UpdateHighlights(brightness);
            
            // Check for newly completed locations and remove their highlights
            CleanupCheckedLocations();
        }
        
        /// <summary>
        /// Scan the scene for props that are pickup/drag locations
        /// </summary>
        private void ScanForProps()
        {
            // Find all Prop components in the scene
            var allProps = UnityEngine.Object.FindObjectsOfType<Prop>();
            
            foreach (var prop in allProps)
            {
                if (prop == null || prop.gameObject == null) continue;
                
                int instanceId = prop.gameObject.GetInstanceID();
                
                // Skip if already tracking
                if (highlightedProps.ContainsKey(instanceId)) continue;
                
                // Get the item key for this prop
                string itemName = prop.gameObject.name;
                string itemKey = LocationMappings.CleanItemName(itemName);
                
                // Check if this is a valid pickup or drag location
                bool isPickup = LocationMappings.GetItemLocationId(itemKey).HasValue;
                bool isDrag = LocationMappings.GetDragLocationId(itemKey).HasValue;
                
                if (!isPickup && !isDrag) continue;
                
                // Check if already checked
                if (IsLocationChecked(itemKey, isPickup)) continue;
                
                // Add to tracking
                var highlighted = new HighlightedProp
                {
                    gameObject = prop.gameObject,
                    itemKey = itemKey,
                    isPickup = isPickup,
                    needsRefresh = false
                };
                
                // Get renderers
                highlighted.renderers = prop.gameObject.GetComponentsInChildren<Renderer>();
                
                if (highlighted.renderers != null && highlighted.renderers.Length > 0)
                {
                    highlightedProps[instanceId] = highlighted;
                    Log?.LogInfo($"[HIGHLIGHT] Tracking new prop: {itemKey} (isPickup={isPickup}, renderers={highlighted.renderers.Length})");
                }
            }

            var checkAllGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            Log.LogInfo($"[HIGHLIGHT] Found {checkAllGameObjects.Length} 'checkAllGameObjects' total");
            foreach (var gameObj in checkAllGameObjects)
            {
                if (gameObj == null) continue;
                if (gameObj.name == "braSkinned")
                {
                    HighlightSpecificGameObject(gameObj, "rightstrap");
                }
                else if (gameObj.name == "fertilizer")
                {
                    if (gameObj.transform.parent.name == "FertilizerPile")
                    {
                        HighlightSpecificGameObject(gameObj, "top_1");
                    }
                    else if (gameObj.transform.parent.name == "HoleBlockerHomes")
                    {
                        HighlightSpecificGameObject(gameObj, "top_3");
                    }
                }
                else if (gameObj.name == "fertilizer (1)" && gameObj.transform.parent.name == "FertilizerPile")
                {
                    HighlightSpecificGameObject(gameObj, "top_2");
                }
                // TO DO: check if darts highlighting can be separate from each other & dartboard
            }
        }

        /// <summary>
        /// For highlighting specific special props
        /// </summary>
        private void HighlightSpecificGameObject(GameObject gameObj, string cleanName)
        {    
            int instanceId = gameObj.GetInstanceID();
            
            // Skip if already tracking
            if (highlightedProps.ContainsKey(instanceId)) return;
        
            // Get the item key for this prop
            string itemKey = cleanName;
            
            // Check if this is a valid pickup or drag location
            bool isPickup = LocationMappings.GetItemLocationId(itemKey).HasValue;
            bool isDrag = LocationMappings.GetDragLocationId(itemKey).HasValue;
            
            if (!isPickup && !isDrag) return;
            
            // Check if already checked
            if (IsLocationChecked(itemKey, isPickup)) return;
            
            // Add to tracking
            var highlighted = new HighlightedProp
            {
                gameObject = gameObj,
                itemKey = itemKey,
                isPickup = isPickup,
                needsRefresh = false
            };
            
            // Get renderers
            highlighted.renderers = gameObj.GetComponentsInChildren<Renderer>();
            
            if (highlighted.renderers != null && highlighted.renderers.Length > 0)
            {
                highlightedProps[instanceId] = highlighted;
                Log?.LogInfo($"[HIGHLIGHT] Tracking new prop: {itemKey} (isPickup={isPickup}, renderers={highlighted.renderers.Length})");
            }
        }
        
        /// <summary>
        /// Update highlight colors on all tracked props
        /// </summary>
        private void UpdateHighlights(float brightness)
        {
            Color baseColor;
            
            if (rainbowMode)
            {
                // Convert HSV to RGB for rainbow effect
                baseColor = Color.HSVToRGB(rainbowHue, 1f, 1f);
            }
            else if (useCustomColor)
            {
                baseColor = customColor;
            }
            else
            {
                baseColor = currentHighlightColor;
            }
            
            Color tintColor = Color.Lerp(Color.white, baseColor, brightness);
            
            foreach (var kvp in highlightedProps)
            {
                var highlighted = kvp.Value;
                if (highlighted.gameObject == null)
                {
                    highlighted.needsRefresh = true;
                    continue;
                }
                
                ApplyHighlightColor(highlighted, tintColor);
            }
        }
        
        /// <summary>
        /// Apply highlight color to a prop's renderers using vertex colors
        /// Uses static dictionary to store originals - same pattern as GooseColourManager
        /// </summary>
        private void ApplyHighlightColor(HighlightedProp highlighted, Color tintColor)
        {
            foreach (var renderer in highlighted.renderers)
            {
                if (renderer == null) continue;
                
                try
                {
                    // Check if renderer is still valid
                    if (renderer.gameObject == null)
                    {
                        highlighted.needsRefresh = true;
                        continue;
                    }

                    // Get mesh from MeshFilter or SkinnedMeshRenderer
                    Mesh mesh = null;
                    MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        mesh = meshFilter.sharedMesh;
                    }
                    else if (renderer is SkinnedMeshRenderer skinnedRenderer)
                    {
                        mesh = skinnedRenderer.sharedMesh;
                    }
                    
                    if (mesh == null) continue;
                    
                    // Get or create original colors for this mesh (never re-read from mesh after first store)
                    Color[] originalColors;
                    if (!originalVertexColorsByMesh.TryGetValue(mesh, out originalColors))
                    {
                        // First time seeing this mesh - store the REAL original colors
                        Color[] meshColors = mesh.colors;
                        
                        if (meshColors != null && meshColors.Length > 0)
                        {
                            // Clone the actual original colors
                            originalColors = (Color[])meshColors.Clone();
                        }
                        else
                        {
                            // No vertex colors exist, default to white
                            originalColors = new Color[mesh.vertexCount];
                            for (int v = 0; v < originalColors.Length; v++)
                            {
                                originalColors[v] = Color.white;
                            }
                        }
                        originalVertexColorsByMesh[mesh] = originalColors;
                    }
                    
                    // Apply tint to vertex colors
                    Color[] newColors = new Color[originalColors.Length];
                    for (int v = 0; v < originalColors.Length; v++)
                    {
                        Color baseColor = originalColors[v];
                        
                        // Multiply the base color by our tint
                        newColors[v] = new Color(
                            baseColor.r * tintColor.r,
                            baseColor.g * tintColor.g,
                            baseColor.b * tintColor.b,
                            baseColor.a
                        );
                    }
                    
                    // Apply the new colors
                    mesh.colors = newColors;
                }
                catch (Exception)
                {
                    highlighted.needsRefresh = true;
                }
            }
        }
        
        /// <summary>
        /// Restore original colors by applying white tint (original * white = original)
        /// </summary>
        private void RestoreOriginalColors(HighlightedProp highlighted)
        {
            Log?.LogInfo($"[HIGHLIGHT] RestoreOriginalColors called for: {highlighted.itemKey}");
            
            if (highlighted.renderers == null)
            {
                Log?.LogWarning($"[HIGHLIGHT] No renderers to restore for {highlighted.itemKey}");
                return;
            }
            
            int restoredCount = 0;
            
            foreach (var renderer in highlighted.renderers)
            {
                if (renderer == null) continue;
                
                try
                {
                    // Get mesh from MeshFilter or SkinnedMeshRenderer
                    Mesh mesh = null;
                    MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        mesh = meshFilter.sharedMesh;
                    }
                    else if (renderer is SkinnedMeshRenderer skinnedRenderer)
                    {
                        mesh = skinnedRenderer.sharedMesh;
                    }
                    
                    if (mesh == null) continue;
                    
                    // Get stored original colors
                    Color[] originalColors;
                    if (originalVertexColorsByMesh.TryGetValue(mesh, out originalColors))
                    {
                        // Restore by applying original colors directly
                        mesh.colors = (Color[])originalColors.Clone();
                        restoredCount++;
                    }
                }
                catch (Exception ex)
                {
                    Log?.LogError($"[HIGHLIGHT] Error restoring colors: {ex.Message}");
                }
            }
            
            Log?.LogInfo($"[HIGHLIGHT] Restored {restoredCount} meshes for {highlighted.itemKey}");
        }
        
        /// <summary>
        /// Check if a location has been checked (sent to AP server)
        /// Uses the actual location ID from LocationMappings to check against AP slot data
        /// </summary>
        private bool IsLocationChecked(string itemKey, bool isPickup)
        {
            long? locationId;
            
            if (isPickup)
            {
                locationId = LocationMappings.GetItemLocationId(itemKey);
            }
            else
            {
                locationId = LocationMappings.GetDragLocationId(itemKey);
            }
            
            if (!locationId.HasValue) return false;
            
            return plugin.IsLocationChecked(locationId.Value);
        }
        
        /// <summary>
        /// Remove highlights from props that have been checked
        /// </summary>
        private void CleanupCheckedLocations()
        {
            List<int> toRemove = new List<int>();
            
            foreach (var kvp in highlightedProps)
            {
                var highlighted = kvp.Value;
                
                // Check if GameObject was destroyed
                if (highlighted.gameObject == null || highlighted.needsRefresh)
                {
                    toRemove.Add(kvp.Key);
                    continue;
                }
                
                // Check if location was checked
                if (IsLocationChecked(highlighted.itemKey, highlighted.isPickup))
                {
                    RestoreOriginalColors(highlighted);
                    toRemove.Add(kvp.Key);
                    Log?.LogInfo($"[HIGHLIGHT] Removed highlight from checked location: {highlighted.itemKey}");
                }
            }
            
            foreach (int key in toRemove)
            {
                highlightedProps.Remove(key);
            }
        }
        
        /// <summary>
        /// Called when a location is checked - immediately remove its highlight
        /// </summary>
        public void OnLocationChecked(string itemKey)
        {
            Log?.LogInfo($"[HIGHLIGHT] OnLocationChecked called for: {itemKey}");
            
            // Extract base key (remove position suffix like _1, _2, etc.)
            string baseKey = itemKey;
            int underscorePos = itemKey.LastIndexOf('_');
            if (underscorePos > 0)
            {
                string suffix = itemKey.Substring(underscorePos + 1);
                int num;
                if (int.TryParse(suffix, out num))
                {
                    baseKey = itemKey.Substring(0, underscorePos);
                }
            }
            
            List<int> toRemove = new List<int>();
            
            foreach (var kvp in highlightedProps)
            {
                // Match either exact key or base key
                if (kvp.Value.itemKey == itemKey || kvp.Value.itemKey == baseKey)
                {
                    Log?.LogInfo($"[HIGHLIGHT] Found matching prop to restore: {kvp.Value.itemKey} (GameObject: {kvp.Value.gameObject?.name})");
                    RestoreOriginalColors(kvp.Value);
                    toRemove.Add(kvp.Key);
                }
            }
            
            Log?.LogInfo($"[HIGHLIGHT] Removed {toRemove.Count} highlights for {itemKey}");
            
            foreach (int key in toRemove)
            {
                highlightedProps.Remove(key);
            }
        }
        
        /// <summary>
        /// Reset all highlighting (e.g., when returning to menu)
        /// </summary>
        public void Reset()
        {
            // Restore all original colors
            foreach (var kvp in highlightedProps)
            {
                RestoreOriginalColors(kvp.Value);
            }
            
            highlightedProps.Clear();
            hasScanned = false;
            rescanTimer = 0f;
            Log?.LogInfo("[HIGHLIGHT] Reset highlight tracking");
        }
        
        /// <summary>
        /// Toggle highlighting on/off
        /// </summary>
        public void Toggle()
        {
            HighlightingEnabled = !HighlightingEnabled;
            
            if (!HighlightingEnabled)
            {
                // Restore all original colors when disabling
                foreach (var kvp in highlightedProps)
                {
                    RestoreOriginalColors(kvp.Value);
                }
            }
            
            Log?.LogInfo($"[HIGHLIGHT] Highlighting {(HighlightingEnabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Get the count of currently highlighted (unchecked) props
        /// </summary>
        public int HighlightedCount => highlightedProps.Count;
    }
}
