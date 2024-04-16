using PitCrewCompiler;

namespace PitCrew.GUI.MainWindow.Components
{
    internal class Buttons
    {
        public void SaveButton_Click(object sender, EventArgs e)
        {
            Utils.SaveFile();
        }

        public void CompileButton_Click(object sender, EventArgs e)
        {
            Utils.SaveFile();

            API.compileManifest(Utils.GetForm().manifestLoc);
            MessageBox.Show("Mods succesfully added.");
        }
    }
}
