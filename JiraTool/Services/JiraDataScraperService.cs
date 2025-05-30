using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using JiraTool.Models;
using JiraTool.Services.Interfaces;
using JiraTool.Utils;
using Microsoft.Extensions.Logging;
using System.Windows;
using Newtonsoft.Json;

namespace JiraTool.Services
{
    /// <summary>
    /// Jira数据抓取服务接口
    /// </summary>
    public interface IJiraDataScraperService
    {
        /// <summary>
        /// 启动定时抓取任务
        /// </summary>
        /// <param name="parentTasksUrl">主单列表URL</param>
        /// <param name="subTasksUrl">子单列表URL</param>
        /// <param name="intervalMinutes">抓取间隔（分钟）</param>
        Task StartScrapingTask(string parentTasksUrl, string subTasksUrl, int intervalMinutes);
        
        /// <summary>
        /// 停止定时抓取任务
        /// </summary>
        void StopScrapingTask();
        
        /// <summary>
        /// 手动触发一次抓取
        /// </summary>
        Task ScrapeDataManually();
        
        /// <summary>
        /// 获取上次抓取时间
        /// </summary>
        DateTime GetLastScrapeTime();
        
        /// <summary>
        /// 是否正在抓取
        /// </summary>
        bool IsScrapingInProgress { get; }
    }
    
    /// <summary>
    /// Jira数据抓取服务实现
    /// </summary>
    public class JiraDataScraperService : IJiraDataScraperService
    {
        private readonly ILogger<JiraDataScraperService> _logger;
        private readonly IDatabaseService _databaseService;
        private readonly IJiraLoginService _jiraLoginService;
        private readonly INotificationService _notificationService;
        private readonly AppConfigService _configService;
        
        private Timer _scrapingTimer;
        private string _parentTasksUrl = string.Empty;
        private string _subTasksUrl = string.Empty;
        private DateTime _lastScrapeTime = DateTime.MinValue;
        private bool _isScrapingInProgress = false;
        private CancellationTokenSource _cancellationTokenSource;
        
        /// <summary>
        /// 是否正在抓取
        /// </summary>
        public bool IsScrapingInProgress => _isScrapingInProgress;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public JiraDataScraperService(
            ILogger<JiraDataScraperService> logger,
            IDatabaseService databaseService,
            IJiraLoginService jiraLoginService,
            INotificationService notificationService,
            AppConfigService configService)
        {
            _logger = logger;
            _databaseService = databaseService;
            _jiraLoginService = jiraLoginService;
            _notificationService = notificationService;
            _configService = configService;
            _cancellationTokenSource = new CancellationTokenSource();
            
            // 从配置中加载URL
            var config = _configService.GetConfig();
            if (!string.IsNullOrEmpty(config.MainTaskListUrl))
            {
                _parentTasksUrl = config.MainTaskListUrl;
            }
            if (!string.IsNullOrEmpty(config.SubTaskListUrl))
            {
                _subTasksUrl = config.SubTaskListUrl;
            }
        }
        
        /// <summary>
        /// 启动定时抓取任务
        /// </summary>
        /// <param name="parentTasksUrl">主单列表URL</param>
        /// <param name="subTasksUrl">子单列表URL</param>
        /// <param name="intervalMinutes">抓取间隔（分钟）</param>
        public async Task StartScrapingTask(string parentTasksUrl, string subTasksUrl, int intervalMinutes)
        {
            try
            {
                // 检查参数
                if (string.IsNullOrEmpty(parentTasksUrl) || string.IsNullOrEmpty(subTasksUrl))
                {
                    _notificationService.ShowError("启动抓取任务失败", "请提供有效的主单和子单列表URL");
                    return;
                }
                
                // 保存URL
                _parentTasksUrl = parentTasksUrl;
                _subTasksUrl = subTasksUrl;
                
                // 保存到配置
                var config = _configService.GetConfig();
                config.MainTaskListUrl = parentTasksUrl;
                config.SubTaskListUrl = subTasksUrl;
                config.RefreshInterval = intervalMinutes;
                await _configService.SaveConfigAsync(config);
                
                // 停止现有定时器
                _scrapingTimer?.Dispose();
                
                // 取消现有任务
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                
                // 立即执行一次抓取
                await ScrapeDataAsync();
                
                // 设置定时器
                int intervalMs = intervalMinutes * 60 * 1000;
                _scrapingTimer = new Timer(async _ => await ScrapeDataAsync(), null, intervalMs, intervalMs);
                
                _notificationService.ShowInformation("定时抓取任务已启动", $"将每{intervalMinutes}分钟自动抓取一次数据");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动抓取任务失败");
                _notificationService.ShowError("启动抓取任务失败", ex.Message);
            }
        }
        
        /// <summary>
        /// 停止定时抓取任务
        /// </summary>
        public void StopScrapingTask()
        {
            try
            {
                // 停止定时器
                _scrapingTimer?.Dispose();
                _scrapingTimer = null;
                
                // 取消正在进行的抓取
                _cancellationTokenSource?.Cancel();
                
                _notificationService.ShowInformation("抓取任务已停止", "定时抓取任务已停止");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止抓取任务失败");
                _notificationService.ShowError("停止抓取任务失败", ex.Message);
            }
        }
        
        /// <summary>
        /// 手动触发一次抓取
        /// </summary>
        public async Task ScrapeDataManually()
        {
            await ScrapeDataAsync();
        }
        
        /// <summary>
        /// 获取上次抓取时间
        /// </summary>
        public DateTime GetLastScrapeTime()
        {
            return _lastScrapeTime;
        }
        
        /// <summary>
        /// 执行数据抓取
        /// </summary>
        private async Task ScrapeDataAsync()
        {
            if (_isScrapingInProgress)
            {
                _logger.LogWarning("已有抓取任务正在进行中，跳过本次抓取");
                return;
            }
            
            try
            {
                _isScrapingInProgress = true;
                
                // 检查是否已登录
                var isLoggedIn = await _jiraLoginService.IsLoggedIn();
                if (!isLoggedIn)
                {
                    _logger.LogWarning("未登录Jira，无法抓取数据");
                    return;
                }
                
                // 抓取主单数据
                var parentTasks = await ScrapeParentTasksAsync(_parentTasksUrl);
                
                // 抓取子单数据
                var subTasks = await ScrapeSubTasksAsync(_subTasksUrl, parentTasks);
                
                // 验证并保存数据
                await ValidateAndSaveSubTasksAsync(subTasks);
                
                // 更新最后抓取时间
                _lastScrapeTime = DateTime.Now;
                
                // 通知用户抓取完成
                _notificationService.ShowInformation("数据抓取完成", $"已成功抓取 {parentTasks.Count} 个主单和 {subTasks.Count} 个子单");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "抓取数据失败");
                _notificationService.ShowError("抓取数据失败", ex.Message);
            }
            finally
            {
                _isScrapingInProgress = false;
            }
        }
        
        /// <summary>
        /// 抓取主单数据
        /// </summary>
        private async Task<List<JiraTask>> ScrapeParentTasksAsync(string url)
        {
            var parentTasks = new List<JiraTask>();
            
            try
            {
                // 获取cookie
                var cookies = _jiraLoginService.GetCookies();
                using (var scraper = new CefWebScraper(null, cookies))
                {
                    // 导航到主单列表页面
                    await scraper.NavigateToUrlAsync(url);
                    
                    // 等待页面加载完成
                    await scraper.WaitForPageLoadAsync();
                    
                    // 执行JavaScript获取表格数据
                    var response = await scraper.ExecuteScriptAsync(@"
                        function extractTableData() {
                            const rows = document.querySelectorAll('table.aui tbody tr');
                            const data = [];
                            
                            for (let i = 0; i < rows.length; i++) {
                                const row = rows[i];
                                const cells = row.querySelectorAll('td');
                                
                                if (cells.length < 5) continue;
                                
                                const taskNumber = cells[0].textContent.trim();
                                const taskName = cells[1].textContent.trim();
                                const status = cells[2].textContent.trim();
                                const assignee = cells[3].textContent.trim();
                                const banCheName = cells[4].textContent.trim();
                                
                                data.push({
                                    taskNumber: taskNumber,
                                    taskName: taskName,
                                    status: status,
                                    assignee: assignee,
                                    banCheName: banCheName
                                });
                            }
                            
                            return JSON.stringify(data);
                        }
                        
                        return extractTableData();
                    ");
                    
                    if (response.Success && response.Result != null)
                    {
                        // 解析JSON结果
                        var jsonResult = response.Result.ToString();
                        var taskDataList = JsonConvert.DeserializeObject<List<dynamic>>(jsonResult);
                        
                        if (taskDataList != null)
                        {
                            foreach (var taskData in taskDataList)
                            {
                                var task = new JiraTask
                                {
                                    TaskNumber = taskData.taskNumber,
                                    Description = taskData.taskName.ToString(),
                                    Status = taskData.status,
                                    BanCheName = taskData.banCheName,
                                    UpdatedAt = DateTime.Now,
                                    CreatedAt = DateTime.Now
                                };
                                
                                parentTasks.Add(task);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "抓取主单数据失败");
                throw;
            }
            
            return parentTasks;
        }
        
        /// <summary>
        /// 抓取子单数据
        /// </summary>
        private async Task<List<JiraSubTask>> ScrapeSubTasksAsync(string url, List<JiraTask> parentTasks)
        {
            var subTasks = new List<JiraSubTask>();
            
            try
            {
                // 获取cookie
                var cookies = _jiraLoginService.GetCookies();
                using (var scraper = new CefWebScraper(null, cookies))
                {
                    // 导航到子单列表页面
                    await scraper.NavigateToUrlAsync(url);
                    
                    // 等待页面加载完成
                    await scraper.WaitForPageLoadAsync();
                    
                    // 执行JavaScript获取表格数据
                    var response = await scraper.ExecuteScriptAsync(@"
                        function extractTableData() {
                            const rows = document.querySelectorAll('table.aui tbody tr');
                            const data = [];
                            
                            for (let i = 0; i < rows.length; i++) {
                                const row = rows[i];
                                const cells = row.querySelectorAll('td');
                                
                                if (cells.length < 8) continue;
                                
                                const taskNumber = cells[0].textContent.trim();
                                const taskName = cells[1].textContent.trim();
                                const status = cells[2].textContent.trim();
                                const assignee = cells[3].textContent.trim();
                                const parentTaskNumber = cells[4].textContent.trim();
                                const estimateHours = cells[5].textContent.trim();
                                const spentHours = cells[6].textContent.trim();
                                const dueDate = cells[7].textContent.trim();
                                
                                data.push({
                                    taskNumber: taskNumber,
                                    taskName: taskName,
                                    status: status,
                                    assignee: assignee,
                                    parentTaskNumber: parentTaskNumber,
                                    estimateHours: estimateHours,
                                    spentHours: spentHours,
                                    dueDate: dueDate
                                });
                            }
                            
                            return JSON.stringify(data);
                        }
                        
                        return extractTableData();
                    ");
                    
                    if (response.Success && response.Result != null)
                    {
                        // 解析JSON结果
                        var jsonResult = response.Result.ToString();
                        var taskDataList = JsonConvert.DeserializeObject<List<dynamic>>(jsonResult);
                        
                        if (taskDataList != null)
                        {
                            foreach (var taskData in taskDataList)
                            {
                                var subTask = new JiraSubTask
                                {
                                    TaskNumber = taskData.taskNumber,
                                    TaskName = taskData.taskName,
                                    Status = taskData.status,
                                    EstimatedHours = ParseDecimal(taskData.estimateHours),
                                    EstimatedCompletionTime = ParseDateTime(taskData.dueDate),
                                    UpdatedAt = DateTime.Now,
                                    CreatedAt = DateTime.Now
                                };
                                
                                // 关联父任务
                                string parentTaskNumber = taskData.parentTaskNumber;
                                if (!string.IsNullOrEmpty(parentTaskNumber))
                                {
                                    var parentTask = parentTasks.FirstOrDefault(t => t.TaskNumber == parentTaskNumber);
                                    if (parentTask != null)
                                    {
                                        subTask.ParentTaskId = parentTask.Id;
                                        subTask.ParentTask = parentTask;
                                    }
                                }
                                
                                subTasks.Add(subTask);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "抓取子单数据失败");
                throw;
            }
            
            return subTasks;
        }
        
        /// <summary>
        /// 验证子单数据并保存到数据库
        /// </summary>
        private async Task ValidateAndSaveSubTasksAsync(List<JiraSubTask> newSubTasks)
        {
            try
            {
                // 获取现有子单
                var existingSubTasks = await _databaseService.GetAllTasksAsync();
                
                // 用于存储需要通知的变更
                var banCheNameChanges = new List<(JiraSubTask SubTask, string OldBanCheName, string NewBanCheName)>();
                
                foreach (var newSubTask in newSubTasks)
                {
                    // 查找现有子单
                    var existingSubTask = existingSubTasks.FirstOrDefault(t => t.TaskNumber == newSubTask.TaskNumber);
                    
                    if (existingSubTask == null)
                    {
                        // 子单不存在，直接添加
                        continue;
                    }
                    
                    // 检查班车号是否变更
                    if (newSubTask.ParentTask != null && 
                        existingSubTask.ParentTask != null &&
                        newSubTask.ParentTask.BanCheName != existingSubTask.ParentTask.BanCheName)
                    {
                        // 记录班车号变更
                        banCheNameChanges.Add((
                            newSubTask, 
                            existingSubTask.ParentTask.BanCheName, 
                            newSubTask.ParentTask.BanCheName
                        ));
                        
                        // 标记班车号已变更
                        newSubTask.IsBanCheNameChanged = true;
                    }
                }
                
                // 保存子单数据到数据库
                await _databaseService.SaveTasksAsync(newSubTasks);
                
                // 显示班车号变更通知
                foreach (var change in banCheNameChanges)
                {
                    // 显示班车号变更通知
                    _notificationService.ShowInformation(
                        "班车号变更通知", 
                        $"任务 {change.SubTask.TaskNumber} 的班车号已从 {change.OldBanCheName} 变更为 {change.NewBanCheName}"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证和保存子单数据失败");
                throw;
            }
        }
        
        /// <summary>
        /// 解析日期时间
        /// </summary>
        private DateTime? ParseDateTime(object dateTimeStr)
        {
            if (dateTimeStr == null)
                return null;
            
            if (DateTime.TryParse(dateTimeStr.ToString(), out var result))
                return result;
            
            return null;
        }
        
        /// <summary>
        /// 解析小数
        /// </summary>
        private double ParseDecimal(object decimalStr)
        {
            if (decimalStr == null)
                return 0;
            
            if (double.TryParse(decimalStr.ToString(), out var result))
                return result;
            
            return 0;
        }
    }
}
