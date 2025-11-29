using FinanceFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceFlow.Data
{
    public class AppDbContext : DbContext
    {
        // Статический конструктор для глобальной настройки поведения Npgsql
        static AppDbContext()
        {
            // Включил совместимость с legacy timestamp поведением для DateTime
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        // Коллекции сущностей для работы с таблицами
        public DbSet<GoalCategory> GoalCategories { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<GoalDeposit> GoalDeposits { get; set; }
        public DbSet<AnalyticsReport> AnalyticsReports { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // Подключение к PostgreSQL с предустановленными учетными данными
            optionsBuilder.UseNpgsql("Host=localhost;Database=financeflow_db;Username=financeflow_user;Password=Ff_Postgres_Mdk_2025!");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Глобальная настройка: все DateTime свойства хранятся без timezone
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("timestamp without time zone");
                    }
                }
            }

            // Конфигурация таблицы GoalCategories
            modelBuilder.Entity<GoalCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.CategoryId).ValueGeneratedOnAdd();
                entity.ToTable("GoalCategories");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.Color).HasMaxLength(7); // HEX формат #RRGGBB
                entity.Property(e => e.Icon).HasMaxLength(20); // Эмодзи
                entity.Property(e => e.Name).HasMaxLength(50); // Название категории
            });

            // Конфигурация таблицы Goals
            modelBuilder.Entity<Goal>(entity =>
            {
                entity.HasKey(e => e.GoalId);
                entity.Property(e => e.GoalId).ValueGeneratedOnAdd();
                entity.ToTable("Goals");

                // Настройка денежных полей с точностью 2 знака после запятой
                entity.Property(g => g.TargetAmount).HasColumnType("decimal(15,2)");
                entity.Property(g => g.CurrentAmount).HasColumnType("decimal(15,2)").HasDefaultValue(0);

                // Значения по умолчанию для дат
                entity.Property(g => g.StartDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(g => g.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Ограничения длины строк
                entity.Property(g => g.Title).HasMaxLength(100);
                entity.Property(g => g.ImagePath).HasMaxLength(255);
                entity.Property(g => g.Description).HasMaxLength(500);

                // Связь с категорией (один-ко-многим)
                entity.HasOne(g => g.GoalCategory)
                      .WithMany(gc => gc.Goals)
                      .HasForeignKey(g => g.CategoryId);

                // Проверочные ограничения для целостности данных
                entity.ToTable(t => t.HasCheckConstraint("CK_Goals_TargetAmount", "\"TargetAmount\" > 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Goals_CurrentAmount", "\"CurrentAmount\" >= 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Goals_Amounts", "\"CurrentAmount\" <= \"TargetAmount\""));
                entity.ToTable(t => t.HasCheckConstraint("CK_Goals_Priority", "\"Priority\" IN (1, 2, 3)"));
            });

            // Конфигурация таблицы GoalDeposits
            modelBuilder.Entity<GoalDeposit>(entity =>
            {
                entity.HasKey(e => e.DepositId);
                entity.Property(e => e.DepositId).ValueGeneratedOnAdd();
                entity.ToTable("GoalDeposits");
                entity.Property(gd => gd.Amount).HasColumnType("decimal(15,2)");
                entity.Property(gd => gd.DepositDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(gd => gd.DepositType).HasMaxLength(20).HasDefaultValue("regular");
                entity.Property(gd => gd.Comment).HasMaxLength(200);

                // Связь с целью + каскадное удаление
                entity.HasOne(gd => gd.Goal)
                      .WithMany(g => g.Deposits)
                      .HasForeignKey(gd => gd.GoalId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Проверочные ограничения
                entity.ToTable(t => t.HasCheckConstraint("CK_GoalDeposits_Amount", "\"Amount\" > 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_GoalDeposits_Type", "\"DepositType\" IN ('regular', 'salary', 'freelance', 'bonus', 'other')"));
            });

            // Конфигурация таблицы AnalyticsReports 
            modelBuilder.Entity<AnalyticsReport>(entity =>
            {
                entity.HasKey(e => e.ReportId);
                entity.Property(e => e.ReportId).ValueGeneratedOnAdd();
                entity.ToTable("AnalyticsReports");
                entity.Property(e => e.GeneratedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.ReportType).HasMaxLength(20);
                entity.Property(e => e.TotalTargetAmount).HasColumnType("decimal(15,2)").HasDefaultValue(0);
                entity.Property(e => e.TotalCurrentAmount).HasColumnType("decimal(15,2)").HasDefaultValue(0);
                entity.Property(e => e.AverageProgress).HasColumnType("decimal(5,2)").HasDefaultValue(0);

                // Проверочные ограничения для отчетов
                entity.ToTable(t => t.HasCheckConstraint("CK_Reports_Type", "\"ReportType\" IN ('monthly', 'quarterly', 'yearly', 'custom')"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Reports_Goals", "\"TotalGoals\" >= \"CompletedGoals\""));
                entity.ToTable(t => t.HasCheckConstraint("CK_Reports_Progress", "\"AverageProgress\" >= 0 AND \"AverageProgress\" <= 100"));
            });
        }
    }
}