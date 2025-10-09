using PitCrewCommon.Utilities;
using System.Xml;
using System.Xml.Linq;

namespace PitCrewCommon.Models
{
    public class Metadata
    {
        public string Name { get; set; } = Translatable.Get("modinfo.default-name");
        public string Description { get; set; } = Translatable.Get("modinfo.default-description");
        public string Author { get; set; } = Translatable.Get("modinfo.default-author");

        public Dictionary<string, string> LocalizedNames { get; } = [];
        public Dictionary<string, string> LocalizedDescriptions { get; } = [];

        public string Location { get; set; }

        public List<string[]> FoundModInfo { get; set; } = [];

        public Metadata(string location)
        {
            this.Location = location;

            if (!File.Exists(location))
            {
                LocalizedNames.Add("English", Name);
                LocalizedDescriptions.Add("English", Description);
                return;
            }

            XDocument doc;
            try
            {
                doc = XDocument.Load(location);
            }
            catch (XmlException)
            {
                Logger.Error(207, Translatable.Get("parser.bad-metadata"));
                throw new XmlException();
            }

            LocalizedNames = doc.Root.Element("names")?.Elements().ToDictionary(element => element.Name.LocalName, element => element.Value) ?? [];
            LocalizedDescriptions = doc.Root.Element("descriptions")?.Elements().ToDictionary(element => element.Name.LocalName, element => element.Value) ?? [];
            Author = doc.Root.Element("author").Value;

            var filesElement = doc.Root.Element("files");
            if (filesElement == null)
                return;

            foreach (XElement fileElement in filesElement.Elements())
            {
                FoundModInfo.Add([fileElement.Attribute("priority")?.Value ?? "0", fileElement.Attribute("loc")?.Value ?? ""]);
            }
        }

        public void Save()
        {
            XElement root = new XElement("metadata");

            //Get names
            XElement names = new XElement("names");
            foreach (string lang in LocalizedNames.Keys)
            {
                names.Add(new XElement(lang, LocalizedNames[lang]));
            }
            root.Add(names);

            //Get Author
            root.Add(new XElement("author", Author));

            //Get Descriptions
            XElement descriptions = new XElement("descriptions");
            foreach (string lang in LocalizedDescriptions.Keys)
            {
                descriptions.Add(new XElement(lang, LocalizedDescriptions[lang]));
            }
            root.Add(descriptions);

            //Get Files
            XElement files = new XElement("files");
            foreach (string[] file in FoundModInfo)
            {
                XElement fileElement = new XElement("file");
                fileElement.SetAttributeValue("priority", file[0]);
                fileElement.SetAttributeValue("loc", file[1]);
                files.Add(fileElement);
            }
            root.Add(files);

            FileUtil.CheckAndCreateFolder(Path.GetDirectoryName(Location));

            XDocument xmlDoc = new XDocument();
            xmlDoc.Add(root);
            xmlDoc.Save(Location);
        }
    }
}