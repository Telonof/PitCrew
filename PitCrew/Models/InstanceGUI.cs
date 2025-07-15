using CommunityToolkit.Mvvm.ComponentModel;
using PitCrewCommon.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace PitCrew.Models
{
    public partial class InstanceGUI : ModelConverter<Instance>
    {
        [ObservableProperty]
        public string name;

        [ObservableProperty]
        public string location;

        [ObservableProperty]
        public ObservableCollection<ModGUI> modsGUI;

        public InstanceGUI(Instance instance, string name) : base(instance)
        {
            Name = name;
            Location = instance.Location;
        }

        public void LoadFromXML()
        {
            BaseModel.LoadFromXML();
            ModsGUI = new ObservableCollection<ModGUI>(BaseModel.Mods.Select(mod => new ModGUI(mod)));
        }

        public void SaveToXML()
        {
            BaseModel.Mods = ModsGUI.Select(mod => {
                mod.SaveToBase();
                mod.BaseModel.Metadata.Save();
                return mod.BaseModel;
            }).ToList();

            BaseModel.SaveToXML();
        }
    }
}
