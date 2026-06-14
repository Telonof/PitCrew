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
        private List<ModFile> Shaders { get; set; } = [];


        public Compile(Instance instance)
        {
            Instance = instance;
        }

        public async Task Run()
        {
            PercentageCalculator.Reset();
            Mod[] files = Instance.GetAllMods().Where(mod => mod.Enabled).ToArray();
            HashSet<string> modIds = files.Select(mod => mod.Id).ToHashSet();
            int priority = 998 - files.Length;
            float shaderPriority = 8.0000f;

            if (!FileUtil.HasWriteAccess(Instance.GetDirectory()))
            {
                Logger.Error(111, Translatable.Get("instances.no-permission"));
                return;
            }

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

                    if (file.Priority == 9)
                        file.ShaderPriority = Math.Min(9, shaderPriority);

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

                    if (file.ShaderPriority != 0)
                        Shaders.Add(file);

                    FilesinfosMods.Add(file);
                }

                priority++;
                shaderPriority += 0.0001f;
            }

            BackupAndUnpack();

            //Run all inserters
            await Task.WhenAll(Task.Run(() => ShaderInserter()), Task.Run(() => StartupXMLInserter(modIds)));

            Logger.Print(Translatable.Get("compiler.success"));
        }

        private async void ShaderInserter()
        {
            _ = new ShaderInserter(Instance.GetDirectory(), Shaders);
            PercentageCalculator.IncrementProgress(Shaders.Count);
        }

        private async void StartupXMLInserter(HashSet<string> modIds)
        {
            _ = new StartupInserter(Instance.GetDirectory(), StartupMods);
            XmlMods = XmlMods.OrderByDescending(mod => mod.Priority).ToList();
            FileMerger inserter = new FileMerger(Instance.GetDirectory(), XmlMods, modIds);
            if (XmlMods.Count > 0)
                FilesinfosMods.Add(new ModFile(XmlMods[0].ParentMod, "mods/PitCrewBase", 9));

            _ = new FilesInfosInserter(Instance.GetDirectory(), FilesinfosMods);

            PercentageCalculator.IncrementProgress(FilesinfosMods.Count);
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
                Logger.Error(110, Translatable.Get("compiler.missing-file"));
                throw new FileNotFoundException();
            }

            BigFileUtil.UnpackBigFile(unpackingFat, FileUtil.GetTempFolder("startup_temp"), Instance.PackageVersion, FileUtil.GetParentDir(Instance.GetDirectory()));

            //Create cstartup if not existing yet
            if (!File.Exists(cstartupFatPath))
            {
                File.Copy(startupFatPath, cstartupFatPath);
                File.Copy(startupDatPath, cstartupDatPath);
            }

            FileUtil.DisableReadonly(startupFatPath);
            FileUtil.DisableReadonly(startupDatPath);
        }
    }
}
