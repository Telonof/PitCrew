using PitCrew.Models;
using PitCrewCommon;
using PitCrewCommon.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace PitCrew.Systems
{
    internal class ConfigManagerGUI : ConfigManager
    {
        public void SetInstanceName(string oldName, string newName)
        {
            XElement? savedInstance = Properties.Root.Element("Instances")?
                                .Elements("Instance")?
                                .FirstOrDefault(at => at.Attribute("Name").Value.Equals(oldName));

            savedInstance?.SetAttributeValue("Name", newName);
            Properties.Save(fileName);
        }

        public void DeleteInstance(InstanceGUI instance) 
        {
            XElement? savedInstance = Properties.Root.Element("Instances")?
                        .Elements("Instance")?
                        .FirstOrDefault(at => at.Attribute("Name").Value.Equals(instance.Name));

            if (savedInstance != null)
            {
                savedInstance.Remove();
                Properties.Save(fileName);
            }
        }

        public void SaveInstance(InstanceGUI instance)
        {
            XElement xmlinstance = new XElement("Instance",
                                    new XAttribute("Name", instance.Name),
                                    new XAttribute("Path", instance.Location));

            Properties.Root.Element("Instances").Add(xmlinstance);
            Properties.Save(fileName);
        }

        public ObservableCollection<InstanceGUI> LoadInstances()
        {
            ObservableCollection<InstanceGUI> instances = [];

            foreach (var instance in Properties.Root.Element("Instances").Elements("Instance"))
            {
                string name = instance.Attribute("Name")?.Value;
                string path = instance.Attribute("Path")?.Value;

                instances.Add(new InstanceGUI(new Instance(path), name));
            }

            return instances;
        }      
    }
}
