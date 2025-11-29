using Avalonia.Data.Converters;
using System.Globalization;

namespace FinanceFlow.Converters
{
    public class ProgressToWidthConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal progress && progress >= 0 && progress <= 100)
            {
                // Преобразуем процент прогресса в пиксели:
                // 100% = 300px, поэтому умножаем на 3
                return progress * 3;
            }
            return 0;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}