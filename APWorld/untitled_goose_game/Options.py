from dataclasses import dataclass
from Options import Toggle, Range, Choice, PerGameCommonOptions


class StartingArea(Choice):
    """Which area the player starts with access to. 
    Without this, you'd be stuck at the hub with nothing to do!
    Model Village is not an option since it's the finale area."""
    display_name = "Starting Area"
    option_garden = 0
    option_high_street = 1
    option_back_gardens = 2
    option_pub = 3
    option_random = 4
    default = 4  # Random by default


class IncludeExtraGoals(Toggle):
    """Include post-game extra challenge goals as locations."""
    display_name = "Include Extra Goals"
    default = False


class IncludeSpeedrunGoals(Toggle):
    """Include speedrun challenge goals (complete areas before noon)."""
    display_name = "Include Speedrun Goals"
    default = False


class IncludeItemPickups(Toggle):
    """Include first-time item pickups as locations (100+ additional checks)."""
    display_name = "Include Item Pickups"
    default = True


class IncludeDragItems(Toggle):
    """Include first-time drag item locations (50+ additional checks for heavy/draggable items)."""
    display_name = "Include Drag Items"
    default = True


class IncludeInteractions(Toggle):
    """Include interaction locations (ringing bells, spinning windmills, breaking boards, etc.)."""
    display_name = "Include Interactions"
    default = True


class TrapPercentage(Range):
    """Percentage of filler items that will be traps."""
    display_name = "Trap Percentage"
    range_start = 0
    range_end = 100
    default = 10


class Goal(Choice):
    """What is required to complete the game."""
    display_name = "Goal"
    option_steal_bell = 0
    option_all_main_goals = 1
    option_all_goals = 2
    default = 0


class DeathLink(Toggle):
    """When you get caught/shooed, everyone dies. When someone else dies, you get teleported to the hub."""
    display_name = "Death Link"
    default = False


@dataclass
class GooseGameOptions(PerGameCommonOptions):
    starting_area: StartingArea
    include_extra_goals: IncludeExtraGoals
    include_speedrun_goals: IncludeSpeedrunGoals
    include_item_pickups: IncludeItemPickups
    include_drag_items: IncludeDragItems
    include_interactions: IncludeInteractions
    trap_percentage: TrapPercentage
    goal: Goal
    death_link: DeathLink
