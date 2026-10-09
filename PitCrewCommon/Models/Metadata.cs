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

        public string CompiledVersion { get; set; }

        public Version Version { get; set; } = Version.Parse("0.0.0");

        public long InstallTime { get; set; }

        public int MwsId { get; set; } = -1;

        public List<string[]> FoundModInfo { get; set; } = [];

        
        public Metadata(string location)
        {
            this.Location = location;

            if (!File.Exists(location))
            {
                GenerateDefaultIfNeeded();
                return;
            }

            XDocument doc;
            try
            {
                doc = XDocument.Load(location);
            }
            catch (XmlException)
            {
                Logger.Error(207, string.Format(Translatable.Get("parser.bad-metadata"), location));
                return;
            }

            LocalizedNames = doc.Root.Element("names")?.Elements().ToDictionary(element => element.Name.LocalName, element => element.Value) ?? [];
            LocalizedDescriptions = doc.Root.Element("descriptions")?.Elements().ToDictionary(element => element.Name.LocalName, element => element.Value) ?? [];

            if (LocalizedNames.Count > 1)
            {
                LocalizedNames = LocalizedNames.Where(langName => Translatable.IsValidCulture(langName.Key)).ToDictionary();
                LocalizedDescriptions = LocalizedDescriptions.Where(langName => Translatable.IsValidCulture(langName.Key)).ToDictionary();
                GenerateDefaultIfNeeded();
            }

            Author = doc.Root.Element("author").Value;
            CompiledVersion = doc.Root.Element("compiledVersion")?.Value;
            
            string mwsIdString = doc.Root.Element("mws-id")?.Value;
            if (!string.IsNullOrWhiteSpace(mwsIdString) && int.TryParse(mwsIdString, out int mwsId))
                MwsId = mwsId;

            string version = doc.Root.Element("version")?.Value;
            if (version != null && Version.TryParse(version, out Version parsedVersion))
                Version = parsedVersion;

            var filesElement = doc.Root.Element("files");
            if (filesElement == null)
                return;

            foreach (XElement fileElement in filesElement.Elements())
            {
                FoundModInfo.Add([fileElement.Attribute("priority")?.Value ?? "0", fileElement.Attribute("loc")?.Value ?? ""]);
            }
        }

        public string GrabNameOrDefault(string lang)
        {
            if (LocalizedNames.Count == 0)
                return Name;

            if (!LocalizedNames.ContainsKey(lang))
                return LocalizedNames.First().Value;

            return LocalizedNames[lang];
        }

        public string GrabDescriptionOrDefault(string lang)
        {
            if (LocalizedDescriptions.Count == 0)
                return Description;

            if (!LocalizedDescriptions.ContainsKey(lang))
                return LocalizedDescriptions.First().Value;

            return LocalizedDescriptions[lang];
        }

        public void Save(bool install = true)
        {
            FileUtil.CheckAndCreateFolder(Path.GetDirectoryName(Location));

            XDocument xmlDoc = new XDocument();
            xmlDoc.Add(Generate(install));
            xmlDoc.Save(Location);
        }

        private void GenerateDefaultIfNeeded()
        {
            if (LocalizedNames.Count == 0)
                LocalizedNames.Add(Translatable.GetCurrentLanguage(), Name);

            if (LocalizedDescriptions.Count == 0)
                LocalizedDescriptions.Add(Translatable.GetCurrentLanguage(), Description);
        }

        private XElement Generate(bool installed)
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

            if (installed)
                root.Add(new XElement("installTime", InstallTime));
            
            if (installed && MwsId != -1)
                root.Add(new XElement("mws-id", MwsId));

            root.Add(new XElement("compiledVersion", ManifestUtil.FormatVersion()));
            root.Add(new XElement("version", Version));

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

            return root;
        }
    }
}