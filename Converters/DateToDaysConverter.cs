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
                // 1. Сначала определяем режим (Text или Color), так как он нужен везде
                string mode = parameter as string ?? "Text";

                // 2. ЗАЩИТА: Если дата "пустая" (0001-01-01)
                if (date == DateTime.MinValue)
                {
                    // Если просят цвет - возвращаем серый
                    if (mode == "Color")
                        return SolidColorBrush.Parse("#6B7280");

                    // Если просят текст - возвращаем сообщение об ошибке
                    return "Ошибка даты";
                }

                // 3. Основная логика
                var today = DateTime.Today;
                var diff = (date.Date - today).Days;

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