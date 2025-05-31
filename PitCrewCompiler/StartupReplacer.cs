using PitCrewCommon;

namespace PitCrewCompiler
{
    class StartupReplacer
    {
        //Goal here is to unpack the files of each startup mod and dump it into the startup of the main game.
        public StartupReplacer(string directory, List<string> files)
        {
            BigFileUtil.UnpackBigFile(Path.Combine(directory, "startup.fat"), "tmp");

            if (files.Count < 1)
                return;

            foreach (string file in files)
            {
                string fullPath = Path.Combine(directory, file + ".fat");

                BigFileUtil.UnpackBigFile(fullPath, "moddata");

                //We ignore filesinfos.xml here because PitCrew relies on that for regular mods.
                FileUtil.checkAndDeleteFile(Path.Combine("moddata", "engine", "filesinfos.xml"));

                foreach (string modFile in Directory.GetFiles("moddata", "*", SearchOption.AllDirectories))
                {
                    string relativePath = modFile[8..];
                    string copyPath = Path.Combine("tmp", relativePath);

                    //Make sure the folder exists if modder added a completely new folder.
                    FileUtil.checkAndCreateFolder(Path.GetDirectoryName(copyPath));

                    File.Copy(modFile, copyPath, true);
                }

                FileUtil.checkAndDeleteFolder("moddata");
            }
        }
    }
}
