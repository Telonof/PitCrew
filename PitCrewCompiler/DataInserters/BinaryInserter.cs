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

        private readonly HashSet<string> localizationFolders = ["pc_steam_ww", "pc_ww", "pc_steam_rus", "pc_rus"];

        private readonly List<string> Patches = ["", "_patch", "_patch_1"];

        public bool ServerDataUsed { get; private set; } = false;

        public BinaryInserter(string directory, List<ModFile> xmlFiles)
        {
            BinaryObjectMerger merger = new BinaryObjectMerger();
            int packageVersion = xmlFiles[0].ParentMod.ParentInstance.PackageVersion;

            //Do not even attempt TC2 merging for now until online is gone
            if (packageVersion == 6)
                return;

            //Find what pc_ folder the player is using as different copies have different pc_ directories.
            //Used for localization merging.
            string? pc_dir = localizationFolders
                .Select(locFolder => Path.Combine(directory, locFolder))
                .FirstOrDefault(Directory.Exists);
            
            //Extract all global_db's if existing, and overwrite any files for each
            foreach (string patch in Patches)
            {
                string bigFile = Path.Combine(directory, $"global_db{patch}.fat");
                CheckAndUnpackFile(bigFile, packageVersion);
                
                bigFile = Path.Combine(directory, pc_dir, $"localization{patch}.fat");
                CheckAndUnpackFile(bigFile, packageVersion);
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
                        mergingPath = Path.Combine(locDirectory, "13.localization.bin");
                        CheckAndMergeFile(merger, xmlDoc, mergingPath);
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
                CheckAndMergeFile(merger, xmlDoc, mergingPath);

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

            BigFileUtil.RepackBigFile(XmlDirectory, Path.Combine(directory, OutputFile), "PitCrew");
            FileUtil.CheckAndDeleteFolder(XmlDirectory);
        }

        private void CheckAndMergeFile(BinaryObjectMerger merger, XDocument inputXML, string outputBinary)
        {
            if (!ModifiedFiles.ContainsKey(outputBinary))
            {
                BinaryObjectFile binaryObjectFile = new BinaryObjectFile();
                FileStream stream = File.OpenRead(outputBinary);
                //Memory intensive but it's to be expected dealing with massive binaries.
                //Expect 1.5 gigs of ram to be used if modding the heavier files.
                binaryObjectFile.Deserialize(stream);
                stream.Close();
                ModifiedFiles.Add(outputBinary, binaryObjectFile);
            }
            BinaryObjectFile bof = ModifiedFiles[outputBinary];
            bof.Root = merger.Merge(bof.Root, inputXML);
        }

        private void CheckAndUnpackFile(string bigFile, int packageVersion)
        {
            if (File.Exists(bigFile))
                BigFileUtil.UnpackBigFile(bigFile, XmlDirectory, packageVersion);
        }
    }
}
