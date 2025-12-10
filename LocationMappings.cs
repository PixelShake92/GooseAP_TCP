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
                case 100: return "Garden Access";
                case 101: return "High Street Access";
                case 102: return "Back Gardens Access";
                case 103: return "Pub Access";
                case 104: return "Model Village Access";
                case 110: return "Progressive Area";
                case 200: return "Mega Honk";
                case 201: return "Speedy Feet";
                case 202: return "Silent Steps";
                case 203: return "A Goose Day";
                case 300: return "Tired Goose";
                case 301: return "Clumsy Feet";
                case 302: return "Butterfingers";
                case 303: return "Suspicious Goose";
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
            
            Log.LogInfo("[ITEM LOOKUP] Raw: '" + itemName + "' | Cleaned: '" + lowerName + "'");
            
            switch (lowerName)
            {
                // Garden items (1001-1020)
                case "boot": return BASE_ID + 1001;
                case "radiosmall": return BASE_ID + 1002;
                case "trowel": return BASE_ID + 1003;
                case "keys": return BASE_ID + 1004;
                case "carrot": return BASE_ID + 1005;
                case "tulip": return BASE_ID + 1006;
                case "apple": return BASE_ID + 1007;
                case "jam": return BASE_ID + 1008;
                case "picnicmug": return BASE_ID + 1009;
                case "thermos (1)": return BASE_ID + 1010;
                case "sandwichr": return BASE_ID + 1011;
                case "sandwichl": return BASE_ID + 1012;
                case "forkgarden": return BASE_ID + 1013;
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
                case "lilyflower": return BASE_ID + 1035;
                case "orange": return BASE_ID + 1036;
                case "tomato (1)": return BASE_ID + 1037;
                case "carrotnogreen (1)": return BASE_ID + 1038;
                case "cucumber (1)": return BASE_ID + 1039;
                case "leek (1)": return BASE_ID + 1040;
                case "fusilage": return BASE_ID + 1041;
                case "pintbottle": return BASE_ID + 1042;
                case "spraybottle": return BASE_ID + 1043;
                case "walkietalkieb": return BASE_ID + 1044;
                case "walkietalkie": return BASE_ID + 1045;
                case "applecore": return BASE_ID + 1046;
                case "applecore (1)": return BASE_ID + 1058;
                case "dustbinlid": return BASE_ID + 1047;
                case "pintbottle (1)": return BASE_ID + 1048;
                case "coin": return BASE_ID + 1049;
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
                case "roseprop": return BASE_ID + 1094;  // Spawns when rose is pruned
                
                // Pub items (1101-1124)
                case "fishingbobberprop": return BASE_ID + 1101;
                case "exitletterprop":
                case "exitletter": return BASE_ID + 1102;
                case "pubtomato": return BASE_ID + 1103;
                case "plate": return BASE_ID + 1104;
                case "plate (1)": return BASE_ID + 1105;
                case "plate (2)": return BASE_ID + 1106;
                case "quoitgreen (2)": return BASE_ID + 1107;
                case "quoitred (1)": return BASE_ID + 1108;
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
                
                default: 
                    Log.LogWarning("[ITEM] Unknown item not mapped: " + itemName);
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
            
            // Strip trailing numbers after underscore (e.g., "item_1" -> "item")
            int underscoreIdx = lowerName.LastIndexOf('_');
            if (underscoreIdx > 0 && underscoreIdx < lowerName.Length - 1)
            {
                string afterUnderscore = lowerName.Substring(underscoreIdx + 1);
                if (int.TryParse(afterUnderscore, out _))
                {
                    lowerName = lowerName.Substring(0, underscoreIdx);
                }
            }
            
            Log.LogInfo("[DRAG LOOKUP] Raw: '" + itemName + "' | Cleaned: '" + lowerName + "'");
            
            switch (lowerName)
            {
                // Garden drags (1201-1216)
                case "rake": return BASE_ID + 1201;
                case "basket": return BASE_ID + 1202;
                case "coolbox": return BASE_ID + 1203;
                case "top": return BASE_ID + 1204;  // topsoil bag
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
                case "tomatobox": return BASE_ID + 1280;
                
                // Model Village drags (1290-1295)
                case "minibenchprop": return BASE_ID + 1290;
                case "minipumpprop": return BASE_ID + 1291;
                case "minibenchstreetprop": return BASE_ID + 1292;
                case "birdbathprop": return BASE_ID + 1293;
                case "easelprop": return BASE_ID + 1294;
                case "sunloungeprop": return BASE_ID + 1295;
                
                default:
                    Log.LogWarning("[DRAG] Unknown drag item not mapped: " + itemName);
                    return null;
            }
        }
    }
}