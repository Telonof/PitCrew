using Avalonia.Controls;
using Avalonia.Interactivity;
using PitCrew.GUI;
using PitCrew.Models;
using PitCrewCommon;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PitCrew.Views;

internal partial class ModWindow : Window
{
    private readonly bool inEdit = false;

    public ModWindow(ModInfo? modInfo = null)
    {
        InitializeComponent();
        
        //Change to edit mode.
        if (modInfo != null)
        {
            IDTextbox.IsEnabled = false;
            SubmitButton.Content = Translate.Get("modedit.edit");
            IDTextbox.Text = modInfo.ID;
            TitleTextbox.Text = modInfo.Name;
            AuthorTextbox.Text = modInfo.Author;
            DescriptionTextbox.Text = modInfo.Description;
            inEdit = true;
        }
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        //Ensure no line is empty.
        string[]? list = ValidateModInputs();

        if (list == null)
            return;

        //Creating new metadata
        if (!inEdit)
        {
            //Append author to file.
            list[0] = $"{list[2].Replace(" ", "_")}_{list[0].Replace(" ", "")}";
            //Windows...
            list[0] = Regex.Replace(list[0], @"[<>:""/\\|?* ]+", "_");

            if (!GetNewModInfo(list))
            {
                Utils.ShowDialog(this, Translate.Get("modedit.mod-exists"));
                return;
            }

            Close();
            return;
        }

        //Editing existing one.
        WriteToFile(list, true);
        List<FileEntry> entries = IM.modList[IM.currentMod];
        IM.modList.Remove(IM.currentMod);
        IM.currentMod.Name = list[1];
        IM.currentMod.Author = list[2];
        IM.currentMod.Description = list[3];

        IM.modList[IM.currentMod] = entries;
        IM.modListBox.setList(IM.modList.Keys.ToList());
        Close();

    }

    private bool GetNewModInfo(string[] modInfo)
    {
        if (!WriteToFile(modInfo))
            return false;
        
        ModInfo info = new ModInfo();
        info.ID = modInfo[0];
        info.Name = modInfo[1];
        info.Author = modInfo[2];
        info.Description = modInfo[3];
        IM.modList[info] = [];
        IM.modList[info].Add(new FileEntry());
        IM.modListBox.setList(IM.modList.Keys.ToList());
        return true;
    }

    private bool WriteToFile(string[] modInfo, bool overwrite = false)
    {
        string path = Path.Combine(Path.GetDirectoryName(IM.currentInstance.ManifestPath), "pitcrewmetadata");
        FileUtil.checkAndCreateFolder(path);

        string modMetadataPath = Path.Combine(path, modInfo[0] + ".mdata");

        if (!overwrite && File.Exists(modMetadataPath))
            return false;

        using (StreamWriter newFile = new StreamWriter(modMetadataPath))
        {
            newFile.WriteLine(modInfo[1]);
            newFile.WriteLine(modInfo[2]);
            newFile.WriteLine(modInfo[3]);
        }
        return true;
    }

    private string[]? ValidateModInputs()
    {
        string[] list = [IDTextbox.Text, TitleTextbox.Text, AuthorTextbox.Text, DescriptionTextbox.Text];

        if (IDTextbox.Text.Any(char.IsWhiteSpace))
        {
            Utils.ShowDialog(this, Translate.Get("modedit.no-whitespace-allowed"));
            return null;
        }

        foreach (string input in list)
        {
            if (!string.IsNullOrWhiteSpace(input))
                continue;

            Utils.ShowDialog(this, Translate.Get("modedit.no-blank-fields"));
            return null;
        }

        return list;
    }
}
