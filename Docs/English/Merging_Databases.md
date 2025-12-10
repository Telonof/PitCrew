# Merging Babel Databases

These databases contain information regarding valid vinyls on cars, models, colors etc.

Similarly to Far Cry Binaries, there can only be one of each file, which can lead to two mods wanting to add an item into the vinyl database for example incompatible with each other.

PitCrew offers a way to merge data into any Babel Database (known as .babdb's) found inside the global_db's or scenaric's of the game. Simply insert the specific XML files into your mod as a file, and PitCrew will do the work of merging the data inside the specific file needed to be edited.

## Specification

Each XML file you insert into PitCrew will need to follow a structure like this:

```xml
<root file="road66database/sticker.babdb">
    <!--This is a comment, please ignore-->
    <add>
      <id>4654495753743100</id>
      <type>78429DB7</type>
      <modelid>FFFFFFFFFFFFFFFF</modelid>
      <packindex type="UInt32">32</packindex>
      <typeid>4573840700000000</typeid>
    </add>

    <edit index="0">
      <packindex type="UInt32">32</packindex>
    </edit>
</root>
```

The root of the XML should have an attribute called file, which tells PitCrew which file you wish to insert your data into. You can find these paths by extracting any of the global_db's and seeing their relative path inside the unpacked folder.


> [!NOTE]  
> To see exactly the column names and rows in a human-readable way, consider compiling [these tools](https://github.com/Telonof/Gibbed.Dunia2) and using the `Dunia2.ConvertBabelDB` program to read these babdb files in the CSV format.


From there, there are 2 commands PitCrew will listen to.

`add` - This will add a row to the database.

`edit` - This will allow editing of columns inside the row specified. The `index` attribute starts at 0.

## Attributes
In the example, this line was shown: `<packindex type="UInt32">32</packindex>`

By setting a `type="<type>"` as an attribute to the row, you can specify a certain type of value to be parsed rather than having to put down an entire hash. You can use any format supported [here](https://github.com/Telonof/Gibbed.Dunia2/blob/master/Gibbed.Dunia2.BinaryObjectInfo/FieldType.cs).


## Commands

### Add Command

This will add a row to the specified database. Each node inside the add command represents a column. The column names do <ins>not</ins> need to be specified, so long as they are in the correct order and the column count matches that of the database.

### Edit Command

The edit command will allow editing a row's data.

The index attribute is required and tells the merger which row to edit.

The order in which you edit the columns in the row does <ins>not</ins> matter, so long as the name <ins>matches</ins> the column's name (case can be insensitive).

## Compiling
Save the file as a `.xml` file and when referencing the file in your mod, ensure the extension is included.

When compiling, whichever XMLs have the highest priority will merge first. So if you wish to edit an existing mod's data, you will need to set your XMLs priority lower.