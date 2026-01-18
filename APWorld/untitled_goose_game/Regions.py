from typing import TYPE_CHECKING
from BaseClasses import Region
from .Locations import (
    BASE_ID, location_table, extra_locations, speedrun_locations, completion_location,
    milestone_locations, item_pickup_locations, drag_item_locations, interaction_locations,
    unique_item_pickup_locations, unique_item_drag_locations, sandcastle_peck_locations,
    sandcastle_first_peck_locations, milestone_locations_main_tasks, GooseGameLocation
)
from .Rules import UntitledGooseRules

if TYPE_CHECKING:
    from . import GooseGameWorld


def create_regions(world: "GooseGameWorld") -> None:
    """Create all regions and their locations
    
    Region Structure (Hub-based):
    
            Menu
             |
    Hub (Well - Starting Area)
      /      |      \      \ 
    Garden  High   Back    Pub
           Street  Gardens    \ 
                            Model Village
    
    The Hub (well area) is the starting location.
    - Garden, High Street, Back Gardens, Pub: Each requires their own access item
    - Model Village: Requires BOTH Pub Access AND Model Village Access
    - Victory: Requires ALL 5 access items (must carry bell back through every area)
    """
    
    multiworld = world.multiworld
    player = world.player
    rules = UntitledGooseRules(world)
    
    # Create regions
    menu = Region("Menu", player, multiworld)
    hub = Region("Hub", player, multiworld)  # Starting area at the well
    garden = Region("Garden", player, multiworld)
    high_street = Region("High Street", player, multiworld)
    back_gardens = Region("Back Gardens", player, multiworld)
    pub = Region("Pub", player, multiworld)
    model_village = Region("Model Village", player, multiworld)
    
    # Add regions to multiworld
    multiworld.regions += [menu, hub, garden, high_street, back_gardens, pub, model_village]
    
    # Create connections - HUB-BASED
    # Hub is the starting area, all other areas require access items
    menu.connect(hub)  # Hub is starting area - always accessible
    
    # All areas connect directly FROM the hub
    # Rules for these entrances are set in Rules.py
    hub.connect(garden, lambda state: rules.has_garden(state))
    hub.connect(high_street, lambda state: rules.has_high_street(state))
    hub.connect(back_gardens, lambda state: rules.has_back_gardens(state))
    hub.connect(pub, lambda state: rules.has_pub(state))
    hub.connect(model_village, lambda state: rules.has_model_village(state))
    
    # Helper to add location to correct region
    def add_location(loc_name: str, loc_id: int, region_name: str):
        region = multiworld.get_region(region_name, player)
        location = GooseGameLocation(player, loc_name, loc_id, region)
        region.locations.append(location)
    
    # Add main task locations (always included)
    for loc_name, loc_data in location_table.items():
        add_location(loc_name, loc_data.id, loc_data.region)
    
    # Add extra task locations if enabled
    if world.options.include_extra_tasks:
        for loc_name, loc_data in extra_locations.items():
            add_location(loc_name, loc_data.id, loc_data.region)
    
    # Add speedrun goal locations if enabled
    if world.options.include_speedrun_tasks:
        for loc_name, loc_data in speedrun_locations.items():
            add_location(loc_name, loc_data.id, loc_data.region)
    
    # Add item pickup locations if enabled
    if world.options.include_item_pickups:
        for loc_name, loc_data in item_pickup_locations.items():
            add_location(loc_name, loc_data.id, loc_data.region)
        
        # Add unique tracked item pickup locations (position-based carrots, etc.)
        for loc_name, loc_data in unique_item_pickup_locations.items():
            add_location(loc_name, loc_data.id, loc_data.region)
    
    # Add drag item locations if enabled (separate toggle from pickups)
    if world.options.include_drag_items:
        for loc_name, loc_data in drag_item_locations.items():
            add_location(loc_name, loc_data.id, loc_data.region)
        
        # Add unique tracked item drag locations (position-based carrots, etc.)
        for loc_name, loc_data in unique_item_drag_locations.items():
            add_location(loc_name, loc_data.id, loc_data.region)
    
    # Add interaction locations if enabled
    if world.options.include_interactions:
        for loc_name, loc_data in interaction_locations.items():
            add_location(loc_name, loc_data.id, loc_data.region)
    
    # Add sandcastle peck locations if enabled
    pecking = world.options.include_model_church_pecks.value
    if pecking == 1:
        for loc_name, loc_data in sandcastle_first_peck_locations.items():
            add_location(loc_name, loc_data.id, loc_data.region)
    elif pecking == 2:
        for loc_name, loc_data in sandcastle_peck_locations.items():
            add_location(loc_name, loc_data.id, loc_data.region)
    
    # Add milestone locations if enabled
    if world.options.include_milestone_locations:
        for loc_name, loc_data in milestone_locations_main_tasks.items():
            add_location(loc_name, loc_data.id, loc_data.region)
    
        # Add extra task milestone if enabled
        if world.options.include_extra_tasks:
            add_location("All To Do (As Well) Tasks Complete", BASE_ID + 85, "Hub")
        
        # Add speedrun task milestone if enabled
        if world.options.include_speedrun_tasks:
            add_location("All Speedrun Tasks Complete", BASE_ID + 86, "Hub")
    
        # Add all tasks milestone if both extra and speedrun are enabled
        if world.options.include_extra_tasks and world.options.include_speedrun_tasks:
            add_location("All Tasks Complete", BASE_ID + 90, "Hub")
        
    
    # Add locations based on goal option
    goal = world.options.goal.value
    if goal == 0:  # Just reach the bell
        add_location("Get into the Model Village (Golden Bell Soul)", BASE_ID + 93, "Model Village")
    # elif goal == 1:  # Find bell
    elif goal == 2:  # All main tasks
        add_location("All Main Task Lists Complete (Golden Bell Soul)", BASE_ID + 89, "Hub")
    elif goal == 3:  # Only speedrun tasks
        add_location("All Speedrun Tasks Complete (Golden Bell Soul)", BASE_ID + 87, "Hub")
    elif goal == 4:  # All except speedrun tasks
        add_location("All Main Task Lists + To Do (As Well) Complete (Golden Bell Soul)", BASE_ID + 92, "Hub")
    elif goal == 5:  # All tasks
        add_location("All Tasks Complete (Golden Bell Soul)", BASE_ID + 91, "Hub")
    elif goal == 6:  # Four Final Tasks
        add_location("Complete the Four Final Area Tasks (Golden Bell Soul)", BASE_ID + 94, "Hub")
    
    
    # Base items always needed
    base_items = [
        "Garden Access", "High Street Access", "Back Gardens Access", 
        "Pub Access", "Model Village Access", "Golden Bell Soul"
    ]
    
    if world.options.include_prop_souls.value:
        base_items.extend(["Timber Handle Soul"])