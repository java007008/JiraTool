using JiraTool.Models;
using Microsoft.EntityFrameworkCore;

namespace JiraTool.Data
{
    /// <summary>
    /// 应用程序数据库上下文
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options">DbContext配置选项</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Jira主任务表
        /// </summary>
        public DbSet<JiraTask> Tasks { get; set; } = null!;

        /// <summary>
        /// Jira子任务表
        /// </summary>
        public DbSet<JiraSubTask> SubTasks { get; set; } = null!;

        /// <summary>
        /// 用户设置表
        /// </summary>
        public DbSet<UserSettings> UserSettings { get; set; } = null!;

        /// <summary>
        /// 模型创建配置
        /// </summary>
        /// <param name="modelBuilder">模型构建器</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置主任务和子任务的关系
            modelBuilder.Entity<JiraSubTask>()
                .HasOne(s => s.ParentTask)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(s => s.ParentTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // 索引配置
            modelBuilder.Entity<JiraTask>()
                .HasIndex(t => t.TaskNumber)
                .IsUnique();

            modelBuilder.Entity<JiraSubTask>()
                .HasIndex(s => s.TaskNumber)
                .IsUnique();

            // 初始化默认设置
            modelBuilder.Entity<UserSettings>().HasData(
                new UserSettings
                {
                    Id = 1,
                    Username = string.Empty,
                    JiraServerUrl = "https://jira.example.com",
                    RefreshInterval = 1,
                    ShowTaskNameColumn = true,
                    ShowTaskNumberColumn = true,
                    ShowParentTaskNumberColumn = true,
                    ShowTaskStatusColumn = true,
                    ShowParentTaskStatusColumn = true,
                    ShowEstimatedHoursColumn = true,
                    ShowEstimatedCompletionTimeColumn = false,
                    ShowActualCompletionTimeColumn = false,
                    ShowCreatedAtColumn = true,
                    ShowUpdatedAtColumn = true,
                    ShowPriorityColumn = true,
                    ShowBanCheNameColumn = true,
                    ShowCodeMergedColumn = true,
                    ShowSqlColumn = true,
                    ShowConfigurationColumn = true,
                    WindowWidth = 1280,
                    WindowHeight = 720,
                    MinimizeToTrayOnClose = true
                });
        }
    }
}
