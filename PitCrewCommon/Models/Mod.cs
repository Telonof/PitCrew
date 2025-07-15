using PitCrewCommon.Utilities;
using System.Xml.Linq;

namespace PitCrewCommon.Models
{
    public class Mod
    {
        public string Id { get; set; }
        public Metadata Metadata { get; set; }
        
        public bool Enabled { get; set; }
        public Instance ParentInstance { get; set; }
        public List<ModFile> ModFiles { get; set; } = [];

        public string Name
        {
            get => Metadata.Name;
            set => Metadata.Name = value;
        }

        public string Description
        {
            get => Metadata.Description;
            set => Metadata.Description = value;
        }

        public string Author
        {
            get => Metadata.Author;
            set => Metadata.Author = value;
        }

        public Mod(Instance parentInstance)
        {
            this.ParentInstance = parentInstance;
            Enabled = true;
        }

        public void LoadFromElement(XElement element)
        {
            if (element.Attribute("id") == null)
            {
                Logger.Error(206, Translatable.Get("compiler.cannot-find-id"));
                throw new InvalidOperationException();
            }

            Id = element.Attribute("id").Value;

            bool enabled = true;

            if (element.Attribute("enabled") != null)
                bool.TryParse(element.Attribute("enabled").Value, out enabled);

            Enabled = enabled;

            string mdatapath = Path.Combine(ParentInstance.GetDirectory(), Constants.METADATA_FOLDER, $"{Id}.mdata");

            LoadFromMData(mdatapath);

            foreach (XElement modFile in element.Elements("file"))
            {
                //Windows supports both / and \ but Linux only supports /
                string fileLoc = modFile.Attribute("loc").Value.Replace('\\', Path.DirectorySeparatorChar);
                string priority = modFile.Attribute("priority").Value;

                //We don't need the same file added multiple times, one only.
                if (ModFiles.Any(file => file.Location.Equals(fileLoc)))
                {
                    Logger.Warn(201, string.Format(Translatable.Get("parser.duplicate-file"), fileLoc));
                    continue;
                }

                if (!ManifestUtil.IsValidMod(this, ParentInstance.GetDirectory(), fileLoc, priority))
                    continue;

                ModFiles.Add(new ModFile(this, fileLoc, int.Parse(priority)));
                Logger.Print(string.Format(Translatable.Get("parser.successful-parse"), fileLoc));
            }
        }

        public void LoadFromMData(string mdataPath, bool import = false)
        {
            //We really don't care about metadata if a modder is running this directly from compiler.
            if (ParentInstance.IsCLI)
                return;

            Metadata = new Metadata(mdataPath);

            //if we don't return, then we know it's a brand new mod and we need additional data.
            if (!import)
                return;

            Id = Path.GetFileNameWithoutExtension(mdataPath);

            string baseDirectory = ParentInstance.GetDirectory();

            foreach (string[] modInfo in Metadata.FoundModInfo)
            {
                string newLocation = Path.Combine("mods", Id, modInfo[1]);
                if (!ManifestUtil.IsValidMod(this, ParentInstance.GetDirectory(), modInfo[1], modInfo[0], import))
                    continue;

                ModFiles.Add(new ModFile(this, newLocation, int.Parse(modInfo[0])));
                Logger.Print(string.Format(Translatable.Get("parser.successful-parse"), modInfo[1]));

                //Now move the file(s) associated with that modInfo to their correct position.
                FileUtil.CheckAndCreateFolder(Path.Combine(baseDirectory, "mods", Id, Path.GetDirectoryName(modInfo[1])));

                string currentFileLoc = Path.Combine(Path.GetDirectoryName(mdataPath), Path.GetFileName(modInfo[1]));
                string newFileLoc = Path.Combine(baseDirectory, newLocation);

                //Warn and don't copy
                if (!File.Exists(currentFileLoc))
                {
                    Logger.Error(208, string.Format(Translatable.Get("compiler.file-not-found-in-xml"), currentFileLoc));
                    continue;
                }

                if (!Path.HasExtension(currentFileLoc))
                {
                    File.Copy(Path.ChangeExtension(currentFileLoc, ".dat"), Path.ChangeExtension(newFileLoc, ".dat"), true);
                    File.Copy(Path.ChangeExtension(currentFileLoc, ".fat"), Path.ChangeExtension(newFileLoc, ".fat"), true);
                    continue;
                }
                File.Copy(currentFileLoc, newFileLoc, true);
            }

            //Now copy the metadata to it's correct location.
            string metadataFolder = Path.Combine(baseDirectory, Constants.METADATA_FOLDER);
            string fullMetadataPath = Path.Combine(metadataFolder, Path.GetFileName(mdataPath));

            //Already at location, do not copy.
            //if (mdataPath.Equals(fullMetadataPath))
            //    return;

            FileUtil.CheckAndCreateFolder(metadataFolder);
            File.Copy(mdataPath, fullMetadataPath, true);
            Metadata.Location = fullMetadataPath;
        }
    }
}
