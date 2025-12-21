from typing import TYPE_CHECKING
from worlds.generic.Rules import set_rule

if TYPE_CHECKING:
    from . import GooseGameWorld

# MAIN RULES

def set_rules(world: "GooseGameWorld") -> None:
    multiworld = world.multiworld
    player = world.player
    
    def set_location_rule(location_name, rule_func):
        try:
            location = multiworld.get_location(location_name, player)
            if location:
                set_rule(location, rule_func)
        except:
            pass
    
    for entrance_name, rule in get_region_rules(player).items():
        set_rule(multiworld.get_entrance(entrance_name, player), rule)
    
    for location_name, rule in get_goal_rules(player).items():
        set_location_rule(location_name, rule)
    
    if world.options.include_extra_goals:
        for location_name, rule in get_extra_goal_rules(player).items():
            set_location_rule(location_name, rule)
    
    if world.options.include_speedrun_goals:
        for location_name, rule in get_speedrun_rules(player).items():
            set_location_rule(location_name, rule)
    
    if world.options.include_interactions:
        for location_name, rule in get_interaction_rules(player).items():
            set_location_rule(location_name, rule)
    
    for location_name, rule in get_pickup_rules(player).items():
        set_location_rule(location_name, rule)


# CORE MACROS

def has_area(state, player, area):
    return state.has(f"{area} Access", player)

def has_npc(state, player, npc):
    return state.has(f"{npc} Soul", player)

def has_soul(state, player, item):
    return state.has(f"{item} Soul", player)


# AREA ACCESS

def has_garden(state, player):
    return has_area(state, player, "Garden")

def has_high_street(state, player):
    return has_area(state, player, "High Street")

def has_back_gardens(state, player):
    return has_area(state, player, "Back Gardens")

def has_pub(state, player):
    return has_area(state, player, "Pub")

def has_model_village(state, player):
    return has_area(state, player, "Model Village")


# NPC SOULS

def has_groundskeeper(state, player):
    return has_npc(state, player, "Groundskeeper")

def has_boy(state, player):
    return has_npc(state, player, "Boy")

def has_tv_shop_owner(state, player):
    return has_npc(state, player, "TV Shop Owner")

def has_market_lady(state, player):
    return has_npc(state, player, "Market Lady")

def has_tidy_neighbour(state, player):
    return has_npc(state, player, "Tidy Neighbour")

def has_messy_neighbour(state, player):
    return has_npc(state, player, "Messy Neighbour")

def has_burly_man(state, player):
    return has_npc(state, player, "Burly Man")

def has_old_man(state, player):
    return has_npc(state, player, "Old Man")

def has_pub_lady(state, player):
    return has_npc(state, player, "Pub Lady")

def has_fancy_ladies(state, player):
    return has_npc(state, player, "Fancy Ladies")



# COMPOUND REQUIREMENTS

def has_picnic_items(state, player):
    return (
        has_soul(state, player, "Picnic Basket") 
        and has_soul(state, player, "Apple") 
        and has_soul(state, player, "Sandwich")
    )

def has_bust_items(state, player):
    return (
        has_soul(state, player, "Bust Hat") 
        and has_soul(state, player, "Bust Glasses") 
        and has_soul(state, player, "Bust Pipe")
    )

def has_table_items(state, player):
    return (
        has_soul(state, player, "Plate") 
        and has_soul(state, player, "Fork") 
        and has_soul(state, player, "Knife") 
        and has_soul(state, player, "Pepper Grinder") 
        and has_soul(state, player, "Candlestick") 
        and has_soul(state, player, "Pub Cloth")
    )

def has_shopping_items(state, player):
    return (
        has_soul(state, player, "Shopping Basket") 
        and has_soul(state, player, "Toothbrush") 
        and has_soul(state, player, "Hairbrush") 
        and has_soul(state, player, "Toilet Paper") 
        and has_soul(state, player, "Dish Soap Bottle") 
        and has_soul(state, player, "Orange")
    )

def has_washing_items(state, player):
    return (
        has_soul(state, player, "Sock") 
        and has_soul(state, player, "Right Strap") 
        and has_soul(state, player, "Soap") 
        and has_soul(state, player, "Slipper")
    )

def has_any_wrong_glasses(state, player):
    return (
        has_soul(state, player, "Horn-Rimmed Glasses") 
        or has_soul(state, player, "Red Glasses") 
        or has_soul(state, player, "Sunglasses")
    )

def has_five_flowers(state, player):
    return (
        has_soul(state, player, "Tulip") 
        and has_soul(state, player, "Lily Flower") 
        and has_soul(state, player, "Rose") 
        and has_soul(state, player, "Flower for Vase") 
        and has_soul(state, player, "Poppy Flower")
    )

def has_two_scale_items(state, player):
    item_count = 0
    if has_high_street(state, player) and has_soul(state, player, "Carrot"):
        item_count += 1
    if has_high_street(state, player) and has_soul(state, player, "Tomato"):
        item_count += 1
    if has_high_street(state, player) and has_soul(state, player, "Orange"):
        item_count += 1
    if has_high_street(state, player) and has_soul(state, player, "Leek"):
        item_count += 1
    if has_high_street(state, player) and has_soul(state, player, "Cucumber"):
        item_count += 1
    if has_garden(state, player) and has_soul(state, player, "Apple"):
        item_count += 1
    if has_garden(state, player) and has_soul(state, player, "Jam"):
        item_count += 1
    if has_garden(state, player) and has_soul(state, player, "Sandwich"):
        item_count += 1
    if has_garden(state, player) and has_soul(state, player, "Tulip"):
        item_count += 1
    if has_garden(state, player) and has_soul(state, player, "Picnic Mug"):
        item_count += 1
    if has_garden(state, player) and has_soul(state, player, "Thermos"):
        item_count += 1
    if has_back_gardens(state, player) and has_soul(state, player, "Tea Cup"):
        item_count += 1
    if has_back_gardens(state, player) and has_soul(state, player, "Soap"):
        item_count += 1
    if has_back_gardens(state, player) and has_soul(state, player, "Cricket Ball"):
        item_count += 1
    if has_pub(state, player) and has_soul(state, player, "Cork"):
        item_count += 1
    if has_pub(state, player) and has_soul(state, player, "Quoit"):
        item_count += 1
    return item_count >= 2

# GOAL MACROS

def can_do_washing(state, player):
    return (
        has_back_gardens(state, player) 
        and has_tidy_neighbour(state, player) 
        and has_washing_items(state, player)
    )

def can_make_scales_ding(state, player):
    return (
        has_high_street(state, player) 
        and has_market_lady(state, player) 
        and has_two_scale_items(state, player)
    )


# SPEEDRUN REQUIREMENTS

def can_complete_garden_todos(state, player):
    return (
        has_garden(state, player) 
        and has_groundskeeper(state, player) 
        and has_soul(state, player, "Keys") 
        and has_soul(state, player, "Straw Hat") 
        and has_soul(state, player, "Rake") 
        and has_picnic_items(state, player)
    )

def can_complete_high_street_todos(state, player):
    return (
        has_high_street(state, player) 
        and has_boy(state, player) 
        and has_tv_shop_owner(state, player) 
        and has_market_lady(state, player) 
        and has_soul(state, player, "Boy's Glasses") 
        and has_any_wrong_glasses(state, player) 
        and has_soul(state, player, "Toy Car") 
        and has_soul(state, player, "Push Broom") 
        and has_shopping_items(state, player)
    )

def can_complete_back_gardens_todos(state, player):
    return (
        has_back_gardens(state, player) 
        and has_tidy_neighbour(state, player) 
        and has_messy_neighbour(state, player) 
        and has_soul(state, player, "Tea Cup") 
        and has_soul(state, player, "Slipper") 
        and has_soul(state, player, "Sock") 
        and has_soul(state, player, "Clippers") 
        and has_soul(state, player, "Rose") 
        and has_bust_items(state, player) 
        and has_soul(state, player, "Bow") 
        and has_soul(state, player, "Vase")
    )

def can_complete_pub_todos(state, player):
    return (
        has_pub(state, player) 
        and has_back_gardens(state, player) 
        and has_old_man(state, player) 
        and has_burly_man(state, player) 
        and has_pub_lady(state, player) 
        and has_fancy_ladies(state, player) 
        and has_messy_neighbour(state, player) 
        and has_soul(state, player, "Toy Boat") 
        and has_soul(state, player, "Portable Stool") 
        and has_soul(state, player, "Dartboard") 
        and has_soul(state, player, "Dart") 
        and has_table_items(state, player) 
        and has_soul(state, player, "Mop Bucket") 
        and has_soul(state, player, "Pint Glass") 
        and has_soul(state, player, "Bow") 
        and has_soul(state, player, "Harmonica")
    )


# LOCATION RULES

def get_region_rules(player):
    return {
        "To Garden":
            lambda state: has_garden(state, player),
        "To High Street":
            lambda state: has_high_street(state, player),
        "To Back Gardens":
            lambda state: has_back_gardens(state, player),
        "To Pub":
            lambda state: has_pub(state, player),
        "To Model Village":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player)
            ),
    }


def get_goal_rules(player):
    return {
        # Garden Goals
        "Get the groundskeeper wet":
            lambda state: (
                has_garden(state, player) 
                and has_groundskeeper(state, player)
            ),
        "Steal the groundskeeper's keys":
            lambda state: (
                has_garden(state, player) 
                and has_groundskeeper(state, player) 
                and has_soul(state, player, "Keys")
            ),
        "Make the groundskeeper wear his sun hat":
            lambda state: (
                has_garden(state, player) 
                and has_groundskeeper(state, player) 
                and has_soul(state, player, "Straw Hat") 
                and has_soul(state, player, "Tulip")
            ),
        "Rake in the lake":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Rake")
            ),
        "Have a picnic":
            lambda state: (
                has_garden(state, player) 
                and has_picnic_items(state, player)
            ),
        "Make the groundskeeper hammer his thumb":
            lambda state: (
                has_garden(state, player) 
                and has_groundskeeper(state, player) 
                and has_soul(state, player, "Mallet") 
                and has_high_street(state, player)
            ),
        
        # High Street Goals
        "Trap the boy in the phone booth":
            lambda state: (
                has_high_street(state, player) 
                and has_boy(state, player)
            ),
        "Make the boy wear the wrong glasses":
            lambda state: (
                has_high_street(state, player) 
                and has_boy(state, player) 
                and has_soul(state, player, "Boy's Glasses") 
                and has_any_wrong_glasses(state, player)
            ),
        "Make someone buy back their own stuff":
            lambda state: (
                has_high_street(state, player) 
                and has_boy(state, player) 
                and has_market_lady(state, player) 
                and has_soul(state, player, "Fusilage")
            ),
        "Get on TV":
            lambda state: (
                has_high_street(state, player) 
                and has_tv_shop_owner(state, player)
            ),
        "Break the broom":
            lambda state: (
                has_high_street(state, player) 
                and has_market_lady(state, player) 
                and has_soul(state, player, "Push Broom")
            ),
        "Go shopping":
            lambda state: (
                has_high_street(state, player) 
                and has_market_lady(state, player) 
                and has_tv_shop_owner(state, player) 
                and has_shopping_items(state, player)
            ),
        "Trap the shopkeeper in the garage":
            lambda state: (
                has_high_street(state, player) 
                and has_market_lady(state, player) 
                and can_complete_high_street_todos(state, player)
            ),
        
        # Back Gardens Goals
        "Make the man spit out his tea":
            lambda state: (
                has_back_gardens(state, player) 
                and has_tidy_neighbour(state, player) 
                and has_messy_neighbour(state, player) 
                and has_soul(state, player, "Tea Cup")
            ),
        "Make the man go barefoot":
            lambda state: (
                has_back_gardens(state, player) 
                and has_tidy_neighbour(state, player) 
                and has_soul(state, player, "Slipper")
            ),
        "Do the washing":
            lambda state: can_do_washing(state, player),
        "Make someone prune the prize rose":
            lambda state: (
                has_back_gardens(state, player) 
                and has_tidy_neighbour(state, player) 
                and has_messy_neighbour(state, player) 
                and has_soul(state, player, "Clippers") 
                and has_soul(state, player, "Rose")
            ),
        "Help the woman dress up the bust":
            lambda state: (
                has_back_gardens(state, player) 
                and has_messy_neighbour(state, player) 
                and has_bust_items(state, player)
            ),
        "Get dressed up with a ribbon":
            lambda state: (
                has_back_gardens(state, player) 
                and has_messy_neighbour(state, player) 
                and has_soul(state, player, "Bow")
            ),
        "Make someone break the fancy vase":
            lambda state: (
                has_back_gardens(state, player) 
                and has_tidy_neighbour(state, player) 
                and has_messy_neighbour(state, player) 
                and has_soul(state, player, "Vase")
            ),
        
        # Pub Goals
        "Get the toy boat":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Toy Boat")
            ),
        "Make the old man fall on his bum":
            lambda state: (
                has_pub(state, player) 
                and has_old_man(state, player) 
                and has_soul(state, player, "Portable Stool") 
                and has_soul(state, player, "Harmonica")
            ),
        "Break the dartboard":
            lambda state: (
                has_pub(state, player) 
                and has_old_man(state, player) 
                and has_soul(state, player, "Dartboard") 
                and has_soul(state, player, "Dart")
            ),
        "Set the table":
            lambda state: (
                has_pub(state, player) 
                and has_table_items(state, player)
            ),
        "Steal a pint glass":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Pint Glass")
            ),
        "Drop a bucket on the burly man's head":
            lambda state: (
                has_pub(state, player) 
                and has_burly_man(state, player) 
                and has_soul(state, player, "Mop Bucket")
            ),
        "Be awarded a flower":
            lambda state: (
                has_pub(state, player) 
                and has_back_gardens(state, player) 
                and has_fancy_ladies(state, player) 
                and has_soul(state, player, "Flower for Vase")
            ),
        
        # Victory
        "Complete the game":
            lambda state: (
                has_garden(state, player) 
                and has_high_street(state, player) 
                and has_back_gardens(state, player) 
                and has_pub(state, player) 
                and has_model_village(state, player) 
                and state.has("Golden Bell", player)
            ),
    }


def get_extra_goal_rules(player):
    return {
        "Complete all goals":
            lambda state: state.can_reach_location("Complete the game", player),
        "Lock the groundskeeper out of the garden":
            lambda state: (
                has_garden(state, player) 
                and has_groundskeeper(state, player) 
                and has_soul(state, player, "Keys")
            ),
        "Cabbage picnic":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Picnic Basket")
            ),
        "Trip the boy in the puddle":
            lambda state: (
                has_high_street(state, player) 
                and has_boy(state, player)
            ),
        "Trap the boy in the garage":
            lambda state: (
                has_high_street(state, player) 
                and has_boy(state, player) 
                and has_market_lady(state, player) 
                and can_complete_high_street_todos(state, player)
            ),
        "Make the scales go ding":
            lambda state: can_make_scales_ding(state, player),
        "Open an umbrella inside the TV shop":
            lambda state: (
                has_high_street(state, player) 
                and has_tv_shop_owner(state, player) 
                and has_soul(state, player, "Umbrella")
            ),
        "Collect the five flowers":
            lambda state: (
                has_garden(state, player) 
                and has_high_street(state, player) 
                and has_back_gardens(state, player) 
                and has_pub(state, player) 
                and has_model_village(state, player) 
                and has_five_flowers(state, player)
            ),
        "Make someone from outside the high street buy back their own stuff":
            lambda state: (
                has_high_street(state, player) 
                and has_garden(state, player) 
                and has_market_lady(state, player) 
                and has_groundskeeper(state, player) 
                and has_soul(state, player, "Trowel")
            ),
        "Dress up the bust with things from outside the back gardens":
            lambda state: (
                has_back_gardens(state, player) 
                and has_high_street(state, player) 
                and has_messy_neighbour(state, player)
            ),
        "Perform at the pub wearing a ribbon":
            lambda state: (
                has_pub(state, player) 
                and has_back_gardens(state, player) 
                and has_messy_neighbour(state, player) 
                and has_fancy_ladies(state, player) 
                and has_soul(state, player, "Bow") 
                and has_soul(state, player, "Harmonica")
            ),
        "Get thrown over the fence":
            lambda state: (
                has_back_gardens(state, player) 
                and has_pub(state, player) 
                and has_tidy_neighbour(state, player) 
                and has_soul(state, player, "Stealth Box")
            ),
        "Get caught by the man over the fence":
            lambda state: (
                has_back_gardens(state, player) 
                and has_pub(state, player) 
                and has_tidy_neighbour(state, player)
            ),
        "Steal the old man's woolen hat":
            lambda state: (
                has_pub(state, player) 
                and has_old_man(state, player) 
                and has_soul(state, player, "Wooly Hat")
            ),
        "Sail the toy boat under the bridge":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Toy Boat")
            ),
        "Score a goal":
            lambda state: (
                has_back_gardens(state, player) 
                and has_high_street(state, player)
            ),
    }


def get_speedrun_rules(player):
    return {
        "Complete Garden before noon":
            lambda state: can_complete_garden_todos(state, player),
        "Complete High Street before noon":
            lambda state: can_complete_high_street_todos(state, player),
        "Complete Back Gardens before noon":
            lambda state: can_complete_back_gardens_todos(state, player),
        "Complete Pub before noon":
            lambda state: can_complete_pub_todos(state, player),
    }


def get_interaction_rules(player):
    return {
        # Hub
        "Open Intro Gate":
            lambda state: True,
        
        # Garden
        "Ring the Bike Bell":
            lambda state: has_garden(state, player),
        
        # High Street
        "Unplug the Radio":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Radio")
            ),
        "Open Black Umbrella":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Umbrella")
            ),
        "Open Rainbow Umbrella":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Umbrella")
            ),
        "Open Red Umbrella":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Umbrella")
            ),
        "Untie Boy's Laces (Left)":
            lambda state: (
                has_high_street(state, player) 
                and has_boy(state, player)
            ),
        "Untie Boy's Laces (Right)":
            lambda state: (
                has_high_street(state, player) 
                and has_boy(state, player)
            ),
        
        # Back Gardens
        "Ring the Back Gardens Bell":
            lambda state: has_back_gardens(state, player),
        "Play Wind Chime (G)":
            lambda state: has_back_gardens(state, player),
        "Play Wind Chime (F)":
            lambda state: has_back_gardens(state, player),
        "Play Wind Chime (E)":
            lambda state: has_back_gardens(state, player),
        "Play Wind Chime (D)":
            lambda state: has_back_gardens(state, player),
        "Play Wind Chime (C)":
            lambda state: has_back_gardens(state, player),
        "Play Wind Chime (B)":
            lambda state: has_back_gardens(state, player),
        "Play Wind Chime (A)":
            lambda state: has_back_gardens(state, player),
        
        # Pub
        "Untie Burly Man's Laces (Left)":
            lambda state: (
                has_pub(state, player) 
                and has_burly_man(state, player)
            ),
        "Untie Burly Man's Laces (Right)":
            lambda state: (
                has_pub(state, player) 
                and has_burly_man(state, player)
            ),
    }


def get_pickup_rules(player):
    return {
        # GARDEN PICKUPS
        "Pick up Radio":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Radio")
            ),
        "Pick up Trowel":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Trowel")
            ),
        "Pick up Keys":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Keys")
            ),
        "Pick up Carrot":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Carrot")
            ),
        "Pick up Tulip":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Tulip")
            ),
        "Pick up Apple":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Apple")
            ),
        "Pick up Jam":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Jam")
            ),
        "Pick up Picnic Mug":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Picnic Mug")
            ),
        "Pick up Thermos":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Thermos")
            ),
        "Pick up Sandwich (Right)":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Sandwich")
            ),
        "Pick up Sandwich (Left)":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Sandwich")
            ),
        "Pick up Straw Hat":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Straw Hat")
            ),
        "Pick up Drink Can":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Drink Can")
            ),
        "Pick up Tennis Ball":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Tennis Ball")
            ),
        "Pick up Gardener Hat":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Gardener Hat")
            ),
        "Pick up Apple 2":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Apple")
            ),
        "Pick up Garden Boot":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Boot")
            ),
        # Garden Carrots (unique locations)
        "Pick up Carrot 1":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Carrot")
            ),
        "Pick up Carrot 2":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Carrot")
            ),
        "Pick up Carrot 3":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Carrot")
            ),
        "Pick up Carrot 4":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Carrot")
            ),
        "Pick up Carrot 5":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Carrot")
            ),
        "Pick up Carrot 6":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Carrot")
            ),
        "Pick up Carrot 7":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Carrot")
            ),
        "Pick up Carrot 8":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Carrot")
            ),
        "Pick up Carrot 9":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Carrot")
            ),
        "Pick up Carrot 10":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Carrot")
            ),
        
        # GARDEN DRAGS
        "Drag Rake":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Rake")
            ),
        "Drag Picnic Basket":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Picnic Basket")
            ),
        "Drag Esky":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Esky")
            ),
        "Drag Shovel":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Shovel")
            ),
        "Drag Pumpkin":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Pumpkin")
            ),
        "Drag Pumpkin 2":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Pumpkin")
            ),
        "Drag Pumpkin 3":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Pumpkin")
            ),
        "Drag Pumpkin 4":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Pumpkin")
            ),
        "Drag Watering Can":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Watering Can")
            ),
        "Drag Gumboot 1":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Gumboot")
            ),
        "Drag Gumboot 2":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Gumboot")
            ),
        "Drag Gardener Sign":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Gardener Sign")
            ),
        "Drag Wooden Crate":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Wooden Crate")
            ),
        "Drag Fence Bolt":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Fence Bolt")
            ),
        "Drag Mallet":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Mallet")
            ),
        "Drag Topsoil Bag 1":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Topsoil Bag")
            ),
        "Drag Topsoil Bag 2":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Topsoil Bag")
            ),
        "Drag Topsoil Bag 3":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Topsoil Bag")
            ),
        
        # HIGH STREET PICKUPS
        "Pick up Boy's Glasses":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Boy's Glasses")
            ),
        "Pick up Horn-Rimmed Glasses":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Horn-Rimmed Glasses")
            ),
        "Pick up Red Glasses":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Red Glasses")
            ),
        "Pick up Sunglasses":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Sunglasses")
            ),
        "Pick up Toilet Paper":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Toilet Paper")
            ),
        "Pick up Toy Car":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Toy Car")
            ),
        "Pick up Hairbrush":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Hairbrush")
            ),
        "Pick up Toothbrush":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Toothbrush")
            ),
        "Pick up Stereoscope":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Stereoscope")
            ),
        "Pick up Dish Soap Bottle":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Dish Soap Bottle")
            ),
        "Pick up Blue Can":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Spray Can")
            ),
        "Pick up Yellow Can":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Spray Can")
            ),
        "Pick up Orange Can":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Spray Can")
            ),
        "Pick up Weed Tool":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Weed Tool")
            ),
        "Pick up Lily Flower":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Lily Flower")
            ),
        "Pick up Orange":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Orange")
            ),
        "Pick up Orange 2":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Orange")
            ),
        "Pick up Orange 3":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Orange")
            ),
        "Pick up Tomato 1":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Tomato")
            ),
        "Pick up Tomato 2":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Tomato")
            ),
        "Pick up Tomato 3":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Tomato")
            ),
        "Pick up Shop Carrot 1":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Carrot")
            ),
        "Pick up Shop Carrot 2":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Carrot")
            ),
        "Pick up Shop Carrot 3":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Carrot")
            ),
        "Pick up Carrot 11":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Carrot")
            ),
        "Pick up Carrot 12":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Carrot")
            ),
        "Pick up Carrot 13":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Carrot")
            ),
        "Pick up Cucumber 1":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Cucumber")
            ),
        "Pick up Cucumber 2":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Cucumber")
            ),
        "Pick up Cucumber 3":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Cucumber")
            ),
        "Pick up Leek 1":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Leek")
            ),
        "Pick up Leek 2":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Leek")
            ),
        "Pick up Leek 3":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Leek")
            ),
        "Pick up Fusilage":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Fusilage")
            ),
        "Pick up Pint Bottle":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Pint Bottle")
            ),
        "Pick up Pint Bottle 2":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Pint Bottle")
            ),
        "Pick up Spray Bottle":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Spray Bottle")
            ),
        "Pick up Walkie Talkie":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Walkie Talkie")
            ),
        "Pick up Walkie Talkie B":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Walkie Talkie")
            ),
        "Pick up Apple Core":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Apple Core")
            ),
        "Pick up Apple Core 2":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Apple Core")
            ),
        "Pick up Dustbin Lid":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Dustbin Lid")
            ),
        "Pick up Coin":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Coin")
            ),
        "Pick up Chalk":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Chalk")
            ),

        # HIGH STREET DRAGS
        "Drag Shopping Basket":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Shopping Basket")
            ),
        "Drag Black Umbrella":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Umbrella")
            ),
        "Drag Push Broom":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Push Broom")
            ),
        "Drag Broken Broom Head":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Broken Broom Head")
            ),
        "Drag Dustbin":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Dustbin")
            ),
        "Drag Baby Doll":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Baby Doll")
            ),
        "Drag Pricing Gun":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Pricing Gun")
            ),
        "Drag Adding Machine":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Adding Machine")
            ),
        "Drag Rainbow Umbrella":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Umbrella")
            ),
        "Drag Red Umbrella":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Umbrella")
            ),
        
        # BACK GARDENS PICKUPS
        "Pick up Bow (Blue)":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Bow")
            ),
        "Pick up Bow":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Bow")
            ),
        "Pick up Dummy":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Dummy")
            ),
        "Pick up Cricket Ball":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Cricket Ball")
            ),
        "Pick up Bust Pipe":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Bust Pipe")
            ),
        "Pick up Bust Hat":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Bust Hat")
            ),
        "Pick up Bust Glasses":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Bust Glasses")
            ),
        "Pick up Right Slipper":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Slipper") 
                and has_tidy_neighbour(state, player)
            ),
        "Pick up Left Slipper":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Slipper") 
                and has_tidy_neighbour(state, player)
            ),
        "Pick up Tea Cup":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Tea Cup")
            ),
        "Pick up Newspaper":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Newspaper")
            ),
        "Pick up Socks":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Sock")
            ),
        "Pick up Socks 2":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Sock")
            ),
        "Pick up Vase":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Vase")
            ),
        "Pick up Pot Stack":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Pot Stack")
            ),
        "Pick up Soap":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Soap")
            ),
        "Pick up Paintbrush":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Paintbrush")
            ),
        "Pick up Broken Vase Piece 1":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Vase Piece")
            ),
        "Pick up Broken Vase Piece 2":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Vase Piece")
            ),
        "Pick up Right Strap":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Right Strap")
            ),
        "Pick up Badminton Racket":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Badminton Racket")
            ),
        "Pick up Rose":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Rose")
            ),
        
        # BACK GARDENS DRAGS
        "Drag Rose Box":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Rose Box")
            ),
        "Drag Cricket Bat":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Cricket Bat")
            ),
        "Drag Tea Pot":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Tea Pot")
            ),
        "Drag Clippers":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Clippers")
            ),
        "Drag Duck Statue":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Duck Statue")
            ),
        "Drag Frog Statue":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Frog Statue")
            ),
        "Drag Jeremy Fish":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Jeremy Fish")
            ),
        "Drag Messy Sign":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Messy Sign")
            ),
        "Drag Drawer":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Drawer")
            ),
        "Drag Enamel Jug":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Enamel Jug")
            ),
        "Drag Clean Sign":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Clean Sign")
            ),
        
        # PUB PICKUPS

        "Pick up Fishing Bobber":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Fishing Bobber")
            ),
        "Pick up Exit Letter":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Exit Letter")
            ),
        "Pick up Plate":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Plate")
            ),
        "Pick up Plate 2":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Plate")
            ),
        "Pick up Plate 3":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Plate")
            ),
        "Pick up Green Quoit 1":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Quoit")
            ),
        "Pick up Green Quoit 2":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Quoit")
            ),
        "Pick up Green Quoit 3":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Quoit")
            ),
        "Pick up Red Quoit 1":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Quoit")
            ),
        "Pick up Red Quoit 2":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Quoit")
            ),
        "Pick up Red Quoit 3":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Quoit")
            ),
        "Pick up Fork":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Fork")
            ),
        "Pick up Fork 2":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Fork")
            ),
        "Pick up Knife":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Knife")
            ),
        "Pick up Knife 2":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Knife")
            ),
        "Pick up Cork":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Cork")
            ),
        "Pick up Candlestick":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Candlestick")
            ),
        "Pick up Flower for Vase":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Flower for Vase")
            ),
        "Pick up Dart 1":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Dart")
            ),
        "Pick up Dart 2":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Dart")
            ),
        "Pick up Dart 3":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Dart")
            ),
        "Pick up Harmonica":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Harmonica")
            ),
        "Pick up Pint Glass":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Pint Glass")
            ),
        "Pick up Toy Boat":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Toy Boat")
            ),
        "Pick up Wooly Hat":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Wooly Hat") 
                and has_old_man(state, player)
            ),
        "Pick up Pepper Grinder":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Pepper Grinder")
            ),
        "Pick up Pub Woman's Cloth":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Pub Cloth") 
                and has_pub_lady(state, player)
            ),
        # Pub Tomatoes (unique locations)
        "Pick up Pub Tomato 1":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
            ),
        "Pick up Pub Tomato 2":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
            ),
        "Pick up Pub Tomato 3":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
            ),
        "Pick up Pub Tomato 4":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
            ),
        "Pick up Pub Tomato 5":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
            ),
        "Pick up Pub Tomato 6":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
            ),
        "Pick up Pub Tomato 7":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
            ),
        "Pick up Pub Tomato 8":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
            ),
        "Pick up Pub Tomato 9":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
            ),
        "Pick up Pub Tomato 10":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
            ),
        "Pick up Pub Tomato 11":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
            ),
        
        # PUB DRAGS
        "Drag Tackle Box":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tackle Box")
            ),
        "Drag Traffic Cone":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Traffic Cone")
            ),
        "Drag Exit Parcel":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Exit Parcel")
            ),
        "Drag Stealth Box":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Stealth Box")
            ),
        "Drag No Goose Sign":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "No Goose Sign")
            ),
        "Drag Portable Stool":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Portable Stool")
            ),
        "Drag Dartboard":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Dartboard")
            ),
        "Drag Mop Bucket":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Mop Bucket")
            ),
        "Drag Mop":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Mop")
            ),
        "Drag Delivery Box":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Delivery Box")
            ),
        # "Drag Tomato Box":
        #     lambda state: (
        #         has_pub(state, player) 
        #         and has_soul(state, player, "Tomato Box")
        #     ),
        
        # MODEL VILLAGE PICKUPS
        "Pick up Mini Person (Child)":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Mini Person")
            ),
        "Pick up Mini Person (Jumpsuit)":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Mini Person")
            ),
        "Pick up Mini Person (Gardener)":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Mini Person")
            ),
        "Pick up Mini Person (Old Woman)":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Mini Person")
            ),
        "Pick up Mini Person (Postie)":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Mini Person")
            ),
        "Pick up Mini Person (Vest Man)":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Mini Person")
            ),
        "Pick up Mini Person":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Mini Person")
            ),
        "Pick up Mini Person (Goose)":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Mini Person")
            ),
        "Pick up Mini Shovel":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Mini Shovel")
            ),
        "Pick up Poppy Flower":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Poppy Flower")
            ),
        "Pick up Mini Phone Door":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Mini Phone Door")
            ),
        "Pick up Mini Mail Pillar":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Mini Mail Pillar")
            ),
        "Pick up Timber Handle":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Timber Handle")
            ),
        "Pick up Golden Bell":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Golden Bell")
            ),
        
        # MODEL VILLAGE DRAGS
        "Drag Mini Bench":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Mini Bench")
            ),
        "Drag Mini Pump":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Mini Pump")
            ),
        "Drag Mini Street Bench":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Mini Street Bench")
            ),
        "Drag Birdbath":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Birdbath")
            ),
        "Drag Easel":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Easel")
            ),
        "Drag Sun Lounge":
            lambda state: (
                has_pub(state, player) 
                and has_model_village(state, player) 
                and has_soul(state, player, "Sun Lounge")
            ),
        
        # HUB
        "Pick up Hub Boot":
            lambda state: has_soul(state, player, "Boot"),
    }
