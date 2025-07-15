using CommunityToolkit.Mvvm.ComponentModel;
using PitCrew.Systems;
using PitCrewCommon;
using PitCrewCommon.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace PitCrew.Models
{
    public partial class ModGUI : ModelConverter<Mod>
    {
        [ObservableProperty]
        public bool enabled;

        [ObservableProperty]
        public string name;

        [ObservableProperty]
        public string author;

        [ObservableProperty]
        public string description;

        [ObservableProperty]
        public ObservableCollection<ModFileGUI> modFilesGUI;

        public List<ulong> Hashes = [];

        public ModGUI(Mod mod) : base(mod)
        {
            //Get Name and Description based on language, first in list if lang not found.
            string language = Service.Config.GetSetting(ConfigKey.Language);

            name = mod.Metadata.LocalizedNames.First().Value;
            if (mod.Metadata.LocalizedNames.ContainsKey(language))
                name = mod.Metadata.LocalizedNames[language];

            description = mod.Metadata.LocalizedDescriptions.First().Value;
            if (mod.Metadata.LocalizedDescriptions.ContainsKey(language))
                description = mod.Metadata.LocalizedDescriptions[language];

            author = mod.Metadata.Author;
            Enabled = mod.Enabled;

            ModFilesGUI = new ObservableCollection<ModFileGUI>(BaseModel.ModFiles.Select(modFile => {
                var file = new ModFileGUI(modFile);
                Hashes.AddRange(GetAllHashes(modFile.Location));
                file.PropertyChanged += ModFileChanged;
                return file;
            }));

            var file = new ModFileGUI(new ModFile(BaseModel, "", 0));
            file.PropertyChanged += ModFileChanged;
            ModFilesGUI.Add(file);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            //Apply localization specific name and description
            string language = Service.Config.GetSetting(ConfigKey.Language);

            if (e.PropertyName == nameof(Name))
            {
                BaseModel.Metadata.LocalizedNames[language] = Name;
                BaseModel.Metadata.Save();
            }

            if (e.PropertyName == nameof(Description))
            {
                BaseModel.Metadata.LocalizedDescriptions[language] = Description;
                BaseModel.Metadata.Save();
            }

            if (e.PropertyName == nameof(Author))
                BaseModel.Metadata.Save();
        }


        protected void ModFileChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is not ModFileGUI modFile)
                return;

            List<ulong> hashes = GetAllHashes(modFile.PreviousLocation);

            if (modFile.ToDelete)
            {
                ModFilesGUI.Remove(modFile);
                Hashes.RemoveAll(hash => hashes.Contains(hash));
                return;
            }

            Hashes.AddRange(hashes);

            //We check if the edit was the last empty one and add a new empty one for new files.
            if (ReferenceEquals(modFile, ModFilesGUI.Last()))
            {
                var file = new ModFileGUI(new ModFile(BaseModel, "", 0));
                file.PropertyChanged += ModFileChanged;
                ModFilesGUI.Add(file);
            }
        }

        public void SaveToBase()
        {
            List<ModFile> files = [];

            foreach (ModFileGUI file in ModFilesGUI)
            {
                if (file.Priority == 0)
                    continue;

                files.Add(new ModFile(this.BaseModel, file.Location, file.Priority));
            }
            BaseModel.ModFiles = files;
        }

        private List<ulong> GetAllHashes(string location)
        {
            const int headerSize = 12;
            const int loopCountSize = 4;
            const int nameHashSize = 8;
            List<ulong> hashes = [];

            if (Path.GetExtension(location).Equals(".xml"))
                return hashes;

            location = Path.Combine(BaseModel.ParentInstance.GetDirectory(), Path.ChangeExtension(location, ".fat"));

            using (FileStream stream = new FileStream(location, FileMode.Open))
            {
                stream.Seek(headerSize, SeekOrigin.Begin);

                byte[] loopCountBytes = new byte[loopCountSize];
                stream.Read(loopCountBytes, 0, loopCountSize);
                uint loopCount = BitConverter.ToUInt32(loopCountBytes, 0);

                for (int i = 0; i < loopCount; i++)
                {
                    stream.Seek(nameHashSize, SeekOrigin.Current);

                    byte[] dataBytes = new byte[nameHashSize];
                    stream.Read(dataBytes, 0, nameHashSize);
                    ulong data = BitConverter.ToUInt64(dataBytes);

                    if (!hashes.Contains(data))
                        hashes.Add(data);

                    stream.Seek(nameHashSize, SeekOrigin.Current);
                }
            }
            return hashes;
        }
    }
}
