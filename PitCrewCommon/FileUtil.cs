namespace PitCrewCommon
{
    public class FileUtil
    {
        public static void checkAndCreateFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }

        public static void checkAndDeleteFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
                Directory.Delete(folderPath, true);
        }

        public static void checkAndDeleteFile(string filePath)
        {
            if (Directory.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
