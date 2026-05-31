using Gibbed.Dunia2.FileFormats;
using Gibbed.Dunia2.FileFormats.Big;
using Gibbed.IO;
using PitCrewCommon;
using PitCrewCommon.Models;

namespace PitCrewCompiler.DataInserters
{
    internal class ShaderInserter
    {

        private Dictionary<string, Stream> _activeStreams = [];


        //53 seconds on boot, this is bad.
        //10 seconds subsequent.
        public ShaderInserter(string directory, List<ModFile> modFiles)
        {
            if (modFiles.Count == 0)
                return;

            Dictionary<ulong, EntryWithPath> entries = [];

            Logger.Print(Translatable.Get("compiler.merging-shaders"));

            //Add every shader entry unless it already exists as the goal is to have the mods on top of the list have their files prioritized like dat/fat/xml's already.
            foreach (ModFile modFile in modFiles)
            {
                //We only want fat/dat combos as they have shader files, not mod xml's.
                if (Path.GetExtension(modFile.Location).Equals(".xml"))
                    continue;

                string path = Path.Combine(directory, modFile.Location);

                BigFile fat = new BigFile();
                using (var input = File.OpenRead($"{path}.fat"))
                {
                    fat.Deserialize(input);
                }

                foreach (Entry entry in fat.Entries)
                {
                    if (entries.ContainsKey(entry.NameHash))
                        continue;

                    EntryWithPath entryWithPath = new EntryWithPath
                    {
                        entryInfo = entry,
                        datPath = $"{path}.dat"
                    };

                    entries.TryAdd(entry.NameHash, entryWithPath);
                }
            }

            //get original fat to read from
            string backupFile = BackupAndCheck(directory);
            BigFile shadersFat = new BigFile();
            using (var input = File.OpenRead(backupFile))
            {
                shadersFat.Deserialize(input);
            }

            Stream originalFile = new FileStream(Path.ChangeExtension(backupFile, ".dat"), FileMode.Open, FileAccess.Read, FileShare.Read);
            Stream moddedFile = new FileStream(Path.Combine(directory, "shadersobj.dat"), FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan);
            
            for (int i = 0; i < shadersFat.Entries.Count; i++)
            {
                Entry entry = shadersFat.Entries[i];

                Stream loadedStream = originalFile;
                if (entries.TryGetValue(entry.NameHash, out EntryWithPath value))
                {
                    loadedStream = CacheStream(value.datPath);
                    entry = value.entryInfo;
                }

                loadedStream.Seek(entry.Offset, SeekOrigin.Begin);
                entry.Offset = moddedFile.Position;
                
                moddedFile.WriteFromStream(loadedStream, entry.CompressedSize);
                long align = moddedFile.Length.Align(16) - moddedFile.Length;
                moddedFile.WriteBytes(new byte[align]);
                shadersFat.Entries[i] = entry;
            }

            foreach (Stream stream in _activeStreams.Values)
            {
                stream.Dispose();
            }
            
            originalFile.Dispose();
            moddedFile.Dispose();

            moddedFile = File.Open(Path.Combine(directory, "shadersobj.fat"), FileMode.Create);
            shadersFat.Serialize(moddedFile);
            moddedFile.Dispose();
        }

        private Stream CacheStream(string datPath)
        {
            if (!_activeStreams.ContainsKey(datPath))
                _activeStreams[datPath] = File.OpenRead(datPath);

            return _activeStreams[datPath];
        }

        /* This backs up shadersobj so people can recover from bad shader mods. We also use it as our way to merge into shadersobj to work from a fully vanilla file. */
        private string BackupAndCheck(string directory)
        {
            string shadersObj = Path.Combine(directory, "shadersobj.fat");
            string shadersBackupObj = Path.Combine(directory, "shadersobj_pitcrewbak.fat");
            
            if (!File.Exists(shadersObj))
            {
                Logger.Error(106, string.Format(Translatable.Get("compiler.missing-file"), "shadersobj"));
                throw new FileNotFoundException();
            }

            if (File.Exists(shadersBackupObj))
                return shadersBackupObj;
            
            File.Copy(shadersObj, shadersBackupObj);
            File.Copy(Path.ChangeExtension(shadersObj, ".dat"), Path.ChangeExtension(shadersBackupObj, ".dat"));

            return shadersBackupObj;
        }

        private struct EntryWithPath
        {
            public Entry entryInfo;
            public string datPath;
        }
    }
}
