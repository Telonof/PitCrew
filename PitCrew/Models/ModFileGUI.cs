using CommunityToolkit.Mvvm.ComponentModel;
using PitCrew.Systems;
using PitCrew.ViewModels;
using PitCrewCommon;
using PitCrewCommon.Models;
using PitCrewCommon.Utilities;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace PitCrew.Models
{
    public partial class ModFileGUI : ModelConverter<ModFile>
    {
        [ObservableProperty]
        public string location;
        [ObservableProperty]
        public int priority;

        private int PreviousPriority;

        public string PreviousLocation { get; set; }

        public bool ToDelete { get; set; } = false;

        public ModFileGUI(ModFile file) : base(file)
        {
            location = file.Location;
            priority = file.Priority;
            PreviousLocation = file.Location;
            PreviousPriority = file.Priority;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Location) && !string.IsNullOrWhiteSpace(PreviousLocation))
            {
                ToDelete = true;
                base.OnPropertyChanged(e);
                return;
            }

            if (string.IsNullOrWhiteSpace(Location) && Priority != 0)
            {
                Service.WindowManager.ShowDialogMainWindow(new MessageBoxViewModel(Translatable.Get("filelist.priority-no-edit-before-path")));
                Priority = PreviousPriority;
                return;
            }

            if (string.IsNullOrWhiteSpace(Location) && Priority == 0)
                return;

            if (Priority < 10 && !string.IsNullOrWhiteSpace(PreviousLocation))
            {
                Service.WindowManager.ShowDialogMainWindow(new MessageBoxViewModel(Translatable.Get("filelist.priority-less-than-ten")));
                Priority = PreviousPriority;
                return;
            }

            //Check if any other mod is using this location.
            Mod? mod = BaseModel.ParentMod?.ParentInstance?.Mods.FirstOrDefault(mod =>
                    mod != BaseModel.ParentMod &&
                    mod.ModFiles.Any(modFile => modFile.Location.Equals(Location)));

            if (mod != null)
            {
                Service.WindowManager.ShowDialogMainWindow(new MessageBoxViewModel(string.Format(Translatable.Get("filelist.already-in-use"), mod.Metadata.Name)));
                Location = PreviousLocation;
                return;
            }

            if (!ManifestUtil.IsValidFile(Path.Combine(BaseModel.ParentMod.ParentInstance.GetDirectory(), Location)))
            {
                Service.WindowManager.ShowDialogMainWindow(new MessageBoxViewModel(string.Format(Translatable.Get("filelist.unable-to-find-file"), Location)));
                Location = PreviousLocation;
                return;
            }

            if (Priority == 0)
                Priority = 998;

            PreviousLocation = Location;
            PreviousPriority = Priority;
            base.OnPropertyChanged(e);
        }
    }
}
