# Merging Binaries

This documentation is for being able to merge multiple Far Cry Binaries (.fcb, .bin, .bwo) into each other.

This is needed for The Crew since there can only be one of each file, and some binaries in particular house a lot of data that multiple mods may all need, hence the reason we need to be able to merge.

PitCrew offers a way to merge data into any binary found inside the global_db/scenaric/localization archives found in the game. By creating a specific XML file and linking it to your mod, PitCrew will do the work of merging the data inside that XML file into the .bin files found within the game.

## Specification

Each XML file you insert into PitCrew will need to follow a structure like this:

```xml
<root file="entity/generated/archetypes.entities.bin">
    <!--This is a comment, please ignore-->
    <variables>
	    <mass type="Int32">5</mass>
    </variables>
    
    <add depth="root">
        <object hash="FFFFFFFF">
            <field hash="B7F8C275" type="BinHex">FFFFFFFFFFFFFFFF</field>
            <object name="Example1">
	            <field name="Float1" type="Float32">5</field>
            </object>
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

    <edit id="0x22CD940700000000">
        <object depth="0">
            <object name="Mass">
                <field name="Mass" type="Variable">mass</field>
            </object>
        </object>
    </edit>
	
    <editall depth="root" required_mods="ExampleMod1 ExampleMod2" blacklisted_mods="ExampleMod3">
        <field hash="A733C3FA" type="BinHex">FFFFFFFF</field>
    </editall>

    <delete depth="25:0:1:5:0"></delete>
    
    <delete id="0xE326840700000000">
        <object name="Emitter" />
        <object name="Generic">
            <object depth="1" />
        </object>
    </delete>
</root>
```

The root of the XML should have an attribute called file which tells PitCrew which file you wish to insert your data into. You can find these paths by extracting any of the archives mentioned above and seeing their relative path inside the unpacked folder.

Setting the `file` attribute to `server` will merge into the binary file found within the Assets folder of PitCrew (only applies for The Crew 1).

Setting the `file` attribute to `localization` will add your string to every language in the game. See more info [here](#localization-file).

From there, there are 4 commands PitCrew will listen to.

`add` - Add any child objects of this command as child objects to the object specified.

`edit` - Add/Remove/Edit <ins>fields</ins> at the specified object or child objects specified.

`editall` - Edits/Deletes all specific <ins>fields</ins> located in the object and any child object.

`delete` - Delete the specified object.


## Entrypoint
Each command needs an entrypoint to tell the compiler where exactly the data it stores should go. There are two ways to define an entrypoint.

`id` - Tells the compiler to go to the object that has an ID field associated with it.
`depth` - Specifically set the exact XML depth the compiler should head to.

Both can be used simultaneously.

### ID System
Shown in the example above (`<edit id="0x22CD940700000000">`), by setting a specific ID, you are able to tell the compiler to go to and run the command (edit in this case) at the object (depth) that has an ID field associated with it. Those objects typically look like:
```xml
	<object name="Entity">
		<field name="ID" type="BinHex">22CD940700000000</field>
		<!--<field name="ID" type="Int64">127192354</field>-->
		<!--Actual data here-->
	</object>
```
By putting a 0x before the id, you tell the compiler you are giving it a hexadecimal integer of length 16. Without the 0x, you tell it you are giving it an Int64.

The ID system can be combined with the depth system. By default, the compiler will add a `depth="root"` to your command, but this can be overridden with any specific depth. If overridden, the compiler will first go to the ID's depth, then the sub-depth specified. An example is by setting depth to `0`, the compiler will go to the first child object of that ID.

In an example, by using an add command with an ID, you ask the compiler to add whatever data you requested to be added inside the object with the id associated with it. A delete command tells the compiler to delete the object that has that ID associated with it.

### Depth System

The depth system tells the compiler the exact depth within the XML tree of the game binary to edit. This can be useful to tell the compiler precise locations to edit variables, especially if the location in question does not have a ID associated with it.

The depth system works as follows:

`root` means add/edit the root itself of the binary or root of the object with an ID if one is specified (depth of 0).

Specifying the depth with numbers has the root automatically truncated from the beginning. Meaning for a depth of `25:0:1:5`, the compiler will head to root(0) -> (26)th child object -> (1)st child object -> (2)nd child object -> (6)th child object and edit the specific fields. Each depth starts at 0 hence `25:0` is 26th child -> 1st child.

Depths are required for each command you add to the file. Exclusions are if an ID is set, where a `root` depth will be invisibly set alongside it if none is provided.

## Attributes
### Modded Tag

In the edit command, you'll see a modded tag of 1, not applying this gives a tag of 0.

If the tag is specified with 1, that particular command will attempt to traverse the binary it's trying to merge into with the intent that it's looking for modified depth values. You would only use this tag if you're attempting to mod something on top of another mod, such as an edit to an existing mod.

A tag of 0 will attempt to find the original depth value even if mods after it may have modified around the depth value. This is the default as most mods do not usually try to modify something else from another mod.

The modded tag can be applied to any command but is not required.

### Required Mods Tag
In the editall command example, there is a required_mods tag. When applied, the command will only work if all mod id's are being loaded on compile. Each mod is separated by a space, so in the example above ExampleMod1 and ExampleMod2 both have to be enabled on compile for that command to function.

> [!INFO]
> To find mod id's, right click any mod in the GUI and select "Edit Metadata" to see the grayed out mod id at the top.<br>For CLI, head into the manifest file and look for the id attribute associated with each mod.

### Blacklisted Mods Tag
Additionally, mods can also be blacklisted. In the editall command example there is a blacklisted_mods tag. When applied, if any mod id's are to be compiled that is on the list, the command will refuse to execute. Using the example above, if a user has ExampleMod3 enabled and ready to compile that specific command will not execute.

## Variable System
At the top of the file, you can define custom variables that can be used inside fields in replacement of hardcoded values. These variables will hook into the GUI allowing users to edit them. If you don't wish for a variable to be exposed through the GUI, hardcode it.

To utilize this system, first add a \<variables> element to the top of the root, then set the name of the variables (case-insensitive) as the name of the variable element. The value you want editable will be set as the value of the variable.

Optionally, you can add a `type` attribute to specify a type of value. By default it will be set to `BinHex`, but can be set to any in [this list](https://github.com/Telonof/Gibbed.Dunia2/blob/master/Gibbed.Dunia2.BinaryObjectInfo%2FFieldType.cs).

> The example above shows `mass` being set to `5` with a type of `Int32`.

Then, to utilize the defined variables, change the `type` attribute of the field you want the variable in to `Variable`, then replace the value of the field with the variable name you set. The example above shows an edit command being given the mass variable.

## Commands

### Add Command

Any child object of the command will be added to that depth. It does not support adding fields only, they would have to be enclosed inside an object node first.

### Edit Command

The edit command allows you to modify fields at the depth specified as well as edit fields to any child object.
 
 For all fields, the following rules will apply:

- If the hash/name does not exist at that depth, the compiler will add it.

- If the hash/name exists but the value you set for it is empty, the compiler will remove it.

- If the hash/name exists and your value isn't empty, the compiler will replace that value.

For any sub-object set within the edit command, the following rules will apply:

- Any depth specified can only contain one number. `0:1` would be invalid.

- If the object has a `depth` attribute but no name/hash, the compiler will go to the child found at the specified depth in the edit command. An example is if `depth="1"` is set and the depth of the edit command is set to `4159:0`, then the edit command will go to `4159:0:1`. 

- If the object has a name/hash attribute but no depth attribute, the compiler will silently add a depth of 0 to the object.

- If the object has a name/hash attribute and a depth attribute, the compiler will go to the occurrence specified by the depth and name. An example is if name is set to `Test` and depth is set to `0`, the compiler will go to the first occurrence of `Test` if found.

- For any fields located in any object, the rules for fields above will apply.

### Edit All Command

The editall command applies most field rules from the edit command to the specified depth and every child object that can be found by the compiler at that depth. The only rule ignored is the first one.

editall will **only** allow editing/deleting fields. It also does not allow any child objects inside the command and will only accept fields.

### Delete Command

The delete command will delete the last object in each object chain.

If a delete command is made with no child objects, the compiler will simply delete the depth/id you specified.

By making sub-objects similar to edit commands, it will instead go through each child object until no more are found and delete whatever is last in the chain.

An example of this is the 2nd delete command listed above. For that command, the emitter object will be deleted inside the object specified via the ID, as well as the 2nd object found inside Generic.

The rules for sub-objects are identical to edit command sub-objects aside from field rules being applied.

## Compiling
Save the file as a .xml file. Then add the file with the extension to PitCrew to allow it to register.

When compiling, mods are loaded from bottom to top, meaning if your XML is specifically trying to change/override data of an existing modded XML, the mod associated with the XML would need to be set above it.

## Localization File
If you want to add a string to every language globally, set the file attribute to `localization`. This will make a new string under the 13th localization bundle. From there, you will need to add to your mod a new file accounting for the table containing all strings.

An example is say you want the string `AB`, first you would make the localization file like so:
```xml
<root file="localization">
  <add depth="root">
    <object name="_">
      <field name="_" type="String16">AB</field>
      <!-- Set the value below to a high number to avoid conflicts -->
      <field name="id" type="Int32">80000</field>
    </object>
  </add>
</root>
```

Then you need a new file editing the global table:
```xml
<root file="localization/tat.localization.bin">
  <add depth="root">
    <object name="_">
      <!-- Always set this to 0D (13) -->
      <field name="bid" type="Int32">13</field>
      <!-- Set this to the value you set above -->
      <field name="id" type="Int32">80000</field>
    </object>
  </add>
</root>
```

Add those two together in your mod to successfully add a string in-game.
