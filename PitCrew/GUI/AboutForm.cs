using System.Diagnostics;

namespace PitCrew
{
    partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            serverLinkLabel.MouseEnter += ServerLinkLabel_MouseEnter;
            serverLinkLabel.MouseLeave += ServerLinkLabel_MouseLeave;
        }

        private void ServerLinkLabel_Click(object sender, EventArgs e)
        {
            ProcessStartInfo webBrowserLink = new ProcessStartInfo("https://discord.com/invite/gUczTkphGE");
            webBrowserLink.UseShellExecute = true;
            Process.Start(webBrowserLink);
        }

        private void ServerLinkLabel_MouseEnter(object sender, EventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void ServerLinkLabel_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }
    }
}
