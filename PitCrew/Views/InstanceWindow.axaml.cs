using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PitCrewCommon;
using PitCrew.GUI;
using PitCrew.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System;

namespace PitCrew.Views;

internal partial class InstanceWindow : Window
{
    private ObservableCollection<Instance> liveInstances { get; } = [];

    private MainWindow owner;

    public InstanceWindow(MainWindow owner)
    {
        InitializeComponent();

        this.owner = owner;

        InstanceList.ItemsSource = liveInstances;
        InstanceList.DoubleTapped += InstanceBox_DoubleTapped;
        InstanceList.AddHandler(DragDrop.DropEvent, InstanceBox_Drop);

        RefreshList();

        var customListBoxView = new FuncDataTemplate<Instance>((instance, build) =>
        {
            var stackPanel = new StackPanel
            {
                Margin = new Thickness(5),
                Spacing = 5
            };

            var nameTextBlock = new TextBlock
            {
                [!TextBlock.TextProperty] = new Avalonia.Data.Binding("Name"),
                FontSize = 20,
                FontWeight = Avalonia.Media.FontWeight.Bold
            };

            var pathTextBlock = new TextBlock
            {
                [!TextBlock.TextProperty] = new Avalonia.Data.Binding("ManifestPath"),
                FontSize = 14,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };

            stackPanel.Children.Add(nameTextBlock);
            stackPanel.Children.Add(pathTextBlock);

            return stackPanel;
        });

        InstanceList.ItemTemplate = customListBoxView;
    }

    private void RefreshList()
    {
        liveInstances.Clear();
        foreach (Instance instance in IM.instances)
        {
            liveInstances.Add(instance);
        }
    }

    private void ContextMenu_Opening(object sender, CancelEventArgs e)
    {
        if (InstanceList.SelectedItem == null)
            e.Cancel = true;
    }

    private async void ContextMenuRenameItem_Click(object sender, RoutedEventArgs e)
    {
        Instance instance = (Instance) InstanceList.SelectedItem;
        TextInputWindow window = new TextInputWindow();
        await window.ShowDialog(this);
        if (string.IsNullOrWhiteSpace(window.GetText()))
            return;

        IM.config.SetInstanceName(instance.Name, window.GetText());
        instance.Name = window.GetText();
        RefreshList();
    }

    private async void ContextMenuDeleteItem_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Result result = await Utils.ShowDialog(this, Translate.Get("instances.delete-confirmation"), MessageBox.ButtonType.OkCancel);
        if (result != MessageBox.Result.OK)
            return;
        Instance selectedInstance = (Instance) InstanceList.SelectedItem;
        if (IM.currentInstance == selectedInstance)
        {
            Utils.SaveFile();
            IM.currentMod = null;
            owner.FileList.ItemsSource = null;
            owner.ModsMenu.IsVisible = false;
            IM.modListBox.Keys.Clear();
            IM.modList.Clear();
        }
        IM.instances.Remove(selectedInstance);
        IM.currentInstance = null;
        IM.config.DeleteInstance(selectedInstance);
        RefreshList();
    }

    private void InstanceBox_Drop(object sender, DragEventArgs e)
    {
        //Accept .exe
        foreach (IStorageItem item in e.Data.GetFiles())
        {
            if (!item.Name.EndsWith(".exe"))
                continue;

            AddInstance(Path.GetDirectoryName(item.Path.LocalPath));
        }
    }

    private void InstanceBox_DoubleTapped(object sender, TappedEventArgs e)
    {
        Instance selectedInstance = (Instance)InstanceList.SelectedItem;
        if (selectedInstance == null)
            return;

        if (!File.Exists(selectedInstance.ManifestPath))
        {
            Utils.ShowDialog(this, Translate.Get("instances.invalid-manifest"));
            return;
        }

        Utils.LoadManifest(selectedInstance, owner);
        IM.config.SetSetting("LastOpenedPath", selectedInstance.ManifestPath);
        Close();
    }

    private async void AddInstanceButton_Click(object sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Translate.Get("instances.selectexe.title"),
            AllowMultiple = false,
            FileTypeFilter = [Utils.CustomFileOptions(Translate.Get("instances.selectexe.filter"), ["*.exe"])]
        });

        if (files.Count == 0)
            return;

        AddInstance(Path.GetDirectoryName(files[0].Path.LocalPath));
    }

    private async void AddInstance(string path)
    {
        string combinedpath = Path.Combine(path, "data_win32");
        if (!Directory.Exists(combinedpath))
        {
            Utils.ShowDialog(this, Translate.Get("instances.invalid-exe"));
            return;
        }
        string manifestPath = "";

		//Find any valid manifest in the folder if it exists already.
        string[] potentialManifests = Directory.GetFiles(combinedpath, "*.txt");
        foreach (string file in potentialManifests)
        {
            if (ManifestUtil.ValidateManifestFile(file) == null)
            {
                manifestPath = file;
                break;
            }
        }
        //Test if instance already exists with this manifest.
        if (!string.IsNullOrWhiteSpace(manifestPath))
        {
            bool exists = IM.instances.Any(inst => inst.ManifestPath == manifestPath);
            if (exists)
            {
                Utils.ShowDialog(this, Translate.Get("instances.instance-already-found"));
                return;
            }
        } else
        {
            manifestPath = Path.Combine(combinedpath, "pitcrewmanifest.txt");
            using var _ = File.Create(manifestPath);
        }
        Instance instance = new Instance();
        instance.ManifestPath = manifestPath;
		
        //Prompt for custom name of instance
        TextInputWindow window = new TextInputWindow();
        await window.ShowDialog(this);
        if (string.IsNullOrWhiteSpace(window.GetText()))
            instance.Name = Translate.Get("modinfo.default-id");
        instance.Name = window.GetText();
        
        IM.instances.Add(instance);
        IM.config.SaveInstance(instance);
        RefreshList();
    }
}
