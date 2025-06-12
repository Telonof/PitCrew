using PitCrewCommon;

namespace PitCrewCompiler
{
    public class API
    {
        public static void compileManifest(string manifestFile)
        {
            Dictionary<string, string> fileInfosData = [];
            List<string> startupMods = [];
            Dictionary<string, int> xmlFiles = [];

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

                if (parts[1].EndsWith(".xml"))
                {
                    //Priority should be validated from check above
                    int.TryParse(parts[0], out int priority);
                    xmlFiles.Add(parts[1], priority);
                    continue;
                }

                fileInfosData[parts[1]] = parts[0];
            }

            if (xmlFiles.Count > 0)
            {
                List<string> sortedFiles = xmlFiles.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
                new BinaryInserter(manifestFile, sortedFiles);
                fileInfosData["PitCrewBase"] = "11";
            }
            _ = new StartupReplacer(Path.GetDirectoryName(manifestFile), startupMods);
            _ = new DataInserter(fileInfosData, Path.GetDirectoryName(manifestFile));
        }
    }
}
