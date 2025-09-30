using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using PitCrew.Models;
using PitCrew.Systems;
using PitCrewCommon;
using PitCrewCommon.Models;
using PitCrewCommon.Utilities;
using PitCrewCompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace PitCrew.ViewModels
{
    internal partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        public List<Language> languages = [];

        [ObservableProperty]
        public InstanceGUI loadedInstance;

        [ObservableProperty]
        public ModGUI loadedMod;

        public UI UI { get; } = new UI();

        public ListBoxViewModel ListBox { get; }

        public MainWindowViewModel()
        {
            SwitchTheme(Service.Config.GetSetting(ConfigKey.Theme));
            ListBox = new ListBoxViewModel(this);

            //Generate Languages
            foreach (string file in Directory.GetFiles("Languages", "*"))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                languages.Add(new Language(name, () => SwitchLang(name)));
            }

            //Check for last opened path
            string path = Service.Config.GetSetting(ConfigKey.LastOpenedPath);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;

            LoadedInstance = new InstanceGUI(new Instance(path), "");
            LoadInstance();
        }

        public async void AboutWindow()
        {
            await Service.WindowManager.ShowDialog(this, new AboutWindowViewModel());
        }

        public async void InstanceWindow()
        {
            var model = new InstanceWindowViewModel(this);
            await Service.WindowManager.ShowDialog(this, model);
            if (!model.LoadingInstance)
                return;

            LoadedInstance = model.HighlightedInstance;
            LoadInstance();
        }

        private void LoadInstance()
        {
            LoadedInstance.LoadFromXML();

            if (LoadedInstance.ModsGUI.Count == 0)
                UI.ConflictBoxText = Translatable.Get("conflictbox.no-mods");
            else
                UI.ConflictBoxText = Translatable.Get("conflictbox.no-conflicts");

            UI.ModListBorderColor = "LightBlue";
            UI.ModsTabVisible = true;
        }

        public async void NewMod()
        {
            ModWindowViewModel viewModel = new ModWindowViewModel(null, LoadedInstance.BaseModel.GetDirectory());
            await Service.WindowManager.ShowDialog(this, viewModel);

            if (!viewModel.SubmitClose)
                return;

            //Generate folder first
            string metadataFolder = Path.Combine(LoadedInstance.BaseModel.GetDirectory(), Constants.METADATA_FOLDER);
            FileUtil.CheckAndCreateFolder(metadataFolder);

            //Generate mod and apply ID to make the metadata file.
            Mod mod = new Mod(LoadedInstance.BaseModel);
            mod.Id = viewModel.Id;

            Metadata metadata = new Metadata(Path.Combine(metadataFolder, $"{mod.Id}.mdata"));

            string currentLanguage = Service.Config.GetSetting(ConfigKey.Language);

            //Apply data to metadata
            metadata.LocalizedNames[currentLanguage] = viewModel.Name;
            metadata.LocalizedDescriptions[currentLanguage] = viewModel.Description;
            metadata.Author = viewModel.Author;
            mod.Metadata = metadata;

            ModGUI modGui = new ModGUI(mod);
            modGui.BaseModel.Metadata.Save();

            LoadedInstance.ModsGUI.Add(modGui);
        }

        public async void ImportModButton()
        {
            var files = await Service.WindowManager.OpenFileDialogAsync(this,
                                                                       Translatable.Get("importmod.filechooser.title"),
                                                                       Translatable.Get("importmod.filechooser.filefilter"),
                                                                       ["*.mdata", "*.zip"],
                                                                       false,
                                                                       true);

            if (files.Count == 0)
                return;

            foreach (var file in files)
            {
                string path = file.Path.LocalPath;
                if (!Path.GetExtension(path).Equals(".mdata") && !Path.GetExtension(path).Equals(".zip"))
                {
                    await Service.WindowManager.ShowDialog(this, new MessageBoxViewModel(Translatable.Get("importmod.invalid-extension")));
                    continue;
                }
                ImportMod(path);
            }
        }

        public async void ImportMod(string path)
        {
            if (LoadedInstance == null)
            {
                await Service.WindowManager.ShowDialog(this, new MessageBoxViewModel(Translatable.Get("server.no-instance")));
                return;
            }

            string zipTempFolder = "!PitCrewZipTempFolder";

            //If a zip was given, extract it to a temporary folder first then run as if it was mdata.
            if (Path.GetExtension(path).Equals(".zip"))
            {
                ZipFile.ExtractToDirectory(path, zipTempFolder, true);

                string[] files = Directory.GetFiles(zipTempFolder, "*.mdata");
                if (files.Length == 0)
                {
                    await Service.WindowManager.ShowDialog(this, new MessageBoxViewModel(Translatable.Get("importmod.invalid-zip")));
                    FileUtil.CheckAndDeleteFolder(zipTempFolder);
                    return;
                }

                path = files[0];
            }

            Mod mod = new Mod(LoadedInstance.BaseModel);
            mod.LoadFromMData(path, true);

            string Id = Path.GetFileNameWithoutExtension(path);
            ModGUI? existingMod = LoadedInstance.ModsGUI.FirstOrDefault(modGUI => modGUI.BaseModel.Id == Id);
            ModGUI modGUI = new ModGUI(mod);
            if (existingMod != null)
            {
                var result = new MessageBoxViewModel(Translatable.Get("importmod.overwrite-old-mod"), MessageBoxViewModel.ButtonType.YesNo);
                await Service.WindowManager.ShowDialog(this, result);

                if (result.Result != MessageBoxViewModel.ResultType.OK)
                {
                    LoadedInstance.BaseModel.Mods.Remove(mod);
                    return;
                }

                existingMod.ModFilesGUI = modGUI.ModFilesGUI;
                existingMod.Name = modGUI.Name;
                existingMod.Author = modGUI.Author;
                existingMod.Description = modGUI.Description;
            }
            else
            {
                LoadedInstance.ModsGUI.Insert(0, modGUI);
            }
            
            if (LoadedMod == null)
                UI.ConflictBoxText = Translatable.Get("conflictbox.no-conflicts");

            FileUtil.CheckAndDeleteFolder(zipTempFolder);
            Save();
        }

        public void Save()
        {
            if (LoadedInstance == null)
                return;

            LoadedInstance.SaveToXML();
        }

        public async void Compile()
        {
            if (LoadedInstance == null)
                return;

            UI.WindowClickable = false;
            Save();

            LogWindowViewModel logWindow = new LogWindowViewModel();
            Service.WindowManager.Show(logWindow);

            Compile compile = new Compile(LoadedInstance.BaseModel);
            await Task.Run(() => compile.Run());

            Service.WindowManager.CloseWindow(logWindow);
            UI.WindowClickable = true;

            await Service.WindowManager.ShowDialog(this, new MessageBoxViewModel(Translatable.Get("compiler.success")));
        }

        public void BigFileWindow(string pack)
        {
            int version = 5;
            if (LoadedInstance != null)
                version = LoadedInstance.BaseModel.PackageVersion;

            Service.WindowManager.Show(new BigFileWindowViewModel(version, pack.Equals("pack")));
        }

        partial void OnLoadedModChanged(ModGUI value)
        {
            List<string> conflictingMods = [];
            string conflictBoxMessage = Translatable.Get("conflictbox.conflicting");

            //This is here for when someone switches instances or deletes a mod as we dont care to edit the conflict box.
            if (LoadedInstance.ModsGUI == null || value == null)
                return;

            foreach (ModGUI modGUI in LoadedInstance.ModsGUI)
            {
                if (value == modGUI)
                    continue;

                if (!value.Hashes.Any(hash => modGUI.Hashes.Contains(hash)))
                    continue;

                conflictingMods.Add(modGUI.Name);
            }

            if (conflictingMods.Count == 0)
            {
                conflictBoxMessage = Translatable.Get("conflictbox.no-conflicts");
            }
            else
            {
                foreach (string mod in conflictingMods)
                {
                    conflictBoxMessage += Environment.NewLine + mod;
                }
            }

            if (value.ModFilesGUI.Any(file => file.Priority == 10))
            {
                conflictBoxMessage = Translatable.Get("conflictbox.startupfile") + Environment.NewLine + conflictBoxMessage;
            }

            UI.ConflictBoxText = conflictBoxMessage;
        }

        public void LogWindow()
        {
            Service.WindowManager.Show(new LogWindowViewModel(false));
        }

        public async void SwitchLang(string name)
        {
            Translatable.Load(Path.ChangeExtension(name, ".json"));
            Service.Config.SetSetting(ConfigKey.Language, name);
            await Service.WindowManager.ShowDialog(this, new MessageBoxViewModel(Translatable.Get("language-restart")));
        }

        public async void SwitchTheme(string theme)
        {
            Application app = Application.Current;
            Uri uriLoc = new Uri($"avares://PitCrew/Styles/{theme}.axaml");

            foreach (var style in app.Styles.ToList())
            {
                if (style is StyleInclude styInclude && styInclude.Source != uriLoc)
                    app.Styles.Remove(style);
            }

            ThemeVariant variant = ThemeVariant.Dark;

            if (theme.Equals("Light"))
                variant = ThemeVariant.Light;

            app.RequestedThemeVariant = variant;

            StyleInclude newStyle = new StyleInclude(uriLoc) { Source = uriLoc };
            app.Styles.Add(newStyle);

            Service.Config.SetSetting(ConfigKey.Theme, theme);
        }
    }

    internal partial class UI : ObservableObject
    {
        [ObservableProperty]
        public bool modsTabVisible = false;

        [ObservableProperty]
        public string modListBorderColor = "Gray";

        [ObservableProperty]
        public string conflictBoxText = Translatable.Get("conflictbox.default");

        [ObservableProperty]
        public bool windowClickable = true;

        public string Version => Constants.VERSION;
    }
}
