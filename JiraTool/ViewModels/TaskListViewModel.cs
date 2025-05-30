using JiraTool.Models;
using JiraTool.Services;
using JiraTool.Services.Interfaces;
using JiraTool.Utils;
using JiraTool.ViewModels.Base;
using JiraTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using JiraTool.Data;
using Microsoft.EntityFrameworkCore;

namespace JiraTool.ViewModels
{
    /// <summary>
    /// 任务列表视图模型
    /// </summary>
    public class TaskListViewModel : ViewModelBase, ILoginAwareViewModel
    {
        private readonly IJiraService _jiraService;
        private readonly INotificationService _notificationService;
        private readonly DatabaseService _databaseService;
        private readonly ITrayIconService _trayIconService;

        private ObservableCollection<JiraSubTask> _subTasks = new();
        private JiraSubTask? _selectedTask;
        private bool _isRefreshing = false;
        private string _searchText = string.Empty;
        private string _statusMessage = string.Empty;
        private Timer? _refreshTimer;
        private int _refreshInterval = 60000; // 默认1分钟
        private bool _isBusy = false;
        private DateTime _lastUpdateTime = DateTime.Now;
        private bool _isLoggedIn = false;

        // 列显示设置
        private bool _showTaskNameColumn = true;
        private bool _showTaskNumberColumn = true;
        private bool _showParentTaskNumberColumn = true;
        private bool _showTaskStatusColumn = true;
        private bool _showParentTaskStatusColumn = true;
        private bool _showEstimatedHoursColumn = true;
        private bool _showEstimatedCompletionTimeColumn = false;
        private bool _showActualCompletionTimeColumn = false;
        private bool _showCreatedAtColumn = true;
        private bool _showUpdatedAtColumn = true;
        private bool _showPriorityColumn = true;
        private bool _showBanCheNameColumn = true;
        private bool _showCodeMergedColumn = true;
        private bool _showSqlColumn = true;
        private bool _showConfigurationColumn = true;

        #region 属性

        /// <summary>
        /// 子任务列表
        /// </summary>
        public ObservableCollection<JiraSubTask> SubTasks
        {
            get => _subTasks;
            set => SetProperty(ref _subTasks, value);
        }

        /// <summary>
        /// 选中的任务
        /// </summary>
        public JiraSubTask? SelectedTask
        {
            get => _selectedTask;
            set => SetProperty(ref _selectedTask, value);
        }

        /// <summary>
        /// 是否正在刷新
        /// </summary>
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        /// <summary>
        /// 搜索文本
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value, FilterTasks);
        }

        /// <summary>
        /// 状态消息
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdateTime
        {
            get => _lastUpdateTime;
            set => SetProperty(ref _lastUpdateTime, value);
        }

        /// <summary>
        /// 是否忙碌
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        /// <summary>
        /// 是否已登录
        /// </summary>
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }

        #endregion

        #region 列显示属性

        /// <summary>
        /// 是否显示子任务名称列
        /// </summary>
        public bool ShowTaskNameColumn
        {
            get => _showTaskNameColumn;
            set => SetProperty(ref _showTaskNameColumn, value);
        }

        /// <summary>
        /// 是否显示子任务单号列
        /// </summary>
        public bool ShowTaskNumberColumn
        {
            get => _showTaskNumberColumn;
            set => SetProperty(ref _showTaskNumberColumn, value);
        }

        /// <summary>
        /// 是否显示父任务单号列
        /// </summary>
        public bool ShowParentTaskNumberColumn
        {
            get => _showParentTaskNumberColumn;
            set => SetProperty(ref _showParentTaskNumberColumn, value);
        }

        /// <summary>
        /// 是否显示子任务状态列
        /// </summary>
        public bool ShowTaskStatusColumn
        {
            get => _showTaskStatusColumn;
            set => SetProperty(ref _showTaskStatusColumn, value);
        }

        /// <summary>
        /// 是否显示父任务状态列
        /// </summary>
        public bool ShowParentTaskStatusColumn
        {
            get => _showParentTaskStatusColumn;
            set => SetProperty(ref _showParentTaskStatusColumn, value);
        }

        /// <summary>
        /// 是否显示预计工时列
        /// </summary>
        public bool ShowEstimatedHoursColumn
        {
            get => _showEstimatedHoursColumn;
            set => SetProperty(ref _showEstimatedHoursColumn, value);
        }

        /// <summary>
        /// 是否显示预计完成时间列
        /// </summary>
        public bool ShowEstimatedCompletionTimeColumn
        {
            get => _showEstimatedCompletionTimeColumn;
            set => SetProperty(ref _showEstimatedCompletionTimeColumn, value);
        }

        /// <summary>
        /// 是否显示实际完成时间列
        /// </summary>
        public bool ShowActualCompletionTimeColumn
        {
            get => _showActualCompletionTimeColumn;
            set => SetProperty(ref _showActualCompletionTimeColumn, value);
        }

        /// <summary>
        /// 是否显示创建时间列
        /// </summary>
        public bool ShowCreatedAtColumn
        {
            get => _showCreatedAtColumn;
            set => SetProperty(ref _showCreatedAtColumn, value);
        }

        /// <summary>
        /// 是否显示更新时间列
        /// </summary>
        public bool ShowUpdatedAtColumn
        {
            get => _showUpdatedAtColumn;
            set => SetProperty(ref _showUpdatedAtColumn, value);
        }

        /// <summary>
        /// 是否显示优先级列
        /// </summary>
        public bool ShowPriorityColumn
        {
            get => _showPriorityColumn;
            set => SetProperty(ref _showPriorityColumn, value);
        }

        /// <summary>
        /// 是否显示班车名列
        /// </summary>
        public bool ShowBanCheNameColumn
        {
            get => _showBanCheNameColumn;
            set => SetProperty(ref _showBanCheNameColumn, value);
        }

        /// <summary>
        /// 是否显示代码已合并列
        /// </summary>
        public bool ShowCodeMergedColumn
        {
            get => _showCodeMergedColumn;
            set => SetProperty(ref _showCodeMergedColumn, value);
        }

        /// <summary>
        /// 是否显示SQL列
        /// </summary>
        public bool ShowSqlColumn
        {
            get => _showSqlColumn;
            set => SetProperty(ref _showSqlColumn, value);
        }

        /// <summary>
        /// 是否显示配置列
        /// </summary>
        public bool ShowConfigurationColumn
        {
            get => _showConfigurationColumn;
            set => SetProperty(ref _showConfigurationColumn, value);
        }

        #endregion

        #region 命令

        /// <summary>
        /// 加载命令
        /// </summary>
        public ICommand LoadedCommand { get; }

        /// <summary>
        /// 刷新命令
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// 显示列设置命令
        /// </summary>
        public ICommand ShowColumnSettingsCommand { get; }

        /// <summary>
        /// 打开子任务命令
        /// </summary>
        public ICommand OpenTaskCommand { get; }

        /// <summary>
        /// 打开父任务命令
        /// </summary>
        public ICommand OpenParentTaskCommand { get; }

        /// <summary>
        /// 添加SQL命令
        /// </summary>
        public ICommand AddSqlCommand { get; }

        /// <summary>
        /// 添加配置命令
        /// </summary>
        public ICommand AddConfigCommand { get; }

        /// <summary>
        /// 开始开发命令
        /// </summary>
        public ICommand StartDevelopmentCommand { get; }

        /// <summary>
        /// 完成开发命令
        /// </summary>
        public ICommand CompleteDevelopmentCommand { get; }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="jiraService">Jira服务</param>
        /// <param name="notificationService">通知服务</param>
        /// <param name="databaseService">数据库服务</param>
        /// <param name="trayIconService">托盘图标服务</param>
        public TaskListViewModel(IJiraService jiraService, INotificationService notificationService, DatabaseService databaseService, ITrayIconService trayIconService)
        {
            _jiraService = jiraService;
            _notificationService = notificationService;
            _databaseService = databaseService;
            _trayIconService = trayIconService;

            // 初始化时就加载数据库中的任务数据
            _ = LoadTasksFromDatabaseAsync(); // 使用废弃操作符表示我们不等待这个异步操作

            LoadedCommand = new RelayCommand(async _ => await LoadTasksFromDatabaseAsync());
            RefreshCommand = new RelayCommand(async _ => await FetchTasksFromWeb(), _ => IsLoggedIn); // 只有在登录状态下才能刷新
            ShowColumnSettingsCommand = new RelayCommand(_ => ShowColumnSettings());
            OpenTaskCommand = new RelayCommand(OpenTask, _ => SelectedTask != null);
            OpenParentTaskCommand = new RelayCommand(OpenParentTask, _ => SelectedTask?.ParentTask != null);
            AddSqlCommand = new RelayCommand(AddSql, _ => SelectedTask != null);
            AddConfigCommand = new RelayCommand(AddConfig, _ => SelectedTask != null);
            StartDevelopmentCommand = new RelayCommand(StartDevelopment, _ => SelectedTask != null);
            CompleteDevelopmentCommand = new RelayCommand(CompleteDevelopment, _ => SelectedTask != null);

            // 加载列设置
            LoadColumnSettings();
            
            // 启动定时刷新
            StartRefreshTimer();
        }

        /// <summary>
        /// 从数据库加载任务
        /// </summary>
        private async Task LoadTasksFromDatabaseAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "正在从数据库加载任务...";

                // 从数据库加载任务
                var tasks = await _databaseService.GetAllTasksAsync();
                SubTasks = new ObservableCollection<JiraSubTask>(tasks);

                LastUpdateTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                StatusMessage = "加载任务失败";
                _notificationService.ShowError("加载失败", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// 从Web获取任务
        /// </summary>
        public async Task FetchTasksFromWeb()
        {
            if (IsRefreshing)
            {
                return;
            }
            
            // 如果未登录，显示提示并返回
            if (!IsLoggedIn)
            {
                _notificationService.ShowInformation("未登录", "请先登录再刷新数据");
                return;
            }

            try
            {
                IsRefreshing = true;
                IsBusy = true;
                StatusMessage = "正在从Web获取任务...";

                // 从Web获取任务
                var webTasks = await _jiraService.GetTasksAsync();
                if (webTasks == null || !webTasks.Any())
                {
                    StatusMessage = "未获取到任务";
                    return;
                }

                try
                {
                    // 采用最简单的方法：直接使用原始SQL清空并重建数据
                    using (var scope = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeAsyncFlowOption.Enabled))
                    {
                        // 1. 清空数据库
                        using (var context = new JiraTool.Data.ApplicationDbContext(new DbContextOptionsBuilder<JiraTool.Data.ApplicationDbContext>().UseSqlite("Data Source=jira.db").Options))
                        {
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM SubTasks");
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM Tasks");
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM sqlite_sequence WHERE name='Tasks' OR name='SubTasks'");
                        }

                        // 2. 创建父任务字典
                        var parentTasks = new Dictionary<string, JiraTask>();
                        foreach (var webTask in webTasks.Where(t => t.ParentTask != null))
                        {
                            if (!parentTasks.ContainsKey(webTask.ParentTask.TaskNumber))
                            {
                                parentTasks[webTask.ParentTask.TaskNumber] = new JiraTask
                                {
                                    TaskNumber = webTask.ParentTask.TaskNumber,
                                    Description = webTask.ParentTask.Description,
                                    Status = webTask.ParentTask.Status,
                                    BanCheName = webTask.ParentTask.BanCheName,
                                    PreviousBanCheName = webTask.ParentTask.PreviousBanCheName,
                                    TaskUrl = webTask.ParentTask.TaskUrl,
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                };
                            }
                        }

                        // 3. 保存父任务
                        using (var context = new JiraTool.Data.ApplicationDbContext(new DbContextOptionsBuilder<JiraTool.Data.ApplicationDbContext>().UseSqlite("Data Source=jira.db").Options))
                        {
                            await context.Tasks.AddRangeAsync(parentTasks.Values);
                            await context.SaveChangesAsync();
                        }

                        // 4. 获取已保存的父任务（带ID）
                        Dictionary<string, JiraTask> savedParentTasks;
                        using (var context = new JiraTool.Data.ApplicationDbContext(new DbContextOptionsBuilder<JiraTool.Data.ApplicationDbContext>().UseSqlite("Data Source=jira.db").Options))
                        {
                            savedParentTasks = await context.Tasks
                                .AsNoTracking()
                                .ToDictionaryAsync(t => t.TaskNumber, t => t);
                        }

                        // 5. 创建子任务列表
                        var subTasks = new List<JiraSubTask>();
                        foreach (var webTask in webTasks)
                        {
                            var subTask = new JiraSubTask
                            {
                                TaskNumber = webTask.TaskNumber,
                                TaskName = webTask.TaskName,
                                Status = webTask.Status,
                                Priority = webTask.Priority,
                                EstimatedHours = webTask.EstimatedHours,
                                EstimatedCompletionTime = webTask.EstimatedCompletionTime,
                                ActualCompletionTime = webTask.ActualCompletionTime,
                                CreatedAt = webTask.CreatedAt,
                                UpdatedAt = DateTime.Now,
                                IsCodeMerged = webTask.IsCodeMerged,
                                HasSqlScript = webTask.HasSqlScript,
                                SqlScript = webTask.SqlScript,
                                HasConfiguration = webTask.HasConfiguration,
                                Configuration = webTask.Configuration,
                                TaskUrl = webTask.TaskUrl
                            };

                            // 设置父任务ID
                            if (webTask.ParentTask != null && savedParentTasks.TryGetValue(webTask.ParentTask.TaskNumber, out var savedParent))
                            {
                                subTask.ParentTaskId = savedParent.Id;
                            }

                            subTasks.Add(subTask);
                        }

                        // 6. 保存子任务
                        using (var context = new JiraTool.Data.ApplicationDbContext(new DbContextOptionsBuilder<JiraTool.Data.ApplicationDbContext>().UseSqlite("Data Source=jira.db").Options))
                        {
                            await context.SubTasks.AddRangeAsync(subTasks);
                            await context.SaveChangesAsync();
                        }

                        // 提交事务
                        scope.Complete();
                    }

                    // 7. 重新加载任务列表
                    await LoadTasksFromDatabaseAsync();

                    LastUpdateTime = DateTime.Now;
                    StatusMessage = $"已从Web获取并重建 {webTasks.Count} 条任务数据";
                    _notificationService.ShowSuccess("刷新成功", $"已从Web获取并重建 {webTasks.Count} 条任务数据");

                    // 更新托盘图标提示
                    _trayIconService.UpdateToolTip($"Jira任务工具 - {webTasks.Count} 条任务");
                }
                catch (Exception dbEx)
                {
                    StatusMessage = "数据库操作失败";
                    _notificationService.ShowError("数据库操作失败", dbEx.Message);
                    System.Diagnostics.Debug.WriteLine($"数据库操作失败: {dbEx.Message}");
                    if (dbEx.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"内部异常: {dbEx.InnerException.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "获取任务失败";
                _notificationService.ShowError("刷新失败", ex.Message);
                System.Diagnostics.Debug.WriteLine($"获取任务失败: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"内部异常: {ex.InnerException.Message}");
                }
            }
            finally
            {
                IsRefreshing = false;
                IsBusy = false;
            }
        }

        /// <summary>
        /// 启动定时刷新
        /// </summary>
        private void StartRefreshTimer()
        {
            // 如果未登录，不启动定时器
            if (!IsLoggedIn)
            {
                return;
            }
            
            // 停止现有的定时器
            _refreshTimer?.Dispose();
            
            // 创建新的定时器，每分钟抽取一次数据
            _refreshTimer = new Timer(async _ =>
            {
                // 在UI线程上执行
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    try
                    {
                        if (!IsRefreshing) // 防止重复抽取
                        {
                            await FetchTasksFromWeb();
                        }
                    }
                    catch (Exception ex)
                    {
                        _notificationService.ShowError("定时抽取数据失败", ex.Message);
                    }
                });
            }, null, _refreshInterval, _refreshInterval);
        }

        /// <summary>
        /// 过滤任务
        /// </summary>
        private void FilterTasks()
        {
            // 实现任务过滤逻辑
        }

        /// <summary>
        /// 显示列设置
        /// </summary>
        private void ShowColumnSettings()
        {
            try
            {
                // 创建列设置对话框
                var dialog = new ColumnSettingsDialog
                {
                    // 设置当前列的可见性
                    ShowTaskNameColumn = ShowTaskNameColumn,
                    ShowTaskNumberColumn = ShowTaskNumberColumn,
                    ShowParentTaskNumberColumn = ShowParentTaskNumberColumn,
                    ShowTaskStatusColumn = ShowTaskStatusColumn,
                    ShowParentTaskStatusColumn = ShowParentTaskStatusColumn,
                    ShowEstimatedHoursColumn = ShowEstimatedHoursColumn,
                    ShowEstimatedCompletionTimeColumn = ShowEstimatedCompletionTimeColumn,
                    ShowActualCompletionTimeColumn = ShowActualCompletionTimeColumn,
                    ShowCreatedAtColumn = ShowCreatedAtColumn,
                    ShowUpdatedAtColumn = ShowUpdatedAtColumn,
                    ShowPriorityColumn = ShowPriorityColumn,
                    ShowBanCheNameColumn = ShowBanCheNameColumn,
                    ShowCodeMergedColumn = ShowCodeMergedColumn,
                    ShowSqlColumn = ShowSqlColumn,
                    ShowConfigurationColumn = ShowConfigurationColumn
                };
                
                // 显示对话框
                var result = dialog.ShowDialog();
                
                // 如果用户点击了"确定"
                if (result == true)
                {
                    // 先将所有列设置为false，然后再设置为用户选择的值
                    // 这样可以确保触发属性变更通知
                    ShowTaskNameColumn = false;
                    ShowTaskNumberColumn = false;
                    ShowParentTaskNumberColumn = false;
                    ShowTaskStatusColumn = false;
                    ShowParentTaskStatusColumn = false;
                    ShowEstimatedHoursColumn = false;
                    ShowEstimatedCompletionTimeColumn = false;
                    ShowActualCompletionTimeColumn = false;
                    ShowCreatedAtColumn = false;
                    ShowUpdatedAtColumn = false;
                    ShowPriorityColumn = false;
                    ShowBanCheNameColumn = false;
                    ShowCodeMergedColumn = false;
                    ShowSqlColumn = false;
                    ShowConfigurationColumn = false;
                    
                    // 设置用户选择的值
                    ShowTaskNameColumn = dialog.ShowTaskNameColumn;
                    ShowTaskNumberColumn = dialog.ShowTaskNumberColumn;
                    ShowParentTaskNumberColumn = dialog.ShowParentTaskNumberColumn;
                    ShowTaskStatusColumn = dialog.ShowTaskStatusColumn;
                    ShowParentTaskStatusColumn = dialog.ShowParentTaskStatusColumn;
                    ShowEstimatedHoursColumn = dialog.ShowEstimatedHoursColumn;
                    ShowEstimatedCompletionTimeColumn = dialog.ShowEstimatedCompletionTimeColumn;
                    ShowActualCompletionTimeColumn = dialog.ShowActualCompletionTimeColumn;
                    ShowCreatedAtColumn = dialog.ShowCreatedAtColumn;
                    ShowUpdatedAtColumn = dialog.ShowUpdatedAtColumn;
                    ShowPriorityColumn = dialog.ShowPriorityColumn;
                    ShowBanCheNameColumn = dialog.ShowBanCheNameColumn;
                    ShowCodeMergedColumn = dialog.ShowCodeMergedColumn;
                    ShowSqlColumn = dialog.ShowSqlColumn;
                    ShowConfigurationColumn = dialog.ShowConfigurationColumn;
                    
                    // 保存设置到数据库
                    SaveColumnSettings();
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("打开列设置对话框失败", ex.Message);
            }
        }
        
        /// <summary>
        /// 保存列设置到数据库
        /// </summary>
        private async void SaveColumnSettings()
        {
            try
            {
                var settings = await _databaseService.GetUserSettingsAsync();
                
                // 更新列设置
                settings.ShowTaskNameColumn = ShowTaskNameColumn;
                settings.ShowTaskNumberColumn = ShowTaskNumberColumn;
                settings.ShowParentTaskNumberColumn = ShowParentTaskNumberColumn;
                settings.ShowTaskStatusColumn = ShowTaskStatusColumn;
                settings.ShowParentTaskStatusColumn = ShowParentTaskStatusColumn;
                settings.ShowEstimatedHoursColumn = ShowEstimatedHoursColumn;
                settings.ShowEstimatedCompletionTimeColumn = ShowEstimatedCompletionTimeColumn;
                settings.ShowActualCompletionTimeColumn = ShowActualCompletionTimeColumn;
                settings.ShowCreatedAtColumn = ShowCreatedAtColumn;
                settings.ShowUpdatedAtColumn = ShowUpdatedAtColumn;
                settings.ShowPriorityColumn = ShowPriorityColumn;
                settings.ShowBanCheNameColumn = ShowBanCheNameColumn;
                settings.ShowCodeMergedColumn = ShowCodeMergedColumn;
                settings.ShowSqlColumn = ShowSqlColumn;
                settings.ShowConfigurationColumn = ShowConfigurationColumn;
                
                // 保存设置
                await _databaseService.SaveUserSettingsAsync(settings);
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("保存列设置失败", ex.Message);
            }
        }
        
        /// <summary>
        /// 从数据库加载列设置
        /// </summary>
        private async void LoadColumnSettings()
        {
            try
            {
                // 从数据库加载用户设置
                var settings = await _databaseService.GetUserSettingsAsync();
                
                // 应用列设置
                ShowTaskNameColumn = settings.ShowTaskNameColumn;
                ShowTaskNumberColumn = settings.ShowTaskNumberColumn;
                ShowParentTaskNumberColumn = settings.ShowParentTaskNumberColumn;
                ShowTaskStatusColumn = settings.ShowTaskStatusColumn;
                ShowParentTaskStatusColumn = settings.ShowParentTaskStatusColumn;
                ShowEstimatedHoursColumn = settings.ShowEstimatedHoursColumn;
                ShowEstimatedCompletionTimeColumn = settings.ShowEstimatedCompletionTimeColumn;
                ShowActualCompletionTimeColumn = settings.ShowActualCompletionTimeColumn;
                ShowCreatedAtColumn = settings.ShowCreatedAtColumn;
                ShowUpdatedAtColumn = settings.ShowUpdatedAtColumn;
                ShowPriorityColumn = settings.ShowPriorityColumn;
                ShowBanCheNameColumn = settings.ShowBanCheNameColumn;
                ShowCodeMergedColumn = settings.ShowCodeMergedColumn;
                ShowSqlColumn = settings.ShowSqlColumn;
                ShowConfigurationColumn = settings.ShowConfigurationColumn;
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("加载列设置失败", ex.Message);
            }
        }

        /// <summary>
        /// 打开子任务
        /// </summary>
        private void OpenTask(object parameter)
        {
            if (SelectedTask == null || string.IsNullOrEmpty(SelectedTask.TaskUrl))
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = SelectedTask.TaskUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("打开任务失败", ex.Message);
            }
        }

        /// <summary>
        /// 打开父任务
        /// </summary>
        private void OpenParentTask(object parameter)
        {
            if (SelectedTask?.ParentTask == null || string.IsNullOrEmpty(SelectedTask.ParentTask.TaskUrl))
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = SelectedTask.ParentTask.TaskUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("打开父任务失败", ex.Message);
            }
        }

        /// <summary>
        /// 添加SQL
        /// </summary>
        private void AddSql(object parameter)
        {
            if (SelectedTask == null)
            {
                return;
            }

            // 实现添加SQL逻辑
        }

        /// <summary>
        /// 添加配置
        /// </summary>
        private void AddConfig(object parameter)
        {
            if (SelectedTask == null)
            {
                return;
            }

            // 实现添加配置逻辑
        }

        /// <summary>
        /// 开始开发
        /// </summary>
        private void StartDevelopment(object parameter)
        {
            if (SelectedTask == null)
            {
                return;
            }

            // 实现开始开发逻辑
        }

        /// <summary>
        /// 完成开发
        /// </summary>
        private void CompleteDevelopment(object parameter)
        {
            if (SelectedTask == null)
            {
                return;
            }

            // 实现完成开发逻辑
        }
    }
}
