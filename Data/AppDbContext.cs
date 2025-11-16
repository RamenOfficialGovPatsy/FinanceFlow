using FinanceFlow.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace FinanceFlow.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<GoalCategory> GoalCategories { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<GoalDeposit> GoalDeposits { get; set; }
        public DbSet<AnalyticsReport> AnalyticsReports { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=financeflow_db;Username=financeflow_user;Password=Ff_Postgres_Mdk_2025!");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- GoalCategory (Категории) ---
            modelBuilder.Entity<GoalCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.CategoryId).ValueGeneratedOnAdd();

                entity.ToTable("GoalCategories");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.Color).HasMaxLength(7);
                entity.Property(e => e.Icon).HasMaxLength(20);
                entity.Property(e => e.Name).HasMaxLength(50);
            });

            // --- Goal (Цели) ---
            modelBuilder.Entity<Goal>(entity =>
            {
                entity.HasKey(e => e.GoalId);
                entity.Property(e => e.GoalId).ValueGeneratedOnAdd();

                entity.ToTable("Goals");
                entity.Property(g => g.TargetAmount).HasColumnType("decimal(15,2)");
                entity.Property(g => g.CurrentAmount).HasColumnType("decimal(15,2)").HasDefaultValue(0);
                entity.Property(g => g.StartDate).HasDefaultValueSql("CURRENT_DATE");
                entity.Property(g => g.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(g => g.Title).HasMaxLength(100);
                entity.Property(g => g.ImagePath).HasMaxLength(255);
                entity.Property(g => g.Description).HasMaxLength(500);

                // Связь 1:M (Категория -> Цели)
                entity.HasOne(g => g.GoalCategory)
                      .WithMany(gc => gc.Goals)
                      .HasForeignKey(g => g.CategoryId);

                // Ограничения (CHECK) - ИСПРАВЛЕННЫЕ имена колонок
                entity.ToTable(t => t.HasCheckConstraint("CK_Goals_TargetAmount", "\"TargetAmount\" > 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Goals_CurrentAmount", "\"CurrentAmount\" >= 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Goals_Amounts", "\"CurrentAmount\" <= \"TargetAmount\""));
                entity.ToTable(t => t.HasCheckConstraint("CK_Goals_Dates", "\"EndDate\" > \"StartDate\""));
                entity.ToTable(t => t.HasCheckConstraint("CK_Goals_Priority", "\"Priority\" IN (1, 2, 3)"));
            });

            // --- GoalDeposit (Пополнения) ---
            modelBuilder.Entity<GoalDeposit>(entity =>
            {
                entity.HasKey(e => e.DepositId);
                entity.Property(e => e.DepositId).ValueGeneratedOnAdd();

                entity.ToTable("GoalDeposits");
                entity.Property(gd => gd.Amount).HasColumnType("decimal(15,2)");
                entity.Property(gd => gd.DepositDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(gd => gd.DepositType).HasMaxLength(20).HasDefaultValue("regular");
                entity.Property(gd => gd.Comment).HasMaxLength(200);

                // Связь 1:M (Цель -> Пополнения) с каскадным удалением
                entity.HasOne(gd => gd.Goal)
                      .WithMany(g => g.Deposits)
                      .HasForeignKey(gd => gd.GoalId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Ограничения (CHECK) - ИСПРАВЛЕННЫЕ имена колонок
                entity.ToTable(t => t.HasCheckConstraint("CK_GoalDeposits_Amount", "\"Amount\" > 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_GoalDeposits_Type", "\"DepositType\" IN ('regular', 'salary', 'freelance', 'bonus', 'other')"));
            });

            // --- AnalyticsReport (Отчеты) ---
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

                // Ограничения (CHECK) - ИСПРАВЛЕННЫЕ имена колонок
                entity.ToTable(t => t.HasCheckConstraint("CK_Reports_Type", "\"ReportType\" IN ('monthly', 'quarterly', 'yearly', 'custom')"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Reports_Goals", "\"TotalGoals\" >= \"CompletedGoals\""));
                entity.ToTable(t => t.HasCheckConstraint("CK_Reports_Progress", "\"AverageProgress\" >= 0 AND \"AverageProgress\" <= 100"));
            });

            // --- Предзаполнение данных (Seeding) ---
            modelBuilder.Entity<GoalCategory>().HasData(
                new GoalCategory { CategoryId = 1, Name = "Техника", Icon = "📱", Color = "#8B5CF6", SortOrder = 1, CreatedAt = DateTime.UtcNow, IsActive = true },
                new GoalCategory { CategoryId = 2, Name = "Авто", Icon = "🚗", Color = "#EF4444", SortOrder = 2, CreatedAt = DateTime.UtcNow, IsActive = true },
                new GoalCategory { CategoryId = 3, Name = "Путешествия", Icon = "✈️", Color = "#10B981", SortOrder = 3, CreatedAt = DateTime.UtcNow, IsActive = true },
                new GoalCategory { CategoryId = 4, Name = "Недвижимость", Icon = "🏠", Color = "#F59E0B", SortOrder = 4, CreatedAt = DateTime.UtcNow, IsActive = true },
                new GoalCategory { CategoryId = 5, Name = "Образование", Icon = "🎓", Color = "#3B82F6", SortOrder = 5, CreatedAt = DateTime.UtcNow, IsActive = true },
                new GoalCategory { CategoryId = 6, Name = "Здоровье", Icon = "🏥", Color = "#EC4899", SortOrder = 6, CreatedAt = DateTime.UtcNow, IsActive = true },
                new GoalCategory { CategoryId = 7, Name = "Развлечения", Icon = "🎮", Color = "#8B5CF6", SortOrder = 7, CreatedAt = DateTime.UtcNow, IsActive = true },
                new GoalCategory { CategoryId = 8, Name = "Другое", Icon = "⭐", Color = "#6B7280", SortOrder = 8, CreatedAt = DateTime.UtcNow, IsActive = true }
            );
        }
    }
}