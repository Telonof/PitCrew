using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
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
using System.Reflection.Metadata.Ecma335;

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
            if (MainWindow.SelectedItem is not ModGUI mod)
                return;

            mod.SaveToBase();

            string baseDirectory = MainWindow.LoadedInstance.BaseModel.GetDirectory();
            string modId = mod.BaseModel.Id;
            Metadata metadata = mod.BaseModel.Metadata;

            //We do this to check if the mod that's trying to be packaged isn't the default metadata assigned.
            if (!File.Exists(metadata.Location))
            {
                await Service.WindowManager.ShowDialog(MainWindow, new MessageBoxViewModel(Translatable.Get("modlist.packagemod.no-metadata")));
                return;
            }

            if (mod.ModFilesGUI.Count == 0)
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

            foreach (ModFile modFile in mod.BaseModel.ModFiles)
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
            metadata.Save(false);
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
            if (MainWindow.SelectedItem is not ModGUI mod)
                return;

            ModWindowViewModel viewModel = new ModWindowViewModel(mod, MainWindow.LoadedInstance.BaseModel);
            await Service.WindowManager.ShowDialog(MainWindow, viewModel);

            if (!viewModel.SubmitClose)
                return;

            mod.Name = viewModel.Name;
            mod.Author = viewModel.Author;
            mod.Description = viewModel.Description;
        }

        public async void NewGroup()
        {
            var window = new GroupsWindowViewModel(MainWindow);
            window.Create();
        }

        public async void ChangeGroup()
        {
            var window = new GroupsWindowViewModel(MainWindow);
            await Service.WindowManager.ShowDialog(MainWindow, window);

            if (!window.Confirmed)
                return;

            int count = 0;
            foreach (ObservableObject obj in MainWindow.SelectedItems.ToList())
            {
                if (obj is not ModGUI mod)
                    continue;

                if (mod.Enabled)
                    window.SelectedGroup.ForceState(true);

                GroupGUI? group = MainWindow.LoadedInstance.GetGroup(mod);
                group?.ModsGUI.Remove(mod);
                window.SelectedGroup.ModsGUI.Insert(count, mod);
                count++;
            }
        }

        public async void RenameGroup()
        {
            if (MainWindow.SelectedItem is not GroupGUI group)
                return;

            var textInput = new TextInputWindowViewModel();
            textInput.TextBox = group.Name;
            await Service.WindowManager.ShowDialog(MainWindow, textInput);

            if (!textInput.SubmitClose)
                return;

            group.Name = textInput.TextBox;
            group.BaseModel.IsDefault = false;
        }

        public async void DeleteMod()
        {
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

            foreach (ObservableObject item in MainWindow.SelectedItems.ToList())
            {
                if (item is not ModGUI mod)
                    continue;

                File.Delete(Path.Combine(baseDirectory, Constants.METADATA_FOLDER, $"{mod.BaseModel.Id}.mdata"));

                foreach (ModFileGUI file in mod.ModFilesGUI)
                {
                    if (!Path.HasExtension(file.Location))
                    {
                        FileUtil.CheckAndDeleteFile(Path.Combine(baseDirectory, $"{file.Location}.dat"));
                        FileUtil.CheckAndDeleteFile(Path.Combine(baseDirectory, $"{file.Location}.fat"));
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

                    if (Directory.GetFiles(modDirectory).Length == 0 && !FileUtil.CheckAndDeleteFolder(modDirectory))
                    {
                        await Service.WindowManager.ShowDialog(MainWindow, new MessageBoxViewModel(Translatable.Get("modlist.deletemod-folder-in-use")));
                        return;
                    }
                }

                GroupGUI? group = MainWindow.LoadedInstance.GetGroup(mod);
                group?.ModsGUI.Remove(mod);
            }

            MainWindow.SelectedItems.Clear();
            MainWindow.SelectedItem = null;
            MainWindow.Save();
        }

        public void DeleteGroup()
        {
            var window = new GroupsWindowViewModel(MainWindow);
            window.DeleteFromListBox();
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

        public void MoveItemUp()
        {
            if (MainWindow.SelectedItems == null || MainWindow.SelectedItems.Count == 0)
                return;

            SortedDictionary<int, ModGUI> mods = [];
            SortedDictionary<int, GroupGUI> groups = [];

            //only move mods that aren't at the top or bottom of the list.
            foreach (ObservableObject item in MainWindow.SelectedItems)
            {
                if (item is GroupGUI group)
                {
                    int modIndex = MainWindow.LoadedInstance.GroupsGUI.IndexOf(group);
                    if (modIndex == 0)
                        continue;

                    groups.Add(modIndex, group);
                }

                if (item is ModGUI mod)
                {
                    int modIndex = MainWindow.LoadedInstance.GetGroup(mod).ModsGUI.IndexOf(mod);
                    if (modIndex == 0)
                        continue;

                    mods.Add(modIndex, mod);
                }
            }

            //ensure everything moves from top to bottom.
            foreach (int modIndex in mods.Keys)
            {
                MainWindow.LoadedInstance.GetGroup(mods[modIndex])?.ModsGUI.Move(modIndex, modIndex - 1);
            }

            foreach (int groupIndex in groups.Keys)
            {
                MainWindow.LoadedInstance.GroupsGUI.Move(groupIndex, groupIndex - 1);
            }
        }

        public void MoveItemDown()
        {
            if (MainWindow.SelectedItems == null || MainWindow.SelectedItems.Count == 0)
                return;

            SortedDictionary<int, ModGUI> mods = [];
            SortedDictionary<int, GroupGUI> groups = [];

            //only move mods that aren't at the top or bottom of the list.
            foreach (ObservableObject item in MainWindow.SelectedItems)
            {
                if (item is GroupGUI group)
                {
                    int modIndex = MainWindow.LoadedInstance.GroupsGUI.IndexOf(group);
                    if (modIndex == MainWindow.LoadedInstance.GroupsGUI.Count - 1)
                        continue;

                    groups.Add(modIndex, group);
                }

                if (item is ModGUI mod)
                {
                    int modIndex = MainWindow.LoadedInstance.GetGroup(mod).ModsGUI.IndexOf(mod);
                    if (modIndex == MainWindow.LoadedInstance.GetGroup(mod).ModsGUI.Count - 1)
                        continue;

                    mods.Add(modIndex, mod);
                }
            }

            //ensure everything moves from bottom to top.
            foreach (int modIndex in mods.Keys.Reverse())
            {
                MainWindow.LoadedInstance.GetGroup(mods[modIndex])?.ModsGUI.Move(modIndex, modIndex + 1);
            }

            foreach (int groupIndex in groups.Keys.Reverse())
            {
                MainWindow.LoadedInstance.GroupsGUI.Move(groupIndex, groupIndex + 1);
            }
        }
    }
}
