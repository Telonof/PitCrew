using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using PitCrewCommon;
using PitCrew.Models;
using PitCrew.Views;

namespace PitCrew.GUI;

internal class FileGridList
{
    private readonly MainWindow owner;

    public FileGridList(MainWindow owner)
    {
        this.owner = owner;
    }

    public void OnCellSelect(object sender, DataGridCellPointerPressedEventArgs e)
    {
        if (sender is not DataGrid grid)
            return;

        string? strpriority = GetText(grid.Columns[1].GetCellContent(e.Row));
        string conflictText = Translate.Get("conflictbox.startupfile");

        if (strpriority.Equals("10"))
        {
            if (owner.ConflictBox.Text.Contains(conflictText))
                return;

            owner.ConflictBox.Text = $"{conflictText}\n\n" + owner.ConflictBox.Text;
        } else
        {
            if (!owner.ConflictBox.Text.Contains(conflictText))
                return;

            owner.ConflictBox.Text = owner.ConflictBox.Text.Substring(conflictText.Length + 2);
        }
    }

    public void OnCellEdit(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (sender is not DataGrid grid)
            return;

        //We only care if they are finish changing something, not cancelling.
        if (e.EditAction == DataGridEditAction.Cancel)
            return;

        //Get whatever the edited text is.
        string? path = GetText(grid.Columns[0].GetCellContent(e.Row));
        string? strpriority = GetText(grid.Columns[1].GetCellContent(e.Row));

        //If the priority is blank, cancel.
        if (e.Column.DisplayIndex == 1 && string.IsNullOrWhiteSpace(strpriority))
        {
            grid.CancelEdit();
            Utils.ShowDialog(owner, Translate.Get("filelist.priority-blank"));
            return;
        }

        //If the priority is not an integer, cancel.
        if (!int.TryParse(strpriority, out int priority))
        {
            grid.CancelEdit();
            Utils.ShowDialog(owner, Translate.Get("filelist.priority-requires-number"));
            return;
        }

        //If path is set to empty, delete row.
        if (e.Column.DisplayIndex == 0 && string.IsNullOrWhiteSpace(path))
        {
            if (priority != 0)
            {
                grid.CancelEdit();
                //Get the path again now that we restored the original text before it was wiped.
                path = GetText(grid.Columns[0].GetCellContent(e.Row));
                IM.modList[IM.currentMod].RemoveAt(e.Row.GetIndex());
                Utils.SetAllHashes(Path.Combine(Path.GetDirectoryName(IM.currentInstance.ManifestPath), path + ".fat"), IM.currentMod, true);
            }
            return;
        }

        //Do not allow editing 2nd column if 1st is empty.
        if (e.Column.DisplayIndex == 1 && string.IsNullOrWhiteSpace(path))
        {
            grid.CancelEdit();
            Utils.ShowDialog(owner, Translate.Get("filelist.priority-no-edit-before-path"));
            return;
        }

        //Do not allow priorities lower than 10.
        if (e.Column.DisplayIndex == 1 && priority < 10)
        {
            grid.CancelEdit();
            Utils.ShowDialog(owner, Translate.Get("filelist.priority-less-than-ten"));
            return;
        }

        string combinedPath = Path.Combine(Path.GetDirectoryName(IM.currentInstance.ManifestPath), path);

        //Check if first column file path is correct.
        if (e.Column.DisplayIndex == 0 && !ManifestUtil.CheckForValidFile(combinedPath))
        {
            grid.CancelEdit();
            Utils.ShowDialog(owner, Translate.Get("filelist.unable-to-find") + " " + path);
            return;
        }

        //Check if any other mod is using this file and cancel if another is.
        foreach (ModInfo info in IM.modList.Keys)
        {
            bool exists = IM.modList[info]
                .Where((entry, index) => info != IM.currentMod || info == IM.currentMod && index != e.Row.GetIndex())
                .Any(entry => !string.IsNullOrEmpty(entry.ModPath) && entry.ModPath.Replace("\\", "/").Equals(path.Replace("\\", "/")));

            if (exists)
            {
                grid.CancelEdit();
                Utils.ShowDialog(owner, string.Format(Translate.Get("filelist.already-in-use"), info.ID));
                return;
            }
        }

        Utils.SetAllHashes(combinedPath + ".fat", IM.currentMod);

        //Auto set priority to 998 if success and add a new empty row.
        if (e.Column.DisplayIndex == 0 && priority == 0)
        {
            ((TextBlock)grid.Columns[1].GetCellContent(e.Row)).Text = "998";
            IM.modList[IM.currentMod].Add(new FileEntry());
        }
    }

    public void OnCellDelete(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete && e.Key != Key.Back)
            return;

        if (sender is not DataGrid grid)
            return;

        if (string.IsNullOrWhiteSpace(((FileEntry)grid.SelectedItem).ModPath))
            return;

        IM.modList[IM.currentMod].RemoveAt(grid.SelectedIndex);
        Utils.SetAllHashes(Path.Combine(Path.GetDirectoryName(IM.currentInstance.ManifestPath), ((FileEntry)grid.SelectedItem).ModPath + ".fat"), IM.currentMod, true);
        grid.SelectedItem = null;
    }

    private string? GetText(Control cell)
    {
        if (cell is TextBlock textBlock)
            return textBlock.Text;

        if (cell is TextBox textBox)
            return textBox.Text;

        return null;
    }
}
