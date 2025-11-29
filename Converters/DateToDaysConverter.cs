using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;

namespace FinanceFlow.Converters
{
    public class DateToDaysConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                string mode = parameter as string ?? "Text";

                // Защита от неинициализированных дат (DateTime.MinValue)
                if (date == DateTime.MinValue)
                {
                    // Возвращаем нейтральный цвет или пустую строку вместо ошибки
                    if (mode == "Color") return SolidColorBrush.Parse("#6B7280");
                    return string.Empty;
                }

                var today = DateTime.Today;
                var diff = (date.Date - today).Days;

                if (mode == "Color")
                {
                    return diff switch
                    {
                        < 0 => SolidColorBrush.Parse("#EF4444"), // Красный: просрочено
                        <= 7 => SolidColorBrush.Parse("#EF4444"), // Красный: меньше недели
                        <= 30 => SolidColorBrush.Parse("#F59E0B"), // Оранжевый: меньше месяца
                        _ => SolidColorBrush.Parse("#10B981") // Зеленый: больше месяца
                    };
                }
                else
                {
                    if (diff < 0) return $"Просрочено ({Math.Abs(diff)} дн.)";
                    if (diff == 0) return "Сегодня";
                    return $"{diff} дней";
                }
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}