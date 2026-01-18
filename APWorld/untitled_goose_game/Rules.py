from BaseClasses import CollectionState
from typing import TYPE_CHECKING
from worlds.generic.Rules import set_rule

if TYPE_CHECKING:
    from . import GooseGameWorld

class UntitledGooseRules:
    world: "GooseGameWorld"

    def __init__(self, world: "GooseGameWorld") -> None:
        self.player = world.player
        self.world = world
        
        # To Do (As Well) Task Rules
        if self.world.options.include_extra_tasks.value:
            self.extra_task_rules = {
                "Lock the groundskeeper out of the garden": self.lock_groundskeeper_out,
                "Cabbage picnic": self.cabbage_picnic,
                "Trip the boy in the puddle": self.trip_boy_in_puddle,
                "Make the scales go ding": self.make_scales_ding,
                "Open an umbrella inside the TV shop": self.open_umbrella_on_tv,
                "Make someone from outside the high street buy back their own stuff": self.make_groundskeeper_buyback,
                "Collect the five flowers": self.collect_five_flowers,
                "Trap the boy in the garage": self.trap_boy_in_garage,
                "Catch an object as it's thrown over the fence": self.catch_thrown_object,
                "Get thrown over the fence": self.get_thrown_over_fence,
                "Dress up the bust with things from outside the back gardens": self.dress_up_bust_outside_items,
                "Score a goal": self.score_goal,
                "Sail the toy boat under the bridge": self.sail_boat_under_bridge,
                "Perform at the pub wearing a ribbon": self.perform_with_ribbon,
                "Steal the old man's woolen hat": self.steal_woolen_hat,
            }

        # To Do (Quickly!!) Task Rules
        if self.world.options.include_speedrun_tasks.value:
            self.speedrun_task_rules = {
                "Complete Garden before noon": self.speedrun_garden,
                "Complete High Street before noon": self.speedrun_high_street,
                "Complete Back Gardens before noon": self.speedrun_back_gardens,
                "Complete Pub before noon": self.speedrun_pub,
            }

        # Item Pickup Rules
        if self.world.options.include_item_pickups.value:
            self.pickup_rules = {
                "Pick up Radio": self.pickup_radio,
                "Pick up Trowel": self.pickup_trowel,
                "Pick up Keys": self.pickup_gardener_gear,
                "Pick up Tulip": self.pickup_tulip,
                "Pick up Apple": self.pickup_apples,
                "Pick up Jam": self.pickup_jam,
                "Pick up Picnic Mug": self.pickup_picnic_mug,
                "Pick up Thermos": self.pickup_thermos,
                "Pick up Sandwich (Right)": self.pickup_sandwich,
                "Pick up Sandwich (Left)": self.pickup_sandwich,
                "Pick up Straw Hat": self.pickup_straw_hat,
                "Pick up Drink Can": self.pickup_drink_can,
                "Pick up Tennis Ball": self.pickup_tennis_ball,
                "Pick up Gardener Hat": self.pickup_gardener_gear,
                "Pick up Apple 2": self.pickup_apples,
                "Pick up Boy's Glasses": self.pickup_boys_glasses,
                "Pick up Horn-Rimmed Glasses": self.pickup_horn_rimmed_glasses,
                "Pick up Red Glasses": self.pickup_red_glasses,
                "Pick up Sunglasses": self.pickup_sunglasses,
                "Pick up Loo Paper": self.pickup_loo_paper,
                "Pick up Toy Car": self.pickup_toy_car,
                "Pick up Hairbrush": self.pickup_hairbrush,
                "Pick up Toothbrush": self.pickup_toothbrush,
                "Pick up Stereoscope": self.pickup_stereoscope,
                "Pick up Dish Soap Bottle": self.pickup_dish_soap_bottle,
                "Pick up Blue Can": self.pickup_food_cans,
                "Pick up Yellow Can": self.pickup_food_cans,
                "Pick up Orange Can": self.pickup_food_cans,
                "Pick up Weed Tool": self.pickup_weed_tools,
                "Pick up Lily Flower": self.pickup_lily_flower,
                "Pick up Orange": self.pickup_oranges,
                "Pick up Orange 2": self.pickup_oranges,
                "Pick up Orange 3": self.pickup_oranges,
                "Pick up Tomato 1": self.pickup_tomatoes_high_street,
                "Pick up Tomato 2": self.pickup_tomatoes_high_street,
                "Pick up Tomato 3": self.pickup_tomatoes_high_street,
                "Pick up Shop Carrot 1": self.pickup_carrots_high_street,
                "Pick up Shop Carrot 2": self.pickup_carrots_high_street,
                "Pick up Shop Carrot 3": self.pickup_carrots_high_street,
                "Pick up Cucumber 1": self.pickup_cucumbers,
                "Pick up Cucumber 2": self.pickup_cucumbers,
                "Pick up Cucumber 3": self.pickup_cucumbers,
                "Pick up Leek 1": self.pickup_leeks,
                "Pick up Leek 2": self.pickup_leeks,
                "Pick up Leek 3": self.pickup_leeks,
                "Pick up Fusilage": self.pickup_fusilage,
                "Pick up Pint Bottle": self.pickup_pint_bottle_hub,
                "Pick up Pint Bottle 2": self.pickup_pint_bottle_high_street,
                "Pick up Pint Bottle 3": self.pickup_pint_bottle_high_street,
                "Pick up Spray Bottle": self.pickup_spray_bottle,
                "Pick up Walkie Talkie": self.pickup_walkie_talkies,
                "Pick up Walkie Talkie B": self.pickup_walkie_talkies,
                "Pick up Apple Core": self.pickup_apple_cores,
                "Pick up Apple Core 2": self.pickup_apple_cores,
                "Pick up Dustbin Lid": self.pickup_dustbin_lid,
                "Pick up Chalk": self.pickup_chalk,
                "Pick up Garden Fork": self.pickup_weed_tools,
                "Pick up Bow": self.pickup_red_bow,
                "Pick up Bow (Blue)": self.pickup_blue_bow,
                "Pick up Dummy": self.pickup_dummy,
                "Pick up Cricket Ball": self.pickup_cricket_ball,
                "Pick up Bust Pipe": self.pickup_bust_pipe,
                "Pick up Bust Hat": self.pickup_bust_hat,
                "Pick up Bust Glasses": self.pickup_bust_glasses,
                "Pick up Right Slipper": self.pickup_slippers,
                "Pick up Left Slipper": self.pickup_slippers,
                "Pick up Tea Cup": self.pickup_tea_cup,
                "Pick up Newspaper": self.pickup_newspaper,
                "Pick up Socks": self.pickup_socks,
                "Pick up Socks 2": self.pickup_socks,
                "Pick up Vase": self.pickup_vase,
                "Pick up Pot Stack": self.pickup_pot_stack,
                "Pick up Soap": self.pickup_soap,
                "Pick up Paintbrush": self.pickup_paintbrush,
                "Pick up Broken Vase Piece 1": self.pickup_vase_pieces,
                "Pick up Broken Vase Piece 2": self.pickup_vase_pieces,
                "Pick up Bra": self.pickup_bra,
                "Pick up Badminton Racket": self.pickup_badminton_racket,
                "Pick up Rose": self.pickup_rose,
                "Pick up Fishing Bobber": self.pickup_fishing_bobber,
                "Pick up Exit Letter": self.pickup_exit_letter,
                "Pick up Plate": self.pickup_plates,
                "Pick up Plate 2": self.pickup_plates,
                "Pick up Plate 3": self.pickup_plates,
                "Pick up Green Quoit 1": self.pickup_quoits,
                "Pick up Green Quoit 2": self.pickup_quoits,
                "Pick up Green Quoit 3": self.pickup_quoits,
                "Pick up Red Quoit 1": self.pickup_quoits,
                "Pick up Red Quoit 2": self.pickup_quoits,
                "Pick up Red Quoit 3": self.pickup_quoits,
                "Pick up Fork": self.pickup_forks,
                "Pick up Fork 2": self.pickup_forks,
                "Pick up Knife": self.pickup_knives,
                "Pick up Knife 2": self.pickup_knives,
                "Pick up Cork": self.pickup_cork,
                "Pick up Candlestick": self.pickup_candlestick,
                "Pick up Flower for Vase": self.pickup_vase_flower,
                "Pick up Dart 1": self.pickup_darts,
                "Pick up Dart 2": self.pickup_darts,
                "Pick up Dart 3": self.pickup_darts,
                "Pick up Harmonica": self.pickup_harmonica,
                "Pick up Pint Glass": self.pickup_pint_glass,
                "Pick up Toy Boat": self.pickup_toy_boat,
                "Pick up Wooly Hat": self.pickup_woolen_hat,
                "Pick up Pepper Grinder": self.pickup_pepper_grinder,
                "Pick up Pub Woman's Cloth": self.pickup_pub_woman_cloth,
                "Pick up Mini Person (Child)": self.pickup_people_miniatures,
                "Pick up Mini Person (Jumpsuit)": self.pickup_people_miniatures,
                "Pick up Mini Person (Gardener)": self.pickup_people_miniatures,
                "Pick up Mini Person (Old Woman)": self.pickup_people_miniatures,
                "Pick up Mini Person (Postie)": self.pickup_people_miniatures,
                "Pick up Mini Person (Vest Man)": self.pickup_people_miniatures,
                "Pick up Mini Person": self.pickup_people_miniatures,
                "Pick up Mini Person (Goose)": self.pickup_people_miniatures,
                "Pick up Mini Shovel": self.pickup_mini_shovel,
                "Pick up Poppy Flower": self.pickup_poppy,
                "Pick up Mini Phone Door": self.pickup_mini_phone_booth,
                "Pick up Mini Mail Pillar": self.pickup_mini_mail_pillar,
                "Pick up Timber Handle": self.pickup_timber_handle,
                "Pick up Golden Bell": self.pickup_golden_bell,
                "Pick up Carrot 1": self.pickup_garden_carrots,
                "Pick up Carrot 2": self.pickup_garden_carrots,
                "Pick up Carrot 3": self.pickup_garden_carrots,
                "Pick up Carrot 4": self.pickup_garden_carrots,
                "Pick up Carrot 5": self.pickup_garden_carrots,
                "Pick up Carrot 6": self.pickup_garden_carrots,
                "Pick up Carrot 7": self.pickup_garden_carrots,
                "Pick up Carrot 8": self.pickup_garden_carrots,
                "Pick up Carrot 9": self.pickup_garden_carrots,
                "Pick up Carrot 10": self.pickup_garden_carrots,
                "Pick up Pub Tomato 1": self.pickup_pub_boxed_tomatoes,
                "Pick up Pub Tomato 2": self.pickup_pub_boxed_tomatoes,
                "Pick up Pub Tomato 3": self.pickup_pub_boxed_tomatoes,
                "Pick up Pub Tomato 4": self.pickup_pub_boxed_tomatoes,
                "Pick up Pub Tomato 5": self.pickup_pub_boxed_tomatoes,
                "Pick up Pub Tomato 6": self.pickup_pub_boxed_tomatoes,
                "Pick up Pub Tomato 7": self.pickup_pub_boxed_tomatoes,
                "Pick up Pub Tomato 8": self.pickup_pub_boxed_tomatoes,
                "Pick up Pub Tomato 9": self.pickup_pub_boxed_tomatoes,
                "Pick up Pub Tomato 10": self.pickup_pub_open_tomatoes,
                "Pick up Pub Tomato 11": self.pickup_pub_open_tomatoes,
                "Pick up Garden Boot": self.pickup_boots,
                "Pick up Hub Boot": self.pickup_boots,
            }

        # Item Drag Rules
        if self.world.options.include_drag_items.value:
            self.drag_rules = {
                "Drag Rake": self.drag_rake,
                "Drag Picnic Basket": self.drag_picnic_basket,
                "Drag Esky": self.drag_esky,
                "Drag Shovel": self.drag_shovel,
                "Drag Pumpkin": self.drag_pumpkins,
                "Drag Pumpkin 2": self.drag_pumpkins,
                "Drag Pumpkin 3": self.drag_pumpkins,
                "Drag Pumpkin 4": self.drag_pumpkins,
                "Drag Watering Can": self.drag_watering_can,
                "Drag Gumboot 1": self.drag_gumboots,
                "Drag Gumboot 2": self.drag_gumboots,
                "Drag Gardener Sign": self.drag_gardener_sign,
                "Drag Wooden Crate": self.drag_wooden_crate,
                "Drag Fence Bolt": self.drag_fence_bolt,
                "Drag Mallet": self.drag_mallet,
                "Drag Shopping Basket": self.drag_shopping_basket,
                "Drag Black Umbrella": self.drag_umbrellas,
                "Drag Rainbow Umbrella": self.drag_umbrellas,
                "Drag Red Umbrella": self.drag_umbrellas,
                "Drag Push Broom": self.drag_push_broom,
                "Drag Broken Broom Head": self.drag_broom_head,
                "Drag Dustbin": self.drag_dustbin,
                "Drag Baby Doll": self.drag_baby_doll,
                "Drag Pricing Gun": self.drag_pricing_gun,
                "Drag Adding Machine": self.drag_adding_machine,
                "Drag Rose Box": self.drag_rose_box,
                "Drag Cricket Bat": self.drag_cricket_bat,
                "Drag Tea Pot": self.drag_tea_pot,
                "Drag Clippers": self.drag_clippers,
                "Drag Duck Statue": self.drag_duck_statue,
                "Drag Frog Statue": self.drag_frog_statue,
                "Drag Jeremy Fish": self.drag_jeremy_fish,
                "Drag Messy Sign": self.drag_messy_sign,
                "Drag Drawer": self.drag_drawer,
                "Drag Enamel Jug": self.drag_enamel_jug,
                "Drag Clean Sign": self.drag_clean_sign,
                "Drag Tackle Box": self.drag_tackle_box,
                "Drag Traffic Cone": self.drag_traffic_cone,
                "Drag Exit Parcel": self.drag_exit_parcel,
                "Drag Stealth Box": self.drag_stealth_box,
                "Drag No Goose Sign": self.drag_no_goose_sign,
                "Drag Portable Stool": self.drag_portable_stool,
                "Drag Dartboard": self.drag_dartboard,
                "Drag Mop Bucket": self.drag_mop_bucket,
                "Drag Mop": self.drag_mop,
                "Drag Delivery Box": self.drag_delivery_box,
                "Drag Burly Mans Bucket": self.drag_burly_mans_bucket,
                "Drag Mini Bench": self.drag_mini_benches,
                "Drag Mini Pump": self.drag_mini_pump,
                "Drag Mini Street Bench": self.drag_mini_benches,
                "Drag Birdbath": self.drag_mini_birdbath,
                "Drag Easel": self.drag_mini_easel,
                "Drag Sun Lounge": self.drag_sun_lounge,
                "Drag Topsoil Bag 1": self.drag_topsoil_bags,
                "Drag Topsoil Bag 2": self.drag_topsoil_bags,
                "Drag Topsoil Bag 3": self.drag_topsoil_bags,
            }

        # Interaction Rules
        if self.world.options.include_interactions.value:
            self.interaction_rules = {
                "Ring the Bike Bell": self.interact_bike_bell,
                "Turn on Garden Tap": self.interact_garden_water,
                "Turn on Sprinkler": self.interact_garden_water,
                "Open Intro Gate": self.interact_intro_gate,
                "Drop Something in the Well": self.interact_well,
                "Break Through Boards": self.interact_boards,
                "Unplug the Radio": self.interact_radio,
                "Open Black Umbrella": self.interact_umbrellas,
                "Open Rainbow Umbrella": self.interact_umbrellas,
                "Open Red Umbrella": self.interact_umbrellas,
                "Untie Boy's Laces (Left)": self.interact_boys_laces,
                "Untie Boy's Laces (Right)": self.interact_boys_laces,
                "Ring the Back Gardens Bell": self.interact_back_gardens_objects,
                "Spin the Windmill": self.interact_back_gardens_objects,
                "Spin Purple Flower": self.interact_back_gardens_objects,
                "Break Through Trellis": self.interact_trellis,
                "Spin Sunflower": self.interact_back_gardens_objects,
                "Play Wind Chime (C)": self.interact_back_gardens_objects,
                "Play Wind Chime (D)": self.interact_back_gardens_objects,
                "Play Wind Chime (E)": self.interact_back_gardens_objects,
                "Play Wind Chime (F)": self.interact_back_gardens_objects,
                "Play Wind Chime (G)": self.interact_back_gardens_objects,
                "Play Wind Chime (A)": self.interact_back_gardens_objects,
                "Play Wind Chime (B)": self.interact_back_gardens_objects,
                "Close Van Door (Left)": self.interact_van_doors,
                "Close Van Door (Right)": self.interact_van_doors,
                "Untie Burly Man's Laces (Left)": self.interact_burly_laces,
                "Untie Burly Man's Laces (Right)": self.interact_burly_laces,
                "Turn on Pub Tap": self.interact_pub_tap,
            }

        # Model Church Pecking Rules
        if self.world.options.include_model_church_pecks.value == 1:
            self.church_first_peck_rules = {
                "Peck Model Church Doorway": self.peck_church,
                "Peck Model Church Tower": self.peck_church,
            }
        elif self.world.options.include_model_church_pecks.value == 2:
            self.church_all_peck_rules = {
                "Peck Model Church Doorway 1": self.peck_church,
                "Peck Model Church Doorway 2": self.peck_church,
                "Peck Model Church Doorway 3": self.peck_church,
                "Peck Model Church Doorway 4": self.peck_church,
                "Peck Model Church Doorway 5": self.peck_church,
                "Peck Model Church Doorway 6": self.peck_church,
                "Peck Model Church Doorway 7": self.peck_church,
                "Peck Model Church Doorway 8": self.peck_church,
                "Peck Model Church Doorway 9": self.peck_church,
                "Peck Model Church Doorway 10": self.peck_church,
                "Peck Model Church Doorway 11": self.peck_church,
                "Peck Model Church Doorway 12": self.peck_church,
                "Peck Model Church Doorway 13": self.peck_church,
                "Peck Model Church Doorway 14": self.peck_church,
                "Peck Model Church Doorway 15": self.peck_church,
                "Peck Model Church Doorway 16": self.peck_church,
                "Peck Model Church Doorway 17": self.peck_church,
                "Peck Model Church Doorway 18": self.peck_church,
                "Peck Model Church Doorway 19": self.peck_church,
                "Peck Model Church Tower 1": self.peck_church,
                "Peck Model Church Tower 2": self.peck_church,
                "Peck Model Church Tower 3": self.peck_church,
                "Peck Model Church Tower 4": self.peck_church,
                "Peck Model Church Tower 5": self.peck_church,
                "Peck Model Church Tower 6": self.peck_church,
                "Peck Model Church Tower 7": self.peck_church,
                "Peck Model Church Tower 8": self.peck_church,
                "Peck Model Church Tower 9": self.peck_church,
                "Peck Model Church Tower 10": self.peck_church,
                "Peck Model Church Tower 11": self.peck_church,
                "Peck Model Church Tower 12": self.peck_church,
                "Peck Model Church Tower 13": self.peck_church,
                "Peck Model Church Tower 14": self.peck_church,
                "Peck Model Church Tower 15": self.peck_church,
                "Peck Model Church Tower 16": self.peck_church,
            }

        # Milestone Rules
        if self.world.options.include_milestone_locations.value and self.world.options.include_extra_tasks.value and self.world.options.include_speedrun_tasks.value:
            self.all_milestone_rules = {
                "All Garden Tasks Complete": self.all_garden_tasks,
                "All High Street Tasks Complete": self.all_high_street_tasks,
                "All Back Gardens Tasks Complete": self.all_back_gardens_tasks,
                "All Pub Tasks Complete": self.all_pub_tasks,
                "All Main Task Lists Complete": self.all_main_task_lists,
                "All To Do (As Well) Tasks Complete": self.all_to_do_as_well_tasks,
                "All Speedrun Tasks Complete": self.all_speedrun_tasks,
                "All Tasks Complete": self.all_tasks_complete,
            }
        elif self.world.options.include_milestone_locations.value and self.world.options.include_extra_tasks.value:
            self.non_speedrun_milestone_rules = {
                "All Garden Tasks Complete": self.all_garden_tasks,
                "All High Street Tasks Complete": self.all_high_street_tasks,
                "All Back Gardens Tasks Complete": self.all_back_gardens_tasks,
                "All Pub Tasks Complete": self.all_pub_tasks,
                "All Main Task Lists Complete": self.all_main_task_lists,
                "All To Do (As Well) Tasks Complete": self.all_to_do_as_well_tasks,
            }
        elif self.world.options.include_milestone_locations.value and self.world.options.include_speedrun_tasks.value:
            self.non_to_do_as_well_milestone_rules = {
                "All Garden Tasks Complete": self.all_garden_tasks,
                "All High Street Tasks Complete": self.all_high_street_tasks,
                "All Back Gardens Tasks Complete": self.all_back_gardens_tasks,
                "All Pub Tasks Complete": self.all_pub_tasks,
                "All Main Task Lists Complete": self.all_main_task_lists,
                "All Speedrun Tasks Complete": self.all_speedrun_tasks,
            }
        elif self.world.options.include_milestone_locations.value:
            self.basic_milestone_rules = {
                "All Garden Tasks Complete": self.all_garden_tasks,
                "All High Street Tasks Complete": self.all_high_street_tasks,
                "All Back Gardens Tasks Complete": self.all_back_gardens_tasks,
                "All Pub Tasks Complete": self.all_pub_tasks,
                "All Main Task Lists Complete": self.all_main_task_lists,
            }

        # Goals
        if self.world.options.goal.value == 0:
            self.simple_goal_rules = {
                "Get into the Model Village (Golden Bell Soul)": self.get_into_model_village,
            }
        # elif self.world.options.goal.value == 1:\
            # No special locations
        elif self.world.options.goal.value == 2:
            self.all_main_tasks_goal_rules = {
                "All Main Task Lists Complete (Golden Bell Soul)": self.all_main_task_lists,
            }
        elif self.world.options.goal.value == 3:
            self.all_speedrun_tasks_goal_rules = {
                "All Speedrun Tasks Complete (Golden Bell Soul)": self.all_speedrun_tasks,
            }
        elif self.world.options.goal.value == 4:
            self.all_non_speedrun_tasks_goal_rules = {
                "All Main Task Lists + To Do (As Well) Complete (Golden Bell Soul)": self.all_non_speedrun_tasks,
            }
        elif self.world.options.goal.value == 5:
            self.all_tasks_goal_rules = {
                "All Tasks Complete (Golden Bell Soul)": self.all_tasks_complete,
            }
        elif self.world.options.goal.value == 6:
            self.four_final_tasks_rules = {
                "Complete the Four Final Area Tasks (Golden Bell Soul)": self.four_final_tasks,
            }
        
        # Main Task Rules
        self.main_tasks_rules = {
            "Get into the garden": self.get_into_garden,
            "Get the groundskeeper wet": self.get_groundskeeper_wet,
            "Steal the groundskeeper's keys": self.steal_groundskeepers_keys,
            "Make the groundskeeper wear his sun hat": self.make_groundskeeper_wear_sun_hat,
            "Rake in the lake": self.rake_in_lake,
            "Have a picnic": self.picnic,
            "Make the groundskeeper hammer his thumb": self.make_groundskeeper_hammer_thumb,
            "Break the broom": self.break_broom,
            "Trap the boy in the phone booth": self.trap_boy_in_phone_booth,
            "Make the boy wear the wrong glasses": self.make_boy_wear_wrong_glasses,
            "Make someone buy back their own stuff": self.make_someone_buyback,
            "Get on TV": self.get_on_tv,
            "Go shopping": self.go_shopping,
            "Trap the shopkeeper in the garage": self.trap_shopkeep_in_garage,
            "Make someone break the fancy vase": self.make_someone_break_vase,
            "Help the woman dress up the bust": self.dress_up_bust,
            "Make the man spit out his tea": self.make_man_spit_out_tea,
            "Get dressed up with a ribbon": self.get_dressed_up,
            "Make the man go barefoot": self.make_man_barefoot,
            "Do the washing": self.do_washing,
            "Make someone prune the prize rose": self.make_someone_prune_rose,
            "Get into the pub": self.get_into_pub,
            "Break the dartboard": self.break_dartboard,
            "Get the toy boat": self.get_toy_boat,
            "Make the old man fall on his bum": self.make_old_man_fall_on_bum,
            "Be awarded a flower": self.be_awarded_flower,
            "Steal a pint glass and drop it in the canal": self.drop_pint_glass_in_canal,
            "Set the table": self.set_table,
            "Drop a bucket on the burly man's head": self.drop_bucket_on_burly_man,
        }
        
        # Model Village Rules/Victory Rules
        self.victory_rules = {
            "Get into the Model Village": self.get_into_model_village,
            "Steal the Beautiful Miniature Golden Bell": self.steal_bell,
            "...and Take it All the Way Back Home": self.steal_bell,
        }
    
    
    # ----- Region Defs -----
    
    def has_area(self, state: CollectionState, area) -> bool:
        return state.has(f"{area} Access", self.player)

    def has_garden(self, state: CollectionState) -> bool:
        return self.has_area(state, "Garden")

    def has_high_street(self, state: CollectionState) -> bool:
        return self.has_area(state, "High Street")

    def has_back_gardens(self, state: CollectionState) -> bool:
        return self.has_area(state, "Back Gardens")

    def has_pub(self, state: CollectionState) -> bool:
        return self.has_area(state, "Pub")

    def has_model_village(self, state: CollectionState) -> bool:
        return (
            self.has_area(state, "Pub")
            and self.has_area(state, "Model Village")
        )
    
    
    # ----- Souls Defs -----

    def has_npc(self, state: CollectionState, npc) -> bool:
        if self.world.options.include_npc_souls.value:
            return state.has(f"{npc} Soul", self.player)
        return True
    
    def has_prop(self, state: CollectionState, prop) -> bool:
        if self.world.options.include_prop_souls.value:
            return state.has(f"{prop} Soul", self.player)
        return True
    
    
    # ----- Garden Task Rule Defs -----
    
    def get_into_garden(self, state: CollectionState) -> bool:
        return self.has_garden(state)
    
    def get_groundskeeper_wet(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_npc(state, "Groundskeeper")
        )
    
    def steal_groundskeepers_keys(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_npc(state, "Groundskeeper")
        )
    
    def make_groundskeeper_wear_sun_hat(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_npc(state, "Groundskeeper")
            and self.has_prop(state, "Straw Hat")
            and self.has_prop(state, "Tulip")
        )
    
    def rake_in_lake(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Rake")
        )
    
    def picnic(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Picnic Basket") 
            and self.has_prop(state, "Apple") 
            and self.has_prop(state, "Sandwich")
            and self.has_prop(state, "Pumpkin")
            and self.has_prop(state, "Jam")
            and self.has_prop(state, "Thermos")
        )
    
    def make_groundskeeper_hammer_thumb(self, state: CollectionState) -> bool:
        task_count = 0
        
        if self.get_into_garden(state):
            task_count += 1
        if self.get_groundskeeper_wet(state):
            task_count += 1
        if self.steal_groundskeepers_keys(state):
            task_count += 1
        if self.make_groundskeeper_wear_sun_hat(state):
            task_count += 1
        if self.rake_in_lake(state):
            task_count += 1
        if self.picnic(state):
            task_count += 1
        
        return (
            self.has_npc(state, "Groundskeeper")
            and self.has_prop(state, "Mallet")
            and task_count >= 5
        )
    
    
    # ----- High Street Task Rule Defs -----
    
    def break_broom(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_npc(state, "Market Lady")
            and self.has_prop(state, "Push Broom")
        )
    
    def trap_boy_in_phone_booth(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_npc(state, "Boy")
            and self.has_npc(state, "TV Shop Owner")
        )
    
    def make_boy_wear_wrong_glasses(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_npc(state, "Boy")
            and (
                self.has_prop(state, "Horn-Rimmed Glasses")
                or self.has_prop(state, "Red Glasses") 
                or self.has_prop(state, "Sunglasses")
            )
        )
    
    def make_someone_buyback(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_npc(state, "Market Lady")
            and (
                self.has_npc(state, "Boy") and self.has_prop(state, "Fusilage")
                or self.has_garden(state) and self.has_npc(state, "Groundskeeper") and self.has_prop(state, "Trowel")
            )
        )
    
    def get_on_tv(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_npc(state, "TV Shop Owner")
            and (
                self.has_npc(state, "Boy")
                or self.has_prop(state, "Walkie Talkie")
            )
        )
    
    def go_shopping(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Shopping Basket")
            and self.has_prop(state, "Toothbrush")
            and self.has_prop(state, "Hairbrush")
            and self.has_prop(state, "Loo Paper")
            and (
                self.has_prop(state, "Dish Soap Bottle")
                or self.has_prop(state, "Spray Bottle")
            )
            and (
                self.has_prop(state, "Orange")
                or self.has_prop(state, "Cucumber")
                or self.has_prop(state, "Leek")
                or self.has_prop(state, "Carrot")
                or self.has_prop(state, "Tomato")
                or self.has_garden(state) and self.has_prop(state, "Apple")
            )
        )
    
    def trap_shopkeep_in_garage(self, state: CollectionState) -> bool:
        task_count = 0
        
        if self.break_broom(state):
            task_count += 1
        if self.trap_boy_in_phone_booth(state):
            task_count += 1
        if self.make_boy_wear_wrong_glasses(state):
            task_count += 1
        if self.make_someone_buyback(state):
            task_count += 1
        if self.get_on_tv(state):
            task_count += 1
        if self.go_shopping(state):
            task_count += 1
        
        return (
            self.has_npc(state, "Market Lady")
            and self.has_prop(state, "Chalk")
            and task_count >= 5
        )
    
    
    # ----- Back Gardens Task Rule Defs -----
    
    def make_someone_break_vase(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_npc(state, "Tidy Neighbour")
            and self.has_prop(state, "Vase")
            and self.has_prop(state, "Drawer")
        )
    
    def make_man_spit_out_tea(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_npc(state, "Tidy Neighbour")
            and self.has_prop(state, "Messy Neighbour")
            and self.has_prop(state, "Tea Cup")
        )
    
    def get_dressed_up(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_npc(state, "Messy Neighbour")
            and self.has_prop(state, "Duck Statue")
            and self.has_prop(state, "Bow")
        )
    
    def make_man_barefoot(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_npc(state, "Tidy Neighbour")
        )
    
    def do_washing(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_npc(state, "Tidy Neighbour")
            and self.has_prop(state, "Drawer")
            and self.has_prop(state, "Sock")
            and self.has_prop(state, "Bra")
            and self.has_prop(state, "Soap")
        )
    
    def dress_up_bust(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_npc(state, "Messy Neighbour")
            and self.has_prop(state, "Drawer")
            and (
                self.has_prop(state, "Bust Hat")
                or self.has_garden(state) and self.has_npc(state, "Groundskeeper")
                or self.has_pub(state) and (
                    self.has_prop(state, "Traffic Cone")
                    or self.has_npc(state, "Old Man")
                )
            )
            and (
                self.has_prop(state, "Bust Glasses")
                or self.has_high_street(state) and (
                    self.has_prop(state, "Horn-Rimmed Glasses")
                    or self.has_prop(state, "Red Glasses")
                    or self.has_prop(state, "Sunglasses")
                    or self.has_prop(state, "Stereoscope")
                    or self.has_npc(state, "Boy")
                )
            )
            and (
                self.has_prop(state, "Bust Pipe")
                or self.has_prop(state, "Dummy")
                or self.has_garden(state) and self.has_prop(state, "Tulip")
                or self.has_high_street(state) and (
                    self.has_prop(state, "Toothbrush")
                    or self.has_prop(state, "Lily Flower")
                )
                or self.has_pub(state) and (
                    self.has_prop(state, "Knife")
                    or self.has_prop(state, "Fork")
                    or self.has_npc(state, "Fancy Ladies") and self.has_prop(state, "Flower for Vase")
                )
                or self.has_model_village(state) and self.has_prop(state, "Poppy Flower")
                or ( # Rose from completing all other Back Gardens tasks
                    self.make_someone_break_vase(state)
                    and self.make_man_spit_out_tea(state)
                    and self.get_dressed_up(state)
                    and self.make_man_barefoot(state)
                    and self.do_washing(state)
                    and self.has_prop(state, "Rose")
                    # Removing Rose Box Soul until I can solve the physics issues with it
                    # and self.has_prop(state, "Rose Box")
                    and self.has_prop(state, "Clippers")
                    and self.has_prop(state, "Clean Sign")
                )
            )
        )
    
    def make_someone_prune_rose(self, state: CollectionState) -> bool:
        task_count = 0
        
        if self.make_someone_break_vase(state):
            task_count += 1
        if self.make_man_spit_out_tea(state):
            task_count += 1
        if self.get_dressed_up(state):
            task_count += 1
        if self.make_man_barefoot(state):
            task_count += 1
        if self.do_washing(state):
            task_count += 1
        if self.dress_up_bust(state):
            task_count += 1
        
        return (
            self.has_npc(state, "Tidy Neighbour")
            and self.has_npc(state, "Messy Neighbour")
            and self.has_prop(state, "Drawer")
            and self.has_prop(state, "Rose")
            # Removing Rose Box Soul until I can solve the physics issues with it
            # and self.has_prop(state, "Rose Box")
            and self.has_prop(state, "Clippers")
            and self.has_prop(state, "Clean Sign")
            and task_count >= 5
        )
    
    
    # ----- Pub Task Rule Defs -----
    
    def get_into_pub(self, state: CollectionState) -> bool:
        return self.has_pub(state)
    
    def break_dartboard(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_npc(state, "Old Man")
            and self.has_prop(state, "Dartboard")
        )
    
    def get_toy_boat(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Toy Boat")
        )
    
    def make_old_man_fall_on_bum(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_npc(state, "Old Man")
            and self.has_prop(state, "Portable Stool")
        )
    
    def be_awarded_flower(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_npc(state, "Fancy Ladies")
            and self.has_prop(state, "Flower for Vase")
        )
    
    def drop_pint_glass_in_canal(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Pint Glass")
        )
    
    def set_table(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Plate")
            and self.has_prop(state, "Fork")
            and self.has_prop(state, "Knife")
            and self.has_prop(state, "Pepper Grinder")
            and self.has_prop(state, "Candlestick")
        )
    
    def drop_bucket_on_burly_man(self, state: CollectionState) -> bool:
        task_count = 0
        
        if self.get_into_pub(state):
            task_count += 1
        if self.break_dartboard(state):
            task_count += 1
        if self.get_toy_boat(state):
            task_count += 1
        if self.make_old_man_fall_on_bum(state):
            task_count += 1
        if self.be_awarded_flower(state):
            task_count += 1
        if self.drop_pint_glass_in_canal(state):
            task_count += 1
        if self.set_table(state):
            task_count += 1
        
        return (
            self.has_npc(state, "Burly Man")
            and self.has_npc(state, "Pub Lady")
            and self.has_prop(state, "Burly Mans Bucket")
            and self.has_prop(state, "No Goose Sign")
            and task_count >= 6
        )
    
    
    # ----- To Do (As Well) Task Rule Defs -----
    
    def lock_groundskeeper_out(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_npc(state, "Groundskeeper")
        )
    
    def cabbage_picnic(self, state: CollectionState) -> bool:
        return self.has_garden(state)
    
    def trip_boy_in_puddle(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_npc(state, "Boy")
        )
    
    def make_scales_ding(self, state: CollectionState) -> bool:
        if not self.has_high_street(state):
            return False
        
        # Any of these provides enough items to complete the task alone
        if self.has_prop(state, "Carrot"):
            return True
        if self.has_prop(state, "Tomato"):
            return True
        if self.has_prop(state, "Orange"):
            return True
        if self.has_prop(state, "Leek"):
            return True
        if self.has_prop(state, "Cucumber"):
            return True
        if self.has_prop(state, "Tinned Food"):
            return True
        if self.has_pub(state) and self.has_prop(state, "Quoit"):
            return True
        if self.has_pub(state) and self.has_prop(state, "Plate"):
            return True
        if self.has_pub(state) and self.has_prop(state, "Dartboard"):
            return True
            
        item_count = 0
        
        # High Street items (1 each)
        if self.has_prop(state, "Toothbrush"):
            item_count += 1
        if self.has_prop(state, "Hairbrush"):
            item_count += 1
        if self.has_prop(state, "Loo Paper"):
            item_count += 1
        if self.has_prop(state, "Dish Soap Bottle"):
            item_count += 1
        if self.has_prop(state, "Spray Bottle"):
            item_count += 1
        if self.has_prop(state, "Toy Car"):
            item_count += 1
        if self.has_prop(state, "Horn-Rimmed Glasses"):
            item_count += 1
        if self.has_prop(state, "Red Glasses"):
            item_count += 1
        if self.has_prop(state, "Sunglasses"):
            item_count += 1
        if self.has_npc(state, "Boy"):
            item_count += 1
        if self.has_prop(state, "Fusilage"):
            item_count += 1
        if self.has_prop(state, "Lily Flower"):
            item_count += 1
        if self.has_prop(state, "Stereoscope"):
            item_count += 1
        if self.has_prop(state, "Dustbin Lid"):
            item_count += 1
        
        # High Street items (2 each)
        if self.has_prop(state, "Apple Core"):
            item_count += 2
        if self.has_prop(state, "Pint Bottle"):
            item_count += 2 # One in High Street, one in the hub near the dummy
        if self.has_prop(state, "Walkie Talkie"):
            item_count += 2
        if self.has_prop(state, "Weed Tool"):
            item_count += 2
        
        # Hub items (1 each)
        if self.has_prop(state, "Tennis Ball"):
            item_count += 1
        if self.has_prop(state, "Dummy"):
            item_count += 1
        if self.has_prop(state, "Fishing Bobber"):
            item_count += 1
        if self.has_prop(state, "Drink Can"):
            item_count += 1
        if self.has_prop(state, "Bow"):
            item_count += 1
        
        # Hub items (2 each)
        if self.has_prop(state, "Boot"):
            item_count += 2
        
        if self.has_garden(state):
            # Garden items (1 each)
            if self.has_prop(state, "Jam"):
                item_count += 1
            if self.has_prop(state, "Tulip"):
                item_count += 1
            if self.has_prop(state, "Picnic Mug"):
                item_count += 1
            if self.has_prop(state, "Thermos"):
                item_count += 1
            if self.has_prop(state, "Trowel"):
                item_count += 1
            if self.has_prop(state, "Radio"):
                item_count += 1
                
            # Garden items (2 each)
            if self.has_prop(state, "Apple"):
                item_count += 2
            if self.has_prop(state, "Sandwich"):
                item_count += 2
        
        if self.has_back_gardens(state):
            # Back Garden front items (1 each)
            if self.has_prop(state, "Tea Cup"):
                item_count += 1
            if self.has_prop(state, "Cricket Ball"):
                item_count += 1
            if self.has_prop(state, "Bust Pipe"):
                item_count += 1
            if self.has_prop(state, "Bust Hat"):
                item_count += 1
            if self.has_prop(state, "Bust Glasses"):
                item_count += 1
            if self.has_prop(state, "Newspaper"):
                item_count += 1
                
            # Back Garden back items (mostly 1 each)
            if self.has_prop(state, "Drawer"):
                if self.has_prop(state, "Soap"):
                    item_count += 1
                if self.has_prop(state, "Pot Stack"):
                    item_count += 1
                if self.has_prop(state, "Paintbrush"):
                    item_count += 1
                if self.has_prop(state, "Bra"):
                    item_count += 1
                    
                if self.has_prop(state, "Sock"):
                    item_count += 2
        
        if self.has_pub(state):
            # Pub items (1 each)
            if self.has_prop(state, "Cork"):
                item_count += 1
            if self.has_prop(state, "Exit Letter"):
                item_count += 1
            if self.has_prop(state, "Candlestick"):
                item_count += 1
            if self.has_prop(state, "Harmonica"):
                item_count += 1
            if self.has_prop(state, "Toy Boat"):
                item_count += 1
            if self.has_prop(state, "Pepper Grinder"):
                item_count += 1
                
            # Pub items (2 each)
            if self.has_prop(state, "Knife"):
                item_count += 2
            if self.has_prop(state, "Fork"):
                item_count += 2
        
        return item_count >= 3
    
    def open_umbrella_on_tv(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_npc(state, "TV Shop Owner")
            and self.has_npc(state, "Market Lady")
            and self.has_prop(state, "Umbrella")
            and (
                self.has_npc(state, "Boy")
                or self.has_prop(state, "Walkie Talkie")
            )
        )
    
    def make_groundskeeper_buyback(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_high_street(state)
            and self.has_npc(state, "Groundskeeper")
            and self.has_npc(state, "Market Lady")
            and self.has_prop(state, "Trowel")
        )
    
    def collect_five_flowers(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_high_street(state)
            and self.has_pub(state)
            and self.has_model_village(state)
            and self.has_npc(state, "Fancy Ladies")
            and self.has_prop(state, "Tulip")
            and self.has_prop(state, "Lily Flower")
            and self.has_prop(state, "Flower for Vase")
            and self.has_prop(state, "Poppy Flower")
            and self.make_someone_prune_rose(state)
        )
    
    def trap_boy_in_garage(self, state: CollectionState) -> bool:
        return self.trap_shopkeep_in_garage(state)
    
    def catch_thrown_object(self, state: CollectionState) -> bool:
        return ( # Tracking any prop souls here is unnecessary as it can be done with the Fence Bolt from the starting area
            self.has_back_gardens(state)
            and self.has_npc(state, "Tidy Neighbour")
            and self.has_prop(state, "Drawer")
        )
    
    def get_thrown_over_fence(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_pub(state)
            and self.has_npc(state, "Tidy Neighbour")
            and self.has_prop(state, "Drawer")
            and self.has_prop(state, "Stealth Box")
        )
    
    def dress_up_bust_outside_items(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_npc(state, "Messy Neighbour")
            and self.has_prop(state, "Drawer")
            and (
                self.has_garden(state) and self.has_npc(state, "Groundskeeper")
                or self.has_pub(state) and (
                    self.has_prop(state, "Traffic Cone")
                    or self.has_npc(state, "Old Man")
                )
            )
            and self.has_high_street(state) and (
                self.has_prop(state, "Horn-Rimmed Glasses")
                or self.has_prop(state, "Red Glasses")
                or self.has_prop(state, "Sunglasses")
                or self.has_prop(state, "Stereoscope")
                or self.has_npc(state, "Boy")
            )
            and (
                self.has_prop(state, "Dummy")
                or self.has_garden(state) and self.has_prop(state, "Tulip")
                or self.has_high_street(state) and (
                    self.has_prop(state, "Toothbrush")
                    or self.has_prop(state, "Lily Flower")
                )
                or self.has_pub(state) and (
                    self.has_prop(state, "Knife")
                    or self.has_prop(state, "Fork")
                    or self.has_npc(state, "Fancy Ladies") and self.has_prop(state, "Flower for Vase")
                )
                or self.has_model_village(state) and self.has_prop(state, "Poppy Flower")
            )
        )
    
    def score_goal(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.make_someone_prune_rose(state)
            and self.has_prop(state, "Messy Sign")
        )
    
    def sail_boat_under_bridge(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Toy Boat")
        )
    
    def perform_with_ribbon(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_back_gardens(state)
            and self.has_npc(state, "Fancy Ladies")
            and self.has_npc(state, "Messy Neighbour")
            and self.has_prop(state, "Duck Statue")
            and self.has_prop(state, "Bow")
            and self.has_prop(state, "Drawer")
        )
    
    def steal_woolen_hat(self, state: CollectionState) -> bool:
        return self.make_old_man_fall_on_bum(state)
    
    
    # ----- To Do (Quickly!!) Task Rule Defs -----
    
    def speedrun_garden(self, state: CollectionState) -> bool:
        return self.make_groundskeeper_hammer_thumb(state)
    
    def speedrun_high_street(self, state: CollectionState) -> bool:
        return self.trap_shopkeep_in_garage(state)
    
    def speedrun_back_gardens(self, state: CollectionState) -> bool:
        task_count = 0
        
        if self.make_someone_break_vase(state):
            task_count += 1
        if self.make_man_spit_out_tea(state):
            task_count += 1
        if self.get_dressed_up(state):
            task_count += 1
        if self.make_man_barefoot(state):
            task_count += 1
        if self.do_washing(state):
            task_count += 1
        if (
            self.has_npc(state, "Messy Neighbour")
            and self.has_prop(state, "Bust Hat")
            and self.has_prop(state, "Bust Glasses")
            and self.has_prop(state, "Bust Pipe")
        ):
            task_count += 1
        
        return (
            self.has_npc(state, "Tidy Neighbour")
            and self.has_npc(state, "Messy Neighbour")
            and self.has_prop(state, "Drawer")
            and self.has_prop(state, "Rose")
            # Removing Rose Box Soul until I can solve the physics issues with it
            # and self.has_prop(state, "Rose Box")
            and self.has_prop(state, "Clippers")
            and self.has_prop(state, "Clean Sign")
            and task_count >= 5
        )
    
    def speedrun_pub(self, state: CollectionState) -> bool:
        return self.drop_bucket_on_burly_man(state)
    
    
    # ----- Milestone & Goal Defs -----
    
    def all_garden_tasks(self, state: CollectionState) -> bool:
        return (
            self.get_into_garden(state)
            and self.get_groundskeeper_wet(state)
            and self.steal_groundskeepers_keys(state)
            and self.make_groundskeeper_wear_sun_hat(state)
            and self.rake_in_lake(state)
            and self.picnic(state)
            and self.make_groundskeeper_hammer_thumb(state)
        )
    
    def all_high_street_tasks(self, state: CollectionState) -> bool:
        return (
            self.break_broom(state)
            and self.trap_boy_in_phone_booth(state)
            and self.make_boy_wear_wrong_glasses(state)
            and self.make_someone_buyback(state)
            and self.get_on_tv(state)
            and self.go_shopping(state)
            and self.trap_shopkeep_in_garage(state)
        )
    
    def all_back_gardens_tasks(self, state: CollectionState) -> bool:
        return (
            self.make_someone_break_vase(state)
            and self.make_man_spit_out_tea(state)
            and self.get_dressed_up(state)
            and self.make_man_barefoot(state)
            and self.do_washing(state)
            and self.dress_up_bust(state)
            and self.make_someone_prune_rose(state)
        )
    
    def all_pub_tasks(self, state: CollectionState) -> bool:
        return (
            self.get_into_pub(state)
            and self.break_dartboard(state)
            and self.get_toy_boat(state)
            and self.make_old_man_fall_on_bum(state)
            and self.be_awarded_flower(state)
            and self.drop_pint_glass_in_canal(state)
            and self.set_table(state)
            and self.drop_bucket_on_burly_man(state)
        )
    
    def all_main_task_lists(self, state: CollectionState) -> bool:
        return (
            self.all_garden_tasks(state)
            and self.all_high_street_tasks(state)
            and self.all_back_gardens_tasks(state)
            and self.all_pub_tasks(state)
        )
    
    def all_to_do_as_well_tasks(self, state: CollectionState) -> bool:
        return (
            self.lock_groundskeeper_out(state)
            and self.cabbage_picnic(state)
            and self.trip_boy_in_puddle(state)
            and self.make_scales_ding(state)
            and self.open_umbrella_on_tv(state)
            and self.make_groundskeeper_buyback(state)
            and self.collect_five_flowers(state)
            and self.trap_boy_in_garage(state)
            and self.catch_thrown_object(state)
            and self.get_thrown_over_fence(state)
            and self.dress_up_bust_outside_items(state)
            and self.score_goal(state)
            and self.sail_boat_under_bridge(state)
            and self.perform_with_ribbon(state)
            and self.steal_woolen_hat(state)
        )
    
    def all_speedrun_tasks(self, state: CollectionState) -> bool:
        return (
            self.speedrun_garden(state)
            and self.speedrun_high_street(state)
            and self.speedrun_back_gardens(state)
            and self.speedrun_pub(state)
        )
    
    def all_tasks_complete(self, state: CollectionState) -> bool:
        return (
            self.all_main_task_lists(state)
            and self.all_to_do_as_well_tasks(state)
            and self.all_speedrun_tasks(state)
        )
    
    def all_non_speedrun_tasks(self, state: CollectionState) -> bool:
        return (
            self.all_main_task_lists(state)
            and self.all_to_do_as_well_tasks(state)
        )
    
    def four_final_tasks(self, state: CollectionState) -> bool:
        return (
            self.make_groundskeeper_hammer_thumb(state)
            and self.trap_shopkeep_in_garage(state)
            and self.make_someone_prune_rose(state)
            and self.drop_bucket_on_burly_man(state)
        )
    
    
    # ----- Model Village Defs -----
    
    def get_into_model_village(self, state: CollectionState) -> bool:
        return self.has_model_village(state)
    
    def steal_bell(self, state: CollectionState) -> bool:
        if self.world.options.logically_require_npc_souls.value:
            return (
                self.has_garden(state)
                and self.has_high_street(state)
                and self.has_back_gardens(state)
                and self.has_pub(state)
                and self.has_model_village(state)
                and self.has_prop(state, "Timber Handle")
                and state.has("Golden Bell Soul", self.player)
                and self.has_npc(state, "Groundskeeper")
                and self.has_npc(state, "Boy")
                and self.has_npc(state, "TV Shop Owner")
                and self.has_npc(state, "Market Lady")
                and self.has_npc(state, "Tidy Neighbour")
                and self.has_npc(state, "Messy Neighbour")
                and self.has_npc(state, "Burly Man")
                and self.has_npc(state, "Old Man")
                and self.has_npc(state, "Pub Lady")
                and self.has_npc(state, "Fancy Ladies")
                and self.has_npc(state, "Cook")
            )

        return (
            self.has_garden(state)
            and self.has_high_street(state)
            and self.has_back_gardens(state)
            and self.has_pub(state)
            and self.has_model_village(state)
            and self.has_prop(state, "Timber Handle")
            and state.has("Golden Bell Soul", self.player)
        )
    
    
    # ----- Hub Item Pickup Defs -----
    
    def pickup_drink_can(self, state: CollectionState) -> bool:
        return self.has_prop(state, "Drink Can")
    
    def pickup_tennis_ball(self, state: CollectionState) -> bool:
        return self.has_prop(state, "Tennis Ball")
    
    def pickup_blue_bow(self, state: CollectionState) -> bool:
        return self.has_prop(state, "Bow")
    
    def pickup_dummy(self, state: CollectionState) -> bool:
        return self.has_prop(state, "Dummy")
    
    def pickup_fishing_bobber(self, state: CollectionState) -> bool:
        return self.has_prop(state, "Fishing Bobber")
    
    def pickup_boots(self, state: CollectionState) -> bool:
        return self.has_prop(state, "Boot")
    
    
    # ----- Garden Item Pickup Defs -----
    
    def pickup_radio(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Radio")
        )
    
    def pickup_trowel(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Trowel")
        )
    
    def pickup_gardener_gear(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_npc(state, "Groundskeeper")
        )
    
    def pickup_tulip(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Tulip")
        )
    
    def pickup_apples(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Apple")
        )
    
    def pickup_jam(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Jam")
        )
    
    def pickup_picnic_mug(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Picnic Mug")
        )
    
    def pickup_thermos(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Thermos")
        )
    
    def pickup_sandwich(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Sandwich")
        )
    
    def pickup_straw_hat(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_npc(state, "Groundskeeper")
            and self.has_prop(state, "Straw Hat")
        )
    
    def pickup_garden_carrots(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Carrot")
        )
    
    
    # ----- High Street Item Pickup Defs -----
    
    def pickup_boys_glasses(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_npc(state, "Boy")
        )
    
    def pickup_horn_rimmed_glasses(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Horn-Rimmed Glasses")
        )
    
    def pickup_red_glasses(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Red Glasses")
        )
    
    def pickup_sunglasses(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Sunglasses")
        )
    
    def pickup_loo_paper(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Loo Paper")
        )
    
    def pickup_toy_car(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Toy Car")
        )
    
    def pickup_hairbrush(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Hairbrush")
        )
    
    def pickup_toothbrush(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Toothbrush")
        )
    
    def pickup_stereoscope(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Stereoscope")
        )
    
    def pickup_dish_soap_bottle(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Dish Soap Bottle")
        )
    
    def pickup_food_cans(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Tinned Food")
        )
    
    def pickup_weed_tools(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Weed Tool")
        )
    
    def pickup_lily_flower(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Lily Flower")
        )
    
    def pickup_oranges(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Orange")
        )
    
    def pickup_tomatoes_high_street(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Tomato")
        )
    
    def pickup_carrots_high_street(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Carrot")
        )
    
    def pickup_cucumbers(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Cucumber")
        )
    
    def pickup_leeks(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Leek")
        )
    
    def pickup_fusilage(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Fusilage")
        )
    
    def pickup_pint_bottle_hub(self, state: CollectionState) -> bool:
        return self.has_prop(state, "Pint Bottle")
    
    def pickup_pint_bottle_high_street(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Pint Bottle")
        )
    
    def pickup_spray_bottle(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Spray Bottle")
        )
    
    def pickup_walkie_talkies(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Walkie Talkie")
        )
    
    def pickup_apple_cores(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Apple Core")
        )
    
    def pickup_dustbin_lid(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Dustbin Lid")
        )
    
    def pickup_chalk(self, state: CollectionState) -> bool:
        return self.trap_shopkeep_in_garage(state)
    
    
    # ----- Back Gardens Item Pickup Defs -----
    
    def pickup_red_bow(self, state: CollectionState) -> bool:
        return self.get_dressed_up(state)
    
    def pickup_cricket_ball(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Cricket Ball")
        )
    
    def pickup_bust_pipe(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Bust Pipe")
        )
    
    def pickup_bust_hat(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Bust Hat")
        )
    
    def pickup_bust_glasses(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Bust Glasses")
        )
    
    def pickup_slippers(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_npc(state, "Tidy Neighbour")
        )
    
    def pickup_tea_cup(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Tea Cup")
        )
    
    def pickup_newspaper(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Newspaper")
        )
    
    def pickup_socks(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Sock")
            and self.has_prop(state, "Drawer")
        )
    
    def pickup_vase(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Vase")
            and self.has_prop(state, "Drawer")
        )
    
    def pickup_pot_stack(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Pot Stack")
            and self.has_prop(state, "Drawer")
        )
    
    def pickup_soap(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Soap")
            and self.has_prop(state, "Drawer")
        )
    
    def pickup_paintbrush(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Paintbrush")
            and self.has_prop(state, "Drawer")
        )
    
    def pickup_vase_pieces(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_npc(state, "Tidy Neighbour")
            and self.has_prop(state, "Vase")
            and self.has_prop(state, "Drawer")
        )
    
    def pickup_bra(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Bra")
            and self.has_prop(state, "Drawer")
        )
    
    def pickup_badminton_racket(self, state: CollectionState) -> bool:
        return (
            self.has_prop(state, "Badminton Racket")
            and self.has_prop(state, "Messy Sign")
            and self.make_someone_prune_rose(state)
        )
    
    def pickup_rose(self, state: CollectionState) -> bool:
        return self.make_someone_prune_rose(state)
    
    
    # ----- Pub Item Pickup Defs -----
    
    def pickup_exit_letter(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Exit Letter")
        )
    
    def pickup_plates(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Plate")
        )
    
    def pickup_quoits(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Quoit")
        )
    
    def pickup_forks(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Fork")
        )
    
    def pickup_knives(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Knife")
        )
    
    def pickup_cork(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Cork")
        )
    
    def pickup_candlestick(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Candlestick")
        )
    
    def pickup_vase_flower(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_npc(state, "Fancy Ladies")
            and self.has_prop(state, "Flower for Vase")
        )
    
    def pickup_darts(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_npc(state, "Old Man")
            and self.has_prop(state, "Dartboard")
        )
    
    def pickup_harmonica(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Harmonica")
        )
    
    def pickup_pint_glass(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Pint Glass")
        )
    
    def pickup_toy_boat(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Toy Boat")
        )
    
    def pickup_woolen_hat(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_npc(state, "Old Man")
        )
    
    def pickup_pepper_grinder(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Pepper Grinder")
        )
    
    def pickup_pub_woman_cloth(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_npc(state, "Pub Lady")
        )
    
    def pickup_pub_open_tomatoes(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Tomato")
        )
    
    def pickup_pub_boxed_tomatoes(self, state: CollectionState) -> bool:
        task_count = 0
        
        if self.get_into_pub(state):
            task_count += 1
        if self.break_dartboard(state):
            task_count += 1
        if self.get_toy_boat(state):
            task_count += 1
        if self.make_old_man_fall_on_bum(state):
            task_count += 1
        if self.be_awarded_flower(state):
            task_count += 1
        if self.drop_pint_glass_in_canal(state):
            task_count += 1
        if self.set_table(state):
            task_count += 1
        
        return (
            self.has_npc(state, "Pub Lady")
            and self.has_prop(state, "No Goose Sign")
            and self.has_prop(state, "Tomato")
            and task_count >= 6
        )
    
    
    # ----- Model Village Item Pickup Defs -----
    
    def pickup_people_miniatures(self, state: CollectionState) -> bool:
        return (
            self.has_model_village(state)
            and self.has_prop(state, "Mini Person")
        )
    
    def pickup_mini_shovel(self, state: CollectionState) -> bool:
        return (
            self.has_model_village(state)
            and self.has_prop(state, "Mini Shovel")
        )
    
    def pickup_poppy(self, state: CollectionState) -> bool:
        return (
            self.has_model_village(state)
            and self.has_prop(state, "Poppy Flower")
        )
    
    def pickup_mini_phone_booth(self, state: CollectionState) -> bool:
        return (
            self.has_model_village(state)
            and self.has_prop(state, "Mini Phone Door")
        )
    
    def pickup_mini_mail_pillar(self, state: CollectionState) -> bool:
        return (
            self.has_model_village(state)
            and self.has_prop(state, "Mini Mail Pillar")
        )
    
    def pickup_timber_handle(self, state: CollectionState) -> bool:
        return (
            self.has_model_village(state)
            and self.has_prop(state, "Timber Handle")
        )
    
    def pickup_golden_bell(self, state: CollectionState) -> bool:
        return self.steal_bell(state)
    
    
    # ----- Hub Item Drag Defs -----
    
    def drag_fence_bolt(self, state: CollectionState) -> bool:
        return True
    
    def drag_tackle_box(self, state: CollectionState) -> bool:
        return self.has_prop(state, "Tackle Box")
    
    
    # ----- Garden Item Drag Defs -----
    
    def drag_rake(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Rake")
        )
    
    def drag_picnic_basket(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Picnic Basket")
        )
    
    def drag_esky(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Esky")
        )
    
    def drag_shovel(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Shovel")
        )
    
    def drag_pumpkins(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Pumpkin")
        )
    
    def drag_watering_can(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Watering Can")
        )
    
    def drag_gumboots(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Gumboot")
        )
    
    def drag_gardener_sign(self, state: CollectionState) -> bool:
        task_count = 0
        
        if self.get_into_garden(state):
            task_count += 1
        if self.get_groundskeeper_wet(state):
            task_count += 1
        if self.steal_groundskeepers_keys(state):
            task_count += 1
        if self.make_groundskeeper_wear_sun_hat(state):
            task_count += 1
        if self.rake_in_lake(state):
            task_count += 1
        if self.picnic(state):
            task_count += 1
        
        return (
            self.has_npc(state, "Groundskeeper")
            and task_count >= 5
        )
    
    def drag_wooden_crate(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Wooden Crate")
        )
    
    def drag_mallet(self, state: CollectionState) -> bool:
        return self.make_groundskeeper_hammer_thumb(state)
    
    def drag_topsoil_bags(self, state: CollectionState) -> bool:
        return (
            self.has_garden(state)
            and self.has_prop(state, "Topsoil Bag")
        )
    
    
    # ----- High Street Item Drag Defs -----
    
    def drag_shopping_basket(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Shopping Basket")
        )
    
    def drag_umbrellas(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_npc(state, "Market Lady")
            and self.has_prop(state, "Umbrella")
        )
    
    def drag_push_broom(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Push Broom")
        )
    
    def drag_broom_head(self, state: CollectionState) -> bool:
        return self.break_broom(state)
    
    def drag_dustbin(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Dustbin")
        )
    
    def drag_baby_doll(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Baby Doll")
        )
    
    def drag_pricing_gun(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Pricing Gun")
        )
    
    def drag_adding_machine(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_prop(state, "Adding Machine")
        )
    
    
    # ----- Back Gardens Item Drag Defs -----
    
    def drag_rose_box(self, state: CollectionState) -> bool:
        task_count = 0
        
        if self.make_someone_break_vase(state):
            task_count += 1
        if self.make_man_spit_out_tea(state):
            task_count += 1
        if self.get_dressed_up(state):
            task_count += 1
        if self.make_man_barefoot(state):
            task_count += 1
        if self.do_washing(state):
            task_count += 1
        if self.dress_up_bust(state):
            task_count += 1
        
        return (
            self.has_npc(state, "Tidy Neighbour")
            # Removing Rose Box Soul until I can solve the physics issues with it
            # and self.has_prop(state, "Rose Box")
            and self.has_prop(state, "Clean Sign")
            and task_count >= 5
        )
    
    def drag_cricket_bat(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Cricket Bat")
        )
    
    def drag_tea_pot(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Tea Pot")
        )
    
    def drag_clippers(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Clippers")
        )
    
    def drag_duck_statue(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Duck Statue")
            and self.has_prop(state, "Drawer")
        )
    
    def drag_frog_statue(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Frog Statue")
            and self.has_prop(state, "Drawer")
        )
    
    def drag_jeremy_fish(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Jeremy Fish")
            and self.has_prop(state, "Drawer")
        )
    
    def drag_messy_sign(self, state: CollectionState) -> bool:
        return (
            self.has_prop(state, "Messy Sign")
            and self.make_someone_prune_rose(state)
        )
    
    def drag_drawer(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Drawer")
        )
    
    def drag_enamel_jug(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Enamel Jug")
            and self.has_prop(state, "Drawer")
        )
    
    def drag_clean_sign(self, state: CollectionState) -> bool:
        task_count = 0
        
        if self.make_someone_break_vase(state):
            task_count += 1
        if self.make_man_spit_out_tea(state):
            task_count += 1
        if self.get_dressed_up(state):
            task_count += 1
        if self.make_man_barefoot(state):
            task_count += 1
        if self.do_washing(state):
            task_count += 1
        if self.dress_up_bust(state):
            task_count += 1
        
        return (
            self.has_npc(state, "Tidy Neighbour")
            and self.has_prop(state, "Clean Sign")
            and task_count >= 5
        )
    
    
    # ----- Pub Item Drag Defs -----
    
    def drag_traffic_cone(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Traffic Cone")
        )
    
    def drag_exit_parcel(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Exit Parcel")
        )
    
    def drag_stealth_box(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Stealth Box")
        )
    
    def drag_no_goose_sign(self, state: CollectionState) -> bool:
        task_count = 0
        
        if self.get_into_pub(state):
            task_count += 1
        if self.break_dartboard(state):
            task_count += 1
        if self.get_toy_boat(state):
            task_count += 1
        if self.make_old_man_fall_on_bum(state):
            task_count += 1
        if self.be_awarded_flower(state):
            task_count += 1
        if self.drop_pint_glass_in_canal(state):
            task_count += 1
        if self.set_table(state):
            task_count += 1
        
        return (
            self.has_npc(state, "Pub Lady")
            and self.has_prop(state, "No Goose Sign")
            and task_count >= 6
        )
    
    def drag_portable_stool(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Portable Stool")
        )
    
    def drag_dartboard(self, state: CollectionState) -> bool:
        return self.break_dartboard(state)
    
    def drag_mop_bucket(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Mop Bucket")
        )
    
    def drag_mop(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Mop")
        )
    
    def drag_delivery_box(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_npc(state, "Cook")
        )
    
    def drag_burly_mans_bucket(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_prop(state, "Burly Mans Bucket")
        )
    
    
    # ----- Model Village Item Drag Defs -----
    
    def drag_mini_benches(self, state: CollectionState) -> bool:
        return (
            self.has_model_village(state)
            and self.has_prop(state, "Mini Bench")
        )
    
    def drag_mini_pump(self, state: CollectionState) -> bool:
        return (
            self.has_model_village(state)
            and self.has_prop(state, "Mini Pump")
        )
    
    def drag_mini_birdbath(self, state: CollectionState) -> bool:
        return (
            self.has_model_village(state)
            and self.has_prop(state, "Birdbath")
        )
    
    def drag_mini_easel(self, state: CollectionState) -> bool:
        return (
            self.has_model_village(state)
            and self.has_prop(state, "Easel")
        )
    
    def drag_sun_lounge(self, state: CollectionState) -> bool:
        return (
            self.has_model_village(state)
            and self.has_prop(state, "Sun Lounge")
        )
    
    
    # ----- Interaction & Church Pecking Defs -----
    
    def interact_bike_bell(self, state: CollectionState) -> bool:
        return True
    
    def interact_garden_water(self, state: CollectionState) -> bool:
        return self.has_garden(state)
    
    def interact_intro_gate(self, state: CollectionState) -> bool:
        return True
    
    def interact_well(self, state: CollectionState) -> bool:
        return True
    
    def interact_boards(self, state: CollectionState) -> bool:
        return self.has_back_gardens(state)
    
    def interact_radio(self, state: CollectionState) -> bool:
        return self.has_high_street(state)
    
    def interact_umbrellas(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_npc(state, "Market Lady")
            and self.has_prop(state, "Umbrella")
        )
    
    def interact_boys_laces(self, state: CollectionState) -> bool:
        return (
            self.has_high_street(state)
            and self.has_npc(state, "Boy")
        )
    
    def interact_back_gardens_objects(self, state: CollectionState) -> bool:
        return (
            self.has_back_gardens(state)
            and self.has_prop(state, "Drawer")
        )
    
    def interact_trellis(self, state: CollectionState) -> bool:
        return self.has_back_gardens(state)
    
    def interact_van_doors(self, state: CollectionState) -> bool:
        return self.has_pub(state)
    
    def interact_burly_laces(self, state: CollectionState) -> bool:
        return (
            self.has_pub(state)
            and self.has_npc(state, "Burly Man")
        )
    
    def interact_pub_tap(self, state: CollectionState) -> bool:
        return self.has_pub(state)
    
    def peck_church(self, state: CollectionState) -> bool:
        return self.has_model_village(state)
    
    
    # --------------- Set Rules ---------------

    def set_rules(self) -> None:
        # Model Village Rules/Victory Rules
        for location, rules in self.victory_rules.items():
            new_rule = self.world.multiworld.get_location(location, self.player)
            set_rule(new_rule, rules)
        
        # Main Task Rules
        for location, rules in self.main_tasks_rules.items():
            new_rule = self.world.multiworld.get_location(location, self.player)
            set_rule(new_rule, rules)
        
        # To Do (As Well) Task Rules
        if self.world.options.include_extra_tasks.value:
            for location, rules in self.extra_task_rules.items():
                new_rule = self.world.multiworld.get_location(location, self.player)
                set_rule(new_rule, rules)

        # To Do (Quickly!!) Task Rules
        if self.world.options.include_speedrun_tasks.value:
            for location, rules in self.speedrun_task_rules.items():
                new_rule = self.world.multiworld.get_location(location, self.player)
                set_rule(new_rule, rules)

        # Item Pickup Rules
        if self.world.options.include_item_pickups.value:
            for location, rules in self.pickup_rules.items():
                new_rule = self.world.multiworld.get_location(location, self.player)
                set_rule(new_rule, rules)

        # Item Drag Rules
        if self.world.options.include_drag_items.value:
            for location, rules in self.drag_rules.items():
                new_rule = self.world.multiworld.get_location(location, self.player)
                set_rule(new_rule, rules)

        # Interaction Rules
        if self.world.options.include_interactions.value:
            for location, rules in self.interaction_rules.items():
                new_rule = self.world.multiworld.get_location(location, self.player)
                set_rule(new_rule, rules)

        # Model Church Pecking Rules
        if self.world.options.include_model_church_pecks.value == 1:
            for location, rules in self.church_first_peck_rules.items():
                new_rule = self.world.multiworld.get_location(location, self.player)
                set_rule(new_rule, rules)
        elif self.world.options.include_model_church_pecks.value == 2:
            for location, rules in self.church_all_peck_rules.items():
                new_rule = self.world.multiworld.get_location(location, self.player)
                set_rule(new_rule, rules)

        # Milestone Rules
        if self.world.options.include_milestone_locations.value and self.world.options.include_extra_tasks.value and self.world.options.include_speedrun_tasks.value:
            for location, rules in self.all_milestone_rules.items():
                new_rule = self.world.multiworld.get_location(location, self.player)
                set_rule(new_rule, rules)
        elif self.world.options.include_milestone_locations.value and self.world.options.include_extra_tasks.value:
            for location, rules in self.non_speedrun_milestone_rules.items():
                new_rule = self.world.multiworld.get_location(location, self.player)
                set_rule(new_rule, rules)
        elif self.world.options.include_milestone_locations.value and self.world.options.include_speedrun_tasks.value:
            for location, rules in self.non_to_do_as_well_milestone_rules.items():
                new_rule = self.world.multiworld.get_location(location, self.player)
                set_rule(new_rule, rules)
        elif self.world.options.include_milestone_locations.value:
            for location, rules in self.basic_milestone_rules.items():
                new_rule = self.world.multiworld.get_location(location, self.player)
                set_rule(new_rule, rules)

        # Goals
        if self.world.options.goal.value == 0:
            for location, rules in self.simple_goal_rules.items():
                new_rule = self.world.multiworld.get_location(location, self.player)
                set_rule(new_rule, rules)
        # elif self.world.options.goal.value == 1:\
            # No special locations
        elif self.world.options.goal.value == 2:
            for location, rules in self.all_main_tasks_goal_rules.items():
                new_rule = self.world.multiworld.get_location(location, self.player)
                set_rule(new_rule, rules)
        elif self.world.options.goal.value == 3:
            for location, rules in self.all_speedrun_tasks_goal_rules.items():
                new_rule = self.world.multiworld.get_location(location, self.player)
                set_rule(new_rule, rules)
        elif self.world.options.goal.value == 4:
            for location, rules in self.all_non_speedrun_tasks_goal_rules.items():
                new_rule = self.world.multiworld.get_location(location, self.player)
                set_rule(new_rule, rules)
        elif self.world.options.goal.value == 5:
            for location, rules in self.all_tasks_goal_rules.items():
                new_rule = self.world.multiworld.get_location(location, self.player)
                set_rule(new_rule, rules)
        elif self.world.options.goal.value == 6:
            for location, rules in self.four_final_tasks_rules.items():
                new_rule = self.world.multiworld.get_location(location, self.player)
                set_rule(new_rule, rules)
        
        self.world.multiworld.completion_condition[self.player] = self.steal_bell