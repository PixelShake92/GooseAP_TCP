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
                "pubDynamic/GROUP_BucketOnHead/PubToFinaleGateSystem",
                "pubDynamic/GROUP_BucketOnHead/PubToFinaleGateSystem/gate",
                "pubDynamic/GROUP_BucketOnHead/PubToFinaleGateSystem/gate/gateMetal",
                "overworldStatic/GROUP_ParkToPub/FinaleToParkGateSystem"
            }}
            // NOTE: Garden not listed - we only disable colliders, don't open the gate
            // This preserves the "Lock the Groundskeeper out" checklist item
        };
        
        public GateManager(Plugin plugin)
        {
            this.plugin = plugin;
        }
        
        public void SyncGatesFromAccessFlags()
        {
            Log.LogInfo("=== SYNCING GATES FROM ACCESS FLAGS ===");
            Log.LogInfo("  Garden: " + plugin.HasGardenAccess);
            Log.LogInfo("  High Street: " + plugin.HasHighStreetAccess);
            Log.LogInfo("  Back Gardens: " + plugin.HasBackGardensAccess);
            Log.LogInfo("  Pub: " + plugin.HasPubAccess);
            Log.LogInfo("  Model Village: " + plugin.HasModelVillageAccess);
            
            // Clear hub blockers first - hub should always be walkable
            Log.LogInfo("  Clearing Hub blockers...");
            DisableAreaBlockers("Hub");
            
            if (plugin.HasGardenAccess)
            {
                OpenGatesForArea("Garden");
                Log.LogInfo("  Opened Garden gates");
            }
            if (plugin.HasHighStreetAccess)
            {
                OpenGatesForArea("HighStreet");
                Log.LogInfo("  Opened High Street gates");
            }
            if (plugin.HasBackGardensAccess)
            {
                OpenGatesForArea("Backyards");
                Log.LogInfo("  Opened Back Gardens gates");
            }
            if (plugin.HasPubAccess)
            {
                OpenGatesForArea("Pub");
                Log.LogInfo("  Opened Pub gates");
            }
            if (plugin.HasModelVillageAccess)
            {
                OpenGatesForArea("Finale");
                Log.LogInfo("  Opened Model Village gates");
            }
            
            // Ensure hub paths are open
            DisableHubBlocker();
            
            plugin.UI.ShowNotification("Gates synced from server!");
            Log.LogInfo("=== GATE SYNC COMPLETE ===");
        }
        
        private void DisableHubBlocker()
        {
            var parkHubBlocker = GameObject.Find("highStreetDynamic/GROUP_Garage/irongate/GateSystem/GateExtraColliders/ParkHubGateExtraCollider");
            if (parkHubBlocker != null)
            {
                parkHubBlocker.SetActive(false);
                Log.LogInfo("  Disabled ParkHubGateExtraCollider");
            }
        }
        
        public void OpenGatesForArea(string areaName)
        {
            if (!AreaGates.ContainsKey(areaName))
            {
                Log.LogWarning("No gates defined for area: " + areaName);
                return;
            }

            Log.LogInfo("Opening gates for area: " + areaName);
            
            // Trigger relevant switch events
            string[] eventsToTry = GetEventsForArea(areaName);
            if (eventsToTry != null)
            {
                foreach (var evt in eventsToTry)
                {
                    try 
                    { 
                        SwitchEventManager.TriggerEvent(evt);
                        Log.LogInfo("  Triggered event: " + evt);
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
                Log.LogInfo("  Set switch state to 1 for: " + gatePath);
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
                Log.LogInfo($"  Disabled: {path} ({colliders.Length} colliders)");
            }
        }
        
        public void ClearHubBlockers()
        {
            Log.LogInfo("=== CLEARING HUB BLOCKERS ===");
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
                        Log.LogInfo($"  Opened hub gate: {path}");
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
            Log.LogInfo("Teleported goose to hub at: " + WellPosition);
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
                            Log.LogInfo("Teleported goose to: " + position);
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
                Log.LogInfo("Triggered area unlock event: " + eventName);
            }
            catch (Exception ex)
            {
                Log.LogWarning("Failed to trigger event " + eventName + ": " + ex.Message);
            }
        }
        
        public void OpenAllGates()
        {
            Log.LogInfo("=== OPENING ALL GATES ===");
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