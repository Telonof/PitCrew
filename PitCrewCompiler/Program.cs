using PitCrewCommon;
using PitCrewCommon.Models;
using PitCrewCommon.Utilities;

namespace PitCrewCompiler
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            ConfigManager config = new ConfigManager();
            Translatable.Initialize(config.GetSetting(ConfigKey.Language) + ".json");
            //legacy stuff, force updating language files
            config.SetSetting(ConfigKey.Language, Translatable.GetCurrentLanguage());

            Logger.EraseLog();

            if (args.Length < 1)
            {
                Console.WriteLine($"{Translatable.Get("compiler.usage")}: {Path.GetFileName(Environment.ProcessPath)} <{Translatable.Get("compiler.file")}>");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Logger.Error(107, Translatable.Get("compiler.file-not-found"));
                return;
            }

            //Load instance first to grab package version.
            Instance instance = new Instance(args[0]);
            if (!instance.LoadFromXML(true))
                return;

            if (FileUtil.ProcessRunning(Path.GetDirectoryName(args[0]), instance.PackageVersion))
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