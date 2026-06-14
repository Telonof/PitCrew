# Usage (For Users)

If you don't plan on making your own mods and just want to use other mods, this section is for you.

## Opening/Creating Manifests

If loading PitCrew for the first time, boot up PitCrew.exe and head to `Instances` then click Add or drag and drop the exe of your game onto the box.

(This is knowing your game has a data_win32 folder where the manifest will be stored.)

Provide a custom name for the instance, then double click it to load it. You should see a blue outlined box if you loaded the instance correctly.

To switch instances, head to `Instances` and double click on the copy of the game you wish to switch to. The program will auto open whatever last instance you were working on.

## Importing Mods
To import mods, either drag and drop an .mdata/.zip, or use `Mod -> Import` in the menubar.

If downloading mods from ModWorkshop, you can select `Install with PitCrew` to auto import it.

A typical mod should look like this in a directory or zip:

- ModId.mdata
- modfile1.dat
- modfile1.fat

Each mod should contain an .mdata as well as one or multiple .dat/.fat pairs/.xml files. Select the the unpackaged or packaged mod to import it into the GUI.

By default the mod will be put inside a `Default` group. If you select a group before importing, it will import into that group.

## Editing Mods
The mod provided should have everything configured and ready to go for you. If you want to disable/enable certain mods on the next boot of your game, click the checkbox to the right of each mod.

By selecting a mod and clicking/holding down the arrows on the right, you can change the priority order of how the mods will load. So mods at the top of the list will be prioritized over mods below them. 

If you want to delete a mod, right click on the mod then choose `Delete Mod`.

## Groups
Groups let you organize mods into custom categories to allow for better navigation through the mod list.

To make a group, either right click on an existing group, an empty part of the mod list, or by selecting `Mods -> New Group` in the menubar.

Give your group a custom name, then press OK. To rename a group, right click on the group and select `Rename Group`.

To delete a group, right click on the group and select `Delete Group`. All mods will be moved to a default group and will not be deleted. If you wish to delete the mods themselves, select the mods themselves then select `Delete Mod`.

If the button on the right of the group is green, pressing it will disable every mod in that group and turn red. If pressing the button when red, all mods will be enabled in that group and turn green.

To move mods into different groups, right click on all the mods to be moved (you can multi-select by holding CTRL or SHIFT), then select `Change Group`. In the new menu you can create groups, delete groups, or select any group to move the mods into.

## Applying mods
Once all your mods are setup, click the `Compile` button to apply all files in the mods to your game.

## Conflicts
Some mods may notify you below their file list what they conflict with. These will load together, but whichever mod has lower priorities will run first and ignore some or all files from the other mods on the list.
