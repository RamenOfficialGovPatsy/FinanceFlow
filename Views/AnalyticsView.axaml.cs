using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FinanceFlow; // Для DonutChart
using System.Collections.Generic;

namespace FinanceFlow.Views
{
    public partial class AnalyticsView : UserControl
    {
        public AnalyticsView()
        {
            InitializeComponent();

            var chart = this.FindControl<DonutChart>("CategoryChart");

            if (chart != null)
            {
                // Значения
                chart.Values = new List<double> { 42, 25, 15, 10, 8 };

                // Цвета (Темные)
                chart.Colors = new List<Color>
                {
                    Color.Parse("#311B92"), // Техника
                    Color.Parse("#1A237E"), // Путешествия
                    Color.Parse("#880E4F"), // Авто
                    Color.Parse("#B45309"), // Жилье
                    Color.Parse("#065F46")  // Образование
                };
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void GeneratePdfButton_Click(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("Генерация PDF отчета...");
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            if (VisualRoot is Window parentWindow)
            {
                parentWindow.Close();
            }
        }
    }
}