using System;
using CommunityToolkit.Mvvm.ComponentModel;
using PitCrew.Models;
using PitCrew.Systems;
using PitCrewCommon;
using PitCrewCommon.Models;
using System.Linq;

namespace PitCrew.ViewModels
{
    internal partial class ModWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        public bool idTextBoxEnabled = true;

        [ObservableProperty]
        public string buttonText = Translatable.Get("modedit.create");

        [ObservableProperty]
        public string id, name, author, majorRev, minorRev, buildRev, description = Translatable.Get("modedit.desc-text");

        public bool SubmitClose { get; set; } = false;

        private readonly Instance Instance;


        public ModWindowViewModel(ModGUI? modGUI, Instance instance)
        {
            Instance = instance;

            if (modGUI == null)
                return;

            ButtonText = Translatable.Get("modedit.edit");

            IdTextBoxEnabled = false;

            Id = modGUI.BaseModel.Id;
            Name = modGUI.Name;
            Author = modGUI.Author;
            Description = modGUI.Description;
            
            Version version = modGUI.Version;
            MajorRev = version.Major.ToString();
            MinorRev = version.Minor.ToString();
            BuildRev = version.Build.ToString();
        }

        public void Submit()
        {
            if (!ValidateInputs())
                return;

            SubmitClose = true;
            Service.WindowManager.CloseWindow(this);
        }

        public bool ValidateInputs()
        {
            string[] list = [Id, Name, Author, Description];

            if (list.Any(item => string.IsNullOrWhiteSpace(item)))
            {
                Service.WindowManager.ShowDialog(this, new MessageBoxViewModel(Translatable.Get("modedit.no-blank-fields")));
                return false;
            }

            if (Id.Any(char.IsWhiteSpace))
            {
                Service.WindowManager.ShowDialog(this, new MessageBoxViewModel(Translatable.Get("modedit.no-whitespace-allowed")));
                return false;
            }

            if (IdTextBoxEnabled && Instance.GetAllMods().Any(mod => mod.Id.Equals(Id)))
            {
                Service.WindowManager.ShowDialog(this, new MessageBoxViewModel(Translatable.Get("modedit.mod-exists")));
                return false;
            }

            if (!int.TryParse(MajorRev, out _) || !int.TryParse(MinorRev, out _) || !int.TryParse(BuildRev, out _))
            {
                Service.WindowManager.ShowDialog(this, new MessageBoxViewModel(Translatable.Get("modedit.numbers-only")));
                return false;
            }

            return true;
        }
    }
}
