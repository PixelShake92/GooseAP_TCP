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
        
        # NPC Soul items (10 total) - required for NPC-related goals
        npc_souls = [
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
        ]
        
        # Prop Soul items - required for picking up/dragging items
        prop_souls = [
            # Grouped Prop Souls (26)
            "Carrot Soul",
            "Tomato Soul",
            "Pumpkin Soul",
            "Topsoil Bag Soul",
            "Quoit Soul",
            "Plate Soul",
            "Orange Soul",
            "Leek Soul",
            "Cucumber Soul",
            "Dart Soul",
            "Umbrella Soul",
            "Spray Can Soul",
            "Sock Soul",
            "Pint Bottle Soul",
            "Knife Soul",
            "Gumboot Soul",
            "Fork Soul",
            "Vase Piece Soul",
            "Apple Core Soul",
            "Apple Soul",
            "Sandwich Soul",
            "Slipper Soul",
            "Bow Soul",
            "Walkie Talkie Soul",
            "Boot Soul",
            "Mini Person Soul",
            
            # Garden Prop Souls (20)
            "Radio Soul",
            "Trowel Soul",
            "Keys Soul",
            "Tulip Soul",
            "Jam Soul",
            "Picnic Mug Soul",
            "Thermos Soul",
            "Straw Hat Soul",
            "Drink Can Soul",
            "Tennis Ball Soul",
            "Gardener Hat Soul",
            "Rake Soul",
            "Picnic Basket Soul",
            "Esky Soul",
            "Shovel Soul",
            "Watering Can Soul",
            "Fence Bolt Soul",
            "Mallet Soul",
            "Wooden Crate Soul",
            "Gardener Sign Soul",
            
            # High Street Prop Souls (24)
            "Boy's Glasses Soul",
            "Horn-Rimmed Glasses Soul",
            "Red Glasses Soul",
            "Sunglasses Soul",
            "Toilet Paper Soul",
            "Toy Car Soul",
            "Hairbrush Soul",
            "Toothbrush Soul",
            "Stereoscope Soul",
            "Dish Soap Bottle Soul",
            "Spray Bottle Soul",
            "Weed Tool Soul",
            "Lily Flower Soul",
            "Fusilage Soul",
            "Coin Soul",
            "Chalk Soul",
            "Dustbin Lid Soul",
            "Shopping Basket Soul",
            "Push Broom Soul",
            "Broken Broom Head Soul",
            "Dustbin Soul",
            "Baby Doll Soul",
            "Pricing Gun Soul",
            "Adding Machine Soul",
            
            # Back Gardens Prop Souls (25)
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
            "Right Strap Soul",
            "Rose Soul",
            "Rose Box Soul",
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
            
            # Pub Prop Souls (22)
            "Fishing Bobber Soul",
            "Exit Letter Soul",
            "Pint Glass Soul",
            "Toy Boat Soul",
            "Wooly Hat Soul",
            "Pepper Grinder Soul",
            "Pub Cloth Soul",
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
            "Delivery Box Soul",
           # "Burly Mans Bucket Soul",
            
            # Model Village Prop Souls (11)
            "Mini Mail Pillar Soul",
            "Mini Phone Door Soul",
            "Mini Shovel Soul",
            "Poppy Flower Soul",
            "Timber Handle Soul",
            "Birdbath Soul",
            "Easel Soul",
            "Mini Bench Soul",
            "Mini Pump Soul",
            "Mini Street Bench Soul",
            "Sun Lounge Soul",
            
            # Victory item soul
            "Golden Bell Soul",
        ]
        
        # Add area items to pool (except the starting one)
        for item_name in area_items:
            if item_name == starting_area:
                # Give starting area to player directly (precollected)
                self.multiworld.push_precollected(self.create_item(item_name))
            else:
                self.multiworld.itempool.append(self.create_item(item_name))
        
        # Add all NPC souls to pool
        for soul in npc_souls:
            self.multiworld.itempool.append(self.create_item(soul))
        
        # Add all Prop souls to pool
        for soul in prop_souls:
            self.multiworld.itempool.append(self.create_item(soul))
        
        # Victory item
        self.multiworld.itempool.append(self.create_item("Golden Bell"))
        
        # Calculate filler needed
        total_locations = len(self.multiworld.get_unfilled_locations(self.player))
        # 4 area items in pool + NPC souls + prop souls + 1 Golden Bell
        items_added = 4 + len(npc_souls) + len(prop_souls) + 1
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
