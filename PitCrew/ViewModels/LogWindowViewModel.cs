using CommunityToolkit.Mvvm.ComponentModel;
using PitCrewCommon;

namespace PitCrew.ViewModels
{
    internal partial class LogWindowViewModel : ViewModelBase
    {
        public string LogsText => Logger.GetLogsForGui();

        [ObservableProperty]
        public int maxPercentage, currentPercentage;

        [ObservableProperty]
        public bool progressVisible = true;

        public LogWindowViewModel(bool progressVisible = true)
        {
            if (progressVisible)
                LockClosure = true;

            Logger.Logs.CollectionChanged += (s, e) => OnPropertyChanged(nameof(LogsText));
            PercentageCalculator.CurrentChange += (value) => CurrentPercentage = value;
            PercentageCalculator.TotalChange += (value) => MaxPercentage = value;
            ProgressVisible = progressVisible;
        }
    }
}
