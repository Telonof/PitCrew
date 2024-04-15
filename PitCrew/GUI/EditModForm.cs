namespace PitCrew
{
    public partial class EditModForm : Form
    {
        private readonly string modMetadata;

        //Get the current mod info and replace it with any new data.
        public EditModForm(string[] data, string modMetadata)
        {
            InitializeComponent();

            this.modMetadata = modMetadata;
            titleBox.Text = data[0];
            authorBox.Text = data[1];
            descriptionBox.Text = data[2];
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            string[] lines = File.ReadAllLines(modMetadata);
            lines[0] = titleBox.Text;
            lines[1] = authorBox.Text;
            lines[2] = descriptionBox.Text;
            File.WriteAllLines(modMetadata, lines);
            Close();
        }
    }
}
