using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace PitCrewCommon
{
    public class ProcessUtil
    {
        public static Dictionary<string, string> GetConfig()
        {
            Dictionary<string, string> config = new Dictionary<string, string>();
            //Set default config options
            if (!File.Exists("config.txt"))
            {
                config["unpacker"] = "tools\\Gibbed.Dunia2.Unpack.exe";
                config["packer"] = "tools\\Gibbed.Dunia2.Pack.exe";
                return config;
            }

            string[] lines = File.ReadAllLines("config.txt");

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] configLine = line.Split('=');
                config[configLine[0]] = configLine[1];
            }
            return config;
        }

        public static bool ProgramExecution(string tool, string arguments)
        {
            if (!File.Exists(tool))
                return false;

            ProcessStartInfo startInfo = new ProcessStartInfo(tool);
            startInfo.Arguments = arguments;

            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            return true;
        }
    }
}
