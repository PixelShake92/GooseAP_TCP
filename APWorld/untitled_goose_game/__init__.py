from typing import Dict, Any, ClassVar
from worlds.AutoWorld import World, WebWorld
from BaseClasses import Item, Tutorial
from .Items import item_table, GooseGameItem, ITEM_GROUPS
from .Locations import location_table, GooseGameLocation, get_all_location_ids
from .Regions import create_regions
from .Options import GooseGameOptions


class GooseGameWeb(WebWorld):
    theme = "grass"
    
    setup_en = Tutorial(
        "Multiworld Setup Guide",
        "A guide to setting up Untitled Goose Game for Archipelago multiworld.",
        "English",
        "setup_en.md",
        "setup/en",
        ["YourName"]
    )
    
    tutorials = [setup_en]


class GooseGameWorld(World):
    """
    Untitled Goose Game - It's a lovely morning in the village, 
    and you are a horrible goose.
    
    Start at the Hub (well) and collect area access items to explore!
    Complete to-do list goals, pick up items, and drag objects to send checks!
    """
    
    game = "Untitled Goose Game"
    web = GooseGameWeb()
    
    options_dataclass = GooseGameOptions
    options: GooseGameOptions
    
    item_name_to_id: ClassVar[Dict[str, int]] = {
        name: data.id for name, data in item_table.items()
    }
    
    # Register ALL possible locations - AP needs these upfront
    location_name_to_id: ClassVar[Dict[str, int]] = get_all_location_ids()
    
    item_name_groups = ITEM_GROUPS
    
    def create_item(self, name: str) -> Item:
        item_data = item_table[name]
        return GooseGameItem(name, item_data.classification, item_data.id, self.player)
    
    def create_regions(self) -> None:
        create_regions(self)
    
    def get_starting_area_name(self) -> str:
        """Determine which area the player starts with access to."""
        starting_option = self.options.starting_area.value
        
        # Only 4 valid starting areas (Model Village excluded - it's the finale!)
        area_names = [
            "Garden Access",
            "High Street Access", 
            "Back Gardens Access",
            "Pub Access"
        ]
        
        if starting_option == 4:  # Random
            return self.random.choice(area_names)
        else:
            return area_names[starting_option]
    
    def create_items(self) -> None:
        # Determine starting area
        starting_area = self.get_starting_area_name()
        
        # All area access items (5 total)
        area_items = [
            "Garden Access",
            "High Street Access",
            "Back Gardens Access", 
            "Pub Access",
            "Model Village Access"
        ]
        
        # Add area items to pool (except the starting one)
        for item_name in area_items:
            if item_name == starting_area:
                # Give starting area to player directly (precollected)
                self.multiworld.push_precollected(self.create_item(item_name))
            else:
                self.multiworld.itempool.append(self.create_item(item_name))
        
        # Victory item
        self.multiworld.itempool.append(self.create_item("Golden Bell"))
        
        # Calculate filler needed
        total_locations = len(self.multiworld.get_unfilled_locations(self.player))
        # 4 area items in pool (one is precollected) + 1 Golden Bell = 5 items
        items_added = 5
        filler_needed = total_locations - items_added
        
        # Capped filler items - these have maximum quantities
        capped_items = {
            "Mega Honk": 3,      # Max 3 levels
            "Speedy Feet": 10,   # Max 10 (50% speed bonus cap)
            "Silent Steps": 1,  # Only need 1
            "A Goose Day": 3,   # Max 3 stored
        }
        
        # Trap items (unlimited)
        trap_items = ["Tired Goose", "Confused Feet", "Butterbeak", "Suspicious Goose"]
        
        trap_percentage = self.options.trap_percentage.value
        
        # Track how many of each capped item we've added
        capped_counts = {name: 0 for name in capped_items}
        
        for _ in range(filler_needed):
            if trap_percentage > 0 and self.random.randint(1, 100) <= trap_percentage:
                # Add a trap
                item_name = self.random.choice(trap_items)
            else:
                # Try to add a capped filler item
                available_capped = [
                    name for name, max_count in capped_items.items()
                    if capped_counts[name] < max_count
                ]
                
                if available_capped:
                    item_name = self.random.choice(available_capped)
                    capped_counts[item_name] += 1
                else:
                    # All capped items are maxed out, add a trap instead
                    item_name = self.random.choice(trap_items)
            
            self.multiworld.itempool.append(self.create_item(item_name))
    
    def set_rules(self) -> None:
        from .Rules import set_rules
        set_rules(self)
    
    def fill_slot_data(self) -> Dict[str, Any]:
        return {
            "starting_area": self.get_starting_area_name(),
            "include_extra_goals": self.options.include_extra_goals.value,
            "include_speedrun_goals": self.options.include_speedrun_goals.value,
            "include_item_pickups": self.options.include_item_pickups.value,
            "include_drag_items": self.options.include_drag_items.value,
            "include_interactions": self.options.include_interactions.value,
            "goal": self.options.goal.value,
            "death_link": self.options.death_link.value,
        }
