using Avalonia.Data.Converters;
using System.Globalization;

namespace FinanceFlow.Converters
{
    public class ProgressToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal progress)
            {
                // Цветовая шкала прогресса:
                return progress switch
                {
                    >= 100 => "#10B981", // Зеленый: цель достигнута
                    >= 67 => "#10B981", // Зеленый: больше 2/3 выполнено
                    >= 34 => "#F59E0B", // Оранжевый: от 1/3 до 2/3
                    _ => "#EF4444" // Красный: меньше 1/3
                };
            }
            // Серый цвет по умолчанию при отсутствии данных
            return "#6B7280";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}