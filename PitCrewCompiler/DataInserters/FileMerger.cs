using Dunia2.MergeBinaryObject;
using Gibbed.Dunia2.FileFormats;
using PitCrewCommon;
using PitCrewCommon.Models;
using PitCrewCommon.Utilities;
using System.Xml.Linq;

namespace PitCrewCompiler.DataInserters
{
    internal class FileMerger
    {
        private readonly string XmlDirectory = "xmltemp";

        private readonly string OutputFile = "mods/PitCrewBase.fat";

        private readonly Dictionary<string, BinaryObjectFile> ModifiedBinaryFiles = [];

        private readonly Dictionary<string, BabelDBFile> ModifiedDatabaseFiles = [];

        private readonly HashSet<string> AllowedFiles = [".bin", ".fcb", ".bwo", ".babdb"];

        private readonly HashSet<string> localizationFolders = ["pc_steam_ww", "pc_ww", "pc_steam_rus", "pc_rus"];

        private readonly List<string> Patches = ["", "_patch", "_patch_1"];

        public bool ServerDataUsed { get; private set; } = false;

        public FileMerger(string directory, List<ModFile> xmlFiles)
        {
            BinaryObjectMerger merger = new BinaryObjectMerger();
            BabelDBMerger bdbMerger = new BabelDBMerger();
            int packageVersion = xmlFiles[0].ParentMod.ParentInstance.PackageVersion;

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

                //TC2
                bigFile = Path.Combine(directory, $"scenaric{patch}.fat");
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

                string mergingPath = Path.Combine(XmlDirectory, mergerFile.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));

                //Handle server-side data
                if (mergerFile.Equals("server", StringComparison.OrdinalIgnoreCase))
                {
                    ServerDataUsed = true;

                    string serverPath = FileUtil.GetParentDir(xmlFile.ParentMod.ParentInstance.GetDirectory());
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
                if (mergingPath.EndsWith(".babdb"))
                    CheckAndMergeFile(bdbMerger, xmlDoc, mergingPath);
                else
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
            foreach (string moddedFile in ModifiedBinaryFiles.Keys)
            {
                FileStream stream = File.OpenWrite(moddedFile);
                ModifiedBinaryFiles[moddedFile].Serialize(stream);
                stream.Close();
            }

            foreach (string moddedFile in ModifiedDatabaseFiles.Keys)
            {
                FileStream stream = File.OpenWrite(moddedFile);
                ModifiedDatabaseFiles[moddedFile].Serialize(stream);
                stream.Close();
            }

            BigFileUtil.RepackBigFile(XmlDirectory, Path.Combine(directory, OutputFile), "PitCrew");
            FileUtil.CheckAndDeleteFolder(XmlDirectory);
        }

        private void CheckAndMergeFile(BinaryObjectMerger merger, XDocument inputXML, string outputBinary)
        {
            if (!ModifiedBinaryFiles.ContainsKey(outputBinary))
            {
                BinaryObjectFile binaryObjectFile = new BinaryObjectFile();
                FileStream stream = File.OpenRead(outputBinary);
                //Memory intensive but it's to be expected dealing with massive binaries.
                //Expect 1.5 gigs of ram to be used if modding the heavier files.
                binaryObjectFile.Deserialize(stream);
                stream.Close();
                ModifiedBinaryFiles.Add(outputBinary, binaryObjectFile);
            }
            BinaryObjectFile bof = ModifiedBinaryFiles[outputBinary];
            bof.Root = merger.Merge(bof.Root, inputXML);
        }

        private void CheckAndMergeFile(BabelDBMerger merger, XDocument inputXML, string outputBinary)
        {
            if (!ModifiedDatabaseFiles.ContainsKey(outputBinary))
            {
                BabelDBFile bdb = new BabelDBFile();
                FileStream stream = File.OpenRead(outputBinary);
                bdb.Deseralize(stream);
                stream.Close();
                ModifiedDatabaseFiles.Add(outputBinary, bdb);
            }

            BabelDBFile babdb = ModifiedDatabaseFiles[outputBinary];
            ModifiedDatabaseFiles[outputBinary] = merger.Merge(babdb, inputXML);
        }

        private void CheckAndUnpackFile(string bigFile, int packageVersion)
        {
            //By this point BackupAndUnpack has already done the job of copying the oodle file.
            //So no need for a directory in UnpackBigFile here.
            if (File.Exists(bigFile))
                BigFileUtil.UnpackBigFile(bigFile, XmlDirectory, packageVersion);
        }
    }
}
