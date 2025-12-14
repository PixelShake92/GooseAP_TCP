from typing import Dict, NamedTuple, Optional
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
    
    # Filler Items
    "Mega Honk": GooseGameItemData(BASE_ID + 200, ItemClassification.filler),
    "Speedy Feet": GooseGameItemData(BASE_ID + 201, ItemClassification.filler),
    "Silent Steps": GooseGameItemData(BASE_ID + 202, ItemClassification.filler),
    "A Goose Day": GooseGameItemData(BASE_ID + 203, ItemClassification.useful),
    
    # Trap Items
    "Tired Goose": GooseGameItemData(BASE_ID + 300, ItemClassification.trap),
    "Confused Feet": GooseGameItemData(BASE_ID + 301, ItemClassification.trap),
    "Butterbeak": GooseGameItemData(BASE_ID + 302, ItemClassification.trap),
    "Suspicious Goose": GooseGameItemData(BASE_ID + 303, ItemClassification.trap),
    
    # Victory
    "Golden Bell": GooseGameItemData(BASE_ID + 999, ItemClassification.progression),
}

ITEM_GROUPS = {
    "Area Unlocks": {
        "Garden Access",
        "High Street Access", 
        "Back Gardens Access",
        "Pub Access",
        "Model Village Access",
    },
    "Fillers": {
        "Mega Honk",
        "Speedy Feet",
        "Silent Steps",
        "A Goose Day",
    },
    "Traps": {
        "Tired Goose",
        "Confused Feet",
        "Butterbeak",
        "Suspicious Goose",
    }
}
