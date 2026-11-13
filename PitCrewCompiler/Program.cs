using PitCrewCommon;
using PitCrewCommon.Models;
using PitCrewCommon.Utilities;

namespace PitCrewCompiler
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Logger.EraseLog();
            ConfigManager config = new ConfigManager();
            Translatable.Initialize(config.GetSetting(ConfigKey.Language) + ".json");
            //legacy stuff, force updating language files
            config.SetSetting(ConfigKey.Language, Translatable.GetCurrentLanguage());
            
            //Check for last opened path
            string? lastPath = config.GetSetting(ConfigKey.LastOpenedPath);
            if (string.IsNullOrWhiteSpace(lastPath) || !File.Exists(lastPath))
            {
                Logger.Error(113, Translatable.Get("instances.invalid-manifest"));
                lastPath = null;
            }

            if (args.Length < 1 && string.IsNullOrWhiteSpace(lastPath))
            {
                Console.WriteLine($"{Translatable.Get("compiler.usage")}: {Path.GetFileName(Environment.ProcessPath)} <{Translatable.Get("compiler.file")}>");
                return;
            }

            if (string.IsNullOrWhiteSpace(lastPath) && !File.Exists(args[0]))
            {
                Logger.Error(107, Translatable.Get("compiler.file-not-found"));
                return;
            }

            lastPath ??= args[0];

            //Load instance first to grab package version.
            Instance instance = new Instance(lastPath);
            if (instance.LoadFromXML(true) != 0)
                return;

            if (FileUtil.ProcessRunning(Path.GetDirectoryName(lastPath), instance.PackageVersion))
            {
                Logger.Error(108, Translatable.Get("game-running"));
                return;
            }
            
            Compile compile = new Compile(instance);

            try
            {
                compile.Run().Wait();
                Logger.WriteAndFlush();
            }
            catch (Exception ex)
            {
                CrashLogger.Init();
                Logger.Error(0,$"CRASH: {CrashLogger.DumpCrash(ex.Message, ex.StackTrace)}");
                Logger.WriteAndFlush();
            }
        }
    }
}