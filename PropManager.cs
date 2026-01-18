using System;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

namespace GooseGameAP
{
    /// <summary>
    /// Manages prop visibility based on prop soul items.
    /// Props are disabled until their corresponding soul is received.
    /// When PropSoulsEnabled is false, all props are always accessible.
    /// </summary>
    public class PropManager
    {
        private static ManualLogSource Log => Plugin.Log;
        private Plugin plugin;
        
        // Track which souls we have received
        private HashSet<string> receivedSouls = new HashSet<string>();
        
        // Cache of props by soul type
        private Dictionary<string, List<GameObject>> propCache = new Dictionary<string, List<GameObject>>();

        private Dictionary<string, List<Prop>> propCacheUsingProps = new Dictionary<string, List<Prop>>();
        
        private bool hasScannedProps = false;
        
        // Props that are ignored when connecting prop names to souls in the dictionary
        private static readonly List<string> IgnoredProps = new List<string>
        {
            "dart",
            "tomatobox",
            "roseboxprop",
        };
        
        // Prop names that have to be a perfect match in the PropToSoul dictionary so that they aren't erroneously attached to the wrong souls
        private static readonly List<string> PerfectMatchOnlyProps = new List<string>
        {
            "minishovelprop",
        };
        
        // Map item name patterns to soul names
        private static readonly Dictionary<string, string> PropToSoul = new Dictionary<string, string>
        {
            // Grouped souls - items with multiple instances
            { "carrot", "Carrot Soul" },
            { "tomato", "Tomato Soul" },
            { "pumpkin", "Pumpkin Soul" },
            { "topsoilbag", "Topsoil Bag Soul" },
            { "topsoil", "Topsoil Bag Soul" },
            { "fertiliser", "Topsoil Bag Soul" },
            { "fertliser", "Topsoil Bag Soul" },  // Typo variant
            { "fertilizer", "Topsoil Bag Soul" },
            { "quoit", "Quoit Soul" },
            { "plate", "Plate Soul" },
            { "orange", "Orange Soul" },
            { "leek", "Leek Soul" },
            { "cucumber", "Cucumber Soul" },
            { "umbrella", "Umbrella Soul" },
            { "bluecan", "Tinned Food Soul" },
            { "orangecan", "Tinned Food Soul" },
            { "yellowcan", "Tinned Food Soul" },
            { "canblue", "Tinned Food Soul" },
            { "canorange", "Tinned Food Soul" },
            { "canyellow", "Tinned Food Soul" },
            { "sock", "Sock Soul" },
            { "pintbottle", "Pint Bottle Soul" },
            { "knife", "Knife Soul" },
            { "gumboot", "Gumboot Soul" },
            { "fork", "Fork Soul" },
            { "applecore", "Apple Core Soul" },
            { "apple", "Apple Soul" },
            { "sandwich", "Sandwich Soul" },
            { "bow", "Bow Soul" },
            { "walkietalkie", "Walkie Talkie Soul" },
            { "boot", "Boot Soul" },
            { "miniperson", "Mini Person Soul" },
            
            // Garden one-offs
            { "radio", "Radio Soul" },
            { "radiosmall", "Radio Soul" },
            { "trowel", "Trowel Soul" },
            { "tulip", "Tulip Soul" },
            { "jam", "Jam Soul" },
            { "picnicmug", "Picnic Mug Soul" },
            { "thermos", "Thermos Soul" },
            { "strawhat", "Straw Hat Soul" },
            { "sunhat", "Straw Hat Soul" },
            { "drinkcan", "Drink Can Soul" },
            { "tennisball", "Tennis Ball Soul" },
            { "rake", "Rake Soul" },
            { "basket", "Picnic Basket Soul" },  // Short name from hierarchy
            { "esky", "Esky Soul" },
            { "coolbox", "Esky Soul" },
            { "shovel", "Shovel Soul" },
            { "wateringcan", "Watering Can Soul" },
            { "mallet", "Mallet Soul" },
            { "woodencrate", "Wooden Crate Soul" },
            { "cratewooden", "Wooden Crate Soul" },
            
            // High Street one-offs
            { "hornrimmedglasses", "Horn-Rimmed Glasses Soul" },
            { "redglasses", "Red Glasses Soul" },
            { "sunglasses", "Sunglasses Soul" },
            { "toiletpaper", "Toilet Paper Soul" },
            { "toycar", "Toy Car Soul" },
            { "hairbrush", "Hairbrush Soul" },
            { "toothbrush", "Toothbrush Soul" },
            { "stereoscope", "Stereoscope Soul" },
            { "dishsoapbottle", "Dish Soap Bottle Soul" },
            { "dishwashbottle", "Dish Soap Bottle Soul" },
            { "spraybottle", "Spray Bottle Soul" },
            { "weedtool", "Weed Tool Soul" },
            { "forkgarden", "Weed Tool Soul" },
            { "lilyflower", "Lily Flower Soul" },
            { "fusilage", "Fusilage Soul" },
            { "coin", "Coin" },
            { "chalk", "Chalk Soul" },
            { "dustbinlid", "Dustbin Lid Soul" },
            { "basketprop", "Shopping Basket Soul" },
            { "top", "Topsoil Bag Soul" },  // Short name after cleaning top_1, top_2, top_3
            { "pushbroom", "Push Broom Soul" },
            { "dustbin", "Dustbin Soul" },
            { "babydoll", "Baby Doll Soul" },
            { "pricinggun", "Pricing Gun Soul" },
            { "addingmachine", "Adding Machine Soul" },
            
            // Back Gardens one-offs
            { "dummy", "Dummy Soul" },
            { "cricketball", "Cricket Ball Soul" },
            { "bustpipe", "Bust Pipe Soul" },
            { "busthat", "Bust Hat Soul" },
            { "bustglasses", "Bust Glasses Soul" },
            { "teacup", "Tea Cup Soul" },
            { "newspaper", "Newspaper Soul" },
            { "badmintonracket", "Badminton Racket Soul" },
            { "potstack", "Pot Stack Soul" },
            { "soap", "Soap Soul" },
            { "paintbrush", "Paintbrush Soul" },
            { "vase", "Vase Soul" },
            { "rightstrap", "Bra Soul" },
            { "rose", "Rose Soul" },
            // Removing Rose Box Soul until I can solve the physics issues with it
            // { "roseboxprop", "Rose Box Soul" },
            { "cricketbat", "Cricket Bat Soul" },
            { "teapot", "Tea Pot Soul" },
            { "clippers", "Clippers Soul" },
            { "duckstatue", "Duck Statue Soul" },
            { "duckstatueprop", "Duck Statue Soul" },  // Full prop name
            { "duck", "Duck Statue Soul" },
            { "frogstatue", "Frog Statue Soul" },
            { "frog", "Frog Statue Soul" },
            { "jeremyfish", "Jeremy Fish Soul" },
            { "messysign", "Messy Sign Soul" },
            { "drawer", "Drawer Soul" },
            { "enameljug", "Enamel Jug Soul" },
            { "jugenamel", "Enamel Jug Soul" },
            { "cleansign", "Clean Sign Soul" },
            
            // Pub one-offs
            { "fishingbobber", "Fishing Bobber Soul" },
            { "exitletter", "Exit Letter Soul" },
            { "pintglass", "Pint Glass Soul" },
            { "toyboat", "Toy Boat Soul" },
            { "peppergrinder", "Pepper Grinder Soul" },
            { "cork", "Cork Soul" },
            { "candlestick", "Candlestick Soul" },
            { "flowerforvase", "Flower for Vase Soul" },
            { "harmonica", "Harmonica Soul" },
            { "tacklebox", "Tackle Box Soul" },
            { "trafficcone", "Traffic Cone Soul" },
            { "coneprop", "Traffic Cone Soul" },
            { "exitparcel", "Exit Parcel Soul" },
            { "stealthbox", "Stealth Box Soul" },
            { "nogoosesign", "No Goose Sign Soul" },
            { "pubnogoose", "No Goose Sign Soul" },
            { "portablestool", "Portable Stool Soul" },
            { "dartboard", "Dartboard Soul" },
            { "mopbucket", "Mop Bucket Soul" },
            { "pail", "Burly Mans Bucket Soul" },
            { "bucket", "Mop Bucket Soul" },
            { "mop", "Mop Soul" },
            { "mophandle", "Mop Soul" },
            { "mophead", "Mop Soul" },
            { "deliverybox", "Delivery Box Soul" },
            
            // Model Village one-offs
            { "minimailpillar", "Mini Mail Pillar Soul" },
            { "miniphonedoor", "Mini Phone Door Soul" },
            { "minishovelprop", "Mini Shovel Soul" },
            { "poppyflower", "Poppy Flower Soul" },
            { "flowerpoppy", "Poppy Flower Soul" },
            { "timberhandle", "Timber Handle Soul" },
            { "birdbath", "Birdbath Soul" },
            { "easel", "Easel Soul" },
            { "minibench", "Mini Bench Soul" },
            { "minipump", "Mini Pump Soul" },
            { "ministreetbench", "Mini Bench Soul" },
            { "streetbench", "Mini Bench Soul" },
            { "benchstreet", "Mini Bench Soul" },
            { "sunlounge", "Sun Lounge Soul" },
            
            // Victory item
            { "goldenbell", "Golden Bell Soul" },
        };
        
        public PropManager(Plugin plugin)
        {
            this.plugin = plugin;
        }
        
        /// <summary>
        /// Check if prop souls are enabled for this session
        /// </summary>
        private bool PropSoulsEnabled => plugin.PropSoulsEnabled;
        
        // Props that should ALWAYS be enabled (needed for basic progression)
        // Props that must always be visible for game mechanics to work
        // These props are visible but still require their soul to be interacted with
        // (drag blocking handled separately in Patches.cs)
        private static readonly HashSet<string> AlwaysEnabledProps = new HashSet<string>
        {
            // "Drawer Soul"       // Drawer must exist for desk-breaking mechanic to work
                                // Player still needs soul to drag it
        };
        
        /// <summary>
        /// Called every frame from Plugin.Update()
        /// </summary>
        public void Update()
        {
            try
            {
                if (GameManager.instance == null) return;
                if (GameManager.instance.allGeese == null) return;
                if (GameManager.instance.allGeese.Count == 0) return;
                
                if (!hasScannedProps)
                {
                    Log.LogInfo("[Prop] Game loaded - scanning for props");
                    ScanAndCacheProps();
                    
                    // Only disable props if prop souls are enabled
                    if (PropSoulsEnabled)
                    {
                        Log.LogInfo("[Prop] Prop souls ENABLED - disabling props until souls received");
                        DisableAllProps();
                        ApplyAllPropStates();
                    }
                    else
                    {
                        Log.LogInfo("[Prop] Prop souls DISABLED - all props accessible");
                        EnableAllProps();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.LogError($"[Prop] Update error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Disable ALL cached props
        /// </summary>
        private void DisableAllProps()
        {
            int count = 0;
            foreach (var kvp in propCache)
            {
                foreach (var prop in kvp.Value)
                {
                    if (prop != null)
                    {
                        prop.SetActive(false);
                        count++;
                    }
                }
            }
            Log.LogInfo($"[Prop] Disabled {count} props");
        }
        
        /// <summary>
        /// Enable ALL cached props (when souls are disabled)
        /// </summary>
        private void EnableAllProps()
        {
            int count = 0;
            foreach (var kvp in propCache)
            {
                bool shouldEnable = true;
                string soul = kvp.Key;
                if (soul == "goldenbell")
                {
                    if (!receivedSouls.Contains(soul))
                    {
                        shouldEnable = false;
                        Log.LogInfo($"[Prop] Preventing Golden Bell Soul from being enabled as it has not yet been received");
                    }
                    else
                    {
                        Log.LogInfo($"[Prop] Enabling Golden Bell Soul as it has already been received");
                    }
                }

                foreach (var prop in kvp.Value)
                {
                    if (prop != null)
                    {
                        prop.SetActive(shouldEnable);
                        count++;
                    }
                }
            }
            Log.LogInfo($"[Prop] Enabled {count} props (souls disabled)");
        }
        
        /// <summary>
        /// Scan the game world for props and cache them by soul type
        /// </summary>
        private void ScanAndCacheProps()
        {
            if (hasScannedProps) return;
            hasScannedProps = true;
            
            propCache.Clear();
            
            try
            {
                GameObject bra = null;
                GameObject drawer = null;
                GameObject drawer2 = null;
                GameObject drawer3 = null;
                GameObject drawer4 = null;
                GameObject drawer5 = null;
                GameObject picnicBasket1 = null;
                GameObject picnicBasket2 = null;

                var checkAllGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                Log.LogInfo($"[Prop] Found {checkAllGameObjects.Length} GameObjects");
                foreach (var gameObj in checkAllGameObjects)
                {
                    if (gameObj == null) continue;
                    // Log.LogInfo($"[Prop DEBUG] Noting existence of game object with name: '{gameObj.name}'");
                    if (gameObj.name == "braSkinned")
                    {
                        bra = gameObj;
                        Log.LogInfo($"[Prop] Using '{gameObj.name}' for bra");
                    }
                    else if (gameObj.name == "drawer starting home")
                    {
                        drawer = gameObj;
                        Log.LogInfo($"[Prop] Using '{gameObj.name}' for drawer");
                    }
                    else if (gameObj.name == "drawer")
                    {
                        drawer2 = gameObj;
                        Log.LogInfo($"[Prop] Using '{gameObj.name}' for drawer2");
                    }
                    else if (gameObj.name == "postBreakDrawerHome")
                    {
                        drawer3 = gameObj;
                        Log.LogInfo($"[Prop] Using '{gameObj.name}' for drawer3");
                    }
                    else if (gameObj.name == "drawerBreakerTrigger")
                    {
                        drawer4 = gameObj;
                        Log.LogInfo($"[Prop] Using '{gameObj.name}' for drawer4");
                    }
                    else if (gameObj.name == "drawerAssembledHome")
                    {
                        drawer5 = gameObj;
                        Log.LogInfo($"[Prop] Using '{gameObj.name}' for drawer5");
                    }
                    else if (gameObj.name == "basketHandle1")
                    {
                        picnicBasket1 = gameObj;
                        Log.LogInfo($"[Prop] Using '{gameObj.name}' for picnicBasket1");
                    }
                    else if (gameObj.name == "basketHandle2")
                    {
                        picnicBasket2 = gameObj;
                        Log.LogInfo($"[Prop] Using '{gameObj.name}' for picnicBasket2");
                    }
                }

                var allProps = UnityEngine.Object.FindObjectsOfType<Prop>();
                Log.LogInfo($"[Prop] Found {allProps.Length} props total");
                List<Prop> cloneCleanedPropsList = new List<Prop>();
                
                int logCount = 0;
                foreach (var prop in allProps)
                {
                    if (prop == null) continue;
                    if (logCount < 20)//500)
                    {
                        string cleanName = CleanPropName(prop.name);
                        Log.LogInfo($"[Prop DEBUG] Raw: '{prop.name}' -> Clean: '{cleanName}'");
                        logCount++;
                    }

                    // Cleaning duplicate Coins on reload
                    if (prop.name.Contains("(Clone)"))
                    {
                        Log.LogInfo($"[Prop DEBUG] Attempting to destroy: '{prop.name}'");
                        UnityEngine.Object.Destroy(prop);
                        continue;
                    }
                    else
                    {
                        cloneCleanedPropsList.Add(prop);
                    }
                    
                    // Debug logging for problematic props
                    string lowerName = prop.name.ToLower();
                    if (lowerName.Contains("mop") || lowerName.Contains("top") ||
                        lowerName.Contains("duck") || lowerName.Contains("rose") )
                    {
                        string hierarchy = GetHierarchy(prop.transform);
                        Log.LogInfo($"[Prop HIERARCHY] {prop.name}: {hierarchy}");
                    }
                }
                
                // Log.LogInfo($"[Prop DEBUG] Cleaned list made");
                int matched = 0;
                int unmatched = 0;
                
                foreach (var prop in cloneCleanedPropsList)
                {
                    if (prop == null) continue;
                    
                    string cleanName = CleanPropName(prop.name);
                    string soul = GetSoulForProp(cleanName);
                    
                    if (soul != null)
                    {
                        if (!propCache.ContainsKey(soul))
                        {
                            propCache[soul] = new List<GameObject>();
                            propCacheUsingProps[soul] = new List<Prop>();
                        }
                        
                        // Get the appropriate object to disable - check for parent containers
                        GameObject objToCache = GetDisableTarget(prop.gameObject, cleanName);
                        if (objToCache == null)
                        {
                            if (cleanName == "rightstrap")
                            {
                                if (bra != null)
                                {
                                    Log.LogInfo($"[Prop DEBUG] Forcing soul match for: '{cleanName}'. Attached to: '{soul}'. '{prop.name}' added to propCacheUsingProps");
                                    propCache[soul].Add(bra);
                                    propCacheUsingProps[soul].Add(prop);
                                }
                            }
                            else if (cleanName == "drawer")
                            {
                                if (drawer != null && drawer2 != null && drawer3 != null && drawer4 != null && drawer5 != null)
                                {
                                    Log.LogInfo($"[Prop DEBUG] Forcing soul match for: '{cleanName}'. Attached to: '{soul}'. '{prop.name}' added to propCacheUsingProps");
                                    propCache[soul].Add(drawer);
                                    propCache[soul].Add(drawer2);
                                    propCache[soul].Add(drawer3);
                                    propCache[soul].Add(drawer4);
                                    propCache[soul].Add(drawer5);
                                    propCacheUsingProps[soul].Add(prop);
                                }
                            }
                            else if (cleanName == "basket")
                            {
                                if (picnicBasket1 != null && picnicBasket2 != null)
                                {
                                    Log.LogInfo($"[Prop DEBUG] Forcing soul match for: '{cleanName}'. Attached to: '{soul}'. '{prop.name}' added to propCacheUsingProps");
                                    propCache[soul].Add(prop.gameObject);
                                    propCache[soul].Add(picnicBasket1);
                                    propCache[soul].Add(picnicBasket2);
                                    propCacheUsingProps[soul].Add(prop);
                                }
                            }
                            continue;
                        }
                        propCache[soul].Add(objToCache);
                        propCacheUsingProps[soul].Add(prop);
                        matched++;
                        Log.LogInfo($"[Prop] Soul match for: '{objToCache.name}'. Attached to: '{soul}'. '{prop.name}' added to propCacheUsingProps");
                    }
                    else
                    {
                        unmatched++;
                        Log.LogWarning($"[Prop] No soul match for: '{prop.name}' (cleaned: '{cleanName}')");
                    }
                }
                
                Log.LogInfo($"[Prop] Matched {matched} props, unmatched {unmatched}");
                
                foreach (var kvp in propCache)
                {
                    Log.LogInfo($"[Prop] {kvp.Key}: {kvp.Value.Count} props");
                }
                /*foreach (var kvp in propCacheUsingProps)
                {
                    Log.LogInfo($"[Prop DEBUG] (Using props) {kvp.Key}: {kvp.Value.Count} props");
                }*/
            }
            catch (System.Exception ex)
            {
                Log.LogError($"[Prop] ScanAndCacheProps error: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// Get the appropriate GameObject to disable for this prop.
        /// Some props have parent containers that include handles, meshes, etc.
        /// </summary>
        private GameObject GetDisableTarget(GameObject propObj, string cleanName)
        {
            // For certain props, we need to disable a parent container
            // to get all child parts (handles, mesh renderers, physics)
            
            Transform current = propObj.transform;
            
            // Check if parent has a meaningful name that suggests it's the container
            // e.g., "mopProp" might be under "mop" parent with handle as sibling
            if (current.parent != null)
            {
                string parentName = current.parent.name.ToLower();
                
                // Mop - the mopProp is the handle, we need parent for both parts
                if (cleanName.Contains("mop") && !cleanName.Contains("bucket"))
                {
                    // Check if parent has multiple children (mop head + handle)
                    if (current.parent.childCount > 1)
                    {
                        Log.LogInfo($"[Prop] Using parent '{current.parent.name}' for mop (has {current.parent.childCount} children)");
                        return current.parent.gameObject;
                    }
                }
                
                // Picnic basket - BasketProp might have handle as sibling
                if (cleanName.Contains("basket") && parentName.Contains("picnic"))
                {
                    if (current.parent.childCount > 1)
                    {
                        Log.LogInfo($"[Prop] Using parent '{current.parent.name}' for picnic basket");
                        return current.parent.gameObject;
                    }
                }
                
                // Topsoil bags - check if there's a parent with mesh/collider
                if (cleanName.Contains("top") && (parentName.Contains("top") || parentName.Contains("soil") || parentName.Contains("fertil")))
                {
                    Log.LogInfo($"[Prop] Using parent '{current.parent.name}' for topsoil");
                    return current.parent.gameObject;
                }

                // Objects that need to be handled differently (eg, bra). basket is == instead of Contains else it ruins the shopping basket
                if (cleanName.Contains("rightstrap") && parentName == "brahome" || cleanName.Contains("drawer") || cleanName == "basket")
                {
                    return null;
                }
            }
            
            // Default - just use the prop's own GameObject
            return propObj;
        }
        
        private string CleanPropName(string name)
        {
            string clean = name.ToLower()
                .Replace("(clone)", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(" ", "")
                .Replace("_", "")
                .Trim();
            
            while (clean.Length > 0 && char.IsDigit(clean[clean.Length - 1]))
                clean = clean.Substring(0, clean.Length - 1);
            
            return clean;
        }
        
        private string GetHierarchy(Transform t)
        {
            List<string> parts = new List<string>();
            Transform current = t;
            int depth = 0;
            while (current != null && depth < 6)
            {
                parts.Add(current.name + $"[{current.childCount}ch]");
                current = current.parent;
                depth++;
            }
            parts.Reverse();
            return string.Join(" > ", parts);
        }
        
        private string GetSoulForProp(string cleanName)
        {
            // Check if cleanname is in list of props to ignore first, skip if so
            if (IgnoredProps.Contains(cleanName))
                return null;

            // Direct match first
            if (PropToSoul.TryGetValue(cleanName, out string soul))
                return soul;
            
            // Check if cleanName starts with or contains a key
            foreach (var kvp in PropToSoul)
            {
                if (!PerfectMatchOnlyProps.Contains(kvp.Key) && (cleanName.StartsWith(kvp.Key) || cleanName.Contains(kvp.Key)))
                {
                    //Log.LogInfo($"[Prop DEBUG] cleanname '{cleanName}' .StartsWith or .Contains soul: '{kvp.Key}'");
                    return kvp.Value;
                }
            }
            
            // Check if a key starts with or contains cleanName (for short names like "top", "basket")
            foreach (var kvp in PropToSoul)
            {
                if (!PerfectMatchOnlyProps.Contains(kvp.Key) && (kvp.Key.StartsWith(cleanName) || kvp.Key.Contains(cleanName)))
                {
                    //Log.LogInfo($"[Prop DEBUG] soul '{kvp.Key}' .StartsWith or .Contains cleanname: '{cleanName}'");
                    return kvp.Value;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Apply prop states based on received souls
        /// </summary>
        private void ApplyAllPropStates()
        {
            // If souls are disabled, enable everything
            if (!PropSoulsEnabled)
            {
                EnableAllProps();
                return;
            }
            
            int enabledCount = 0;
            foreach (var kvp in propCache)
            {
                string soul = kvp.Key;
                // Always enable props needed for progression, or if we have the soul
                bool shouldEnable = AlwaysEnabledProps.Contains(soul) || receivedSouls.Contains(soul);
                
                foreach (var prop in kvp.Value)
                {
                    if (prop != null)
                    {
                        prop.SetActive(shouldEnable);
                        //Log.LogInfo($"[Prop DEBUG] {prop.name} set active");

                        if (shouldEnable)
                        {
                            enabledCount++;
                        }

                        // Special handling for physics-based props that may need Rigidbody reset
                        ResetPhysicsIfNeeded(prop);
                    }
                }
                if (propCacheUsingProps.TryGetValue(soul, out var actualProps))
                {
                    foreach (var actualProp in actualProps)
                    {
                        if (actualProp != null)
                        {
                            actualProp.ResetPosition();
                            //Log.LogInfo($"[Prop DEBUG] Reset position of: {actualProp.name}");
                        }
                    }
                }
            }
            
            Log.LogInfo($"[Prop] Applied states for {receivedSouls.Count} received souls (+ always-enabled props)");
            Log.LogInfo($"[Prop] Enabled {enabledCount} total props");
        }
        
        /// <summary>
        /// Called when a soul is received
        /// </summary>
        public void ReceiveSoul(string soulName)
        {
            if (receivedSouls.Contains(soulName))
                return;
            
            Log.LogInfo($"[Prop] Received soul: {soulName}");
            receivedSouls.Add(soulName); 

            // Only enable specific props if souls are enabled
            // If souls disabled, props are already enabled
            if (!PropSoulsEnabled)
                return;
            
            if (propCache.TryGetValue(soulName, out var props))
            {
                foreach (var prop in props)
                {
                    if (prop != null)
                    {
                        prop.SetActive(true);

                        // Special handling for physics-based props that may need Rigidbody reset
                        ResetPhysicsIfNeeded(prop);
                        
                        Log.LogInfo($"[Prop] Enabled: {prop.name}");
                    }
                }
                if (propCacheUsingProps.TryGetValue(soulName, out var actualProps))
                {
                    foreach (var actualProp in actualProps)
                    {
                        if (actualProp != null)
                        {
                            actualProp.ResetPosition();
                            Log.LogInfo($"[Prop] Reset position of: {actualProp.name}");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Called when a Coin is received
        /// </summary>
        public void ReceiveCoin()
        {
            // Coins are a special case; as a filler item, we will spawn one every time it's received but only add it to the list once
            bool originalCoin = true;
            if (receivedSouls.Contains("Coin"))
            {
                originalCoin = false;
            }
            else
            {
                receivedSouls.Add("Coin"); 
            }
            
            Log.LogInfo($"[Prop] Received Coin. original: {originalCoin}");

            // If souls disabled, props are already enabled, and thus the original coin is already spawned
            if (!PropSoulsEnabled)
                originalCoin = false;
            
            bool firstCoinFound = false;
            if (propCache.TryGetValue("Coin", out var props))
            {
                foreach (var prop in props)
                {
                    if (prop != null)
                    {
                        if (!firstCoinFound)
                        {
                            if (originalCoin)
                            {
                                prop.SetActive(true);
                                Log.LogInfo($"[Prop] Enabled: {prop.name}");
                            }
                            else
                            {
                                prop.Spawn();
                                Log.LogInfo($"[Prop] Spawned: {prop.name}");
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Reset physics components for props that need it after being enabled
        /// </summary>
        private void ResetPhysicsIfNeeded(GameObject prop)
        {
            // Most props work fine with just SetActive(true)
            // Add special handling here if specific props need physics reset
            
            // Check for Rigidbody and wake it up
            var rb = prop.GetComponent<Rigidbody>();
            if (rb == null) rb = prop.GetComponentInChildren<Rigidbody>();
            
            if (rb != null)
            {
                rb.WakeUp();
                // Log.LogInfo($"[Prop DEBUG] Physics reset for: {prop.name}");
            }
            else
            {
                // Log.LogInfo($"[Prop DEBUG] No physics to reset for: {prop.name}");
            }
        }
        
        /// <summary>
        /// Check if we have a specific soul (or if souls are disabled, or if it's always enabled)
        /// </summary>
        public bool HasSoul(string soulName)
        {
            // Always-enabled props don't need souls for interaction purposes
            if (AlwaysEnabledProps.Contains(soulName))
                return true;
            
            // If souls are disabled, always return true
            if (!PropSoulsEnabled)
                return true;
            
            return receivedSouls.Contains(soulName);
        }
        
        /// <summary>
        /// Check if the player has actually received a specific soul (for tracker display)
        /// Unlike HasSoul, this doesn't return true for AlwaysEnabledProps
        /// </summary>
        public bool HasReceivedSoul(string soulName)
        {
            // If souls are disabled, show as having all souls
            if (!PropSoulsEnabled)
                return true;
            
            return receivedSouls.Contains(soulName);
        }
        
        /// <summary>
        /// Check if dragging a prop should be blocked
        /// This is for "always enabled" props that are visible but can't be dragged without their soul
        /// </summary>
        public bool ShouldBlockDrag(string soulName)
        {
            Log.LogInfo($"[Prop] ShouldBlockDrag called for: {soulName}, PropSoulsEnabled={PropSoulsEnabled}");
            
            // If souls are disabled, never block
            if (!PropSoulsEnabled)
            {
                Log.LogInfo($"[Prop] ShouldBlockDrag: souls disabled, not blocking");
                return false;
            }
            
            // If it's an always-enabled prop, block drag if we don't have the soul
            if (AlwaysEnabledProps.Contains(soulName))
            {
                bool hasSoul = receivedSouls.Contains(soulName);
                Log.LogInfo($"[Prop] ShouldBlockDrag: {soulName} is always-enabled, hasSoul={hasSoul}, blocking={!hasSoul}");
                return !hasSoul;
            }
            
            // For normal props, they're already disabled so no need to block
            Log.LogInfo($"[Prop] ShouldBlockDrag: {soulName} not in AlwaysEnabledProps, not blocking");
            return false;
        }
        
        /// <summary>
        /// Refresh prop states (called when reconnecting or settings change)
        /// </summary>
        public void RefreshPropStates()
        {
            if (!hasScannedProps) return;
            
            Log.LogInfo($"[Prop] RefreshPropStates called (PropSoulsEnabled={PropSoulsEnabled})");
            
            if (PropSoulsEnabled)
            {
                DisableAllProps();
                ApplyAllPropStates();
            }
            else
            {
                EnableAllProps();
            }
        }
        
        /// <summary>
        /// Reset for returning to menu
        /// </summary>
        public void Reset()
        {
            Log.LogInfo("[Prop] Reset called");
            hasScannedProps = false;
            propCache.Clear();
        }
        
        /// <summary>
        /// Clear all received souls (for new slot)
        /// </summary>
        public void ClearAllSouls()
        {
            Log.LogInfo("[Prop] Clearing all souls");
            receivedSouls.Clear();
        }
        
        /// <summary>
        /// Save received souls to PlayerPrefs
        /// </summary>
        public void SaveSouls()
        {
            string soulList = string.Join(",", receivedSouls);
            PlayerPrefs.SetString("AP_PropSouls", soulList);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Load received souls from PlayerPrefs
        /// </summary>
        public void LoadSouls()
        {
            receivedSouls.Clear();
            string soulList = PlayerPrefs.GetString("AP_PropSouls", "");
            if (!string.IsNullOrEmpty(soulList))
            {
                foreach (string soul in soulList.Split(','))
                {
                    if (!string.IsNullOrEmpty(soul))
                        receivedSouls.Add(soul);
                }
            }
            Log.LogInfo($"[Prop] Loaded {receivedSouls.Count} souls from save");
        }
        
        /// <summary>
        /// Clear saved souls from PlayerPrefs
        /// </summary>
        public void ClearSavedSouls()
        {
            PlayerPrefs.DeleteKey("AP_PropSouls");
            PlayerPrefs.Save();
        }
    }
}