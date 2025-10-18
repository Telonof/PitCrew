using System.Diagnostics;

namespace PitCrewCommon.Utilities
{
    public class FileUtil
    {
        public static void CheckAndCreateFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }

        public static void CheckAndDeleteFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
                Directory.Delete(folderPath, true);
        }

        public static void CheckAndDeleteFile(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public static bool FileInUse(string path)
        {
            if (!File.Exists(path))
                return false;

            try
            {
                using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

        public static bool ProcessRunning(string compareDirectory, int packageVersion)
        {
            string executable = "TheCrew";
            if (packageVersion == 6)
                executable += "2";

            Process[] processes = Process.GetProcessesByName(executable);

            //Double check if it's the same directory.
            foreach(Process process in processes)
            {
                string path = Path.Combine(Path.GetDirectoryName(process.MainModule.FileName), "data_win32");
                if (path.Equals(compareDirectory))
                    return true;
            }

            return false;
        }
    }
}
