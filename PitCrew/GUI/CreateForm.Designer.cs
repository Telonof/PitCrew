namespace PitCrew
{
    partial class CreateForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            titleBox = new TextBox();
            label2 = new Label();
            label3 = new Label();
            authorBox = new TextBox();
            label4 = new Label();
            descriptionBox = new RichTextBox();
            createButton = new Button();
            label5 = new Label();
            modIdBox = new TextBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Verdana", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.White;
            label1.Location = new Point(112, 9);
            label1.Name = "label1";
            label1.Size = new Size(118, 23);
            label1.TabIndex = 0;
            label1.Text = "Create Mod";
            // 
            // titleBox
            // 
            titleBox.BackColor = Color.FromArgb(64, 64, 64);
            titleBox.BorderStyle = BorderStyle.FixedSingle;
            titleBox.ForeColor = Color.White;
            titleBox.Location = new Point(92, 118);
            titleBox.Name = "titleBox";
            titleBox.Size = new Size(156, 23);
            titleBox.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Verdana", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.White;
            label2.Location = new Point(90, 97);
            label2.Name = "label2";
            label2.Size = new Size(38, 18);
            label2.TabIndex = 2;
            label2.Text = "Title";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Verdana", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.ForeColor = Color.White;
            label3.Location = new Point(90, 151);
            label3.Name = "label3";
            label3.Size = new Size(58, 18);
            label3.TabIndex = 4;
            label3.Text = "Author";
            // 
            // authorBox
            // 
            authorBox.BackColor = Color.FromArgb(64, 64, 64);
            authorBox.BorderStyle = BorderStyle.FixedSingle;
            authorBox.ForeColor = Color.White;
            authorBox.Location = new Point(92, 172);
            authorBox.Name = "authorBox";
            authorBox.Size = new Size(156, 23);
            authorBox.TabIndex = 3;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = Color.Transparent;
            label4.Font = new Font("Verdana", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label4.ForeColor = Color.White;
            label4.Location = new Point(53, 207);
            label4.Name = "label4";
            label4.Size = new Size(90, 18);
            label4.TabIndex = 6;
            label4.Text = "Description";
            // 
            // descriptionBox
            // 
            descriptionBox.BackColor = Color.FromArgb(64, 64, 64);
            descriptionBox.BorderStyle = BorderStyle.None;
            descriptionBox.ForeColor = Color.White;
            descriptionBox.Location = new Point(53, 230);
            descriptionBox.Name = "descriptionBox";
            descriptionBox.Size = new Size(242, 47);
            descriptionBox.TabIndex = 7;
            descriptionBox.Text = "Keep it short please.";
            // 
            // createButton
            // 
            createButton.BackColor = Color.FromArgb(38, 38, 38);
            createButton.FlatAppearance.BorderColor = Color.FromArgb(64, 64, 64);
            createButton.FlatAppearance.BorderSize = 2;
            createButton.FlatStyle = FlatStyle.Flat;
            createButton.Font = new Font("Segoe UI", 16.25F, FontStyle.Bold);
            createButton.ForeColor = SystemColors.Window;
            createButton.Location = new Point(84, 292);
            createButton.Name = "createButton";
            createButton.Size = new Size(164, 40);
            createButton.TabIndex = 13;
            createButton.Text = "Create";
            createButton.UseVisualStyleBackColor = false;
            createButton.Click += Create_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.BackColor = Color.Transparent;
            label5.Font = new Font("Verdana", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label5.ForeColor = Color.White;
            label5.Location = new Point(89, 45);
            label5.Name = "label5";
            label5.Size = new Size(147, 18);
            label5.TabIndex = 15;
            label5.Text = "Mod ID (file name)";
            // 
            // modIdBox
            // 
            modIdBox.BackColor = Color.FromArgb(64, 64, 64);
            modIdBox.BorderStyle = BorderStyle.FixedSingle;
            modIdBox.ForeColor = Color.White;
            modIdBox.Location = new Point(91, 66);
            modIdBox.Name = "modIdBox";
            modIdBox.Size = new Size(156, 23);
            modIdBox.TabIndex = 14;
            // 
            // CreateForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(38, 38, 38);
            ClientSize = new Size(346, 352);
            Controls.Add(authorBox);
            Controls.Add(titleBox);
            Controls.Add(modIdBox);
            Controls.Add(createButton);
            Controls.Add(descriptionBox);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(label5);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CreateForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Create Mod Group";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox titleBox;
        private Label label2;
        private Label label3;
        private TextBox authorBox;
        private Label label4;
        private RichTextBox descriptionBox;
        private Button createButton;
        private Label label5;
        private TextBox modIdBox;
    }
}