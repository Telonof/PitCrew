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
    }
}
