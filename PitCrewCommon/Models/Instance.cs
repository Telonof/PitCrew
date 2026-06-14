using System.Xml;
using PitCrewCommon.Utilities;
using System.Xml.Linq;

namespace PitCrewCommon.Models
{
    public class Instance
    {
        public string Location { get; set; }
        public int PackageVersion { get; set; }
        public List<Group> Groups { get; set; } = [];
        public bool IsCLI { get; set; }

        public Instance(string path)
        {
            Location = path;
        }

        public bool LoadFromXML(bool cli = false)
        {
            string root = FileUtil.GetParentDir(GetDirectory());

            if (!File.Exists(Location) || new FileInfo(Location).Length < 1)
                GenerateDummy();

            XDocument document;
            try
            {
                document = XDocument.Load(Location);
            }
            catch (XmlException e)
            {
                Logger.Error(209, Translatable.Get("instances.corrupted"));
                Logger.Error(209, e.Message);
                return false;
            }
            
            IsCLI = cli;

            if (document.Root.Attribute("packageversion") == null || !int.TryParse(document.Root.Attribute("packageversion").Value, out int packageVersion))
                packageVersion = ManifestUtil.GetPackageVersion(root);

            if (packageVersion != 5 && packageVersion != 6)
                packageVersion = ManifestUtil.GetPackageVersion(root);

            PackageVersion = packageVersion;

            foreach (XElement groupElement in document.Root.Elements("group"))
            {
                Group group = new Group();
                group.LoadFromElement(this, groupElement);

                if (group.Mods.Count > 0)
                    Groups.Add(group);
            }

            //Default group if no group found for some mods
            Group defaultGroup = new Group(true);
            defaultGroup.LoadFromElement(this, document.Root);

            if (defaultGroup.Mods.Count > 0)
                Groups.Add(defaultGroup);

            return true;
        }

        public void SaveToXML()
        {
            //Root instance with server attribute
            XElement rootElement =  new XElement("instance");
            rootElement.SetAttributeValue("packageversion", PackageVersion);

            foreach (Group group in Groups)
            {
                XElement groupElement = new XElement("group");
                groupElement.SetAttributeValue("name", group.Name);
                groupElement.SetAttributeValue("gui-opened", group.Opened);

                foreach (Mod mod in group.Mods)
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
                    groupElement.Add(modElement);
                }

                rootElement.Add(groupElement);
            }

            XDocument document = new XDocument();
            document.Add(rootElement);
            document.Save(Location);
        }

        public List<Mod> GetAllMods()
        {
            List<Mod> mods = [];

            foreach (Group group in Groups)
            {
                mods.AddRange(group.Mods);
            }

            return mods;
        }

        public string GetDirectory()
        {
            return Path.GetDirectoryName(Location);
        }

        private void GenerateDummy()
        {
            XElement rootElement = new XElement("instance");
            rootElement.SetAttributeValue("packageversion", 0);

            XDocument document = new XDocument();
            document.Add(rootElement);
            document.Save(Location);
        }
    }
}
