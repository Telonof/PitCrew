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

            //Clear this array as it's only used for packing, we don't need duplicate files found.
            metadata.FoundModInfo.Clear();

            var result = new MessageBoxViewModel(string.Format(Translatable.Get("modlist.packagemod.success"), modId), MessageBoxViewModel.ButtonType.YesNo);
            await Service.WindowManager.ShowDialog(MainWindow, result);
            if (result.Result != MessageBoxViewModel.ResultType.OK)
                return;

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = Path.GetDirectoryName(Environment.ProcessPath),
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

            var result = new MessageBoxViewModel(Translatable.Get("modlist.deletemod-warning"), MessageBoxViewModel.ButtonType.OkCancel);
            await Service.WindowManager.ShowDialog(MainWindow, result);
            if (result.Result != MessageBoxViewModel.ResultType.OK)
                return;

            File.Delete(Path.Combine(baseDirectory, Constants.METADATA_FOLDER, $"{MainWindow.LoadedMod.BaseModel.Id}.mdata"));

            foreach (ModFileGUI file in MainWindow.LoadedMod.ModFilesGUI)
            {
                if (!Path.HasExtension(file.Location))
                {
                    FileUtil.CheckAndDeleteFile(Path.Combine(baseDirectory, Path.ChangeExtension(file.Location, ".dat")));
                    FileUtil.CheckAndDeleteFile(Path.Combine(baseDirectory, Path.ChangeExtension(file.Location, ".fat")));
                    continue;
                }

                FileUtil.CheckAndDeleteFile(Path.Combine(baseDirectory, file.Location));
            }

            MainWindow.LoadedInstance.ModsGUI.Remove(MainWindow.LoadedMod);
            MainWindow.LoadedMod = null;
            MainWindow.Save();
        }

        public async void ImportModDragDrop(IEnumerable<IStorageItem> items)
        {
            foreach (var item in items)
            {
                MainWindow.ImportMod(item.Path.LocalPath);
            }
        }
    }
}
