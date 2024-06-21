using System.IO.Compression;

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

        public static void ImportMod(string? path = null)
        {
            MainForm form = GetForm();

            string manifestLoc = form.manifestLoc;
            string mainFolder = Path.GetDirectoryName(manifestLoc);

            if (string.IsNullOrWhiteSpace(manifestLoc))
                return;

            if (path == null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Please Select an .mdata or zip file.";
                openFileDialog.Filter = "Mod Metadata (*.mdata, *.zip)|*.mdata;*.zip";
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                path = openFileDialog.FileName;
            }

            if (!path.EndsWith(".zip") && !path.EndsWith(".mdata"))
            {
                MessageBox.Show("Wrong file extension.", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string zipTempFolder = "!PitCrewZipTempFolder";

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
                    MessageBox.Show("Invalid zip file, cannot find .mdata.", "Bad ZIP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Directory.CreateDirectory(zipTempFolder);
                ZipFile.ExtractToDirectory(path, zipTempFolder);
                path = Path.Combine(zipTempFolder, mdataName);
            }

            string[] lines = File.ReadAllLines(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string newFilePath;
            //Don't reset entry on bad import.
            if (!form.modList.ContainsKey(fileName))
                form.modList[fileName] = new List<FileEntry>();

            //Generate needed directories
            checkAndCreateFolder(Path.Combine(mainFolder, "mods"));
            checkAndCreateFolder(Path.Combine(mainFolder, "pitcrewmetadata"));

            foreach (string line in lines.Skip(3))
            {
                string[] separator = line.Split(' ');
                newFilePath = Path.Combine(mainFolder, "mods", separator[1] + ".fat");

                //Skip checking for .dat assuming both .fat and .dat would be in the same folder.
                if (File.Exists(newFilePath))
                {
                    checkAndDeleteFolder(zipTempFolder);
                    MessageBox.Show("Another mod is currently using a file with the name " + separator[1] + ". Please rename the file for this mod aswell in the metadata.",
                        "Duplicate Mod File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                File.Copy(Path.Combine(Path.GetDirectoryName(path), separator[1] + ".fat"), newFilePath);
                File.Copy(Path.Combine(Path.GetDirectoryName(path), separator[1] + ".dat"), newFilePath.Replace(".fat", ".dat"));

                FileEntry entry = new FileEntry
                {
                    priority = Convert.ToInt32(separator[0]),
                    modPath = "mods/" + separator[1]
                };
                form.modList[fileName].Add(entry);
            }

            newFilePath = Path.Combine(mainFolder, "pitcrewmetadata", Path.GetFileName(path));
            File.Copy(path, newFilePath, true);

            form.listBox.Items.Add(fileName);

            form.emptyLabel.Visible = false;

            checkAndDeleteFolder(zipTempFolder);

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

        public static void checkAndCreateFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        //This should only be called regarding the deletion of a temporary folder, at least until this has use elsewhere.
        private static void checkAndDeleteFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
        }
    }
}
