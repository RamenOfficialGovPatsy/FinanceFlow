using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
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

                // ЗАЩИТА: Если дата "пустая" (0001-01-01)
                if (date == DateTime.MinValue)
                {
                    // Если режим цвета - возвращаем прозрачный/серый (чтобы не мелькало красным)
                    if (mode == "Color") return SolidColorBrush.Parse("#6B7280");

                    // Если режим текста - возвращаем пустую строку или "..."
                    // Это скроет надпись "Ошибка даты", пока не подгрузится нормальная дата
                    return string.Empty;
                }

                var today = DateTime.Today;
                var diff = (date.Date - today).Days;

                if (mode == "Color")
                {
                    return diff switch
                    {
                        < 0 => SolidColorBrush.Parse("#EF4444"),
                        <= 7 => SolidColorBrush.Parse("#EF4444"),
                        <= 30 => SolidColorBrush.Parse("#F59E0B"),
                        _ => SolidColorBrush.Parse("#10B981")
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