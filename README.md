# Untitled Goose Game - Archipelago Randomizer

A multiworld randomizer client for Untitled Goose Game using the Archipelago framework.

## Overview

This mod turns Untitled Goose Game into an Archipelago multiworld game where:
- **Completing goals** sends checks to the multiworld
- **Area unlocks** are now items you receive from the multiworld
- **Play with friends** in other games - your goose goals and locations might unlock their items!

## Installation

### Prerequisites
1. **Untitled Goose Game** (Steam)
2. **BepInEx 5.4.9.0 x64** - Unity mod loader

### Installing BepInEx
1. Download https://github.com/BepInEx/BepInEx/releases/tag/v5.4.9 x64
2. Extract the contents to your game folder (`Untitled Goose Game/`)
3. Run the game once to generate necesarry files and folder for Bepin
4. Close the game

### Installing the AP Mod and World
1. Download the latest release files from the Releases page
2. Copy `GooseGameAP.dll` to `BepInEx/plugins/`
3. Copy `APProxy.exe` to `BepInEx/plugins/` 
4. Copy `APProxy.runtimeconfig.json` to `BepInEx/plugins/` 
5. Copy `APProxy.dll` to `BepInEx/plugins/`
6. Double-click the APWorld file or drag it into custom worlds folder to install. 


## Usage

### Connecting to Archipelago
1. Start the game
2. Press `F1` to open the AP connection menu
3. Enter:
   - **Server**: Your AP server address (e.g., `archipelago.gg:12345`)
   - **Slot Name**: Your player name
   - **Password**: (if required)
4. Click Connect

### Gameplay Changes

#### Area Progression
In vanilla Goose Game, areas unlock automatically as you progress. With AP:
- You start with 1 **Random Area** unlocked or whatever was chosen in the yaml
- If you try to enter an area that you do not have access to you will be warped to the Hub area (Well)
- **High Street**, **Gardens**, **Back Gardens**, **Pub**, and **Model Village** require AP items
- Complete goals to send checks and receive items from the multiworld

#### Goals = Locations
Every goal on your to-do list is a location check:
- Complete "Rake in the lake" → Send check → Maybe unlock someone else's item!
- Someone completes their check → You might receive "High Street Access"!

## Location List (282 checks)
Actually too many to list now.

## Item List

### Progression Items
- Garden Access 
- High Street Access
- Back Gardens Access
- Pub Access
- Model Village Access

-NPC Souls
-Object Souls

### upgrades
- Mega Honk (Ugradable honk with final level forcing NPCs to drop their held items)
- Speedy Feet (faster movement up to a total of 150%)
- Goose Day (Force NPCs to have their neutral behaviour for 15s)

### Trap Items
- Tired Goose (slower movement)
- Suspicious Goose (Honk randomly for 10s)
- Confused feet (Random Stick directions for 15s)
- Butterbeak (drop items and be unable to grab anything for 10s)

## YAML Options

```yaml
Untitled Goose Game:
  # Include extra/post-game goals?
  include_extra_goals: false
  
  # Include speedrun challenge goals?
  include_speedrun_goals: false
  
  # Trap item percentage (0-100)
  trap_percentage: 10
  
  # Choose Starting area
```


## Credits
- House House - Untitled Goose Game
- Archipelago Team - Multiworld framework
- BepInEx Team - Unity mod loader

## License
MIT License - See LICENSE file

---

*Honk!* 🪿
