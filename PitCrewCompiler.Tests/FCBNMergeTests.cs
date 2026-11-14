using System.Reflection;
using Gibbed.Dunia2.FileFormats;
using PitCrewCommon.Models;
using PitCrewCommon.Utilities;

namespace PitCrewCompiler.Tests;

/** The BinaryObjectMerger for Far Cry binaries has gotten packed with so many systems that I felt I needed to make a few automated tests
 * to ensure when I do major changes to the merger that I don't royally fuck up 100+ player mods at once.
 */
public class FCBNMergeTests
{

    private readonly string _gamePath = "/mnt/E/Games/The Crew (Worldwide)/data_win32";
    private string _testFolder;
    private BinaryObject objRoot;
    
    
    [OneTimeSetUp]
    public void Setup()
    {
        _testFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "data_win32");
        Directory.CreateDirectory(Path.Combine(_testFolder, "mods"));
        Directory.CreateDirectory(Path.Combine(_testFolder, "pc_ww"));
        
        File.Copy(Path.Combine(_gamePath, "cstartup.fat"), Path.Combine(_testFolder, "startup.fat"));
        File.Copy(Path.Combine(_gamePath, "cstartup.dat"), Path.Combine(_testFolder, "startup.dat"));
        File.Copy(Path.Combine(_gamePath, "global_db_patch_1.fat"), Path.Combine(_testFolder, "global_db_patch_1.fat"));
        File.Copy(Path.Combine(_gamePath, "global_db_patch_1.dat"), Path.Combine(_testFolder, "global_db_patch_1.dat"));
        File.Copy(Path.Combine(_gamePath, "shadersobj.fat"), Path.Combine(_testFolder, "shadersobj.fat"));
        
        CopyEmbedToFolder("PitCrewCompiler.Tests.Assets.manifesttest.xml", Path.Combine(_testFolder, "manifest.xml"));
        CopyEmbedToFolder("PitCrewCompiler.Tests.Assets.binarytest.xml", Path.Combine(_testFolder, "mods", "binarytest.xml"));
        CopyEmbedToFolder("PitCrewCompiler.Tests.Assets.lowpritest.xml", Path.Combine(_testFolder, "mods", "lowpritest.xml"));
        
        Instance instance = new Instance(Path.Combine(_testFolder, "manifest.xml"));
        instance.LoadFromXML(true);
        Compile compile = new Compile(instance);
        compile.Run().Wait();
        
        BigFileUtil.UnpackBigFile(Path.Combine(_testFolder, "mods", "PitCrewBase.fat"), Path.Combine(_testFolder, "base_unpack"), 5);
        BinaryObjectFile bof = new BinaryObjectFile();
        Stream archetypes = new FileStream(Path.Combine(_testFolder, "base_unpack", "entity", "generated", "archetypes.entities.bin"), FileMode.Open);
        bof.Deserialize(archetypes);
        archetypes.Dispose();
        objRoot = bof.Root;
    }

    [OneTimeTearDown]
    public void Cleanup()
    {
        Directory.Delete(_testFolder, true);
    }

    //Does obj with id 99999999999 exist and does it have Fun3 & Subfun3 as sub-objects.
    [Test]
    public void AddTest1()
    {
        BinaryObject? obj = ObjExistsWithID(99999999999);
        if (obj == null)
        {
            Assert.Fail();
            return;
        }

        //fun3
        obj = obj.Children.FirstOrDefault(child => child.NameHash == 0x89af5579);
        if (obj == null)
        {
            Assert.Fail();
            return;
        }
        
        //subfun3
        obj = obj.Children.FirstOrDefault(child => child.NameHash == 0x3f95b150);
        if (obj == null)
            Assert.Fail();
        else
            Assert.Pass();
    }
    
    //Does obj with id 99999999991 exist and does it have Fun2 and Fun3.
    [Test]
    public void AddTest2()
    {
        BinaryObject? obj = ObjExistsWithID(99999999991);
        if (obj == null)
        {
            Assert.Fail();
            return;
        }

        bool exists = obj.Children.Any(child => child.NameHash == 0xfea865ef) &&
                      obj.Children.Any(child => child.NameHash == 0x89af5579);

        if (exists)
            Assert.Pass();
        else
            Assert.Fail();
    }

    //Does obj with id 99999999993 exist at the bottom.
    [Test]
    public void AddTest3()
    {
        BinaryObject? obj = ObjExistsWithID(99999999993);
        if (obj != null && objRoot.Children.Last() == obj)
            Assert.Pass();
        else
            Assert.Fail();
    }

    //See if Value has a modified number, Value2 exists and has lowpri's value, and Coeff is gone.
    [Test]
    public void EditTest1()
    {
        BinaryObject? obj = ObjExistsWithID(1161799);
        if (obj == null)
        {
            Assert.Fail();
            return;
        }

        obj = obj.Children[0].Children[0].Children[0].Children[0].Children[0];

        byte[]? value = GetField(obj, 0xdcb67730);
        if (BitConverter.ToSingle(value) != 0.315f)
        {
            Assert.Fail();
            return;
        }

        value = GetField(obj, 0x3cd038d6);
        if (value == null || BitConverter.ToSingle(value) != 0.5f)
        {
            Assert.Fail();
            return;
        }

        value = GetField(obj, 0x635e3356);
        if (value != null)
        {
            Assert.Fail();
            return;
        }

        Assert.Pass();
    }

    //Test a depth + id combo.
    [Test]
    public void EditTest2()
    {
        BinaryObject? obj = ObjExistsWithID(126207190);
        if (obj == null)
        {
            Assert.Fail();
            return;
        }

        obj = obj.Children[0].Children[0].Children[1].Children[0].Children[0];

        byte[]? value = GetField(obj, 0xb7f8c275);
        if (BitConverter.ToInt32(value) != -1)
        {
            Assert.Fail();
            return;
        }

        Assert.Pass();
    }

    //Test nested objects
    [Test]
    public void EditTest3()
    {
        BinaryObject? obj = ObjExistsWithID(126207190);
        if (obj == null)
        {
            Assert.Fail();
            return;
        }

        BinaryObject child = obj.Children[0];
        if (GetField(child, 0x25f8bc33) != null)
        {
            Assert.Fail();
            return;
        }

        if (obj.Children.Count > 2)
        {
            Assert.Fail();
            return;
        }

        child = obj.Children[1];
        if (GetField(child, 0x25f8bc33) == null)
        {
            Assert.Fail();
            return;
        }

        child = obj.Children[0].Children[3].Children[0];
        if (GetField(child, 0xa733c3f9)[0] != 0x01)
        {
            Assert.Fail();
            return;
        }

        Assert.Pass();
    }

    //Test if edit was made succesfully to modded obj
    [Test]
    public void EditTest4()
    {
        BinaryObject? obj = ObjExistsWithID(99999999991);
        if (obj == null)
        {
            Assert.Fail();
            return;
        }

        bool exists = obj.Children.Any(child => child.NameHash == 0x17cbc0da);
        if (!exists)
        {
            Assert.Fail();
            return;
        }

        obj = obj.Children.FirstOrDefault(child => child.NameHash == 0xfea865ef);
        if (obj == null)
        {
            Assert.Fail();
            return;
        }

        byte[]? value = GetField(obj, 0x3060fa4b);
        if (value is not [0x00])
        {
            Assert.Fail();
            return;
        }

        Assert.Pass();
    }

    //Every ContainedItemsListValue should be -1 + there should be no child object with namehash Fun2
    [Test]
    public void EditAllTest1()
    {
        BinaryObject? obj = ObjExistsWithID(126291468);
        if (obj == null)
        {
            Assert.Fail();
            return;
        }
        
        bool valid = EditAllTest1Recurse(obj);
        if (!valid)
            Assert.Fail();
        else
            Assert.Pass();
    }

    private bool EditAllTest1Recurse(BinaryObject obj)
    {
        if (obj.Fields.Count > 0 && obj.Fields.Any(field => field.Key == 0xb7f8c275 && BitConverter.ToInt64(field.Value) != -1))
            return false;
        
        foreach (BinaryObject child in obj.Children)
        {
            return child.NameHash != 0xfea865ef && EditAllTest1Recurse(child);
        }

        return true;
    }
    
    //Does obj with id 99999999992 exist (it shouldnt).
    [Test]
    public void RemoveTest1()
    {
        BinaryObject? obj = ObjExistsWithID(99999999992);
        if (obj == null)
            Assert.Pass();
        else
            Assert.Fail();
    }
    
    //Does obj with id 0x1ABB940700000000 exist (it shouldnt).
    [Test]
    public void RemoveTest2()
    {
        BinaryObject? obj = ObjExistsWithID(0x1ABB940700000000);
        if (obj == null)
            Assert.Pass();
        else
            Assert.Fail();
    }
    
    //Does obj with id 0x18BB940700000000 exist (it shouldnt).
    [Test]
    public void RemoveTest3()
    {
        BinaryObject? obj = ObjExistsWithID(0x18BB940700000000);
        if (obj == null)
            Assert.Pass();
        else
            Assert.Fail();
    }
    
    //CITEntityComponentMeshDeform should not exist @ 0x1CBB940700000000
    [Test]
    public void RemoveTest4()
    {
        BinaryObject? obj = ObjExistsWithID(127187740).Children[0];
        
        bool exists = obj.Children.Any(child => child.NameHash == 0xc0327da6);

        if (exists)
            Assert.Fail();
        else
            Assert.Pass();
    }
    
    //The child count at 2558 (1BBB940700000000) should be 0
    [Test]
    public void RemoveTest5()
    {
        BinaryObject? obj = ObjExistsWithID(127187739);
        if (obj == null)
        {
            Assert.Fail();
            return;
        }
        
        if (obj.Children.Count == 0)
            Assert.Pass();
        else
            Assert.Fail();
    }

    //Emitter should be gone and Camera should be gone from "Generic" @0xE326840700000000
    [Test]
    public void RemoveTest6()
    {
        BinaryObject? obj = ObjExistsWithID(126101219);
        if (obj == null)
        {
            Assert.Fail();
            return;
        }

        //emitter
        bool exists = obj.Children.Any(child => child.NameHash == 0xba33f4ad);
        if (exists)
        {
            Assert.Fail();
            return;
        }

        //generic
        obj = obj.Children.FirstOrDefault(child => child.NameHash == 0x59f2984f);
        if (obj == null)
        {
            Assert.Fail();
            return;
        }

        //camera
        exists = obj.Children.Any(child => child.NameHash == 0x3cb0eb33);
        if (exists)
            Assert.Fail();
        else
            Assert.Pass();
    }

    private void CopyEmbedToFolder(string source, string destination)
    {
        Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(source);
        FileStream fileStream = File.Create(destination);
        stream.Seek(0, SeekOrigin.Begin);
        stream.CopyTo(fileStream);
        fileStream.Close();
        stream.Close();
    }

    private byte[]? GetField(BinaryObject? obj, uint hash)
    {
        if (obj == null)
            return null;

        if (!obj.Fields.ContainsKey(hash))
            return null;

        return obj.Fields[hash];
    }

    private BinaryObject? ObjExistsWithID(ulong id)
    {
        foreach (BinaryObject child in objRoot.Children)
        {
            if (!child.Fields.ContainsKey(0x11d3633a))
                continue;

            if (BitConverter.ToUInt64(child.Fields[0x11d3633a]) != id)
                continue;

            return child;
        }

        return null;
    }
}
