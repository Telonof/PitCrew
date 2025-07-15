using Avalonia.Data.Converters;
using PitCrewCommon;
using System;
using System.Globalization;

namespace PitCrew.Systems
{
    //This is for axaml's being able to load translation data.
    internal class TranslateConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string key)
                return value;

            return Translatable.Get(key);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}