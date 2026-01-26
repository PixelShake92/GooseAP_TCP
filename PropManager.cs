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
            "minipersonvariant-goose",
            "rosebox",
        };
        
        // Map item name patterns to soul names
        private static readonly Dictionary<string, string> PropToSoul = new Dictionary<string, string>
        {
            // Grouped souls - items with multiple instances
            { "carrot", "Carrots" },
            { "tomato", "Tomatoes" },
            { "pumpkin", "Pumpkins" },
            { "topsoilbag", "Topsoil Bags" },
            { "topsoil", "Topsoil Bags" },
            { "fertiliser", "Topsoil Bags" },
            { "fertliser", "Topsoil Bags" },  // Typo variant
            { "fertilizer", "Topsoil Bags" },
            { "quoitgreen", "Quoits (Green)" },
            { "quoitred", "Quoits (Red)" },
            { "plate", "Plates" },
            { "orange", "Oranges" },
            { "leek", "Leeks" },
            { "cucumber", "Cucumbers" },
            { "umbrella", "Umbrellas" },
            { "bluecan", "Tinned Food" },
            { "orangecan", "Tinned Food" },
            { "yellowcan", "Tinned Food" },
            { "canblue", "Tinned Food" },
            { "canorange", "Tinned Food" },
            { "canyellow", "Tinned Food" },
            { "sock", "Socks" },
            { "pintbottle", "Pint Bottles" },
            { "knife", "Knives" },
            { "gumboot", "Gumboots" },
            { "fork", "Forks" },
            { "applecore", "Apple Cores" },
            { "apple", "Apples" },
            { "sandwich", "Sandwich" },
            { "bow", "Ribbons" },
            { "walkietalkie", "Walkie Talkies" },
            { "boot", "Boots" },
            { "miniperson", "Miniature People" },
            { "ministreetbench", "Miniature Benches" },
            { "streetbench", "Miniature Benches" },
            { "benchstreet", "Miniature Benches" },
            { "minibench", "Miniature Benches" },
            
            // Garden one-offs
            { "cabbage", "Cabbages" },
            { "radio", "Radio" },
            { "radiosmall", "Radio" },
            { "trowel", "Trowel" },
            { "tulip", "Tulip" },
            { "jam", "Jam" },
            { "picnicmug", "Picnic Mug" },
            { "thermos", "Thermos" },
            { "strawhat", "Sun Hat" },
            { "sunhat", "Sun Hat" },
            { "drinkcan", "Drink Can" },
            { "tennisball", "Tennis Ball" },
            { "rake", "Rake" },
            { "basket", "Picnic Basket" },  // Short name from hierarchy
            { "esky", "Esky" },
            { "coolbox", "Esky" },
            { "shovel", "Shovel" },
            { "wateringcan", "Watering Can" },
            { "mallet", "Mallet" },
            { "woodencrate", "Wooden Crate" },
            { "cratewooden", "Wooden Crate" },
            
            // High Street one-offs
            { "hornrimmedglasses", "Horn-Rimmed Glasses" },
            { "redglasses", "Red Glasses" },
            { "sunglasses", "Sunglasses" },
            { "toiletpaper", "Loo Paper" },
            { "toycar", "Toy Car" },
            { "hairbrush", "Hairbrush" },
            { "toothbrush", "Toothbrush" },
            { "stereoscope", "Stereoscope" },
            { "dishsoapbottle", "Dish Soap Bottle" },
            { "dishwashbottle", "Dish Soap Bottle" },
            { "spraybottle", "Spray Bottle" },
            { "weedtool", "Weed Tools" },
            { "forkgarden", "Weed Tools" },
            { "lilyflower", "Lily Flower" },
            { "fusilage", "Toy Plane" },
            { "coin", "Coin" },
            { "chalk", "Chalk" },
            { "dustbinlid", "Dustbin Lid" },
            { "basketprop", "Shopping Basket" },
            { "top", "Topsoil Bags" },  // Short name after cleaning top_1, top_2, top_3
            { "pushbroom", "Push Broom" },
            { "dustbin", "Dustbin" },
            { "babydoll", "Baby Doll" },
            { "pricinggun", "Pricing Gun" },
            { "addingmachine", "Adding Machine" },
            { "football", "Football" },
            { "garagedoorrope", "Garage Rope" },
            
            // Back Gardens one-offs
            { "dummy", "Dummy" },
            { "cricketball", "Cricket Ball" },
            { "bustpipe", "Bust Pipe" },
            { "busthat", "Bust Hat" },
            { "bustglasses", "Bust Glasses" },
            { "teacup", "Tea Cup" },
            { "newspaper", "Newspaper" },
            { "badmintonracket", "Badminton Racket" },
            { "potstack", "Pot Stack" },
            { "soap", "Soap" },
            { "paintbrush", "Paintbrush" },
            { "vase", "Vase" },
            { "rightstrap", "Bra" },
            { "rose", "Rose" },
            // Removing Rose Box Soul until I can solve the physics issues with it
            { "rosebox", "Rose Box" },
            { "cricketbat", "Cricket Bat" },
            { "teapot", "Tea Pot" },
            { "clippers", "Clippers" },
            { "duckstatue", "Duck Statue" },
            { "duckstatueprop", "Duck Statue" },  // Full prop name
            { "duck", "Duck Statue" },
            { "frogstatue", "Frog Statue" },
            { "frog", "Frog Statue" },
            { "jeremyfish", "Jeremy Fish" },
            { "messysign", "No Goose Sign (Messy)" },
            { "drawer", "Drawer" },
            { "enameljug", "Enamel Jug" },
            { "jugenamel", "Enamel Jug" },
            { "cleansign", "No Goose Sign (Clean)" },
            
            // Pub one-offs
            { "fishingbobber", "Fishing Bobber" },
            { "exitletter", "Letter" },
            { "pintglass", "Pint Glasses" },
            { "toyboat", "Toy Boat" },
            { "peppergrinder", "Pepper Grinder" },
            { "cork", "Cork" },
            { "candlestick", "Candlestick" },
            { "flowerforvase", "Flower for Vase" },
            { "harmonica", "Harmonica" },
            { "tacklebox", "Tackle Box" },
            { "trafficcone", "Traffic Cone" },
            { "coneprop", "Traffic Cone" },
            { "exitparcel", "Parcel" },
            { "stealthbox", "Stealth Box" },
            { "nogoosesign", "No Goose Sign (Pub)" },
            { "pubnogoose", "No Goose Sign (Pub)" },
            { "portablestool", "Portable Stool" },
            { "dartboard", "Dartboard" },
            { "mopbucket", "Mop Bucket" },
            { "pail", "Bucket" },
            { "bucket", "Mop Bucket" },
            { "mop", "Mop" },
            { "mophandle", "Mop" },
            { "mophead", "Mop" },
            { "deliverybox", "Delivery Box" },
            
            // Model Village one-offs
            { "minimailpillar", "Miniature Mail Pillar" },
            { "miniphonedoor", "Miniature Phone Door" },
            { "minishovelprop", "Miniature Shovel" },
            { "poppyflower", "Poppy Flower" },
            { "flowerpoppy", "Poppy Flower" },
            { "timberhandle", "Timber Handle" },
            { "birdbath", "Miniature Birdbath" },
            { "easel", "Miniature Easel" },
            { "minipump", "Miniature Pump" },
            { "sunlounge", "Miniature Sun Lounge" },
            { "minipersonvariant-goose", "Miniature Goose" },
            
            // Victory item
            { "goldenbell", "Golden Bell" },
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
                        //DisableAllProps();
                        //ApplyAllPropStates();
                        InverseApplyAllPropStates();
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
                GameObject fertilizer = null;
                GameObject fertilizer2 = null;
                GameObject fertilizer3 = null;
                GameObject fertilizer4 = null;
                GameObject fertilizer5 = null;
                GameObject fertilizer6 = null;
                GameObject garageRope = null;
                GameObject garageRope2 = null;
                GameObject roseBox = null;

                var checkAllGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                Log.LogInfo($"[Prop] Found {checkAllGameObjects.Length} GameObjects");
                foreach (var gameObj in checkAllGameObjects)
                {
                    if (gameObj == null) continue;
                    /*Log.LogInfo($"[Prop DEBUG] Noting existence of game object with name: '{gameObj.name}'");

                    if (gameObj.name.ToLower().Contains("proplaunchzone") || gameObj.transform != null && gameObj.transform.parent != null && gameObj.transform.parent.name.ToLower().Contains("proplaunchzone"))
                    {
                        string hierarchy = GetHierarchy(gameObj.transform);
                        Log.LogInfo($"             {hierarchy}");
                    }
                    
                    if (gameObj.name.ToLower().Contains("bucket") || gameObj.name.ToLower().Contains("pail"))
                    {
                        string hierarchy = GetHierarchy(gameObj.transform);
                        Log.LogInfo($"[Prop DEBUG] Noting existence of bucket game object with name: '{gameObj.name}': {hierarchy}");
                    }*/
                    
                    // TO DO: fix bucket bug
                    // TO DO: make sure rose box works right
                    // TO DO: fix umbrellas spawning in wrong

                    if (gameObj.name == "braSkinned")
                    {
                        bra = gameObj;
                        Log.LogInfo($"[Prop] Using '{gameObj.name}' for bra");
                        if (bra.transform != null)
                        {
                            Log.LogInfo($"[Prop DEBUG] Parent of '{gameObj.name}' is '{bra.transform.name}'");
                        }
                        if (bra.transform.parent != null)
                        {
                            Log.LogInfo($"[Prop DEBUG] Grandparent of '{gameObj.name}' is '{bra.transform.parent.name}'");
                        }
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
                    else if (gameObj.name == "fertilizer")
                    {
                        if (fertilizer == null)
                        {
                            fertilizer = gameObj;
                            Log.LogInfo($"[Prop] Using '{gameObj.name}' for fertilizer");
                        }
                        else if (fertilizer2 == null)
                        {
                            fertilizer2 = gameObj;
                            Log.LogInfo($"[Prop] Using '{gameObj.name}' for fertilizer2");
                        }
                        else if (fertilizer3 == null)
                        {
                            fertilizer3 = gameObj;
                            Log.LogInfo($"[Prop] Using '{gameObj.name}' for fertilizer3");
                        }
                        else if (fertilizer4 == null)
                        {
                            fertilizer4 = gameObj;
                            Log.LogInfo($"[Prop] Using '{gameObj.name}' for fertilizer4");
                        }
                        else if (fertilizer5 == null)
                        {
                            fertilizer5 = gameObj;
                            Log.LogInfo($"[Prop] Using '{gameObj.name}' for fertilizer5");
                        }
                    }
                    else if (gameObj.name == "fertilizer (1)")
                    {
                        fertilizer6 = gameObj;
                        Log.LogInfo($"[Prop] Using '{gameObj.name}' for fertilizer6");
                    }
                    else if (gameObj.name == "garageDoorRope")
                    {
                        garageRope = gameObj;
                        Log.LogInfo($"[Prop] Using '{gameObj.name}' for garageRope");
                    }
                    else if (gameObj.name == "RopeRingParent")
                    {
                        garageRope2 = gameObj;
                        Log.LogInfo($"[Prop] Using '{gameObj.name}' for garageRope2");
                    }
                    else if (gameObj.name == "rosebox")
                    {
                        roseBox = gameObj;
                        Log.LogInfo($"[Prop] Using '{gameObj.name}' for rosebox");
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

                // Have to do garage rope stuff outside of the foreach because there is no associated prop
                if (!propCache.ContainsKey("Garage Rope"))
                {
                    propCache["Garage Rope"] = new List<GameObject>();
                }
                Log.LogInfo($"[Prop DEBUG] Forcing soul match for: '{garageRope.name}'. Attached to: 'Garage Rope'.");
                propCache["Garage Rope"].Add(garageRope);
                Log.LogInfo($"[Prop DEBUG] Forcing soul match for: '{garageRope2.name}'. Attached to: 'Garage Rope'.");
                propCache["Garage Rope"].Add(garageRope2);
                
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
                            else if (cleanName == "top")
                            {
                                if (fertilizer != null && !propCache[soul].Contains(fertilizer))
                                {
                                    Log.LogInfo($"[Prop DEBUG] Forcing soul match for: '{cleanName}'. Attached to: '{soul}'. '{prop.name}' added to propCacheUsingProps");
                                    propCache[soul].Add(fertilizer);
                                    propCache[soul].Add(fertilizer2);
                                    propCache[soul].Add(fertilizer3);
                                    propCache[soul].Add(fertilizer4);
                                    propCache[soul].Add(fertilizer5);
                                    propCache[soul].Add(fertilizer6);
                                    propCacheUsingProps[soul].Add(prop);
                                }
                            }
                            else if (cleanName == "rosebox")
                            {
                                if (roseBox != null && !propCache[soul].Contains(roseBox))
                                {
                                    Log.LogInfo($"[Prop DEBUG] Forcing soul match for: '{cleanName}'. Attached to: '{soul}'.");
                                    propCache[soul].Add(roseBox);
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
                /*if (cleanName.Contains("top") && (parentName.Contains("top") || parentName.Contains("soil") || parentName.Contains("fertil")))
                {
                    Log.LogInfo($"[Prop] Using parent '{current.parent.name}' for topsoil");
                    return current.parent.gameObject;
                }*/

                // Objects that need to be handled differently (eg, bra). basket is == instead of Contains else it ruins the shopping basket
                if (cleanName.Contains("rightstrap") && parentName == "brahome" || cleanName.Contains("drawer") || cleanName == "basket" || cleanName == "top" || cleanName == "rosebox")
                {
                    return null;
                }
            }
            
            // Default - just use the prop's own GameObject
            return propObj;
        }
        
        public static string CleanPropName(string name)
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
                    Log.LogInfo($"[Prop DEBUG] cleanname '{cleanName}' .StartsWith or .Contains soul: '{kvp.Key}'");
                    return kvp.Value;
                }
            }
            
            // Check if a key starts with or contains cleanName (for short names like "top", "basket")
            foreach (var kvp in PropToSoul)
            {
                if (!PerfectMatchOnlyProps.Contains(kvp.Key) && (kvp.Key.StartsWith(cleanName) || kvp.Key.Contains(cleanName)))
                {
                    Log.LogInfo($"[Prop DEBUG] soul '{kvp.Key}' .StartsWith or .Contains cleanname: '{cleanName}'");
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
        /// Disable any prop we don't have a soul for
        /// </summary>
        private void InverseApplyAllPropStates()
        {
            // If souls are disabled, enable everything
            if (!PropSoulsEnabled)
            {
                return;
            }

            int count = 0;
            foreach (var kvp in propCache)
            {
                string soul = kvp.Key;
                // Never disable props needed for progression, or if we have the soul
                if (AlwaysEnabledProps.Contains(soul) || receivedSouls.Contains(soul)) continue;

                // Otherwise, disable until we have the soul
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
                //DisableAllProps();
                //ApplyAllPropStates();
                InverseApplyAllPropStates();
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