using PitCrew.GUI.MainWindow;

namespace PitCrew
{
    public partial class CreateForm : Form
    {
        public CreateForm()
        {
            InitializeComponent();
        }

        private void Create_Click(object sender, EventArgs e)
        {
            MainForm form = Utils.GetForm();

            //Ensure no line is empty.
            string[]? list = ValidateModInputs();

            if (list == null)
                return;

            //Append author to file.
            list[0] = $"{list[2]}_{list[0].Replace(" ", "")}";

            if (!GetNewModInfo(form, list))
            {
                MessageBox.Show("Mod ID already exists.", "Create Mod Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Close();
        }

        private bool GetNewModInfo(MainForm form, string[] modInfo)
        {
            string path = Path.Combine(Path.GetDirectoryName(form.manifestLoc), "pitcrewmetadata");
            Utils.checkAndCreateFolder(path);

            String modMetadataPath = Path.Combine(path, modInfo[0] + ".mdata");

            if (File.Exists(modMetadataPath))
                return false;

            using (StreamWriter newFile = new StreamWriter(modMetadataPath))
            {
                newFile.WriteLine(modInfo[1]);
                newFile.WriteLine(modInfo[2]);
                newFile.WriteLine(modInfo[3]);
            }

            form.modList[modInfo[0]] = new List<FileEntry>();
            form.listBox.Items.Add(modInfo[0]);
            form.emptyLabel.Hide();
            return true;
        }

        private string[]? ValidateModInputs()
        {
            string[] list = { modIdBox.Text, titleBox.Text, authorBox.Text, descriptionBox.Text };

            foreach (string input in list)
            {
                if (!String.IsNullOrWhiteSpace(input))
                    continue;

                MessageBox.Show("All fields must have an input.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return list;
        }
    }
}
