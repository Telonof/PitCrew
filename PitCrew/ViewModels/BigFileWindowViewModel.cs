using CommunityToolkit.Mvvm.ComponentModel;
using PitCrew.Models;
using PitCrew.Systems;
using PitCrewCommon;
using PitCrewCommon.Utilities;

namespace PitCrew.ViewModels
{
    internal partial class BigFileWindowViewModel : ViewModelBase
    {
        public UI UI { get; } = new UI();

        [ObservableProperty]
        public string fileText, folderText, authorText;

        [ObservableProperty]
        public bool compression = true;

        private readonly bool Pack;

        private readonly int PackageVersion;

        private readonly InstanceGUI Instance;

        public BigFileWindowViewModel(InstanceGUI instance, int packageVersion, bool pack = false)
        {
            Pack = pack;
            PackageVersion = packageVersion;
            Instance = instance;

            if (pack)
                UI.PackSettings();
        }

        public async void FileSelect()
        {
            //Unpack
            if (!Pack)
            {
                var files = await Service.WindowManager.OpenFileDialogAsync(this,
                                                                            Translatable.Get("bigfile.selectfile.title"),
                                                                            Translatable.Get("bigfile.selectfile.filter"),
                                                                            ["*.fat"]);

                if (files.Count < 1)
                    return;
                FileText = files[0].Path.LocalPath;
                return;
            }

            //Pack data
            var file = await Service.WindowManager.SaveFileDialogAsync(this, Translatable.Get("bigfile.set-output-file"), "fat");

            if (file == null)
                return;

            FileText = file.Path.LocalPath;
        }

        public async void FolderSelect()
        {
            var folders = await Service.WindowManager.OpenFileDialogAsync(this, Translatable.Get("bigfile.select-folder"), "", [], true);
            if (folders.Count == 0)
                return;

            FolderText = folders[0].Path.LocalPath;
        }

        public async void Submit()
        {
            if (string.IsNullOrWhiteSpace(FileText) || string.IsNullOrWhiteSpace(FolderText))
                return;

            if (Pack)
            {
                BigFileUtil.RepackBigFile(FolderText, FileText, AuthorText, Compression);
                await Service.WindowManager.ShowDialog(this, new MessageBoxViewModel(string.Format(Translatable.Get("bigfile.pack-success"), FileText)));
                return;
            }

            string baseGameDirectory = "";
            if (Instance != null)
                baseGameDirectory = FileUtil.GetParentDir(Instance.BaseModel.GetDirectory());

            BigFileUtil.UnpackBigFile(FileText, FolderText, PackageVersion, baseGameDirectory);
            await Service.WindowManager.ShowDialog(this, new MessageBoxViewModel(string.Format(Translatable.Get("bigfile.unpack-success"), FolderText)));
        }
    }

    internal partial class UI : ObservableObject
    {
        [ObservableProperty]
        public int windowHeight = 200;

        [ObservableProperty]
        public string windowTitle = Translatable.Get("bigfile.unpacktitle");

        [ObservableProperty]
        public string fileLabelText = Translatable.Get("bigfile.inputfile");

        [ObservableProperty]
        public string folderLabelText = Translatable.Get("bigfile.outputfolder");

        [ObservableProperty]
        public string submitButtonText = Translatable.Get("bigfile.unpack");

        [ObservableProperty]
        public bool packOptionVisible = false;

        internal void PackSettings()
        {
            WindowHeight = 280;
            WindowTitle = Translatable.Get("bigfile.packtitle");
            FileLabelText = Translatable.Get("bigfile.outputfile");
            FolderLabelText = Translatable.Get("bigfile.inputfolder");
            SubmitButtonText = Translatable.Get("bigfile.pack");
            PackOptionVisible = true;
        }
    }
}
