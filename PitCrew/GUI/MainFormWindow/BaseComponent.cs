namespace PitCrew.GUI.MainWindow
{
    internal abstract class BaseComponent
    {    
        internal MainForm form;

        public BaseComponent(MainForm form)
        {
            this.form = form;
        }
    }
}
