using Avalonia.Data.Converters;
using System.Globalization;

namespace FinanceFlow.Converters
{
    public class PriorityToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int priority)
            {
                return priority switch
                {
                    1 => "#EF4444", // Высокий - красный
                    2 => "#F59E0B", // Средний - оранжевый  
                    3 => "#10B981", // Низкий - зеленый
                    _ => "#6B7280"  // По умолчанию - серый
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