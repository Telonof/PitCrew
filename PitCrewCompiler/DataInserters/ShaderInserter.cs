using Gibbed.Dunia2.FileFormats;
using Gibbed.Dunia2.FileFormats.Big;
using PitCrewCommon;
using PitCrewCommon.Models;
using PitCrewCommon.Utilities;

namespace PitCrewCompiler.DataInserters
{
    internal class ShaderInserter
    {
        public ShaderInserter(string directory, List<ModFile> modFiles)
        {
            if (modFiles.Count == 0)
            {
                BackupAndRestore(directory, true);
                return;
            }

            HashSet<ulong> entries = [];
            BigFile shadersFat = new BigFile();

            //We copy the backup back to the original just to work with a fully clean file and not ontop of existing modded files.
            //PitCrew is designed to work from vanilla files recompiling.
            string fatFilePath = BackupAndRestore(directory);
            var fatFile = new FileStream(fatFilePath, FileMode.Open, FileAccess.ReadWrite);
            shadersFat.Deserialize(fatFile);

            Logger.Print(Translatable.Get("compiler.merging-shaders"));

            //Add every shader entry.
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
                    entries.Add(entry.NameHash);
                }
            }

            for (int i = 0;  i < shadersFat.Entries.Count; i++)
            {
                Entry entry = shadersFat.Entries[i];
                if (!entries.Contains(entry.NameHash))
                    continue;

                shadersFat.Entries.Remove(entry);
            }

            fatFile.Dispose();
            fatFile = new FileStream(fatFilePath, FileMode.Create, FileAccess.Write);
            shadersFat.Serialize(fatFile);
            fatFile.Dispose();
        }

        /* This backs up shadersobj.fat so people can recover from bad shader mods. We also use it as our way to work from a fully vanilla file. */
        private string BackupAndRestore(string directory, bool delete = false)
        {
            string shadersObj = Path.Combine(directory, "shadersobj.fat");
            string shadersBackupObj = Path.Combine(directory, "shadersobj_pitcrewbak.fat");
            
            if (!File.Exists(shadersObj))
            {
                Logger.Error(106, string.Format(Translatable.Get("compiler.missing-file"), "shadersobj"));
                throw new FileNotFoundException();
            }

            if (File.Exists(shadersBackupObj))
                File.Copy(shadersBackupObj, shadersObj, true);
            else
                File.Copy(shadersObj, shadersBackupObj);

            if (delete)
                FileUtil.CheckAndDeleteFile(shadersBackupObj);

            FileUtil.DisableReadonly(shadersObj);
            return shadersObj;
        }
    }
}
