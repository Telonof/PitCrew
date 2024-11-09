using System;
using System.Globalization;
using Avalonia.Data.Converters;
using PitCrewCommon;

namespace PitCrew;

//This is for Xaml's being able to load translation data.
public class TranslateBinding : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string key)
            return value;

        return Translate.Get(key);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
