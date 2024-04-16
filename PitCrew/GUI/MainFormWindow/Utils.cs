namespace PitCrew.GUI.MainWindow
{
    internal class Utils
    {
        public static MainForm GetForm()
        {
            //Should never be null.
            return Application.OpenForms.OfType<MainForm>().FirstOrDefault();
        }

        public static void SaveFile()
        {
            MainForm form = GetForm();
            List<string> textLines = new List<string>();

            foreach (String key in form.modList.Keys)
            {
                foreach (FileEntry entry in form.modList[key])
                {
                    string text = $"{entry.priority} {entry.modPath} {key}";

                    if (form.disabledMods.Contains(key))
                        text = "#" + text;

                    textLines.Add(text);
                }
            }

            string filePath = form.manifestLoc;
            File.WriteAllLines(filePath, textLines);

            form.Text = form.Text.TrimEnd('*');
        }

        public static void OpenMDataFile()
        {
            MainForm form = GetForm();

            string manifestLoc = form.manifestLoc;
            string mainFolder = Path.GetDirectoryName(manifestLoc);

            if (string.IsNullOrWhiteSpace(manifestLoc))
                return;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Please Select Your .mdata File";
            openFileDialog.Filter = "Mod Metadata (*.mdata)|*.mdata";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;


            string[] lines = File.ReadAllLines(openFileDialog.FileName);
            string fileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
            string newFilePath;
            form.modList[fileName] = new List<FileEntry>();

            foreach (string line in lines.Skip(3))
            {
                string[] separator = line.Split(' ');
                newFilePath = Path.Combine(mainFolder, "mods", separator[1] + ".fat");

                //Skip checking for .dat assuming both .fat and .dat would be in the same folder.
                if (File.Exists(newFilePath))
                {
                    MessageBox.Show("Another mod is currently using a file with the name " + separator[1] + ". Please rename the file for this mod aswell in the metadata.",
                        "Duplicate Mod File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!Directory.Exists(Path.Combine(mainFolder, "mods")))
                    Directory.CreateDirectory(Path.Combine(mainFolder, "mods"));

                File.Move(Path.Combine(Path.GetDirectoryName(openFileDialog.FileName), separator[1] + ".fat"), newFilePath);
                File.Move(Path.Combine(Path.GetDirectoryName(openFileDialog.FileName), separator[1] + ".dat"), newFilePath.Replace(".fat", ".dat"));

                FileEntry entry = new FileEntry
                {
                    priority = Convert.ToInt32(separator[0]),
                    modPath = "mods/" + separator[1]
                };
                form.modList[fileName].Add(entry);
            }

            if (!Directory.Exists(Path.Combine(mainFolder, "pitcrewmetadata")))
                Directory.CreateDirectory(Path.Combine(mainFolder, "pitcrewmetadata"));

            newFilePath = Path.Combine(mainFolder, "pitcrewmetadata", Path.GetFileName(openFileDialog.FileName));
            File.Move(openFileDialog.FileName, newFilePath, true);

            form.listBox.Items.Add(fileName);

            form.emptyLabel.Visible = false;

            //Just in case people close out immeditealy.
            SaveFile();
        }

        public static void SetAllHashes(string file, string modName, bool remove = false)
        {
            const int headerSize = 12;
            const int loopCountSize = 4;
            const int dataBlockSize = 8;

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
                    string data = BitConverter.ToString(dataBytes).Replace("-", "");

                    if (remove)
                        GetForm().modifiedFiles[modName].Remove(data);
                    else
                        GetForm().modifiedFiles[modName].Add(data);

                    stream.Seek(dataBlockSize, SeekOrigin.Current);
                }
            }
        }
    }
}
