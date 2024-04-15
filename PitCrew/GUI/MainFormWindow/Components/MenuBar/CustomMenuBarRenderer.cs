namespace PitCrew.GUI.MainWindow.Components.MenuBar
{
    internal class CustomMenuBarRenderer : ProfessionalColorTable
    {
        public override Color MenuItemSelected
        {
            get { return Color.FromArgb(51, 153, 255); }
        }

        public override Color ToolStripDropDownBackground
        {
            get { return Color.FromArgb(78, 78, 78); }
        }

        public override Color MenuItemSelectedGradientBegin
        {
            get { return Color.FromArgb(51, 153, 255); }
        }

        public override Color MenuItemSelectedGradientEnd
        {
            get { return Color.FromArgb(51, 153, 255); }
        }

        public override Color MenuItemPressedGradientBegin
        {
            get { return Color.FromArgb(51, 153, 255); }
        }

        public override Color MenuItemPressedGradientMiddle
        {
            get { return Color.FromArgb(51, 153, 255); }
        }

        public override Color MenuItemPressedGradientEnd
        {
            get { return Color.FromArgb(51, 153, 255); }
        }

        public override Color MenuItemBorder
        {
            get { return Color.FromArgb(51, 153, 255); }

        }
    }
}
