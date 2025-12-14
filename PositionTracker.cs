using System;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Logging;

namespace GooseGameAP
{
    /// <summary>
    /// Tracks original positions of duplicate items (carrots, umbrellas, fertilizer)
    /// to give them unique identifiers based on where they started in the scene.
    /// </summary>
    public class PositionTracker
    {
        private static ManualLogSource Log => Plugin.Log;
        
        // Cache of item positions by type
        // Key: item type (e.g., "carrot"), Value: list of (position, index) tuples
        private Dictionary<string, List<CachedItem>> cachedItems = new Dictionary<string, List<CachedItem>>();
        
        // Items we want to track via position caching (legacy - most now use renaming)
        private static readonly string[] TrackedItemTypes = new string[]
        {
            "fertiliser",  // UK spelling used in game
            "fertilizer"   // Just in case
        };
        
        // Distance threshold for matching - items within this distance are considered the same
        private const float MATCH_DISTANCE = 2.0f;
        
        private class CachedItem
        {
            public Vector3 OriginalPosition;
            public int Index;
            public int InstanceId;
            public bool Collected;
            
            public CachedItem(Vector3 pos, int index, int instanceId)
            {
                OriginalPosition = pos;
                Index = index;
                InstanceId = instanceId;
                Collected = false;
            }
        }
        
        /// <summary>
        /// Scan the scene for duplicate items and cache their positions.
        /// Call this when entering a new area.
        /// </summary>
        public void ScanAndCacheItems()
        {
            Log?.LogInfo("[POSITION] Scanning scene for duplicate items...");
            cachedItems.Clear();
            
            // Rename duplicate items to give them unique names
            RenameCarrots();
            RenamePubTomatoes();
            RenameUmbrellas();
            RenameBoots();
            RenameTopsoil();
            
            // Find all Prop objects in scene
            var allProps = UnityEngine.Object.FindObjectsOfType<Prop>();
            Log?.LogInfo($"[POSITION] Found {allProps.Length} total Props in scene");
            
            foreach (var prop in allProps)
            {
                if (prop == null || prop.gameObject == null) continue;
                
                string itemName = prop.gameObject.name.ToLower().Replace("(clone)", "").Trim();
                
                // Check tracked item types
                foreach (string trackedType in TrackedItemTypes)
                {
                    if (itemName.Contains(trackedType))
                    {
                        CacheItem(trackedType, prop);
                        break;
                    }
                }
            }
            
            // Log summary
            foreach (var kvp in cachedItems)
            {
                Log?.LogInfo($"[POSITION] Cached {kvp.Value.Count} {kvp.Key}(s):");
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    var item = kvp.Value[i];
                    Log?.LogInfo($"[POSITION]   {kvp.Key} #{item.Index}: ({item.OriginalPosition.x:F2}, {item.OriginalPosition.y:F2}, {item.OriginalPosition.z:F2})");
                }
            }
        }
        
        /// <summary>
        /// Find all carrots in the scene and rename them to carrot_1, carrot_2, etc.
        /// This gives each carrot a unique name for tracking purposes.
        /// </summary>
        public void RenameCarrots()
        {
            Log?.LogInfo("[RENAME] Looking for carrots to rename...");
            
            var allProps = UnityEngine.Object.FindObjectsOfType<Prop>();
            var carrots = new List<Prop>();
            
            foreach (var prop in allProps)
            {
                if (prop == null || prop.gameObject == null) continue;
                
                string name = prop.gameObject.name.ToLower().Trim();
                
                // Only rename carrots that are exactly "carrot" (not already renamed, not carrotnogreen)
                if (name == "carrot")
                {
                    carrots.Add(prop);
                }
            }
            
            if (carrots.Count == 0)
            {
                Log?.LogInfo("[RENAME] No carrots found to rename");
                return;
            }
            
            // Sort by position for consistent ordering (x, then z, then y)
            carrots.Sort((a, b) => {
                Vector3 posA = a.transform.position;
                Vector3 posB = b.transform.position;
                
                // First sort by X (large difference = different areas)
                if (Mathf.Abs(posA.x - posB.x) > 1.0f)
                    return posA.x.CompareTo(posB.x);
                
                // Then by Z
                if (Mathf.Abs(posA.z - posB.z) > 0.5f)
                    return posA.z.CompareTo(posB.z);
                
                // Then by Y
                return posA.y.CompareTo(posB.y);
            });
            
            // Rename each carrot
            for (int i = 0; i < carrots.Count; i++)
            {
                string newName = $"carrot_{i + 1}";
                Vector3 pos = carrots[i].transform.position;
                
                Log?.LogInfo($"[RENAME] Renaming '{carrots[i].gameObject.name}' to '{newName}' at ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
                carrots[i].gameObject.name = newName;
            }
            
            Log?.LogInfo($"[RENAME] Renamed {carrots.Count} carrots");
        }
        
        /// <summary>
        /// Find all pub tomatoes in the scene and rename them to pubtomato_1, pubtomato_2, etc.
        /// </summary>
        public void RenamePubTomatoes()
        {
            Log?.LogInfo("[RENAME] Looking for pub tomatoes to rename...");
            
            var allProps = UnityEngine.Object.FindObjectsOfType<Prop>();
            var tomatoes = new List<Prop>();
            
            foreach (var prop in allProps)
            {
                if (prop == null || prop.gameObject == null) continue;
                
                string name = prop.gameObject.name.ToLower().Trim();
                
                // Only rename tomatoes that are exactly "pubtomato" (not already renamed)
                if (name == "pubtomato")
                {
                    tomatoes.Add(prop);
                }
            }
            
            if (tomatoes.Count == 0)
            {
                Log?.LogInfo("[RENAME] No pub tomatoes found to rename");
                return;
            }
            
            // Sort by position for consistent ordering (x, then z, then y)
            tomatoes.Sort((a, b) => {
                Vector3 posA = a.transform.position;
                Vector3 posB = b.transform.position;
                
                if (Mathf.Abs(posA.x - posB.x) > 1.0f)
                    return posA.x.CompareTo(posB.x);
                
                if (Mathf.Abs(posA.z - posB.z) > 0.5f)
                    return posA.z.CompareTo(posB.z);
                
                return posA.y.CompareTo(posB.y);
            });
            
            // Rename each tomato
            for (int i = 0; i < tomatoes.Count; i++)
            {
                string newName = $"pubtomato_{i + 1}";
                Vector3 pos = tomatoes[i].transform.position;
                
                Log?.LogInfo($"[RENAME] Renaming '{tomatoes[i].gameObject.name}' to '{newName}' at ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
                tomatoes[i].gameObject.name = newName;
            }
            
            Log?.LogInfo($"[RENAME] Renamed {tomatoes.Count} pub tomatoes");
        }
        
        /// <summary>
        /// Find all umbrellas in the scene and rename them to umbrella_1, umbrella_2, etc.
        /// </summary>
        public void RenameUmbrellas()
        {
            Log?.LogInfo("[RENAME] Looking for umbrellas to rename...");
            
            var allProps = UnityEngine.Object.FindObjectsOfType<Prop>();
            var umbrellas = new List<Prop>();
            
            foreach (var prop in allProps)
            {
                if (prop == null || prop.gameObject == null) continue;
                
                string name = prop.gameObject.name.ToLower().Trim();
                
                // Only rename umbrellas that are exactly "umbrella" (not already renamed, not umbrellaOnStand etc)
                if (name == "umbrella")
                {
                    umbrellas.Add(prop);
                }
            }
            
            if (umbrellas.Count == 0)
            {
                Log?.LogInfo("[RENAME] No umbrellas found to rename");
                return;
            }
            
            // Sort by position for consistent ordering (x, then z, then y)
            umbrellas.Sort((a, b) => {
                Vector3 posA = a.transform.position;
                Vector3 posB = b.transform.position;
                
                if (Mathf.Abs(posA.x - posB.x) > 1.0f)
                    return posA.x.CompareTo(posB.x);
                
                if (Mathf.Abs(posA.z - posB.z) > 0.5f)
                    return posA.z.CompareTo(posB.z);
                
                return posA.y.CompareTo(posB.y);
            });
            
            // Rename each umbrella
            for (int i = 0; i < umbrellas.Count; i++)
            {
                string newName = $"umbrella_{i + 1}";
                Vector3 pos = umbrellas[i].transform.position;
                
                Log?.LogInfo($"[RENAME] Renaming '{umbrellas[i].gameObject.name}' to '{newName}' at ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
                umbrellas[i].gameObject.name = newName;
            }
            
            Log?.LogInfo($"[RENAME] Renamed {umbrellas.Count} umbrellas");
        }
        
        /// <summary>
        /// Find all boots in the scene and rename them to boot_1, boot_2, etc.
        /// Boot 1 is at Garden start area, Boot 2 is in Hub near dummyprop
        /// </summary>
        public void RenameBoots()
        {
            Log?.LogInfo("[RENAME] Looking for boots to rename...");
            
            var allProps = UnityEngine.Object.FindObjectsOfType<Prop>();
            var boots = new List<Prop>();
            
            foreach (var prop in allProps)
            {
                if (prop == null || prop.gameObject == null) continue;
                
                string name = prop.gameObject.name.ToLower().Trim();
                
                // Only rename boots that are exactly "boot" (not gumboot, not already renamed)
                if (name == "boot")
                {
                    boots.Add(prop);
                }
            }
            
            if (boots.Count == 0)
            {
                Log?.LogInfo("[RENAME] No boots found to rename");
                return;
            }
            
            // Sort by position for consistent ordering (x, then z, then y)
            boots.Sort((a, b) => {
                Vector3 posA = a.transform.position;
                Vector3 posB = b.transform.position;
                
                if (Mathf.Abs(posA.x - posB.x) > 1.0f)
                    return posA.x.CompareTo(posB.x);
                
                if (Mathf.Abs(posA.z - posB.z) > 0.5f)
                    return posA.z.CompareTo(posB.z);
                
                return posA.y.CompareTo(posB.y);
            });
            
            // Rename each boot
            for (int i = 0; i < boots.Count; i++)
            {
                string newName = $"boot_{i + 1}";
                Vector3 pos = boots[i].transform.position;
                
                Log?.LogInfo($"[RENAME] Renaming '{boots[i].gameObject.name}' to '{newName}' at ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
                boots[i].gameObject.name = newName;
            }
            
            Log?.LogInfo($"[RENAME] Renamed {boots.Count} boots");
        }
        
        /// <summary>
        /// Find all topsoil bags in the scene and rename them to top_1, top_2, etc.
        /// </summary>
        public void RenameTopsoil()
        {
            Log?.LogInfo("[RENAME] Looking for topsoil bags to rename...");
            
            var allProps = UnityEngine.Object.FindObjectsOfType<Prop>();
            var topsoil = new List<Prop>();
            
            foreach (var prop in allProps)
            {
                if (prop == null || prop.gameObject == null) continue;
                
                string name = prop.gameObject.name.ToLower().Trim();
                
                // Only rename topsoil that is exactly "top" (not already renamed)
                if (name == "top")
                {
                    topsoil.Add(prop);
                }
            }
            
            if (topsoil.Count == 0)
            {
                Log?.LogInfo("[RENAME] No topsoil bags found to rename");
                return;
            }
            
            // Sort by position for consistent ordering (x, then z, then y)
            topsoil.Sort((a, b) => {
                Vector3 posA = a.transform.position;
                Vector3 posB = b.transform.position;
                
                if (Mathf.Abs(posA.x - posB.x) > 1.0f)
                    return posA.x.CompareTo(posB.x);
                
                if (Mathf.Abs(posA.z - posB.z) > 0.5f)
                    return posA.z.CompareTo(posB.z);
                
                return posA.y.CompareTo(posB.y);
            });
            
            // Rename each topsoil bag
            for (int i = 0; i < topsoil.Count; i++)
            {
                string newName = $"top_{i + 1}";
                Vector3 pos = topsoil[i].transform.position;
                
                Log?.LogInfo($"[RENAME] Renaming '{topsoil[i].gameObject.name}' to '{newName}' at ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
                topsoil[i].gameObject.name = newName;
            }
            
            Log?.LogInfo($"[RENAME] Renamed {topsoil.Count} topsoil bags");
        }
        
        private void CacheItem(string itemType, Prop prop)
        {
            if (!cachedItems.ContainsKey(itemType))
            {
                cachedItems[itemType] = new List<CachedItem>();
            }
            
            Vector3 pos = prop.transform.position;
            int index = cachedItems[itemType].Count + 1; // 1-indexed
            int instanceId = prop.gameObject.GetInstanceID();
            
            cachedItems[itemType].Add(new CachedItem(pos, index, instanceId));
        }
        
        /// <summary>
        /// Try to identify which specific item this is based on its position or instance ID.
        /// Returns the index (1-based) or 0 if not found.
        /// </summary>
        public int IdentifyItem(string itemName, Vector3 currentPosition, int instanceId)
        {
            string itemType = GetItemType(itemName);
            if (itemType == null || !cachedItems.ContainsKey(itemType))
            {
                Log?.LogInfo($"[POSITION] Item type '{itemName}' not tracked");
                return 0;
            }
            
            var items = cachedItems[itemType];
            
            // First try to match by instance ID (most accurate)
            foreach (var item in items)
            {
                if (item.InstanceId == instanceId && !item.Collected)
                {
                    Log?.LogInfo($"[POSITION] Matched {itemType} #{item.Index} by InstanceID {instanceId}");
                    item.Collected = true;
                    return item.Index;
                }
            }
            
            // Fall back to position matching
            float closestDist = float.MaxValue;
            CachedItem closestItem = null;
            
            foreach (var item in items)
            {
                if (item.Collected) continue;
                
                float dist = Vector3.Distance(item.OriginalPosition, currentPosition);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestItem = item;
                }
            }
            
            if (closestItem != null && closestDist < MATCH_DISTANCE)
            {
                Log?.LogInfo($"[POSITION] Matched {itemType} #{closestItem.Index} by position (dist: {closestDist:F2})");
                closestItem.Collected = true;
                return closestItem.Index;
            }
            
            Log?.LogInfo($"[POSITION] Could not match {itemType} at position ({currentPosition.x:F2}, {currentPosition.y:F2}, {currentPosition.z:F2})");
            return 0;
        }
        
        /// <summary>
        /// Check if we're tracking this item type
        /// </summary>
        public bool IsTrackedItem(string itemName)
        {
            return GetItemType(itemName) != null;
        }
        
        private string GetItemType(string itemName)
        {
            string lower = itemName.ToLower();
            
            // Check tracked types
            foreach (string trackedType in TrackedItemTypes)
            {
                if (lower.Contains(trackedType))
                {
                    return trackedType;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Reset tracking (call on area reset or when re-entering area)
        /// </summary>
        public void Reset()
        {
            cachedItems.Clear();
            Log?.LogInfo("[POSITION] Position cache cleared");
        }
        
        /// <summary>
        /// Get how many of an item type we've cached
        /// </summary>
        public int GetCachedCount(string itemType)
        {
            if (cachedItems.ContainsKey(itemType.ToLower()))
            {
                return cachedItems[itemType.ToLower()].Count;
            }
            return 0;
        }
    }
}