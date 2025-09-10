# Merging Binaries

This documentation is for being able to merge multiple Far Cry Binaries (.fcb, .bin, .bwo) into each other.

This is needed for The Crew especially since there can only be one of each file, and some binaries in particular house a lot of data that multiple mods may all need, hence the reason we need to be able to merge.

PitCrew offers a way to merge data into any binary found inside the global_db's of the game. Simply insert the specific XML files into your mod as a file, and PitCrew will do the work of merging the data inside the specific file needed to be edited.

## Specification

Each XML file you insert into PitCrew will need to follow a structure like this:

```xml
<root file="entity/generated/archetypes.entities.bin">
    <!--This is a comment, please ignore-->
    <add depth="root">
        <object hash="FFFFFFFF">
            <field hash="B7F8C275" type="BinHex">FFFFFFFFFFFFFFFF</field>
        </object>
        <object hash="FFFFFFFA">
            <field hash="B7F8C275" type="BinHex">FFFFFFFFFFFFFFFF</field>
        </object>
    </add>

    <edit modded="1" depth="25:0:1:5">
        <!--Empty field means delete since fields do not have depth id's-->
        <field hash="A733C3F9" type="BinHex"></field>
        <field hash="A733C3FA" type="BinHex">FF</field>
    </edit>

    <delete depth="25:0:1:5:0"></delete>
</root>
```

The root of the XML should have an attribute called file which tells PitCrew which file you wish to insert your data into. You can find these paths by extracting any of the global_db's and seeing their relative path inside the unpacked folder.

Setting the `file` attribute to `server` will merge into the binary file found within the Assets folder of PitCrew.

Setting the `file` attribute to `localization` will add your string to every language in the game. See more info [here](#localization-file).

From there, there are 3 commands PitCrew will listen to.

`add` - This will add any child objects of this command as child objects to the object specified in the depth.

`edit` - Add/Remove/Edit <ins>fields</ins> at the specified object (depth).

`delete` - Delete the specified object.

## Depth System

To be able to quickly merge multiple mods into these files, a depth system is needed to tell the compiler where to go exactly to add/edit/remove.

The depth system works as follows:

`root` means add/edit the root itself of the binary (depth of 0)

Specifying the depth with numbers has the root automatically truncated from the beginning. Meaning for a depth of `25:0:1:5`, the compiler will head to root(0) -> (26)th child object -> (1)st child object -> (2)nd child object -> (6)th child object and edit the specific fields. Each depth starts at 0 hence `25:0` is 26th child -> 1st child.

Depths are required for each command you add to the file.

## Modded tag

In the edit command, you'll see a modded tag of 1, not applying this gives a tag of 0.

If the tag is specified with 1, that particular command will attempt to traverse the binary it's trying to merge into with the intent that it's looking for modified depth values. You would only use this tag if you're attempting to mod something on top of another mod, such as an edit to an existing mod.

A tag of 0 will attempt to find the original depth value even if mods after it may have modified around the depth value. This is the default as most mods do not usually try to modify something else from another mod.

The modded tag can be applied to any command but is not required.

## Commands

### Add Command

Any child object of the command will be added to that depth. It does not support adding fields only, they would have to be enclosed inside an object node first.

### Edit Command

The edit command only supports fields and all of them have to be a child directly to the command.

If the hash does not exist at that depth, the compiler will add it.

If the hash exists but the value you set for it is empty, the compiler will remove it.

If the hash exists and your value isn't empty, the compiler will replace that value.

### Delete Command

The delete command will simply delete the specified depth along with any child objects. Deleting the root is not possible.

## Compiling
When compiling, whichever XMLs have the highest priority will merge first. So if you wish to edit an existing mod's data, you will need to set your XMLs priority lower.

## Localization File
If you want to add a string to every language globally, set the file attribute to `localization`. This will make a new string under the 99 localization bundle. From there, you will need to add to your mod a new file accounting for the table containing all strings.

An example is say you want the string `AB`, first you would make the localization file like so:
```xml
<root file="localization">
  <add depth="root">
    <object hash="29D6A3E8">
      <field hash="29D6A3E8" type="BinHex">41004200</field>
      <!-- Set the value below to a high number to avoid conflicts -->
      <field hash="BF396750" type="BinHex">45424D55</field>
    </object>
  </add>
</root>
```

Then you need a new file editing the global table:
```xml
<root file="localization/tat.localization.bin">
  <add depth="root">
    <object hash="29D6A3E8">
      <!-- Always set this to 63 (100) -->
      <field hash="4AF2B3F3" type="BinHex">63000000</field>
      <!-- Set this to the value you set above -->
      <field hash="BF396750" type="BinHex">45424D55</field>
    </object>
  </add>
  <edit depth="root">
    <!-- Always set this to 64 -->
    <field hash="1855B0EF" type="BinHex">64000000</field>
  </edit>
</root>
```

Add those two together in your mod to successfully add a string in-game.