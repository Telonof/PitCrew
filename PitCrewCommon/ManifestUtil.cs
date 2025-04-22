namespace PitCrewCommon
{
    public class ManifestUtil
    {
        static Dictionary<string, string> scannedFiles = [];

        public static string? ValidateManifestFile(string fileName)
        {
            //Ensure scannedFiles is reset each time.
            scannedFiles = [];
            string? folderPath = Path.GetDirectoryName(fileName);

            if (folderPath == null)
                return Translate.Get("manifestutil.invalid-folder");

            if (!folderPath.EndsWith("data_win32"))
                return Translate.Get("manifestutil.invalid-location");

            if (!CheckForValidFile(Path.Combine(folderPath, "startup")))
                return Translate.Get("manifestutil.no-startup-file");

            string[] lines = File.ReadAllLines(fileName);

            int count = 1;
            foreach (string line in lines)
            {
                //Ignore comments and blanks.
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("##"))
                    continue;

                string[] parts = line.Split(' ');

                //Assume bad line and ignore.
                if (parts.Length < 2)
                    continue;

                //Validating a manifest has to ensure disabled mods are also correct.
                if (parts[0].StartsWith('#'))
                    parts[0] = parts[0].Substring(1);

                if (!int.TryParse(parts[0], out int priority))
                    return string.Format(Translate.Get("manifestutil.invalid-priority"), count);

                if (priority < 11)
                    return string.Format(Translate.Get("manifestutil.too-low-priority"), count);

                string filePath = Path.Combine(folderPath, parts[1]);

                if (!CheckForValidFile(filePath))
                    return string.Format(Translate.Get("manifestutil.cannot-find-file"), count, filePath.Replace("\\", "/"));

                if (filePath.EndsWith(".xml"))
                    continue;

                string group = parts.Length > 2 ? parts[2] : "Default";

                //The same file should never be listed twice.
                if (scannedFiles.TryGetValue(parts[1], out string? value))
                    return string.Format(Translate.Get("manifestutil.conflict-found"), group, value, parts[1]);

                scannedFiles[parts[1]] = group;
                count++;              
            }

            return null;
        }

        //When checking a big file, we want both .fat and .dat
        public static bool CheckForValidFile(string item)
        {
            if (item.EndsWith(".xml"))
            {
                if (File.Exists(item))
                    return true;

                return false;
            }

            string[] letters = ["f", "d"];
            foreach (string s in letters)
            {
                if (!File.Exists($"{item}.{s}at"))
                    return false;
            }
            return true;
        }
    }
}
