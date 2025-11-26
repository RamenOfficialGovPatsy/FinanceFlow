using FinanceFlow.Data;
using FinanceFlow.Models;
using FinanceFlow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceFlow.Services.Implementations
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;

        public AnalyticsService(AppDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<AnalyticsSummaryDto> GetGeneralStatisticsAsync()
        {
            var goals = await _context.Goals.ToListAsync();
            var summary = new AnalyticsSummaryDto();

            if (!goals.Any()) return summary;

            summary.TotalGoals = goals.Count;
            summary.CompletedGoals = goals.Count(g => g.IsCompleted);
            summary.OverdueGoals = goals.Count(g => !g.IsCompleted && g.EndDate < DateTime.Today);
            summary.InProgressGoals = goals.Count(g => !g.IsCompleted && g.EndDate >= DateTime.Today);
            summary.TotalTargetAmount = goals.Sum(g => g.TargetAmount);
            summary.TotalCurrentAmount = goals.Sum(g => g.CurrentAmount);

            if (summary.TotalTargetAmount > 0)
            {
                summary.AverageProgress = (double)(summary.TotalCurrentAmount / summary.TotalTargetAmount) * 100;
                if (summary.AverageProgress > 100) summary.AverageProgress = 100;
            }

            return summary;
        }

        public async Task<List<CategoryDistributionItem>> GetCategoryDistributionAsync()
        {
            var goals = await _context.Goals.Include(g => g.GoalCategory).ToListAsync();
            if (!goals.Any()) return new List<CategoryDistributionItem>();

            var totalGoals = goals.Count;

            return goals
                .GroupBy(g => g.GoalCategory)
                .Select(g => new CategoryDistributionItem
                {
                    CategoryName = g.Key != null ? g.Key.Name : "Без категории",
                    Icon = g.Key != null ? g.Key.Icon : "⭐",
                    Color = g.Key != null ? g.Key.Color : "#6B7280",
                    GoalsCount = g.Count(),
                    Percentage = Math.Round((double)g.Count() / totalGoals * 100, 1)
                })
                .OrderByDescending(x => x.Percentage)
                .ToList();
        }

        public async Task<List<Goal>> GetUpcomingDeadlinesAsync(int count = 3)
        {
            return await _context.Goals
                .Include(g => g.GoalCategory)
                .Where(g => !g.IsCompleted)
                .OrderBy(g => g.EndDate)
                .Take(count)
                .ToListAsync();
        }

        // --- ГЕНЕРАЦИЯ PDF ---
        public async Task<string> GeneratePdfReportAsync(string reportType)
        {
            var stats = await GetGeneralStatisticsAsync();
            var allGoals = await _context.Goals
                .Include(g => g.GoalCategory)
                .OrderByDescending(g => g.IsCompleted)
                .ThenBy(g => g.EndDate)
                .ToListAsync();

            // 1. Создаем запись в БД
            var reportRecord = new AnalyticsReport
            {
                ReportType = reportType,
                ReportDate = DateTime.Today,
                GeneratedAt = DateTime.Now,
                TotalGoals = stats.TotalGoals,
                CompletedGoals = stats.CompletedGoals,
                TotalTargetAmount = stats.TotalTargetAmount,
                TotalCurrentAmount = stats.TotalCurrentAmount,
                AverageProgress = (decimal)stats.AverageProgress
            };
            _context.AnalyticsReports.Add(reportRecord);
            await _context.SaveChangesAsync();

            // 2. Настройка путей (Папка "Документы/FinanceFlow-Отчеты")
            string docsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            // ИСПРАВЛЕНИЕ: Путь изменен по требованию
            string reportsFolder = Path.Combine(docsFolder, "FinanceFlow-Отчеты");

            if (!Directory.Exists(reportsFolder))
            {
                Directory.CreateDirectory(reportsFolder);
            }

            // Имя файла: FinanceFlow_Report_yyyy-MM-dd_HH_mm_ss.pdf
            string fileName = $"FinanceFlow_Report_{DateTime.Now:yyyy-MM-dd_HH_mm_ss}.pdf";
            string filePath = Path.Combine(reportsFolder, fileName);

            // ЦВЕТОВАЯ ПАЛИТРА (DARK)
            string BgColor = "#1F2937";
            string CardBgColor = "#374151";
            string TextColor = "#F9FAFB";
            string TextSecondary = "#9CA3AF";
            string BorderColor = "#4B5563";
            string AccentColor = "#8B5CF6";

            // 3. Верстка документа
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(BgColor);

                    // ИСПРАВЛЕНИЕ: Шрифт Calibri
                    page.DefaultTextStyle(x => x.FontFamily("DejaVu Sans").FontSize(14).FontColor(TextColor));

                    // --- ШАПКА ---
                    page.Header().Column(headerCol =>
                    {
                        headerCol.Item()
                            .BorderBottom(2).BorderColor(AccentColor)
                            .PaddingBottom(20)
                            .Row(row =>
                            {
                                row.RelativeItem().Text("FinanceFlow")
                                    .FontSize(28).Bold().FontColor(AccentColor);

                                row.RelativeItem().AlignRight().Column(info =>
                                {
                                    info.Item().Text("Отчет по финансовым целям").FontSize(14).FontColor(TextSecondary);
                                    info.Item().Text(t =>
                                    {
                                        t.Span("Дата генерации: ").FontSize(14).FontColor(TextSecondary);
                                        // ИСПРАВЛЕНИЕ: Формат HH:mm (24 часа)
                                        t.Span($"{reportRecord.GeneratedAt:dd.MM.yyyy HH:mm}").Bold().FontColor(TextSecondary);
                                    });
                                });
                            });

                        headerCol.Item().Height(30);
                    });

                    // --- КОНТЕНТ ---
                    page.Content().Column(col =>
                    {
                        // Сводка
                        col.Item().Row(row =>
                        {
                            row.Spacing(20);
                            DrawCard(row, "Всего целей", $"{stats.TotalGoals}", false, CardBgColor, TextSecondary, TextColor, AccentColor);
                            DrawCard(row, "Накоплено", $"{stats.TotalCurrentAmount:N0} ₽", true, CardBgColor, TextSecondary, TextColor, AccentColor);
                            DrawCard(row, "Общий прогресс", $"{stats.AverageProgress:F0}%", false, CardBgColor, TextSecondary, TextColor, AccentColor);
                        });

                        col.Item().Height(30);

                        // Заголовок таблицы
                        col.Item()
                            .BorderLeft(4).BorderColor(AccentColor)
                            .PaddingLeft(10)
                            .Text("Детализация целей")
                            .FontSize(18).Bold().FontColor(TextColor);

                        col.Item().Height(15);

                        // Таблица
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(40);
                                columns.RelativeColumn(20);
                                columns.RelativeColumn(25);
                                columns.RelativeColumn(15);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(c => HeaderCellStyle(c, CardBgColor, BorderColor)).Text("Название");
                                header.Cell().Element(c => HeaderCellStyle(c, CardBgColor, BorderColor)).Text("Категория");
                                header.Cell().Element(c => HeaderCellStyle(c, CardBgColor, BorderColor)).Text("Прогресс (Сумма)");
                                header.Cell().Element(c => HeaderCellStyle(c, CardBgColor, BorderColor)).Text("Статус");
                            });

                            foreach (var goal in allGoals)
                            {
                                table.Cell().Element(c => BodyCellStyle(c, BorderColor)).Text(goal.Title);
                                table.Cell().Element(c => BodyCellStyle(c, BorderColor)).Text(goal.GoalCategory?.Name ?? "-");

                                var percent = goal.TargetAmount > 0 ? (goal.CurrentAmount / goal.TargetAmount) * 100 : 0;
                                table.Cell().Element(c => BodyCellStyle(c, BorderColor)).Text($"{percent:F0}% ({goal.CurrentAmount:N0})");

                                string statusText;
                                string colorHex;

                                if (goal.IsCompleted)
                                {
                                    statusText = "Выполнено";
                                    colorHex = "#10B981";
                                }
                                else if (goal.EndDate < DateTime.Today)
                                {
                                    statusText = "Просрочено";
                                    colorHex = "#EF4444";
                                }
                                else
                                {
                                    statusText = "В процессе";
                                    colorHex = "#F59E0B";
                                }

                                bool isBold = goal.IsCompleted || goal.EndDate < DateTime.Today;

                                table.Cell().Element(c => BodyCellStyle(c, BorderColor)).Text(t =>
                                {
                                    var txt = t.Span(statusText).FontColor(colorHex);
                                    if (isBold) txt.Bold();
                                });
                            }
                        });
                    });

                    // --- ФУТЕР ---
                    page.Footer().Column(col =>
                    {
                        col.Item().PaddingTop(20).BorderTop(1).BorderColor(BorderColor);
                        col.Item().AlignCenter().Text("Документ сгенерирован автоматически приложением FinanceFlow.")
                            .FontSize(12).FontColor(TextSecondary);

                        col.Item().AlignCenter().Text(x =>
                        {
                            x.Span("Страница ")
                             .FontSize(10).FontColor(TextSecondary);
                            x.CurrentPageNumber()
                             .FontSize(10).FontColor(TextSecondary);
                            x.Span(" из ")
                             .FontSize(10).FontColor(TextSecondary);
                            x.TotalPages()
                             .FontSize(10).FontColor(TextSecondary);
                        });
                    });
                });
            })
            .GeneratePdf(filePath);

            return filePath;
        }

        private void DrawCard(RowDescriptor row, string title, string value, bool isAccent, string bg, string textSec, string textMain, string accent)
        {
            row.RelativeItem()
                .Background(bg)
                .CornerRadius(8)
                .Padding(15)
                .Column(col =>
                {
                    col.Item().AlignCenter().Text(title.ToUpper())
                        .FontSize(12).FontColor(textSec).LetterSpacing(0.1f);

                    col.Item().Height(5);

                    var txt = col.Item().AlignCenter().Text(value)
                        .FontSize(20).Bold().FontColor(textMain);

                    if (isAccent) txt.FontColor(accent);
                });
        }

        private static IContainer HeaderCellStyle(IContainer container, string bg, string border)
        {
            return container
                .Background(bg)
                .BorderBottom(2).BorderColor(border)
                .Padding(10);
        }

        private static IContainer BodyCellStyle(IContainer container, string border)
        {
            return container
                .BorderBottom(1).BorderColor(border)
                .Padding(10);
        }
    }
}