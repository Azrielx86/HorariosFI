using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using HorariosFI.App.Common;

namespace HorariosFI.App.Converters;

public class GradeColorConverter : IValueConverter
{
    
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double grade) return new SolidColorBrush(Color.FromRgb(180, 180, 180));

        return grade switch
        {
            <= 5 => AppColors.Red,
            <= 7 => AppColors.Yellow,
            _ => AppColors.Green
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}