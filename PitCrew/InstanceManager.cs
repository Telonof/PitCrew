using System.Collections.Generic;
using System.IO;
using System.Linq;
using PitCrew.GUI;
using PitCrew.Models;
using PitCrew.Views;

namespace PitCrew;

public class IM
{
    internal static Dictionary<ModInfo, List<FileEntry>> modList { get; set; } = [];
    internal static ModListBox modListBox { get; set; }
    internal static FileGridList fileGridList { get; set; }
    internal static ModInfo currentMod { get; set; }
    internal static List<Instance> instances { get; set; }
    internal static Instance currentInstance { get; set; }
    internal static ConfigManager config { get; set; }
    internal static Dictionary<ModInfo, List<ulong>> allHashes { get; set; } = [];

    internal void Initialize(MainWindow owner, ConfigManager config)
    {
        modListBox = new ModListBox(owner);
        fileGridList = new FileGridList(owner);
        IM.config = config;
        instances = config.LoadInstances();
        //Load last manifest opened
        Instance foundInstance = instances.FirstOrDefault(instance => instance.ManifestPath == config.GetSetting("LastOpenedPath"));
        if (foundInstance != null && File.Exists(foundInstance.ManifestPath))
            Utils.LoadManifest(foundInstance, owner);  
    }
}
