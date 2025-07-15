using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace PitCrew.Models
{
    public partial class Language : ObservableObject
    {
        [ObservableProperty]
        public string name;

        [ObservableProperty]
        public IRelayCommand command;

        public Language(string name, Action action)
        {
            Name = name;
            Command = new RelayCommand(action);
        }
    }
}
