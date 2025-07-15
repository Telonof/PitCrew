using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using PitCrew.ViewModels;
using PitCrewCommon;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PitCrew.Systems
{
    public class WindowManager : IWindowService
    {
        private readonly ViewLocator ViewLocator;

        private IEnumerable<Window> Windows => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows;

        public WindowManager(ViewLocator viewLocator)
        {
            this.ViewLocator = viewLocator;
        }

        public async Task ShowDialog(ViewModelBase viewModel, ViewModelBase dialogedModel)
        {
            Window? parentWindow = GetParentWindow(viewModel);

            Control? view = ViewLocator.Build(dialogedModel);

            if (view is not Window window)
                return;

            window.DataContext = dialogedModel;

            await window.ShowDialog(parentWindow);
        }

        public async Task ShowDialogMainWindow(ViewModelBase dialogedModel)
        {
            Window? parentWindow = Windows.FirstOrDefault(window => window.DataContext.GetType() == typeof(MainWindowViewModel));

            if (parentWindow is null)
            {
                Logger.Error(301, Translatable.Get("windowmanager.no-main-window"));
                return;
            }

            Control? view = ViewLocator.Build(dialogedModel);

            if (view is not Window window)
                return;

            window.DataContext = dialogedModel;

            await window.ShowDialog(parentWindow);
        }

        public void Show(ViewModelBase newModel)
        {
            Control? view = ViewLocator.Build(newModel);

            if (view is not Window window)
                return;

            window.DataContext = newModel;

            window.Show();
        }

        public void CloseWindow(ViewModelBase viewModel)
        {
            Window? window = GetParentWindow(viewModel);
            if (window == null)
                return;

            window.Close();
        }

        public async Task<IReadOnlyList<IStorageItem>> OpenFileDialogAsync(ViewModelBase parent,
                                                                           string dialogTitle,
                                                                           string fileType,
                                                                           string[] patterns,
                                                                           bool folderSelect = false, 
                                                                           bool allowMultiple = false)
        {
            Window? parentWindow = GetParentWindow(parent);

            if (folderSelect)
            {
                return await parentWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = dialogTitle,
                    AllowMultiple = allowMultiple
                });
            }

            FilePickerFileType type = new FilePickerFileType(fileType) { Patterns = patterns };

            return await parentWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = dialogTitle,
                FileTypeFilter = [type],
                AllowMultiple = allowMultiple
            });

        }

        public async Task<IStorageItem>? SaveFileDialogAsync(ViewModelBase parent,
                                                             string dialogTitle,
                                                             string defaultExtension)
        {
            Window? parentWindow = GetParentWindow(parent);

            if (parentWindow == null)
                return null;

            return await parentWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = dialogTitle,
                DefaultExtension = defaultExtension
            });
        }

        private Window? GetParentWindow(ViewModelBase viewModel)
        {
            return Windows.FirstOrDefault(window => window.DataContext == viewModel);
        }
    }
	
    public interface IWindowService
    {
        Task<IStorageItem>? SaveFileDialogAsync(ViewModelBase parent,
                                                string dialogTitle,
                                                string defaultExtension);

        Task<IReadOnlyList<IStorageItem>> OpenFileDialogAsync(ViewModelBase parent,
                                                              string dialogTitle,
                                                              string fileType,
                                                              string[] patterns,
                                                              bool folderSelect = false,
                                                              bool allowMultiple = false);

        Task ShowDialog(ViewModelBase viewModel, ViewModelBase dialogedModel);

        /// <summary>
        /// Used for methods that don't have access to the required view model.
        /// </summary>
        Task ShowDialogMainWindow(ViewModelBase dialogedModel);

        void Show(ViewModelBase newModel);

        void CloseWindow(ViewModelBase viewModel);
    }
}
