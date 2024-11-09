using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using PitCrewCommon;
using PitCrew.Models;
using PitCrew.Views;

namespace PitCrew.GUI;

internal class Utils
{
    public static void SaveFile()
    {
        if (IM.currentInstance == null)
            return;

        Dictionary<ModInfo, List<FileEntry>> newList = [];
        List<ModInfo> updatedModInfos = IM.modListBox.Keys.ToList();
        string filePath = IM.currentInstance.ManifestPath;
        List<string> textLines = [];
        if (filePath == null)
            return;

        //Set the original mod list to be updated.
        for (int i = 0; i < updatedModInfos.Count; i++)
        {
            newList[updatedModInfos[i]] = IM.modList.ElementAt(i).Value;
        }
        IM.modList = newList;

        foreach (ModInfo key in IM.modListBox.Keys)
        {
            foreach (FileEntry entry in IM.modList[key])
            {
                if (string.IsNullOrWhiteSpace(entry.ModPath))
                    continue;

                string text = $"{entry.Priority} {entry.ModPath} {key.ID}";

                if (!key.Enabled)
                    text = "#" + text;

                textLines.Add(text);
            }
        }

        File.WriteAllLines(filePath, textLines);
    }

    public async static void ImportMod(MainWindow owner, string? path = null)
    {

        string manifestLoc = IM.currentInstance.ManifestPath;
        string mainFolder = Path.GetDirectoryName(manifestLoc);

		//If not dragged and dropped, prompt for mod file.
        if (path == null)
        {
            var files = await owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = Translate.Get("importmod.filechooser.title"),
                AllowMultiple = false,
                FileTypeFilter = [Utils.CustomFileOptions(Translate.Get("importmod.filechooser.filefilter"), ["*.mdata", "*.zip"])]
            });

            if (files.Count == 0)
                return;

            path = files[0].Path.LocalPath;
        }

        if (!path.EndsWith(".zip") && !path.EndsWith(".mdata"))
        {
            await ShowDialog(owner, Translate.Get("importmod.invalid-extension"));
            return;
        }

        string zipTempFolder = "!PitCrewZipTempFolder";

		//Validate and extract zip to a temporary folder.
        if (path.EndsWith(".zip"))
        {
            bool validmodzip = false;
            string mdataName = "";
            using (ZipArchive archive = ZipFile.OpenRead(path))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (!Path.GetExtension(entry.FullName).Equals(".mdata", StringComparison.OrdinalIgnoreCase))
                        continue;

                    validmodzip = true;
                    mdataName = entry.Name;
                    break;
                }
            }
            if (!validmodzip)
            {
                await ShowDialog(owner, Translate.Get("importmod.invalid-zip"));
                return;
            }

            Directory.CreateDirectory(zipTempFolder);
            ZipFile.ExtractToDirectory(path, zipTempFolder);
            path = Path.Combine(zipTempFolder, mdataName);
        }

		//Create new mod info and overwrite if existing
        string[] lines = File.ReadAllLines(path);
        string fileName = Path.GetFileNameWithoutExtension(path);
        string newFilePath;
        ModInfo info = new ModInfo();
        info.ID = fileName;
        info.Name = lines[0];
        info.Author = lines[1];
        info.Description = lines[2];
        ModInfo modExists = IM.modList.FirstOrDefault(modInfo => modInfo.Key.ID == info.ID).Key;
        if (modExists != null)
        {
            MessageBox.Result result = await ShowDialog(owner, Translate.Get("importmod.overwrite-old-mod"), MessageBox.ButtonType.YesNo);
            if (result == MessageBox.Result.Cancel || result == MessageBox.Result.None)
            {
                checkAndDeleteFolder(zipTempFolder);
                return;
            }
            IM.modList.Remove(modExists);
            IM.allHashes.Remove(modExists);
        }
        IM.modList[info] = [];

        //Generate needed directories
        checkAndCreateFolder(Path.Combine(mainFolder, "mods", info.ID));
        checkAndCreateFolder(Path.Combine(mainFolder, "pitcrewmetadata"));

		//For each mod in the .mdata, copy over to the relative location set by the mdata
        foreach (string line in lines.Skip(3))
        {
            string[] separator = line.Split(' ');
            newFilePath = Path.Combine(mainFolder, "mods", info.ID, separator[1]);

            /** Modders should be smart enough not to name their files similar to other mods, there's enough failsafes as is
            if (ManifestUtil.CheckForBigFile(newFilePath))
            {
                checkAndDeleteFolder(zipTempFolder);
                await ShowDialog(owner, string.Format(Translate.Get("importmod.file-in-use"), separator[1]));
                return;
            }
            */

            File.Copy(Path.Combine(Path.GetDirectoryName(path), separator[1] + ".fat"), newFilePath + ".fat", true);
            File.Copy(Path.Combine(Path.GetDirectoryName(path), separator[1] + ".dat"), newFilePath + ".dat", true);

            FileEntry entry = new FileEntry
            {
                Priority = Convert.ToInt32(separator[0]),
                ModPath = Path.Combine("mods", info.ID, separator[1])
            };
            
            Utils.SetAllHashes(newFilePath + ".fat", info);
            IM.modList[info].Add(entry);
        }
        IM.modList[info].Add(new FileEntry());

        newFilePath = Path.Combine(mainFolder, "pitcrewmetadata", Path.GetFileName(path));
        if (!File.Exists(newFilePath))
            File.Copy(path, newFilePath, true);

        IM.modListBox.setList(IM.modList.Keys.ToList());
        owner.ConflictBox.Text = Translate.Get("conflictbox.no-conflicts");
        checkAndDeleteFolder(zipTempFolder);
        //Just in case people close out immeditealy.
        SaveFile();
    }

    public static void LoadManifest(Instance instance, MainWindow owner)
    {
        SaveFile();
        //Delete old manifest in path if it doesn't exist.
        if (!File.Exists(instance.ManifestPath))
        {
            IM.config.SetSetting("LastOpenedPath", "");
            return;
        }
        //Check if the manifest being loaded is valid.
        string? output = ManifestUtil.ValidateManifestFile(instance.ManifestPath);
        if (output != null)
        {
            Utils.ShowDialog(owner, output);
            return;
        }
        //Reset all active variables.
        IM.modList.Clear();
        IM.allHashes.Clear();
        IM.currentMod = null;
        owner.FileList.ItemsSource = null;
        owner.ModsMenu.IsVisible = true;
        IM.currentInstance = instance;
        owner.ModGUIList.BorderBrush = Brushes.SkyBlue;

        //Load all mods from the instance
        string[] lines = File.ReadAllLines(instance.ManifestPath);
        string rootDirectory = Path.GetDirectoryName(instance.ManifestPath);
        string metadatafolder = Path.Combine(rootDirectory, "pitcrewmetadata");
        Utils.checkAndCreateFolder(metadatafolder);
        foreach (string line in lines)
        {
            ModInfo modInfo = new ModInfo();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("##"))
                continue;

            string[] parts = line.Split(' ');

            if (parts[0].StartsWith('#'))
            {
                modInfo.Enabled = false;
                parts[0] = parts[0].Substring(1);
            }

            string groupName = parts.Length > 2 ? parts[2] : "Default";

            //Get mod info
            modInfo.ID = groupName;

            //If anything in the dictionary for this group exists, just add to the file entries.
            bool modExists = false;
            foreach (ModInfo exisitingMods in IM.modList.Keys)
            {
                if (exisitingMods.ID == modInfo.ID)
                {
                    IM.modList[exisitingMods].Add(new FileEntry() { ModPath = parts[1], Priority = int.Parse(parts[0]) });
                    SetAllHashes(Path.Combine(rootDirectory, parts[1] + ".fat"), exisitingMods);
                    modExists = true;
                    break;
                }
            }
            if (modExists)
            {
                continue;
            }

            modExists = false;
            //Scan to see if group is existing, fill info with rest of the data. 
            foreach (string file in Directory.GetFiles(metadatafolder))
            {
                if (Path.GetFileName(file).Equals(groupName + ".mdata", StringComparison.OrdinalIgnoreCase))
                {
                    string[] infoLines = File.ReadAllLines(file);
                    modInfo.Name = infoLines[0];
                    modInfo.Author = infoLines[1];
                    modInfo.Description = infoLines[2];
                    IM.modList[modInfo] = [new FileEntry() { ModPath = parts[1], Priority = int.Parse(parts[0]) }];
                    SetAllHashes(Path.Combine(rootDirectory, parts[1] + ".fat"), modInfo);
                    modExists = true;
                    break;
                }
            }
            //Dump the remaining files into the unsorted group.
            if (!modExists)
            {
                if (!IM.modList.ContainsKey(modInfo))
                    IM.modList[modInfo] = [];

                IM.modList[modInfo].Add(new FileEntry() { ModPath = parts[1], Priority = int.Parse(parts[0]) });
                SetAllHashes(Path.Combine(rootDirectory, parts[1] + ".fat"), modInfo);
            }
        }

        owner.ConflictBox.Text = Translate.Get("conflictbox.no-conflicts");
        if (IM.modList.Count == 0)
            owner.ConflictBox.Text = Translate.Get("conflictbox.no-mods");
        
        IM.modListBox.setList(IM.modList.Keys.ToList());
    }

	//Get all name hashes and add/remove them to the global list.
    public static void SetAllHashes(string file, ModInfo info, bool remove = false)
    {
        const int headerSize = 12;
        const int loopCountSize = 4;
        const int dataBlockSize = 8;

        if (!IM.allHashes.ContainsKey(info))
            IM.allHashes[info] = [];

        using (FileStream stream = new FileStream(file, FileMode.Open))
        {
            stream.Seek(headerSize, SeekOrigin.Begin);

            byte[] loopCountBytes = new byte[loopCountSize];
            stream.Read(loopCountBytes, 0, loopCountSize);
            uint loopCount = BitConverter.ToUInt32(loopCountBytes, 0);

            for (int i = 0; i < loopCount; i++)
            {
                stream.Seek(dataBlockSize, SeekOrigin.Current);

                byte[] dataBytes = new byte[dataBlockSize];
                stream.Read(dataBytes, 0, dataBlockSize);
                ulong data = BitConverter.ToUInt64(dataBytes);

                if (IM.allHashes[info].Contains(data))
                    continue;

                if (remove)
                    IM.allHashes[info].Remove(data);
                else
                    IM.allHashes[info].Add(data);

                stream.Seek(dataBlockSize, SeekOrigin.Current);
            }
        }
    }

    public static void checkAndCreateFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
    }

    //This should only be called regarding the deletion of a temporary folder, at least until this has use elsewhere.
    private static void checkAndDeleteFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
            Directory.Delete(folderPath, true);
    }

    public static FilePickerFileType CustomFileOptions(string name, string[] pattern)
    {
        return new FilePickerFileType(name) { Patterns = pattern };
    }

    public static Task<MessageBox.Result> ShowDialog(Window owner, string message, MessageBox.ButtonType buttonType = MessageBox.ButtonType.Ok, string title = "Info")
    {
        return new MessageBox().ShowDialog(owner, message, buttonType, title);
    }
}
