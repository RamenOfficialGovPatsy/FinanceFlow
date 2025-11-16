using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace FinanceFlow.Converters
{
    public class ProgressToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal progress)
            {
                return progress switch
                {
                    >= 100 => "#10B981",
                    >= 67 => "#10B981",
                    >= 34 => "#F59E0B",
                    _ => "#EF4444"
                };
            }
            return "#6B7280";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}