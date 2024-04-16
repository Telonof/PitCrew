using PitCrewCompiler;
using System.Reflection;

public class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine($"Usage: {Assembly.GetExecutingAssembly().GetName().Name} <file>");
            return;
        }

        string manifestFile = args[0];

        if (!File.Exists(manifestFile))
        {
            Console.WriteLine("Unknown file.");
            return;
        }

        API.compileManifest(manifestFile);
    }
}
