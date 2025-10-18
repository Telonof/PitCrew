using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using PitCrew.Models;
using PitCrew.Systems;
using PitCrewCommon;
using PitCrewCommon.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace PitCrew.ViewModels
{
    internal partial class InstanceWindowViewModel : ViewModelBase
    {
        private MainWindowViewModel MainWindow;

        [ObservableProperty]
        public ObservableCollection<InstanceGUI> instances;

        [ObservableProperty]
        public InstanceGUI? highlightedInstance;

        //This is designed for when the user actually double clicks an instance. If they didn't, then code can access this to check.
        public bool LoadingInstance { get; set; } = false;

        public InstanceWindowViewModel(MainWindowViewModel viewModel)
        {
            Instances = Service.Config.LoadInstances();
            MainWindow = viewModel;
        }

        public async void AddInstanceButton()
        {
            var files = await Service.WindowManager.OpenFileDialogAsync(this,
                                                                        Translatable.Get("instances.selectexe.title"),
                                                                        Translatable.Get("instances.selectexe.filter"),
                                                                        ["*.exe"]);

            if (files.Count == 0)
                return;

            AddInstance(files);
        }

        public async void AddInstance(IEnumerable<IStorageItem> items)
        {
            IStorageItem item = items.FirstOrDefault();

            if (item == null)
                return;

            string path = Path.GetDirectoryName(item.Path.LocalPath);
            string combinedpath = Path.Combine(path, "data_win32");

            if (!Directory.Exists(combinedpath))
            {
                await Service.WindowManager.ShowDialog(this, new MessageBoxViewModel(Translatable.Get("instances.invalid-exe")));
                return;
            }

            //Find any manifest in the folder if it exists already and pick the first one. There should only be 1 manifest per game copy.
            string[] potentialManifests = Directory.GetFiles(combinedpath, "*.xml");
            string manifestPath = potentialManifests.Length > 0 ? potentialManifests[0] : "";

            //Test if instance already exists with this manifest.
            if (!string.IsNullOrWhiteSpace(manifestPath))
            {
                bool exists = Instances.ToList().Any(inst => inst.Location == manifestPath);
                if (exists)
                {
                    await Service.WindowManager.ShowDialog(this, new MessageBoxViewModel(Translatable.Get("instances.instance-already-found")));
                    return;
                }
            }
            else
            {
                manifestPath = Path.Combine(combinedpath, "pitcrewmanifest.xml");
                File.Create(manifestPath).Dispose();
            }

            var textInput = new TextInputWindowViewModel();
            await Service.WindowManager.ShowDialog(this, textInput);
            if (!textInput.SubmitClose || string.IsNullOrWhiteSpace(textInput.TextBox))
                return;

            InstanceGUI instance = new InstanceGUI(new Instance(manifestPath), textInput.TextBox);

            Instances.Add(instance);
            Service.Config.SaveInstance(instance);
        }

        public async void RenameInstance()
        {
            if (HighlightedInstance == null)
                return;

            var textInput = new TextInputWindowViewModel();
            await Service.WindowManager.ShowDialog(this, textInput);

            if (!textInput.SubmitClose)
                return;

            Service.Config.SetInstanceName(HighlightedInstance.Name, textInput.TextBox);
            HighlightedInstance.Name = textInput.TextBox;
        }

        public void LoadInstance()
        {
            LoadingInstance = true;
            Service.Config.SetSetting(ConfigKey.LastOpenedPath, HighlightedInstance.Location);
            Service.WindowManager.CloseWindow(this);
        }

        public async void DeleteInstance()
        {
            if (HighlightedInstance == null)
                return;

            var confirm = new MessageBoxViewModel(Translatable.Get("instances.delete-confirmation"), MessageBoxViewModel.ButtonType.YesNo);
            await Service.WindowManager.ShowDialog(this, confirm);

            if (confirm.Result != MessageBoxViewModel.ResultType.OK)
                return;

            if (MainWindow.LoadedInstance != null && HighlightedInstance.Location.Equals(MainWindow.LoadedInstance.Location))
            {
                MainWindow.LoadedInstance = null;
                MainWindow.LoadedMod = null;
                Service.Config.SetSetting(ConfigKey.LastOpenedPath, "");
                MainWindow.UI.ModsTabVisible = false;
                MainWindow.UI.ModListBorderColor = "Gray";
            }

            Service.Config.DeleteInstance(HighlightedInstance);
            Instances.Remove(HighlightedInstance);
        }
    }
}
