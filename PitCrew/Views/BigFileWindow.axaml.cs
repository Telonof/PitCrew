using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PitCrew.GUI;
using PitCrewCommon;

namespace PitCrew.Views;

public partial class BigFileWindow : Window
{
    private readonly bool pack;

    public BigFileWindow(bool pack = false)
    {
        InitializeComponent();
        this.pack = pack;

        if (pack)
        {
            Height = 280;
            PackOptions.IsVisible = true;
            SubmitButton.Content = Translate.Get("bigfile.pack");
            Title = Translate.Get("bigfile.packtitle");
            FileLabel.Text = Translate.Get("bigfile.outputfile");
            FolderLabel.Text = Translate.Get("bigfile.inputfolder");
        }
    }

    //Get fat file to unpack/pack from
    private async void FatFileSelector_Click(object sender, RoutedEventArgs e)
    {
        string filePath = "";
        if (!pack)
        {
            var file = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = Translate.Get("bigfile.selectfile.title"),
                AllowMultiple = false,
                FileTypeFilter = [Utils.CustomFileOptions(Translate.Get("bigfile.selectfile.filter"), ["*.fat"])]
            });
            
            if (file.Count > 0)
                filePath = file[0].Path.LocalPath;
        }
        else
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = Translate.Get("bigfile.set-output-file"),
                DefaultExtension = "fat"
            });
            
            if (file != null)
                filePath = file.Path.LocalPath;
        }

        if (string.IsNullOrWhiteSpace(filePath))
            return;

        FatFileTextBox.Text = filePath;
    }

    //Get folder to unpack into/pack from
    private async void FolderSelector_Click(object sender, RoutedEventArgs e)
    {
        var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Translate.Get("bigfile.select-folder"),
            AllowMultiple = false,
        });

        if (folder.Count == 0)
            return;

        FolderTextBox.Text = folder[0].Path.LocalPath;
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FolderTextBox.Text) || string.IsNullOrWhiteSpace(FatFileTextBox.Text))
            return;

        if (pack)
        {
            BigFileUtil.RepackBigFile(FolderTextBox.Text, FatFileTextBox.Text, AuthorTextBox.Text, LZOBox.IsChecked.Value);
            Utils.ShowDialog(this, string.Format(Translate.Get("bigfile.pack-success"), FatFileTextBox.Text));
            return;
        }

        BigFileUtil.UnpackBigFile(FatFileTextBox.Text, FolderTextBox.Text);
        Utils.ShowDialog(this, string.Format(Translate.Get("bigfile.unpack-success"), FolderTextBox.Text));
    }
}
