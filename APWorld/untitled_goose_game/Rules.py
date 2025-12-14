from typing import TYPE_CHECKING
from worlds.generic.Rules import set_rule

if TYPE_CHECKING:
    from . import GooseGameWorld


def set_rules(world: "GooseGameWorld") -> None:
    """Set access rules for regions and locations
    
    IMPORTANT: This APWorld uses a hub-based region structure.
    The Hub (well area) is the starting location.
    
    Access Requirements:
    - Hub: Always accessible (starting area)
    - Garden: Requires "Garden Access"
    - High Street: Requires "High Street Access"
    - Back Gardens: Requires "Back Gardens Access"
    - Pub: Requires "Pub Access"
    - Model Village: Requires "Pub Access" + "Model Village Access" (accessed through Pub)
    
    Victory Requirements:
    - "Complete the game" requires ALL 5 access items because the player must
      carry the golden bell back through every area to the starting pond.
    
    All items, drags, and goals in a region inherit that region's
    access requirement automatically.
    """
    
    multiworld = world.multiworld
    player = world.player
    
    # =========================================================================
    # REGION ENTRANCE RULES
    # These gate access to entire regions and all locations within them
    # =========================================================================
    
    # Garden requires Garden Access (even though it's the "first" area in vanilla)
    set_rule(
        multiworld.get_entrance("To Garden", player),
        lambda state: state.has("Garden Access", player)
    )
    
    # High Street requires High Street Access
    set_rule(
        multiworld.get_entrance("To High Street", player),
        lambda state: state.has("High Street Access", player)
    )
    
    # Back Gardens requires Back Gardens Access
    set_rule(
        multiworld.get_entrance("To Back Gardens", player),
        lambda state: state.has("Back Gardens Access", player)
    )
    
    # Pub requires Pub Access
    set_rule(
        multiworld.get_entrance("To Pub", player),
        lambda state: state.has("Pub Access", player)
    )
    
    # Model Village requires BOTH Pub Access (to get there) AND Model Village Access
    set_rule(
        multiworld.get_entrance("To Model Village", player),
        lambda state: (
            state.has("Pub Access", player) and
            state.has("Model Village Access", player)
        )
    )
    
    # =========================================================================
    # SPECIAL GOAL RULES
    # Some goals have additional requirements beyond just area access
    # =========================================================================
    
    # "Make the groundskeeper hammer his thumb" requires High Street Access
    # Due to a quirk in how the groundskeeper AI works, he only picks up
    # the hammer when the High Street gate is unlocked
    set_rule(
        multiworld.get_location("Make the groundskeeper hammer his thumb", player),
        lambda state: (
            state.has("Garden Access", player) and
            state.has("High Street Access", player)
        )
    )
    
    # "Steal the golden bell" just requires reaching Model Village
    # (already handled by region access)
    
    # "Complete the game" requires ALL access items because you must carry
    # the bell back through every area: Model Village -> Pub -> Back Gardens
    # -> High Street -> Garden -> Intro area
    set_rule(
        multiworld.get_location("Complete the game", player),
        lambda state: (
            state.has("Garden Access", player) and
            state.has("High Street Access", player) and
            state.has("Back Gardens Access", player) and
            state.has("Pub Access", player) and
            state.has("Model Village Access", player)
        )
    )
    
    # =========================================================================
    # EXTRA GOAL RULES (if enabled)
    # =========================================================================
    
    if world.options.include_extra_goals:
        # 100% completion requires beating the game first
        set_rule(
            multiworld.get_location("Complete all goals", player),
            lambda state: state.can_reach_location("Complete the game", player)
        )
        
        # "Collect the five flowers" - flowers are spread across multiple areas
        set_rule(
            multiworld.get_location("Collect the five flowers", player),
            lambda state: (
                state.can_reach_region("Garden", player) and
                state.can_reach_region("High Street", player) and
                state.can_reach_region("Back Gardens", player) and
                state.can_reach_region("Pub", player)
            )
        )
        
        # "Make someone from outside the high street buy back their own stuff"
        # (Gardener is from Garden, bring their stuff to shopkeeper)
        set_rule(
            multiworld.get_location("Make someone from outside the high street buy back their own stuff", player),
            lambda state: (
                state.can_reach_region("High Street", player) and
                state.can_reach_region("Garden", player)
            )
        )
        
        # "Dress up the bust with things from outside the back gardens"
        set_rule(
            multiworld.get_location("Dress up the bust with things from outside the back gardens", player),
            lambda state: (
                state.can_reach_region("Back Gardens", player) and
                state.can_reach_region("High Street", player) and
                (
                    state.can_reach_region("Garden", player) or
                    state.can_reach_region("Pub", player)
                )
            )
        )
        
        # "Perform at the pub wearing a ribbon" - ribbon is in Back Gardens
        set_rule(
            multiworld.get_location("Perform at the pub wearing a ribbon", player),
            lambda state: (
                state.can_reach_region("Pub", player) and
                state.can_reach_region("Back Gardens", player)
            )
        )
        
        # "Get thrown over the fence" - the box is in the hub/warp zone,
        # but you get thrown INTO the Pub area, so need both accesses
        set_rule(
            multiworld.get_location("Get thrown over the fence", player),
            lambda state: (
                state.can_reach_region("Back Gardens", player) and
                state.can_reach_region("Pub", player)
            )
        )
        
        # "Get caught by the man over the fence" - same situation
        set_rule(
            multiworld.get_location("Get caught by the man over the fence", player),
            lambda state: (
                state.can_reach_region("Back Gardens", player) and
                state.can_reach_region("Pub", player)
            )
        )
    
    
    # ITEM PICKUP RULES
    # Most items just need area access (handled by region).
    
    if world.options.include_item_pickups:
        set_rule(
            multiworld.get_location("Pick up Golden Bell", player),
            lambda state: state.can_reach_region("Model Village", player)
        )
    

    # DRAG ITEM RULES  
    # All drags just need their area access (handled by region).
    
