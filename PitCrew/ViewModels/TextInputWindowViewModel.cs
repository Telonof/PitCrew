using CommunityToolkit.Mvvm.ComponentModel;
using PitCrew.Systems;

namespace PitCrew.ViewModels
{
    internal partial class TextInputWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        public string textBox;

        public bool SubmitClose { get; set; } = false;

        public void Submit()
        {
            SubmitClose = true;
            Service.WindowManager.CloseWindow(this);
        }
    }
}
