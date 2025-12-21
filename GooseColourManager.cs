using System;
using System.Collections.Generic;
using UnityEngine;

namespace GooseGameAP
{
    /// <summary>
    /// Manages goose color customization
    /// Based on PsychedelicGooseMod approach
    /// </summary>
    public class GooseColourManager
    {
        private Plugin plugin;
        private SkinnedMeshRenderer[] gooseRenderers;
        private Material[] originalMaterials;
        private Material gooseSkinMaterial; // Direct reference to goose's skin material
        private Dictionary<Mesh, Color[]> originalVertexColoursByMesh = new Dictionary<Mesh, Color[]>(); // Store originals per mesh - never cleared
        private bool hasStoredOriginals = false;
        private bool hasLoggedSearch = false;
        
        // Current color state
        private int currentColourIndex = 0;
        private Color currentColor = Color.white;
        private Color targetColor = Color.white;
        private bool rainbowMode = false;
        private float rainbowHue = 0f;
        private float lerpProgress = 0f;
        
        // Preset colors
        public static readonly List<GooseColourPreset> ColourPresets = new List<GooseColourPreset>
        {
            new GooseColourPreset("Default", new Color(1f, 1f, 1f, 1f)),
            new GooseColourPreset("Golden Goose", new Color(1f, 0.84f, 0f, 1f)),
            new GooseColourPreset("Angry Red", new Color(1f, 0.3f, 0.3f, 1f)),
            new GooseColourPreset("Mysterious Purple", new Color(0.7f, 0.3f, 1f, 1f)),
            new GooseColourPreset("Cyber Blue", new Color(0.3f, 0.7f, 1f, 1f)),
            new GooseColourPreset("Toxic Green", new Color(0.3f, 1f, 0.3f, 1f)),
            new GooseColourPreset("Shadow Goose", new Color(0.2f, 0.2f, 0.2f, 1f)),
            new GooseColourPreset("Pink Menace", new Color(1f, 0.5f, 0.8f, 1f)),
            new GooseColourPreset("Orange Chaos", new Color(1f, 0.5f, 0.1f, 1f)),
            new GooseColourPreset("Ice Cold", new Color(0.7f, 0.9f, 1f, 1f)),
            new GooseColourPreset("Rainbow", Color.white, true)
        };
        
        public string CurrentColourName => ColourPresets[currentColourIndex].Name;
        public Color CurrentColor => currentColor;
        public bool IsRainbowMode => rainbowMode;
        
        public GooseColourManager(Plugin plugin)
        {
            this.plugin = plugin;
        }
        
        /// <summary>
        /// Update - call from Plugin.Update for rainbow mode and color lerping
        /// </summary>
        public void Update()
        {
            // Try to find renderers if we don't have them or if they became invalid
            if (gooseRenderers == null || gooseRenderers.Length == 0 || !AreRenderersValid())
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
        /// Check if cached renderers are still valid (not destroyed)
        /// </summary>
        private bool AreRenderersValid()
        {
            if (gooseRenderers == null || gooseRenderers.Length == 0)
                return false;
                
            // Check if first renderer is still valid (Unity objects become "fake null" when destroyed)
            try
            {
                var firstRenderer = gooseRenderers[0];
                if (firstRenderer == null) return false;
                
                // This will throw or return null if the object was destroyed
                var go = firstRenderer.gameObject;
                if (go == null) return false;
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Cycle to the next color preset
        /// </summary>
        public void CycleColour()
        {
            currentColourIndex = (currentColourIndex + 1) % ColourPresets.Count;
            ApplyPreset(currentColourIndex);
        }
        
        /// <summary>
        /// Cycle to the previous color preset
        /// </summary>
        public void CycleColourBack()
        {
            currentColourIndex--;
            if (currentColourIndex < 0) currentColourIndex = ColourPresets.Count - 1;
            ApplyPreset(currentColourIndex);
        }
        
        /// <summary>
        /// Apply a specific preset by index
        /// </summary>
        public void ApplyPreset(int index)
        {
            if (index < 0 || index >= ColourPresets.Count) return;
            
            currentColourIndex = index;
            var preset = ColourPresets[index];
            
            if (preset.IsRainbow)
            {
                rainbowMode = true;
            }
            else
            {
                rainbowMode = false;
                targetColor = preset.Color;
                lerpProgress = 0f;
                
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
                if (gooseRenderers == null || gooseRenderers.Length == 0 || !AreRenderersValid())
                {
                    FindGooseRenderers();
                    if (gooseRenderers == null || gooseRenderers.Length == 0)
                    {
                        return;
                    }
                }
                
                for (int i = 0; i < gooseRenderers.Length; i++)
                {
                    var renderer = gooseRenderers[i];
                    if (renderer == null) continue;
                    
                    // Check if renderer is still valid (destroyed objects are "fake null")
                    try
                    {
                        if (renderer.gameObject == null)
                        {
                            // Renderer was destroyed, trigger refresh next frame
                            gooseRenderers = null;
                            return;
                        }
                    }
                    catch
                    {
                        gooseRenderers = null;
                        return;
                    }
                    
                    // For vertex color shaders, we need to modify the mesh's vertex colors
                    Mesh mesh = renderer.sharedMesh;
                    if (mesh == null) continue;
                    
                    // Get or create original colours for this mesh (never re-read from mesh after first store)
                    Color[] originalColours;
                    if (!originalVertexColoursByMesh.TryGetValue(mesh, out originalColours))
                    {
                        // First time seeing this mesh - store the REAL original colours
                        Color[] meshColours = mesh.colors;
                        
                        if (meshColours != null && meshColours.Length > 0)
                        {
                            // Clone the actual original colours (beak orange, feet orange, body white, etc.)
                            originalColours = (Color[])meshColours.Clone();
                        }
                        else
                        {
                            // No vertex colours exist, default to white
                            originalColours = new Color[mesh.vertexCount];
                            for (int v = 0; v < originalColours.Length; v++)
                            {
                                originalColours[v] = Color.white;
                            }
                        }
                        originalVertexColoursByMesh[mesh] = originalColours;
                    }
                    
                    // Apply tint to vertex colours
                    Color[] newColours = new Color[originalColours.Length];
                    for (int v = 0; v < originalColours.Length; v++)
                    {
                        Color baseColour = originalColours[v];
                        
                        // Multiply the base colour by our tint
                        newColours[v] = new Color(
                            baseColour.r * color.r,
                            baseColour.g * color.g,
                            baseColour.b * color.b,
                            baseColour.a
                        );
                    }
                    
                    // Apply the new colours
                    mesh.colors = newColours;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[GOOSE COLOUR] Error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Reset to default color
        /// </summary>
        public void ResetToDefault()
        {
            currentColourIndex = 0;
            rainbowMode = false;
            targetColor = Color.white;
            currentColor = Color.white;
            lerpProgress = 0f;
            
            // Restore original vertex colors
            if (gooseRenderers != null)
            {
                foreach (var renderer in gooseRenderers)
                {
                    if (renderer?.sharedMesh != null)
                    {
                        Mesh mesh = renderer.sharedMesh;
                        if (originalVertexColoursByMesh.TryGetValue(mesh, out Color[] originals))
                        {
                            mesh.colors = originals;
                        }
                    }
                }
            }
            
        }
        
        /// <summary>
        /// Find the goose's renderers - like PsychedelicGooseMod does
        /// </summary>
        private void FindGooseRenderers()
        {
            try
            {
                // Try to find goose via GameManager
                Goose goose = null;
                
                if (GameManager.instance?.allGeese != null && GameManager.instance.allGeese.Count > 0)
                {
                    goose = GameManager.instance.allGeese[0];
                }
                
                // Also try GameObject.Find like the mod does
                if (goose == null)
                {
                    var gooseObj = GameObject.Find("Goose");
                    if (gooseObj != null)
                    {
                        goose = gooseObj.GetComponent<Goose>();
                    }
                }
                
                // Try finding any Goose component in the scene
                if (goose == null)
                {
                    goose = GameObject.FindObjectOfType<Goose>();
                }
                
                if (goose == null)
                {
                    hasLoggedSearch = true;
                    return;
                }
                
                // Get all SkinnedMeshRenderers - this is what PsychedelicGooseMod does
                gooseRenderers = goose.GetComponentsInChildren<SkinnedMeshRenderer>();
                
                
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
                    }
                    else
                    {
                        
                        // List all fields on Goose for debugging
                        var allFields = typeof(Goose).GetFields(flags);
                        foreach (var f in allFields)
                        {
                            if (f.FieldType == typeof(Material) || 
                                f.FieldType == typeof(SkinnedMeshRenderer) ||
                                f.FieldType.Name.Contains("Skin") ||
                                f.Name.ToLower().Contains("skin") ||
                                f.Name.ToLower().Contains("material") ||
                                f.Name.ToLower().Contains("render"))
                            {
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
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
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                            
                            // Log enabled keywords
                            string[] keywords = mat.shaderKeywords;
                            if (keywords.Length > 0)
                            {
                            }
                        }
                    }
                }
                else
                {
                    // Try regular MeshRenderers as fallback
                    var meshRenderers = goose.GetComponentsInChildren<MeshRenderer>();
                    
                    // Also check all Renderers
                    var allRenderers = goose.GetComponentsInChildren<Renderer>();
                    foreach (var r in allRenderers)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        
        /// <summary>
        /// Force refresh the goose renderers
        /// </summary>
        public void RefreshRenderers()
        {
            gooseRenderers = null;
            gooseSkinMaterial = null;
            // NOTE: Do NOT clear originalVertexColoursByMesh - we need to keep the true originals
            hasStoredOriginals = false;
            hasLoggedSearch = false;
            FindGooseRenderers();
            
            // Re-apply current color
            if (currentColourIndex > 0 || rainbowMode)
            {
                ApplyColorDirect(currentColor);
            }
        }
    }
    
    /// <summary>
    /// Represents a color preset for the goose
    /// </summary>
    public class GooseColourPreset
    {
        public string Name { get; }
        public Color Color { get; }
        public bool IsRainbow { get; }
        
        public GooseColourPreset(string name, Color color, bool isRainbow = false)
        {
            Name = name;
            Color = color;
            IsRainbow = isRainbow;
        }
    }
}