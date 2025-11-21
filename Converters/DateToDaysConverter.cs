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
                var today = DateTime.Today;
                var diff = (date.Date - today).Days;
                string mode = parameter as string ?? "Text";

                // Режим "Color": Возвращаем кисть для цвета текста
                if (mode == "Color")
                {
                    return diff switch
                    {
                        < 0 => SolidColorBrush.Parse("#EF4444"),   // Просрочено (Красный)
                        <= 7 => SolidColorBrush.Parse("#EF4444"),  // Горит (Красный)
                        <= 30 => SolidColorBrush.Parse("#F59E0B"), // Скоро (Оранжевый)
                        _ => SolidColorBrush.Parse("#10B981")      // Не скоро (Зеленый)
                    };
                }
                // Режим "Text": Возвращаем строку
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