namespace PitCrew
{
    partial class EditModForm
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
            editButton = new Button();
            descriptionBox = new RichTextBox();
            label4 = new Label();
            label3 = new Label();
            authorBox = new TextBox();
            label2 = new Label();
            titleBox = new TextBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // editButton
            // 
            editButton.BackColor = Color.FromArgb(38, 38, 38);
            editButton.FlatAppearance.BorderColor = Color.FromArgb(64, 64, 64);
            editButton.FlatAppearance.BorderSize = 2;
            editButton.FlatStyle = FlatStyle.Flat;
            editButton.Font = new Font("Segoe UI", 16.25F, FontStyle.Bold);
            editButton.ForeColor = SystemColors.Window;
            editButton.Location = new Point(88, 261);
            editButton.Name = "editButton";
            editButton.Size = new Size(164, 40);
            editButton.TabIndex = 21;
            editButton.Text = "Edit";
            editButton.UseVisualStyleBackColor = false;
            editButton.Click += EditButton_Click;
            // 
            // descriptionBox
            // 
            descriptionBox.BackColor = Color.FromArgb(64, 64, 64);
            descriptionBox.BorderStyle = BorderStyle.None;
            descriptionBox.ForeColor = Color.White;
            descriptionBox.Location = new Point(52, 184);
            descriptionBox.Name = "descriptionBox";
            descriptionBox.Size = new Size(242, 47);
            descriptionBox.TabIndex = 20;
            descriptionBox.Text = "";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Verdana", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label4.ForeColor = Color.White;
            label4.Location = new Point(52, 163);
            label4.Name = "label4";
            label4.Size = new Size(90, 18);
            label4.TabIndex = 19;
            label4.Text = "Description";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Verdana", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.ForeColor = Color.White;
            label3.Location = new Point(89, 106);
            label3.Name = "label3";
            label3.Size = new Size(58, 18);
            label3.TabIndex = 18;
            label3.Text = "Author";
            // 
            // authorBox
            // 
            authorBox.BackColor = Color.FromArgb(64, 64, 64);
            authorBox.BorderStyle = BorderStyle.FixedSingle;
            authorBox.ForeColor = Color.White;
            authorBox.Location = new Point(91, 126);
            authorBox.Name = "authorBox";
            authorBox.Size = new Size(156, 23);
            authorBox.TabIndex = 17;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Verdana", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.White;
            label2.Location = new Point(89, 52);
            label2.Name = "label2";
            label2.Size = new Size(38, 18);
            label2.TabIndex = 16;
            label2.Text = "Title";
            // 
            // titleBox
            // 
            titleBox.BackColor = Color.FromArgb(64, 64, 64);
            titleBox.BorderStyle = BorderStyle.FixedSingle;
            titleBox.ForeColor = Color.White;
            titleBox.Location = new Point(91, 72);
            titleBox.Name = "titleBox";
            titleBox.Size = new Size(156, 23);
            titleBox.TabIndex = 15;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Verdana", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.White;
            label1.Location = new Point(81, 15);
            label1.Name = "label1";
            label1.Size = new Size(187, 23);
            label1.TabIndex = 14;
            label1.Text = "Edit Mod Metadata";
            // 
            // EditModForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(38, 38, 38);
            ClientSize = new Size(346, 317);
            Controls.Add(editButton);
            Controls.Add(descriptionBox);
            Controls.Add(label4);
            Controls.Add(authorBox);
            Controls.Add(titleBox);
            Controls.Add(label1);
            Controls.Add(label3);
            Controls.Add(label2);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "EditModForm";
            ShowIcon = false;
            Text = "Edit Metadata";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button editButton;
        private RichTextBox descriptionBox;
        private Label label4;
        private Label label3;
        private TextBox authorBox;
        private Label label2;
        private TextBox titleBox;
        private Label label1;
    }
}