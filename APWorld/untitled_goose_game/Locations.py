from typing import Dict, NamedTuple
from BaseClasses import Location


class GooseGameLocationData(NamedTuple):
    id: int
    region: str


class GooseGameLocation(Location):
    game = "Untitled Goose Game"


BASE_ID = 119000000

# GOAL LOCATIONS - Completing to-do list objectives (IDs 1-80)

location_table: Dict[str, GooseGameLocationData] = {
    # Garden (7 goals)
    "Get into the garden": GooseGameLocationData(BASE_ID + 1, "Garden"),
    "Get the groundskeeper wet": GooseGameLocationData(BASE_ID + 2, "Garden"),
    "Steal the groundskeeper's keys": GooseGameLocationData(BASE_ID + 3, "Garden"),
    "Make the groundskeeper wear his sun hat": GooseGameLocationData(BASE_ID + 4, "Garden"),
    "Rake in the lake": GooseGameLocationData(BASE_ID + 5, "Garden"),
    "Have a picnic": GooseGameLocationData(BASE_ID + 6, "Garden"),
    "Make the groundskeeper hammer his thumb": GooseGameLocationData(BASE_ID + 7, "Garden"),
    
    # High Street (7 goals)
    "Break the broom": GooseGameLocationData(BASE_ID + 10, "High Street"),
    "Trap the boy in the phone booth": GooseGameLocationData(BASE_ID + 11, "High Street"),
    "Make the boy wear the wrong glasses": GooseGameLocationData(BASE_ID + 12, "High Street"),
    "Make someone buy back their own stuff": GooseGameLocationData(BASE_ID + 13, "High Street"),
    "Get on TV": GooseGameLocationData(BASE_ID + 14, "High Street"),
    "Go shopping": GooseGameLocationData(BASE_ID + 15, "High Street"),
    "Trap the shopkeeper in the garage": GooseGameLocationData(BASE_ID + 16, "High Street"),
    
    # Back Gardens (7 goals)
    "Make someone break the fancy vase": GooseGameLocationData(BASE_ID + 20, "Back Gardens"),
    "Help the woman dress up the bust": GooseGameLocationData(BASE_ID + 21, "Back Gardens"),
    "Make the man spit out his tea": GooseGameLocationData(BASE_ID + 22, "Back Gardens"),
    "Get dressed up with a ribbon": GooseGameLocationData(BASE_ID + 23, "Back Gardens"),
    "Make the man go barefoot": GooseGameLocationData(BASE_ID + 24, "Back Gardens"),
    "Do the washing": GooseGameLocationData(BASE_ID + 25, "Back Gardens"),
    "Make someone prune the prize rose": GooseGameLocationData(BASE_ID + 26, "Back Gardens"),
    
    # Pub (8 goals)
    "Get into the pub": GooseGameLocationData(BASE_ID + 30, "Pub"),
    "Break the dartboard": GooseGameLocationData(BASE_ID + 31, "Pub"),
    "Get the toy boat": GooseGameLocationData(BASE_ID + 32, "Pub"),
    "Make the old man fall on his bum": GooseGameLocationData(BASE_ID + 33, "Pub"),
    "Be awarded a flower": GooseGameLocationData(BASE_ID + 34, "Pub"),
    "Steal a pint glass": GooseGameLocationData(BASE_ID + 35, "Pub"),
    "Set the table": GooseGameLocationData(BASE_ID + 36, "Pub"),
    "Drop a bucket on the burly man's head": GooseGameLocationData(BASE_ID + 37, "Pub"),
    
    # Model Village (3 goals)
    "Get to the model village": GooseGameLocationData(BASE_ID + 40, "Model Village"),
    "Steal the golden bell": GooseGameLocationData(BASE_ID + 41, "Model Village"),
    "Complete the game": GooseGameLocationData(BASE_ID + 42, "Model Village"),
}

# Extra goals (post-game) - placed in their actual area regions
extra_locations: Dict[str, GooseGameLocationData] = {
    # Extra Garden goals
    "Lock the groundskeeper out of the garden": GooseGameLocationData(BASE_ID + 50, "Garden"),
    "Cabbage picnic": GooseGameLocationData(BASE_ID + 51, "Garden"),
    
    # Extra High Street goals
    "Trip the boy in the puddle": GooseGameLocationData(BASE_ID + 52, "High Street"),
    "Make the scales go ding": GooseGameLocationData(BASE_ID + 53, "High Street"),
    "Open an umbrella inside the TV shop": GooseGameLocationData(BASE_ID + 54, "High Street"),
    "Make someone from outside the high street buy back their own stuff": GooseGameLocationData(BASE_ID + 55, "High Street"),
    "Collect the five flowers": GooseGameLocationData(BASE_ID + 56, "High Street"),
    
    # Extra Back Gardens goals
    "Trap the boy in the garage": GooseGameLocationData(BASE_ID + 60, "Back Gardens"),
    "Catch an object as it's thrown over the fence": GooseGameLocationData(BASE_ID + 61, "Back Gardens"),
    "Get thrown over the fence": GooseGameLocationData(BASE_ID + 62, "Back Gardens"),
    "Dress up the bust with things from outside the back gardens": GooseGameLocationData(BASE_ID + 63, "Back Gardens"),
    "Score a goal": GooseGameLocationData(BASE_ID + 64, "Back Gardens"),
    
    # Extra Pub goals
    "Sail the toy boat under the bridge": GooseGameLocationData(BASE_ID + 65, "Pub"),
    "Perform at the pub wearing a ribbon": GooseGameLocationData(BASE_ID + 66, "Pub"),
    "Steal the old man's woolen hat": GooseGameLocationData(BASE_ID + 67, "Pub"),
}

# Speedrun goals
speedrun_locations: Dict[str, GooseGameLocationData] = {
    "Complete Garden before noon": GooseGameLocationData(BASE_ID + 70, "Garden"),
    "Complete High Street before noon": GooseGameLocationData(BASE_ID + 71, "High Street"),
    "Complete Back Gardens before noon": GooseGameLocationData(BASE_ID + 72, "Back Gardens"),
    "Complete Pub before noon": GooseGameLocationData(BASE_ID + 73, "Pub"),
}

# 100% completion
completion_location: Dict[str, GooseGameLocationData] = {
    "Complete all goals": GooseGameLocationData(BASE_ID + 80, "Model Village"),
}

# =============================================================================
# ITEM PICKUP LOCATIONS - First time picking up each item (IDs 1001-1150)
# =============================================================================

item_pickup_locations: Dict[str, GooseGameLocationData] = {
    # Garden items (1002-1020) - Boots tracked separately in unique_item_locations
    "Pick up Radio": GooseGameLocationData(BASE_ID + 1002, "Garden"),
    "Pick up Trowel": GooseGameLocationData(BASE_ID + 1003, "Garden"),
    "Pick up Keys": GooseGameLocationData(BASE_ID + 1004, "Garden"),
    "Pick up Carrot": GooseGameLocationData(BASE_ID + 1005, "Garden"),
    "Pick up Tulip": GooseGameLocationData(BASE_ID + 1006, "Garden"),
    "Pick up Apple": GooseGameLocationData(BASE_ID + 1007, "Garden"),
    "Pick up Jam": GooseGameLocationData(BASE_ID + 1008, "Garden"),
    "Pick up Picnic Mug": GooseGameLocationData(BASE_ID + 1009, "Garden"),
    "Pick up Thermos": GooseGameLocationData(BASE_ID + 1010, "Garden"),
    "Pick up Sandwich (Right)": GooseGameLocationData(BASE_ID + 1011, "Garden"),
    "Pick up Sandwich (Left)": GooseGameLocationData(BASE_ID + 1012, "Garden"),
    "Pick up Straw Hat": GooseGameLocationData(BASE_ID + 1014, "Garden"),
    "Pick up Drink Can": GooseGameLocationData(BASE_ID + 1015, "Garden"),
    "Pick up Tennis Ball": GooseGameLocationData(BASE_ID + 1016, "Garden"),
    "Pick up Gardener Hat": GooseGameLocationData(BASE_ID + 1017, "Garden"),
    "Pick up Apple 2": GooseGameLocationData(BASE_ID + 1018, "Garden"),
    
    # High Street items (1021-1056)
    "Pick up Boy's Glasses": GooseGameLocationData(BASE_ID + 1021, "High Street"),
    "Pick up Horn-Rimmed Glasses": GooseGameLocationData(BASE_ID + 1022, "High Street"),
    "Pick up Red Glasses": GooseGameLocationData(BASE_ID + 1023, "High Street"),
    "Pick up Sunglasses": GooseGameLocationData(BASE_ID + 1024, "High Street"),
    "Pick up Toilet Paper": GooseGameLocationData(BASE_ID + 1025, "High Street"),
    "Pick up Toy Car": GooseGameLocationData(BASE_ID + 1026, "High Street"),
    "Pick up Hairbrush": GooseGameLocationData(BASE_ID + 1027, "High Street"),
    "Pick up Toothbrush": GooseGameLocationData(BASE_ID + 1028, "High Street"),
    "Pick up Stereoscope": GooseGameLocationData(BASE_ID + 1029, "High Street"),
    "Pick up Dish Soap Bottle": GooseGameLocationData(BASE_ID + 1030, "High Street"),
    "Pick up Blue Can": GooseGameLocationData(BASE_ID + 1031, "High Street"),
    "Pick up Yellow Can": GooseGameLocationData(BASE_ID + 1032, "High Street"),
    "Pick up Orange Can": GooseGameLocationData(BASE_ID + 1033, "High Street"),
    "Pick up Weed Tool": GooseGameLocationData(BASE_ID + 1034, "High Street"),
    "Pick up Lily Flower": GooseGameLocationData(BASE_ID + 1035, "High Street"),
    "Pick up Orange": GooseGameLocationData(BASE_ID + 1036, "High Street"),
    "Pick up Tomato 1": GooseGameLocationData(BASE_ID + 1037, "High Street"),
    "Pick up Shop Carrot 1": GooseGameLocationData(BASE_ID + 1038, "High Street"),
    "Pick up Cucumber 1": GooseGameLocationData(BASE_ID + 1039, "High Street"),
    "Pick up Leek 1": GooseGameLocationData(BASE_ID + 1040, "High Street"),
    "Pick up Fusilage": GooseGameLocationData(BASE_ID + 1041, "High Street"),
    "Pick up Pint Bottle": GooseGameLocationData(BASE_ID + 1042, "High Street"),
    "Pick up Spray Bottle": GooseGameLocationData(BASE_ID + 1043, "High Street"),
    "Pick up Walkie Talkie B": GooseGameLocationData(BASE_ID + 1044, "High Street"),
    "Pick up Walkie Talkie": GooseGameLocationData(BASE_ID + 1045, "High Street"),
    "Pick up Apple Core": GooseGameLocationData(BASE_ID + 1046, "High Street"),
    "Pick up Apple Core 2": GooseGameLocationData(BASE_ID + 1058, "High Street"),
    "Pick up Dustbin Lid": GooseGameLocationData(BASE_ID + 1047, "High Street"),
    "Pick up Pint Bottle 2": GooseGameLocationData(BASE_ID + 1048, "High Street"),
    "Pick up Coin": GooseGameLocationData(BASE_ID + 1049, "High Street"),
    "Pick up Chalk": GooseGameLocationData(BASE_ID + 1050, "High Street"),
    "Pick up Tomato 2": GooseGameLocationData(BASE_ID + 1051, "High Street"),
    "Pick up Orange 2": GooseGameLocationData(BASE_ID + 1052, "High Street"),
    "Pick up Orange 3": GooseGameLocationData(BASE_ID + 1053, "High Street"),
    "Pick up Shop Carrot 2": GooseGameLocationData(BASE_ID + 1054, "High Street"),
    "Pick up Cucumber 2": GooseGameLocationData(BASE_ID + 1055, "High Street"),
    "Pick up Leek 2": GooseGameLocationData(BASE_ID + 1056, "High Street"),
    "Pick up Shop Carrot 3": GooseGameLocationData(BASE_ID + 1057, "High Street"),
    "Pick up Leek 3": GooseGameLocationData(BASE_ID + 1059, "High Street"),
    "Pick up Tomato 3": GooseGameLocationData(BASE_ID + 1060, "High Street"),
    "Pick up Cucumber 3": GooseGameLocationData(BASE_ID + 1061, "High Street"),
    
    # Back Gardens items (1071-1093)
    "Pick up Bow (Blue)": GooseGameLocationData(BASE_ID + 1071, "Back Gardens"),
    "Pick up Dummy": GooseGameLocationData(BASE_ID + 1072, "Back Gardens"),
    "Pick up Cricket Ball": GooseGameLocationData(BASE_ID + 1073, "Back Gardens"),
    "Pick up Bust Pipe": GooseGameLocationData(BASE_ID + 1074, "Back Gardens"),
    "Pick up Bust Hat": GooseGameLocationData(BASE_ID + 1075, "Back Gardens"),
    "Pick up Bust Glasses": GooseGameLocationData(BASE_ID + 1076, "Back Gardens"),
    "Pick up Right Slipper": GooseGameLocationData(BASE_ID + 1077, "Back Gardens"),
    "Pick up Left Slipper": GooseGameLocationData(BASE_ID + 1078, "Back Gardens"),
    "Pick up Tea Cup": GooseGameLocationData(BASE_ID + 1079, "Back Gardens"),
    "Pick up Newspaper": GooseGameLocationData(BASE_ID + 1080, "Back Gardens"),
    "Pick up Socks": GooseGameLocationData(BASE_ID + 1081, "Back Gardens"),
    "Pick up Socks 2": GooseGameLocationData(BASE_ID + 1082, "Back Gardens"),
    "Pick up Vase": GooseGameLocationData(BASE_ID + 1083, "Back Gardens"),
    "Pick up Bow": GooseGameLocationData(BASE_ID + 1084, "Back Gardens"),
    "Pick up Pot Stack": GooseGameLocationData(BASE_ID + 1085, "Back Gardens"),
    "Pick up Soap": GooseGameLocationData(BASE_ID + 1086, "Back Gardens"),
    "Pick up Paintbrush": GooseGameLocationData(BASE_ID + 1087, "Back Gardens"),
    "Pick up Broken Vase Piece 1": GooseGameLocationData(BASE_ID + 1088, "Back Gardens"),
    "Pick up Broken Vase Piece 2": GooseGameLocationData(BASE_ID + 1089, "Back Gardens"),
    "Pick up Right Strap": GooseGameLocationData(BASE_ID + 1090, "Back Gardens"),
    "Pick up Badminton Racket": GooseGameLocationData(BASE_ID + 1093, "Back Gardens"),
    "Pick up Rose": GooseGameLocationData(BASE_ID + 1094, "Back Gardens"),  # Spawns when rose is pruned
    
    # Pub items (1101-1128)
    "Pick up Fishing Bobber": GooseGameLocationData(BASE_ID + 1101, "Pub"),
    "Pick up Exit Letter": GooseGameLocationData(BASE_ID + 1102, "Pub"),
    # Pub Tomato removed - now tracked as unique Pub Tomato 1-10 in unique_item_locations
    "Pick up Plate": GooseGameLocationData(BASE_ID + 1104, "Pub"),
    "Pick up Plate 2": GooseGameLocationData(BASE_ID + 1105, "Pub"),
    "Pick up Plate 3": GooseGameLocationData(BASE_ID + 1106, "Pub"),
    "Pick up Green Quoit 1": GooseGameLocationData(BASE_ID + 1107, "Pub"),
    "Pick up Red Quoit 1": GooseGameLocationData(BASE_ID + 1108, "Pub"),
    "Pick up Fork": GooseGameLocationData(BASE_ID + 1109, "Pub"),
    "Pick up Fork 2": GooseGameLocationData(BASE_ID + 1110, "Pub"),
    "Pick up Knife": GooseGameLocationData(BASE_ID + 1111, "Pub"),
    "Pick up Knife 2": GooseGameLocationData(BASE_ID + 1112, "Pub"),
    "Pick up Cork": GooseGameLocationData(BASE_ID + 1113, "Pub"),
    "Pick up Candlestick": GooseGameLocationData(BASE_ID + 1114, "Pub"),
    "Pick up Flower for Vase": GooseGameLocationData(BASE_ID + 1115, "Pub"),
    "Pick up Dart 1": GooseGameLocationData(BASE_ID + 1116, "Pub"),
    "Pick up Dart 2": GooseGameLocationData(BASE_ID + 1117, "Pub"),
    "Pick up Dart 3": GooseGameLocationData(BASE_ID + 1118, "Pub"),
    "Pick up Harmonica": GooseGameLocationData(BASE_ID + 1119, "Pub"),
    "Pick up Pint Glass": GooseGameLocationData(BASE_ID + 1120, "Pub"),
    "Pick up Toy Boat": GooseGameLocationData(BASE_ID + 1121, "Pub"),
    "Pick up Wooly Hat": GooseGameLocationData(BASE_ID + 1122, "Pub"),
    "Pick up Pepper Grinder": GooseGameLocationData(BASE_ID + 1123, "Pub"),
    "Pick up Pub Woman's Cloth": GooseGameLocationData(BASE_ID + 1124, "Pub"),
    "Pick up Green Quoit 2": GooseGameLocationData(BASE_ID + 1125, "Pub"),
    "Pick up Green Quoit 3": GooseGameLocationData(BASE_ID + 1126, "Pub"),
    "Pick up Red Quoit 2": GooseGameLocationData(BASE_ID + 1127, "Pub"),
    "Pick up Red Quoit 3": GooseGameLocationData(BASE_ID + 1128, "Pub"),

    # Model Village items (1131-1143)
    "Pick up Mini Person (Child)": GooseGameLocationData(BASE_ID + 1131, "Model Village"),
    "Pick up Mini Person (Jumpsuit)": GooseGameLocationData(BASE_ID + 1132, "Model Village"),
    "Pick up Mini Person (Gardener)": GooseGameLocationData(BASE_ID + 1133, "Model Village"),
    "Pick up Mini Shovel": GooseGameLocationData(BASE_ID + 1134, "Model Village"),
    "Pick up Poppy Flower": GooseGameLocationData(BASE_ID + 1135, "Model Village"),
    "Pick up Mini Person (Old Woman)": GooseGameLocationData(BASE_ID + 1136, "Model Village"),
    "Pick up Mini Phone Door": GooseGameLocationData(BASE_ID + 1137, "Model Village"),
    "Pick up Mini Mail Pillar": GooseGameLocationData(BASE_ID + 1138, "Model Village"),
    "Pick up Mini Person (Postie)": GooseGameLocationData(BASE_ID + 1139, "Model Village"),
    "Pick up Mini Person (Vest Man)": GooseGameLocationData(BASE_ID + 1140, "Model Village"),
    "Pick up Mini Person": GooseGameLocationData(BASE_ID + 1141, "Model Village"),
    "Pick up Mini Person (Goose)": GooseGameLocationData(BASE_ID + 1144, "Model Village"),
    "Pick up Timber Handle": GooseGameLocationData(BASE_ID + 1142, "Model Village"),
    "Pick up Golden Bell": GooseGameLocationData(BASE_ID + 1143, "Model Village"),
}

# =============================================================================
# DRAG ITEM LOCATIONS - First time dragging heavy items (IDs 1201-1299)
# =============================================================================

drag_item_locations: Dict[str, GooseGameLocationData] = {
    # Garden drags (1201-1215) - Topsoil moved to unique_item_locations
    "Drag Rake": GooseGameLocationData(BASE_ID + 1201, "Garden"),
    "Drag Picnic Basket": GooseGameLocationData(BASE_ID + 1202, "Garden"),
    "Drag Esky": GooseGameLocationData(BASE_ID + 1203, "Garden"),
    "Drag Shovel": GooseGameLocationData(BASE_ID + 1205, "Garden"),
    "Drag Pumpkin": GooseGameLocationData(BASE_ID + 1206, "Garden"),
    "Drag Pumpkin 2": GooseGameLocationData(BASE_ID + 1207, "Garden"),
    "Drag Pumpkin 3": GooseGameLocationData(BASE_ID + 1208, "Garden"),
    "Drag Pumpkin 4": GooseGameLocationData(BASE_ID + 1209, "Garden"),
    "Drag Watering Can": GooseGameLocationData(BASE_ID + 1210, "Garden"),
    "Drag Gumboot 1": GooseGameLocationData(BASE_ID + 1211, "Garden"),
    "Drag Gumboot 2": GooseGameLocationData(BASE_ID + 1212, "Garden"),
    "Drag Gardener Sign": GooseGameLocationData(BASE_ID + 1213, "Garden"),
    "Drag Wooden Crate": GooseGameLocationData(BASE_ID + 1214, "Garden"),
    "Drag Fence Bolt": GooseGameLocationData(BASE_ID + 1215, "Garden"),
    "Drag Mallet": GooseGameLocationData(BASE_ID + 1216, "Garden"),
    
    # High Street drags (1220-1229)
    "Drag Shopping Basket": GooseGameLocationData(BASE_ID + 1220, "High Street"),
    "Drag Black Umbrella": GooseGameLocationData(BASE_ID + 1221, "High Street"),
    "Drag Push Broom": GooseGameLocationData(BASE_ID + 1222, "High Street"),
    "Drag Broken Broom Head": GooseGameLocationData(BASE_ID + 1223, "High Street"),
    "Drag Dustbin": GooseGameLocationData(BASE_ID + 1224, "High Street"),
    "Drag Baby Doll": GooseGameLocationData(BASE_ID + 1225, "High Street"),
    "Drag Pricing Gun": GooseGameLocationData(BASE_ID + 1226, "High Street"),
    "Drag Adding Machine": GooseGameLocationData(BASE_ID + 1227, "High Street"),
    "Drag Rainbow Umbrella": GooseGameLocationData(BASE_ID + 1228, "High Street"),
    "Drag Red Umbrella": GooseGameLocationData(BASE_ID + 1229, "High Street"),
    
    # Back Gardens drags (1240-1250)
    "Drag Rose Box": GooseGameLocationData(BASE_ID + 1240, "Back Gardens"),
    "Drag Cricket Bat": GooseGameLocationData(BASE_ID + 1241, "Back Gardens"),
    "Drag Tea Pot": GooseGameLocationData(BASE_ID + 1242, "Back Gardens"),
    "Drag Clippers": GooseGameLocationData(BASE_ID + 1243, "Back Gardens"),
    "Drag Duck Statue": GooseGameLocationData(BASE_ID + 1244, "Back Gardens"),
    "Drag Frog Statue": GooseGameLocationData(BASE_ID + 1245, "Back Gardens"),
    "Drag Jeremy Fish": GooseGameLocationData(BASE_ID + 1246, "Back Gardens"),
    "Drag Messy Sign": GooseGameLocationData(BASE_ID + 1247, "Back Gardens"),
    "Drag Drawer": GooseGameLocationData(BASE_ID + 1248, "Back Gardens"),
    "Drag Enamel Jug": GooseGameLocationData(BASE_ID + 1249, "Back Gardens"),
    "Drag Clean Sign": GooseGameLocationData(BASE_ID + 1250, "Back Gardens"),
    
    # Pub drags (1270-1280)
    "Drag Tackle Box": GooseGameLocationData(BASE_ID + 1270, "Pub"),
    "Drag Traffic Cone": GooseGameLocationData(BASE_ID + 1271, "Pub"),
    "Drag Exit Parcel": GooseGameLocationData(BASE_ID + 1272, "Pub"),
    "Drag Stealth Box": GooseGameLocationData(BASE_ID + 1273, "Pub"),
    "Drag No Goose Sign": GooseGameLocationData(BASE_ID + 1274, "Pub"),
    "Drag Portable Stool": GooseGameLocationData(BASE_ID + 1275, "Pub"),
    "Drag Dartboard": GooseGameLocationData(BASE_ID + 1276, "Pub"),
    "Drag Mop Bucket": GooseGameLocationData(BASE_ID + 1277, "Pub"),
    "Drag Mop": GooseGameLocationData(BASE_ID + 1278, "Pub"),
    "Drag Delivery Box": GooseGameLocationData(BASE_ID + 1279, "Pub"),
    # "Drag Burly Mans Bucket": GooseGameLocationData(BASE_ID + 1280, "Pub"),
    
    # Model Village drags (1290-1295)
    "Drag Mini Bench": GooseGameLocationData(BASE_ID + 1290, "Model Village"),
    "Drag Mini Pump": GooseGameLocationData(BASE_ID + 1291, "Model Village"),
    "Drag Mini Street Bench": GooseGameLocationData(BASE_ID + 1292, "Model Village"),
    "Drag Birdbath": GooseGameLocationData(BASE_ID + 1293, "Model Village"),
    "Drag Easel": GooseGameLocationData(BASE_ID + 1294, "Model Village"),
    "Drag Sun Lounge": GooseGameLocationData(BASE_ID + 1295, "Model Village"),
}

# =============================================================================
# INTERACTION LOCATIONS - First time interacting with objects (IDs 1301-1399)
# =============================================================================

interaction_locations: Dict[str, GooseGameLocationData] = {
    # Garden interactions (1301-1303)
    "Ring the Bike Bell": GooseGameLocationData(BASE_ID + 1301, "Garden"),
    "Turn on Garden Tap": GooseGameLocationData(BASE_ID + 1302, "Garden"),
    "Turn on Sprinkler": GooseGameLocationData(BASE_ID + 1303, "Garden"),
    
    # Hub interactions (1306)
    "Open Intro Gate": GooseGameLocationData(BASE_ID + 1306, "Hub"),  # Very first gate at game start
    
    # High Street interactions (1310-1317)
    "Break Through Boards": GooseGameLocationData(BASE_ID + 1310, "High Street"),
    "Unplug the Radio": GooseGameLocationData(BASE_ID + 1311, "High Street"),
    "Open Black Umbrella": GooseGameLocationData(BASE_ID + 1313, "High Street"),
    "Open Rainbow Umbrella": GooseGameLocationData(BASE_ID + 1314, "High Street"),
    "Open Red Umbrella": GooseGameLocationData(BASE_ID + 1315, "High Street"),
    "Untie Boy's Laces (Left)": GooseGameLocationData(BASE_ID + 1316, "High Street"),
    "Untie Boy's Laces (Right)": GooseGameLocationData(BASE_ID + 1317, "High Street"),
    
    # Back Gardens interactions (1320-1346)
    "Ring the Back Gardens Bell": GooseGameLocationData(BASE_ID + 1320, "Back Gardens"),
    "Spin the Windmill": GooseGameLocationData(BASE_ID + 1322, "Back Gardens"),
    "Spin Purple Flower": GooseGameLocationData(BASE_ID + 1323, "Back Gardens"),
    "Break Through Trellis": GooseGameLocationData(BASE_ID + 1324, "Back Gardens"),
    "Spin Sunflower": GooseGameLocationData(BASE_ID + 1325, "Back Gardens"),
    
    # Wind Chimes - individual notes (1340-1346) - left to right: G, F, E, D, C, B, A
    "Play Wind Chime (G)": GooseGameLocationData(BASE_ID + 1340, "Back Gardens"),
    "Play Wind Chime (F)": GooseGameLocationData(BASE_ID + 1341, "Back Gardens"),
    "Play Wind Chime (E)": GooseGameLocationData(BASE_ID + 1342, "Back Gardens"),
    "Play Wind Chime (D)": GooseGameLocationData(BASE_ID + 1343, "Back Gardens"),
    "Play Wind Chime (C)": GooseGameLocationData(BASE_ID + 1344, "Back Gardens"),
    "Play Wind Chime (B)": GooseGameLocationData(BASE_ID + 1345, "Back Gardens"),
    "Play Wind Chime (A)": GooseGameLocationData(BASE_ID + 1346, "Back Gardens"),
    
    # Pub interactions (1330-1334)
    "Close Van Door (Left)": GooseGameLocationData(BASE_ID + 1330, "Pub"),
    "Close Van Door (Right)": GooseGameLocationData(BASE_ID + 1331, "Pub"),
    "Untie Burly Man's Laces (Left)": GooseGameLocationData(BASE_ID + 1332, "Pub"),
    "Untie Burly Man's Laces (Right)": GooseGameLocationData(BASE_ID + 1333, "Pub"),
    "Turn on Pub Tap": GooseGameLocationData(BASE_ID + 1334, "Pub"),
}

# UNIQUE TRACKED ITEMS - Runtime-renamed items (IDs 1401-1450)
# Items are renamed at runtime based on position sorting for unique tracking

# Carrots (IDs 1401-1413)
# Carrots 1-10: Garden area (lower X positions)
# Carrots 11-13: High Street shop display (higher X positions around 28)
unique_item_locations: Dict[str, GooseGameLocationData] = {
    "Pick up Carrot 1": GooseGameLocationData(BASE_ID + 1401, "Garden"),
    "Pick up Carrot 2": GooseGameLocationData(BASE_ID + 1402, "Garden"),
    "Pick up Carrot 3": GooseGameLocationData(BASE_ID + 1403, "Garden"),
    "Pick up Carrot 4": GooseGameLocationData(BASE_ID + 1404, "Garden"),
    "Pick up Carrot 5": GooseGameLocationData(BASE_ID + 1405, "Garden"),
    "Pick up Carrot 6": GooseGameLocationData(BASE_ID + 1406, "Garden"),
    "Pick up Carrot 7": GooseGameLocationData(BASE_ID + 1407, "Garden"),
    "Pick up Carrot 8": GooseGameLocationData(BASE_ID + 1408, "Garden"),
    "Pick up Carrot 9": GooseGameLocationData(BASE_ID + 1409, "Garden"),
    "Pick up Carrot 10": GooseGameLocationData(BASE_ID + 1410, "Garden"),
    "Pick up Carrot 11": GooseGameLocationData(BASE_ID + 1411, "High Street"),  # Shop carrot
    "Pick up Carrot 12": GooseGameLocationData(BASE_ID + 1412, "High Street"),  # Shop carrot
    "Pick up Carrot 13": GooseGameLocationData(BASE_ID + 1413, "High Street"),  # Shop carrot
    
    # Pub Tomatoes (IDs 1421-1431)
    "Pick up Pub Tomato 1": GooseGameLocationData(BASE_ID + 1421, "Pub"),
    "Pick up Pub Tomato 2": GooseGameLocationData(BASE_ID + 1422, "Pub"),
    "Pick up Pub Tomato 3": GooseGameLocationData(BASE_ID + 1423, "Pub"),
    "Pick up Pub Tomato 4": GooseGameLocationData(BASE_ID + 1424, "Pub"),
    "Pick up Pub Tomato 5": GooseGameLocationData(BASE_ID + 1425, "Pub"),
    "Pick up Pub Tomato 6": GooseGameLocationData(BASE_ID + 1426, "Pub"),
    "Pick up Pub Tomato 7": GooseGameLocationData(BASE_ID + 1427, "Pub"),
    "Pick up Pub Tomato 8": GooseGameLocationData(BASE_ID + 1428, "Pub"),
    "Pick up Pub Tomato 9": GooseGameLocationData(BASE_ID + 1429, "Pub"),
    "Pick up Pub Tomato 10": GooseGameLocationData(BASE_ID + 1430, "Pub"),
    "Pick up Pub Tomato 11": GooseGameLocationData(BASE_ID + 1431, "Pub"),
    
    # Boots (IDs 1440-1441)
    "Pick up Garden Boot": GooseGameLocationData(BASE_ID + 1440, "Garden"),
    "Pick up Hub Boot": GooseGameLocationData(BASE_ID + 1441, "Hub"),
    
    # Topsoil Bags (IDs 1450-1452)
    "Drag Topsoil Bag 1": GooseGameLocationData(BASE_ID + 1450, "Garden"),
    "Drag Topsoil Bag 2": GooseGameLocationData(BASE_ID + 1451, "Garden"),
    "Drag Topsoil Bag 3": GooseGameLocationData(BASE_ID + 1452, "Garden"),
}

# Sandcastle peck locations (1350-1384) - 35 total raw pecks
sandcastle_peck_locations: Dict[str, GooseGameLocationData] = {
    # Doorway side - 19 pecks
    "Peck Model Church Doorway 1": GooseGameLocationData(BASE_ID + 1350, "Model Village"),
    "Peck Model Church Doorway 2": GooseGameLocationData(BASE_ID + 1351, "Model Village"),
    "Peck Model Church Doorway 3": GooseGameLocationData(BASE_ID + 1352, "Model Village"),
    "Peck Model Church Doorway 4": GooseGameLocationData(BASE_ID + 1353, "Model Village"),
    "Peck Model Church Doorway 5": GooseGameLocationData(BASE_ID + 1354, "Model Village"),
    "Peck Model Church Doorway 6": GooseGameLocationData(BASE_ID + 1355, "Model Village"),
    "Peck Model Church Doorway 7": GooseGameLocationData(BASE_ID + 1356, "Model Village"),
    "Peck Model Church Doorway 8": GooseGameLocationData(BASE_ID + 1357, "Model Village"),
    "Peck Model Church Doorway 9": GooseGameLocationData(BASE_ID + 1358, "Model Village"),
    "Peck Model Church Doorway 10": GooseGameLocationData(BASE_ID + 1359, "Model Village"),
    "Peck Model Church Doorway 11": GooseGameLocationData(BASE_ID + 1360, "Model Village"),
    "Peck Model Church Doorway 12": GooseGameLocationData(BASE_ID + 1361, "Model Village"),
    "Peck Model Church Doorway 13": GooseGameLocationData(BASE_ID + 1362, "Model Village"),
    "Peck Model Church Doorway 14": GooseGameLocationData(BASE_ID + 1363, "Model Village"),
    "Peck Model Church Doorway 15": GooseGameLocationData(BASE_ID + 1364, "Model Village"),
    "Peck Model Church Doorway 16": GooseGameLocationData(BASE_ID + 1365, "Model Village"),
    "Peck Model Church Doorway 17": GooseGameLocationData(BASE_ID + 1366, "Model Village"),
    "Peck Model Church Doorway 18": GooseGameLocationData(BASE_ID + 1367, "Model Village"),
    "Peck Model Church Doorway 19": GooseGameLocationData(BASE_ID + 1368, "Model Village"),
    # Tower side - 16 pecks
    "Peck Model Church Tower 1": GooseGameLocationData(BASE_ID + 1369, "Model Village"),
    "Peck Model Church Tower 2": GooseGameLocationData(BASE_ID + 1370, "Model Village"),
    "Peck Model Church Tower 3": GooseGameLocationData(BASE_ID + 1371, "Model Village"),
    "Peck Model Church Tower 4": GooseGameLocationData(BASE_ID + 1372, "Model Village"),
    "Peck Model Church Tower 5": GooseGameLocationData(BASE_ID + 1373, "Model Village"),
    "Peck Model Church Tower 6": GooseGameLocationData(BASE_ID + 1374, "Model Village"),
    "Peck Model Church Tower 7": GooseGameLocationData(BASE_ID + 1375, "Model Village"),
    "Peck Model Church Tower 8": GooseGameLocationData(BASE_ID + 1376, "Model Village"),
    "Peck Model Church Tower 9": GooseGameLocationData(BASE_ID + 1377, "Model Village"),
    "Peck Model Church Tower 10": GooseGameLocationData(BASE_ID + 1378, "Model Village"),
    "Peck Model Church Tower 11": GooseGameLocationData(BASE_ID + 1379, "Model Village"),
    "Peck Model Church Tower 12": GooseGameLocationData(BASE_ID + 1380, "Model Village"),
    "Peck Model Church Tower 13": GooseGameLocationData(BASE_ID + 1381, "Model Village"),
    "Peck Model Church Tower 14": GooseGameLocationData(BASE_ID + 1382, "Model Village"),
    "Peck Model Church Tower 15": GooseGameLocationData(BASE_ID + 1383, "Model Village"),
    "Peck Model Church Tower 16": GooseGameLocationData(BASE_ID + 1384, "Model Village"),
}


def get_all_locations(include_extra: bool = False, include_speedrun: bool = False, 
                      include_items: bool = True, include_drags: bool = True,
                      include_interactions: bool = True, include_unique: bool = True,
                      include_sandcastle: bool = True) -> Dict[str, GooseGameLocationData]:
    """Get locations based on options (for region creation)"""
    locations = dict(location_table)
    
    if include_extra:
        locations.update(extra_locations)
        locations.update(completion_location)
    
    if include_speedrun:
        locations.update(speedrun_locations)
    
    if include_items:
        locations.update(item_pickup_locations)
    
    if include_drags:
        locations.update(drag_item_locations)
    
    if include_interactions:
        locations.update(interaction_locations)
    
    if include_unique:
        locations.update(unique_item_locations)
    
    if include_sandcastle:
        locations.update(sandcastle_peck_locations)
    
    return locations


def get_all_location_ids() -> Dict[str, int]:
    """Get ALL location name->ID mappings (for AP registration)
    
    IMPORTANT: AP requires all possible locations registered upfront,
    regardless of whether they're enabled by options.
    """
    all_locs = {}
    all_locs.update({name: data.id for name, data in location_table.items()})
    all_locs.update({name: data.id for name, data in extra_locations.items()})
    all_locs.update({name: data.id for name, data in speedrun_locations.items()})
    all_locs.update({name: data.id for name, data in completion_location.items()})
    all_locs.update({name: data.id for name, data in item_pickup_locations.items()})
    all_locs.update({name: data.id for name, data in drag_item_locations.items()})
    all_locs.update({name: data.id for name, data in interaction_locations.items()})
    all_locs.update({name: data.id for name, data in unique_item_locations.items()})
    all_locs.update({name: data.id for name, data in sandcastle_peck_locations.items()})
    return all_locs
