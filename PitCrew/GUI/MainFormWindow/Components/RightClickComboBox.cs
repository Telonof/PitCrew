using PitCrew.GUI.MainWindow.Components.MenuBar;
using System.IO.Compression;

namespace PitCrew.GUI.MainWindow.Components
{
    internal class RightClickComboBox : BaseComponent
    {
        internal string modName;

        private ContextMenuStrip contextMenu = new ContextMenuStrip();

        public RightClickComboBox(MainForm form) : base(form)
        {
            base.form = form;

            contextMenu.ShowImageMargin = false;

            //Create the three buttons for mod modification.
            ToolStripMenuItem packageModMenuItem = new ToolStripMenuItem("Package Mod");
            ToolStripMenuItem editModMenuItem = new ToolStripMenuItem("Edit Metadata");
            ToolStripMenuItem deleteModMenuItem = new ToolStripMenuItem("Delete Mod");
            packageModMenuItem.ForeColor = Color.White;
            editModMenuItem.ForeColor = Color.White;
            deleteModMenuItem.ForeColor = Color.White;
            contextMenu.Renderer = new ToolStripProfessionalRenderer(new CustomMenuBarRenderer());

            //Assign the buttons to functions.
            packageModMenuItem.Click += (s, args) => PackageMod();
            editModMenuItem.Click += (s, args) => EditMod();
            deleteModMenuItem.Click += (s, args) => DeleteMod();

            contextMenu.Items.Add(packageModMenuItem);
            contextMenu.Items.Add(editModMenuItem);
            contextMenu.Items.Add(deleteModMenuItem);
        }

        public void ToggleOn(string modName, Point location)
        {
            this.modName = modName;
            contextMenu.Show(form.listBox, location);
        }

        private void PackageMod()
        {
            string manifestLoc = form.manifestLoc;
            List<string> filesToZip = new List<string>();
            string modMetadata = Path.Combine(Path.GetDirectoryName(manifestLoc), "pitcrewmetadata", $"{modName}.mdata");

            if (!File.Exists(modMetadata))
            {
                MessageBox.Show("Could not find proper metadata.\nPlease edit this mod.", "No Metadata Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (form.modList[modName].Count == 0)
            {
                MessageBox.Show("No files to package.", "No Mod Files Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            filesToZip.Add(modMetadata);
            List<string> lines = File.ReadAllLines(modMetadata).ToList();
            int count = 2;

            //Get all mods needed to be stored.
            foreach (FileEntry entry in form.modList[modName])
            {
                string directName = Path.GetFileName(entry.modPath);
                count++;

                if (lines.Count <= count)
                {
                    lines.Add($"{entry.priority} {directName}");
                }
                else
                {
                    lines[count] = $"{entry.priority} {directName}";
                }

                filesToZip.Add(Path.Combine(Path.GetDirectoryName(manifestLoc), entry.modPath + ".fat"));
                filesToZip.Add(Path.Combine(Path.GetDirectoryName(manifestLoc), entry.modPath + ".dat"));
            }

            File.WriteAllLines(modMetadata, lines);

            //Zip it
            using (var zip = new ZipArchive(File.Open($"{modName}.zip", FileMode.Create), ZipArchiveMode.Create))
            {
                foreach (string filePath in filesToZip)
                {
                    zip.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                }
            }

            DialogResult result = MessageBox.Show($"Mod packaged for sharing at {modName}.zip\nView in explorer?", "Mod Packaged", MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
                return;

            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{modName}.zip\"");
        }

        private void DeleteMod()
        {
            string manifestLoc = form.manifestLoc;

            DialogResult result = MessageBox.Show("Are you sure you want to delete this?\nThe metadata and all mod files will be gone.", "Final warning", MessageBoxButtons.OKCancel);
            if (result != DialogResult.OK)
                return;

            form.listBox.Items.Remove(modName);

            string modMetadata = Path.Combine(Path.GetDirectoryName(manifestLoc), "pitcrewmetadata", $"{modName}.mdata");
            File.Delete(modMetadata);

            foreach (FileEntry entry in form.modList[modName])
            {
                File.Delete(Path.Combine(Path.GetDirectoryName(manifestLoc), entry.modPath + ".fat"));
                File.Delete(Path.Combine(Path.GetDirectoryName(manifestLoc), entry.modPath + ".dat"));
            }

            form.modList.Remove(modName);
            form.dataGridView.Visible = false;
            form.modifiedFiles.Remove(modName);

            MessageBox.Show("Mod successfully deleted.");
        }

        private void EditMod()
        {
            string modMetadata = Path.Combine(Path.GetDirectoryName(form.manifestLoc), "pitcrewmetadata", $"{modName}.mdata");
            string[] lines = File.ReadAllLines(modMetadata);
            new EditModForm(lines, modMetadata).ShowDialog();
        }
    }
}
