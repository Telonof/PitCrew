using PitCrewCommon;
using PitCrewCommon.Models;
using PitCrewCommon.Utilities;
using System.Xml.Linq;

namespace PitCrewCompiler.DataInserters
{
    internal class FilesInfosInserter
    {
        private readonly List<ModFile> ModFiles;
        private readonly string Directory;
        private XDocument XmlFile;
        private readonly List<string> ArchiveNames = [];

        public FilesInfosInserter(string directory, List<ModFile> modFiles)
        {
            Directory = directory;
            ModFiles = modFiles;

            ScanFileInfo();
            ScanForConflict();
            InsertModdedFiles();
            Repack();
        }

        private void ScanFileInfo()
        {
            string filesinfosPath = Path.Combine("tmp", "engine", "filesinfos.xml");
            XmlFile = XDocument.Load(filesinfosPath);

            //Remove any mods previously added by the tool.
            //There really shouldn't be any in cstartup, but this is a just in case.
            var sectionStartComment = XmlFile.DescendantNodes().OfType<XComment>()
                                        .FirstOrDefault(c => c.Value.Contains("PitCrew mods"));

            if (sectionStartComment != null)
            {
                var sectionEndComment = sectionStartComment.NodesAfterSelf().OfType<XComment>()
                                        .FirstOrDefault(c => c.Value.Contains("PitCrew mods end"));

                sectionStartComment.Remove();
            }

            XmlFile.Save(filesinfosPath);

            //Get all existing mods and store them to test for conflicts later.
            IEnumerable<XElement> archiveElements = XmlFile.Descendants("Archive");

            foreach (var archiveElement in archiveElements)
            {
                string? archiveName = archiveElement.Attribute("Name")?.Value;
                if (archiveName != null)
                    ArchiveNames.Add(archiveName);
            }
        }

        private void ScanForConflict()
        {
            List<ModFile> removals = [];
            foreach (ModFile modFile in ModFiles)
            {
                if (!ArchiveNames.Contains(modFile.Location))
                    continue;

                //Delete it and log it
                Logger.Error(105, string.Format(Translatable.Get("compiler.conflict-in-original-file"), modFile.Location));
                removals.Add(modFile);
            }
            ModFiles.RemoveAll(modFile => removals.Contains(modFile));
        }

        private void InsertModdedFiles()
        {
            XComment modsComment = new XComment("PitCrew mods");
            XmlFile.Root.Add(modsComment);

            foreach (ModFile modFile in ModFiles)
            {
                //Create a new xml element and add it to the list.
                XElement newArchive = new XElement("Archive",
                    new XAttribute("Name", modFile.Location),
                    new XAttribute("IsSkued", "0"),
                    new XAttribute("IsStartChunk", "1"),
                    new XAttribute("IsAutoOpenOnChunkInstalled", "1"),
                    new XAttribute("ChunkId", "4"),
                    new XAttribute("Priority", modFile.Priority));

                XmlFile.Root.Add(newArchive);
            }

            modsComment = new XComment("PitCrew mods end");
            XmlFile.Root.Add(modsComment);

            XmlFile.Save(Path.Combine("tmp", "engine", "filesinfos.xml"));
        }

        private void Repack()
        {
            int packageVersion = 5;

            if (ModFiles.Count > 0)
                packageVersion = ModFiles[0].ParentMod.ParentInstance.PackageVersion;

            BigFileUtil.RepackBigFile("tmp", Path.Combine(Directory, "startup.fat"), packageVersion, "PitCrew");
            FileUtil.CheckAndDeleteFolder("tmp");
        }
    }
}
