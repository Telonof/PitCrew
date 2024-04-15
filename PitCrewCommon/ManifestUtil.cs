namespace PitCrewCommon
{
    public class ManifestUtil
    {
        static Dictionary<string, string> scannedFiles = new Dictionary<string, string>();

        public static string? ValidateManifestFile(string fileName)
        {
            string folderPath = Path.GetDirectoryName(fileName);

            if (!folderPath.EndsWith("data_win32"))
            {
                return "File not in the data_win32 folder of The Crew.";
            }

            string[] lines = File.ReadAllLines(fileName);

            int count = 1;
            foreach (string line in lines)
            {
                if (String.IsNullOrWhiteSpace(line) || line.StartsWith("##"))
                    continue;

                string[] parts = line.Split(' ');

                //Assume bad line and ignore.
                if (parts.Length < 2)
                    continue;

                //Disabled doesn't matter for validating file.
                if (parts[0].StartsWith('#'))
                {
                    parts[0] = parts[0].Substring(1);
                }

                if (!int.TryParse(parts[0], out int priority))
                {
                    return $"Line {count} is invalid: Priority cannot be lower than 11.";
                }

                if (priority < 11)
                {
                    return $"Line {count} is invalid: Priority cannot be lower than 11.";
                }

                //This would detect .dat as well, but based on good faith the user should have both .fat and .dat in their folder.
                string filePath = Path.Combine(folderPath, parts[1] + ".fat");

                if (!File.Exists(filePath))
                {
                    return $"Line {count} is invalid: Cannot find file {filePath.Replace("\\", "/")}";
                }

                string group = parts.Length > 2 ? parts[2] : "Default";

                //The same file should never be listed twice.
                if (scannedFiles.ContainsKey(parts[1]))
                {
                    return $"{group} and {scannedFiles[parts[1]]} are conflicting with {parts[1]}";
                }

                scannedFiles[parts[1]] = group;
                count++;              
            }

            return null;
        }
    }
}
