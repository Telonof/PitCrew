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
            int priority = 998 - Instance.Mods.Count;

            foreach (Mod mod in Instance.Mods)
            {
                if (!mod.Enabled)
                    continue;

                List<ModFile> files = [];

                //Update 998 priorities based on order of mod list.
                for (int i = 0; i < mod.ModFiles.Count; i++)
                {
                    ModFile file = new ModFile(mod.ModFiles[i]);
                    if (file.Priority == 998)
                        file.Priority = Math.Max(11, priority);

                    if (file.Priority == 997)
                        file.Priority = Math.Max(11, priority - 1);

                    files.Add(file);
                }

                foreach (ModFile file in files)
                {
                    if (!Instance.IsCLI)
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

            //Run all inserters
            _ = new StartupInserter(Instance.GetDirectory(), StartupMods);

            if (XmlMods.Count > 0)
            {
                XmlMods = XmlMods.OrderByDescending(mod => mod.Priority).ToList();

                FileMerger inserter = new FileMerger(Instance.GetDirectory(), XmlMods);

                FilesinfosMods.Add(new ModFile(XmlMods[0].ParentMod, "mods/PitCrewBase", 9));

                if (!inserter.ServerDataUsed)
                    FileUtil.CheckAndDeleteFile(Path.Combine(FileUtil.GetParentDir(Instance.GetDirectory()), Constants.SERVER_DATA_FILE));
            }
            else
            {
                //Delete that server data binary we may have edited.
                //TODO This stinks if someone is manually editing it.
                FileUtil.CheckAndDeleteFile(Path.Combine(FileUtil.GetParentDir(Instance.GetDirectory()), Constants.SERVER_DATA_FILE));
            }

            _ = new FilesInfosInserter(Instance.GetDirectory(), FilesinfosMods);

            if (!Instance.IsCLI)
                PercentageCalculator.IncrementProgress(FilesinfosMods.Count);

            Logger.Print(Translatable.Get("compiler.success"));
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
