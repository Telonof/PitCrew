# Usage (For Users)

If you don't plan on making your own mods and just want to use other mods, this section is for you.

## Opening/Creating Manifests

PitCrew relies on custom manifests to know what mods to apply to the game and what priority each file in the mod uses. 

If this is your first time, boot up PitCrew.exe and head to `Instances` then click Add or drag and drop the exe of your game.
(This is knowing your game has a data_win32 folder where the manifest will be stored.)

To then open the manifest on next boot, head to `Instances` and double click the version of the game.
(The program will auto open whatever last instance you were working on.)

## Importing Mods
To import mods, either drag and drop an .mdata/.zip, or use `Mod -> Import` in the menubar.

A typical mod should look like this in a directory or zip:

- ModId.mdata
- modfile1.dat
- modfile1.fat

Each mod should contain an .mdata as well as one or multiple .dat/.fat pairs/.xml files. Select the the unpackaged or packaged mod to import it into the GUI.

## Editing Mods
The mod provided should have everything configured and ready to go for you. If you want to disable/enable certain mods on the next boot of your game, click the checkbox to the right of each mod.

By clicking the arrows you change the priority order of how the mods will load. So mods at the top of the list will load before mods below them.

If you want to delete a mod, right click on the mod then choose `Delete Mod`.

## Applying mods
Once all your mods are setup, click the `Compile` button to apply all files in the mods to your game.

## Conflicts

Some mods may notify you below their file list what they conflict with. These will load together, but whichever mod has lower priorities will run first and ignore some or all files from the other mods on the list.
