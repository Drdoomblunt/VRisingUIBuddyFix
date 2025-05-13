# UI Buddy mod for V Rising
![image](https://github.com/panthernet/VRisingUIBuddy/blob/main/logo.png?raw=true)

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Guide](#guide)
- [In case of Emergency](#emergency)
- [Credits](#credits)
- [ChangeLog](CHANGELOG.md)

## Sponsor this project

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/panthernet)

## Features

Buddy UI allow advanced UI elements customization for V Rising. Currently it works with Mouse+Keyboard mode only. It is represented by two control windows. 
 - Select different UI elements and adjust their visibility, position, scale and rotation
 - Supports: Main Game Menu, Character HUD in game
 - UI layout is saved and loaded automatically so your custom layout persist through game launches




### Current Version Warnings

 - Individual skill buttons modififcation is still WIP
 - Some elements has (?) prefix that means it is likely impossible to modify this element while it is hidden, like notification or tutorial popup. Needs further investigation.
 - Some elements has (!!) prefix that means they can have some weird offset between control panel and actual element placement on the screen

## Installation

 - Install [BepInExRC2](https://github.com/decaprime/VRising-Modding/releases/tag/1.733.2)
 - Download latest *.zip file form Releases
 - Unpack `BepInEx` folder from zip archive and replace `BepInEx` folder in VRising game root folder
 - Run the game, use default `PageDown` key to show/hide control panels

## Guide

The main windows allows you to configure general behavior options for the mod and also displays information about selected UI element, providing means to control its position and state. Second window displays a list of configurable UI elements available for modification on the current screen. Configurable UI elements or PANELS are marked with grayed out overlay rectangles.
![image](https://github.com/user-attachments/assets/1b38f11a-304a-47c3-b5bd-1f46e082bcc9)

You can select element by clicking a list item or clicking the overlay panel if `Select panels with mouse` option is enabled on the main panel. Once PANEL is selected it will display yellow outline and also will be marked in a list. Main panel will display the `Name` of currently selected panel and available control elements will apply modifications to the selected PANEL. When panel has an ouline you can click and drag it to a new position. You can also change PANEL `Visibility` by clicking toggle button in the list on the left of the desired panel name.
![image](https://github.com/user-attachments/assets/634c3b20-e371-4f44-9fb9-562cf0a52cbd)

Main panel provide input controls to change selected panel scale and rotation values. It also provide following options:

 - `Show Panels` - will show/hide overlay panels for UI elements
 - `Select panels with mouse` - will enable/disable the ability to select panels with mouse which can come in handy in some cases when several panels occupy one space.
 - `Reload elements` button - will try to reload all elements and fill the list with the missing ones. It is a rare case but still happens, for example, on new character start, when some UI elements will become available a bit later.

![image](https://github.com/user-attachments/assets/751164ce-3b27-4510-8321-cda016405a55)

Some list element names might start with (?) or (!!) symbols which signify that these elements must be treated carefully.

 - (?) - element is invisible by default so it is hard or impossible to change its position unless it is shown. This often relates to some notifications.
 - (!!) - element is available for modification but has some weird UI placement or overlay position which hasn't been fixed yet. 

![image](https://github.com/user-attachments/assets/f5ca61c7-6deb-4ece-8b38-d994a66b9813)

Some controls like `Chat Window` have quirky structure so we use `Detached` panels to control their position. Detached panels are rendered as a separate gray panels that are always visible and can be placed anywhere, moving this panel will move corresponding UI element even if it is currently invisible.
![image](https://github.com/user-attachments/assets/566f659d-5527-4c11-a2ec-51e3041c8b89)

## Emergency

OMG! UI IS MESSED UP! WHAT DO?!!!
Don't worry. In case you can't fix the UI using the provided tools you can always delete config file located at `V Rising\BepInEx\config\panthernet.UIBuddy.cfg`. Or you can edit/remove individual panel settings from `Panels` config section if you confident in what you're doing.

## Credits

- [Bloodstone](https://github.com/decaprime/Bloodstone) keybinds work logic
- [Retro Camera](https://github.com/mfoltz/RetroCamera) enhanced and working keybinds update
- [XP Rising](https://github.com/aontas/XPRising) enhanced UI management base
- [V Rising Mod Community](https://discord.gg/vrisingmods) invaluable help on digging into the code
