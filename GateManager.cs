using System;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

namespace GooseGameAP
{
    /// <summary>
    /// Manages gate states, area access, and teleportation
    /// </summary>
    public class GateManager
    {
        private static ManualLogSource Log => Plugin.Log;
        private Plugin plugin;
        
        public static readonly Vector3 WellPosition = new Vector3(1.0f, 1.5f, -1.5f);
        public static readonly Vector3 StartPosition = new Vector3(-38.2f, 1.0f, -21.5f);     // On the tree stump
        
        // Area warp positions (approximate entry points)
        public static readonly Vector3 GardenPosition = new Vector3(-16.3f, 0.5f, -17.5f);     // Near garden entrance (OOB)
        public static readonly Vector3 HighStreetPosition = new Vector3(13.9f, 0.9f, -17.7f);  // High street area (OOB)
        public static readonly Vector3 BackGardensPosition = new Vector3(18.7f, 1.4f, 14.0f); // Back gardens (Currently coming out at the right edge of high street near the walky talkies.)
        public static readonly Vector3 PubPosition = new Vector3(-15.9f, 1.2f, 1.6f); // Back area of the Pub right near the Pub tomatoes. 
        public static readonly Vector3 ModelVillagePosition = new Vector3(-49.3f, 0.5f, 2.1f); // Model village (actually in the village at the high street part of it.)
        
        // Track if finale is active - gates should close
        public bool FinaleActive { get; private set; } = false;
        
        private static readonly Dictionary<string, string[]> AreaGates = new Dictionary<string, string[]>
        {
            { "HighStreet", new[] { 
                // The tall gate the groundskeeper hammers - this is fine to open
                "gardenDynamic/GROUP_Hammering/gateTall/gateTallOpenSystem",
                "overworldStatic/GROUP_Hub/HallToHubGateSystem/HallToHubGateMainSystem",
                "overworldStatic/GROUP_Hub/HallToHubGateSystem/HallToHubGateLockSystem"
            }},
            { "Backyards", new[] {
                "highStreetDynamic/GROUP_Garage/irongate/GateSystem"
            }},
            { "Pub", new[] {
                "pubDynamic/GROUP_pubItems/PubGateSystem",
                "overworldStatic/GROUP_BackyardToPub/SluiceGateSystem",
                "overworldStatic/GROUP_Hub/PubToHubGateSystem"
            }},
            { "Finale", new[] {
                "pubDynamic/GROUP_BucketOnHead/binSkip_openable/switchSystem",  // The bin lid that makes the ramp
                "pubDynamic/GROUP_BucketOnHead/PubToFinaleGateSystem",
                "pubDynamic/GROUP_BucketOnHead/PubToFinaleGateSystem/gate",
                "pubDynamic/GROUP_BucketOnHead/PubToFinaleGateSystem/gate/gateMetal",
                "overworldStatic/GROUP_ParkToPub/FinaleToParkGateSystem"
            }}
            // NOTE: Garden not listed - we only disable colliders, don't open the gate
            // This preserves the "Lock the Groundskeeper out" checklist item
        };
        
        // Gates to close when finale starts (hub connections - all except pub)
        // We'll find these dynamically instead of hardcoding paths
        
        public GateManager(Plugin plugin)
        {
            this.plugin = plugin;
        }
        
        /// <summary>
        /// Called when the finale starts (bell grabbed from sandcastle)
        /// </summary>
        public void OnFinaleStart()
        {
            if (FinaleActive) return;
            FinaleActive = true;
            Log.LogInfo("[FINALE] Finale started - closing hub gates!");
            
            // Close all hub gates except pub
            CloseHubGatesForFinale();
        }
        
        /// <summary>
        /// Reset finale state (when game restarts)
        /// </summary>
        public void ResetFinale()
        {
            FinaleActive = false;
        }
        
        private void CloseHubGatesForFinale()
        {
            Log.LogInfo("[FINALE] Closing hub gates for finale...");
            
            var allSwitches = UnityEngine.Object.FindObjectsOfType<SwitchSystem>();
            
            foreach (var sw in allSwitches)
            {
                if (sw?.gameObject == null) continue;
                
                string path = GetGameObjectPath(sw.gameObject);
                string lowerPath = path.ToLower();
                
                // Skip pub gates - those stay open
                if (lowerPath.Contains("pub")) continue;
                
                // Handle hub gates
                if (lowerPath.Contains("hub") && lowerPath.Contains("gate"))
                {
                    Log.LogInfo($"[FINALE] Closing gate: {path}");
                    
                    // Lock systems - activate them to lock the gate
                    if (lowerPath.Contains("lock"))
                    {
                        sw.SetState(1, null);
                    }
                    // Main gate systems - close them
                    else if (lowerPath.Contains("main"))
                    {
                        // Re-enable auto-closer if it exists
                        Transform autoCloser = sw.transform.Find("autoCloser");
                        if (autoCloser != null)
                        {
                            autoCloser.gameObject.SetActive(true);
                        }
                        
                        sw.SetState(0, null);
                    }
                }
            }
            
            // Re-enable hub gate blockers (except pub)
            ReenableHubBlockers();
        }
        
        private void ReenableHubBlockers()
        {
            Log.LogInfo("[FINALE] Re-enabling hub blockers...");
            
            // Re-enable ALL the colliders/blockers we disabled for Hub area access
            // These block the gaps around the gates
            // Must match what DisableAreaBlockers("Hub") disables, EXCEPT pub
            string[] hubBlockerPaths = new[]
            {
                // From DisableAreaBlockers("Hub") - except pub paths
                "highStreetDynamic/GROUP_Garage/irongate/GateSystem/GateExtraColliders/AlleyHubGateExtraCollider",
                "highStreetDynamic/GROUP_Garage/irongate/GateSystem/GateExtraColliders/ParkHubGateExtraCollider",
                "highStreetDynamic/GROUP_Garage/irongate/GateSystem/GateExtraColliders",
                "overworldStatic/GROUP_Hub/HallToHubGateSystem/gateFrame/colllidersNegScalingFlipped",
                "overworldStatic/GROUP_Hub/HallToHubGateSystem/gateFrame",
                "overworldStatic/GROUP_Hub/HubGateSystem/HubGateMainSystem/gateFrame",
                // Note: NOT re-enabling PubToHubGateSystem - that stays open for the chase
                
                // Also re-enable the invisible walls for area transitions (except pub)
                "gardenDynamic/GROUP_Hammering/InvisibleWall",
                "gardenDynamic/GROUP_Hammering/gateTall/GateExtraColliders",
                "highStreetDynamic/GROUP_Garage/InvisibleWall",
            };
            
            foreach (string path in hubBlockerPaths)
            {
                EnableObjectByPath(path);
            }
        }
        
        private void EnableObjectByPath(string path)
        {
            var obj = GameObject.Find(path);
            if (obj != null)
            {
                obj.SetActive(true);
                
                // Re-enable all colliders on this object and children
                var colliders = obj.GetComponentsInChildren<Collider>(true);
                foreach (var col in colliders)
                {
                    col.enabled = true;
                }
                Log.LogInfo($"[FINALE] Enabled blocker: {path}");
            }
            else
            {
                // Object might be disabled, try to find it through parent
                string parentPath = path.Substring(0, path.LastIndexOf('/'));
                string childName = path.Substring(path.LastIndexOf('/') + 1);
                
                var parentObj = GameObject.Find(parentPath);
                if (parentObj != null)
                {
                    Transform child = parentObj.transform.Find(childName);
                    if (child != null)
                    {
                        child.gameObject.SetActive(true);
                        var colliders = child.GetComponentsInChildren<Collider>(true);
                        foreach (var col in colliders)
                        {
                            col.enabled = true;
                        }
                        Log.LogInfo($"[FINALE] Enabled blocker via parent: {path}");
                    }
                }
            }
        }
        
        public void SyncGatesFromAccessFlags()
        {
            // Don't reopen gates during finale
            if (FinaleActive)
            {
                Log.LogInfo("[GATE] Skipping gate sync - finale is active");
                return;
            }
            
            // Clear hub blockers first - hub should always be walkable
            DisableAreaBlockers("Hub");
            
            if (plugin.HasGardenAccess)
                OpenGatesForArea("Garden");
            if (plugin.HasHighStreetAccess)
                OpenGatesForArea("HighStreet");
            if (plugin.HasBackGardensAccess)
                OpenGatesForArea("Backyards");
            if (plugin.HasPubAccess)
                OpenGatesForArea("Pub");
            if (plugin.HasModelVillageAccess)
                OpenGatesForArea("Finale");
            
            // Ensure hub paths are open
            DisableHubBlocker();
        }
        
        private void DisableHubBlocker()
        {
            var parkHubBlocker = GameObject.Find("highStreetDynamic/GROUP_Garage/irongate/GateSystem/GateExtraColliders/ParkHubGateExtraCollider");
            if (parkHubBlocker != null)
            {
                parkHubBlocker.SetActive(false);
            }
        }
        
        public void OpenGatesForArea(string areaName)
        {
            if (!AreaGates.ContainsKey(areaName))
            {
                Log.LogWarning("No gates defined for area: " + areaName);
                return;
            }

            
            // Trigger relevant switch events
            string[] eventsToTry = GetEventsForArea(areaName);
            if (eventsToTry != null)
            {
                foreach (var evt in eventsToTry)
                {
                    try 
                    { 
                        SwitchEventManager.TriggerEvent(evt);
                    } 
                    catch { }
                }
            }
            
            // Disable area-specific invisible blockers
            DisableAreaBlockers(areaName);

            // Directly manipulate gate objects (if any are defined for this area)
            if (AreaGates.ContainsKey(areaName))
            {
                foreach (string gatePath in AreaGates[areaName])
                {
                    ProcessGate(gatePath, areaName);
                }
            }
        }
        
        private string[] GetEventsForArea(string areaName)
        {
            // Only trigger unlock and open events, NOT goal completion events
            // Goal events (goalHammering, goalGarage, etc) would mark goals as complete
            // and change NPC behaviors unexpectedly
            switch (areaName)
            {
                case "Garden": return null;  // No events - only disable colliders, don't touch the gate
                case "HighStreet": return new[] { "unlockHighStreet", "openHighStreet" };
                case "Backyards": return new[] { "unlockBackyards", "openBackyards" };
                case "Pub": return new[] { "unlockPub", "openPub" };
                case "Finale": return new[] { "unlockFinale", "openFinale" };
                default: return null;
            }
        }
        
        private void ProcessGate(string gatePath, string areaName)
        {
            GameObject gateObj = GameObject.Find(gatePath);
            if (gateObj == null)
            {
                // Try finding by partial path
                var allSwitches = UnityEngine.Object.FindObjectsOfType<SwitchSystem>();
                foreach (var sw in allSwitches)
                {
                    string path = GetGameObjectPath(sw.gameObject);
                    if (path.EndsWith(gatePath) || path == gatePath)
                    {
                        gateObj = sw.gameObject;
                        break;
                    }
                }
            }
            
            if (gateObj == null)
            {
                Log.LogWarning("  Could not find gate: " + gatePath);
                return;
            }
            
            // Set switch state
            var switchSystem = gateObj.GetComponent<SwitchSystem>();
            if (switchSystem != null)
            {
                // Disable auto-closer first
                Transform autoCloser = switchSystem.transform.Find("autoCloser");
                if (autoCloser != null)
                {
                    autoCloser.gameObject.SetActive(false);
                }
                
                switchSystem.SetState(1, null);
            }
            
            // Disable colliders
            DisableGateColliders(gateObj, areaName);
        }
        
        private void DisableGateColliders(GameObject gateObj, string areaName)
        {
            // Disable extra colliders
            Transform extraColliders = gateObj.transform.Find("GateExtraColliders");
            if (extraColliders != null)
            {
                foreach (Transform child in extraColliders)
                {
                    // Don't disable hub path blockers
                    if (!child.name.Contains("Hub"))
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }
            
            // Disable wall colliders
            string[] wallNames = { "InvisibleWall", "invisibleWall", "Wall", "Blocker", "blocker" };
            foreach (string wallName in wallNames)
            {
                Transform wall = gateObj.transform.Find(wallName);
                if (wall != null)
                {
                    wall.gameObject.SetActive(false);
                }
            }
            
            // Recursively disable colliders on children named with "collider" or "blocker"
            DisableCollidersRecursive(gateObj.transform);
        }
        
        private void DisableCollidersRecursive(Transform parent)
        {
            foreach (Transform child in parent)
            {
                string lowerName = child.name.ToLower();
                if (lowerName.Contains("collider") || lowerName.Contains("blocker") || lowerName.Contains("invisible"))
                {
                    if (!lowerName.Contains("hub")) // Keep hub paths
                    {
                        child.gameObject.SetActive(false);
                    }
                }
                DisableCollidersRecursive(child);
            }
        }
        
        private void DisableAreaBlockers(string areaName)
        {
            switch (areaName)
            {
                case "Hub":
                    // The actual hub blockers from game scans
                    DisableObjectByPath("highStreetDynamic/GROUP_Garage/irongate/GateSystem/GateExtraColliders/AlleyHubGateExtraCollider");
                    DisableObjectByPath("highStreetDynamic/GROUP_Garage/irongate/GateSystem/GateExtraColliders/ParkHubGateExtraCollider");
                    DisableObjectByPath("highStreetDynamic/GROUP_Garage/irongate/GateSystem/GateExtraColliders");
                    DisableObjectByPath("overworldStatic/GROUP_Hub/HallToHubGateSystem/gateFrame/colllidersNegScalingFlipped");
                    DisableObjectByPath("overworldStatic/GROUP_Hub/HallToHubGateSystem/gateFrame");
                    DisableObjectByPath("overworldStatic/GROUP_Hub/HubGateSystem/HubGateMainSystem/gateFrame");
                    DisableObjectByPath("overworldStatic/GROUP_Hub/PubToHubGateSystem/gateFrame");
                    break;
                case "Garden":
                    // No blockers disabled - GardenGate has vanilla behavior
                    // Groundskeeper unlocks it with keys, player can close it to lock him out
                    break;
                case "HighStreet":
                    DisableObjectByPath("gardenDynamic/GROUP_Hammering/InvisibleWall");
                    DisableObjectByPath("gardenDynamic/GROUP_Hammering/gateTall/GateExtraColliders");
                    break;
                case "Backyards":
                    DisableObjectByPath("highStreetDynamic/GROUP_Garage/InvisibleWall");
                    DisableObjectByPath("highStreetDynamic/GROUP_Garage/irongate/GateSystem/GateExtraColliders");
                    break;
                case "Pub":
                    DisableObjectByPath("overworldStatic/GROUP_BackyardToPub/InvisibleWall");
                    DisableObjectByPath("overworldStatic/GROUP_BackyardToPub/SluiceGateSystem/InvisibleWall");
                    DisableObjectByPath("pubDynamic/GROUP_pubItems/PubGateSystem/GateExtraColliders");
                    break;
                case "Finale":
                    DisableObjectByPath("pubDynamic/GROUP_BucketOnHead/InvisibleWall");
                    DisableObjectByPath("pubDynamic/GROUP_BucketOnHead/PubToFinaleGateSystem/GateExtraColliders");
                    break;
            }
        }
        
        private void DisableObjectByPath(string path)
        {
            var obj = GameObject.Find(path);
            if (obj != null)
            {
                // Disable all colliders on this object and children
                var colliders = obj.GetComponentsInChildren<Collider>();
                foreach (var col in colliders)
                {
                    col.enabled = false;
                }
                obj.SetActive(false);
            }
        }
        
        public void ClearHubBlockers()
        {
            DisableAreaBlockers("Hub");
            
            // Also set all hub gate SwitchSystems to open
            var allSwitches = UnityEngine.Object.FindObjectsOfType<SwitchSystem>();
            foreach (var sw in allSwitches)
            {
                if (sw == null) continue;
                string path = GetFullPath(sw.gameObject);
                
                if (path.Contains("GROUP_Hub") && path.Contains("Gate"))
                {
                    if (sw.currentState == 0)
                    {
                        sw.SetState(1, null);
                    }
                }
            }
        }
        
        private string GetFullPath(GameObject obj)
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
        
        public void TeleportGooseToWell()
        {
            TeleportGoose(WellPosition);
        }
        
        public void TeleportGooseToStart()
        {
            TeleportGoose(StartPosition);
        }
        
        public void TeleportGoose(Vector3 position)
        {
            try
            {
                if (GameManager.instance != null && GameManager.instance.allGeese != null)
                {
                    foreach (var goose in GameManager.instance.allGeese)
                    {
                        if (goose != null && goose.isActiveAndEnabled)
                        {
                            // Stop movement
                            if (goose.mover != null)
                            {
                                goose.mover.currentSpeed = 0f;
                            }
                            
                            // Stop physics
                            var rb = goose.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                rb.velocity = Vector3.zero;
                                rb.angularVelocity = Vector3.zero;
                            }
                            
                            goose.transform.position = position;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError("TeleportGoose error: " + ex.Message);
            }
        }
        
        public void TriggerAreaUnlock(string areaName)
        {
            string eventName = "unlock" + areaName;
            try
            {
                SwitchEventManager.TriggerEvent(eventName);
            }
            catch (Exception ex)
            {
                Log.LogWarning("Failed to trigger event " + eventName + ": " + ex.Message);
            }
        }
        
        public void OpenAllGates()
        {
            foreach (var area in AreaGates.Keys)
            {
                OpenGatesForArea(area);
            }
            DisableHubBlocker();
            plugin.UI.ShowNotification("All gates opened!");
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
    }
}