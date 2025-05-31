using PitCrewCommon;

namespace PitCrewCompiler
{
    public class API
    {
        public static void compileManifest(string manifestFile)
        {
            Dictionary<string, string> fileInfosData = new Dictionary<string, string>();
            List<string> startupMods = [];

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
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                    continue;

                string[] parts = line.Split(' ');

                //If priority is ten, assume it's a mod for the startup file and send to a different handler
                if (parts[0] == "10")
                {
                    startupMods.Add(parts[1]);
                    continue;
                }

                fileInfosData[parts[1]] = parts[0];
            }

            _ = new StartupReplacer(Path.GetDirectoryName(manifestFile), startupMods);
            _ = new DataInserter(fileInfosData, Path.GetDirectoryName(manifestFile));
        }
    }
}
