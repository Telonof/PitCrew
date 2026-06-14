using PitCrewCommon;
using PitCrewCommon.Models;
using PitCrewCommon.Utilities;

namespace PitCrewCompiler.DataInserters
{
    internal class StartupInserter
    {
        public StartupInserter(string directory, List<ModFile> files)
        {
            foreach (ModFile file in files)
            {
                Logger.Print(string.Format(Translatable.Get("compiler.merging-startup-file"), Path.GetFileName(file.Location)));

                string fullPath = Path.Combine(directory, file.Location + ".fat");

                BigFileUtil.UnpackBigFile(fullPath, FileUtil.GetTempFolder("startupdata"), file.ParentMod.ParentInstance.PackageVersion);

                //We ignore filesinfos.xml here because PitCrew relies on that for regular mods.
                FileUtil.CheckAndDeleteFile(Path.Combine(FileUtil.GetTempFolder("startupdata"), "engine", "filesinfos.xml"));

                foreach (string modFile in Directory.GetFiles(FileUtil.GetTempFolder("startupdata"), "*", SearchOption.AllDirectories))
                {
                    string relativePath = Path.GetRelativePath(FileUtil.GetTempFolder("startupdata"), modFile);
                    string copyPath = Path.Combine(FileUtil.GetTempFolder("startup_temp"), relativePath);

                    //Make sure the folder exists if modder added a completely new folder.
                    FileUtil.CheckAndCreateFolder(Path.GetDirectoryName(copyPath));

                    File.Copy(modFile, copyPath, true);
                }

                FileUtil.CheckAndDeleteFolder(FileUtil.GetTempFolder("startupdata"));
                PercentageCalculator.IncrementProgress();
            }
        }
    }
}
