using Avalonia.Platform.Storage;
using PitCrew.Models;
using PitCrew.Systems;
using PitCrewCommon;
using PitCrewCommon.Models;
using PitCrewCommon.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace PitCrew.ViewModels
{
    internal class ListBoxViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel MainWindow;

        public ListBoxViewModel(MainWindowViewModel mainWindow)
        {
            this.MainWindow = mainWindow;
        }

        public async void PackageMod()
        {
            if (MainWindow.LoadedMod == null)
                return;

            string baseDirectory = MainWindow.LoadedInstance.BaseModel.GetDirectory();
            string modId = MainWindow.LoadedMod.BaseModel.Id;
            Metadata metadata = MainWindow.LoadedMod.BaseModel.Metadata;

            //We do this to check if the mod that's trying to be packaged isn't the default metadata assigned.
            if (!File.Exists(metadata.Location))
            {
                await Service.WindowManager.ShowDialog(MainWindow, new MessageBoxViewModel(Translatable.Get("modlist.packagemod.no-metadata")));
                return;
            }

            if (MainWindow.LoadedMod.ModFilesGUI.Count == 0)
            {
                await Service.WindowManager.ShowDialog(MainWindow, new MessageBoxViewModel(Translatable.Get("modlist.packagemod.empty-mod")));
                return;
            }

            if (FileUtil.FileInUse($"{modId}.zip"))
            {
                await Service.WindowManager.ShowDialog(MainWindow, new MessageBoxViewModel(Translatable.Get("modlist.packagemod.file-in-use")));
                return;
            }

            //Clear this array as it's only used for packing, we don't need duplicate files found.
            metadata.FoundModInfo.Clear();

            var zip = new ZipArchive(File.Open($"{modId}.zip", FileMode.Create), ZipArchiveMode.Create);

            foreach (ModFile modFile in MainWindow.LoadedMod.BaseModel.ModFiles)
            {
                string baseName = Path.GetFileName(modFile.Location);
                metadata.FoundModInfo.Add([modFile.Priority.ToString(), baseName]);

                if (!Path.HasExtension(modFile.Location))
                {
                    zip.CreateEntryFromFile(Path.Combine(baseDirectory, modFile.Location + ".fat"), baseName + ".fat");
                    zip.CreateEntryFromFile(Path.Combine(baseDirectory, modFile.Location + ".dat"), baseName + ".dat");
                    continue;
                }

                zip.CreateEntryFromFile(Path.Combine(baseDirectory, modFile.Location), baseName);
            }

            //Add metadata to zip
            metadata.Save();
            zip.CreateEntryFromFile(Path.Combine(baseDirectory, Constants.METADATA_FOLDER, $"{modId}.mdata"), $"{modId}.mdata");

            zip.Dispose();

            var result = new MessageBoxViewModel(string.Format(Translatable.Get("modlist.packagemod.success"), modId), MessageBoxViewModel.ButtonType.YesNo);
            await Service.WindowManager.ShowDialog(MainWindow, result);
            if (result.Result != MessageBoxViewModel.ResultType.OK)
                return;

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = FileUtil.GetProcessDir(),
                UseShellExecute = true
            });
        }

        public async void EditMetadata()
        {
            if (MainWindow.LoadedMod == null)
                return;

            ModWindowViewModel viewModel = new ModWindowViewModel(MainWindow.LoadedMod, MainWindow.LoadedInstance.BaseModel.GetDirectory());
            await Service.WindowManager.ShowDialog(MainWindow, viewModel);

            if (!viewModel.SubmitClose)
                return;

            MainWindow.LoadedMod.Name = viewModel.Name;
            MainWindow.LoadedMod.Author = viewModel.Author;
            MainWindow.LoadedMod.Description = viewModel.Description;
        }

        public async void DeleteMod()
        {
            if (MainWindow.LoadedMod == null)
                return;

            string baseDirectory = MainWindow.LoadedInstance.BaseModel.GetDirectory();

            if (FileUtil.ProcessRunning(baseDirectory, MainWindow.LoadedInstance.BaseModel.PackageVersion))
            {
                var error = new MessageBoxViewModel(Translatable.Get("game-running"));
                await Service.WindowManager.ShowDialog(MainWindow, error);
                return;
            }

            var result = new MessageBoxViewModel(Translatable.Get("modlist.deletemod-warning"), MessageBoxViewModel.ButtonType.OkCancel);
            await Service.WindowManager.ShowDialog(MainWindow, result);
            if (result.Result != MessageBoxViewModel.ResultType.OK)
                return;

            foreach (ModGUI mod in MainWindow.SelectedMods.ToList())
            {
                File.Delete(Path.Combine(baseDirectory, Constants.METADATA_FOLDER, $"{mod.BaseModel.Id}.mdata"));

                foreach (ModFileGUI file in mod.ModFilesGUI)
                {
                    if (!Path.HasExtension(file.Location))
                    {
                        FileUtil.CheckAndDeleteFile(Path.Combine(baseDirectory, Path.ChangeExtension(file.Location, ".dat")));
                        FileUtil.CheckAndDeleteFile(Path.Combine(baseDirectory, Path.ChangeExtension(file.Location, ".fat")));
                    }
                    else
                    {
                        FileUtil.CheckAndDeleteFile(Path.Combine(baseDirectory, file.Location));
                    }

                    //Stops the empty mod file for adding new mods from triggering below.
                    if (string.IsNullOrWhiteSpace(file.Location))
                        continue;

                    //Clear generated directory if no more files are associated in it.
                    string modDirectory = Path.Combine(baseDirectory, Path.GetDirectoryName(file.Location));
                    if (Directory.GetFiles(modDirectory).Length == 0)
                        FileUtil.CheckAndDeleteFolder(modDirectory);

                }

                MainWindow.LoadedInstance.ModsGUI.Remove(mod);
            }

            MainWindow.LoadedMod = null;
            MainWindow.SelectedMods.Clear();
            MainWindow.Save();
        }

        public async void ImportModDragDrop(IEnumerable<IStorageItem> items)
        {
            foreach (var item in items)
            {
                string path = item.Path.LocalPath;
                if (!Path.GetExtension(path).Equals(".mdata") && !Path.GetExtension(path).Equals(".zip"))
                {
                    await Service.WindowManager.ShowDialogMainWindow(new MessageBoxViewModel(Translatable.Get("importmod.invalid-extension")));
                    continue;
                }
                MainWindow.ImportMod(path);
            }
        }

        public void MoveModUp()
        {
            if (MainWindow.LoadedMod == null)
                return;

            SortedDictionary<int, ModGUI> mods = [];

            //only move mods that aren't at the top or bottom of the list.
            foreach (ModGUI mod in MainWindow.SelectedMods)
            {
                int modIndex = MainWindow.LoadedInstance.ModsGUI.IndexOf(mod);
                if (modIndex == 0)
                    continue;

                mods.Add(modIndex, mod);
            }

            //ensure everything moves from top to bottom.
            foreach (int modIndex in mods.Keys)
            {
                MainWindow.LoadedInstance.ModsGUI.Move(modIndex, modIndex - 1);
            }

            //Observable collection doesn't have AddRange?
            foreach (ModGUI mod in mods.Values)
            {
                MainWindow.SelectedMods.Add(mod);
            }
        }

        public void MoveModDown()
        {
            if (MainWindow.LoadedMod == null)
                return;

            SortedDictionary<int, ModGUI> mods = [];

            foreach (ModGUI mod in MainWindow.SelectedMods)
            {
                int modIndex = MainWindow.LoadedInstance.ModsGUI.IndexOf(mod);
                if (modIndex >= MainWindow.LoadedInstance.ModsGUI.Count - 1)
                    continue;

                mods.Add(modIndex, mod);
            }

            foreach (int modIndex in mods.Keys.Reverse())
            {
                MainWindow.LoadedInstance.ModsGUI.Move(modIndex, modIndex + 1);
            }

            foreach (ModGUI mod in mods.Values)
            {
                MainWindow.SelectedMods.Add(mod);
            }
        }
    }
}
