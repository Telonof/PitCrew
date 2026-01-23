# PitCrew

PitCrew is a simple mod loader for the game "The Crew" and "The Crew 2" written in C#. 

Requires .NET Runtime 8.X found [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

Uses the Avalonia UI Framework.

# What does it do?

PitCrew is designed to handle all custom mods that will be applied to the game by automatically editing the necessary files to let the game load the mod. It comes in two forms.

### PitCrew
PitCrew is the main GUI application, allowing you to create, edit, and package mods with a conflict manager letting you know what mods will be incompatible with each other.

### PitCrewCompiler
PitCrewCompiler is a standalone CLI application that PitCrew relies on to compile. This reads a basic manifest file generated either manually or by the GUI and applies the data to the file it needs to.

# Usage
For GUI Users: Check [GUI_Users](Docs/English/GUI_Users.md)

For GUI Mod Creators: Check [GUI_Modders](Docs/English/GUI_Modders.md)

For CLI Usage: Check [here](Docs/English/CLI_Usage.md)

# PitCrew on Linux for The Crew 2
Due to the game's proprietary use of the Oodle compressor, PitCrew cannot work on Linux natively for users wanting to use it for The Crew 2. If Epic Games allows non-commerical distribution of their oodle binaries, then this may change in the future.
