using PitCrewCommon;
using PitCrewCommon.Models;

namespace PitCrewCompiler
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            ConfigManager config = new ConfigManager();
            Translatable.Initialize(config.GetSetting(ConfigKey.Language) + ".json");
            Logger.EraseLog();

            if (args.Length < 1)
            {
                Console.WriteLine($"{Translatable.Get("compiler.usage")}: PitCrewCompiler.exe <{Translatable.Get("compiler.file")}>");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine(Translatable.Get("compiler.file-not-found"));
                return;
            }

            Instance instance = new Instance(args[0]);
            instance.LoadFromXML(true);
            
            Compile compile = new Compile(instance);
            compile.Run();
        }
    }
}