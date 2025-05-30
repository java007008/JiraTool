using JiraTool.Data;
using JiraTool.Models;
using JiraTool.Services;
using JiraTool.Services.Interfaces;
using JiraTool.Utils;
using JiraTool.ViewModels;
using JiraTool.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace JiraTool
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 主机服务对象
        /// </summary>
        public static IHost? AppHost { get; private set; }

        /// <summary>
        /// 应用启动事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // 初始化应用程序
                AppHost = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        ConfigureServices(services);
                    })
                    .Build();

                // 启动所有服务
                AppHost.Start();

                // 初始化数据库
                InitializeDatabase();

                // 创建并显示主窗口
                var mainWindow = new MainWindow(AppHost.Services.GetRequiredService<MainViewModel>());
                mainWindow.Show();
                Current.MainWindow = mainWindow;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"应用程序启动错误: {ex.Message}\n\n{ex.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        /// <summary>
        /// 配置依赖注入服务
        /// </summary>
        /// <param name="services">服务集合</param>
        private void ConfigureServices(IServiceCollection services)
        {
            // 注册数据库上下文
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jira_tool.db");
            string connectionString = $"Data Source={dbPath}";
            
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(connectionString);
            }, ServiceLifetime.Transient);
            
            // 注册数据库上下文工厂
            services.AddDbContextFactory<ApplicationDbContext>(options =>
            {
                options.UseSqlite(connectionString);
            });

            // 注册配置服务
            services.AddSingleton<AppConfigService>(provider => 
            {
                var logger = provider.GetService<ILogger<AppConfigService>>();
                var configuration = provider.GetService<IConfiguration>();
                return new AppConfigService(logger, configuration);
            });
            
            // 注册其他服务
            services.AddSingleton<IJiraService, JiraWebService>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<ITrayIconService, TrayIconService>();
            services.AddSingleton<DatabaseService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<IJiraLoginService, JiraLoginService>();
            services.AddSingleton<IJiraDataScraperService, JiraDataScraperService>();
            
            // 初始化CEF浏览器
            CefBrowserHelper.Initialize();

            // 注册视图模型
            services.AddSingleton<LoginViewModel>(provider => {
                var jiraLoginService = provider.GetRequiredService<IJiraLoginService>();
                var notificationService = provider.GetRequiredService<INotificationService>();
                var databaseService = provider.GetRequiredService<IDatabaseService>();
                var configService = provider.GetRequiredService<AppConfigService>();
                return new LoginViewModel(jiraLoginService, notificationService, databaseService, configService);
            });
            services.AddSingleton<TaskListViewModel>();
            services.AddSingleton<TrayIconViewModel>();
            services.AddSingleton<TestViewModel>();
            services.AddSingleton<MainViewModel>(provider => {
                var notificationService = provider.GetRequiredService<INotificationService>();
                var trayIconService = provider.GetRequiredService<ITrayIconService>();
                var databaseService = provider.GetRequiredService<DatabaseService>();
                var jiraService = provider.GetRequiredService<IJiraService>();
                var jiraLoginService = provider.GetRequiredService<IJiraLoginService>();
                var jiraDataScraperService = provider.GetRequiredService<IJiraDataScraperService>();
                var loginViewModel = provider.GetRequiredService<LoginViewModel>();
                var taskListViewModel = provider.GetRequiredService<TaskListViewModel>();
                var testViewModel = provider.GetRequiredService<TestViewModel>();
                return new MainViewModel(notificationService, trayIconService, databaseService, jiraService, jiraLoginService, jiraDataScraperService, loginViewModel, taskListViewModel, testViewModel);
            });

            // 注册视图
            services.AddSingleton<MainWindow>();
            services.AddTransient<LoginView>();
            services.AddTransient<TaskListView>();
            services.AddTransient<TestView>();
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                using var scope = AppHost.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // 确保数据库存在
                dbContext.Database.EnsureCreated();
                
                // 检查并更新数据库结构
                try
                {
                    // 检查Tasks表是否有TaskUrl列
                    bool hasTaskUrl = false;
                    try
                    {
                        // 尝试查询TaskUrl列，如果不存在会抛出异常
                        dbContext.Database.ExecuteSqlRaw("SELECT TaskUrl FROM Tasks LIMIT 1");
                        hasTaskUrl = true;
                    }
                    catch
                    {
                        // TaskUrl列不存在，需要添加
                        System.Diagnostics.Debug.WriteLine("添加TaskUrl列到Tasks表");
                    }
                    
                    // 如果TaskUrl列不存在，添加它
                    if (!hasTaskUrl)
                    {
                        dbContext.Database.ExecuteSqlRaw("ALTER TABLE Tasks ADD COLUMN TaskUrl TEXT");
                    }
                    
                    // 尝试查询数据，如果表不存在会抛出异常
                    var settings = dbContext.UserSettings.FirstOrDefault();
                }
                catch (Exception ex) when (ex.Message.Contains("no such table"))
                {
                    // 如果表不存在，手动创建表
                    System.Diagnostics.Debug.WriteLine($"创建数据库表: {ex.Message}");
                    dbContext.Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS UserSettings (
                        Id INTEGER PRIMARY KEY,
                        Username TEXT,
                        EncryptedPassword TEXT,
                        JiraServerUrl TEXT,
                        RememberPassword INTEGER,
                        AutoLogin INTEGER,
                        WindowWidth REAL,
                        WindowHeight REAL,
                        RefreshInterval INTEGER,
                        MinimizeToTrayOnStart INTEGER,
                        MinimizeToTrayOnClose INTEGER,
                        ShowTaskNameColumn INTEGER,
                        ShowTaskNumberColumn INTEGER,
                        ShowParentTaskNumberColumn INTEGER,
                        ShowTaskStatusColumn INTEGER,
                        ShowParentTaskStatusColumn INTEGER,
                        ShowEstimatedHoursColumn INTEGER,
                        ShowEstimatedCompletionTimeColumn INTEGER,
                        ShowActualCompletionTimeColumn INTEGER,
                        ShowCreatedAtColumn INTEGER,
                        ShowUpdatedAtColumn INTEGER,
                        ShowPriorityColumn INTEGER,
                        ShowBanCheNameColumn INTEGER,
                        ShowCodeMergedColumn INTEGER,
                        ShowSqlColumn INTEGER,
                        ShowConfigurationColumn INTEGER
                    );
                    
                    CREATE TABLE IF NOT EXISTS Tasks (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        TaskNumber TEXT NOT NULL,
                        Description TEXT,
                        Status TEXT,
                        BanCheName TEXT,
                        PreviousBanCheName TEXT,
                        TaskUrl TEXT,
                        IsBanCheNameChangeNotified INTEGER DEFAULT 0,
                        IsDescriptionChangeNotified INTEGER DEFAULT 0
                    );
                    
                    CREATE TABLE IF NOT EXISTS SubTasks (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        TaskNumber TEXT NOT NULL,
                        TaskName TEXT,
                        Status TEXT,
                        Priority TEXT,
                        EstimatedHours REAL,
                        EstimatedCompletionTime TEXT,
                        ActualCompletionTime TEXT,
                        CreatedAt TEXT,
                        UpdatedAt TEXT,
                        ParentTaskId INTEGER,
                        IsCodeMerged INTEGER DEFAULT 0,
                        HasSqlScript INTEGER DEFAULT 0,
                        SqlScript TEXT,
                        HasConfiguration INTEGER DEFAULT 0,
                        Configuration TEXT,
                        FOREIGN KEY(ParentTaskId) REFERENCES Tasks(Id)
                    );
                    ");
                }
                
                // 初始化默认用户设置
                if (!dbContext.UserSettings.Any())
                {
                    dbContext.UserSettings.Add(new Models.UserSettings
                    {
                        Id = 1,
                        WindowWidth = 1280,
                        WindowHeight = 720,
                        RefreshInterval = 1, // 1分钟
                        MinimizeToTrayOnClose = true,
                        ShowTaskNameColumn = true,
                        ShowTaskNumberColumn = true,
                        ShowParentTaskNumberColumn = true,
                        ShowTaskStatusColumn = true,
                        ShowParentTaskStatusColumn = true,
                        ShowEstimatedHoursColumn = true,
                        ShowCreatedAtColumn = true,
                        ShowUpdatedAtColumn = true,
                        ShowPriorityColumn = true,
                        ShowBanCheNameColumn = true,
                        ShowCodeMergedColumn = true,
                        ShowSqlColumn = true,
                        ShowConfigurationColumn = true
                    });
                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化数据库失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 应用程序退出事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override async void OnExit(ExitEventArgs e)
        {
            // 关闭所有服务
            if (AppHost != null)
            {
                await AppHost.StopAsync();
                AppHost.Dispose();
            }

            base.OnExit(e);
        }
    }
}
