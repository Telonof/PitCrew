using System.Windows.Forms.VisualStyles;

namespace PitCrew.GUI.MainWindow.Components
{
    internal class ModListBox : BaseComponent
    {
        private ListBox listBox;

        public ModListBox(MainForm form) : base(form)
        {
            base.form = form;
            listBox = form.listBox;
        }

        public string[] GetMetadata(string modId)
        {
            string filePath = Path.Combine(Path.GetDirectoryName(form.manifestLoc), "pitcrewmetadata", modId + ".mdata");
            if (!File.Exists(filePath))
            {
                return [modId, "No metadata found for this mod.", ""];
            }

            return File.ReadAllLines(filePath);

        }

        public void ModListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            //Ensure an item is selected and is being hovered over.
            int index = listBox.IndexFromPoint(e.Location);
            if (index == ListBox.NoMatches)
                return;

            Rectangle itemRect = listBox.GetItemRectangle(index);

            if (!itemRect.Contains(e.Location))
                return;

            form.rightClickComboBox.ToggleOn(listBox.Items[index].ToString(), e.Location);
        }

        public void ModListBox_DoubleClick(object sender, EventArgs e)
        {
            Utils.ImportMod();
        }

        public void ModListBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (form.manifestLoc == null || listBox.Items.Count < 1)
                return;

            int itemIndex = listBox.IndexFromPoint(e.Location);

            if (itemIndex == ListBox.NoMatches)
                return;

            string itemText = listBox.Items[itemIndex].ToString();

            if (e.X < form.listBox.Width - 39)
                return;

            if (form.disabledMods.Contains(itemText))
            {
                form.disabledMods.Remove(itemText);
            }
            else
            {
                form.disabledMods.Add(itemText);
            }

            listBox.Invalidate();

            form.Text = form.Text.EndsWith("*") ? form.Text : form.Text + " *";
        }

        public void ModListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            e.DrawBackground();

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 64, 64)), e.Bounds);
            }

            //Get the text and fonts
            string itemText = listBox.Items[e.Index].ToString();
            string[] lines = GetMetadata(itemText);
            int yPos = e.Bounds.Top;
            Font titleFont = new Font(e.Font.FontFamily, e.Font.Size + 2, FontStyle.Bold);
            Font authorFont = new Font(e.Font.FontFamily, e.Font.Size - 4, FontStyle.Bold);
            Font descFont = new Font(e.Font.FontFamily, e.Font.Size - 4);
            Font[] fonts = { titleFont, authorFont, descFont };

            for (int i = 0; i < 3; i++)
            {
                //Handle word wrapping
                if (i == 2)
                {
                    string descLine = "";
                    foreach (String word in lines[i].Split(' '))
                    {
                        descLine += word + " ";
                        if (e.Graphics.MeasureString(descLine, fonts[i]).Width < 400)
                            continue;

                        int lastIndex = descLine.LastIndexOf(word);
                        descLine = descLine.Substring(0, lastIndex);
                        e.Graphics.DrawString(descLine, fonts[i], Brushes.White, e.Bounds.Left, yPos);
                        yPos += (int)e.Graphics.MeasureString(lines[i], fonts[i]).Height;
                        lines[i] = lines[i].Replace(descLine, "");
                        break;
                    }
                }

                //Append ellipsis on word wrap
                bool appendEllipsis = false;
                while (e.Graphics.MeasureString(lines[i], fonts[i]).Width > 400)
                {
                    lines[i] = string.Join(" ", lines[i].Split(' ')[..^1]);
                    appendEllipsis = true;
                }

                if (appendEllipsis)
                    lines[i] += "...";

                e.Graphics.DrawString(lines[i], fonts[i], Brushes.White, e.Bounds.Left, yPos);
                yPos += (int)e.Graphics.MeasureString(lines[i], fonts[i]).Height;
            }

            //Draw the checkbox
            bool isChecked = !form.disabledMods.Contains(itemText);
            CheckBoxState state = isChecked ? CheckBoxState.CheckedPressed : CheckBoxState.UncheckedNormal;
            CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(e.Bounds.Right - 20, e.Bounds.Top + 38), state);
        }

        public void ModListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox.SelectedItem == null)
                return;

            form.dataGridView.Rows.Clear();

            foreach (FileEntry entry in form.modList[listBox.SelectedItem.ToString()])
            {
                form.dataGridView.Rows.Add(entry.modPath, entry.priority);
            }

            form.dataGridView.Visible = true;
            form.currentMod = listBox.SelectedItem.ToString();

            //Get all conflicts with other mods.
            form.conflictBox.Clear();

            if (!form.modifiedFiles.ContainsKey(form.currentMod))
                return;

            form.conflictBox.AppendText("Mods conflicting with this:\n");
            foreach (string modName in form.modifiedFiles.Keys)
            {
                if (modName.Equals(form.currentMod))
                    continue;

                IEnumerable<string> intersects = form.modifiedFiles[modName].Intersect(form.modifiedFiles[form.currentMod]);
                if (!intersects.Any())
                    continue;

                form.conflictBox.Visible = true;
                form.conflictBox.AppendText(GetMetadata(modName)[0] + "\n");
            }
        }
    }
}
