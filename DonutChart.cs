using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Globalization;

namespace FinanceFlow
{
    public class DonutChart : Control
    {
        public static readonly StyledProperty<List<double>> ValuesProperty =
            AvaloniaProperty.Register<DonutChart, List<double>>(nameof(Values));

        public static readonly StyledProperty<List<Color>> ColorsProperty =
            AvaloniaProperty.Register<DonutChart, List<Color>>(nameof(Colors));

        public static readonly StyledProperty<double> ThicknessProperty =
            AvaloniaProperty.Register<DonutChart, double>(nameof(Thickness), 40);

        public List<double> Values { get => GetValue(ValuesProperty); set => SetValue(ValuesProperty, value); }
        public List<Color> Colors { get => GetValue(ColorsProperty); set => SetValue(ColorsProperty, value); }
        public double Thickness { get => GetValue(ThicknessProperty); set => SetValue(ThicknessProperty, value); }

        static DonutChart()
        {
            AffectsRender<DonutChart>(ValuesProperty, ColorsProperty, ThicknessProperty);
            AffectsMeasure<DonutChart>(ValuesProperty, ThicknessProperty);
        }

        // --- ФИКС КРАША: Ограничиваем размер ---
        protected override Size MeasureOverride(Size availableSize)
        {
            double width = double.IsInfinity(availableSize.Width) ? 200 : availableSize.Width;
            double height = double.IsInfinity(availableSize.Height) ? 200 : availableSize.Height;
            double minDim = Math.Min(width, height);
            return new Size(minDim, minDim);
        }
        // ---------------------------------------

        public override void Render(DrawingContext context)
        {
            if (Values == null || !Values.Any() || Colors == null || !Colors.Any()) return;

            double total = Values.Sum();
            if (total <= 0) return;

            double width = Bounds.Width;
            double height = Bounds.Height;
            double minDim = Math.Min(width, height);
            Point center = new Point(width / 2, height / 2);

            double outerRadius = minDim / 2;
            double innerRadius = outerRadius - Thickness;

            if (innerRadius <= 0) return;

            double startAngle = -90;
            var borderPen = new Pen(Brushes.White, 1.5);
            var typeface = new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.SemiBold);

            for (int i = 0; i < Values.Count; i++)
            {
                double val = Values[i];
                Color color = i < Colors.Count ? Colors[i] : Colors[0];
                Color labelColor = i < Colors.Count ? Colors[i] : Colors[0];

                double sweepAngle = (val / total) * 360;
                double percentage = (val / total) * 100;

                DrawDonutSegment(context, center, innerRadius, outerRadius, startAngle, sweepAngle, color, borderPen);

                if (percentage >= 5)
                {
                    DrawPercentageLabel(context, center, outerRadius + 15, startAngle, sweepAngle, $"{percentage:F0}%", labelColor, typeface);
                }
                startAngle += sweepAngle;
            }
        }

        private void DrawDonutSegment(DrawingContext context, Point center, double innerRadius, double outerRadius, double startAngle, double sweepAngle, Color color, Pen borderPen)
        {
            double startRad = startAngle * Math.PI / 180;
            double endRad = (startAngle + sweepAngle) * Math.PI / 180;

            Point outerStart = new Point(center.X + outerRadius * Math.Cos(startRad), center.Y + outerRadius * Math.Sin(startRad));
            Point outerEnd = new Point(center.X + outerRadius * Math.Cos(endRad), center.Y + outerRadius * Math.Sin(endRad));
            Point innerStart = new Point(center.X + innerRadius * Math.Cos(startRad), center.Y + innerRadius * Math.Sin(startRad));
            Point innerEnd = new Point(center.X + innerRadius * Math.Cos(endRad), center.Y + innerRadius * Math.Sin(endRad));

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(outerStart, true);
                ctx.ArcTo(outerEnd, new Size(outerRadius, outerRadius), 0, sweepAngle > 180, SweepDirection.Clockwise);
                ctx.LineTo(innerEnd);
                ctx.ArcTo(innerStart, new Size(innerRadius, innerRadius), 0, sweepAngle > 180, SweepDirection.CounterClockwise);
                ctx.LineTo(outerStart);
                ctx.EndFigure(true);
            }
            context.DrawGeometry(new SolidColorBrush(color), borderPen, geometry);
        }

        private void DrawPercentageLabel(DrawingContext context, Point center, double radius, double startAngle, double sweepAngle, string text, Color color, Typeface typeface)
        {
            double midAngle = startAngle + (sweepAngle / 2);
            double midRad = midAngle * Math.PI / 180;
            Point textPos = new Point(center.X + radius * Math.Cos(midRad), center.Y + radius * Math.Sin(midRad));
            var formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 14, new SolidColorBrush(color));
            Point renderPos = new Point(textPos.X - formattedText.Width / 2, textPos.Y - formattedText.Height / 2);
            context.DrawText(formattedText, renderPos);
        }
    }
}