# CLI Usage

If you want to ignore the GUI and just add mods to the manifest manually and compile, this is for you.

## Manifest Structure

PitCrew relies on custom manifests to know what mods to apply and what priority each file in the mod uses. The manifests are forcefully located within the data_win32 folder of the game's location and look typically like this:

```xml
<instance packageversion="5">
  <mod id="id" enabled="false">
    <file priority="998" loc="mods/modFile1" />
  </mod>
</instance>
```

The root node should contain a `packageversion` containing what game the manifest is for. If it's not set it will default to 5.

5 = The Crew

6 = The Crew 2

<br></br>

Each child object should be a `mod` node. The `id` attribute is required and is used to hook into the metadata and to serve as a name for messages.

The `enabled` attribute is not required and will default to true, setting it to false will skip that mod from compiling.

Each mod node should have `file` child nodes with each one having a priority and loc attribute.

For every file with a priority of 998 the priority will automatically be determined by the mod order. Any mod on top will overwrite any mod that modifies something similar below it.

`priority=998`
A file's priority determines when it should load, the lower it is the earlier it will load. 998 should be the default for most files unless the file in question is trying to mod another file with priority 998.

`loc=mods/modFile1`
The loc is the relative path of your file from the manifest, it will account for both .dat and .fat files of that name if given without an extension, adding .xml means the file is a binary needing to be merged, see more about that [here](Merging_Binaries.md).

## Groups
> [!NOTE]
> If working with the CLI you do not need to work with the group system, this section is purely for documentation if you come across groups in the manifest.

Groups will encapsulate mods and is mainly used for the GUI. They will look like so:
```xml
<group name="Group name" gui-opened="false">
	<!--mod data in here-->
</group>
```

`name="Group name"`
name simply serves as the name seen for the group in the GUI and can be anything.

`gui-opened="false"`
This attribute tells the GUI whether on load should the group be collapsed or expanded. `False` means the group should stay collapsed on load and `True` means the group should be expanded on load.

## Applying Manifest to Game
In the command line, run `PitCrewCompiler.exe <manifest file>`. If the manifest is valid, it will backup your startup.fat/.dat and modify it to apply any files. If it's invalid, the program will output why and exit.
