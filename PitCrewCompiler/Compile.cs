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

            foreach (Mod mod in Instance.Mods)
            {
                if (!mod.Enabled)
                    continue;

                foreach (ModFile file in mod.ModFiles)
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
            }

            BackupAndUnpack();

            //Run all inserters
            _ = new StartupInserter(Instance.GetDirectory(), StartupMods);

            if (XmlMods.Count > 0)
            {
                XmlMods = XmlMods.OrderByDescending(mod => mod.Priority).ToList();

                _ = new BinaryInserter(Instance.GetDirectory(), XmlMods);

                //We really don't care what PitCrewBase's parent mod is, it only ever exists on compile time and we don't need much access to it's data.
                FilesinfosMods.Add(new ModFile(null, "mods/PitCrewBase", 11));
            }
            else
            {
                //Delete that server data binary we may have edited.
                //TODO This stinks if someone is manually editing it.
                if (!string.IsNullOrWhiteSpace(Instance.ServerLocation))
                    FileUtil.CheckAndDeleteFile(Path.Combine(Instance.ServerLocation, "server_data.fcb"));
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

            BigFileUtil.UnpackBigFile(unpackingFat, "tmp");

            //Create cstartup if not existing yet
            if (!File.Exists(cstartupFatPath))
            {
                File.Copy(startupFatPath, cstartupFatPath);
                File.Copy(startupDatPath, cstartupDatPath);
            }
        }
    }
}
