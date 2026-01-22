using BepInEx.Logging;

namespace GooseGameAP
{
    /// <summary>
    /// Maps game object names to Archipelago location IDs and vice versa.
    /// ID Ranges:
    /// - Goals: 1-80
    /// - Items: 1001-1199
    /// - Drags: 1201-1299
    /// </summary>
    public static class LocationMappings
    {
        private static ManualLogSource Log => Plugin.Log;
        private const long BASE_ID = Plugin.BASE_ID;

        /// <summary>
        /// Clean item name by removing (Clone) suffix and trailing numbers
        /// </summary>
        public static string CleanItemName(string itemName)
        {
            string cleaned = itemName.ToLower().Trim();
            
            // Strip (Clone) suffix if present (happens after area reset)
            if (cleaned.Contains("(clone)"))
            {
                cleaned = cleaned.Replace("(clone)", "").Trim();
            }
            
            return cleaned;
        }

        /// <summary>
        /// Get Archipelago item name from item ID
        /// </summary>
        public static string GetItemName(long itemId)
        {
            long offset = itemId - BASE_ID;
            switch (offset)
            {
                // Area Access
                case 100: return "Garden Access";
                case 101: return "High Street Access";
                case 102: return "Back Gardens Access";
                case 103: return "Pub Access";
                case 104: return "Model Village Access";
                case 110: return "Progressive Area";
                
                // NPC Souls (120-130)
                case 120: return "Groundskeeper's Soul";
                case 121: return "Boy's Soul";
                case 122: return "TV Shop Owner's Soul";
                case 123: return "Market Lady's Soul";
                case 124: return "Tidy Neighbour's Soul";
                case 125: return "Messy Neighbour's Soul";
                case 126: return "Burly Man's Soul";
                case 127: return "Old Man's Soul";
                case 128: return "Pub Lady's Soul";
                case 129: return "Fancy Ladies' Souls";
                case 130: return "Cook's Soul";
                
                // Filler/Useful
                case 200: return "Mega Honk";
                case 201: return "Speedy Feet";
                case 202: return "Silent Steps";
                case 203: return "A Goose Day";
                case 204: return "Coin";
                
                // Traps
                case 300: return "Tired Goose";
                case 301: return "Confused Feet";
                case 302: return "Butterbeak";
                case 303: return "Suspicious Goose";
                
                // Grouped Prop Souls (400-425)
                case 400: return "Carrots";
                case 401: return "Tomatoes";
                case 402: return "Pumpkins";
                case 403: return "Topsoil Bags";
                case 404: return "Quoits (Green)";
                case 405: return "Plates";
                case 406: return "Oranges";
                case 407: return "Leeks";
                case 408: return "Cucumbers";
                case 409: return "Quoits (Red)";
                case 410: return "Umbrellas";
                case 411: return "Tinned Food";
                case 412: return "Socks";
                case 413: return "Pint Bottles";
                case 414: return "Knives";
                case 415: return "Gumboots";
                case 416: return "Forks";
                // case 417: return "";
                case 418: return "Apple Cores";
                case 419: return "Apples";
                case 420: return "Sandwich";
                // case 421: return "";
                case 422: return "Ribbons";
                case 423: return "Walkie Talkies";
                case 424: return "Boots";
                case 425: return "Miniature People";
                
                // Garden One-Off Prop Souls (500-519)
                case 500: return "Radio";
                case 501: return "Trowel";
                // case 502: return "";
                case 503: return "Tulip";
                case 504: return "Jam";
                case 505: return "Picnic Mug";
                case 506: return "Thermos";
                case 507: return "Sun Hat";
                case 508: return "Drink Can";
                case 509: return "Tennis Ball";
                // case 510: return "";
                case 511: return "Rake";
                case 512: return "Picnic Basket";
                case 513: return "Esky";
                case 514: return "Shovel";
                case 515: return "Watering Can";
                // case 516: return "";
                case 517: return "Mallet";
                case 518: return "Wooden Crate";
                
                // High Street One-Off Prop Souls (520-543)
                // case 520: return "";
                case 521: return "Horn-Rimmed Glasses";
                case 522: return "Red Glasses";
                case 523: return "Sunglasses";
                case 524: return "Loo Paper";
                case 525: return "Toy Car";
                case 526: return "Hairbrush";
                case 527: return "Toothbrush";
                case 528: return "Stereoscope";
                case 529: return "Dish Soap Bottle";
                case 530: return "Spray Bottle";
                case 531: return "Weed Tools";
                case 532: return "Lily Flower";
                case 533: return "Toy Plane";
                case 534: return "Football";
                case 535: return "Chalk";
                case 536: return "Dustbin Lid";
                case 537: return "Shopping Basket";
                case 538: return "Push Broom";
                case 540: return "Dustbin";
                case 541: return "Baby Doll";
                case 542: return "Pricing Gun";
                case 543: return "Adding Machine";
                // case 545: return "";
                
                // Back Gardens One-Off Prop Souls (550-574)
                case 550: return "Dummy";
                case 551: return "Cricket Ball";
                case 552: return "Bust Pipe";
                case 553: return "Bust Hat";
                case 554: return "Bust Glasses";
                case 555: return "Tea Cup";
                case 556: return "Newspaper";
                case 557: return "Badminton Racket";
                case 558: return "Pot Stack";
                case 559: return "Soap";
                case 560: return "Paintbrush";
                case 561: return "Vase";
                case 562: return "Bra";
                case 563: return "Rose";
                // Removing Rose Box Soul until I can solve the physics issues with it
                // case 564: return "Rose Box";
                case 565: return "Cricket Bat";
                case 566: return "Tea Pot";
                case 567: return "Clippers";
                case 568: return "Duck Statue";
                case 569: return "Frog Statue";
                case 570: return "Jeremy Fish";
                case 571: return "No Goose Sign (Messy)";
                case 572: return "Drawer";
                case 573: return "Enamel Jug";
                case 574: return "No Goose Sign (Clean)";
                
                // Pub One-Off Prop Souls (580-601)
                case 580: return "Fishing Bobber";
                case 581: return "Letter";
                case 582: return "Pint Glasses";
                case 583: return "Toy Boat";
                // case 584: return "";
                case 585: return "Pepper Grinder";
                // case 586: return "";
                case 587: return "Cork";
                case 588: return "Candlestick";
                case 589: return "Flower for Vase";
                case 590: return "Harmonica";
                case 591: return "Tackle Box";
                case 592: return "Traffic Cone";
                case 593: return "Parcel";
                case 594: return "Stealth Box";
                case 595: return "No Goose Sign (Pub)";
                case 596: return "Portable Stool";
                case 597: return "Dartboard";
                case 598: return "Mop Bucket";
                case 599: return "Mop";
                // case 600: return "";
                case 601: return "Bucket";
                
                // Model Village One-Off Prop Souls (610-620)
                case 610: return "Miniature Mail Pillar";
                case 611: return "Miniature Phone Door";
                case 612: return "Miniature Shovel";
                case 613: return "Poppy Flower";
                case 614: return "Timber Handle";
                case 615: return "Miniature Birdbath";
                case 616: return "Miniature Easel";
                case 617: return "Miniature Benches";
                case 618: return "Miniature Pump";
                case 619: return "Miniature Goose";
                case 620: return "Miniature Sun Lounge";
                case 621: return "Golden Bell";
                
                // Victory
                case 999: return "Escape Sequence";
                
                default: return "Unknown Item (" + offset + ")";
            }
        }

        /// <summary>
        /// Get location ID for a goal completion
        /// </summary>
        public static long? GetGoalLocationId(string goalName)
        {
            switch (goalName)
            {
                // Garden goals (1-7)
                case "goalGarden": return BASE_ID + 1;
                case "goalWet": return BASE_ID + 2;
                case "goalKeys": return BASE_ID + 3;
                case "goalHat": return BASE_ID + 4;
                case "goalRake": return BASE_ID + 5;
                case "goalPicnic": return BASE_ID + 6;
                case "goalHammering": return BASE_ID + 7;
                
                // High Street goals (10-16)
                case "goalBroom": return BASE_ID + 10;
                case "goalPhonebooth": return BASE_ID + 11;
                case "goalWrongGlasses": return BASE_ID + 12;
                case "goalBuyBack": return BASE_ID + 13;
                case "goalGetInShop": return BASE_ID + 14;
                case "goalShopping": return BASE_ID + 15;
                case "goalGarage": return BASE_ID + 16;
                
                // Back Gardens goals (20-26)
                case "goalBreakVase": return BASE_ID + 20;
                case "goalDressStatue": return BASE_ID + 21;
                case "goalBell": return BASE_ID + 22;
                case "goalRibbon": return BASE_ID + 23;
                case "goalBarefoot": return BASE_ID + 24;
                case "goalWashing": return BASE_ID + 25;
                case "goalPrune": return BASE_ID + 26;
                
                // Pub goals (30-37)
                case "goalIntoPub": return BASE_ID + 30;
                case "goalOldMan1": return BASE_ID + 31;
                case "goalBoat": return BASE_ID + 32;
                case "goalOldMan2": return BASE_ID + 33;
                case "goalFlower": return BASE_ID + 34;
                case "goalPintGlass": return BASE_ID + 35;
                case "goalSetTable": return BASE_ID + 36;
                case "goalBucket": return BASE_ID + 37;
                
                // Finale goals (40-42)
                case "goalModelVillage": return BASE_ID + 40;
                case "goalStealBell": return BASE_ID + 41;
                case "goalFinale": return BASE_ID + 42;
                
                // Extra goals (50-56)
                case "goalLockout": return BASE_ID + 50;
                case "goalCabbage": return BASE_ID + 51;
                case "goalPuddle": return BASE_ID + 52;
                case "goalScales": return BASE_ID + 53;
                case "goalUmbrella": return BASE_ID + 54;
                case "goalBuyBack2": return BASE_ID + 55;
                case "goalFlowers": return BASE_ID + 56;
                
                // More extra goals (60-67)
                case "goalWimpGarage": return BASE_ID + 60;
                case "goalCatch": return BASE_ID + 61;
                case "goalThrownGoose": return BASE_ID + 62;
                case "goalBust2": return BASE_ID + 63;
                case "goalFootball": return BASE_ID + 64;
                case "goalBoatBridge": return BASE_ID + 65;
                case "goalPerformRibbon": return BASE_ID + 66;
                case "goalOldManHat": return BASE_ID + 67;
                
                // Speedrun goals (70-73)
                case "goalSpeedyGarden": return BASE_ID + 70;
                case "goalSpeedyShops": return BASE_ID + 71;
                case "goalSpeedyBackyards": return BASE_ID + 72;
                case "goalSpeedyPub": return BASE_ID + 73;
                
                // 100% completion
                case "goal100": return BASE_ID + 80;
                
                default: return null;
            }
        }

        /// <summary>
        /// Get location ID for a first-time item pickup
        /// </summary>
        public static long? GetItemLocationId(string itemName)
        {
            string lowerName = itemName.ToLower().Trim();
            
            // Strip (Clone) suffix if present (happens after area reset)
            if (lowerName.Contains("(clone)"))
            {
                lowerName = lowerName.Replace("(clone)", "").Trim();
            }
            
            
            // Check for renamed carrots first (carrot_1 through carrot_13)
            // Sorted by X position: 1-10 are Garden, 11-13 are High Street shop
            switch (lowerName)
            {
                case "carrot_1": return BASE_ID + 1401;
                case "carrot_2": return BASE_ID + 1402;
                case "carrot_3": return BASE_ID + 1403;
                case "carrot_4": return BASE_ID + 1404;
                case "carrot_5": return BASE_ID + 1405;
                case "carrot_6": return BASE_ID + 1406;
                case "carrot_7": return BASE_ID + 1407;
                case "carrot_8": return BASE_ID + 1408;
                case "carrot_9": return BASE_ID + 1409;
                case "carrot_10": return BASE_ID + 1410;
                case "carrot_11": return BASE_ID + 1411;  // Shop carrot
                case "carrot_12": return BASE_ID + 1412;  // Shop carrot
                case "carrot_13": return BASE_ID + 1413;  // Shop carrot
            }
            
            // Check for renamed pub tomatoes (pubtomato_1 through pubtomato_11)
            switch (lowerName)
            {
                case "pubtomato_1": return BASE_ID + 1421;
                case "pubtomato_2": return BASE_ID + 1422;
                case "pubtomato_3": return BASE_ID + 1423;
                case "pubtomato_4": return BASE_ID + 1424;
                case "pubtomato_5": return BASE_ID + 1425;
                case "pubtomato_6": return BASE_ID + 1426;
                case "pubtomato_7": return BASE_ID + 1427;
                case "pubtomato_8": return BASE_ID + 1428;
                case "pubtomato_9": return BASE_ID + 1429;
                case "pubtomato_10": return BASE_ID + 1430;
                case "pubtomato_11": return BASE_ID + 1431;
            }
            
            // Check for renamed boots (boot_1 at Garden start, boot_2 in Hub near dummyprop)
            switch (lowerName)
            {
                case "boot_1": return BASE_ID + 1440;  // Garden/start area boot
                case "boot_2": return BASE_ID + 1441;  // Hub boot (near dummyprop)
            }
            
            // Check for renamed pint bottles ()
            switch (lowerName)
            {
                case "pintbottle_1": return BASE_ID + 1042;  // 
                case "pintbottle_2": return BASE_ID + 1048;  // 
                case "pintbottle_3": return BASE_ID + 1049;  // 
            }
            
            // Check for renamed topsoil bags (top_1, top_2, top_3)
            switch (lowerName)
            {
                case "top_1": return BASE_ID + 1450;
                case "top_2": return BASE_ID + 1451;
                case "top_3": return BASE_ID + 1452;
            }
            
            switch (lowerName)
            {
                // Garden items (1002-1020) - Boot handled above as boot_1/boot_2
                case "radiosmall": return BASE_ID + 1002;
                case "trowel": return BASE_ID + 1003;
                case "keys": return BASE_ID + 1004;
                case "tulip": return BASE_ID + 1006;
                case "apple": return BASE_ID + 1007;
                case "jam": return BASE_ID + 1008;
                case "picnicmug": return BASE_ID + 1009;
                case "thermos (1)": return BASE_ID + 1010;
                case "sandwichr": return BASE_ID + 1011;
                case "sandwichl": return BASE_ID + 1012;
                case "strawhat": return BASE_ID + 1014;
                case "drinkcan": return BASE_ID + 1015;
                case "tennisball": return BASE_ID + 1016;
                case "gardenerhat": return BASE_ID + 1017;
                case "apple (1)": return BASE_ID + 1018;
                
                // High Street items (1021-1070)
                case "wimpglasses": return BASE_ID + 1021;
                case "hornrimmedglasses": return BASE_ID + 1022;
                case "redglasses": return BASE_ID + 1023;
                case "sunglasses": return BASE_ID + 1024;
                case "toiletpaper": return BASE_ID + 1025;
                case "toycar": return BASE_ID + 1026;
                case "hairbrush": return BASE_ID + 1027;
                case "toothbrush": return BASE_ID + 1028;
                case "stereoscope": return BASE_ID + 1029;
                case "dishwashbottle": return BASE_ID + 1030;
                case "canblue": return BASE_ID + 1031;
                case "canyellow": return BASE_ID + 1032;
                case "canorange": return BASE_ID + 1033;
                case "weedtool": return BASE_ID + 1034;
                case "forkgarden": return BASE_ID + 1062;
                case "lilyflower": return BASE_ID + 1035;
                case "orange": return BASE_ID + 1036;
                case "tomato (1)": return BASE_ID + 1037;
                case "carrotnogreen (1)": return BASE_ID + 1038;
                case "cucumber (1)": return BASE_ID + 1039;
                case "leek (1)": return BASE_ID + 1040;
                case "fusilage": return BASE_ID + 1041;
                //case "pintbottle": return BASE_ID + 1042;
                case "spraybottle": return BASE_ID + 1043;
                case "walkietalkieb": return BASE_ID + 1044;
                case "walkietalkie": return BASE_ID + 1045;
                case "applecore": return BASE_ID + 1046;
                case "applecore (1)": return BASE_ID + 1058;
                case "dustbinlid": return BASE_ID + 1047;
                //case "pintbottle (1)": return BASE_ID + 1048;
                // case "coin": return BASE_ID + 1049;
                case "chalk": return BASE_ID + 1050;
                case "tomato (2)": return BASE_ID + 1051;
                case "orange (1)": return BASE_ID + 1052;
                case "orange (2)": return BASE_ID + 1053;
                case "carrotnogreen (3)": return BASE_ID + 1054;
                case "carrotnogreen (2)": return BASE_ID + 1057;
                case "cucumber (2)": return BASE_ID + 1055;
                case "leek (2)": return BASE_ID + 1056;
                case "leek (3)": return BASE_ID + 1059;
                case "tomato (3)": return BASE_ID + 1060;
                case "cucumber": return BASE_ID + 1061;
                
                // Back Gardens items (1071-1093)
                case "bowprop_b": return BASE_ID + 1071;
                case "dummyprop": return BASE_ID + 1072;
                case "cricketball": return BASE_ID + 1073;
                case "bustpipeprop": return BASE_ID + 1074;
                case "busthatprop": return BASE_ID + 1075;
                case "bustglassesprop": return BASE_ID + 1076;
                case "cleanslipperr": return BASE_ID + 1077;
                case "cleanslipperl": return BASE_ID + 1078;
                case "teacup": return BASE_ID + 1079;
                case "newspaper": return BASE_ID + 1080;
                case "socksplaceholder": return BASE_ID + 1081;
                case "socksplaceholder (1)": return BASE_ID + 1082;
                case "vaseprop": return BASE_ID + 1083;
                case "bowprop": return BASE_ID + 1084;
                case "potstack": return BASE_ID + 1085;
                case "soap": return BASE_ID + 1086;
                case "paintbrush": return BASE_ID + 1087;
                case "vasebroken01": return BASE_ID + 1088;
                case "vasebroken02": return BASE_ID + 1089;
                case "rightstrap":
                case "rightstrap (1)":
                case "rightstrap (2)": return BASE_ID + 1090;
                case "badmintonracket": return BASE_ID + 1093;
                case "roseprop": return BASE_ID + 1094;
                
                // Pub items (1101-1130)
                case "fishingbobberprop": return BASE_ID + 1101;
                case "exitletterprop":
                case "exitletter": return BASE_ID + 1102;
                case "plate": return BASE_ID + 1104;
                case "plate (1)": return BASE_ID + 1105;
                case "plate (2)": return BASE_ID + 1106;
                case "quoitgreen": return BASE_ID + 1107;
                case "quoitgreen (1)": return BASE_ID + 1125;
                case "quoitgreen (2)": return BASE_ID + 1126;
                case "quoitred": return BASE_ID + 1108;
                case "quoitred (1)": return BASE_ID + 1127;
                case "quoitred (2)": return BASE_ID + 1128;
                case "fork": return BASE_ID + 1109;
                case "fork (1)": return BASE_ID + 1110;
                case "knife": return BASE_ID + 1111;
                case "knife (1)": return BASE_ID + 1112;
                case "cork": return BASE_ID + 1113;
                case "candlestick": return BASE_ID + 1114;
                case "flowerforvase": return BASE_ID + 1115;
                case "dart1": return BASE_ID + 1116;
                case "dart2": return BASE_ID + 1117;
                case "dart3": return BASE_ID + 1118;
                case "harmonica": return BASE_ID + 1119;
                case "pintglassprop": return BASE_ID + 1120;
                case "toyboat": return BASE_ID + 1121;
                case "woolyhat": return BASE_ID + 1122;
                case "peppergrinder": return BASE_ID + 1123;
                case "pubwomancloth": return BASE_ID + 1124;
                
                // Model Village items (1131-1143)
                case "miniperson variant - child": return BASE_ID + 1131;
                case "miniperson variant - jumpsuit": return BASE_ID + 1132;
                case "miniperson variant - gardener": return BASE_ID + 1133;
                case "minishovelprop": return BASE_ID + 1134;
                case "flowerpoppy": return BASE_ID + 1135;
                case "miniperson variant - old woman": return BASE_ID + 1136;
                case "miniphonedoorprop": return BASE_ID + 1137;
                case "minimailpillarprop": return BASE_ID + 1138;
                case "miniperson variant - postie": return BASE_ID + 1139;
                case "miniperson variant - vestman": return BASE_ID + 1140;
                case "miniperson": return BASE_ID + 1141;
                case "timberhandleprop": return BASE_ID + 1142;
                case "goldenbell": return BASE_ID + 1143;
                case "miniperson variant - goose": return BASE_ID + 1144;
                
                default: 
                    Log.LogWarning("[PICKUP] Unknown item not mapped: " + itemName);
                    return null;
            }
        }

        /// <summary>
        /// Get location ID for a first-time drag item
        /// </summary>
        public static long? GetDragLocationId(string itemName)
        {
            string lowerName = itemName.ToLower().Trim();
            
            // Strip (Clone) suffix if present (happens after area reset)
            if (lowerName.Contains("(clone)"))
            {
                lowerName = lowerName.Replace("(clone)", "").Trim();
            }
            
            
            // Check for unique tracked items FIRST (before stripping underscore suffix)
            // Umbrellas: umbrella_1 (black), umbrella_2 (rainbow), umbrella_3 (red)
            switch (lowerName)
            {
                case "umbrella_1": return BASE_ID + 1221;  // Black umbrella
                case "umbrella_2": return BASE_ID + 1228;  // Rainbow umbrella  
                case "umbrella_3": return BASE_ID + 1229;  // Red umbrella
                
                // Topsoil bags
                case "top_1": return BASE_ID + 1450;
                case "top_2": return BASE_ID + 1451;
                case "top_3": return BASE_ID + 1452;
            }
            
            // Strip trailing numbers after underscore for non-tracked items (e.g., "item_1" -> "item")
            int underscoreIdx = lowerName.LastIndexOf('_');
            if (underscoreIdx > 0 && underscoreIdx < lowerName.Length - 1)
            {
                string afterUnderscore = lowerName.Substring(underscoreIdx + 1);
                if (int.TryParse(afterUnderscore, out _))
                {
                    lowerName = lowerName.Substring(0, underscoreIdx);
                }
            }
            
            switch (lowerName)
            {
                // Garden drags (1201-1216)
                case "rake": return BASE_ID + 1201;
                case "basket": return BASE_ID + 1202;
                case "coolbox": return BASE_ID + 1203;
                case "shovel": return BASE_ID + 1205;
                case "pumpkin": return BASE_ID + 1206;
                case "pumpkin (2)": return BASE_ID + 1207;
                case "pumpkin (3)": return BASE_ID + 1208;
                case "pumpkin (4)": return BASE_ID + 1209;
                case "wateringcan": return BASE_ID + 1210;
                case "gumboot (1)": return BASE_ID + 1211;
                case "gumboot (2)": return BASE_ID + 1212;
                case "gardenersign": return BASE_ID + 1213;
                case "cratewooden": return BASE_ID + 1214;
                case "boltbent": return BASE_ID + 1215;  // fence bolt at game start
                case "mallet": return BASE_ID + 1216;   // groundskeeper's mallet
                
                // High Street drags (1220-1227)
                case "basketprop": return BASE_ID + 1220;
                case "umbrella": return BASE_ID + 1221;
                case "pushbroomprop": return BASE_ID + 1222;
                case "broom_headseperate": return BASE_ID + 1223;
                case "dustbin": return BASE_ID + 1224;
                case "babydoll": return BASE_ID + 1225;
                case "pricinggun": return BASE_ID + 1226;
                case "addingmachine": return BASE_ID + 1227;
                
                // Back Gardens drags (1240-1250)
                case "roseboxprop": return BASE_ID + 1240;
                case "cricketbat": return BASE_ID + 1241;
                case "teapot": return BASE_ID + 1242;
                case "clippers": return BASE_ID + 1243;
                case "duckstatueprop": return BASE_ID + 1244;
                case "frogstatue": return BASE_ID + 1245;
                case "jeremyfish": return BASE_ID + 1246;
                case "messysignprop": return BASE_ID + 1247;
                case "drawer": return BASE_ID + 1248;
                case "jugenamel": return BASE_ID + 1249;
                case "cleansignprop": return BASE_ID + 1250;
                
                // Pub drags (1270-1280)
                case "tackleboxprop": return BASE_ID + 1270;
                case "coneprop": return BASE_ID + 1271;
                case "exitparcel": return BASE_ID + 1272;
                case "stealthbox": return BASE_ID + 1273;
                case "pubnogoose": return BASE_ID + 1274;
                case "portablestool": return BASE_ID + 1275;
                case "dartboard": return BASE_ID + 1276;
                case "mopbucket": return BASE_ID + 1277;
                case "mopprop": return BASE_ID + 1278;
                case "infinitebox": return BASE_ID + 1279;
                case "pail": return BASE_ID + 1280;
                
                // Model Village drags (1290-1295)
                case "minibenchprop": return BASE_ID + 1290;
                case "minipump":
                case "minipumpprop": return BASE_ID + 1291;
                case "minibenchstreetprop": return BASE_ID + 1292;
                case "birdbathprop": return BASE_ID + 1293;
                case "easelprop": return BASE_ID + 1294;
                case "sunloungeprop": return BASE_ID + 1295;
                
                default:
                    Log.LogWarning("[DRAG] Unknown item not mapped: " + itemName);
                    return null;
            }
        }

        /// <summary>
        /// Get location ID for a first-time interaction
        /// </summary>
        public static long? GetInteractionLocationId(string interactionName)
        {
            switch (interactionName)
            {
                // Garden interactions (1301-1303)
                case "BikeBell": return BASE_ID + 1301;      // Ring the bike bell
                case "GardenTap": return BASE_ID + 1302;     // Turn on the tap in the Garden
                case "Sprinkler": return BASE_ID + 1303;     // Turn on the sprinkler
                case "ShortRadio": return BASE_ID + 1304;   // Short out the garden radio
                case "LockedIn": return BASE_ID + 1305;   // Lock the groundskeeper in the garden
                
                // Hub interactions (1306, 1500)
                case "IntroGate": return BASE_ID + 1306;     // Open the intro gate
                case "WellDrop": return BASE_ID + 1500;      // Drop something in the well
                
                // High Street interactions (1310-1317)
                case "BreakBoards": return BASE_ID + 1310;   // Break through the boards to the Back Gardens
                case "UnplugRadio": return BASE_ID + 1311;   // Unplug the shop's radio
                case "UmbrellaStand1": return BASE_ID + 1313; // Open Umbrella (Black)
                case "UmbrellaStand2": return BASE_ID + 1315; // Open Umbrella (Red)
                case "UmbrellaStand3": return BASE_ID + 1314; // Open Umbrella (Rainbow)
                case "WimpLacesLeft": return BASE_ID + 1316;  // Untie Boy's Laces (Left)
                case "WimpLacesRight": return BASE_ID + 1317; // Untie Boy's Laces (Right)
                case "Football": return BASE_ID + 1318;         // Boop the football
                
                // Back Gardens interactions (1320-1325)
                case "GardenBell": return BASE_ID + 1320;    // Ring the bell in the Back Gardens
                case "WindChimes": return BASE_ID + 1321;    // Fallback for wind chimes
                case "Windmill": return BASE_ID + 1322;      // Spin the windmill
                case "SpinPurpleFlower": return BASE_ID + 1323;  // Spin the purple flower
                case "BreakTrellis": return BASE_ID + 1324;  // Break through the trellis
                case "SpinSunflower": return BASE_ID + 1325; // Spin the sunflower
                case "SpinFlower": return BASE_ID + 1323;    // Fallback (maps to purple)
                case "Topiary": return BASE_ID + 1326;      // Mess with topiary
                case "Topiary 2": return BASE_ID + 1327;    // Make the woman fix the topiary
                case "Pose as Duck": return BASE_ID + 1328; // Pose as a duck statue
                
                // Wind Chimes individual notes (1340-1346) - left to right: G, F, E, D, C, B, A
                case "WindChimeG": return BASE_ID + 1340;    // Leftmost chime
                case "WindChimeF": return BASE_ID + 1341;
                case "WindChimeE": return BASE_ID + 1342;
                case "WindChimeD": return BASE_ID + 1343;
                case "WindChimeC": return BASE_ID + 1344;
                case "WindChimeB": return BASE_ID + 1345;
                case "WindChimeA": return BASE_ID + 1346;    // Rightmost chime
                
                // Pub interactions (1330-1339)
                case "VanDoorLeft": return BASE_ID + 1330;   // Close Van Door (Left)
                case "VanDoorRight": return BASE_ID + 1331;  // Close Van Door (Right)
                case "BurlyLacesLeft": return BASE_ID + 1332; // Untie Burly Man's Laces (Left)
                case "BurlyLacesRight": return BASE_ID + 1333; // Untie Burly Man's Laces (Right)
                case "PubTap": return BASE_ID + 1334;        // Turn on the tap in the Pub
                case "TripBurly": return BASE_ID + 1335;        // Trip the burly man
                
                default:
                    return null;
            }
        }
        
        /// <summary>
        /// Get human-readable location name from location ID
        /// </summary>
        public static string GetLocationName(long locationId)
        {
            long offset = locationId - BASE_ID;
            
            // Goals (1-80)
            switch (offset)
            {
                case 1: return "Get into the garden";
                case 2: return "Get the groundskeeper wet";
                case 3: return "Steal the groundskeeper's keys";
                case 4: return "Make the groundskeeper wear his sun hat";
                case 5: return "Rake in the lake";
                case 6: return "Have a picnic";
                case 7: return "Make the groundskeeper hammer his thumb";
                case 10: return "Break the broom";
                case 11: return "Trap the boy in the phone booth";
                case 12: return "Make the boy wear the wrong glasses";
                case 13: return "Make someone buy back their own stuff";
                case 14: return "Get on TV";
                case 15: return "Go shopping";
                case 16: return "Trap the shopkeeper in the garage";
                case 20: return "Make someone break the fancy vase";
                case 21: return "Help the woman dress up the bust";
                case 22: return "Make the man spit out his tea";
                case 23: return "Get dressed up with a ribbon";
                case 24: return "Make the man go barefoot";
                case 25: return "Do the washing";
                case 26: return "Make someone prune the prize rose";
                case 30: return "Get into the pub";
                case 31: return "Break the dartboard";
                case 32: return "Get the toy boat";
                case 33: return "Make the old man fall on his bum";
                case 34: return "Be awarded a flower";
                case 35: return "Steal a pint glass and drop it in the canal";
                case 36: return "Set the table";
                case 37: return "Drop a bucket on the burly man's head";
                case 40: return "Get into the model village";
                case 41: return "Steal the beautiful miniature golden bell";
                case 42: return "...and take it all the way back home";

                case 50: return "Lock the groundskeeper out of the garden";
                case 51: return "Cabbage picnic";
                case 52: return "Trip the boy in the puddle";
                case 53: return "Make the scales go ding";
                case 54: return "Open an umbrella inside the TV shop";
                case 55: return "Make someone from outside the high street buy back their own stuff";
                case 56: return "Collect the five flowers";
                case 60: return "Trap the boy in the garage";
                case 61: return "Catch an object as it's thrown over the fence";
                case 62: return "Get thrown over the fence";
                case 63: return "Dress up the bust with things from outside the back gardens";
                case 64: return "Score a goal";
                case 65: return "Sail the toy boat under the bridge";
                case 66: return "Perform at the pub wearing a ribbon";
                case 67: return "Steal the old man's woolen hat";

                case 70: return "Complete Garden before noon";
                case 71: return "Complete High Street before noon";
                case 72: return "Complete Back Gardens before noon";
                case 73: return "Complete Pub before noon";

                case 80: return "100% Completion";

                // Milestone goals
                case 81: return "All Garden tasks complete";
                case 82: return "All High Street tasks complete";
                case 83: return "All Back Gardens tasks complete";
                case 84: return "All Pub tasks complete";
                case 85: return "All 'To Do (As Well)' tasks complete";
                case 86: return "All speedrun tasks complete";
                case 87: return "All speedrun tasks complete (Golden Bell)";
                case 88: return "All main task lists complete";
                case 89: return "All main task lists complete (Golden Bell)";
                case 90: return "All tasks complete";
                case 91: return "All tasks complete (Golden Bell)";
                case 92: return "All main task lists + 'To Do (As Well)' complete (Golden Bell)";
                case 93: return "Get into the Model Village (Golden Bell)";
                case 94: return "Complete the four final area tasks (Golden Bell)";
                
                // Pickups
                case 1002: return "Pickup Radio";
                case 1003: return "Pickup Trowel";
                case 1004: return "Pickup Keys";
                case 1006: return "Pickup Tulip";
                case 1007: return "Pickup Apple 1";
                case 1008: return "Pickup Jam";
                case 1009: return "Pickup Picnic Mug";
                case 1010: return "Pickup Thermos";
                case 1011: return "Pickup Sandwich (Right)";
                case 1012: return "Pickup Sandwich (Left)";
                case 1014: return "Pickup Sun Hat";
                case 1015: return "Pickup Drink Can";
                case 1016: return "Pickup Tennis Ball";
                case 1017: return "Pickup Groundskeeper's Hat";
                case 1018: return "Pickup Apple 2";
    
                case 1021: return "Pickup Boy's Glasses";
                case 1022: return "Pickup Horn-Rimmed Glasses";
                case 1023: return "Pickup Red Glasses";
                case 1024: return "Pickup Sunglasses";
                case 1025: return "Pickup Loo Paper";
                case 1026: return "Pickup Toy Car";
                case 1027: return "Pickup Hairbrush";
                case 1028: return "Pickup Toothbrush";
                case 1029: return "Pickup Stereoscope";
                case 1030: return "Pickup Dish Soap Bottle";
                case 1031: return "Pickup Tinned Food (Blue)";
                case 1032: return "Pickup Tinned Food (Yellow)";
                case 1033: return "Pickup Tinned Food (Orange)";
                case 1034: return "Pickup Weed Tool";
                case 1035: return "Pickup Lily Flower";
                case 1036: return "Pickup Orange 1";
                case 1037: return "Pickup Shop Tomato 1";
                case 1038: return "Pickup Shop Carrot 1";
                case 1039: return "Pickup Cucumber 1";
                case 1040: return "Pickup Leek 1";
                case 1041: return "Pickup Toy Plane";
                case 1042: return "Pickup Pint Bottle (Hub)";
                case 1043: return "Pickup Spray Bottle";
                case 1044: return "Pickup Walkie Talkie 2";
                case 1045: return "Pickup Walkie Talkie 1";
                case 1046: return "Pickup Apple Core 1";
                case 1058: return "Pickup Apple Core 2";
                case 1047: return "Pickup Dustbin Lid";
                case 1048: return "Pickup Pint Bottle (High Street 1)";
                case 1049: return "Pickup Pint Bottle (High Street 2)";
                case 1050: return "Pickup Chalk";
                case 1051: return "Pickup Shop Tomato 2";
                case 1052: return "Pickup Orange 2";
                case 1053: return "Pickup Orange 3";
                case 1054: return "Pickup Shop Carrot 2";
                case 1055: return "Pickup Cucumber 2";
                case 1056: return "Pickup Leek 2";
                case 1057: return "Pickup Shop Carrot 3";
                case 1059: return "Pickup Leek 3";
                case 1060: return "Pickup Shop Tomato 3";
                case 1061: return "Pickup Cucumber 3";
                case 1062: return "Pickup Garden Fork";
    
                case 1071: return "Pickup Ribbon (Blue)";
                case 1072: return "Pickup Dummy";
                case 1073: return "Pickup Cricket Ball";
                case 1074: return "Pickup Bust Pipe";
                case 1075: return "Pickup Bust Hat";
                case 1076: return "Pickup Bust Glasses";
                case 1077: return "Pickup Slipper (Right)";
                case 1078: return "Pickup Slipper (Left)";
                case 1079: return "Pickup Tea Cup";
                case 1080: return "Pickup Newspaper";
                case 1081: return "Pickup Sock 1";
                case 1082: return "Pickup Sock 2";
                case 1083: return "Pickup Vase";
                case 1084: return "Pickup Ribbon (Red)";
                case 1085: return "Pickup Pot Stack";
                case 1086: return "Pickup Soap";
                case 1087: return "Pickup Paintbrush";
                case 1088: return "Pickup Vase Piece 1";
                case 1089: return "Pickup Vase Piece 2";
                case 1090: return "Pickup Bra";
                case 1093: return "Pickup Badminton Racket";
                case 1094: return "Pickup Rose";
    
                case 1101: return "Pickup Fishing Bobber";
                case 1102: return "Pickup Letter";
                case 1104: return "Pickup Plate 1";
                case 1105: return "Pickup Plate 2";
                case 1106: return "Pickup Plate 3";
                case 1107: return "Pickup Green Quoit 1";
                case 1108: return "Pickup Red Quoit 1";
                case 1109: return "Pickup Fork 1";
                case 1110: return "Pickup Fork 2";
                case 1111: return "Pickup Knife 1";
                case 1112: return "Pickup Knife 2";
                case 1113: return "Pickup Cork";
                case 1114: return "Pickup Candlestick";
                case 1115: return "Pickup Flower for Vase";
                case 1116: return "Pickup Dart 1";
                case 1117: return "Pickup Dart 2";
                case 1118: return "Pickup Dart 3";
                case 1119: return "Pickup Harmonica";
                case 1120: return "Pickup Pint Glass";
                case 1121: return "Pickup Toy Boat";
                case 1122: return "Pickup Old Man's Woolen Hat";
                case 1123: return "Pickup Pepper Grinder";
                case 1124: return "Pickup Pub Woman's Cloth";
                case 1125: return "Pickup Green Quoit 2";
                case 1126: return "Pickup Green Quoit 3";
                case 1127: return "Pickup Red Quoit 2";
                case 1128: return "Pickup Red Quoit 3";

                case 1131: return "Pickup Mini Person (Child)";
                case 1132: return "Pickup Mini Person (Jumpsuit)";
                case 1133: return "Pickup Mini Person (Gardener)";
                case 1134: return "Pickup Mini Shovel";
                case 1135: return "Pickup Poppy Flower";
                case 1136: return "Pickup Mini Person (Old Woman)";
                case 1137: return "Pickup Mini Phone Door";
                case 1138: return "Pickup Mini Mail Pillar";
                case 1139: return "Pickup Mini Person (Postie)";
                case 1140: return "Pickup Mini Person (Vestman)";
                case 1141: return "Pickup Mini Person";
                case 1144: return "Pickup Mini Goose";
                case 1142: return "Pickup Timber Handle";
                case 1143: return "Pickup Golden Bell";
                
                // Drags
                case 1201: return "Drag Rake";
                case 1202: return "Drag Picnic Basket";
                case 1203: return "Drag Esky";
                case 1205: return "Drag Shovel";
                case 1206: return "Drag Pumpkin 1";
                case 1207: return "Drag Pumpkin 2";
                case 1208: return "Drag Pumpkin 3";
                case 1209: return "Drag Pumpkin 4";
                case 1210: return "Drag Watering Can";
                case 1211: return "Drag Gumboot 1";
                case 1212: return "Drag Gumboot 2";
                case 1213: return "No Goose Sign (Garden)";
                case 1214: return "Drag Wooden Crate";
                case 1215: return "Drag Fence Bolt";
                case 1216: return "Drag Mallet";
    
                case 1220: return "Drag Shopping Basket";
                case 1221: return "Drag Umbrella (Black)";
                case 1222: return "Drag Push Broom";
                case 1223: return "Drag Broken Broom Head";
                case 1224: return "Drag Dustbin";
                case 1225: return "Drag Baby Doll";
                case 1226: return "Drag Pricing Gun";
                case 1227: return "Drag Adding Machine";
                case 1228: return "Drag Umbrella (Rainbow)";
                case 1229: return "Drag Umbrella (Red)";
    
                case 1240: return "Drag Rose Box";
                case 1241: return "Drag Cricket Bat";
                case 1242: return "Drag Tea Pot";
                case 1243: return "Drag Clippers";
                case 1244: return "Drag Duck Statue";
                case 1245: return "Drag Frog Statue";
                case 1246: return "Drag Jeremy Fish";
                case 1247: return "Drag No Goose Sign (Messy)";
                case 1248: return "Drag Drawer";
                case 1249: return "Drag Enamel Jug";
                case 1250: return "Drag No Goose Sign (Clean)";
    
                case 1270: return "Drag Tackle Box";
                case 1271: return "Drag Traffic Cone";
                case 1272: return "Drag Parcel";
                case 1273: return "Drag Stealth Box";
                case 1274: return "Drag No Goose Sign (Pub)";
                case 1275: return "Drag Portable Stool";
                case 1276: return "Drag Dartboard";
                case 1277: return "Drag Mop Bucket";
                case 1278: return "Drag Mop";
                case 1279: return "Drag Delivery Box";
                case 1280: return "Drag Bucket";
    
                case 1290: return "Drag Mini Bench";
                case 1291: return "Drag Mini Pump";
                case 1292: return "Drag Mini Street Bench";
                case 1293: return "Drag Mini Birdbath";
                case 1294: return "Drag Mini Easel";
                case 1295: return "Drag Mini Sun Lounge";
                
                // Interactions (1301-1350)
                case 1301: return "Ring Bell";
                case 1302: return "Honk at Speaker";
                case 1303: return "Doorbell";
                case 1304: return "Garden Gate";
                case 1305: return "Greenhouse Window";
                case 1306: return "Intro Gate";
                case 1307: return "TV Remote";
                case 1500: return "Well Drop";
                case 1308: return "Open Umbrella (Black)";
                case 1309: return "Turn on Tap";
                case 1310: return "Open Phone Booth";
                case 1311: return "Spin Propeller";
                case 1312: return "Kick Football";
                case 1313: return "Open Umbrella (Black)";
                case 1314: return "Open Umbrella (Rainbow)";
                case 1315: return "Open Umbrella (Red)";
                case 1316: return "Honk at Wimp";
                case 1317: return "Honk at Burly Kid";
                case 1318: return "Kick Football";
                case 1319: return "Turn on Sprinkler";
                case 1320: return "Untie Wimp's Left Laces";
                case 1321: return "Untie Wimp's Right Laces";
                case 1322: return "Untie Pub Man's Left Laces";
                case 1323: return "Untie Pub Man's Right Laces";
                case 1324: return "Spin Windmill";
                case 1325: return "Wheel Barrow Handle";
                case 1326: return "Spin Purple Flower";
                case 1327: return "Spin Sunflower";
                case 1328: return "Spin TV Dial";
                case 1329: return "Ring Service Bell";
                case 1330: return "Close Van Door (Left)";
                case 1331: return "Close Van Door (Right)";
                case 1332: return "Untie Burly Man's Left Laces";
                case 1333: return "Untie Burly Man's Right Laces";
                case 1334: return "Turn on Pub Tap";
                case 1340: return "Wind Chime G";
                case 1341: return "Wind Chime F";
                case 1342: return "Wind Chime E";
                case 1343: return "Wind Chime D";
                case 1344: return "Wind Chime C";
                case 1345: return "Wind Chime B";
                case 1346: return "Wind Chime A";
                
                // Sandcastle pecks (1350-1384) - 35 raw pecks
                case 1350: return "Peck Model Church Doorway 1";
                case 1351: return "Peck Model Church Doorway 2";
                case 1352: return "Peck Model Church Doorway 3";
                case 1353: return "Peck Model Church Doorway 4";
                case 1354: return "Peck Model Church Doorway 5";
                case 1355: return "Peck Model Church Doorway 6";
                case 1356: return "Peck Model Church Doorway 7";
                case 1357: return "Peck Model Church Doorway 8";
                case 1358: return "Peck Model Church Doorway 9";
                case 1359: return "Peck Model Church Doorway 10";
                case 1360: return "Peck Model Church Doorway 11";
                case 1361: return "Peck Model Church Doorway 12";
                case 1362: return "Peck Model Church Doorway 13";
                case 1363: return "Peck Model Church Doorway 14";
                case 1364: return "Peck Model Church Doorway 15";
                case 1365: return "Peck Model Church Doorway 16";
                case 1366: return "Peck Model Church Doorway 17";
                case 1367: return "Peck Model Church Doorway 18";
                case 1368: return "Peck Model Church Doorway 19";
                case 1369: return "Peck Model Church Tower 1";
                case 1370: return "Peck Model Church Tower 2";
                case 1371: return "Peck Model Church Tower 3";
                case 1372: return "Peck Model Church Tower 4";
                case 1373: return "Peck Model Church Tower 5";
                case 1374: return "Peck Model Church Tower 6";
                case 1375: return "Peck Model Church Tower 7";
                case 1376: return "Peck Model Church Tower 8";
                case 1377: return "Peck Model Church Tower 9";
                case 1378: return "Peck Model Church Tower 10";
                case 1379: return "Peck Model Church Tower 11";
                case 1380: return "Peck Model Church Tower 12";
                case 1381: return "Peck Model Church Tower 13";
                case 1382: return "Peck Model Church Tower 14";
                case 1383: return "Peck Model Church Tower 15";
                case 1384: return "Peck Model Church Tower 16";
                
                case 1390: return "Peck Model Church Doorway";
                case 1391: return "Peck Model Church Tower";
                
                // Renamed pickups (1401-1452)
                case 1401: return "Pickup Carrot 1";
                case 1402: return "Pickup Carrot 2";
                case 1403: return "Pickup Carrot 3";
                case 1404: return "Pickup Carrot 4";
                case 1405: return "Pickup Carrot 5";
                case 1406: return "Pickup Carrot 6";
                case 1407: return "Pickup Carrot 7";
                case 1408: return "Pickup Carrot 8";
                case 1409: return "Pickup Carrot 9";
                case 1410: return "Pickup Carrot 10";
                case 1411: return "Pickup Carrot 11";
                case 1412: return "Pickup Carrot 12";
                case 1413: return "Pickup Carrot 13";
                case 1421: return "Pickup Boxed Pub Tomato 1";
                case 1422: return "Pickup Boxed Pub Tomato 2";
                case 1423: return "Pickup Boxed Pub Tomato 3";
                case 1424: return "Pickup Boxed Pub Tomato 4";
                case 1425: return "Pickup Boxed Pub Tomato 5";
                case 1426: return "Pickup Boxed Pub Tomato 6";
                case 1427: return "Pickup Boxed Pub Tomato 7";
                case 1428: return "Pickup Boxed Pub Tomato 8";
                case 1429: return "Pickup Boxed Pub Tomato 9";
                case 1430: return "Pickup Pub Tomato 1";
                case 1431: return "Pickup Pub Tomato 2";
                case 1440: return "Pickup Boot (Hub)";
                case 1441: return "Pickup Boot (Start)";
                case 1450: return "Pickup Topsoil Bag 1";
                case 1451: return "Pickup Topsoil Bag 2";
                case 1452: return "Pickup Topsoil Bag 3";
            }
            
            // Generic fallbacks for ID ranges
            if (offset >= 1001 && offset <= 1199)
                return "Pickup Item";
            if (offset >= 1201 && offset <= 1299)
                return "Drag Item";
            if (offset >= 1301 && offset <= 1399)
                return "Interaction";
            
            return "Location " + offset;
        }
    }
}