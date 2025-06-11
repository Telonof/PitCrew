using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Avalonia.Interactivity;
using PitCrew.Models;
using PitCrew.Views;
using PitCrewCommon;

namespace PitCrew.GUI;

public class RightClickTree
{
    private readonly MainWindow owner;

    public RightClickTree(MainWindow owner)
    {
        this.owner = owner;
    }

    //Dont show the context menu if no mod has been selected.
    public void ListBox_ContextMenuOpening(object sender, CancelEventArgs e)
    {
        if (IM.currentMod == null)
            e.Cancel = true;
    }

    public async void ListBox_PackageModItem_Click(object sender, RoutedEventArgs e)
    {
        string rootDirectory = Path.GetDirectoryName(IM.currentInstance.ManifestPath);
        string modMetadata = Path.Combine(rootDirectory, "pitcrewmetadata", $"{IM.currentMod.ID}.mdata");

        if (!File.Exists(modMetadata))
        {
            Utils.ShowDialog(owner, Translate.Get("modlist.packagemod.no-metadata"));
            return;
        }

        if (IM.modList[IM.currentMod].Count == 0)
        {
            Utils.ShowDialog(owner, Translate.Get("modlist.packagemod.empty-mod"));
            return;
        }

        List<string> lines = [];
        lines.Add(IM.currentMod.Name);
        lines.Add(IM.currentMod.Author);
        lines.Add(IM.currentMod.Description);

        var zip = new ZipArchive(File.Open($"{IM.currentMod.ID}.zip", FileMode.Create), ZipArchiveMode.Create);
        //Get all mods needed to be stored.
        foreach (FileEntry entry in IM.modList[IM.currentMod])
        {
            if (string.IsNullOrWhiteSpace(entry.ModPath))
                continue;

            string directName = Path.GetFileName(entry.ModPath);
            lines.Add($"{entry.Priority} {directName}");
            zip.CreateEntryFromFile(Path.Combine(rootDirectory, entry.ModPath + ".fat"), directName + ".fat");
            zip.CreateEntryFromFile(Path.Combine(rootDirectory, entry.ModPath + ".dat"), directName + ".dat");
        }

        //Apply new metadata
        var newMetadata = zip.CreateEntry($"{IM.currentMod.ID}.mdata");
        using (var writer = new StreamWriter(newMetadata.Open()))
        {
            writer.Write(string.Join(Environment.NewLine, lines));
        }
        zip.Dispose();

        MessageBox.Result result = await Utils.ShowDialog(owner, string.Format(Translate.Get("modlist.packagemod.success"), IM.currentMod.ID), MessageBox.ButtonType.YesNo);
        if (result != MessageBox.Result.OK)
            return;

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
        {
            FileName = Path.GetDirectoryName(Environment.ProcessPath),
            UseShellExecute = true
        });
    }

    public async void ListBox_EditMetadataItem_Click(object sender, RoutedEventArgs e)
    {
        await new ModWindow(IM.currentMod).ShowDialog(owner);
    }

    public async void ListBox_DeleteModItem_Click(object sender, RoutedEventArgs e)
    {
        string rootDirectory = Path.GetDirectoryName(IM.currentInstance.ManifestPath);
        MessageBox.Result result = await Utils.ShowDialog(owner, Translate.Get("modlist.deletemod-warning"), MessageBox.ButtonType.OkCancel);
        if (result != MessageBox.Result.OK)
            return;

        //Remove all files asscoiated with mod.
        File.Delete(Path.Combine(rootDirectory, "pitcrewmetadata", IM.currentMod.ID + ".mdata"));
        foreach (FileEntry entry in IM.modList[IM.currentMod])
        {
            if (string.IsNullOrWhiteSpace(entry.ModPath))
                continue;

            Utils.SetAllHashes(Path.Combine(rootDirectory, entry.ModPath + ".fat"), IM.currentMod, true);
            File.Delete(Path.Combine(rootDirectory, entry.ModPath + ".dat"));
            File.Delete(Path.Combine(rootDirectory, entry.ModPath + ".fat"));
        }

        //Remove from GUI and list
        IM.modList.Remove(IM.currentMod);
        IM.modListBox.setList(IM.modList.Keys.ToList());
        IM.allHashes.Remove(IM.currentMod);
        owner.FileList.ItemsSource = null;
        IM.currentMod = null;

        Utils.SaveFile();
    }
}