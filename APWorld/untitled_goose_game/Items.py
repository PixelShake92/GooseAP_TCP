from typing import Dict, NamedTuple, Optional, Set
from BaseClasses import Item, ItemClassification


class GooseGameItemData(NamedTuple):
    id: int
    classification: ItemClassification
    

class GooseGameItem(Item):
    game = "Untitled Goose Game"


BASE_ID = 119000000

item_table: Dict[str, GooseGameItemData] = {
    # Area Unlock Items (Progression)
    "Garden Access": GooseGameItemData(BASE_ID + 100, ItemClassification.progression),
    "High Street Access": GooseGameItemData(BASE_ID + 101, ItemClassification.progression),
    "Back Gardens Access": GooseGameItemData(BASE_ID + 102, ItemClassification.progression),
    "Pub Access": GooseGameItemData(BASE_ID + 103, ItemClassification.progression),
    "Model Village Access": GooseGameItemData(BASE_ID + 104, ItemClassification.progression),
    
    # Progressive Area (alternative)
    "Progressive Area": GooseGameItemData(BASE_ID + 110, ItemClassification.progression),
    
    # === NPC Souls (Progression) - NPCs won't appear until you have their soul ===
    "Groundskeeper Soul": GooseGameItemData(BASE_ID + 120, ItemClassification.progression),
    "Boy Soul": GooseGameItemData(BASE_ID + 121, ItemClassification.progression),
    "TV Shop Owner Soul": GooseGameItemData(BASE_ID + 122, ItemClassification.progression),
    "Market Lady Soul": GooseGameItemData(BASE_ID + 123, ItemClassification.progression),
    "Tidy Neighbour Soul": GooseGameItemData(BASE_ID + 124, ItemClassification.progression),
    "Messy Neighbour Soul": GooseGameItemData(BASE_ID + 125, ItemClassification.progression),
    "Burly Man Soul": GooseGameItemData(BASE_ID + 126, ItemClassification.progression),
    "Old Man Soul": GooseGameItemData(BASE_ID + 127, ItemClassification.progression),
    "Pub Lady Soul": GooseGameItemData(BASE_ID + 128, ItemClassification.progression),
    "Fancy Ladies Soul": GooseGameItemData(BASE_ID + 129, ItemClassification.progression),
    "Cook Soul": GooseGameItemData(BASE_ID + 130, ItemClassification.progression),
    
    # === GROUPED PROP SOULS (IDs 400-499) - Multiple instances share one soul ===
    "Carrot Soul": GooseGameItemData(BASE_ID + 400, ItemClassification.progression),
    "Tomato Soul": GooseGameItemData(BASE_ID + 401, ItemClassification.progression),
    "Pumpkin Soul": GooseGameItemData(BASE_ID + 402, ItemClassification.progression),
    "Topsoil Bag Soul": GooseGameItemData(BASE_ID + 403, ItemClassification.progression),
    "Quoit Soul": GooseGameItemData(BASE_ID + 404, ItemClassification.progression),
    "Plate Soul": GooseGameItemData(BASE_ID + 405, ItemClassification.progression),
    "Orange Soul": GooseGameItemData(BASE_ID + 406, ItemClassification.progression),
    "Leek Soul": GooseGameItemData(BASE_ID + 407, ItemClassification.progression),
    "Cucumber Soul": GooseGameItemData(BASE_ID + 408, ItemClassification.progression),
    "Umbrella Soul": GooseGameItemData(BASE_ID + 410, ItemClassification.progression),
    "Tinned Food Soul": GooseGameItemData(BASE_ID + 411, ItemClassification.progression),
    "Sock Soul": GooseGameItemData(BASE_ID + 412, ItemClassification.progression),
    "Pint Bottle Soul": GooseGameItemData(BASE_ID + 413, ItemClassification.progression),
    "Knife Soul": GooseGameItemData(BASE_ID + 414, ItemClassification.progression),
    "Gumboot Soul": GooseGameItemData(BASE_ID + 415, ItemClassification.progression),
    "Fork Soul": GooseGameItemData(BASE_ID + 416, ItemClassification.progression),
    "Apple Core Soul": GooseGameItemData(BASE_ID + 418, ItemClassification.progression),
    "Apple Soul": GooseGameItemData(BASE_ID + 419, ItemClassification.progression),
    "Sandwich Soul": GooseGameItemData(BASE_ID + 420, ItemClassification.progression),
    "Bow Soul": GooseGameItemData(BASE_ID + 422, ItemClassification.progression),
    "Walkie Talkie Soul": GooseGameItemData(BASE_ID + 423, ItemClassification.progression),
    "Boot Soul": GooseGameItemData(BASE_ID + 424, ItemClassification.progression),
    "Mini Person Soul": GooseGameItemData(BASE_ID + 425, ItemClassification.progression),
    "Mini Bench Soul": GooseGameItemData(BASE_ID + 617, ItemClassification.progression),
    
    # === GARDEN ONE-OFF PROP SOULS (IDs 500-519) ===
    "Radio Soul": GooseGameItemData(BASE_ID + 500, ItemClassification.progression),
    "Trowel Soul": GooseGameItemData(BASE_ID + 501, ItemClassification.progression),
    "Tulip Soul": GooseGameItemData(BASE_ID + 503, ItemClassification.progression),
    "Jam Soul": GooseGameItemData(BASE_ID + 504, ItemClassification.progression),
    "Picnic Mug Soul": GooseGameItemData(BASE_ID + 505, ItemClassification.progression_deprioritized),
    "Thermos Soul": GooseGameItemData(BASE_ID + 506, ItemClassification.progression),
    "Straw Hat Soul": GooseGameItemData(BASE_ID + 507, ItemClassification.progression),
    "Drink Can Soul": GooseGameItemData(BASE_ID + 508, ItemClassification.progression_deprioritized),
    "Tennis Ball Soul": GooseGameItemData(BASE_ID + 509, ItemClassification.progression_deprioritized),
    "Rake Soul": GooseGameItemData(BASE_ID + 511, ItemClassification.progression),
    "Picnic Basket Soul": GooseGameItemData(BASE_ID + 512, ItemClassification.progression),
    "Esky Soul": GooseGameItemData(BASE_ID + 513, ItemClassification.progression_deprioritized),
    "Shovel Soul": GooseGameItemData(BASE_ID + 514, ItemClassification.progression_deprioritized),
    "Watering Can Soul": GooseGameItemData(BASE_ID + 515, ItemClassification.progression_deprioritized),
    "Mallet Soul": GooseGameItemData(BASE_ID + 517, ItemClassification.progression),
    "Wooden Crate Soul": GooseGameItemData(BASE_ID + 518, ItemClassification.progression_deprioritized),
    
    # === HIGH STREET ONE-OFF PROP SOULS (IDs 520-549) ===
    "Horn-Rimmed Glasses Soul": GooseGameItemData(BASE_ID + 521, ItemClassification.progression),
    "Red Glasses Soul": GooseGameItemData(BASE_ID + 522, ItemClassification.progression),
    "Sunglasses Soul": GooseGameItemData(BASE_ID + 523, ItemClassification.progression),
    "Loo Paper Soul": GooseGameItemData(BASE_ID + 524, ItemClassification.progression),
    "Toy Car Soul": GooseGameItemData(BASE_ID + 525, ItemClassification.progression_deprioritized),
    "Hairbrush Soul": GooseGameItemData(BASE_ID + 526, ItemClassification.progression),
    "Toothbrush Soul": GooseGameItemData(BASE_ID + 527, ItemClassification.progression),
    "Stereoscope Soul": GooseGameItemData(BASE_ID + 528, ItemClassification.progression),
    "Dish Soap Bottle Soul": GooseGameItemData(BASE_ID + 529, ItemClassification.progression),
    "Spray Bottle Soul": GooseGameItemData(BASE_ID + 530, ItemClassification.progression),
    "Weed Tool Soul": GooseGameItemData(BASE_ID + 531, ItemClassification.progression_deprioritized),
    "Lily Flower Soul": GooseGameItemData(BASE_ID + 532, ItemClassification.progression),
    "Fusilage Soul": GooseGameItemData(BASE_ID + 533, ItemClassification.progression),
    "Chalk Soul": GooseGameItemData(BASE_ID + 535, ItemClassification.progression),
    "Dustbin Lid Soul": GooseGameItemData(BASE_ID + 536, ItemClassification.progression_deprioritized),
    "Shopping Basket Soul": GooseGameItemData(BASE_ID + 537, ItemClassification.progression),
    "Push Broom Soul": GooseGameItemData(BASE_ID + 538, ItemClassification.progression),
    "Dustbin Soul": GooseGameItemData(BASE_ID + 540, ItemClassification.progression_deprioritized),
    "Baby Doll Soul": GooseGameItemData(BASE_ID + 541, ItemClassification.progression_deprioritized),
    "Pricing Gun Soul": GooseGameItemData(BASE_ID + 542, ItemClassification.progression_deprioritized),
    "Adding Machine Soul": GooseGameItemData(BASE_ID + 543, ItemClassification.progression_deprioritized),
    
    # === BACK GARDENS ONE-OFF PROP SOULS (IDs 550-579) ===
    "Dummy Soul": GooseGameItemData(BASE_ID + 550, ItemClassification.progression_deprioritized),
    "Cricket Ball Soul": GooseGameItemData(BASE_ID + 551, ItemClassification.progression_deprioritized),
    "Bust Pipe Soul": GooseGameItemData(BASE_ID + 552, ItemClassification.progression),
    "Bust Hat Soul": GooseGameItemData(BASE_ID + 553, ItemClassification.progression),
    "Bust Glasses Soul": GooseGameItemData(BASE_ID + 554, ItemClassification.progression),
    "Tea Cup Soul": GooseGameItemData(BASE_ID + 555, ItemClassification.progression),
    "Newspaper Soul": GooseGameItemData(BASE_ID + 556, ItemClassification.progression_deprioritized),
    "Badminton Racket Soul": GooseGameItemData(BASE_ID + 557, ItemClassification.progression_deprioritized),
    "Pot Stack Soul": GooseGameItemData(BASE_ID + 558, ItemClassification.progression_deprioritized),
    "Soap Soul": GooseGameItemData(BASE_ID + 559, ItemClassification.progression),
    "Paintbrush Soul": GooseGameItemData(BASE_ID + 560, ItemClassification.progression),
    "Vase Soul": GooseGameItemData(BASE_ID + 561, ItemClassification.progression),
    "Bra Soul": GooseGameItemData(BASE_ID + 562, ItemClassification.progression),
    "Rose Soul": GooseGameItemData(BASE_ID + 563, ItemClassification.progression),
    # Removing Rose Box Soul until I can solve the physics issues with it
    # "Rose Box Soul": GooseGameItemData(BASE_ID + 564, ItemClassification.progression),
    "Cricket Bat Soul": GooseGameItemData(BASE_ID + 565, ItemClassification.progression_deprioritized),
    "Tea Pot Soul": GooseGameItemData(BASE_ID + 566, ItemClassification.progression_deprioritized),
    "Clippers Soul": GooseGameItemData(BASE_ID + 567, ItemClassification.progression),
    "Duck Statue Soul": GooseGameItemData(BASE_ID + 568, ItemClassification.progression),
    "Frog Statue Soul": GooseGameItemData(BASE_ID + 569, ItemClassification.progression_deprioritized),
    "Jeremy Fish Soul": GooseGameItemData(BASE_ID + 570, ItemClassification.progression_deprioritized),
    "Messy Sign Soul": GooseGameItemData(BASE_ID + 571, ItemClassification.progression),
    "Drawer Soul": GooseGameItemData(BASE_ID + 572, ItemClassification.progression),
    "Enamel Jug Soul": GooseGameItemData(BASE_ID + 573, ItemClassification.progression_deprioritized),
    "Clean Sign Soul": GooseGameItemData(BASE_ID + 574, ItemClassification.progression),
    
    # === PUB ONE-OFF PROP SOULS (IDs 580-609) ===
    "Fishing Bobber Soul": GooseGameItemData(BASE_ID + 580, ItemClassification.progression_deprioritized),
    "Exit Letter Soul": GooseGameItemData(BASE_ID + 581, ItemClassification.progression_deprioritized),
    "Pint Glass Soul": GooseGameItemData(BASE_ID + 582, ItemClassification.progression),
    "Toy Boat Soul": GooseGameItemData(BASE_ID + 583, ItemClassification.progression),
    "Pepper Grinder Soul": GooseGameItemData(BASE_ID + 585, ItemClassification.progression),
    "Cork Soul": GooseGameItemData(BASE_ID + 587, ItemClassification.progression_deprioritized),
    "Candlestick Soul": GooseGameItemData(BASE_ID + 588, ItemClassification.progression),
    "Flower for Vase Soul": GooseGameItemData(BASE_ID + 589, ItemClassification.progression),
    "Harmonica Soul": GooseGameItemData(BASE_ID + 590, ItemClassification.progression_deprioritized),
    "Tackle Box Soul": GooseGameItemData(BASE_ID + 591, ItemClassification.progression_deprioritized),
    "Traffic Cone Soul": GooseGameItemData(BASE_ID + 592, ItemClassification.progression),
    "Exit Parcel Soul": GooseGameItemData(BASE_ID + 593, ItemClassification.progression_deprioritized),
    "Stealth Box Soul": GooseGameItemData(BASE_ID + 594, ItemClassification.progression),
    "No Goose Sign Soul": GooseGameItemData(BASE_ID + 595, ItemClassification.progression),
    "Portable Stool Soul": GooseGameItemData(BASE_ID + 596, ItemClassification.progression),
    "Dartboard Soul": GooseGameItemData(BASE_ID + 597, ItemClassification.progression),
    "Mop Bucket Soul": GooseGameItemData(BASE_ID + 598, ItemClassification.progression_deprioritized),
    "Mop Soul": GooseGameItemData(BASE_ID + 599, ItemClassification.progression_deprioritized),
    "Burly Mans Bucket Soul": GooseGameItemData(BASE_ID + 601, ItemClassification.progression),
    
    # === MODEL VILLAGE ONE-OFF PROP SOULS (IDs 610-629) ===
    "Mini Mail Pillar Soul": GooseGameItemData(BASE_ID + 610, ItemClassification.progression_deprioritized),
    "Mini Phone Door Soul": GooseGameItemData(BASE_ID + 611, ItemClassification.progression_deprioritized),
    "Mini Shovel Soul": GooseGameItemData(BASE_ID + 612, ItemClassification.progression_deprioritized),
    "Poppy Flower Soul": GooseGameItemData(BASE_ID + 613, ItemClassification.progression_deprioritized),
    "Timber Handle Soul": GooseGameItemData(BASE_ID + 614, ItemClassification.progression),
    "Birdbath Soul": GooseGameItemData(BASE_ID + 615, ItemClassification.progression_deprioritized),
    "Easel Soul": GooseGameItemData(BASE_ID + 616, ItemClassification.progression_deprioritized),
    "Mini Pump Soul": GooseGameItemData(BASE_ID + 618, ItemClassification.progression_deprioritized),
    "Sun Lounge Soul": GooseGameItemData(BASE_ID + 620, ItemClassification.progression_deprioritized),
    
    # Victory item soul
    "Golden Bell Soul": GooseGameItemData(BASE_ID + 621, ItemClassification.progression),
    
    # === FILLER ITEMS ===
    "Mega Honk": GooseGameItemData(BASE_ID + 200, ItemClassification.useful),
    "Speedy Feet": GooseGameItemData(BASE_ID + 201, ItemClassification.useful),
    "Silent Steps": GooseGameItemData(BASE_ID + 202, ItemClassification.useful),
    "A Goose Day": GooseGameItemData(BASE_ID + 203, ItemClassification.filler),
    "Coin": GooseGameItemData(BASE_ID + 204, ItemClassification.filler),
    
    # === TRAP ITEMS ===
    "Tired Goose": GooseGameItemData(BASE_ID + 300, ItemClassification.trap),
    "Confused Feet": GooseGameItemData(BASE_ID + 301, ItemClassification.trap),
    "Butterbeak": GooseGameItemData(BASE_ID + 302, ItemClassification.trap),
    "Suspicious Goose": GooseGameItemData(BASE_ID + 303, ItemClassification.trap),
    
    # === MILESTONE ITEMS (for goal completion) ===
    "All Main Goals Complete": GooseGameItemData(BASE_ID + 310, ItemClassification.progression),
    "All Goals Complete": GooseGameItemData(BASE_ID + 311, ItemClassification.progression),
    
    # Victory
    
    "Golden Bell": GooseGameItemData(BASE_ID + 999, ItemClassification.progression),

}

# Item groups for logical grouping
ITEM_GROUPS = {
    "Area Unlocks": {
        "Garden Access",
        "High Street Access", 
        "Back Gardens Access",
        "Pub Access",
        "Model Village Access",
    },
    "NPC Souls": {
        "Groundskeeper Soul",
        "Boy Soul",
        "TV Shop Owner Soul",
        "Market Lady Soul",
        "Tidy Neighbour Soul",
        "Messy Neighbour Soul",
        "Burly Man Soul",
        "Old Man Soul",
        "Pub Lady Soul",
        "Fancy Ladies Soul",
        "Cook Soul",
    },
    "Grouped Prop Souls": {
        "Carrot Soul",
        "Tomato Soul",
        "Pumpkin Soul",
        "Topsoil Bag Soul",
        "Quoit Soul",
        "Plate Soul",
        "Orange Soul",
        "Leek Soul",
        "Cucumber Soul",
        "Umbrella Soul",
        "Tinned Food Soul",
        "Sock Soul",
        "Pint Bottle Soul",
        "Knife Soul",
        "Gumboot Soul",
        "Fork Soul",
        "Apple Core Soul",
        "Apple Soul",
        "Sandwich Soul",
        "Bow Soul",
        "Walkie Talkie Soul",
        "Boot Soul",
        "Mini Person Soul",
    },
    "Garden Prop Souls": {
        "Radio Soul",
        "Trowel Soul",
        "Tulip Soul",
        "Jam Soul",
        "Picnic Mug Soul",
        "Thermos Soul",
        "Straw Hat Soul",
        "Drink Can Soul",
        "Tennis Ball Soul",
        "Rake Soul",
        "Picnic Basket Soul",
        "Esky Soul",
        "Shovel Soul",
        "Watering Can Soul",
        "Mallet Soul",
        "Wooden Crate Soul",
    },
    "High Street Prop Souls": {
        "Horn-Rimmed Glasses Soul",
        "Red Glasses Soul",
        "Sunglasses Soul",
        "Loo Paper Soul",
        "Toy Car Soul",
        "Hairbrush Soul",
        "Toothbrush Soul",
        "Stereoscope Soul",
        "Dish Soap Bottle Soul",
        "Spray Bottle Soul",
        "Weed Tool Soul",
        "Lily Flower Soul",
        "Fusilage Soul",
        "Chalk Soul",
        "Dustbin Lid Soul",
        "Shopping Basket Soul",
        "Push Broom Soul",
        "Dustbin Soul",
        "Baby Doll Soul",
        "Pricing Gun Soul",
        "Adding Machine Soul",
    },
    "Back Gardens Prop Souls": {
        "Dummy Soul",
        "Cricket Ball Soul",
        "Bust Pipe Soul",
        "Bust Hat Soul",
        "Bust Glasses Soul",
        "Tea Cup Soul",
        "Newspaper Soul",
        "Badminton Racket Soul",
        "Pot Stack Soul",
        "Soap Soul",
        "Paintbrush Soul",
        "Vase Soul",
        "Bra Soul",
        "Rose Soul",
        # Removing Rose Box Soul until I can solve the physics issues with it
        # "Rose Box Soul",
        "Cricket Bat Soul",
        "Tea Pot Soul",
        "Clippers Soul",
        "Duck Statue Soul",
        "Frog Statue Soul",
        "Jeremy Fish Soul",
        "Messy Sign Soul",
        "Drawer Soul",
        "Enamel Jug Soul",
        "Clean Sign Soul",
    },
    "Pub Prop Souls": {
        "Fishing Bobber Soul",
        "Exit Letter Soul",
        "Pint Glass Soul",
        "Toy Boat Soul",
        "Pepper Grinder Soul",
        "Cork Soul",
        "Candlestick Soul",
        "Flower for Vase Soul",
        "Harmonica Soul",
        "Tackle Box Soul",
        "Traffic Cone Soul",
        "Exit Parcel Soul",
        "Stealth Box Soul",
        "No Goose Sign Soul",
        "Portable Stool Soul",
        "Dartboard Soul",
        "Mop Bucket Soul",
        "Mop Soul",
        "Burly Mans Bucket Soul",
    },
    "Model Village Prop Souls": {
        "Mini Mail Pillar Soul",
        "Mini Phone Door Soul",
        "Mini Shovel Soul",
        "Poppy Flower Soul",
        "Timber Handle Soul",
        "Birdbath Soul",
        "Easel Soul",
        "Mini Bench Soul",
        "Mini Pump Soul",
        "Sun Lounge Soul",
        "Golden Bell Soul",
    },
    "Fillers": {
        "Mega Honk",
        "Speedy Feet",
        "Silent Steps",
        "A Goose Day",
        "Coin",
    },
    "Traps": {
        "Tired Goose",
        "Confused Feet",
        "Butterbeak",
        "Suspicious Goose",
    },
}


def get_all_prop_souls() -> Set[str]:
    """Returns a set of all prop soul item names"""
    souls = set()
    for group in ["Grouped Prop Souls", "Garden Prop Souls", "High Street Prop Souls", 
                  "Back Gardens Prop Souls", "Pub Prop Souls", "Model Village Prop Souls"]:
        souls.update(ITEM_GROUPS[group])
    return souls


# Mapping from location name patterns to required soul
LOCATION_TO_SOUL: Dict[str, str] = {
    # Grouped souls - pattern matching
    "Carrot": "Carrot Soul",
    "Shop Carrot": "Carrot Soul",
    "Tomato": "Tomato Soul",
    "Pub Tomato": "Tomato Soul",
    "Pumpkin": "Pumpkin Soul",
    "Topsoil Bag": "Topsoil Bag Soul",
    "Quoit": "Quoit Soul",
    "Green Quoit": "Quoit Soul",
    "Red Quoit": "Quoit Soul",
    "Plate": "Plate Soul",
    "Orange": "Orange Soul",
    "Orange Can": "Tinned Food Soul",  # Tinned food from shop
    "Leek": "Leek Soul",
    "Cucumber": "Cucumber Soul",
    "Umbrella": "Umbrella Soul",
    "Black Umbrella": "Umbrella Soul",
    "Rainbow Umbrella": "Umbrella Soul",
    "Red Umbrella": "Umbrella Soul",
    "Blue Can": "Tinned Food Soul",
    "Yellow Can": "Tinned Food Soul",
    "Sock": "Sock Soul",
    "Pint Bottle": "Pint Bottle Soul",
    "Knife": "Knife Soul",
    "Gumboot": "Gumboot Soul",
    "Fork": "Fork Soul",
    "Apple Core": "Apple Core Soul",
    "Apple": "Apple Soul",
    "Sandwich": "Sandwich Soul",
    "Bow": "Bow Soul",
    "Walkie Talkie": "Walkie Talkie Soul",
    "Boot": "Boot Soul",
    "Garden Boot": "Boot Soul",
    "Hub Boot": "Boot Soul",
    "Mini Person": "Mini Person Soul",
    "Mini Bench": "Mini Bench Soul",
    
    # Garden one-offs
    "Radio": "Radio Soul",
    "Trowel": "Trowel Soul",
    "Tulip": "Tulip Soul",
    "Jam": "Jam Soul",
    "Picnic Mug": "Picnic Mug Soul",
    "Thermos": "Thermos Soul",
    "Straw Hat": "Straw Hat Soul",
    "Drink Can": "Drink Can Soul",
    "Tennis Ball": "Tennis Ball Soul",
    "Rake": "Rake Soul",
    "Picnic Basket": "Picnic Basket Soul",
    "Esky": "Esky Soul",
    "Shovel": "Shovel Soul",
    "Watering Can": "Watering Can Soul",
    "Mallet": "Mallet Soul",
    "Wooden Crate": "Wooden Crate Soul",
    
    # High Street one-offs
    "Horn-Rimmed Glasses": "Horn-Rimmed Glasses Soul",
    "Red Glasses": "Red Glasses Soul",
    "Sunglasses": "Sunglasses Soul",
    "Loo Paper": "Loo Paper Soul",
    "Toy Car": "Toy Car Soul",
    "Hairbrush": "Hairbrush Soul",
    "Toothbrush": "Toothbrush Soul",
    "Stereoscope": "Stereoscope Soul",
    "Dish Soap Bottle": "Dish Soap Bottle Soul",
    "Spray Bottle": "Spray Bottle Soul",
    "Weed Tool": "Weed Tool Soul",
    "Lily Flower": "Lily Flower Soul",
    "Fusilage": "Fusilage Soul",
    "Chalk": "Chalk Soul",
    "Dustbin Lid": "Dustbin Lid Soul",
    "Dustbin": "Dustbin Soul",
    "Shopping Basket": "Shopping Basket Soul",
    "Push Broom": "Push Broom Soul",
    "Baby Doll": "Baby Doll Soul",
    "Pricing Gun": "Pricing Gun Soul",
    "Adding Machine": "Adding Machine Soul",
    
    # Back Gardens one-offs
    "Dummy": "Dummy Soul",
    "Cricket Ball": "Cricket Ball Soul",
    "Bust Pipe": "Bust Pipe Soul",
    "Bust Hat": "Bust Hat Soul",
    "Bust Glasses": "Bust Glasses Soul",
    "Tea Cup": "Tea Cup Soul",
    "Newspaper": "Newspaper Soul",
    "Badminton Racket": "Badminton Racket Soul",
    "Pot Stack": "Pot Stack Soul",
    "Soap": "Soap Soul",
    "Paintbrush": "Paintbrush Soul",
    "Vase": "Vase Soul",
    "Bra": "Bra Soul",
    "Rose": "Rose Soul",
    # Removing Rose Box Soul until I can solve the physics issues with it
    # "Rose Box": "Rose Box Soul",
    "Cricket Bat": "Cricket Bat Soul",
    "Tea Pot": "Tea Pot Soul",
    "Clippers": "Clippers Soul",
    "Duck Statue": "Duck Statue Soul",
    "Frog Statue": "Frog Statue Soul",
    "Jeremy Fish": "Jeremy Fish Soul",
    "Messy Sign": "Messy Sign Soul",
    "Drawer": "Drawer Soul",
    "Enamel Jug": "Enamel Jug Soul",
    "Clean Sign": "Clean Sign Soul",
    
    # Pub one-offs
    "Fishing Bobber": "Fishing Bobber Soul",
    "Exit Letter": "Exit Letter Soul",
    "Pint Glass": "Pint Glass Soul",
    "Toy Boat": "Toy Boat Soul",
    "Pepper Grinder": "Pepper Grinder Soul",
    "Cork": "Cork Soul",
    "Candlestick": "Candlestick Soul",
    "Flower for Vase": "Flower for Vase Soul",
    "Harmonica": "Harmonica Soul",
    "Tackle Box": "Tackle Box Soul",
    "Traffic Cone": "Traffic Cone Soul",
    "Exit Parcel": "Exit Parcel Soul",
    "Stealth Box": "Stealth Box Soul",
    "No Goose Sign": "No Goose Sign Soul",
    "Portable Stool": "Portable Stool Soul",
    "Dartboard": "Dartboard Soul",
    "Mop Bucket": "Mop Bucket Soul",
    "Mop": "Mop Soul",
    "Burly Mans Bucket": "Burly Mans Bucket Soul",
    
    # Model Village one-offs
    "Mini Mail Pillar": "Mini Mail Pillar Soul",
    "Mini Phone Door": "Mini Phone Door Soul",
    "Mini Shovel": "Mini Shovel Soul",
    "Poppy Flower": "Poppy Flower Soul",
    "Timber Handle": "Timber Handle Soul",
    "Birdbath": "Birdbath Soul",
    "Easel": "Easel Soul",
    "Mini Pump": "Mini Pump Soul",
    "Sun Lounge": "Sun Lounge Soul",
    
    # Victory item
    "Golden Bell": "Golden Bell Soul",
}


def get_soul_for_location(location_name: str) -> Optional[str]:
    """
    Given a location name like "Pick up Carrot 5" or "Drag Pumpkin",
    returns the required soul name, or None if no soul required.
    """
    # Strip "Pick up " or "Drag " prefix
    item_name = location_name
    if item_name.startswith("Pick up "):
        item_name = item_name[8:]
    elif item_name.startswith("Drag "):
        item_name = item_name[5:]
    
    # Try exact match first
    if item_name in LOCATION_TO_SOUL:
        return LOCATION_TO_SOUL[item_name]
    
    # Try without number suffix (e.g., "Carrot 5" -> "Carrot")
    # Handle cases like "Pub Tomato 11", "Green Quoit 2", etc.
    parts = item_name.rsplit(" ", 1)
    if len(parts) == 2 and parts[1].isdigit():
        base_name = parts[0]
        if base_name in LOCATION_TO_SOUL:
            return LOCATION_TO_SOUL[base_name]
    
    # Try matching start of string for compound names
    for pattern, soul in LOCATION_TO_SOUL.items():
        if item_name.startswith(pattern):
            return soul
    
    return None
