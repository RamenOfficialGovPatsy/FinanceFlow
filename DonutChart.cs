using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Globalization;

namespace FinanceFlow
{
    public class DonutChart : Control
    {
        // Зависимые свойства для настройки диаграммы
        public static readonly StyledProperty<List<double>> ValuesProperty =
            AvaloniaProperty.Register<DonutChart, List<double>>(nameof(Values));

        public static readonly StyledProperty<List<Color>> ColorsProperty =
            AvaloniaProperty.Register<DonutChart, List<Color>>(nameof(Colors));

        public static readonly StyledProperty<double> ThicknessProperty =
            AvaloniaProperty.Register<DonutChart, double>(nameof(Thickness), 40);

        // Публичные свойства для привязки данных
        public List<double> Values { get => GetValue(ValuesProperty); set => SetValue(ValuesProperty, value); }
        public List<Color> Colors { get => GetValue(ColorsProperty); set => SetValue(ColorsProperty, value); }
        public double Thickness { get => GetValue(ThicknessProperty); set => SetValue(ThicknessProperty, value); }

        // Конструктор
        static DonutChart()
        {
            AffectsRender<DonutChart>(ValuesProperty, ColorsProperty, ThicknessProperty);
            AffectsMeasure<DonutChart>(ValuesProperty, ThicknessProperty);
        }

        // Предотвращает проблемы с бесконечными размерами в Avalonia
        protected override Size MeasureOverride(Size availableSize)
        {
            // Если размер не ограничен - используем 200x200 как минимальный
            double width = double.IsInfinity(availableSize.Width) ? 200 : availableSize.Width;
            double height = double.IsInfinity(availableSize.Height) ? 200 : availableSize.Height;

            // Диаграмма всегда квадратная - берем минимальное измерение
            double minDim = Math.Min(width, height);
            return new Size(minDim, minDim);
        }

        // Основной метод отрисовки диаграммы
        public override void Render(DrawingContext context)
        {
            // Проверяем что есть данные для отрисовки
            if (Values == null || !Values.Any() || Colors == null || !Colors.Any()) return;

            // Считаем общую сумму всех значений
            double total = Values.Sum();
            if (total <= 0) return;

            // Вычисляем геометрические параметры
            double width = Bounds.Width;
            double height = Bounds.Height;

            // Гарантируем квадратную форму
            double minDim = Math.Min(width, height);

            // Центр диаграммы
            Point center = new Point(width / 2, height / 2);

            // Вычисляем радиусы внешней и внутренней окружности
            double outerRadius = minDim / 2;
            double innerRadius = outerRadius - Thickness;

            // Защита от некорректной толщины
            if (innerRadius <= 0) return;

            // Начальный угол (12 часов)
            double startAngle = -90;

            // Белая обводка сегментов
            var borderPen = new Pen(Brushes.White, 1.5);
            var typeface = new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.SemiBold);

            // Отрисовываем каждый сегмент диаграммы
            for (int i = 0; i < Values.Count; i++)
            {
                double val = Values[i];
                Color color = i < Colors.Count ? Colors[i] : Colors[0];
                Color labelColor = i < Colors.Count ? Colors[i] : Colors[0];

                // Вычисляем угол сегмента пропорционально значению
                double sweepAngle = (val / total) * 360;
                double percentage = (val / total) * 100;

                // Рисуем сегмент диаграммы
                DrawDonutSegment(context, center, innerRadius, outerRadius, startAngle, sweepAngle, color, borderPen);

                // Рисуем подпись процента если сегмент достаточно большой
                if (percentage >= 5)
                {
                    DrawPercentageLabel(context, center, outerRadius + 15, startAngle, sweepAngle, $"{percentage:F0}%", labelColor, typeface);
                }
                startAngle += sweepAngle;
            }
        }

        // Отрисовка одного сегмента диаграммы (кольца)
        private void DrawDonutSegment(DrawingContext context, Point center, double innerRadius, double outerRadius, double startAngle, double sweepAngle, Color color, Pen borderPen)
        {
            // Конвертируем углы из градусов в радианы
            double startRad = startAngle * Math.PI / 180;
            double endRad = (startAngle + sweepAngle) * Math.PI / 180;

            // Вычисляем ключевые точки сегмента
            Point outerStart = new Point(center.X + outerRadius * Math.Cos(startRad), center.Y + outerRadius * Math.Sin(startRad));
            Point outerEnd = new Point(center.X + outerRadius * Math.Cos(endRad), center.Y + outerRadius * Math.Sin(endRad));
            Point innerStart = new Point(center.X + innerRadius * Math.Cos(startRad), center.Y + innerRadius * Math.Sin(startRad));
            Point innerEnd = new Point(center.X + innerRadius * Math.Cos(endRad), center.Y + innerRadius * Math.Sin(endRad));

            // Создаем геометрию сегмента
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                // Начинаем с внешней точки
                ctx.BeginFigure(outerStart, true);

                // Рисуем дугу по внешнему радиусу
                ctx.ArcTo(outerEnd, new Size(outerRadius, outerRadius), 0, sweepAngle > 180, SweepDirection.Clockwise);

                // Переходим к внутреннему радиусу
                ctx.LineTo(innerEnd);

                // Рисуем дугу по внутреннему радиусу (обратным направлением)
                ctx.ArcTo(innerStart, new Size(innerRadius, innerRadius), 0, sweepAngle > 180, SweepDirection.CounterClockwise);

                // Замыкаем фигуру
                ctx.LineTo(outerStart);
                ctx.EndFigure(true);
            }

            // Отрисовываем сегмент с заливкой и обводкой
            context.DrawGeometry(new SolidColorBrush(color), borderPen, geometry);
        }

        // Отрисовка подписи с процентом для сегмента
        private void DrawPercentageLabel(DrawingContext context, Point center, double radius, double startAngle, double sweepAngle, string text, Color color, Typeface typeface)
        {
            // Вычисляем середину сегмента для размещения текста
            double midAngle = startAngle + (sweepAngle / 2);
            double midRad = midAngle * Math.PI / 180;

            // Позиция текста на окружности за пределами диаграммы
            Point textPos = new Point(center.X + radius * Math.Cos(midRad), center.Y + radius * Math.Sin(midRad));

            // Создаем форматированный текст
            var formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 14, new SolidColorBrush(color));

            // Центрируем текст относительно вычисленной позиции
            Point renderPos = new Point(textPos.X - formattedText.Width / 2, textPos.Y - formattedText.Height / 2);

            // Отрисовываем текст
            context.DrawText(formattedText, renderPos);
        }
    }
}