using Dunia2.MergeBinaryObject;
using Gibbed.Dunia2.FileFormats;
using PitCrewCommon;
using System.Xml.Linq;

namespace PitCrewCompiler
{
    class BinaryInserter
    {
        private readonly string xmldirectory = "xmltemp";

        private readonly string outputFile = "mods/PitCrewBase.fat";

        private Dictionary<string, BinaryObjectFile> modifiedFiles = [];

        private HashSet<string> allowedFiles = new HashSet<string>()
        {
            ".bin", ".fcb", ".bwo"
        };

        private List<string> patches = new List<string>()
        {
            "", "_patch", "_patch_1"
        };

        public BinaryInserter(string manifestFile, List<string> xmlFiles)
        {
            BinaryObjectMerger merger = new BinaryObjectMerger();
            string datawinDirectory = Path.GetDirectoryName(manifestFile);
            
            //Extract all global_db's if existing, and overwrite any files for each
            foreach (string patch in patches)
            {
                string bigFile = Path.Combine(datawinDirectory, $"global_db{patch}.fat");
                if (File.Exists(bigFile))
                    BigFileUtil.UnpackBigFile(bigFile, xmldirectory);
            }

            //Clean out everything that's not a binary as we are only interested in merging binaries
            foreach (string item in Directory.GetFiles(xmldirectory, "*", SearchOption.AllDirectories))
            {
                if (!allowedFiles.Contains(Path.GetExtension(item)))
                    FileUtil.checkAndDeleteFile(item);
            }

            foreach (string xmlFile in xmlFiles)
            {
                string file = Path.Combine(datawinDirectory, xmlFile);

                XDocument xmlDoc = XDocument.Load(file);
                if (xmlDoc.Root.Attribute("file") == null)
                {
                    Console.WriteLine(string.Format(Translate.Get("compiler.no-attribute-in-root"), xmlFile));
                    continue;
                }

                //Attempt to find the file this xml is trying to merge into.
                string mergingPath = Path.Combine(xmldirectory, xmlDoc.Root.Attribute("file").Value);
                if (!File.Exists(mergingPath))
                {
                    Console.WriteLine(string.Format(Translate.Get("compiler.file-not-found-in-xml"), mergingPath));
                    continue;
                }

                //Found it, now check the map to see if we already modded it.
                BinaryObjectFile bof;

                if (!modifiedFiles.ContainsKey(mergingPath))
                {
                    BinaryObjectFile binaryObjectFile = new BinaryObjectFile();
                    FileStream stream = File.OpenRead(mergingPath);
                    binaryObjectFile.Deserialize(stream);
                    stream.Close();
                    modifiedFiles.Add(mergingPath, binaryObjectFile);
                }
                bof = modifiedFiles[mergingPath];

                bof.Root = merger.Merge(bof.Root, xmlDoc);
            }

            //Convert all the objects back into their files and pack the directory
            foreach (string moddedFile in modifiedFiles.Keys)
            {
                FileStream stream = File.OpenWrite(moddedFile);
                modifiedFiles[moddedFile].Serialize(stream);
                stream.Close();
            }
            BigFileUtil.RepackBigFile(xmldirectory, Path.Combine(datawinDirectory, outputFile), "PitCrew");
            FileUtil.checkAndDeleteFolder(xmldirectory);
        }
    }
}
