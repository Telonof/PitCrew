using System.Xml.Linq;

namespace PitCrewCommon
{
    public class ConfigManager
    {
        public readonly XDocument Properties;

        public static readonly string fileName = "Properties.xml";

        public ConfigManager()
        {
            //Test for XML and create if not found.
            if (!File.Exists(fileName))
            {
                XElement config = new XElement("Root",
                    new XComment("If you don't know what you are doing, avoid touching this file!"),
                    new XElement("Instances"),
                    new XElement("Settings",
                        new XElement("Setting",
                            new XAttribute("Name", ConfigKey.Language),
                            new XAttribute("Value", "English")
                        ),
                        new XElement("Setting",
                            new XAttribute("Name", ConfigKey.Theme),
                            new XAttribute("Value", "Dark")
                        ),
                        new XElement("Setting",
                            new XAttribute("Name", ConfigKey.LastOpenedPath),
                            new XAttribute("Value", "")
                        )
                    )
                );
                config.Save(fileName);
            }
            Properties = XDocument.Load(fileName);
        }

        public string? GetSetting(string setting)
        {
            return Properties.Root?.Element("Settings")?
                            .Elements("Setting")?
                            .FirstOrDefault(at => at.Attribute("Name").Value.Equals(setting, StringComparison.OrdinalIgnoreCase))?
                            .Attribute("Value")?.Value;
        }

        public void SetSetting(string name, string value)
        {
            XElement? setting = Properties.Root.Element("Settings")?
                                .Elements("Setting")?
                                .FirstOrDefault(at => at.Attribute("Name").Value.Equals(name));

            setting?.SetAttributeValue("Value", value);

            Properties.Save(fileName);
        }
    }

    public class ConfigKey
    {
        public const string Language = "Lang";
        public const string Theme = "Theme";
        public const string LastOpenedPath = "LastOpenedPath";
    }
}
