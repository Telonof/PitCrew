using PitCrewCommon;
using PitCrewCompiler;
using System.Reflection;

public class Program
{
    static void Main(string[] args)
    {
        string manifestFile;
        Dictionary<string, string> fileInfosData = new Dictionary<string, string>();

        if (args.Length < 1)
        {
            Console.WriteLine($"Usage: {Assembly.GetExecutingAssembly().GetName().Name} <file>");
            return;
        }

        manifestFile = args[0];

        if (!File.Exists(manifestFile))
        {
            Console.WriteLine("Unknown file.");
            return;
        }

        string? output = ManifestUtil.ValidateManifestFile(manifestFile);

        //Print back whatever error the validator gives back.
        if (output != null)
        {
            Console.WriteLine(output);
            return;
        }

        string[] lines = File.ReadAllLines(manifestFile);

        //Sort through all mods listed and send them for editing.
        foreach (string line in lines)
        {
            if (String.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            string[] parts = line.Split(' ');

            fileInfosData[parts[1]] = parts[0];
        }

        new DataInserter(fileInfosData, Path.GetDirectoryName(manifestFile));
    }
}
