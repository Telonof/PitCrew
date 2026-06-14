using CommunityToolkit.Mvvm.ComponentModel;
using PitCrew.Models;
using PitCrew.Systems;
using PitCrewCommon;
using PitCrewCommon.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace PitCrew.ViewModels
{
    internal partial class GroupsWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        public GroupGUI selectedGroup;

        [ObservableProperty]
        public ObservableCollection<GroupGUI> groups;

        public bool Confirmed { get; set; } = false;

        private readonly MainWindowViewModel MainWindow;


        public GroupsWindowViewModel(MainWindowViewModel viewModel)
        {
            MainWindow = viewModel;
            Groups = MainWindow.LoadedInstance.GroupsGUI;
        }

        public async void Create()
        {
            var textInput = new TextInputWindowViewModel();
            await Service.WindowManager.ShowDialog(MainWindow, textInput);

            if (!textInput.SubmitClose)
                return;

            Group group = new Group();
            group.Name = textInput.TextBox;
            MainWindow.LoadedInstance.GroupsGUI.Insert(0, new GroupGUI(group));
        }

        public async void Delete()
        {
            var result = new MessageBoxViewModel(Translatable.Get("modlist.deletegroup-warning"), MessageBoxViewModel.ButtonType.OkCancel);
            await Service.WindowManager.ShowDialog(MainWindow, result);
            if (result.Result != MessageBoxViewModel.ResultType.OK)
                return;

            DeleteGroup([SelectedGroup]);
        }

        public async void DeleteFromListBox()
        {
            var result = new MessageBoxViewModel(Translatable.Get("modlist.deletegroup-warning"), MessageBoxViewModel.ButtonType.OkCancel);
            await Service.WindowManager.ShowDialog(MainWindow, result);
            if (result.Result != MessageBoxViewModel.ResultType.OK)
                return;

            DeleteGroup(MainWindow.SelectedItems.ToArray());
            MainWindow.SelectedItems.Clear();
            MainWindow.SelectedItem = null;
            MainWindow.Save();
        }

        public async void DeleteGroup(ObservableObject[] items)
        {
            foreach (ObservableObject item in items)
            {
                if (item is not GroupGUI group)
                    continue;

                if (group.BaseModel.IsDefault && group.ModsGUI.Count > 0)
                {
                    var result = new MessageBoxViewModel(Translatable.Get("modlist.deletegroup-error"));
                    await Service.WindowManager.ShowDialog(MainWindow, result);
                    continue;
                }

                foreach (ModGUI mod in group.ModsGUI)
                {
                    MainWindow.LoadedInstance.GetDefaultGroup()?.ModsGUI.Add(mod);
                }

                MainWindow.LoadedInstance.GroupsGUI.Remove(group);
            }
        }

        public void Confirm()
        {
            Confirmed = true;
            Service.WindowManager.CloseWindow(this);
        }
    }
}
