namespace PitCrew.GUI.MainWindow.Components
{
    internal class FileGridList : BaseComponent
    {
        DataGridView grid;

        public FileGridList(MainForm form) : base(form)
        {
            base.form = form;
            grid = form.dataGridView;
        }

        public void Grid_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell editedCell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];

            if (editedCell.Value == null)
            {
                removeRow(grid.Rows[e.RowIndex], e.RowIndex);
                return;
            }

            switch (e.ColumnIndex)
            {
                case 0:
                    if (!File.Exists(Path.Combine(Path.GetDirectoryName(form.manifestLoc), editedCell.Value.ToString() + ".fat")))
                    {
                        MessageBox.Show("Unable to find file.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        if (form.modList[form.currentMod].Count() <= e.RowIndex)
                        {
                            editedCell.Value = "";
                            grid.Rows.RemoveAt(grid.Rows.Count - 2);
                        }
                        else
                        {
                            editedCell.Value = form.modList[form.currentMod][e.RowIndex].modPath;
                        }
                        break;
                    }

                    foreach (FileEntry file in form.modList.Values.SelectMany(x => x))
                    {
                        if (!file.modPath.Equals(editedCell.Value))
                            continue;

                        MessageBox.Show("File already being used by another mod.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        if (form.modList[form.currentMod].Count() <= e.RowIndex)
                        {
                            editedCell.Value = "";
                            grid.Rows.RemoveAt(grid.Rows.Count - 2);
                        }
                        else
                        {
                            editedCell.Value = form.modList[form.currentMod][e.RowIndex].modPath;
                        }
                        return;
                    }

                    UpdateCell(e.RowIndex, editedCell.Value.ToString());

                    //Update conflicts
                    if (!form.modifiedFiles.ContainsKey(form.currentMod))
                        form.modifiedFiles[form.currentMod] = new List<string>();

                    Utils.SetAllHashes(Path.Combine(Path.GetDirectoryName(form.manifestLoc), editedCell.Value.ToString() + ".fat"), form.currentMod);

                    break;
                case 1:
                    if (grid.Rows[e.RowIndex].Cells[0].Value == null)
                    {
                        MessageBox.Show("Cannot set priority without file path.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        grid.Rows.RemoveAt(grid.Rows.Count - 2);
                        break;
                    }

                    if (!int.TryParse(editedCell.Value.ToString(), out int value))
                    {
                        MessageBox.Show("Cannot set value as non-integer.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        if (form.modList[form.currentMod].Count() <= e.RowIndex)
                            editedCell.Value = "";
                        else
                            editedCell.Value = form.modList[form.currentMod][e.RowIndex].priority;
                        break;
                    }

                    if (value < 11)
                    {
                        MessageBox.Show("Priority cannot be less than 11.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        if (form.modList[form.currentMod].Count() <= e.RowIndex)
                            editedCell.Value = "";
                        else
                            editedCell.Value = form.modList[form.currentMod][e.RowIndex].priority;
                        break;
                    }

                    UpdateCell(e.RowIndex, null, value);
                    editedCell.Value = value;
                    break;
            }
        }

        public void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            DataGridViewRow selectedRow = grid.SelectedCells[0].OwningRow;
            int rowIndex = grid.SelectedCells[0].RowIndex;

            if (!selectedRow.IsNewRow && e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
            {
                removeRow(selectedRow, rowIndex);
            }
        }

        private void removeRow(DataGridViewRow selectedRow, int rowIndex)
        {
            //Clear conflicts from that file.
            Utils.SetAllHashes(Path.Combine(Path.GetDirectoryName(form.manifestLoc), form.modList[form.currentMod][rowIndex].modPath + ".fat"), form.currentMod, true);

            grid.Rows.Remove(selectedRow);
            form.modList[form.currentMod].RemoveAt(rowIndex);

            form.Text = form.Text.EndsWith("*") ? form.Text : form.Text + " *";
        }

        private void UpdateCell(int rowIndex, string? modPath, int? priority = null)
        {
            if (form.modList[form.currentMod].Count() <= rowIndex)
            {
                FileEntry entry = new();
                //Either modPath or priority has to be not null.
                if (priority.HasValue)
                {
                    entry.priority = priority.Value;
                }
                else
                {
                    entry.modPath = modPath;
                    grid.Rows[rowIndex].Cells[1].Value = entry.priority;
                }
                form.modList[form.currentMod].Add(entry);
            }
            else
            {
                FileEntry entry = form.modList[form.currentMod][rowIndex];
                if (priority.HasValue)
                    entry.priority = priority.Value;
                else
                    entry.modPath = modPath;
                form.modList[form.currentMod][rowIndex] = entry;
            }

            form.Text = form.Text.EndsWith("*") ? form.Text : form.Text + " *";
        }
    }
}
