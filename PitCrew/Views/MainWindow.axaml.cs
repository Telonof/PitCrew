using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using PitCrew.GUI;
using PitCrew.Models;
using PitCrewCommon;
using PitCrewCompiler;

namespace PitCrew.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void Initialize()
    {
        //Setup mod list
        ModGUIList.ItemsSource = IM.modListBox.Keys;
        ModGUIList.SelectionChanged += IM.modListBox.ListBox_Select;
        ModGUIList.AddHandler(DragDrop.DropEvent, IM.modListBox.ListBox_Drop);
        var customListBoxView = new FuncDataTemplate<ModInfo>((modInfo, build) =>
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

            var authorTextBlock = new TextBlock
            {
                [!TextBlock.TextProperty] = new Avalonia.Data.Binding("Author"),
                FontSize = 14,
                FontWeight = Avalonia.Media.FontWeight.Bold
            };

            var descriptionTextBlock = new TextBlock
            {
                [!TextBlock.TextProperty] = new Avalonia.Data.Binding("Description"),
                FontSize = 14,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };

            var checkBox = new CheckBox
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 0, -10),
                [!CheckBox.IsCheckedProperty] = new Avalonia.Data.Binding("Enabled")
            };

            stackPanel.Children.Add(nameTextBlock);
            stackPanel.Children.Add(authorTextBlock);
            stackPanel.Children.Add(descriptionTextBlock);
            stackPanel.Children.Add(checkBox);

            return stackPanel;
        });

        ModGUIList.ItemTemplate = customListBoxView;

        //Setup right click options
        RightClickTree rightClickTree = new RightClickTree(this);
        ModGUIList.ContextMenu.Opening += rightClickTree.ListBox_ContextMenuOpening;
        ListBox_EditMetadataItem.Click += rightClickTree.ListBox_EditMetadataItem_Click;
        ListBox_DeleteModItem.Click += rightClickTree.ListBox_DeleteModItem_Click;
        ListBox_PackageModItem.Click += rightClickTree.ListBox_PackageModItem_Click;

        //Setup file list
        var modPath = new DataGridTextColumn
        {
            Header = Translate.Get("filelist.filepath"),
            Binding = new Avalonia.Data.Binding("ModPath"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
        };

        var priority = new DataGridTextColumn
        {
            Header = Translate.Get("filelist.priority"),
            Binding = new Avalonia.Data.Binding("Priority"),
            Width = new DataGridLength(120, DataGridLengthUnitType.Pixel)
        };
        FileList.Columns.Add(modPath);
        FileList.Columns.Add(priority);
        FileList.CellEditEnding += IM.fileGridList.OnCellEdit;
        FileList.KeyDown += IM.fileGridList.OnCellDelete;

        //Setup languages
        List<MenuItem> menuButtons = [];
        string[] files = Directory.GetFiles("Languages", "*.json");
        foreach (string file in files)
        {
            MenuItem item = new MenuItem() { Header=Path.GetFileNameWithoutExtension(file) };
            item.Click += LanguageMenuButton_Click;
            menuButtons.Add(item);
        }
        LanguageMenuButton.ItemsSource = menuButtons;
    }

    //Menu Bar
    private async void InstancesMenuButton_Click(object sender, RoutedEventArgs e)
    {
        await new InstanceWindow(this).ShowDialog(this);
    }

    private async void NewModMenuButton_Click(object sender, RoutedEventArgs e)
    {
        await new ModWindow().ShowDialog(this);
    }

    private void ImportModMenuButton_Click(object sender, RoutedEventArgs e)
    {
        Utils.ImportMod(this);
    }

    private void UnpackArchiveMenuButton_Click(object sender, RoutedEventArgs e)
    {
        new BigFileWindow().Show();
    }

    private void RepackArchiveMenuButton_Click(object sender, RoutedEventArgs e)
    {
        new BigFileWindow(true).Show();
    }

    private void ThemeSwitcherButton_Click(object sender, RoutedEventArgs e)
    {
        IM.config.SwitchTheme();
    }

    private async void AboutMenuButton_Click(object sender, RoutedEventArgs e)
    {
        await new AboutWindow().ShowDialog(this);
    }

    private void LanguageMenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem)
            return;
        
        Translate.Load(menuItem.Header.ToString() + ".json");
        Utils.ShowDialog(this, Translate.Get("language-restart"));
        IM.config.SetSetting("Lang", menuItem.Header.ToString());
    }

    //Buttons
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        Utils.SaveFile();
    }

    private void CompileButton_Click(object sender, RoutedEventArgs e)
    {
        Utils.SaveFile();

        if (IM.currentInstance == null)
            return;

        API.compileManifest(IM.currentInstance.ManifestPath);
        Utils.ShowDialog(this, Translate.Get("compiler.success"));
    }
}