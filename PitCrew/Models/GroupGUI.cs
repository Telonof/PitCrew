using CommunityToolkit.Mvvm.ComponentModel;
using PitCrewCommon.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PitCrew.Models
{
    public partial class GroupGUI : ModelConverter<Group>
    {

        [ObservableProperty]
        public ObservableCollection<ModGUI> modsGUI;

        [ObservableProperty]
        public string name;

        //Setting that shows if in the GUI the group is opened or not.
        [ObservableProperty]
        public bool opened;

        [ObservableProperty]
        public string color = "#3AC244";

        //Used to hide groups if filtered out.
        [ObservableProperty] 
        public bool visible = true;

        private bool Enabled = true;


        public GroupGUI(Group group) : base(group)
        {
            ModsGUI = new ObservableCollection<ModGUI>(group.Mods.Select(mod => new ModGUI(mod)));
            Name = group.Name;
            Enabled = group.Enabled;
            Opened = group.Opened;
            SwitchColor();
        }

        public void SaveToXML()
        {
            BaseModel.Mods = ModsGUI.Select(mod =>
            {
                mod.SaveToBase();
                mod.BaseModel.Metadata.Save();
                return mod.BaseModel;
            }).ToList();
        }

        public bool Filter(string? text)
        {
            bool anyVisible = false;

            foreach (ModGUI mod in ModsGUI)
            {
                mod.Visible = false;
                if (ModFound(mod, text))
                {
                    anyVisible = true;
                    mod.Visible = true;
                }
            }

            return anyVisible;
        }

        public void SwapEnabledState()
        {
            ForceState(!Enabled);

            foreach (ModGUI mod in ModsGUI)
            {
                mod.Enabled = Enabled;
            }
        }

        public void ForceState(bool state)
        {
            Enabled = state;
            SwitchColor();
        }

        private void SwitchColor()
        {
            Color = Enabled ? "#3AC244" : "#C40000";
        }

        private bool ModFound(ModGUI mod, string? text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            return mod.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
                   || mod.Author.Contains(text, StringComparison.OrdinalIgnoreCase)
                   || mod.Description.Contains(text, StringComparison.OrdinalIgnoreCase);
        }
    }
}
