using PitCrewCommon;

namespace PitCrewCompiler
{
    public class API
    {
        public static void compileManifest(string manifestFile)
        {
            Dictionary<string, string> fileInfosData = new Dictionary<string, string>();

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
}
