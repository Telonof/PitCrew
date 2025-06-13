using PitCrewCommon;
using PitCrewCompiler;

public class Program
{
    internal static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine($"Usage: {Path.GetFileName(Environment.ProcessPath)} <file>");
            return;
        }

        string manifestFile = args[0];
        Translate.Initialize("English.json");

        if (!File.Exists(manifestFile))
        {
            Console.WriteLine(Translate.Get("compiler.unknown-file"));
            return;
        }

        API.compileManifest(manifestFile);
    }
}
