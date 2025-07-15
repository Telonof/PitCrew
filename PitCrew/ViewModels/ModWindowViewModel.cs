using CommunityToolkit.Mvvm.ComponentModel;
using PitCrew.Models;
using PitCrew.Systems;
using PitCrewCommon;
using PitCrewCommon.Models;
using PitCrewCommon.Utilities;
using System.IO;
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
        public string id, name, author, description = Translatable.Get("modedit.desc-text");

        public bool SubmitClose { get; set; } = false;

        public ModWindowViewModel(ModGUI? modGUI, string baseDirectory)
        {
            if (modGUI == null)
                return;

            ButtonText = Translatable.Get("modedit.edit");

            IdTextBoxEnabled = false;

            Id = modGUI.BaseModel.Id;
            Name = modGUI.Name;
            Author = modGUI.Author;
            Description = modGUI.Description;
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

            return true;
        }
    }
}
