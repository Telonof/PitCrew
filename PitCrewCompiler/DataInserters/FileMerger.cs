using Dunia2.MergeBinaryObject;
using Gibbed.Dunia2.FileFormats;
using PitCrewCommon;
using PitCrewCommon.Models;
using PitCrewCommon.Utilities;
using System.Xml;
using System.Xml.Linq;

namespace PitCrewCompiler.DataInserters
{
    internal class FileMerger
    {
        private readonly string[] LocalizationFolders = ["pc_steam_ww", "pc_ww", "pc_steam_rus", "pc_rus"];
        private readonly string[] Patches = ["", "_patch", "_patch_1"];

        private readonly string MergingFolder = "merging_folder";
        private readonly string OutputFile = "mods/PitCrewBase.fat";
        private readonly string GameDirectory, ServerBinaryLocation;

        private bool ServerDataUsed { get; set; } = false;
        private int PackageVersion { get; }

        public FileMerger(string gameDirectory, List<ModFile> xmlFiles)
        {
            GameDirectory = gameDirectory;
            string gameRootPath = FileUtil.GetParentDir(gameDirectory);
            ServerBinaryLocation = Path.Combine(gameRootPath, Constants.SERVER_DATA_FILE);
            PackageVersion = File.Exists(Path.Combine(gameRootPath, Constants.OODLE_FILE)) ? 6 : 5;

            //The bool is to tell the merger wether to increase percentage count or not (mainly to ignore the localization files we mass-add ourselves)
            Dictionary<string, Dictionary<XmlFile, bool>> mergingFcbs = [];
            Dictionary<string, HashSet<XmlFile>> mergingBdbs = [];

            FileUtil.CheckAndCreateFolder(MergingFolder);
            string[] files = UnpackAll();

            if (files.Length == 0)
                Logger.Error(101, Translatable.Get("compiler.no-global-db"));

            if (files.Length == 0 || xmlFiles.Count == 0)
            {
                FileUtil.CheckAndDeleteFile(ServerBinaryLocation);
                FileUtil.CheckAndDeleteFolder(MergingFolder);
                return;
            }

            //Figure out where each file is trying to merge.
            foreach (ModFile modFile in xmlFiles)
            {
                ParsePath(modFile, mergingFcbs, mergingBdbs);
            }

            MergeFCBs(mergingFcbs);
            MergeBDBs(mergingBdbs);

            //Now delete every file we did not modify to save space, and pack it up.
            var modifiedFiles = mergingBdbs.Keys.Union(mergingFcbs.Keys);
            foreach (string item in files)
            {
                if (modifiedFiles.Contains(item))
                    continue;

                FileUtil.CheckAndDeleteFile(item);
            }

            BigFileUtil.RepackBigFile(MergingFolder, Path.Combine(gameDirectory, OutputFile), "PitCrew", PackageVersion);
            FileUtil.CheckAndDeleteFolder(MergingFolder);
        }

        private string[] UnpackAll()
        {
            //Grab where localization is stored on computer since there is multiple game versions.
            string pcFolder = LocalizationFolders
                .Where(dir => Directory.Exists(Path.Combine(GameDirectory, dir)))
                .First();

            foreach (string patch in Patches)
            {
                Unpack($"global_db{patch}.fat");
                Unpack($"scenaric{patch}.fat");
                Unpack(Path.Combine(pcFolder, $"localization{patch}.fat"));
            }

            return Directory.GetFiles(MergingFolder, "*", SearchOption.AllDirectories);
        }

        private void Unpack(string file)
        {
            file = Path.Combine(GameDirectory, file);
            if (!File.Exists(file))
                return;

            BigFileUtil.UnpackBigFile(file, MergingFolder, PackageVersion);
        }

        private void ParsePath(ModFile modFile, Dictionary<string, Dictionary<XmlFile, bool>> mergingFcb, Dictionary<string, HashSet<XmlFile>> mergingBdb)
        {
            XDocument doc;
            bool isCrew1 = PackageVersion == 5;
            try
            {
                doc = XDocument.Load(Path.Combine(GameDirectory, modFile.Location));
            }
            catch (XmlException e)
            {
                Logger.Error(102, e.Message);
                return;
            }

            //We need to make sure our path is actually valid first before we check it.
            XAttribute mergingAttribute = doc.Root.Attribute("file");
            if (mergingAttribute == null || string.IsNullOrWhiteSpace(mergingAttribute.Value))
            {
                Logger.Error(103, string.Format(Translatable.Get("compiler.no-attribute-in-root"), Path.Combine(GameDirectory, modFile.Location)));
                return;
            }

            string mergingPath = mergingAttribute.Value.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToLower();

            //Handle localization. The goal is adding strings to every language in the game if modder does not care about specifying languages.
            if (mergingPath.Equals("localization"))
            {
                string[] directories = Directory.GetDirectories(Path.Combine(MergingFolder, "localization"));

                foreach (string locDirectory in directories)
                {
                    mergingPath = Path.Combine(locDirectory, "13.localization.bin");
                    mergingFcb.TryAdd(mergingPath, []);
                    mergingFcb[mergingPath].Add(new XmlFile { Location = modFile.Location, XmlData = doc }, false);
                }

                PercentageCalculator.IncrementProgress();
                return;
            }

            //TC1's server binary is read from the game root due to it needed to be
            //patched in live into a packet for the TCU emulator.
            if (isCrew1 && mergingPath.Equals("server"))
            {
                ServerDataUsed = true;
                mergingFcb.TryAdd(ServerBinaryLocation, []);
                mergingFcb[ServerBinaryLocation].Add(new XmlFile { Location = modFile.Location, XmlData = doc }, true);
                return;
            }

            //Patched into the TCU emulator to properly handle giving out rewards for missions.
            if (isCrew1 && mergingPath.Equals("server_missions"))
            {
                mergingBdb.TryAdd(mergingPath, []);
                mergingBdb[mergingPath].Add(new XmlFile { Location = modFile.Location, XmlData = doc });
                return;
            }

            //These are the files that need to have their control taken away from manually since a modder manually working with these lists
            //could lead to purchased items in savefiles being tampered with, indirectly ruining someone's save with custom cars,
            //items, etc. which is a no-go.
            if (isCrew1 && (mergingPath.Equals(Path.Combine("road66database", "physcarpartdb.bin")) || mergingPath.Equals(Path.Combine("road66database", "rendercarpartdb.bin"))))
            {
                Logger.Error(104, string.Format(Translatable.Get("compiler.forbid-databases"), Path.GetFileName(mergingPath)));
                return;
            }

            mergingPath = Path.Combine(MergingFolder, mergingPath);

            if (!File.Exists(mergingPath))
            {
                Logger.Error(105, string.Format(Translatable.Get("compiler.file-not-found-in-xml"), mergingPath));
                return;
            }

            if (mergingPath.EndsWith(".babdb"))
            {
                mergingBdb.TryAdd(mergingPath, []);
                mergingBdb[mergingPath].Add(new XmlFile { Location = modFile.Location, XmlData = doc });
            }
            else
            {
                mergingFcb.TryAdd(mergingPath, []);
                mergingFcb[mergingPath].Add(new XmlFile { Location = modFile.Location, XmlData = doc }, true);
            }
        }

        private void MergeFCBs(Dictionary<string, Dictionary<XmlFile, bool>> mergingFcbs)
        {
            BinaryObjectMerger merger = new BinaryObjectMerger();

            //First we need to see if we are using the server binary and clone the file from the assets folder if yes.
            if (ServerDataUsed)
                File.Copy(Path.Combine("Assets", Constants.SERVER_DATA_FILE), ServerBinaryLocation, true);
            else
                FileUtil.CheckAndDeleteFile(ServerBinaryLocation);

            foreach (string key in mergingFcbs.Keys)
            {
                BinaryObjectFile bof = new BinaryObjectFile();
                FileStream stream = File.OpenRead(key);
                //Memory intensive but it's to be expected dealing with massive binaries.
                //Expect 1.5-3 gigs of ram to be used if modding the heavier files.
                bof.Deserialize(stream);
                stream.Close();

                foreach (XmlFile subKey in mergingFcbs[key].Keys)
                {
                    bof.Root = merger.Merge(bof.Root, subKey.XmlData);
                    //Only allow files that were added by the user to count as progress, not the localization files added automatically.
                    if (mergingFcbs[key][subKey])
                    {
                        PercentageCalculator.IncrementProgress();
                        Logger.Print(string.Format(Translatable.Get("compiler.merging-file"), Path.GetFileName(subKey.Location), Path.GetFileName(key)));
                    }
                }

                stream = File.OpenWrite(key);
                bof.Serialize(stream);
                stream.Close();
            }
        }

        private void MergeBDBs(Dictionary<string, HashSet<XmlFile>> mergingBdbs)
        {
            DatabaseInserter dbInserter = new DatabaseInserter(mergingBdbs, MergingFolder, GameDirectory, PackageVersion);

            //TC2 doesn't use the addon system as that's strictly for hooking into the TCU emulator
            //which only exists for TC1.
            if (PackageVersion == Constants.THE_CREW_2)
                return;

            mergingBdbs.Add(Path.Combine(MergingFolder, "road66database", "physcarpartdb.bin"), []);
            mergingBdbs.Add(Path.Combine(MergingFolder, "road66database", "rendercarpartdb.bin"), []);
        }

        public record XmlFile
        {
            public string Location { get; set; }

            public XDocument XmlData { get; set; }
        }
    }
}
