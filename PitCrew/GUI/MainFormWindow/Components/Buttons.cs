using System.Reflection;

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
            Assembly assembly = Assembly.LoadFrom("PitCrewCompiler.dll");

            MethodInfo entryPoint = assembly.EntryPoint;

            if (entryPoint == null)
                return;

            entryPoint.Invoke(null, new object[] { new string[] { Utils.GetForm().manifestLoc } });
            MessageBox.Show("Mods succesfully added.");
        }
    }
}
