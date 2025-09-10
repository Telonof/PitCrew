using Dunia2.MergeBinaryObject;
using Gibbed.Dunia2.FileFormats;
using PitCrewCommon;
using PitCrewCommon.Models;
using PitCrewCommon.Utilities;
using System.Xml.Linq;

namespace PitCrewCompiler.DataInserters
{
    internal class BinaryInserter
    {
        private readonly string XmlDirectory = "xmltemp";

        private readonly string OutputFile = "mods/PitCrewBase.fat";

        private readonly Dictionary<string, BinaryObjectFile> ModifiedFiles = [];

        private readonly HashSet<string> AllowedFiles = [".bin", ".fcb", ".bwo"];

        private readonly List<string> Patches = ["", "_patch", "_patch_1"];

        public bool ServerDataUsed { get; private set; } = false;

        public BinaryInserter(string directory, List<ModFile> xmlFiles)
        {
            BinaryObjectMerger merger = new BinaryObjectMerger();

            //Find what pc_ folder the player is using as different copies have different pc_ directories.
            //Used for localization merging.
            string[] localizationDirectories = Directory.GetDirectories(directory, "pc_*");
            
            //There should only be one, it would be very confusing if someone pasted a different pc_ in there.
            string pc_dir = localizationDirectories[0];
            
            //Extract all global_db's if existing, and overwrite any files for each
            foreach (string patch in Patches)
            {
                string bigFile = Path.Combine(directory, $"global_db{patch}.fat");
                if (File.Exists(bigFile))
                    BigFileUtil.UnpackBigFile(bigFile, XmlDirectory);
                
                bigFile = Path.Combine(directory, pc_dir, $"localization{patch}.fat");
                if (File.Exists(bigFile))
                    BigFileUtil.UnpackBigFile(bigFile, XmlDirectory);
            }

            string[] files = Directory.GetFiles(XmlDirectory, "*", SearchOption.AllDirectories);

            if (files.Length == 0)
            {
                Logger.Error(101, Translatable.Get("compiler.no-global-db"));
                FileUtil.CheckAndDeleteFolder(XmlDirectory);
                return;
            }

            //Clean out everything that's not a binary as we are only interested in merging binaries
            foreach (string item in files)
            {
                if (!AllowedFiles.Contains(Path.GetExtension(item)))
                    FileUtil.CheckAndDeleteFile(item);
            }

            foreach (ModFile xmlFile in xmlFiles)
            {
                BinaryObjectFile bof;
                string file = Path.Combine(directory, xmlFile.Location);

                XDocument xmlDoc = XDocument.Load(file);
                string? mergerFile = xmlDoc.Root.Attribute("file").Value;
                if (mergerFile == null)
                {
                    Logger.Error(102, string.Format(Translatable.Get("compiler.no-attribute-in-root"), xmlFile));
                    continue;
                }

                string mergingPath = Path.Combine(XmlDirectory, mergerFile);

                //Handle server-side data
                if (mergerFile.Equals("server", StringComparison.OrdinalIgnoreCase))
                {
                    ServerDataUsed = true;

                    string serverPath = Directory.GetParent(xmlFile.ParentMod.ParentInstance.GetDirectory()).FullName;
                    mergingPath = Path.Combine(serverPath, Constants.SERVER_DATA_FILE);
                    File.Copy(Path.Combine("Assets", Constants.SERVER_DATA_FILE), mergingPath, true);
                }

                //Handle localization. The goal is adding strings to every language in the game if modder does not care about specifying languages.
                if (mergerFile.Equals("localization", StringComparison.OrdinalIgnoreCase))
                {
                    string[] directories = Directory.GetDirectories(Path.Combine(XmlDirectory, "localization"));

                    Logger.Print(string.Format(Translatable.Get("compiler.merging-file"), "localization", Path.GetFileName(mergingPath)));

                    foreach (string locDirectory in directories)
                    {
                        string localizationFile = Path.Combine(locDirectory, "99.localization.bin");
                        if (!ModifiedFiles.ContainsKey(localizationFile))
                            ModifiedFiles.Add(localizationFile, new BinaryObjectFile());

                        bof = ModifiedFiles[localizationFile];

                        if (bof.Root == null)
                        {
                            //Default localization settings
                            BinaryObject obj = new BinaryObject();
                            obj.NameHash = 150977874;
                            obj.Uid = "0";
                            bof.Root = obj;
                        }

                        bof.Root = merger.Merge(bof.Root, xmlDoc);
                    }

                    if (!xmlFile.ParentMod.ParentInstance.IsCLI)
                        PercentageCalculator.IncrementProgress();

                    continue;
                }

                //Attempt to find the file this xml is trying to merge into.
                if (!File.Exists(mergingPath))
                {
                    Logger.Error(104, string.Format(Translatable.Get("compiler.file-not-found-in-xml"), mergingPath));
                    continue;
                }

                Logger.Print(string.Format(Translatable.Get("compiler.merging-file"), Path.GetFileName(xmlFile.Location), Path.GetFileName(mergingPath)));

                //Found it, now check the dictonary to see if we already modded it.
                if (!ModifiedFiles.ContainsKey(mergingPath))
                {
                    BinaryObjectFile binaryObjectFile = new BinaryObjectFile();
                    FileStream stream = File.OpenRead(mergingPath);
                    //Memory intensive but it's to be expected dealing with massive binaries.
                    //Expect 1.5 gigs of ram to be used if modding the heavier files.
                    binaryObjectFile.Deserialize(stream);
                    stream.Close();
                    ModifiedFiles.Add(mergingPath, binaryObjectFile);
                }
                bof = ModifiedFiles[mergingPath];

                bof.Root = merger.Merge(bof.Root, xmlDoc);

                if (!xmlFile.ParentMod.ParentInstance.IsCLI)
                    PercentageCalculator.IncrementProgress();
            }

            //We only want to repack modified files, so get rid of all files that weren't used.
            foreach (string file in Directory.GetFiles(XmlDirectory, "*", SearchOption.AllDirectories))
            {
                FileUtil.CheckAndDeleteFile(file);
            }

            //Convert all the objects back into their files and pack the directory
            foreach (string moddedFile in ModifiedFiles.Keys)
            {
                FileStream stream = File.OpenWrite(moddedFile);
                ModifiedFiles[moddedFile].Serialize(stream);
                stream.Close();
            }

            BigFileUtil.RepackBigFile(XmlDirectory, Path.Combine(directory, OutputFile), xmlFiles[0].ParentMod.ParentInstance.PackageVersion, "PitCrew");
            FileUtil.CheckAndDeleteFolder(XmlDirectory);
        }
    }
}
