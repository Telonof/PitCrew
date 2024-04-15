namespace PitCrew
{
    partial class About
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            label2 = new Label();
            label3 = new Label();
            serverLinkLabel = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Verdana", 30F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.White;
            label1.Location = new Point(114, 10);
            label1.Name = "label1";
            label1.Size = new Size(196, 48);
            label1.TabIndex = 0;
            label1.Text = "PitCrew";
            // 
            // label2
            // 
            label2.Font = new Font("Verdana", 10F, FontStyle.Bold);
            label2.ForeColor = Color.White;
            label2.Location = new Point(87, 56);
            label2.Name = "label2";
            label2.Size = new Size(242, 35);
            label2.TabIndex = 1;
            label2.Text = "A mod loader for The Crew 1";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            label3.Font = new Font("Verdana", 10F, FontStyle.Bold);
            label3.ForeColor = Color.White;
            label3.Location = new Point(161, 87);
            label3.Name = "label3";
            label3.Size = new Size(93, 32);
            label3.TabIndex = 2;
            label3.Text = "By: FTIW";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // serverLinkLabel
            // 
            serverLinkLabel.Font = new Font("Verdana", 9.75F, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, 0);
            serverLinkLabel.ForeColor = Color.DodgerBlue;
            serverLinkLabel.Location = new Point(96, 140);
            serverLinkLabel.Name = "serverLinkLabel";
            serverLinkLabel.Size = new Size(225, 20);
            serverLinkLabel.TabIndex = 3;
            serverLinkLabel.Text = "Come join the TCU Discord!";
            serverLinkLabel.TextAlign = ContentAlignment.MiddleCenter;
            serverLinkLabel.Click += ServerLinkLabel_Click;
            // 
            // About
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(38, 38, 38);
            ClientSize = new Size(408, 169);
            Controls.Add(label3);
            Controls.Add(serverLinkLabel);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "About";
            Padding = new Padding(10);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "About";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label serverLinkLabel;
    }
}
