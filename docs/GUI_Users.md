# Usage (For Users)

If you don't plan on making your own mods and just want to use other mods, this section is for you.

## Opening/Creating Manifests

PitCrew relies on custom manifests to know what mods to apply to the game and what priority each file in the mod uses. 

If this is your first time, boot up PitCrew.exe and head to `Manifest -> Create` and select the exe of your game. 
(This is knowing your game has a data_win32 folder where the manifest will be stored.)

To then open the manifest on next boot, head to `Manifest -> Open` and select the .txt created inside data_win32 (should be named pitcrewmanifest.txt)

## Importing Mods
The GUI will then prompt you to double click on the box or click `Mod -> Import` on the menu bar to get prompted for a .mdata file.

A typical mod should like this in a directory:

- ModId.mdata
- modfile1.dat
- modfile.fat

Each mod should contain an .mdata as well as one or multiple .dat/.fat pairs. Select the .mdata to import it into the GUI.

## Editing Mods
The mod provided should have everything configured and ready to go for you. If you want to disable/enable certain mods on the next boot of your game, click the checkbox to the right of each mod.

If you want to delete a mod, right click on the mod then choose `Delete Mod`.

## Applying mods
Once all your mods are setup, click the `Compile` button to apply all files in the mods to your game.

## Conflicts

Some mods may notify you below their file list what they conflict with. These will load together, but whichever mod has lower priorities will run first and ignore some or all files from the other mods on the list.
