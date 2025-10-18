using CommunityToolkit.Mvvm.ComponentModel;
using PitCrewCommon;
using System.Diagnostics;

namespace PitCrew.ViewModels
{
    public partial class AboutWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        public string translationText = "";

        public AboutWindowViewModel()
        {
            TranslationText = Translatable.Get("about.translator") + Translatable.Get("translation-author");
        }
    }
}
