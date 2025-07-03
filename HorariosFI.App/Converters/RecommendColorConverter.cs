using System;
using System.Globalization;
using Avalonia.Data.Converters;
using HorariosFI.App.Common;

namespace HorariosFI.App.Converters;

public class RecommendColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double recommend) return AppColors.Black;

        return recommend switch
        {
            <= 50 => AppColors.Red,
            <= 70 => AppColors.Yellow,
            _ => AppColors.Green
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}