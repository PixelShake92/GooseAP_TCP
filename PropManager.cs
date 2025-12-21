using System;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

namespace GooseGameAP
{
    /// <summary>
    /// Manages prop visibility based on prop soul items.
    /// Props are disabled until their corresponding soul is received.
    /// Uses a dictionary-based approach for scalability.
    /// </summary>
    public class PropManager
    {
        private static ManualLogSource Log => Plugin.Log;
        private Plugin plugin;
        
        // Track which souls we have received
        private HashSet<string> receivedSouls = new HashSet<string>();
        
        // Cache of props by soul type
        private Dictionary<string, List<GameObject>> propCache = new Dictionary<string, List<GameObject>>();
        
        private bool hasScannedProps = false;
        
        // Map item name patterns to soul names
        // Key = lowercase item name pattern, Value = soul name
        private static readonly Dictionary<string, string> PropToSoul = new Dictionary<string, string>
        {
            // Grouped souls - items with multiple instances
            { "carrot", "Carrot Soul" },
            { "tomato", "Tomato Soul" },
            { "pumpkin", "Pumpkin Soul" },
            { "topsoilbag", "Topsoil Bag Soul" },
            { "topsoil", "Topsoil Bag Soul" },
            { "top", "Topsoil Bag Soul" },
            { "quoit", "Quoit Soul" },
            { "plate", "Plate Soul" },
            { "orange", "Orange Soul" },
            { "leek", "Leek Soul" },
            { "cucumber", "Cucumber Soul" },
            { "dart", "Dart Soul" },
            { "umbrella", "Umbrella Soul" },
            { "bluecan", "Spray Can Soul" },
            { "orangecan", "Spray Can Soul" },
            { "yellowcan", "Spray Can Soul" },
            { "canblue", "Spray Can Soul" },
            { "canorange", "Spray Can Soul" },
            { "canyellow", "Spray Can Soul" },
            { "sock", "Sock Soul" },
            { "pintbottle", "Pint Bottle Soul" },
            { "knife", "Knife Soul" },
            { "gumboot", "Gumboot Soul" },
            { "fork", "Fork Soul" },
            { "brokenvasepiece", "Vase Piece Soul" },
            { "brokenbit", "Vase Piece Soul" },
            { "applecore", "Apple Core Soul" },
            { "apple", "Apple Soul" },
            { "sandwich", "Sandwich Soul" },
            { "slipper", "Slipper Soul" },
            { "bow", "Bow Soul" },
            { "walkietalkie", "Walkie Talkie Soul" },
            { "boot", "Boot Soul" },
            { "miniperson", "Mini Person Soul" },
            
            // Garden one-offs
            { "radio", "Radio Soul" },
            { "trowel", "Trowel Soul" },
            { "keys", "Keys Soul" },
            { "keyring", "Keys Soul" },
            { "carkeys", "Keys Soul" },
            { "tulip", "Tulip Soul" },
            { "jam", "Jam Soul" },
            { "picnicmug", "Picnic Mug Soul" },
            { "thermos", "Thermos Soul" },
            { "strawhat", "Straw Hat Soul" },
            { "sunhat", "Straw Hat Soul" },
            { "drinkcan", "Drink Can Soul" },
            { "tennisball", "Tennis Ball Soul" },
            { "gardenerhat", "Gardener Hat Soul" },
            { "gardenershat", "Gardener Hat Soul" },
            { "hatgardener", "Gardener Hat Soul" },
            { "gardenerssunhat", "Gardener Hat Soul" },
            { "rake", "Rake Soul" },
            { "picnicbasket", "Picnic Basket Soul" },
            { "esky", "Esky Soul" },
            { "coolbox", "Esky Soul" },
            { "shovel", "Shovel Soul" },
            { "wateringcan", "Watering Can Soul" },
            { "fencebolt", "Fence Bolt Soul" },
            { "boltbent", "Fence Bolt Soul" },
            { "mallet", "Mallet Soul" },
            { "woodencrate", "Wooden Crate Soul" },
            { "cratewooden", "Wooden Crate Soul" },
            { "gardenersign", "Gardener Sign Soul" },
            
            // High Street one-offs
            { "boysglasses", "Boy's Glasses Soul" },
            { "boyglasses", "Boy's Glasses Soul" },
            { "glassesboy", "Boy's Glasses Soul" },
            { "wimpglasses", "Boy's Glasses Soul" },
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
            { "lilyflower", "Lily Flower Soul" },
            { "fusilage", "Fusilage Soul" },
            { "coin", "Coin Soul" },
            { "chalk", "Chalk Soul" },
            { "dustbinlid", "Dustbin Lid Soul" },
            { "shoppingbasket", "Shopping Basket Soul" },
            { "basket", "Shopping Basket Soul" },
            { "basketprop", "Picnic Basket Soul" },
            { "pushbroom", "Push Broom Soul" },
            { "brokenbroomhead", "Broken Broom Head Soul" },
            { "broomheadseperate", "Broken Broom Head Soul" },
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
            { "rightstrap", "Right Strap Soul" },
            { "rose", "Rose Soul" },
            { "rosebox", "Rose Box Soul" },
            { "cricketbat", "Cricket Bat Soul" },
            { "teapot", "Tea Pot Soul" },
            { "clippers", "Clippers Soul" },
            { "duckstatue", "Duck Statue Soul" },
            { "frogstatue", "Frog Statue Soul" },
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
            { "woolyhat", "Wooly Hat Soul" },
            { "woollyhat", "Wooly Hat Soul" },
            { "peppergrinder", "Pepper Grinder Soul" },
            { "pubcloth", "Pub Cloth Soul" },
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
            { "pail", "Mop Bucket Soul" },
            { "bucket", "Mop Bucket Soul" },
            { "mop", "Mop Soul" },
            { "deliverybox", "Delivery Box Soul" },
            { "tomatobox", "Tomato Box Soul" },
            
            // Model Village one-offs
            { "minimailpillar", "Mini Mail Pillar Soul" },
            { "miniphonedoor", "Mini Phone Door Soul" },
            { "minishovel", "Mini Shovel Soul" },
            { "poppyflower", "Poppy Flower Soul" },
            { "flowerpoppy", "Poppy Flower Soul" },
            { "timberhandle", "Timber Handle Soul" },
            { "birdbath", "Birdbath Soul" },
            { "easel", "Easel Soul" },
            { "minibench", "Mini Bench Soul" },
            { "minipump", "Mini Pump Soul" },
            { "ministreetbench", "Mini Street Bench Soul" },
            { "streetbench", "Mini Street Bench Soul" },
            { "benchstreet", "Mini Street Bench Soul" },
            { "sunlounge", "Sun Lounge Soul" },
            
            // Victory item
            { "goldenbell", "Golden Bell Soul" },
        };
        
        public PropManager(Plugin plugin)
        {
            this.plugin = plugin;
        }
        
        /// <summary>
        /// Called every frame from Plugin.Update()
        /// </summary>
        public void Update()
        {
            try
            {
                // Only scan when in-game, not at menu
                if (GameManager.instance == null) return;
                if (GameManager.instance.allGeese == null) return;
                if (GameManager.instance.allGeese.Count == 0) return;
                
                // Scan and DISABLE all props when game loads (before connection)
                if (!hasScannedProps)
                {
                    Log.LogInfo("[Prop] Game loaded - scanning for props");
                    ScanAndCacheProps();
                    DisableAllProps();  // Disable ALL props immediately
                    ApplyAllPropStates();  // Re-enable ones we have souls for
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
        /// Scan the game world for props and cache them by soul type
        /// </summary>
        private void ScanAndCacheProps()
        {
            if (hasScannedProps) return;
            hasScannedProps = true;
            
            propCache.Clear();
            
            try
            {
                // Find all Props in scene
                var allProps = UnityEngine.Object.FindObjectsOfType<Prop>();
                Log.LogInfo($"[Prop] Found {allProps.Length} props total");
                
                // DEBUG: Log first 20 prop names to understand naming pattern
                int logCount = 0;
                foreach (var prop in allProps)
                {
                    if (prop == null) continue;
                    if (logCount < 20)
                    {
                        string cleanName = CleanPropName(prop.name);
                        Log.LogInfo($"[Prop DEBUG] Raw: '{prop.name}' -> Clean: '{cleanName}'");
                        logCount++;
                    }
                }
                
                int matched = 0;
                int unmatched = 0;
                
                foreach (var prop in allProps)
                {
                    if (prop == null) continue;
                    
                    string cleanName = CleanPropName(prop.name);
                    string soul = GetSoulForProp(cleanName);
                    
                    if (soul != null)
                    {
                        if (!propCache.ContainsKey(soul))
                            propCache[soul] = new List<GameObject>();
                        
                        propCache[soul].Add(prop.gameObject);
                        matched++;
                    }
                    else
                    {
                        unmatched++;
                        // Log ALL unmatched for debugging
                        Log.LogWarning($"[Prop] No soul match for: '{prop.name}' (cleaned: '{cleanName}')");
                    }
                }
                
                Log.LogInfo($"[Prop] Matched {matched} props, unmatched {unmatched}");
                
                // Log what we found
                foreach (var kvp in propCache)
                {
                    Log.LogInfo($"[Prop] {kvp.Key}: {kvp.Value.Count} props");
                }
            }
            catch (System.Exception ex)
            {
                Log.LogError($"[Prop] ScanAndCacheProps error: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// Clean prop name for matching
        /// </summary>
        private string CleanPropName(string name)
        {
            string clean = name.ToLower()
                .Replace("(clone)", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(" ", "")
                .Replace("_", "")
                .Trim();
            
            // Remove trailing numbers
            while (clean.Length > 0 && char.IsDigit(clean[clean.Length - 1]))
                clean = clean.Substring(0, clean.Length - 1);
            
            return clean;
        }
        
        /// <summary>
        /// Get the soul required for a prop based on its name
        /// </summary>
        private string GetSoulForProp(string cleanName)
        {
            // Exact match first
            if (PropToSoul.TryGetValue(cleanName, out string soul))
                return soul;
            
            // Partial match - check if prop name starts with or contains a key
            foreach (var kvp in PropToSoul)
            {
                if (cleanName.StartsWith(kvp.Key) || cleanName.Contains(kvp.Key))
                    return kvp.Value;
            }
            
            return null;
        }
        
        /// <summary>
        /// Apply prop states based on received souls
        /// </summary>
        private void ApplyAllPropStates()
        {
            foreach (var kvp in propCache)
            {
                string soul = kvp.Key;
                bool hasSoul = receivedSouls.Contains(soul);
                
                foreach (var prop in kvp.Value)
                {
                    if (prop != null)
                        prop.SetActive(hasSoul);
                }
            }
            
            Log.LogInfo($"[Prop] Applied states for {receivedSouls.Count} received souls");
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
            
            // Enable props for this soul
            if (propCache.TryGetValue(soulName, out var props))
            {
                foreach (var prop in props)
                {
                    if (prop != null)
                    {
                        prop.SetActive(true);
                        Log.LogInfo($"[Prop] Enabled: {prop.name}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Check if we have a specific soul
        /// </summary>
        public bool HasSoul(string soulName)
        {
            return receivedSouls.Contains(soulName);
        }
        
        /// <summary>
        /// Refresh prop states (called when reconnecting to same slot)
        /// </summary>
        public void RefreshPropStates()
        {
            if (!hasScannedProps) return;
            
            Log.LogInfo("[Prop] RefreshPropStates called");
            DisableAllProps();
            ApplyAllPropStates();
        }
        
        /// <summary>
        /// Reset for returning to menu
        /// </summary>
        public void Reset()
        {
            Log.LogInfo("[Prop] Reset called");
            hasScannedProps = false;
            propCache.Clear();
            // Don't clear receivedSouls - persist across game resets
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