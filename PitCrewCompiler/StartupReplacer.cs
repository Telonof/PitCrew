using PitCrewCommon;

namespace PitCrewCompiler
{
    class StartupReplacer
    {
        //Goal here is to unpack the files of each startup mod and dump it into the startup of the main game.
        public StartupReplacer(string directory, List<string> files)
        {
            string cstartupFatPath = Path.Combine(directory, "cstartup.fat");
            string cstartupDatPath = Path.Combine(directory, "cstartup.dat");
            string startupFatPath = Path.Combine(directory, "startup.fat");
            string startupDatPath = Path.Combine(directory, "startup.dat");

            //cstartup is "clean startup" meaning we use a clean slate startup when it comes to modding it rather than overwriting modded startup.
            if (File.Exists(cstartupFatPath))
            {
                BigFileUtil.UnpackBigFile(cstartupFatPath, "tmp");
            }
            else
            {
                BigFileUtil.UnpackBigFile(startupFatPath, "tmp");
            }

            //Create cstartup if not existing yet
            File.Copy(startupFatPath, cstartupFatPath, false);
            File.Copy(startupDatPath, cstartupDatPath, false);

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
