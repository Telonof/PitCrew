using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using PitCrew.Models;
using PitCrew.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PitCrewCommon;

namespace PitCrew.GUI;

internal class ModListBox
{
    public ObservableCollection<ModInfo> Keys { get; } = [];

    private readonly MainWindow owner;

    public ModListBox(MainWindow owner)
    {
        this.owner = owner;
    }

    public void setList(List<ModInfo> list)
    {
        Keys.Clear();

        foreach (ModInfo info in list)
        {
            Keys.Add(info);
        }
    }

    public void ListBox_Select(object sender, SelectionChangedEventArgs e)
    {
        if (owner.ModGUIList.SelectedItem is not ModInfo info)
            return;

        IM.currentMod = info;
        //If the last item isn't a blank option for adding, add one.
        if (!string.IsNullOrWhiteSpace(IM.modList[info].Last().ModPath))
            IM.modList[info].Add(new FileEntry());
		
        owner.FileList.ItemsSource = IM.modList[info];

        //Check mod conflicts
        List<string> modNames = [];

        if (!IM.allHashes.ContainsKey(info))
            IM.allHashes[info] = [];

        foreach (ulong hash in IM.allHashes[info])
        {
            foreach (var mod in IM.allHashes)
            {
                if (mod.Key == info)
                    continue;

                if (mod.Value.Contains(hash))
                    modNames.Add(mod.Key.Name);
            }
        }

        if (modNames.Count == 0)
        {
            owner.ConflictBox.Text = Translate.Get("conflictbox.no-conflicts");
            return;
        }

        owner.ConflictBox.Text = Translate.Get("conflictbox.conflicting") + Environment.NewLine + string.Join(Environment.NewLine, modNames);
    }

    public void ListBox_Drop(object sender, DragEventArgs e)
    {
        if (IM.currentInstance.ManifestPath == null)
            return;

        //Accept .zip or .mdata
        foreach (IStorageItem item in e.Data.GetFiles())
        {
            if (!item.Name.EndsWith(".mdata") && !item.Name.EndsWith(".zip"))
                continue;

            Utils.ImportMod(owner, item.Path.LocalPath);
        }
    }
}