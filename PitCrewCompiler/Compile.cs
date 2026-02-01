using Gibbed.Dunia2.FileFormats;
using Gibbed.Dunia2.FileFormats.Big;
using PitCrewCommon;
using PitCrewCommon.Models;
using PitCrewCommon.Utilities;
using PitCrewCompiler.DataInserters;

namespace PitCrewCompiler
{
    public class Compile
    {
        private readonly Instance Instance;
        private List<ModFile> FilesinfosMods { get; set; } = [];
        private List<ModFile> XmlMods { get; set; } = [];
        private List<ModFile> StartupMods { get; set; } = [];

        public Compile(Instance instance)
        {
            Instance = instance;
        }

        public void Run()
        {
            PercentageCalculator.Reset();
            Mod[] files = Instance.Mods.Where(mod => mod.Enabled).ToArray();
            int priority = 998 - files.Length;

            foreach (Mod mod in files)
            {
                List<ModFile> modFiles = [];

                //Update 998 priorities based on order of mod list.
                for (int i = 0; i < mod.ModFiles.Count; i++)
                {
                    ModFile file = new ModFile(mod.ModFiles[i]);
                    if (file.Priority == 998)
                        file.Priority = Math.Max(11, priority);

                    if (file.Priority == 997)
                        file.Priority = Math.Max(11, priority - 1);

                    modFiles.Add(file);
                }

                foreach (ModFile file in modFiles)
                {
                    PercentageCalculator.IncrementTotal();

                    if (file.Location.EndsWith(".xml"))
                    {
                        XmlMods.Add(file);
                        continue;
                    }

                    if (file.Priority == 10)
                    {
                        StartupMods.Add(file);
                        continue;
                    }

                    FilesinfosMods.Add(file);
                }

                priority++;
            }

            BackupAndUnpack();
            ExtractMergingItems();

            //Run all inserters
            _ = new StartupInserter(Instance.GetDirectory(), StartupMods);

            XmlMods = XmlMods.OrderByDescending(mod => mod.Priority).ToList();
            FileMerger inserter = new FileMerger(Instance.GetDirectory(), XmlMods);
            if (XmlMods.Count > 0)
                FilesinfosMods.Add(new ModFile(XmlMods[0].ParentMod, "mods/PitCrewBase", 9));

            _ = new FilesInfosInserter(Instance.GetDirectory(), FilesinfosMods);

            PercentageCalculator.IncrementProgress(FilesinfosMods.Count);

            Logger.Print(Translatable.Get("compiler.success"));
        }

        private void ExtractMergingItems()
        {
            if (Instance.PackageVersion != Constants.THE_CREW_2)
                return;

            for (int i = 0; i < FilesinfosMods.Count; i++)
            {
                BigFile fat = new BigFile();
                using FileStream fatStream = File.OpenRead(Path.Combine(Instance.GetDirectory(), $"{FilesinfosMods[i].Location}.fat"));
                using FileStream datStream = File.OpenRead(Path.Combine(Instance.GetDirectory(), $"{FilesinfosMods[i].Location}.dat"));
                fat.Deserialize(fatStream);

                foreach (Entry entry in fat.Entries)
                {
                    if (entry.CompressionScheme != CompressionScheme.Zlib)
                        continue;

                    string name = Path.GetFileNameWithoutExtension(FilesinfosMods[i].Location);
                    Stream outputStream = new MemoryStream();

                    Entry xmlEntry = new Entry
                    {
                        NameHash = entry.NameHash,
                        UncompressedSize = entry.UncompressedSize,
                        CompressedSize = entry.CompressedSize,
                        Offset = entry.Offset,
                        CompressionScheme = CompressionScheme.oodle
                    };

                    EntryDecompression.Decompress(xmlEntry, datStream, outputStream);
                    PercentageCalculator.IncrementTotal();

                    outputStream.Seek(0, SeekOrigin.Begin);

                    XmlMods.Add(new ModFile { Priority = FilesinfosMods[i].Priority, FileData = outputStream, Location = name});
                }
            }
        }

        private void BackupAndUnpack()
        {
            string cstartupFatPath = Path.Combine(Instance.GetDirectory(), "cstartup.fat");
            string cstartupDatPath = Path.Combine(Instance.GetDirectory(), "cstartup.dat");
            string startupFatPath = Path.Combine(Instance.GetDirectory(), "startup.fat");
            string startupDatPath = Path.Combine(Instance.GetDirectory(), "startup.dat");

            //cstartup is "clean startup" meaning we use a clean slate startup when it comes to modding it rather than overwriting modded startup.
            string unpackingFat = File.Exists(cstartupFatPath) ? cstartupFatPath : startupFatPath;

            if (!File.Exists(unpackingFat))
            {
                Logger.Error(106, Translatable.Get("compiler.no-startup-file"));
                throw new FileNotFoundException();
            }

            BigFileUtil.UnpackBigFile(unpackingFat, "tmp", Instance.PackageVersion, FileUtil.GetParentDir(Instance.GetDirectory()));

            //Create cstartup if not existing yet
            if (!File.Exists(cstartupFatPath))
            {
                File.Copy(startupFatPath, cstartupFatPath);
                File.Copy(startupDatPath, cstartupDatPath);
            }
        }
    }
}
