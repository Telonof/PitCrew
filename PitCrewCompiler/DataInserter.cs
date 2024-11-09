using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using PitCrewCommon;

namespace PitCrewCompiler
{
    internal class DataInserter
    {
        private readonly Dictionary<string, string> data;
        private readonly string directory;
        private XDocument xmlFile;
        private readonly List<string> archiveNames = [];

        public DataInserter(Dictionary<string, string> data, string directory)
        {
            this.data = data;
            this.directory = directory;

            BigFileUtil.UnpackBigFile(Path.Combine(directory, "startup.fat"), "tmp");
            ScanFileInfo();
            ScanForConflict();
            InsertModdedFiles();
            Repack();
        }

        private void ScanFileInfo()
        {
            string filesinfosPath = Path.Combine("tmp", "engine", "filesinfos.xml");
            xmlFile = XDocument.Load(filesinfosPath);

            //Remove any mods previously added by the tool.
            var sectionStartComment = xmlFile.DescendantNodes().OfType<XComment>()
                                        .FirstOrDefault(c => c.Value.Contains("PitCrew mods"));

            if (sectionStartComment != null)
            {
                var sectionEndComment = sectionStartComment.NodesAfterSelf().OfType<XComment>()
                                        .FirstOrDefault(c => c.Value.Contains("PitCrew mods end"));
                //Legacy
                if (sectionEndComment == null)
                {
                    sectionStartComment.NodesAfterSelf().Remove();
                }
                else
                {
                    List<XNode> nodes = sectionStartComment.NodesAfterSelf().TakeWhile(c => c != sectionEndComment).ToList();
                    nodes.ForEach(node => node.Remove());
                    sectionEndComment.Remove();
                }

                sectionStartComment.Remove();
            }

            xmlFile.Save(filesinfosPath);

            //Get all existing mods and store them to test for conflicts later.
            IEnumerable<XElement> archiveElements = xmlFile.Descendants("Archive");

            foreach (var archiveElement in archiveElements)
            {
                string? archiveName = archiveElement.Attribute("Name")?.Value;
                if (archiveName != null)
                    archiveNames.Add(archiveName);
            }
        }

        private void ScanForConflict()
        {
            foreach (string key in data.Keys)
            {
                if (!archiveNames.Contains(key))
                    continue;

                Console.WriteLine(string.Format(Translate.Get("compiler.conflict-in-original-file"), key));
                Directory.Delete("tmp", true);
                Environment.Exit(1);
            }
        }

        private void InsertModdedFiles()
        {
            XComment modsComment = new XComment("PitCrew mods");
            xmlFile.Root.Add(modsComment);

            foreach (string key in data.Keys)
            {
                //Create a new xml element and add it to the list.
                XElement newArchive = new XElement("Archive",
                    new XAttribute("Name", key),
                    new XAttribute("IsSkued", "0"),
                    new XAttribute("IsStartChunk", "1"),
                    new XAttribute("IsAutoOpenOnChunkInstalled", "1"),
                    new XAttribute("ChunkId", "4"),
                    new XAttribute("Priority", data[key]));

                xmlFile.Root.Add(newArchive);
            }

            modsComment = new XComment("PitCrew mods end");
            xmlFile.Root.Add(modsComment);

            xmlFile.Save(Path.Combine("tmp", "engine", "filesinfos.xml"));
        }

        private void Repack()
        {
            string backupPath = Path.Combine(directory, "startupbak.zip");

            //Backup startup if non existing yet.
            if (!File.Exists(backupPath))
            {
                using (var backupZip = ZipFile.Open(backupPath, ZipArchiveMode.Create))
                {
                    backupZip.CreateEntryFromFile(Path.Combine(directory, "startup.fat"), "startup.fat");
                    backupZip.CreateEntryFromFile(Path.Combine(directory, "startup.dat"), "startup.dat");
                }
            }

            BigFileUtil.RepackBigFile("tmp", Path.Combine(directory, "startup.fat"));
            Directory.Delete("tmp", true);
            Console.WriteLine(Translate.Get("compiler.success"));
        }
    }
}
