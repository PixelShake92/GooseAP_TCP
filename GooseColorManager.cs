using System;
using System.Collections.Generic;
using UnityEngine;

namespace GooseGameAP
{
    /// <summary>
    /// Manages goose color customization
    /// Based on PsychedelicGooseMod approach
    /// </summary>
    public class GooseColorManager
    {
        private Plugin plugin;
        private SkinnedMeshRenderer[] gooseRenderers;
        private Material[] originalMaterials;
        private Material gooseSkinMaterial; // Direct reference to goose's skin material
        private Color[] originalVertexColors; // Store original mesh vertex colors
        private bool hasStoredOriginals = false;
        private bool hasLoggedSearch = false;
        private Color lastLoggedColor = Color.clear;
        
        // Current color state
        private int currentColorIndex = 0;
        private Color currentColor = Color.white;
        private Color targetColor = Color.white;
        private bool rainbowMode = false;
        private float rainbowHue = 0f;
        private float lerpProgress = 0f;
        
        // Preset colors
        public static readonly List<GooseColorPreset> ColorPresets = new List<GooseColorPreset>
        {
            new GooseColorPreset("Default", new Color(1f, 1f, 1f, 1f)),
            new GooseColorPreset("Golden Goose", new Color(1f, 0.84f, 0f, 1f)),
            new GooseColorPreset("Angry Red", new Color(1f, 0.3f, 0.3f, 1f)),
            new GooseColorPreset("Mysterious Purple", new Color(0.7f, 0.3f, 1f, 1f)),
            new GooseColorPreset("Cyber Blue", new Color(0.3f, 0.7f, 1f, 1f)),
            new GooseColorPreset("Toxic Green", new Color(0.3f, 1f, 0.3f, 1f)),
            new GooseColorPreset("Shadow Goose", new Color(0.2f, 0.2f, 0.2f, 1f)),
            new GooseColorPreset("Pink Menace", new Color(1f, 0.5f, 0.8f, 1f)),
            new GooseColorPreset("Orange Chaos", new Color(1f, 0.5f, 0.1f, 1f)),
            new GooseColorPreset("Ice Cold", new Color(0.7f, 0.9f, 1f, 1f)),
            new GooseColorPreset("Rainbow", Color.white, true)
        };
        
        public string CurrentColorName => ColorPresets[currentColorIndex].Name;
        public Color CurrentColor => currentColor;
        public bool IsRainbowMode => rainbowMode;
        
        public GooseColorManager(Plugin plugin)
        {
            this.plugin = plugin;
        }
        
        /// <summary>
        /// Update - call from Plugin.Update for rainbow mode and color lerping
        /// </summary>
        public void Update()
        {
            // Try to find renderers if we don't have them
            if (gooseRenderers == null || gooseRenderers.Length == 0)
            {
                FindGooseRenderers();
                if (gooseRenderers == null || gooseRenderers.Length == 0)
                    return;
            }
                
            if (rainbowMode)
            {
                // Cycle through hues
                rainbowHue += Time.deltaTime * 0.5f;
                if (rainbowHue > 1f) rainbowHue -= 1f;
                
                targetColor = Color.HSVToRGB(rainbowHue, 0.7f, 1f);
            }
            
            // Lerp current color toward target
            if (currentColor != targetColor)
            {
                lerpProgress += Time.deltaTime * 2f; // Fade over 0.5 seconds
                currentColor = Color.Lerp(currentColor, targetColor, lerpProgress);
                
                if (lerpProgress >= 1f)
                {
                    currentColor = targetColor;
                    lerpProgress = 0f;
                }
                
                ApplyColorDirect(currentColor);
            }
            else if (rainbowMode)
            {
                // Keep updating in rainbow mode
                lerpProgress = 0f;
                ApplyColorDirect(targetColor);
            }
        }
        
        /// <summary>
        /// Cycle to the next color preset
        /// </summary>
        public void CycleColor()
        {
            currentColorIndex = (currentColorIndex + 1) % ColorPresets.Count;
            ApplyPreset(currentColorIndex);
        }
        
        /// <summary>
        /// Cycle to the previous color preset
        /// </summary>
        public void CycleColorBack()
        {
            currentColorIndex--;
            if (currentColorIndex < 0) currentColorIndex = ColorPresets.Count - 1;
            ApplyPreset(currentColorIndex);
        }
        
        /// <summary>
        /// Apply a specific preset by index
        /// </summary>
        public void ApplyPreset(int index)
        {
            if (index < 0 || index >= ColorPresets.Count) return;
            
            currentColorIndex = index;
            var preset = ColorPresets[index];
            
            if (preset.IsRainbow)
            {
                rainbowMode = true;
                Plugin.Log?.LogInfo($"[GOOSE COLOR] Rainbow mode activated!");
            }
            else
            {
                rainbowMode = false;
                targetColor = preset.Color;
                lerpProgress = 0f;
                Plugin.Log?.LogInfo($"[GOOSE COLOR] Set to: {preset.Name}");
                
                // Immediately try to apply (will also find renderers if needed)
                ApplyColorDirect(preset.Color);
            }
        }
        
        /// <summary>
        /// Apply color directly to all renderers - modifies vertex colors for vertex color shaders
        /// </summary>
        private void ApplyColorDirect(Color color)
        {
            try
            {
                if (gooseRenderers == null || gooseRenderers.Length == 0)
                {
                    FindGooseRenderers();
                    if (gooseRenderers == null || gooseRenderers.Length == 0)
                    {
                        Plugin.Log?.LogWarning("[GOOSE COLOR] No renderers to apply color to");
                        return;
                    }
                }
                
                // Only log when color changes significantly (not every rainbow frame)
                if (lastLoggedColor != color && !rainbowMode)
                {
                    Plugin.Log?.LogInfo($"[GOOSE COLOR] Applying vertex color tint {color}");
                    lastLoggedColor = color;
                }
                
                for (int i = 0; i < gooseRenderers.Length; i++)
                {
                    var renderer = gooseRenderers[i];
                    if (renderer == null) continue;
                    
                    // For vertex color shaders, we need to modify the mesh's vertex colors
                    Mesh mesh = renderer.sharedMesh;
                    if (mesh == null) continue;
                    
                    // Get current vertex colors (or create white array if none)
                    Color[] colors = mesh.colors;
                    if (colors == null || colors.Length == 0)
                    {
                        // Initialize with white if no vertex colors exist
                        colors = new Color[mesh.vertexCount];
                        for (int v = 0; v < colors.Length; v++)
                        {
                            colors[v] = Color.white;
                        }
                        Plugin.Log?.LogInfo($"[GOOSE COLOR] No vertex colors found, initialized {colors.Length} white vertices");
                    }
                    
                    // Store original colors on first access
                    if (originalVertexColors == null)
                    {
                        originalVertexColors = (Color[])colors.Clone();
                        Plugin.Log?.LogInfo($"[GOOSE COLOR] Stored {originalVertexColors.Length} original vertex colors");
                    }
                    
                    // Apply tint to vertex colors
                    Color[] newColors = new Color[originalVertexColors.Length];
                    for (int v = 0; v < originalVertexColors.Length; v++)
                    {
                        Color baseColor = originalVertexColors[v];
                        
                        // Multiply the base color by our tint
                        newColors[v] = new Color(
                            baseColor.r * color.r,
                            baseColor.g * color.g,
                            baseColor.b * color.b,
                            baseColor.a
                        );
                    }
                    
                    // Apply the new colors
                    mesh.colors = newColors;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[GOOSE COLOR] Error applying color: {ex.Message}");
                Plugin.Log?.LogError($"[GOOSE COLOR] Stack: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// Reset to default color
        /// </summary>
        public void ResetToDefault()
        {
            currentColorIndex = 0;
            rainbowMode = false;
            targetColor = Color.white;
            currentColor = Color.white;
            lerpProgress = 0f;
            
            // Restore original vertex colors if we have them
            if (originalVertexColors != null && gooseRenderers != null)
            {
                foreach (var renderer in gooseRenderers)
                {
                    if (renderer?.sharedMesh != null)
                    {
                        renderer.sharedMesh.colors = originalVertexColors;
                    }
                }
            }
            
            Plugin.Log?.LogInfo("[GOOSE COLOR] Reset to default");
        }
        
        /// <summary>
        /// Find the goose's renderers - like PsychedelicGooseMod does
        /// </summary>
        private void FindGooseRenderers()
        {
            try
            {
                if (!hasLoggedSearch)
                {
                    Plugin.Log?.LogInfo("[GOOSE COLOR] Searching for goose renderers...");
                }
                
                // Try to find goose via GameManager
                Goose goose = null;
                
                if (GameManager.instance?.allGeese != null && GameManager.instance.allGeese.Count > 0)
                {
                    goose = GameManager.instance.allGeese[0];
                    if (!hasLoggedSearch)
                        Plugin.Log?.LogInfo($"[GOOSE COLOR] Found goose via GameManager: {goose?.name}");
                }
                
                // Also try GameObject.Find like the mod does
                if (goose == null)
                {
                    var gooseObj = GameObject.Find("Goose");
                    if (gooseObj != null)
                    {
                        goose = gooseObj.GetComponent<Goose>();
                        if (!hasLoggedSearch)
                            Plugin.Log?.LogInfo($"[GOOSE COLOR] Found goose via GameObject.Find: {goose?.name}");
                    }
                }
                
                // Try finding any Goose component in the scene
                if (goose == null)
                {
                    goose = GameObject.FindObjectOfType<Goose>();
                    if (goose != null && !hasLoggedSearch)
                        Plugin.Log?.LogInfo($"[GOOSE COLOR] Found goose via FindObjectOfType: {goose?.name}");
                }
                
                if (goose == null)
                {
                    if (!hasLoggedSearch)
                        Plugin.Log?.LogWarning("[GOOSE COLOR] Could not find Goose");
                    hasLoggedSearch = true;
                    return;
                }
                
                // Get all SkinnedMeshRenderers - this is what PsychedelicGooseMod does
                gooseRenderers = goose.GetComponentsInChildren<SkinnedMeshRenderer>();
                
                Plugin.Log?.LogInfo($"[GOOSE COLOR] Found {gooseRenderers?.Length ?? 0} SkinnedMeshRenderers");
                
                // Try to access m_skin field like PsychedelicGooseMod does
                try
                {
                    var flags = System.Reflection.BindingFlags.Public | 
                                System.Reflection.BindingFlags.NonPublic | 
                                System.Reflection.BindingFlags.Instance;
                    
                    var skinField = typeof(Goose).GetField("m_skin", flags);
                    if (skinField != null)
                    {
                        gooseSkinMaterial = skinField.GetValue(goose) as Material;
                        Plugin.Log?.LogInfo($"[GOOSE COLOR] Found m_skin field: {gooseSkinMaterial?.name}, Shader: {gooseSkinMaterial?.shader?.name}");
                    }
                    else
                    {
                        Plugin.Log?.LogInfo("[GOOSE COLOR] m_skin field not found on Goose");
                        
                        // List all fields on Goose for debugging
                        var allFields = typeof(Goose).GetFields(flags);
                        Plugin.Log?.LogInfo($"[GOOSE COLOR] Goose has {allFields.Length} fields:");
                        foreach (var f in allFields)
                        {
                            if (f.FieldType == typeof(Material) || 
                                f.FieldType == typeof(SkinnedMeshRenderer) ||
                                f.FieldType.Name.Contains("Skin") ||
                                f.Name.ToLower().Contains("skin") ||
                                f.Name.ToLower().Contains("material") ||
                                f.Name.ToLower().Contains("render"))
                            {
                                Plugin.Log?.LogInfo($"[GOOSE COLOR]   {f.Name}: {f.FieldType.Name}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log?.LogInfo($"[GOOSE COLOR] Could not access m_skin: {ex.Message}");
                }
                
                hasLoggedSearch = true;
                
                if (gooseRenderers != null && gooseRenderers.Length > 0)
                {
                    // Store original materials
                    if (!hasStoredOriginals)
                    {
                        originalMaterials = new Material[gooseRenderers.Length];
                        for (int i = 0; i < gooseRenderers.Length; i++)
                        {
                            if (gooseRenderers[i]?.material != null)
                            {
                                // Clone the material to preserve original
                                originalMaterials[i] = new Material(gooseRenderers[i].material);
                            }
                        }
                        hasStoredOriginals = true;
                    }
                    
                    // Log renderer info for debugging
                    foreach (var renderer in gooseRenderers)
                    {
                        if (renderer?.material != null)
                        {
                            var mat = renderer.material;
                            Plugin.Log?.LogInfo($"[GOOSE COLOR] Renderer: {renderer.name}");
                            Plugin.Log?.LogInfo($"[GOOSE COLOR]   Material: {mat.name}");
                            Plugin.Log?.LogInfo($"[GOOSE COLOR]   Shader: {mat.shader?.name}");
                            Plugin.Log?.LogInfo($"[GOOSE COLOR]   Current mat.color: {mat.color}");
                            
                            // Check all possible color property names
                            string[] possibleProps = { 
                                "_Color", "_BaseColor", "_TintColor", "_MainColor", 
                                "_EmissionColor", "_SpecColor", "_ReflectColor",
                                "_Tint", "_ColorTint", "_Albedo", "_MainTex_ST"
                            };
                            
                            foreach (var prop in possibleProps)
                            {
                                if (mat.HasProperty(prop))
                                {
                                    try
                                    {
                                        Color c = mat.GetColor(prop);
                                        Plugin.Log?.LogInfo($"[GOOSE COLOR]   Has {prop}: {c}");
                                    }
                                    catch
                                    {
                                        Plugin.Log?.LogInfo($"[GOOSE COLOR]   Has {prop} (not a color)");
                                    }
                                }
                            }
                            
                            // Log enabled keywords
                            string[] keywords = mat.shaderKeywords;
                            if (keywords.Length > 0)
                            {
                                Plugin.Log?.LogInfo($"[GOOSE COLOR]   Keywords: {string.Join(", ", keywords)}");
                            }
                        }
                    }
                }
                else
                {
                    // Try regular MeshRenderers as fallback
                    var meshRenderers = goose.GetComponentsInChildren<MeshRenderer>();
                    Plugin.Log?.LogInfo($"[GOOSE COLOR] No SkinnedMeshRenderers, found {meshRenderers.Length} MeshRenderers");
                    
                    // Also check all Renderers
                    var allRenderers = goose.GetComponentsInChildren<Renderer>();
                    Plugin.Log?.LogInfo($"[GOOSE COLOR] Total Renderers of any type: {allRenderers.Length}");
                    foreach (var r in allRenderers)
                    {
                        Plugin.Log?.LogInfo($"[GOOSE COLOR]   {r.GetType().Name}: {r.name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[GOOSE COLOR] Error finding renderers: {ex.Message}");
                Plugin.Log?.LogError($"[GOOSE COLOR] Stack: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// Force refresh the goose renderers
        /// </summary>
        public void RefreshRenderers()
        {
            gooseRenderers = null;
            gooseSkinMaterial = null;
            originalVertexColors = null;
            hasStoredOriginals = false;
            hasLoggedSearch = false;
            FindGooseRenderers();
            
            // Re-apply current color
            if (currentColorIndex > 0 || rainbowMode)
            {
                ApplyColorDirect(currentColor);
            }
        }
    }
    
    /// <summary>
    /// Represents a color preset for the goose
    /// </summary>
    public class GooseColorPreset
    {
        public string Name { get; }
        public Color Color { get; }
        public bool IsRainbow { get; }
        
        public GooseColorPreset(string name, Color color, bool isRainbow = false)
        {
            Name = name;
            Color = color;
            IsRainbow = isRainbow;
        }
    }
}