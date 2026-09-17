using System.Runtime.InteropServices;
using PitCrewCommon.Utilities;

namespace PitCrewCommon;

public class CrashLogger
{
    private static string _crashFolder = Path.Combine(FileUtil.GetProcessDir(), "crash-reports");
    
    
    public static void Init()
    {
        if (!FileUtil.HasWriteAccess(FileUtil.GetProcessDir()))
        {
            Logger.Error(211, Translatable.Get("config.no-permission"));
            return;
        }
        
        FileUtil.CheckAndCreateFolder(_crashFolder);
    }

    public static string DumpCrash(string errorMessage, string stacktrace)
    {
        if (!FileUtil.HasWriteAccess(FileUtil.GetProcessDir()))
        {
            Logger.Error(211, Translatable.Get("config.no-permission"));
            return string.Empty;
        }
        
        string[] crashLog = new string[6];
        DateTime currentTime = DateTime.Now;
        string fileName = $"pitcrew-crash-{currentTime.ToString("yyyyMMddHH-mm-ss")}.txt";

        crashLog[0] = "CRASHDUMP";
        crashLog[1] = $"Version: {Constants.VERSION}";
        crashLog[2] = $"OS: {RuntimeInformation.OSDescription}";
        crashLog[3] = "";
        crashLog[4] = errorMessage;
        crashLog[5] = stacktrace;

        File.WriteAllLines(Path.Combine(_crashFolder, fileName), crashLog);
        return fileName;
    }
}