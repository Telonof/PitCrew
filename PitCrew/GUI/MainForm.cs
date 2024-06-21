using PitCrew.GUI.MainWindow;
using PitCrew.GUI.MainWindow.Components;
using PitCrew.GUI.MainWindow.Components.MenuBar;
using System.Windows.Forms;

namespace PitCrew
{
    public partial class MainForm : Form
    {
        internal string manifestLoc = null;

        internal Dictionary<string, List<FileEntry>> modList = new Dictionary<string, List<FileEntry>>();

        internal List<string> disabledMods = new List<string>();

        internal string currentMod;

        internal Dictionary<string, List<string>> modifiedFiles = new Dictionary<string, List<string>>();

        internal RightClickComboBox rightClickComboBox;

        public MainForm()
        {
            InitializeComponent();

            //Initalize menu strip
            MenuBar menuBar = new MenuBar(this, menuStrip);
            aboutMenuTool.Click += menuBar.AboutMenu_Click;
            importModItem.Click += menuBar.ImportMod_Click;
            createModButton.Click += menuBar.CreateModButton_Click;
            openManifestItem.Click += menuBar.OpenManifestItem_Click;
            createManifestItem.Click += menuBar.CreateManifestItem_Click;
            packageArchiveTool.Click += menuBar.PackageArchiveTool_Click;

            //Initalize keyboard shortcuts
            this.KeyDown += DetectKeyPresses;
            this.KeyPreview = true;

            //Initalize file list
            FileGridList gridList = new FileGridList(this);
            dataGridView.CellEndEdit += gridList.Grid_CellEndEdit;
            dataGridView.KeyDown += gridList.Grid_KeyDown;

            //Initalize mod list
            ModListBox modListBox = new ModListBox(this);
            listBox.SelectedIndexChanged += modListBox.ModListBox_SelectedIndexChanged;
            listBox.MouseClick += modListBox.ModListBox_MouseClick;
            listBox.DoubleClick += modListBox.ModListBox_DoubleClick;
            listBox.MouseDown += modListBox.ModListBox_MouseDown;
            listBox.DrawItem += modListBox.ModListBox_DrawItem;
            listBox.DragDrop += modListBox.ModListBox_DragDrop;
            listBox.DragEnter += modListBox.ModListBox_DragEnter;

            //Initialize Right click box
            rightClickComboBox = new RightClickComboBox(this);

            //Initalize Buttons
            Buttons buttons = new Buttons();
            compile.Click += buttons.CompileButton_Click;
            save.Click += buttons.SaveButton_Click;
        }

        private void DetectKeyPresses(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                Utils.SaveFile();
                e.Handled = true;
            }
        }
    }
}
