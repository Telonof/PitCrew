# PitCrew

PitCrew is a simple mod loader for the game "The Crew" written in C#. 

# What does it do?

PitCrew is designed to handle all custom mods that will be applied to the game by automatically editing the necessary files to let the game load the mod. It comes in two forms.

>PitCrew is the main GUI application, allowing you to create, edit, and package mods with a conflict manager letting you know what mods will be incompatible with each other.

>PitCrewCompiler is a standalone CLI application that PitCrew relies on to compile. This reads a basic manifest file generated either manually or by the GUI and applies the data to the file it needs to.

# Usage
For GUI Users: Check [GUI_Users](docs/GUI_Users.md)

For GUI Mod Creators: Check [GUI_Modders](docs/GUI_Modders.md)

For CLI Usage: Check [here](docs/CLI_Usage.md)
