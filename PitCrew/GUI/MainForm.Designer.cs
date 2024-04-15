namespace PitCrew
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            conflictBox = new RichTextBox();
            emptyLabel = new Label();
            save = new Button();
            compile = new Button();
            listBox = new ListBox();
            dataGridView = new DataGridView();
            filePath = new DataGridViewTextBoxColumn();
            priority = new DataGridViewTextBoxColumn();
            menuStrip = new MenuStrip();
            manifestToolStripMenuItem = new ToolStripMenuItem();
            createManifestItem = new ToolStripMenuItem();
            openManifestItem = new ToolStripMenuItem();
            modMenuItem = new ToolStripMenuItem();
            createModButton = new ToolStripMenuItem();
            importModItem = new ToolStripMenuItem();
            helpMenuItem = new ToolStripMenuItem();
            aboutMenuTool = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)dataGridView).BeginInit();
            menuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // conflictBox
            // 
            conflictBox.BackColor = Color.FromArgb(37, 37, 37);
            conflictBox.BorderStyle = BorderStyle.None;
            conflictBox.Font = new Font("Verdana", 10F);
            conflictBox.ForeColor = Color.White;
            conflictBox.Location = new Point(499, 269);
            conflictBox.Name = "conflictBox";
            conflictBox.ReadOnly = true;
            conflictBox.Size = new Size(289, 122);
            conflictBox.TabIndex = 14;
            conflictBox.Text = "";
            conflictBox.UseWaitCursor = true;
            conflictBox.Visible = false;
            // 
            // emptyLabel
            // 
            emptyLabel.AutoSize = true;
            emptyLabel.Font = new Font("Verdana", 19F);
            emptyLabel.ForeColor = Color.White;
            emptyLabel.Location = new Point(38, 231);
            emptyLabel.Name = "emptyLabel";
            emptyLabel.Size = new Size(408, 32);
            emptyLabel.TabIndex = 13;
            emptyLabel.Text = "Double click here to add mods";
            emptyLabel.Visible = false;
            // 
            // save
            // 
            save.BackColor = Color.FromArgb(38, 38, 38);
            save.FlatAppearance.BorderColor = Color.FromArgb(64, 64, 64);
            save.FlatAppearance.BorderSize = 2;
            save.FlatStyle = FlatStyle.Flat;
            save.Font = new Font("Segoe UI", 16.25F, FontStyle.Bold);
            save.ForeColor = SystemColors.Window;
            save.Location = new Point(624, 400);
            save.Name = "save";
            save.Size = new Size(164, 40);
            save.TabIndex = 12;
            save.Text = "Save";
            save.UseVisualStyleBackColor = false;
            save.Visible = false;
            // 
            // compile
            // 
            compile.BackColor = Color.FromArgb(38, 38, 37);
            compile.FlatAppearance.BorderColor = Color.FromArgb(64, 64, 64);
            compile.FlatAppearance.BorderSize = 2;
            compile.FlatStyle = FlatStyle.Flat;
            compile.Font = new Font("Segoe UI", 16.25F, FontStyle.Bold);
            compile.ForeColor = SystemColors.Window;
            compile.Location = new Point(624, 446);
            compile.Name = "compile";
            compile.Size = new Size(164, 40);
            compile.TabIndex = 11;
            compile.Text = "Compile";
            compile.UseVisualStyleBackColor = false;
            compile.Visible = false;
            // 
            // listBox
            // 
            listBox.BackColor = Color.FromArgb(38, 38, 38);
            listBox.BorderStyle = BorderStyle.FixedSingle;
            listBox.DrawMode = DrawMode.OwnerDrawVariable;
            listBox.Font = new Font("Verdana", 12F);
            listBox.ForeColor = SystemColors.Window;
            listBox.FormattingEnabled = true;
            listBox.ItemHeight = 90;
            listBox.Location = new Point(12, 29);
            listBox.Name = "listBox";
            listBox.Size = new Size(459, 462);
            listBox.TabIndex = 9;
            listBox.TabStop = false;
            // 
            // dataGridView
            // 
            dataGridView.BackgroundColor = Color.FromArgb(38, 38, 38);
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(38, 38, 38);
            dataGridViewCellStyle1.Font = new Font("Verdana", 11F, FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = Color.White;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.Columns.AddRange(new DataGridViewColumn[] { filePath, priority });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.WindowFrame;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.Location = new Point(499, 29);
            dataGridView.Name = "dataGridView";
            dataGridView.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.WindowFrame;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.Honeydew;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = Color.Honeydew;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            dataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dataGridView.RowHeadersVisible = false;
            dataGridViewCellStyle4.BackColor = Color.FromArgb(64, 64, 64);
            dataGridViewCellStyle4.ForeColor = Color.White;
            dataGridViewCellStyle4.SelectionBackColor = Color.FromArgb(88, 88, 88);
            dataGridView.RowsDefaultCellStyle = dataGridViewCellStyle4;
            dataGridView.ScrollBars = ScrollBars.Vertical;
            dataGridView.Size = new Size(289, 234);
            dataGridView.TabIndex = 10;
            dataGridView.Visible = false;
            // 
            // filePath
            // 
            filePath.HeaderText = "File Path";
            filePath.Name = "filePath";
            filePath.Width = 166;
            // 
            // priority
            // 
            priority.HeaderText = "Priority";
            priority.Name = "priority";
            priority.Width = 120;
            // 
            // menuStrip
            // 
            menuStrip.BackColor = Color.FromArgb(38, 38, 38);
            menuStrip.Items.AddRange(new ToolStripItem[] { manifestToolStripMenuItem, modMenuItem, helpMenuItem });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.RenderMode = ToolStripRenderMode.Professional;
            menuStrip.Size = new Size(800, 24);
            menuStrip.TabIndex = 15;
            menuStrip.Text = "menuStrip";
            // 
            // manifestToolStripMenuItem
            // 
            manifestToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { createManifestItem, openManifestItem });
            manifestToolStripMenuItem.ForeColor = Color.White;
            manifestToolStripMenuItem.Name = "manifestToolStripMenuItem";
            manifestToolStripMenuItem.Size = new Size(65, 20);
            manifestToolStripMenuItem.Text = "Manifest";
            // 
            // createManifestItem
            // 
            createManifestItem.ForeColor = Color.White;
            createManifestItem.Name = "createManifestItem";
            createManifestItem.Size = new Size(108, 22);
            createManifestItem.Text = "Create";
            // 
            // openManifestItem
            // 
            openManifestItem.ForeColor = Color.White;
            openManifestItem.Name = "openManifestItem";
            openManifestItem.Size = new Size(108, 22);
            openManifestItem.Text = "Open";
            // 
            // modMenuItem
            // 
            modMenuItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            modMenuItem.DropDownItems.AddRange(new ToolStripItem[] { createModButton, importModItem });
            modMenuItem.ForeColor = Color.White;
            modMenuItem.Name = "modMenuItem";
            modMenuItem.Size = new Size(49, 20);
            modMenuItem.Text = "Mods";
            modMenuItem.Visible = false;
            // 
            // createModButton
            // 
            createModButton.ForeColor = Color.White;
            createModButton.Name = "createModButton";
            createModButton.Size = new Size(110, 22);
            createModButton.Text = "New";
            // 
            // importModItem
            // 
            importModItem.ForeColor = Color.White;
            importModItem.Name = "importModItem";
            importModItem.Size = new Size(110, 22);
            importModItem.Text = "Import";
            // 
            // helpMenuItem
            // 
            helpMenuItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            helpMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutMenuTool });
            helpMenuItem.ForeColor = Color.White;
            helpMenuItem.Name = "helpMenuItem";
            helpMenuItem.Size = new Size(44, 20);
            helpMenuItem.Text = "Help";
            // 
            // aboutMenuTool
            // 
            aboutMenuTool.BackColor = SystemColors.Control;
            aboutMenuTool.ForeColor = Color.White;
            aboutMenuTool.Name = "aboutMenuTool";
            aboutMenuTool.Size = new Size(107, 22);
            aboutMenuTool.Text = "About";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(38, 38, 38);
            ClientSize = new Size(800, 499);
            Controls.Add(menuStrip);
            Controls.Add(conflictBox);
            Controls.Add(emptyLabel);
            Controls.Add(save);
            Controls.Add(compile);
            Controls.Add(listBox);
            Controls.Add(dataGridView);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimumSize = new Size(816, 538);
            Name = "MainForm";
            Text = "PitCrew";
            ((System.ComponentModel.ISupportInitialize)dataGridView).EndInit();
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private DataGridViewTextBoxColumn filePath;
        private DataGridViewTextBoxColumn priority;
        private MenuStrip menuStrip;
        private ToolStripMenuItem importModItem;
        private ToolStripMenuItem createModButton;
        private ToolStripMenuItem helpMenuItem;
        private ToolStripMenuItem aboutMenuTool;
        private ToolStripMenuItem manifestToolStripMenuItem;
        private ToolStripMenuItem createManifestItem;
        private ToolStripMenuItem openManifestItem;
        internal ListBox listBox;
        public Label emptyLabel;
        internal RichTextBox conflictBox;
        internal Button save;
        internal Button compile;
        internal DataGridView dataGridView;
        internal ToolStripMenuItem modMenuItem;
    }
}
