
# Usage (For Modders)

This section is an addition of Usage (For Users) going over points not covered there specifically for modders and testers.

## Creating Mod Metadata

Once a manifest is open, head to `Mods -> New` and write everything needed accordingly.

All metadata get stored in data_win32\pitcrewmetadata so if you close the application before applying any files to the mod, head to `Mods -> Import` and select the .mdata generated inside that folder.

### Editing Metadata
Simply right click on the mod, then choose `Edit Metadata`.


## Adding Files to the Mod
Once you select a mod, a grid for file paths and priorities will appear.

File paths are the relative path to the .fat/.dat pairs from the manifest's location.

Priorities are when a mod should be loaded in comparison to the rest of the files, the lower the number the higher the priority.

To add a file, have your .fat/.dat already in the appropriate folder inside your game (preferably in a folder called mods inside data_win32), then write in the file path the location without the extension. An example would be `mods/modfile1`. Once you finish editing, the priority should auto populate with 998. This guarantees your mod will load before any game files, to ensure it loads before other mods, set it lower.

If you set a file with a priority of 10, you are labelling it as a startup mod. Doing so will take any file inside of it and insert/replace them in startup.dat. 

> Tip: Try to name your modded .fat/.dat's something unique to avoid naming conflicts. Rather than gui_file, consider prepending the author name then the modId like AuthorModId_gui_file.

> Only pack the file pair with the files you modified. Avoid repacking files left untouched to save space and have as little conflicts as possible with other mods.

Be sure to save by clicking `Save`.

## Packaging Mods
Packaging a mod will get all the files needed and zipped up for sharing. Simply right click on the mod then choose `Package Mod`.

## Un/Packing Archives
Head to the menu bar, then Utilities -> Unpack Archive.

For unpacking, select your output folder and the .fat file of the archive you wish to unpack, then hit the button.

For packing, select your input folder and the location you want the .fat file to be saved. You can optionally add an author to the file or turn on/off compression.

## Merging Binaries
To see how to merge binaries, please check [Merging_Binaries](Merging_Binaries.md)