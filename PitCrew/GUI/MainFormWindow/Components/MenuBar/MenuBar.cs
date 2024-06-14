using PitCrewCommon;

namespace PitCrew.GUI.MainWindow.Components.MenuBar
{
    internal class MenuBar : BaseComponent
    {
        public MenuBar(MainForm form, MenuStrip menuBar) : base(form)
        {
            base.form = form;

            //Force no image margins on any item.
            foreach (ToolStripMenuItem menuItem in menuBar.Items)
                ((ToolStripDropDownMenu)menuItem.DropDown).ShowImageMargin = false;

            menuBar.Renderer = new ToolStripProfessionalRenderer(new CustomMenuBarRenderer());
        }

        public void ImportMod_Click(object sender, EventArgs e)
        {
            Utils.ImportMod();
        }

        public void AboutMenu_Click(object sender, EventArgs e)
        {
            new About().ShowDialog();
        }

        public void CreateModButton_Click(object sender, EventArgs e)
        {
            new CreateForm().ShowDialog();
        }

        public void OpenManifestItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Please select The Crew's Mod Manifest file";
            openFileDialog.Filter = "Manifest file (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            string output = ManifestUtil.ValidateManifestFile(openFileDialog.FileName);
            if (output != null)
            {
                MessageBox.Show(output, "Invalid Manifest", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            form.modList.Clear();
            form.manifestLoc = openFileDialog.FileName;
            form.save.Visible = true;
            form.compile.Visible = true;
            form.modMenuItem.Visible = true;

            string[] lines = File.ReadAllLines(openFileDialog.FileName);

            foreach (string line in lines)
            {
                bool disabled = false;

                if (String.IsNullOrWhiteSpace(line) || line.StartsWith("##"))
                    continue;

                string[] parts = line.Split(' ');

                if (parts[0].StartsWith('#'))
                {
                    disabled = true;
                    parts[0] = parts[0].Substring(1);
                }

                string groupName = parts.Length > 2 ? parts[2] : "Default";

                if (!form.modifiedFiles.ContainsKey(groupName))
                    form.modifiedFiles[groupName] = new List<string>();

                if (disabled && !form.disabledMods.Contains(groupName))
                    form.disabledMods.Add(groupName);

                if (!form.modList.ContainsKey(groupName))
                    form.modList[groupName] = new List<FileEntry>();

                FileEntry entry = new FileEntry();
                entry.priority = Convert.ToInt32(parts[0]);
                entry.modPath = parts[1];
                form.modList[groupName].Add(entry);

                Utils.SetAllHashes(Path.Combine(Path.GetDirectoryName(form.manifestLoc), parts[1] + ".fat"), groupName);
            }

            InitializeListBox();
        }

        public void CreateManifestItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Please select The Crew's exe";
            openFileDialog.Filter = "Crew's exe file (*.exe)|*.exe";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            string path = Path.Combine(Path.GetDirectoryName(openFileDialog.FileName), "data_win32");

            if (!Directory.Exists(path))
            {
                MessageBox.Show("Could not find folder data_win32.", "Invalid exe", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string newManifestLoc = Path.Combine(path, "pitcrewmanifest.txt");
            if (File.Exists(newManifestLoc))
            {
                MessageBox.Show("Manifest file already detected.", "Found Existing Manifest", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //The GUI won't allow multiple manifests. Encourage one per game.
            string[] potentialManifests = Directory.GetFiles(path, "*.txt");
            foreach (string file in potentialManifests)
            {
                if (file == newManifestLoc)
                    continue;

                if (ManifestUtil.ValidateManifestFile(file) == null)
                {
                    MessageBox.Show("Manifest " + file + " already exists.", "Found Existing Manifest", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            File.Create(newManifestLoc).Close();
            form.modList.Clear();
            form.manifestLoc = newManifestLoc;
            form.save.Visible = true;
            form.compile.Visible = true;
            form.modMenuItem.Visible = true;
            InitializeListBox();
        }

        private void InitializeListBox()
        {
            form.listBox.Items.Clear();
            foreach (var key in form.modList.Keys)
            {
                form.listBox.Items.Add(key);
            }

            if (form.listBox.Items.Count == 0)
            {
                form.emptyLabel.Visible = true;
                form.currentMod = "";
                return;
            }
            
            form.currentMod = form.listBox.Items[0].ToString();
        }
    }
}
