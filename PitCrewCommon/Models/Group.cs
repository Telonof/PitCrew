using System.Xml.Linq;

namespace PitCrewCommon.Models
{
    public class Group
    {
   
        public List<Mod> Mods { get; set; } = [];

        public string Name { get; set; } = Translatable.Get("modinfo.default-name");

        public bool Enabled { get; set; } = false;

        public bool IsDefault { get; set; }

        public bool Opened { get; set; } = false;

        public Group(bool def = false)
        {
            IsDefault = def;
            if (IsDefault)
                Opened = true;
        }

        public void LoadFromElement(Instance instance, XElement element)
        {
            foreach (XElement modElement in element.Elements("mod"))
            {
                Mod mod = new Mod(instance);
                mod.LoadFromElement(modElement);
                if (mod.Enabled)
                    Enabled = true;

                Mods.Add(mod);
            }

            XAttribute name = element.Attribute("name");
            if (name != null && !string.IsNullOrEmpty(name.Value))
                Name = name.Value;

            name = element.Attribute("gui-opened");
            if (name != null && bool.TryParse(name.Value, out bool opened))
                Opened = opened;
        }
    }
}
