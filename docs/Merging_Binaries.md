# Merging Binaries

This documentation is for being able to merge multiple Far Cry Binaries (.fcb, .bin, .bwo) into each other.

This is needed for The Crew especially since there can only be one of each file, and some binaries in particular house a lot of data that multiple mods may all need, hence the reason we need to be able to merge.

PitCrew offers a way to merge data into any binary found inside the global_db's of the game, simply insert the specific xml files into your mod as a file, and PitCrew will do the work of merging the data inside the specific file needed to be edited.

## Specficiation

Each xml file you insert into PitCrew will need to follow a structure like this:
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

The root of the xml should have an attribute called file which tells PitCrew which file you wish to insert your data into, you can find these paths by extracting any of the global_db's and seeing their relative path inside the unpacked folder.

From there, there is 3 commands PitCrew will listen to.

`add` - This will add any child objects of this command as child objects to the object specified in the depth.

`edit` - Add/Remove/Edit <ins>fields</ins> at the specified object (depth).

`delete` - Delete the specified object.

## Depth System
To be able to quickly merge multiple mod's into these files, a depth system is needed to tell the compiler where to go exactly to add/edit/remove.

The depth system works as follows:

`root` means add/edit the root itself (depth of 0)

Specifing the depth with numbers has the root automatically truncuated from the begin. Meaning for a depth of `25:0:1:5`, the compiler will head to root(0) -> (26)th child object -> (1)st child object -> (2)nd child object -> (6)th child object and edit the specific fields.

Depth's are required for each command you add to the file.

## Modded tag
In the edit command, you'll have seen a modded tag of 1, not applying this gives a tag of 0.

If the tag is specified with 1, that particular command will attempt to traverse the binary it's trying to merge into with the intent that it's looking for modified depth values. You would only use this tag if you're attemping to mod something on top of another mod, such as an edit to an existing mod.

A tag of 0 will attempt to find the original depth value even if mods after it may have modified around the depth value. This is the default as most mods are not usually trying to modify something else from another mod.

The modded tag can be applied to any command but is not required.

## Commands

### Add Command
Any child object of the command will be added to that depth. It does not support adding fields only, they would have to be enclosed inside an object node first.

### Edit Command
The edit command only supports fields and all have to be a child directly to the command.

If the hash does not exist at that depth, the compiler will add it.
If the hash exists but the value is empty, the compiler will remove it.
If the hash exists with a value, the compiler will replace that value.

### Delete Command
The delete command will simply delete the specified depth along with any child objects. Deleting the root is not possible.