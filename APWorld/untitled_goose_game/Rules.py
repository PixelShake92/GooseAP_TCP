from typing import TYPE_CHECKING
from worlds.generic.Rules import set_rule

if TYPE_CHECKING:
    from . import GooseGameWorld

# MAIN RULES

def set_rules(world: "GooseGameWorld") -> None:
    multiworld = world.multiworld
    player = world.player
    
    # Extract soul options
    include_npc_souls = bool(world.options.include_npc_souls.value)
    include_prop_souls = bool(world.options.include_prop_souls.value)
    
    def set_location_rule(location_name, rule_func):
        location = multiworld.get_location(location_name, player)
        if location:
            set_rule(location, rule_func)
    
    for entrance_name, rule in get_region_rules(player).items():
        set_rule(multiworld.get_entrance(entrance_name, player), rule)
    
    for location_name, rule in get_goal_rules(player, include_npc_souls, include_prop_souls).items():
        set_location_rule(location_name, rule)
    
    if world.options.include_extra_goals:
        for location_name, rule in get_extra_goal_rules(player, include_npc_souls, include_prop_souls).items():
            set_location_rule(location_name, rule)
    
    if world.options.include_speedrun_goals:
        for location_name, rule in get_speedrun_rules(player, include_npc_souls, include_prop_souls).items():
            set_location_rule(location_name, rule)
    
    if world.options.include_interactions:
        for location_name, rule in get_interaction_rules(player, include_npc_souls, include_prop_souls).items():
            set_location_rule(location_name, rule)
    
    for location_name, rule in get_pickup_rules(player, include_npc_souls, include_prop_souls).items():
        set_location_rule(location_name, rule)


# MACROS - Area access (always required, not affected by options)

def has_area(state, player, area):
    return state.has(f"{area} Access", player)

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


# REGION RULES (no soul requirements)

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


# GOAL RULES

def get_goal_rules(player, include_npc_souls, include_prop_souls):
    # Option-aware helpers - return True if option disabled (no soul needed)
    def has_npc(state, player, npc):
        if not include_npc_souls:
            return True
        return state.has(f"{npc} Soul", player)
    
    def has_soul(state, player, item):
        if not include_prop_souls:
            return True
        return state.has(f"{item} Soul", player)
    
    # Compound requirements
    def has_picnic_items(state, player):
        return (
            has_soul(state, player, "Picnic Basket") 
            and has_soul(state, player, "Apple") 
            and has_soul(state, player, "Sandwich")
            and has_soul(state, player, "Pumpkin")
            and has_soul(state, player, "Jam")
            and has_soul(state, player, "Thermos")
        )
    
    def has_bust_items(state, player):
        return (
            has_soul(state, player, "Bust Hat") 
            and has_soul(state, player, "Bust Glasses") 
            and has_soul(state, player, "Bust Pipe")
        )
    
    def has_bust_outside_items(state, player):
        return (
            has_back_gardens(state, player)
            and has_npc(state, player, "Messy Neighbour")
            and has_high_street(state, player) # Hard-required, as there are no eyewear outside of High Street aside from the glasses normally meant for the bust
            and (
                has_soul(state, player, "Horn-Rimmed Glasses")
                or has_soul(state, player, "Sunglasses")
                or has_soul(state, player, "Stereoscope")
                or (has_soul(state, player, "Boy's Glasses") and has_npc(state, player, "Boy"))
            )
            and (
                has_soul(state, player, "Dummy") # Doesn't require any access
                or has_garden(state, player) and has_soul(state, player, "Tulip")
                or has_soul(state, player, "Toothbrush") # High Street
                or has_soul(state, player, "Lily Flower") # High Street
                or has_pub(state, player)
                and (
                    has_soul(state, player, "Knife")
                    or has_soul(state, player, "Fork")
                    or has_soul(state, player, "Flower for Vase Soul") and has_npc(state, player, "Fancy Ladies Soul")
                    or has_model_village(state, player) and has_soul(state, player, "Poppy Flower")
                )
            )
            and (
                has_garden(state, player)
                and (
                    has_soul(state, player, "Gardener Hat") and has_npc(state, player, "Groundskeeper")
                    # No need to include Straw Hat at the moment as it logically requires the Gardener Hat
                )
                or has_pub(state, player)
                and (
                    has_soul(state, player, "Traffic Cone")
                    or has_soul(state, player, "Wooly Hat") and has_npc(state, player, "Old Man")
                )
            )
        )
    
    def has_table_items(state, player):
        return (
            has_soul(state, player, "Plate") 
            and has_soul(state, player, "Fork") 
            and has_soul(state, player, "Knife") 
            and has_soul(state, player, "Pepper Grinder") 
            and has_soul(state, player, "Candlestick")
        )
    
    def has_shopping_items(state, player):
        return (
            has_soul(state, player, "Shopping Basket") 
            and has_soul(state, player, "Toothbrush") 
            and has_soul(state, player, "Hairbrush") 
            and has_soul(state, player, "Loo Paper")
            and (
                has_soul(state, player, "Dish Soap Bottle") 
                or has_soul(state, player, "Spray Bottle")
            )
            and (
                has_soul(state, player, "Orange") 
                or has_soul(state, player, "Cucumber") 
                or has_soul(state, player, "Leek") 
                or has_soul(state, player, "Carrot") 
                or has_soul(state, player, "Tomato")
                or (has_soul(state, player, "Apple") and has_garden(state, player))
            )
        )
    
    def has_any_wrong_glasses(state, player):
        return (
            has_soul(state, player, "Horn-Rimmed Glasses") 
            or has_soul(state, player, "Red Glasses") 
            or has_soul(state, player, "Sunglasses")
        )
    
    def has_bust_items_in_back_gardens_without_pipe(state, player):
        return (
            has_high_street(state, player)
            and has_soul(state, player, "Bust Hat") 
            and has_soul(state, player, "Bust Glasses") 
            and has_npc(state, player, "Tidy Neighbour")
            and has_npc(state, player, "Messy Neighbour")
            and has_soul(state, player, "Slipper")
            and has_soul(state, player, "Rose")
            and has_soul(state, player, "Rose Box")
            and has_soul(state, player, "Clippers")
            and has_soul(state, player, "Clean Sign") # I could be wrong about this one, but I suspect you can't move the box until he moves the sign, even if the sign hasn't spawned in
            and has_soul(state, player, "Tea Cup")
            and has_soul(state, player, "Sock")
            and has_soul(state, player, "Right Strap")
            and has_soul(state, player, "Soap")
            and has_soul(state, player, "Bow")
            and has_soul(state, player, "Vase")
        )
    
    def can_access_final_garden_task(state, player):
        # Without all of these, this is impossible
        if (
            not has_garden(state, player)
            or not has_npc(state, player, "Groundskeeper")
            or not has_soul(state, player, "Mallet")
            or not has_soul(state, player, "Gardener Sign")
        ):
            return False
        
        # Count total available tasks; the final task is available if we can do at least 5
        task_count = 2 # Getting into the Garden and getting the Groundskeeper wet are free with the above conditions
        
        if has_soul(state, player, "Keys"):
            task_count += 1
        if has_soul(state, player, "Straw Hat") and has_soul(state, player, "Tulip"):
            task_count += 1
        if has_soul(state, player, "Rake"):
            task_count += 1
        if has_picnic_items(state, player):
            task_count += 1
        
        return task_count >= 5
    
    def can_access_final_high_street_task(state, player):
        # Without all of these, this is impossible
        if (
            not has_high_street(state, player)
            or not has_npc(state, player, "Boy")
            or not has_npc(state, player, "Market Lady")
        ):
            return False
        
        # Count total available tasks; the final task is available if we can do at least 5
        task_count = 1 # Trapping the boy in the phone booth is free with the above conditions
        
        if has_soul(state, player, "Boy's Glasses") and has_any_wrong_glasses(state, player):
            task_count += 1
        if (
            has_soul(state, player, "Fusilage")
            or has_garden(state, player)
            and has_npc(state, player, "Groundskeeper") 
            and has_soul(state, player, "Trowel")
        ):
            task_count += 1
        if has_npc(state, player, "TV Shop Owner"):
            task_count += 1
        if has_soul(state, player, "Push Broom"):
            task_count += 1
        if has_shopping_items(state, player):
            task_count += 1
        
        return task_count >= 5
    
    def can_access_final_back_gardens_task(state, player):
        # Without all of these, this is impossible
        if (
            not has_high_street(state, player)
            or not has_npc(state, player, "Tidy Neighbour")
            or not has_npc(state, player, "Messy Neighbour")
            or not has_soul(state, player, "Slipper")
            or not has_soul(state, player, "Rose")
            or not has_soul(state, player, "Rose Box")
            or not has_soul(state, player, "Clippers")
            or not has_soul(state, player, "Clean Sign") # I could be wrong about this one, but I suspect you can't move the box until he moves the sign, even if the sign hasn't spawned in
        ):
            return False
        
        # Count total available tasks; the final task is available if we can do at least 5
        task_count = 1 # Making the man go barefoot is free with the above conditions
        
        if has_soul(state, player, "Tea Cup"):
            task_count += 1
        if (
            has_soul(state, player, "Sock") 
            and has_soul(state, player, "Right Strap") 
            and has_soul(state, player, "Soap") 
        ):
            task_count += 1
        if has_bust_items(state, player) or has_bust_outside_items(state, player):
            task_count += 1
        if has_soul(state, player, "Bow"):
            task_count += 1
        if has_soul(state, player, "Vase"):
            task_count += 1
        
        return task_count >= 5
    
    def can_access_final_pub_task(state, player):
        # Without all of these, this is impossible
        if (
            not has_high_street(state, player)
            or not has_npc(state, player, "Old Man")
            or not has_npc(state, player, "Burly Man Soul")
            or not has_soul(state, player, "Burly Mans Bucket")
            or not has_soul(state, player, "Tomato")
        ):
            return False
        
        # Count total available tasks; the final task is available if we can do at least 5
        task_count = 0
        
        if has_soul(state, player, "Toy Boat"):
            task_count += 1
        if has_soul(state, player, "Portable Stool"):
            task_count += 1
        if has_soul(state, player, "Dartboard") and has_soul(state, player, "Dart"):
            task_count += 1
        if has_table_items(state, player):
            task_count += 1
        if has_soul(state, player, "Pint Glass"):
            task_count += 1
        if has_npc(state, player, "Fancy Ladies") and has_soul(state, player, "Flower for Vase"):
            task_count += 1
        
        return task_count >= 5
    
    return {
        # Garden Goals
        "Get the groundskeeper wet":
            lambda state: (
                has_garden(state, player) 
                and has_npc(state, player, "Groundskeeper")
            ),
        "Steal the groundskeeper's keys":
            lambda state: (
                has_garden(state, player) 
                and has_npc(state, player, "Groundskeeper") 
                and has_soul(state, player, "Keys")
            ),
        "Make the groundskeeper wear his sun hat":
            lambda state: (
                has_garden(state, player) 
                and has_npc(state, player, "Groundskeeper") 
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
            lambda state: can_access_final_garden_task(state, player),
        
        # High Street Goals
        "Trap the boy in the phone booth":
            lambda state: (
                has_high_street(state, player) 
                and has_npc(state, player, "Boy")
            ),
        "Make the boy wear the wrong glasses":
            lambda state: (
                has_high_street(state, player) 
                and has_npc(state, player, "Boy") 
                and has_soul(state, player, "Boy's Glasses") 
                and has_any_wrong_glasses(state, player)
            ),
        "Make someone buy back their own stuff":
            lambda state: (
                has_high_street(state, player) 
                and has_npc(state, player, "Market Lady") 
                and (
                    has_npc(state, player, "Boy") 
                    and has_soul(state, player, "Fusilage")
                    or has_garden(state, player)
                    and has_npc(state, player, "Groundskeeper") 
                    and has_soul(state, player, "Trowel")
                )
            ),
        "Get on TV":
            lambda state: (
                has_high_street(state, player) 
                and has_npc(state, player, "TV Shop Owner") 
                and has_npc(state, player, "Boy")
            ),
        "Break the broom":
            lambda state: (
                has_high_street(state, player) 
                and has_npc(state, player, "Market Lady") 
                and has_soul(state, player, "Push Broom")
            ),
        "Go shopping":
            lambda state: (
                has_high_street(state, player) 
                and has_shopping_items(state, player)
            ),
        "Trap the shopkeeper in the garage":
            lambda state: can_access_final_high_street_task(state, player),
        
        # Back Gardens Goals
        "Make the man spit out his tea":
            lambda state: (
                has_back_gardens(state, player) 
                and has_npc(state, player, "Tidy Neighbour") 
                and has_soul(state, player, "Tea Cup")
            ),
        "Make the man go barefoot":
            lambda state: (
                has_back_gardens(state, player) 
                and has_npc(state, player, "Tidy Neighbour") 
                and has_soul(state, player, "Slipper")
            ),
        "Do the washing":
            lambda state: (
                has_back_gardens(state, player) 
                and has_npc(state, player, "Tidy Neighbour") 
                and has_soul(state, player, "Sock") 
                and has_soul(state, player, "Right Strap") 
                and has_soul(state, player, "Soap") 
                and has_soul(state, player, "Slipper")
            ),
        "Make someone prune the prize rose":
            lambda state: can_access_final_back_gardens_task(state, player),
        "Help the woman dress up the bust":
            lambda state: (
                has_back_gardens(state, player) 
                and has_npc(state, player, "Messy Neighbour") 
                and (
                    has_bust_items(state, player)
                    or has_bust_outside_items(state, player)
                    or has_bust_items_in_back_gardens_without_pipe(state, player) # using the Rose instead
                )
            ),
        "Get dressed up with a ribbon":
            lambda state: (
                has_back_gardens(state, player) 
                and has_npc(state, player, "Messy Neighbour") 
                and has_soul(state, player, "Bow")
            ),
        "Make someone break the fancy vase":
            lambda state: (
                has_back_gardens(state, player) 
                and has_npc(state, player, "Tidy Neighbour") 
                and has_npc(state, player, "Messy Neighbour") 
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
                and has_npc(state, player, "Old Man") 
                and has_soul(state, player, "Portable Stool")
            ),
        "Break the dartboard":
            lambda state: (
                has_pub(state, player) 
                and has_npc(state, player, "Old Man") 
                and has_soul(state, player, "Dartboard") 
                and has_soul(state, player, "Dart")
            ),
        "Set the table":
            lambda state: (
                has_pub(state, player) 
                and has_table_items(state, player)
            ),
        "Steal a pint glass and drop it in the canal":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Pint Glass")
            ),
        "Drop a bucket on the burly man's head":
            lambda state: can_access_final_pub_task(state, player),
        "Be awarded a flower":
            lambda state: (
                has_pub(state, player) 
                and has_back_gardens(state, player) 
                and has_npc(state, player, "Fancy Ladies") 
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
                # Timber Handle and Golden Bell are ALWAYS required regardless of prop souls option
                and has_soul(state, player, "Timber Handle Soul") 
                and has_soul(state, player, "Golden Bell Soul")
                and state.has("Golden Bell", player)
            ),
    }


def get_extra_goal_rules(player, include_npc_souls, include_prop_souls):
    # Option-aware helpers
    def has_npc(state, player, npc):
        if not include_npc_souls:
            return True
        return state.has(f"{npc} Soul", player)
    
    def has_soul(state, player, item):
        if not include_prop_souls:
            return True
        return state.has(f"{item} Soul", player)
    
    # Compound requirements
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
    
    def has_shopping_items(state, player):
        return (
            has_soul(state, player, "Shopping Basket") 
            and has_soul(state, player, "Toothbrush") 
            and has_soul(state, player, "Hairbrush") 
            and has_soul(state, player, "Loo Paper")
            and (
                has_soul(state, player, "Dish Soap Bottle") 
                or has_soul(state, player, "Spray Bottle")
            )
            and (
                has_soul(state, player, "Orange") 
                or has_soul(state, player, "Cucumber") 
                or has_soul(state, player, "Leek") 
                or has_soul(state, player, "Carrot") 
                or has_soul(state, player, "Tomato")
                or (has_soul(state, player, "Apple") and has_garden(state, player))
            )
        )
    
    def has_bust_items(state, player):
        return (
            has_soul(state, player, "Bust Hat") 
            and has_soul(state, player, "Bust Glasses") 
            and has_soul(state, player, "Bust Pipe")
        )
    
    def has_bust_outside_items(state, player):
        return (
            has_back_gardens(state, player)
            and has_npc(state, player, "Messy Neighbour")
            and has_high_street(state, player) # Hard-required, as there are no eyewear outside of High Street aside from the glasses normally meant for the bust
            and (
                has_soul(state, player, "Horn-Rimmed Glasses")
                or has_soul(state, player, "Sunglasses")
                or has_soul(state, player, "Stereoscope")
                or (has_soul(state, player, "Boy's Glasses") and has_npc(state, player, "Boy"))
            )
            and (
                has_soul(state, player, "Dummy") # Doesn't require any access
                or has_garden(state, player) and has_soul(state, player, "Tulip")
                or has_soul(state, player, "Toothbrush") # High Street
                or has_soul(state, player, "Lily Flower") # High Street
                or has_pub(state, player)
                and (
                    has_soul(state, player, "Knife")
                    or has_soul(state, player, "Fork")
                    or has_soul(state, player, "Flower for Vase Soul") and has_npc(state, player, "Fancy Ladies Soul")
                    or has_model_village(state, player) and has_soul(state, player, "Poppy Flower")
                )
            )
            and (
                has_garden(state, player)
                and (
                    has_soul(state, player, "Gardener Hat") and has_npc(state, player, "Groundskeeper")
                    # No need to include Straw Hat at the moment as it logically requires the Gardener Hat
                )
                or has_pub(state, player)
                and (
                    has_soul(state, player, "Traffic Cone")
                    or has_soul(state, player, "Wooly Hat") and has_npc(state, player, "Old Man")
                )
            )
        )
    
    def can_access_final_high_street_task(state, player):
        # Without all of these, this is impossible
        if (
            not has_high_street(state, player)
            or not has_npc(state, player, "Boy")
            or not has_npc(state, player, "Market Lady")
        ):
            return False
        
        # Count total available tasks; the final task is available if we can do at least 5
        task_count = 1 # Trapping the boy in the phone booth is free with the above conditions
        
        if has_soul(state, player, "Boy's Glasses") and has_any_wrong_glasses(state, player):
            task_count += 1
        if (
            has_soul(state, player, "Fusilage")
            or has_garden(state, player)
            and has_npc(state, player, "Groundskeeper") 
            and has_soul(state, player, "Trowel")
        ):
            task_count += 1
        if has_npc(state, player, "TV Shop Owner"):
            task_count += 1
        if has_soul(state, player, "Push Broom"):
            task_count += 1
        if has_shopping_items(state, player):
            task_count += 1
        
        return task_count >= 5
    
    def can_access_final_back_gardens_task(state, player):
        # Without all of these, this is impossible
        if (
            not has_high_street(state, player)
            or not has_npc(state, player, "Tidy Neighbour")
            or not has_npc(state, player, "Messy Neighbour")
            or not has_soul(state, player, "Slipper")
            or not has_soul(state, player, "Rose")
            or not has_soul(state, player, "Rose Box")
            or not has_soul(state, player, "Clippers")
            or not has_soul(state, player, "Clean Sign") # I could be wrong about this one, but I suspect you can't move the box until he moves the sign, even if the sign hasn't spawned in
        ):
            return False
        
        # Count total available tasks; the final task is available if we can do at least 5
        task_count = 1 # Making the man go barefoot is free with the above conditions
        
        if has_soul(state, player, "Tea Cup"):
            task_count += 1
        if (
            has_soul(state, player, "Sock") 
            and has_soul(state, player, "Right Strap") 
            and has_soul(state, player, "Soap") 
        ):
            task_count += 1
        if has_bust_items(state, player) or has_bust_outside_items(state, player):
            task_count += 1
        if has_soul(state, player, "Bow"):
            task_count += 1
        if has_soul(state, player, "Vase"):
            task_count += 1
        
        return task_count >= 5
    
    def has_three_scale_items(state, player):
        # Count total available items that can be placed on the scale
        # Must have High Street access to use the scale
        if not has_high_street(state, player):
            return False
        
        item_count = 0
        
        # High Street shop fruits/vegetables (3 each)
        if has_soul(state, player, "Carrot"):
            item_count += 3  # Shop Carrot 1-3
        if has_soul(state, player, "Tomato"):
            item_count += 3  # Tomato 1-3
        if has_soul(state, player, "Orange"):
            item_count += 3  # Orange 1-3
        if has_soul(state, player, "Leek"):
            item_count += 3  # Leek 1-3
        if has_soul(state, player, "Cucumber"):
            item_count += 3  # Cucumber 1-3
        
        # High Street shop items (1 each)
        if has_soul(state, player, "Toothbrush"):
            item_count += 1
        if has_soul(state, player, "Hairbrush"):
            item_count += 1
        if has_soul(state, player, "Loo Paper"):
            item_count += 1
        if has_soul(state, player, "Dish Soap Bottle"):
            item_count += 1
        if has_soul(state, player, "Spray Bottle"):
            item_count += 1
        if has_soul(state, player, "Toy Car"):
            item_count += 1
        
        # Glasses (1 each, 4 types) - Boy's Glasses needs Boy
        if has_soul(state, player, "Horn-Rimmed Glasses"):
            item_count += 1
        if has_soul(state, player, "Red Glasses"):
            item_count += 1
        if has_soul(state, player, "Sunglasses"):
            item_count += 1
        if has_npc(state, player, "Boy") and has_soul(state, player, "Boy's Glasses"):
            item_count += 1
        
        # Tinned food (3 cans)
        if has_soul(state, player, "Tinned Food"):
            item_count += 3  # Blue, Yellow, Orange cans
        
        # Other High Street items (2 each)
        if has_soul(state, player, "Apple Core"):
            item_count += 2
        if has_soul(state, player, "Pint Bottle"):
            item_count += 2
        
        # Other High Street pickups (1 each)
        if has_soul(state, player, "Fusilage"):
            item_count += 1
        if has_soul(state, player, "Lily Flower"):
            item_count += 1
        if has_soul(state, player, "Weed Tool"):
            item_count += 1
        if has_soul(state, player, "Stereoscope"):
            item_count += 1
        if has_soul(state, player, "Dustbin Lid"):
            item_count += 1
        if has_soul(state, player, "Coin"):
            item_count += 1
        if has_soul(state, player, "Walkie Talkie"):
            item_count += 2  # Two walkie talkies
        
        # Garden items (need Garden access)
        if has_garden(state, player):
            if has_soul(state, player, "Apple"):
                item_count += 2  # Apple and Apple 2
            if has_soul(state, player, "Jam"):
                item_count += 1
            if has_soul(state, player, "Sandwich"):
                item_count += 2  # Left and Right
            if has_soul(state, player, "Tulip"):
                item_count += 1
            if has_soul(state, player, "Picnic Mug"):
                item_count += 1
            if has_soul(state, player, "Thermos"):
                item_count += 1
            if has_soul(state, player, "Trowel"):
                item_count += 1
            if has_soul(state, player, "Radio"):
                item_count += 1
            if has_soul(state, player, "Drink Can"):
                item_count += 1
            if has_soul(state, player, "Tennis Ball"):
                item_count += 1
            if has_soul(state, player, "Boot"):
                item_count += 1  # Garden Boot
        
        # Back Gardens items (need Back Gardens access)
        if has_back_gardens(state, player):
            if has_soul(state, player, "Tea Cup"):
                item_count += 1
            if has_soul(state, player, "Soap"):
                item_count += 1
            if has_soul(state, player, "Cricket Ball"):
                item_count += 1
            if has_soul(state, player, "Bow"):
                item_count += 2  # Blue and regular
            if has_soul(state, player, "Dummy"):
                item_count += 1
            if has_soul(state, player, "Bust Pipe"):
                item_count += 1
            if has_soul(state, player, "Bust Hat"):
                item_count += 1
            if has_soul(state, player, "Bust Glasses"):
                item_count += 1
            if has_soul(state, player, "Newspaper"):
                item_count += 1
            if has_soul(state, player, "Sock"):
                item_count += 2  # Socks and Socks 2
            if has_soul(state, player, "Pot Stack"):
                item_count += 1
            if has_soul(state, player, "Paintbrush"):
                item_count += 1
            if has_soul(state, player, "Right Strap"):
                item_count += 1
            if has_soul(state, player, "Rose"):
                item_count += 1
            if has_soul(state, player, "Badminton Racket"):
                item_count += 1
        
        # Pub items (need Pub access)
        if has_pub(state, player):
            if has_soul(state, player, "Cork"):
                item_count += 1
            if has_soul(state, player, "Quoit"):
                item_count += 6  # 3 green + 3 red
            if has_soul(state, player, "Fishing Bobber"):
                item_count += 1
            if has_soul(state, player, "Exit Letter"):
                item_count += 1
            if has_soul(state, player, "Plate"):
                item_count += 3
            if has_soul(state, player, "Fork"):
                item_count += 2
            if has_soul(state, player, "Knife"):
                item_count += 2
            if has_soul(state, player, "Candlestick"):
                item_count += 1
            if has_soul(state, player, "Dart"):
                item_count += 3
            if has_soul(state, player, "Harmonica"):
                item_count += 1
            if has_soul(state, player, "Pint Glass"):
                item_count += 1
            if has_soul(state, player, "Toy Boat"):
                item_count += 1
            if has_soul(state, player, "Pepper Grinder"):
                item_count += 1
        
        return item_count >= 3
    
    return {
        "Complete all goals":
            lambda state: state.can_reach_location("Complete the game", player),
        "Lock the groundskeeper out of the garden":
            lambda state: (
                has_garden(state, player) 
                and has_npc(state, player, "Groundskeeper") 
                and has_soul(state, player, "Keys")
            ),
        "Cabbage picnic":
            lambda state: has_garden(state, player),
        "Trip the boy in the puddle":
            lambda state: (
                has_high_street(state, player) 
                and has_npc(state, player, "Boy")
            ),
        "Trap the boy in the garage":
            lambda state: can_access_final_high_street_task(state, player),
        "Make the scales go ding":
            lambda state: (
                has_high_street(state, player) 
                and has_three_scale_items(state, player)
            ),
        "Open an umbrella inside the TV shop":
            lambda state: (
                has_high_street(state, player) 
                and has_npc(state, player, "TV Shop Owner") 
                and has_npc(state, player, "Boy")
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
                and has_npc(state, player, "Market Lady") 
                and has_npc(state, player, "Groundskeeper") 
                and has_soul(state, player, "Trowel")
            ),
        "Dress up the bust with things from outside the back gardens":
            lambda state: has_bust_outside_items(state, player),
        "Perform at the pub wearing a ribbon":
            lambda state: (
                has_pub(state, player) 
                and has_back_gardens(state, player) 
                and has_npc(state, player, "Messy Neighbour") 
                and has_npc(state, player, "Fancy Ladies")
                and has_soul(state, player, "Bow") 
            ),
        "Get thrown over the fence":
            lambda state: (
                has_back_gardens(state, player) 
                and has_pub(state, player) 
                and has_npc(state, player, "Tidy Neighbour") 
                and has_soul(state, player, "Stealth Box")
            ),
        "Catch an object as it's thrown over the fence":
            lambda state: (
                has_back_gardens(state, player) 
                and has_npc(state, player, "Tidy Neighbour")
                and (
                    has_soul(state, player, "Bow")
                    or has_soul(state, player, "Cricket Ball")
                    or has_soul(state, player, "Soap")
                    or has_soul(state, player, "Tea Cup")
                )
            ),
        "Steal the old man's woolen hat":
            lambda state: (
                has_pub(state, player) 
                and has_npc(state, player, "Old Man") 
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
                and can_access_final_back_gardens_task(state, player)
                and has_soul(state, player, "Messy Sign")
            ),
    }


def get_speedrun_rules(player, include_npc_souls, include_prop_souls):
    # Option-aware helpers
    def has_npc(state, player, npc):
        if not include_npc_souls:
            return True
        return state.has(f"{npc} Soul", player)
    
    def has_soul(state, player, item):
        if not include_prop_souls:
            return True
        return state.has(f"{item} Soul", player)
    
    # Compound requirements
    def has_picnic_items(state, player):
        return (
            has_soul(state, player, "Picnic Basket") 
            and has_soul(state, player, "Apple") 
            and has_soul(state, player, "Sandwich")
            and has_soul(state, player, "Pumpkin")
            and has_soul(state, player, "Jam")
            and has_soul(state, player, "Thermos")
        )
    
    def has_bust_items(state, player):
        return (
            has_soul(state, player, "Bust Hat") 
            and has_soul(state, player, "Bust Glasses") 
            and has_soul(state, player, "Bust Pipe")
        )
    
    def has_bust_outside_items(state, player):
        return (
            has_back_gardens(state, player)
            and has_npc(state, player, "Messy Neighbour")
            and has_high_street(state, player) # Hard-required, as there are no eyewear outside of High Street aside from the glasses normally meant for the bust
            and (
                has_soul(state, player, "Horn-Rimmed Glasses")
                or has_soul(state, player, "Sunglasses")
                or has_soul(state, player, "Stereoscope")
                or (has_soul(state, player, "Boy's Glasses") and has_npc(state, player, "Boy"))
            )
            and (
                has_soul(state, player, "Dummy") # Doesn't require any access
                or has_garden(state, player) and has_soul(state, player, "Tulip")
                or has_soul(state, player, "Toothbrush") # High Street
                or has_soul(state, player, "Lily Flower") # High Street
                or has_pub(state, player)
                and (
                    has_soul(state, player, "Knife")
                    or has_soul(state, player, "Fork")
                    or has_soul(state, player, "Flower for Vase Soul") and has_npc(state, player, "Fancy Ladies Soul")
                    or has_model_village(state, player) and has_soul(state, player, "Poppy Flower")
                )
            )
            and (
                has_garden(state, player)
                and (
                    has_soul(state, player, "Gardener Hat") and has_npc(state, player, "Groundskeeper")
                    # No need to include Straw Hat at the moment as it logically requires the Gardener Hat
                )
                or has_pub(state, player)
                and (
                    has_soul(state, player, "Traffic Cone")
                    or has_soul(state, player, "Wooly Hat") and has_npc(state, player, "Old Man")
                )
            )
        )
    
    def has_table_items(state, player):
        return (
            has_soul(state, player, "Plate") 
            and has_soul(state, player, "Fork") 
            and has_soul(state, player, "Knife") 
            and has_soul(state, player, "Pepper Grinder") 
            and has_soul(state, player, "Candlestick")
        )
    
    def has_shopping_items(state, player):
        return (
            has_soul(state, player, "Shopping Basket") 
            and has_soul(state, player, "Toothbrush") 
            and has_soul(state, player, "Hairbrush") 
            and has_soul(state, player, "Loo Paper")
            and (
                has_soul(state, player, "Dish Soap Bottle") 
                or has_soul(state, player, "Spray Bottle")
            )
            and (
                has_soul(state, player, "Orange") 
                or has_soul(state, player, "Cucumber") 
                or has_soul(state, player, "Leek") 
                or has_soul(state, player, "Carrot") 
                or has_soul(state, player, "Tomato")
                or (has_soul(state, player, "Apple") and has_garden(state, player))
            )
        )
    
    def has_any_wrong_glasses(state, player):
        return (
            has_soul(state, player, "Horn-Rimmed Glasses") 
            or has_soul(state, player, "Red Glasses") 
            or has_soul(state, player, "Sunglasses")
        )
    
    def can_complete_garden_todos(state, player):
        return (
            has_garden(state, player) 
            and has_npc(state, player, "Groundskeeper") 
            and has_soul(state, player, "Mallet") 
            and has_soul(state, player, "Gardener Sign") 
            and has_soul(state, player, "Keys") 
            and has_soul(state, player, "Straw Hat") 
            and has_soul(state, player, "Tulip") 
            and has_soul(state, player, "Rake") 
            and has_picnic_items(state, player)
        )
    
    def can_complete_high_street_todos(state, player):
        return (
            has_high_street(state, player) 
            and has_npc(state, player, "Boy") 
            and has_npc(state, player, "Market Lady") 
            and has_soul(state, player, "Boy's Glasses") 
            and has_any_wrong_glasses(state, player) 
            and has_soul(state, player, "Fusilage")
            and has_npc(state, player, "TV Shop Owner") 
            and has_soul(state, player, "Push Broom") 
            and has_shopping_items(state, player)
        )
    
    def can_complete_back_gardens_todos(state, player):
        return (
            has_back_gardens(state, player) 
            and has_npc(state, player, "Tidy Neighbour") 
            and has_npc(state, player, "Messy Neighbour") 
            and has_soul(state, player, "Slipper") 
            and has_soul(state, player, "Rose") 
            and has_soul(state, player, "Rose Box") 
            and has_soul(state, player, "Clippers") 
            and has_soul(state, player, "Clean Sign") 
            and has_soul(state, player, "Tea Cup") 
            and has_soul(state, player, "Sock") 
            and has_soul(state, player, "Right Strap") 
            and has_soul(state, player, "Soap") 
            and (
                has_bust_items(state, player)
                or has_bust_outside_items(state, player)
            )
            and has_soul(state, player, "Bow") 
            and has_soul(state, player, "Vase")
        )
    
    def can_complete_pub_todos(state, player):
        return (
            has_pub(state, player) 
            and has_back_gardens(state, player) 
            and has_npc(state, player, "Old Man") 
            and has_npc(state, player, "Burly Man") 
            and has_soul(state, player, "Burly Mans Bucket") 
            and has_soul(state, player, "Tomato") 
            and has_soul(state, player, "Toy Boat") 
            and has_soul(state, player, "Portable Stool") 
            and has_soul(state, player, "Dartboard") 
            and has_soul(state, player, "Dart") 
            and has_table_items(state, player) 
            and has_soul(state, player, "Pint Glass") 
            and has_npc(state, player, "Fancy Ladies")
        )
    
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


def get_interaction_rules(player, include_npc_souls, include_prop_souls):
    # Option-aware helpers
    def has_npc(state, player, npc):
        if not include_npc_souls:
            return True
        return state.has(f"{npc} Soul", player)
    
    def has_soul(state, player, item):
        if not include_prop_souls:
            return True
        return state.has(f"{item} Soul", player)
    
    return {
        # Hub
        "Open Intro Gate":
            lambda state: True,
        
        # Garden
        "Ring the Bike Bell":
            lambda state: True,
        "Turn on Garden Tap":
            lambda state: has_garden(state, player),
        "Turn on Sprinkler":
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
                and has_npc(state, player, "Boy")
            ),
        "Untie Boy's Laces (Right)":
            lambda state: (
                has_high_street(state, player) 
                and has_npc(state, player, "Boy")
            ),
        
        # Back Gardens
        "Break Through Boards":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Boards")
            ),
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
        "Spin the Windmill":
            lambda state: has_back_gardens(state, player),
        "Spin Purple Flower":
            lambda state: has_back_gardens(state, player),
        "Spin Sunflower":
            lambda state: has_back_gardens(state, player),
        "Break Through Trellis":
            lambda state: has_back_gardens(state, player),
        
        # Pub
        "Untie Burly Man's Laces (Left)":
            lambda state: (
                has_pub(state, player) 
                and has_npc(state, player, "Burly Man")
            ),
        "Untie Burly Man's Laces (Right)":
            lambda state: (
                has_pub(state, player) 
                and has_npc(state, player, "Burly Man")
            ),
        "Close Van Door (Left)":
            lambda state: has_pub(state, player),
        "Close Van Door (Right)":
            lambda state: has_pub(state, player),
        "Turn on Pub Tap":
            lambda state: has_pub(state, player),
    }


def get_pickup_rules(player, include_npc_souls, include_prop_souls):
    # Option-aware helpers
    def has_npc(state, player, npc):
        if not include_npc_souls:
            return True
        return state.has(f"{npc} Soul", player)
    
    def has_soul(state, player, item):
        if not include_prop_souls:
            return True
        return state.has(f"{item} Soul", player)
    
    # Compound requirements
    def has_picnic_items(state, player):
        return (
            has_soul(state, player, "Picnic Basket") 
            and has_soul(state, player, "Apple") 
            and has_soul(state, player, "Sandwich")
            and has_soul(state, player, "Pumpkin")
            and has_soul(state, player, "Jam")
            and has_soul(state, player, "Thermos")
        )
    
    def has_bust_items(state, player):
        return (
            has_soul(state, player, "Bust Hat") 
            and has_soul(state, player, "Bust Glasses") 
            and has_soul(state, player, "Bust Pipe")
        )
    
    def has_bust_outside_items(state, player):
        return (
            has_back_gardens(state, player)
            and has_npc(state, player, "Messy Neighbour")
            and has_high_street(state, player) # Hard-required, as there are no eyewear outside of High Street aside from the glasses normally meant for the bust
            and (
                has_soul(state, player, "Horn-Rimmed Glasses")
                or has_soul(state, player, "Sunglasses")
                or has_soul(state, player, "Stereoscope")
                or (has_soul(state, player, "Boy's Glasses") and has_npc(state, player, "Boy"))
            )
            and (
                has_soul(state, player, "Dummy") # Doesn't require any access
                or has_garden(state, player) and has_soul(state, player, "Tulip")
                or has_soul(state, player, "Toothbrush") # High Street
                or has_soul(state, player, "Lily Flower") # High Street
                or has_pub(state, player)
                and (
                    has_soul(state, player, "Knife")
                    or has_soul(state, player, "Fork")
                    or has_soul(state, player, "Flower for Vase Soul") and has_npc(state, player, "Fancy Ladies Soul")
                    or has_model_village(state, player) and has_soul(state, player, "Poppy Flower")
                )
            )
            and (
                has_garden(state, player)
                and (
                    has_soul(state, player, "Gardener Hat") and has_npc(state, player, "Groundskeeper")
                    # No need to include Straw Hat at the moment as it logically requires the Gardener Hat
                )
                or has_pub(state, player)
                and (
                    has_soul(state, player, "Traffic Cone")
                    or has_soul(state, player, "Wooly Hat") and has_npc(state, player, "Old Man")
                )
            )
        )
    
    def has_table_items(state, player):
        return (
            has_soul(state, player, "Plate") 
            and has_soul(state, player, "Fork") 
            and has_soul(state, player, "Knife") 
            and has_soul(state, player, "Pepper Grinder") 
            and has_soul(state, player, "Candlestick")
        )
    
    def has_shopping_items(state, player):
        return (
            has_soul(state, player, "Shopping Basket") 
            and has_soul(state, player, "Toothbrush") 
            and has_soul(state, player, "Hairbrush") 
            and has_soul(state, player, "Loo Paper")
            and (
                has_soul(state, player, "Dish Soap Bottle") 
                or has_soul(state, player, "Spray Bottle")
            )
            and (
                has_soul(state, player, "Orange") 
                or has_soul(state, player, "Cucumber") 
                or has_soul(state, player, "Leek") 
                or has_soul(state, player, "Carrot") 
                or has_soul(state, player, "Tomato")
                or (has_soul(state, player, "Apple") and has_garden(state, player))
            )
        )
    
    def has_any_wrong_glasses(state, player):
        return (
            has_soul(state, player, "Horn-Rimmed Glasses") 
            or has_soul(state, player, "Red Glasses") 
            or has_soul(state, player, "Sunglasses")
        )
    
    def can_access_final_garden_task(state, player):
        # Without all of these, this is impossible
        if (
            not has_garden(state, player)
            or not has_npc(state, player, "Groundskeeper")
            or not has_soul(state, player, "Mallet")
            or not has_soul(state, player, "Gardener Sign")
        ):
            return False
        
        # Count total available tasks; the final task is available if we can do at least 5
        task_count = 2 # Getting into the Garden and getting the Groundskeeper wet are free with the above conditions
        
        if has_soul(state, player, "Keys"):
            task_count += 1
        if has_soul(state, player, "Straw Hat") and has_soul(state, player, "Tulip"):
            task_count += 1
        if has_soul(state, player, "Rake"):
            task_count += 1
        if has_picnic_items(state, player):
            task_count += 1
        
        return task_count >= 5
    
    def can_access_final_high_street_task(state, player):
        # Without all of these, this is impossible
        if (
            not has_high_street(state, player)
            or not has_npc(state, player, "Boy")
            or not has_npc(state, player, "Market Lady")
        ):
            return False
        
        # Count total available tasks; the final task is available if we can do at least 5
        task_count = 1 # Trapping the boy in the phone booth is free with the above conditions
        
        if has_soul(state, player, "Boy's Glasses") and has_any_wrong_glasses(state, player):
            task_count += 1
        if (
            has_soul(state, player, "Fusilage")
            or has_garden(state, player)
            and has_npc(state, player, "Groundskeeper") 
            and has_soul(state, player, "Trowel")
        ):
            task_count += 1
        if has_npc(state, player, "TV Shop Owner"):
            task_count += 1
        if has_soul(state, player, "Push Broom"):
            task_count += 1
        if has_shopping_items(state, player):
            task_count += 1
        
        return task_count >= 5
    
    def can_access_final_back_gardens_task(state, player):
        # Without all of these, this is impossible
        if (
            not has_high_street(state, player)
            or not has_npc(state, player, "Tidy Neighbour")
            or not has_npc(state, player, "Messy Neighbour")
            or not has_soul(state, player, "Slipper")
            or not has_soul(state, player, "Rose")
            or not has_soul(state, player, "Rose Box")
            or not has_soul(state, player, "Clippers")
            or not has_soul(state, player, "Clean Sign") # I could be wrong about this one, but I suspect you can't move the box until he moves the sign, even if the sign hasn't spawned in
        ):
            return False
        
        # Count total available tasks; the final task is available if we can do at least 5
        task_count = 1 # Making the man go barefoot is free with the above conditions
        
        if has_soul(state, player, "Tea Cup"):
            task_count += 1
        if (
            has_soul(state, player, "Sock") 
            and has_soul(state, player, "Right Strap") 
            and has_soul(state, player, "Soap") 
        ):
            task_count += 1
        if has_bust_items(state, player) or has_bust_outside_items(state, player):
            task_count += 1
        if has_soul(state, player, "Bow"):
            task_count += 1
        if has_soul(state, player, "Vase"):
            task_count += 1
        
        return task_count >= 5
    
    def can_access_final_pub_task(state, player):
        # Without all of these, this is impossible
        if (
            not has_high_street(state, player)
            or not has_npc(state, player, "Old Man")
            or not has_npc(state, player, "Burly Man Soul")
            or not has_soul(state, player, "Burly Mans Bucket")
            or not has_soul(state, player, "Tomato")
        ):
            return False
        
        # Count total available tasks; the final task is available if we can do at least 5
        task_count = 0
        
        if has_soul(state, player, "Toy Boat"):
            task_count += 1
        if has_soul(state, player, "Portable Stool"):
            task_count += 1
        if has_soul(state, player, "Dartboard") and has_soul(state, player, "Dart"):
            task_count += 1
        if has_table_items(state, player):
            task_count += 1
        if has_soul(state, player, "Pint Glass"):
            task_count += 1
        if has_npc(state, player, "Fancy Ladies") and has_soul(state, player, "Flower for Vase"):
            task_count += 1
        
        return task_count >= 5
    
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
                and has_npc(state, player, "Groundskeeper")
                and has_soul(state, player, "Keys")
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
                and has_npc(state, player, "Groundskeeper") 
                and has_soul(state, player, "Straw Hat") 
                and has_soul(state, player, "Tulip")
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
                and has_npc(state, player, "Groundskeeper")
                and has_soul(state, player, "Gardener Hat")
                and has_soul(state, player, "Tulip")
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
        # Garden Carrots
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
            lambda state: can_access_final_garden_task(state, player),
        "Drag Wooden Crate":
            lambda state: (
                has_garden(state, player) 
                and has_soul(state, player, "Wooden Crate")
            ),
        "Drag Fence Bolt":
            lambda state: True,  # Starting item - no requirements
        "Drag Mallet":
            lambda state: can_access_final_garden_task(state, player),
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
                and has_npc(state, player, "Boy")
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
        "Pick up Loo Paper":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Loo Paper")
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
                and has_soul(state, player, "Tinned Food")
            ),
        "Pick up Yellow Can":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Tinned Food")
            ),
        "Pick up Orange Can":
            lambda state: (
                has_high_street(state, player) 
                and has_soul(state, player, "Tinned Food")
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
                can_access_final_high_street_task(state, player) 
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
                and has_soul(state, player, "Push Broom") 
                and has_npc(state, player, "Market Lady")
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
                and has_soul(state, player, "Duck Statue")
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
                and has_npc(state, player, "Tidy Neighbour")
            ),
        "Pick up Left Slipper":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Slipper") 
                and has_npc(state, player, "Tidy Neighbour")
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
                and has_soul(state, player, "Vase") 
                and has_npc(state, player, "Tidy Neighbour")
            ),
        "Pick up Broken Vase Piece 2":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Vase Piece") 
                and has_soul(state, player, "Vase") 
                and has_npc(state, player, "Tidy Neighbour")
            ),
        "Pick up Right Strap":
            lambda state: (
                has_back_gardens(state, player) 
                and has_soul(state, player, "Right Strap")
            ),
        "Pick up Badminton Racket":
            lambda state: (
                has_back_gardens(state, player) 
                and can_access_final_back_gardens_task(state, player) 
                and has_soul(state, player, "Badminton Racket") 
                and has_soul(state, player, "Messy Sign")
            ),
        "Pick up Rose":
            lambda state: (
                has_back_gardens(state, player) 
                and can_access_final_back_gardens_task(state, player)
            ),
        
        # BACK GARDENS DRAGS
        "Drag Rose Box":
            lambda state: (
                has_back_gardens(state, player) 
                and can_access_final_back_gardens_task(state, player)
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
                and can_access_final_back_gardens_task(state, player) 
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
                and can_access_final_back_gardens_task(state, player) 
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
                and has_npc(state, player, "Fancy Ladies") 
                and has_soul(state, player, "Flower for Vase")
            ),
        "Pick up Dart 1":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Dart") 
                and has_npc(state, player, "Old Man")
            ),
        "Pick up Dart 2":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Dart") 
                and has_npc(state, player, "Old Man")
            ),
        "Pick up Dart 3":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Dart") 
                and has_npc(state, player, "Old Man")
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
                and has_soul(state, player, "Portable Stool") 
                and has_npc(state, player, "Old Man")
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
                and has_npc(state, player, "Pub Lady")
            ),
        # Pub Tomatoes
        "Pick up Pub Tomato 1":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
                and can_access_final_pub_task(state, player)
            ),
        "Pick up Pub Tomato 2":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
                and can_access_final_pub_task(state, player)
            ),
        "Pick up Pub Tomato 3":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
                and can_access_final_pub_task(state, player)
            ),
        "Pick up Pub Tomato 4":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
                and can_access_final_pub_task(state, player)
            ),
        "Pick up Pub Tomato 5":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
                and can_access_final_pub_task(state, player)
            ),
        "Pick up Pub Tomato 6":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
                and can_access_final_pub_task(state, player)
            ),
        "Pick up Pub Tomato 7":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
                and can_access_final_pub_task(state, player)
            ),
        "Pick up Pub Tomato 8":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
                and can_access_final_pub_task(state, player)
            ),
        "Pick up Pub Tomato 9":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Tomato")
                and can_access_final_pub_task(state, player)
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
                and can_access_final_pub_task(state, player) 
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
                and has_npc(state, player, "Old Man") 
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
        "Drag Burly Mans Bucket":
            lambda state: (
                has_pub(state, player) 
                and has_soul(state, player, "Burly Mans Bucket")
            ),
        
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
