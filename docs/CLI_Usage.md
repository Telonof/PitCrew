# CLI Usage

If you want to ignore the GUI and just add mods to the manifest manually and compile, this is for you.

## Manifest Structure

PitCrew relies on custom manifests to know what mods to apply and what priority each file in the mod uses. The manifests are forcefully located within the data_win32 folder of the game's location and look typically like this:

	##This has 2 pound symbols, so ignore.
	998 mods/modFile1 modGroupName
	#998 mods/modFile2

If the CLI detects a pound symbol on the left, it treats the mod as disabled and will ignore it.
>The GUI will need any lines with one pound symbol to still be formatted correctly, it only treats 2 pound symbols as comments.

`998`
The next is the priority. 998 should be the default but the lower it's set the higher that file's priority will be.

`mods/modFile1`
Next is the relative path of your file from the manifest, it will account for both .dat and .fat files of that name.

`modGroupName`
The last part is the optional mod group meant to organize files with their respective metadata in the GUI.

All of this can be edited in a text editor.

## Applying Manifest to Game
In the command line, run `PitCrewCompiler.exe <manifest file>`. If the manifest is valid, it will backup your startup.fat/.dat and modify it to apply any files. If it's invalid, the program will output why and exit.

