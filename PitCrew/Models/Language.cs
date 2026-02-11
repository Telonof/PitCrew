using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Globalization;

namespace PitCrew.Models
{
    public partial class Language : ObservableObject
    {
        [ObservableProperty]
        public string name;

        [ObservableProperty]
        public string displayName;

        [ObservableProperty]
        public IRelayCommand command;

        public Language(string name, Action action)
        {
            Name = name;
            DisplayName = GetLocalizedName(name);
            Command = new RelayCommand(action);
        }

        private string GetLocalizedName(string code)
        {
            string name = CultureInfo.GetCultureInfo(code).NativeName;

            int bracket = name.IndexOf('(');
            if (bracket < 0)
                bracket = name.IndexOf('（');

            if (bracket >= 0)
                name = name.Substring(0, bracket);

            return char.ToUpper(name[0]) + name.Substring(1);
        }
    }
}
