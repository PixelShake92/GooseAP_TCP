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
                case 120: return "Groundskeeper Soul";
                case 121: return "Boy Soul";
                case 122: return "TV Shop Owner Soul";
                case 123: return "Market Lady Soul";
                case 124: return "Tidy Neighbour Soul";
                case 125: return "Messy Neighbour Soul";
                case 126: return "Burly Man Soul";
                case 127: return "Old Man Soul";
                case 128: return "Pub Lady Soul";
                case 129: return "Fancy Ladies Soul";
                case 130: return "Cook Soul";
                
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
                case 400: return "Carrot Soul";
                case 401: return "Tomato Soul";
                case 402: return "Pumpkin Soul";
                case 403: return "Topsoil Bag Soul";
                case 404: return "Quoit Soul";
                case 405: return "Plate Soul";
                case 406: return "Orange Soul";
                case 407: return "Leek Soul";
                case 408: return "Cucumber Soul";
                // case 409: return "Dart Soul";
                case 410: return "Umbrella Soul";
                case 411: return "Tinned Food Soul";
                case 412: return "Sock Soul";
                case 413: return "Pint Bottle Soul";
                case 414: return "Knife Soul";
                case 415: return "Gumboot Soul";
                case 416: return "Fork Soul";
                case 417: return "Vase Piece Soul";
                case 418: return "Apple Core Soul";
                case 419: return "Apple Soul";
                case 420: return "Sandwich Soul";
                case 421: return "Slipper Soul";
                case 422: return "Bow Soul";
                case 423: return "Walkie Talkie Soul";
                case 424: return "Boot Soul";
                case 425: return "Mini Person Soul";
                
                // Garden One-Off Prop Souls (500-519)
                case 500: return "Radio Soul";
                case 501: return "Trowel Soul";
                case 502: return "Keys Soul";
                case 503: return "Tulip Soul";
                case 504: return "Jam Soul";
                case 505: return "Picnic Mug Soul";
                case 506: return "Thermos Soul";
                case 507: return "Straw Hat Soul";
                case 508: return "Drink Can Soul";
                case 509: return "Tennis Ball Soul";
                case 510: return "Gardener Hat Soul";
                case 511: return "Rake Soul";
                case 512: return "Picnic Basket Soul";
                case 513: return "Esky Soul";
                case 514: return "Shovel Soul";
                case 515: return "Watering Can Soul";
                //case 516: return "Fence Bolt Soul";
                case 517: return "Mallet Soul";
                case 518: return "Wooden Crate Soul";
                
                // High Street One-Off Prop Souls (520-543)
                case 520: return "Boy's Glasses Soul";
                case 521: return "Horn-Rimmed Glasses Soul";
                case 522: return "Red Glasses Soul";
                case 523: return "Sunglasses Soul";
                case 524: return "Toilet Paper Soul";
                case 525: return "Toy Car Soul";
                case 526: return "Hairbrush Soul";
                case 527: return "Toothbrush Soul";
                case 528: return "Stereoscope Soul";
                case 529: return "Dish Soap Bottle Soul";
                case 530: return "Spray Bottle Soul";
                case 531: return "Weed Tool Soul";
                case 532: return "Lily Flower Soul";
                case 533: return "Fusilage Soul";
                // case 534: return "Coin Soul";
                case 535: return "Chalk Soul";
                case 536: return "Dustbin Lid Soul";
                case 537: return "Shopping Basket Soul";
                case 538: return "Push Broom Soul";
                case 540: return "Dustbin Soul";
                case 541: return "Baby Doll Soul";
                case 542: return "Pricing Gun Soul";
                case 543: return "Adding Machine Soul";
                case 545: return "Garden Fork Soul";
                
                // Back Gardens One-Off Prop Souls (550-574)
                case 550: return "Dummy Soul";
                case 551: return "Cricket Ball Soul";
                case 552: return "Bust Pipe Soul";
                case 553: return "Bust Hat Soul";
                case 554: return "Bust Glasses Soul";
                case 555: return "Tea Cup Soul";
                case 556: return "Newspaper Soul";
                case 557: return "Badminton Racket Soul";
                case 558: return "Pot Stack Soul";
                case 559: return "Soap Soul";
                case 560: return "Paintbrush Soul";
                case 561: return "Vase Soul";
                case 562: return "Bra Soul";
                case 563: return "Rose Soul";
                // Removing Rose Box Soul until I can solve the physics issues with it
                // case 564: return "Rose Box Soul";
                case 565: return "Cricket Bat Soul";
                case 566: return "Tea Pot Soul";
                case 567: return "Clippers Soul";
                case 568: return "Duck Statue Soul";
                case 569: return "Frog Statue Soul";
                case 570: return "Jeremy Fish Soul";
                case 571: return "Messy Sign Soul";
                case 572: return "Drawer Soul";
                case 573: return "Enamel Jug Soul";
                case 574: return "Clean Sign Soul";
                
                // Pub One-Off Prop Souls (580-601)
                case 580: return "Fishing Bobber Soul";
                case 581: return "Exit Letter Soul";
                case 582: return "Pint Glass Soul";
                case 583: return "Toy Boat Soul";
                case 584: return "Wooly Hat Soul";
                case 585: return "Pepper Grinder Soul";
                case 586: return "Pub Cloth Soul";
                case 587: return "Cork Soul";
                case 588: return "Candlestick Soul";
                case 589: return "Flower for Vase Soul";
                case 590: return "Harmonica Soul";
                case 591: return "Tackle Box Soul";
                case 592: return "Traffic Cone Soul";
                case 593: return "Exit Parcel Soul";
                case 594: return "Stealth Box Soul";
                case 595: return "No Goose Sign Soul";
                case 596: return "Portable Stool Soul";
                case 597: return "Dartboard Soul";
                case 598: return "Mop Bucket Soul";
                case 599: return "Mop Soul";
                case 600: return "Delivery Box Soul";
                case 601: return "Burly Mans Bucket Soul";
                
                // Model Village One-Off Prop Souls (610-620)
                case 610: return "Mini Mail Pillar Soul";
                case 611: return "Mini Phone Door Soul";
                case 612: return "Mini Shovel Soul";
                case 613: return "Poppy Flower Soul";
                case 614: return "Timber Handle Soul";
                case 615: return "Birdbath Soul";
                case 616: return "Easel Soul";
                case 617: return "Mini Bench Soul";
                case 618: return "Mini Pump Soul";
                case 620: return "Sun Lounge Soul";
                case 621: return "Golden Bell Soul";
                
                // Victory
                case 999: return "Golden Bell";
                
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
                case "BikeBell": return BASE_ID + 1301;      // Ring bike bell
                case "GardenTap": return BASE_ID + 1302;     // Turn on garden water tank tap
                case "Sprinkler": return BASE_ID + 1303;     // Turn on sprinkler
                
                // Hub interactions (1306, 1500)
                case "IntroGate": return BASE_ID + 1306;     // Open intro gate at game start
                case "WellDrop": return BASE_ID + 1500;      // Drop something in the well
                
                // High Street interactions (1310-1317)
                case "BreakBoards": return BASE_ID + 1310;   // Break through fence boards
                case "UnplugRadio": return BASE_ID + 1311;   // Unplug the radio
                case "UmbrellaStand1": return BASE_ID + 1313; // Open black umbrella
                case "UmbrellaStand2": return BASE_ID + 1315; // Open red umbrella
                case "UmbrellaStand3": return BASE_ID + 1314; // Open rainbow umbrella
                case "WimpLacesLeft": return BASE_ID + 1316;  // Untie boy's left laces
                case "WimpLacesRight": return BASE_ID + 1317; // Untie boy's right laces
                
                // Back Gardens interactions (1320-1325)
                case "GardenBell": return BASE_ID + 1320;    // Ring big bell (makes man spit tea)
                case "WindChimes": return BASE_ID + 1321;    // Fallback for wind chimes
                case "Windmill": return BASE_ID + 1322;      // Spin the windmill
                case "SpinPurpleFlower": return BASE_ID + 1323;  // Spin the purple flower
                case "BreakTrellis": return BASE_ID + 1324;  // Break through trellis fence
                case "SpinSunflower": return BASE_ID + 1325; // Spin the sunflower
                case "SpinFlower": return BASE_ID + 1323;    // Fallback (maps to purple)
                
                // Wind Chimes individual notes (1340-1346) - left to right: G, F, E, D, C, B, A
                case "WindChimeG": return BASE_ID + 1340;    // Leftmost chime
                case "WindChimeF": return BASE_ID + 1341;
                case "WindChimeE": return BASE_ID + 1342;
                case "WindChimeD": return BASE_ID + 1343;
                case "WindChimeC": return BASE_ID + 1344;
                case "WindChimeB": return BASE_ID + 1345;
                case "WindChimeA": return BASE_ID + 1346;    // Rightmost chime
                
                // Pub interactions (1330-1339)
                case "VanDoorLeft": return BASE_ID + 1330;   // Close van door (left)
                case "VanDoorRight": return BASE_ID + 1331;  // Close van door (right)
                case "BurlyLacesLeft": return BASE_ID + 1332; // Untie burly man's left laces
                case "BurlyLacesRight": return BASE_ID + 1333; // Untie burly man's right laces
                case "PubTap": return BASE_ID + 1334;        // Turn on pub sink tap
                
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
                case 1: return "Get into the Garden";
                case 2: return "Get the Groundskeeper Wet";
                case 3: return "Steal the Groundskeeper's Keys";
                case 4: return "Make the Groundskeeper Wear his Sun Hat";
                case 5: return "Rake in the Lake";
                case 6: return "Have a Picnic";
                case 7: return "Make the Groundskeeper Hammer his Thumb";
                case 10: return "Break the Broom";
                case 11: return "Trap the Boy in the Phone Booth";
                case 12: return "Make the Boy Wear the Wrong Glasses";
                case 13: return "Make Someone Buy Back Their Own Stuff";
                case 14: return "Get on TV";
                case 15: return "Go Shopping";
                case 16: return "Trap the Shopkeeper in the Garage";
                case 20: return "Make Someone Break the Fancy Vase";
                case 21: return "Help the Woman Dress Up the Bust";
                case 22: return "Make the Man Spit Out his Tea";
                case 23: return "Get Dressed Up with a Ribbon";
                case 24: return "Make the Man Go Barefoot";
                case 25: return "Do the Washing";
                case 26: return "Make Someone Prune the Prize Rose";
                case 30: return "Get into the Pub";
                case 31: return "Break the Dartboard";
                case 32: return "Get the Toy Boat";
                case 33: return "Make the Old Man Fall on his Bum";
                case 34: return "Be Awarded a Flower";
                case 35: return "Steal a Pint Glass and Drop it in the Canal";
                case 36: return "Set the Table";
                case 37: return "Drop a Bucket on the Burly Man's Head";
                case 40: return "Get into the Model Village";
                case 41: return "Steal the Beautiful Miniature Golden Bell";
                case 42: return "...and Take it All the Way Back Home";
                case 50: return "Lock the Groundskeeper Out of the Garden";
                case 51: return "Cabbage Picnic";
                case 52: return "Trip the Boy in the Puddle";
                case 53: return "Make the Shop Scales Go Ding";
                case 54: return "Open an Umbrella Inside the TV Shop";
                case 55: return "Make Someone From Outside High Street Buy Back Their Own Stuff";
                case 56: return "Collect the Five Flowers";
                case 60: return "Trap the Boy in the Garage";
                case 61: return "Catch an Object as it's Thrown Over the Fence";
                case 62: return "Get Thrown Over the Fence";
                case 63: return "Dress Up the Bust with Things from Outside the Back Garden";
                case 64: return "Score a Goal";
                case 65: return "Sail the Toy Boat Under a Bridge";
                case 66: return "Perform at the Pub Wearing a Ribbon";
                case 67: return "Steal the Old Man's Woolen Hat";
                case 70: return "Garden Speedrun";
                case 71: return "High Street Speedrun";
                case 72: return "Back Gardens Speedrun";
                case 73: return "Pub Speedrun";
                case 80: return "100% Completion";

                // Milestone goals
                case 81: return "All Garden Tasks Complete";
                case 82: return "All High Street Tasks Complete";
                case 83: return "All Back Gardens Tasks Complete";
                case 84: return "All Pub Tasks Complete";
                case 85: return "All To Do (As Well) Tasks Complete";
                case 86: return "All Speedrun Tasks Complete";
                case 87: return "All Speedrun Tasks Complete (Golden Bell Soul)";
                case 88: return "All Main Task Lists Complete";
                case 89: return "All Main Task Lists Complete (Golden Bell Soul)";
                case 90: return "All Tasks Complete";
                case 91: return "All Tasks Complete (Golden Bell Soul)";
                case 92: return "All Main Task Lists + To Do (As Well) Complete (Golden Bell Soul)";
                case 93: return "Get into the Model Village (Golden Bell Soul)";
                case 94: return "Complete the Four Final Area Tasks (Golden Bell Soul)";
                
                // Pickups - generic names since there are many
                case 1001: return "Pickup Boot";
                case 1002: return "Pickup Radio";
                case 1003: return "Pickup Trowel";
                case 1004: return "Pickup Keys";
                case 1005: return "Pickup Carrot";
                case 1006: return "Pickup Tulip";
                case 1007: return "Pickup Apple";
                case 1008: return "Pickup Jam";
                case 1009: return "Pickup Mug";
                case 1010: return "Pickup Thermos";
                case 1011: return "Pickup Sandwich (R)";
                case 1012: return "Pickup Sandwich (L)";
                case 1013: return "Pickup Fork";
                case 1014: return "Pickup Straw Hat";
                case 1015: return "Pickup Drink Can";
                case 1016: return "Pickup Tennis Ball";
                case 1017: return "Pickup Gardener Hat";
                case 1018: return "Pickup Apple";
                
                // Drags - generic names
                case 1201: return "Drag Shovel";
                case 1202: return "Drag Watering Can";
                case 1203: return "Drag Trug";
                case 1204: return "Drag Garden Gnome";
                case 1205: return "Drag Plant Pot";
                case 1206: return "Drag Rake";
                case 1207: return "Drag Sprinkler";
                case 1208: return "Drag Boot 1";
                case 1209: return "Drag Boot 2";
                case 1210: return "Drag Mallet";
                case 1211: return "Drag Wheelbarrow";
                case 1212: return "Drag Topsoil 1";
                case 1213: return "Drag Topsoil 2";
                case 1214: return "Drag Topsoil 3";
                case 1215: return "Drag Radio";
                case 1216: return "Drag Wooden Crate";
                case 1217: return "Drag Traffic Cone";
                case 1218: return "Drag Green Bottle";
                case 1219: return "Drag Bin";
                case 1220: return "Drag Broom";
                case 1221: return "Drag Dustpan";
                case 1222: return "Drag Bucket";
                case 1223: return "Drag Broken Broom Head";
                case 1224: return "Drag Golden Statue";
                case 1225: return "Drag Bust 1";
                case 1226: return "Drag Bust 2";
                case 1227: return "Drag Vase";
                case 1228: return "Drag Watering Can (Gardens)";
                case 1229: return "Drag Toy Boat";
                case 1230: return "Drag Pub Bucket";
                case 1231: return "Drag Pub Box";
                case 1280: return "Drag Burly Mans Bucket";
                
                // Model Village items
                case 1144: return "Pick up Mini Person (Goose)";
                
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
                case 1421: return "Pickup Pub Tomato 1";
                case 1422: return "Pickup Pub Tomato 2";
                case 1423: return "Pickup Pub Tomato 3";
                case 1424: return "Pickup Pub Tomato 4";
                case 1425: return "Pickup Pub Tomato 5";
                case 1426: return "Pickup Pub Tomato 6";
                case 1427: return "Pickup Pub Tomato 7";
                case 1428: return "Pickup Pub Tomato 8";
                case 1429: return "Pickup Pub Tomato 9";
                case 1430: return "Pickup Pub Tomato 10";
                case 1431: return "Pickup Pub Tomato 11";
                case 1440: return "Pickup Boot 1";
                case 1441: return "Pickup Boot 2";
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