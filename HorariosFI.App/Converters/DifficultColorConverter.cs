using System;
using System.Globalization;
using Avalonia.Data.Converters;
using HorariosFI.App.Common;

namespace HorariosFI.App.Converters;

public class DifficultColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double difficult) return AppColors.Black;

        return difficult switch
        {
            >= 4 => AppColors.Red,
            >= 2.5 => AppColors.Yellow,
            _ => AppColors.Green
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}