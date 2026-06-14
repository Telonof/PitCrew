using PitCrewCommon.Models;

namespace PitCrewCommon.Utilities
{
    public class ManifestUtil
    {
        public static bool IsValidMod(Mod parentMod, string rootDirectory, string fileLocation, string strPriority, bool import = false)
        {
            string issue = string.Format(Translatable.Get("parser.issue-found"), parentMod.Id, fileLocation);

            if (string.IsNullOrWhiteSpace(fileLocation) || string.IsNullOrWhiteSpace(strPriority))
            {
                Logger.Error(202, $"{issue} {Translatable.Get("parser.empty-string")}");
                return false;
            }

            if (!int.TryParse(strPriority, out int priority))
            {
                Logger.Error(203, $"{issue} {Translatable.Get("parser.not-integer")}");
                return false;
            }

            if (priority < 9)
            {
                Logger.Error(204, $"{issue} {string.Format(Translatable.Get("parser.too-low-priority"), 9)}");
                return false;
            }

            string fullPath = Path.Combine(rootDirectory, fileLocation);

            if (!import && !IsValidFile(fullPath))
            {
                Logger.Error(205, $"{issue} {Translatable.Get("compiler.file-not-found")}");
                return false;
            }

            return true;
        }

        public static bool IsValidFile(string item)
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

        public static int GetPackageVersion(string dir)
        {
            if (File.Exists(Path.Combine(dir, Constants.OODLE_FILE)))
                return 6;

            return 5;
        }

        public static string FormatVersion(string version = Constants.VERSION)
        {
            int extraInfoIndex = version.IndexOf('-') + 1;
            int start = version.StartsWith('v') ? 1 : 0;

            return version[start..^extraInfoIndex];
        }

        /// <summary>
        /// Returns 0 if input is equal.<br></br>
        /// Returns 1 if input is higher.<br></br>
        /// Returns -1 is input is lower.
        /// </summary>
        public static int CompareVersion(string inputVersion)
        {
            if (string.IsNullOrWhiteSpace(inputVersion))
                return -1;

            Version currentVersion = Version.Parse(FormatVersion());
            Version test = Version.Parse(FormatVersion(inputVersion));

            return test.CompareTo(currentVersion);
        }
    }
}
