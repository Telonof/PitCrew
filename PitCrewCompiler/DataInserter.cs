using System.Diagnostics;
using System.IO.Compression;
using System.Xml.Linq;

namespace PitCrewCompiler
{
    internal class DataInserter
    {
        private readonly Dictionary<string, string> data;
        private readonly string directory;
        private XDocument xmlFile;
        private readonly List<string> archiveNames = new List<string>();
        private readonly Dictionary<string, string> config = new Dictionary<string, string>();

        public DataInserter(Dictionary<string, string> data, String directory)
        {
            this.data = data;
            this.directory = directory;

            GetConfig();
            Unpack();
            ScanFileInfo();
            ScanForConflict();
            InsertModdedFiles();
            Console.WriteLine("Repacking...");
            Repack();
        }

        private void GetConfig()
        {
            //Set default config options
            if (!File.Exists("config.txt"))
            {
                config["unpacker"] = "tools\\Gibbed.Dunia2.Unpack.exe";
                config["packer"] = "tools\\Gibbed.Dunia2.Pack.exe";
                return;
            }

            string[] lines = File.ReadAllLines("config.txt");

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] configLine = line.Split('=');
                config[configLine[0]] = configLine[1];
            }
        }

        private void Unpack()
        {
            string arguments = $"-o \"{Path.Combine(directory, "startup.fat")}\" tmp";
            ProgramExecution(config["unpacker"], arguments);
        }

        private void ScanFileInfo()
        {
            string filesinfosPath = Path.Combine("tmp", "engine", "filesinfos.xml");
            xmlFile = XDocument.Load(filesinfosPath);

            //Remove any mods previously added by the tool.
            var modsComment = xmlFile.DescendantNodes().OfType<XComment>()
                                  .FirstOrDefault(c => c.Value.Contains("PitCrew mods"));

            if (modsComment != null)
            {
                modsComment.NodesAfterSelf().Remove();
                modsComment.Remove();
            }

            xmlFile.Save(filesinfosPath);

            //Get all existing mods and store them to test for conflicts later.
            IEnumerable<XElement> archiveElements = xmlFile.Descendants("Archive");

            foreach (var archiveElement in archiveElements)
            {
                string? archiveName = archiveElement.Attribute("Name")?.Value;
                if (archiveName != null)
                {
                    archiveNames.Add(archiveName);
                }
            }
        }

        private void ScanForConflict()
        {
            foreach (string key in data.Keys)
            {
                if (!archiveNames.Contains(key))
                    continue;

                Console.WriteLine($"{key} has a conflict with your current filesinfos.xml");
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
                    new XAttribute("IsStartChunk", "0"),
                    new XAttribute("IsAutoOpenOnChunkInstalled", "1"),
                    new XAttribute("ChunkId", "4"),
                    new XAttribute("Priority", data[key]));

                xmlFile.Root.Add(newArchive);
            }
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

            string arguments = $"-c -v \"{Path.Combine(directory, "startup.fat")}\" tmp";
            ProgramExecution(config["packer"], arguments);

            Directory.Delete("tmp", true);
            Console.WriteLine("All packed up and ready to go!");
        }

        private void ProgramExecution(string tool, string arguments)
        {
            if (!File.Exists(tool))
            {
                Console.WriteLine($"{tool} does not exist.");
                Environment.Exit(1);
            }
            
            ProcessStartInfo startInfo = new ProcessStartInfo(tool);
            startInfo.Arguments = arguments;

            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }
    }
}
