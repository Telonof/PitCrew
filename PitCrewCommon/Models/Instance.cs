using System.Xml.Linq;

namespace PitCrewCommon.Models
{
    public class Instance
    {
        public string Location { get; set; }
        public int PackageVersion { get; set; }
        public List<Mod> Mods { get; set; } = [];
        public bool IsCLI { get; set; }

        public Instance(string path)
        {
            Location = path;
        }

        public void LoadFromXML(bool cli = false)
        {
            if (!File.Exists(Location) || new FileInfo(Location).Length < 1)
                GenerateDummy();

            XDocument document = XDocument.Load(Location);
            IsCLI = cli;

            if (document.Root.Attribute("packageversion") == null || !int.TryParse(document.Root.Attribute("packageversion").Value, out int packageVersion))
                packageVersion = 5;

            PackageVersion = packageVersion;

            foreach (XElement element in document.Root.Elements("mod"))
            {
                Mod mod = new Mod(this);
                mod.LoadFromElement(element);
                Mods.Add(mod);
            }
        }

        private void GenerateDummy()
        {
            XElement rootElement = new XElement("instance");
            rootElement.SetAttributeValue("packageversion", PackageVersion);

            XDocument document = new XDocument();
            document.Add(rootElement);
            document.Save(Location);
        }

        public void SaveToXML()
        {
            //Root instance with server attribute
            XElement rootElement =  new XElement("instance");
            rootElement.SetAttributeValue("packageversion", PackageVersion);
            
            foreach (Mod mod in Mods)
            {
                XElement modElement = new XElement("mod");
                modElement.SetAttributeValue("id", mod.Id);
                modElement.SetAttributeValue("enabled", mod.Enabled);
                foreach (ModFile file in mod.ModFiles)
                {
                    XElement fileElement = new XElement("file");
                    fileElement.SetAttributeValue("priority", file.Priority);
                    fileElement.SetAttributeValue("loc", file.Location);
                    modElement.Add(fileElement);
                }
                rootElement.Add(modElement);
            }

            XDocument document = new XDocument();
            document.Add(rootElement);
            document.Save(Location);
        }

        public string GetDirectory()
        {
            return Path.GetDirectoryName(Location);
        }
    }
}
