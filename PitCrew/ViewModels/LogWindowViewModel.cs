using CommunityToolkit.Mvvm.ComponentModel;
using PitCrewCommon;
using System;
using System.Collections.ObjectModel;

namespace PitCrew.ViewModels
{
    internal partial class LogWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        public ObservableCollection<string> logs = Logger.Logs;

        public string LogsText => string.Join(Environment.NewLine, Logs);

        [ObservableProperty]
        public int maxPercentage, currentPercentage;

        [ObservableProperty]
        public bool progressVisible = true;

        public LogWindowViewModel(bool progressVisible = true)
        {
            Logger.Logs.CollectionChanged += (s, e) => OnPropertyChanged(nameof(LogsText));
            PercentageCalculator.CurrentChange += (value) => CurrentPercentage = value;
            PercentageCalculator.TotalChange += (value) => MaxPercentage = value;
            ProgressVisible = progressVisible;
        }
    }
}
