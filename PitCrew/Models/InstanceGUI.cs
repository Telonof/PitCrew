using CommunityToolkit.Mvvm.ComponentModel;
using PitCrewCommon.Models;
using System.Collections.Generic;
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
        public ObservableCollection<GroupGUI> groupsGUI;

        [ObservableProperty]
        public string filterText;

        
        public InstanceGUI(Instance instance, string name) : base(instance)
        {
            Name = name;
            Location = instance.Location;
        }

        public bool LoadFromXML()
        {
            bool successfulLoad = BaseModel.LoadFromXML();
            GroupsGUI = new ObservableCollection<GroupGUI>(BaseModel.Groups.Select(group => new GroupGUI(group)));
            return successfulLoad;
        }

        public void SaveToXML()
        {
            BaseModel.Groups = GroupsGUI.Select(group =>
            {
                group.SaveToXML();
                return group.BaseModel;
            }).ToList();

            BaseModel.SaveToXML();
        }

        public GroupGUI? GetGroup(ModGUI mod)
        {
            return GroupsGUI.Where(group => group.ModsGUI.Contains(mod)).FirstOrDefault();
        }

        public GroupGUI? GetDefaultGroup(bool createDefault = true)
        {
            GroupGUI? defaultGroup = GroupsGUI.Where(group => group.BaseModel.IsDefault).FirstOrDefault();

            if (defaultGroup == null && createDefault)
            {
                Group group = new Group(true);
                defaultGroup = new GroupGUI(group);
                GroupsGUI.Insert(0, defaultGroup);
            }

            return defaultGroup;
        }

        public List<ModGUI> GetAllMods()
        {
            List<ModGUI> mods = [];

            foreach (GroupGUI group in GroupsGUI)
            {
                mods.AddRange(group.ModsGUI);
            }

            return mods;
        }

        partial void OnFilterTextChanged(string value)
        {
            foreach (GroupGUI group in GroupsGUI)
            {
                FilterGroup(group);
            }
        }

        private void FilterGroup(GroupGUI? group)
        {
            if (group == null)
                return;
            
            group.Visible = group.Filter(FilterText);
        }
    }
}
