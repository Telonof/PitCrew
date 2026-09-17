using System.ComponentModel;
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

        public static bool CheckAndDeleteFolder(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                    Directory.Delete(folderPath, true);
            }
            catch (IOException)
            {
                return false;
            }

            return true;
        }

        public static void CheckAndDeleteFile(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public static string GetParentDir(string folderPath)
        {
            return Directory.GetParent(folderPath).FullName;
        }

        public static string GetProcessDir()
        {
            return Path.GetDirectoryName(Environment.ProcessPath);
        }

        public static void DisableReadonly(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            fileInfo.IsReadOnly = false;
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

        //Bad method, but i'm not sure of an easier cross plat version.
        public static bool HasWriteAccess(string path)
        {
            path = Path.Combine(path, "write_test");
            try
            {
                File.WriteAllBytes(path, [0xff]);

                if (!File.Exists(path))
                    return false;
                    
                CheckAndDeleteFile(path);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        public static bool ProcessRunning(string compareDirectory, int packageVersion)
        {
            string executable = "TheCrew";
            if (packageVersion == Constants.THE_CREW_2)
                executable += "2";
            
            if (OperatingSystem.IsLinux())
                executable += ".exe";

            Process[] processes = Process.GetProcessesByName(executable);

            //Double check if it's the same directory.
            foreach (Process process in processes)
            {
                try
                {
                    string path = Path.Combine(Path.GetDirectoryName(process.MainModule.FileName), "data_win32");
                    if (path.Equals(compareDirectory))
                        return true;
                }
                catch (Win32Exception)
                {
                    //Program we're not allowed to access.
                    continue;
                }
            }

            return false;
        }

        public static string GetTempFolder(string path)
        {
            return Path.Combine(Path.GetTempPath(), "pitcrew_temp", path);
        }
    }
}
