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

        public void DiscordLinkClick()
        {
            ProcessStartInfo discordLink = new ProcessStartInfo("https://discord.com/invite/gUczTkphGE")
            {
                UseShellExecute = true
            };
            Process.Start(discordLink);
        }
    }
}
