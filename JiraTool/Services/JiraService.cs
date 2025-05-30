using JiraTool.Models;
using JiraTool.Services.Interfaces;
using JiraTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using System.Text;

namespace JiraTool.Services
{
    /// <summary>
    /// Jira服务实现类
    /// </summary>
    public class JiraService : IJiraService
    {
        private readonly INotificationService _notificationService;
        private readonly HttpClient _httpClient;
        private DateTime _lastUpdateTime = DateTime.MinValue;
        private string _username = string.Empty;
        private string _jiraUrl = string.Empty;
        private bool _isLoggedIn = false;
        private string _authCookie = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username => _username;

        /// <summary>
        /// 是否已登录
        /// </summary>
        public bool IsLoggedIn => _isLoggedIn;

        /// <summary>
        /// 最后一次数据获取时间
        /// </summary>
        public DateTime LastUpdateTime => _lastUpdateTime;

        /// <summary>
        /// 任务数据更新事件
        /// </summary>
        public event EventHandler TasksUpdated;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="notificationService">通知服务</param>
        public JiraService(INotificationService notificationService)
        {
            _notificationService = notificationService;
            
            // 初始化HttpClient
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// 登录Jira
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="jiraUrl">Jira服务器地址</param>
        /// <returns>登录是否成功</returns>
        public async Task<bool> LoginAsync(string username, string password, string jiraUrl)
        {
            try
            {
                // 存储Jira URL
                _jiraUrl = jiraUrl.TrimEnd('/');
                
                // 首先获取登录页面，获取必要的Cookie和表单信息
                var loginPageResponse = await _httpClient.GetAsync($"{_jiraUrl}/login");
                loginPageResponse.EnsureSuccessStatusCode();
                
                // 获取登录页面内容
                var loginPageContent = await loginPageResponse.Content.ReadAsStringAsync();
                
                // 提取CSRF令牌
                var csrfToken = ExtractCsrfToken(loginPageContent);
                if (string.IsNullOrEmpty(csrfToken))
                {
                    _notificationService.ShowError("登录失败", "无法获取CSRF令牌");
                    return false;
                }
                
                // 准备登录表单数据
                var loginData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("csrfToken", csrfToken),
                    new KeyValuePair<string, string>("login", "Log in")
                });
                
                // 发送登录请求
                var loginResponse = await _httpClient.PostAsync($"{_jiraUrl}/login", loginData);
                
                // 检查是否登录成功
                if (loginResponse.IsSuccessStatusCode)
                {
                    // 保存认证Cookie
                    var cookies = loginResponse.Headers.GetValues("Set-Cookie");
                    foreach (var cookie in cookies)
                    {
                        if (cookie.Contains("JSESSIONID") || cookie.Contains("atlassian.xsrf.token"))
                        {
                            _authCookie = cookie;
                            break;
                        }
                    }
                    
                    // 检查是否有效登录
                    var dashboardResponse = await _httpClient.GetAsync($"{_jiraUrl}/secure/Dashboard.jspa");
                    if (dashboardResponse.IsSuccessStatusCode)
                    {
                        _isLoggedIn = true;
                        _username = username;
                        return true;
                    }
                }
                
                _notificationService.ShowError("登录失败", "用户名或密码错误");
                return false;
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("登录失败", ex.Message);
                return false;
            }
        }
        
        /// <summary>
        /// 从登录页面提取CSRF令牌
        /// </summary>
        private string ExtractCsrfToken(string html)
        {
            // 使用正则表达式提取CSRF令牌
            var match = Regex.Match(html, "<input type=\"hidden\" name=\"csrfToken\" value=\"([^\"]+)\"");
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }

        /// <summary>
        /// 登出Jira
        /// </summary>
        /// <returns>操作任务</returns>
        public async Task LogoutAsync()
        {
            if (_isLoggedIn)
            {
                try
                {
                    // 模拟登出操作
                    await Task.Delay(500); // 模拟网络延迟
                    
                    _isLoggedIn = false;
                    _username = string.Empty;
                    _authCookie = string.Empty;
                    
                    _notificationService.ShowInformation("登出成功", "您已成功登出Jira系统");
                }
                catch (Exception ex)
                {
                    _notificationService.ShowError("登出失败", ex.Message);
                }
            }
        }

        /// <summary>
        /// 获取用户的任务列表
        /// </summary>
        /// <returns>任务列表</returns>
        public async Task<List<JiraSubTask>> GetUserTasksAsync()
        {
            return await GetTasksAsync();
        }

        /// <summary>
        /// 获取所有任务列表
        /// </summary>
        /// <returns>任务列表</returns>
        public async Task<List<JiraSubTask>> GetTasksAsync()
        {
            // 生成模拟数据，以便应用程序可以运行
            var tasks = new List<JiraSubTask>();
            
            try
            {
                // 创建一个字典来跟踪父任务，避免重复创建相同的父任务对象
                var parentTaskDict = new Dictionary<string, JiraTask>();
                
                // 创建模拟的父任务数据
                var parentTaskData = new[]
                {
                    new { Number = "JIRA-1000", Description = "实现用户登录功能", Status = "进行中", BanCheName = "2025年Q2第一批次" },
                    new { Number = "JIRA-1001", Description = "实现数据导出功能", Status = "待处理", BanCheName = "2025年Q2第二批次" },
                    new { Number = "JIRA-1002", Description = "优化系统性能", Status = "已完成", BanCheName = "2025年Q2第三批次" },
                    new { Number = "JIRA-1003", Description = "添加统计报表功能", Status = "进行中", BanCheName = "2025年Q2第四批次" },
                    new { Number = "JIRA-1004", Description = "实现自动化测试", Status = "待测试", BanCheName = "2025年Q3第一批次" }
                };
                
                // 初始化父任务字典
                foreach (var data in parentTaskData)
                {
                    parentTaskDict[data.Number] = new JiraTask
                    {
                        TaskNumber = data.Number,
                        Description = data.Description,
                        Status = data.Status,
                        BanCheName = data.BanCheName,
                        TaskUrl = $"https://jira.example.com/browse/{data.Number}"
                    };
                }
                
                // 为每个父任务创建子任务
                var random = new Random();
                var statusOptions = new[] { "待处理", "进行中", "待测试", "已完成" };
                var priorityOptions = new[] { "高", "中", "低" };
                
                // 使用当前时间作为随机种子，确保每次生成的数据有一些变化
                var seed = (int)DateTime.Now.Ticks % 10000;
                random = new Random(seed);
                
                // 生成一些新的任务号，模拟新增任务
                var newTaskNumbers = new List<string>();
                for (int i = 0; i < 3; i++)
                {
                    newTaskNumbers.Add($"JIRA-{2000 + seed % 100 + i}");
                }
                
                // 模拟班车名变更
                int banCheChangeIndex = random.Next(parentTaskData.Length);
                var banCheChangeTask = parentTaskDict[parentTaskData[banCheChangeIndex].Number];
                banCheChangeTask.PreviousBanCheName = $"2024年Q{random.Next(1, 4)}第{random.Next(1, 10)}批次";
                
                // 为每个父任务创建子任务
                foreach (var parentTaskNumber in parentTaskDict.Keys)
                {
                    var parentTask = parentTaskDict[parentTaskNumber];
                    
                    // 为每个父任务创建2-4个子任务
                    int subTaskCount = random.Next(2, 5);
                    
                    // 定义状态和优先级
                    string[] statuses = { "待处理", "处理中", "已完成", "已关闭" };
                    string[] priorities = { "低", "中", "高", "紧急" };
                    
                    for (int i = 1; i <= subTaskCount; i++)
                    {
                        var subTask = new JiraSubTask
                        {
                            TaskNumber = $"{parentTaskNumber}-{i}",
                            TaskName = $"{parentTask.Description} - 子任务 {i}",
                            Status = statuses[random.Next(statuses.Length)],
                            Priority = priorities[random.Next(priorities.Length)],
                            EstimatedHours = random.Next(1, 40),
                            CreatedAt = DateTime.Now.AddDays(-random.Next(1, 30)),
                            UpdatedAt = DateTime.Now,
                            TaskUrl = $"https://jira.example.com/browse/{parentTaskNumber}-{i}",
                            ParentTask = parentTask
                        };
                        
                        if (random.Next(4) == 0) // 25%的概率
                        {
                            subTask.HasSqlScript = true;
                            subTask.SqlScript = $"SELECT * FROM users WHERE id = {random.Next(1, 100)};";
                        }
                        
                        if (random.Next(4) == 0) // 25%的概率
                        {
                            subTask.HasConfiguration = true;
                            subTask.Configuration = $@"{{
  ""serverUrl"": ""https://api.example.com"",
  ""timeout"": {random.Next(1000, 60000)},
  ""maxRetries"": {random.Next(1, 10)}
}}";
                        }
                        
                        tasks.Add(subTask);
                    }
                }
                
                // 添加一些新的任务，模拟新增任务
                foreach (var newTaskNumber in newTaskNumbers)
                {
                    var newParentTask = new JiraTask
                    {
                        TaskNumber = newTaskNumber,
                        Description = $"新增任务 {newTaskNumber}",
                        Status = statusOptions[random.Next(statusOptions.Length)],
                        BanCheName = $"2025年Q{random.Next(1, 4)}第{random.Next(1, 10)}批次",
                        TaskUrl = $"https://jira.example.com/browse/{newTaskNumber}"
                    };
                    
                    // 将新父任务添加到字典中
                    parentTaskDict[newTaskNumber] = newParentTask;
                    
                    var newSubTask = new JiraSubTask
                    {
                        TaskNumber = $"{newTaskNumber}-1",
                        TaskName = $"{newParentTask.Description} - 子任务 1",
                        Status = statusOptions[random.Next(statusOptions.Length)],
                        Priority = priorityOptions[random.Next(priorityOptions.Length)],
                        CreatedAt = DateTime.Now.AddHours(-random.Next(1, 24)),
                        UpdatedAt = DateTime.Now,
                        EstimatedHours = random.Next(1, 40),
                        ParentTask = newParentTask
                    };
                    
                    tasks.Add(newSubTask);
                }
                
                // 更新最后抽取时间
                _lastUpdateTime = DateTime.Now;
                
                // 触发任务更新事件
                TasksUpdated?.Invoke(this, EventArgs.Empty);
                
                // 模拟网络延迟
                await Task.Delay(500);
                
                // 输出调试信息
                System.Diagnostics.Debug.WriteLine($"生成了 {tasks.Count} 条模拟任务数据");
                
                return tasks;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取任务失败: {ex.Message}");
                return new List<JiraSubTask>();
            }
        }

        /// <summary>
        /// 开始开发子任务
        /// </summary>
        /// <param name="taskNumber">子任务单号</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> StartDevelopmentAsync(string taskNumber)
        {
            if (!_isLoggedIn)
            {
                _notificationService.ShowError("未登录", "请先登录Jira");
                return false;
            }

            try
            {
                // 模拟开始开发操作
                await Task.Delay(1000); // 模拟网络延迟
                
                _notificationService.ShowInformation("开始开发", $"已开始开发任务 {taskNumber}");
                return true;
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("开始开发失败", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 完成子任务开发
        /// </summary>
        /// <param name="taskNumber">子任务单号</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> CompleteDevelopmentAsync(string taskNumber)
        {
            if (!_isLoggedIn)
            {
                _notificationService.ShowError("未登录", "请先登录Jira");
                return false;
            }

            try
            {
                // 模拟完成开发操作
                await Task.Delay(1000); // 模拟网络延迟
                
                _notificationService.ShowInformation("完成开发", $"已完成任务 {taskNumber} 的开发");
                return true;
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("完成开发失败", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 分配任务
        /// </summary>
        /// <param name="taskNumber">子任务单号</param>
        /// <param name="username">用户名</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> AssignTaskAsync(string taskNumber, string username)
        {
            if (!_isLoggedIn)
            {
                _notificationService.ShowError("未登录", "请先登录Jira");
                return false;
            }

            try
            {
                // 模拟分配任务操作
                await Task.Delay(1000); // 模拟网络延迟
                
                _notificationService.ShowInformation("分配任务", $"已将任务 {taskNumber} 分配给 {username}");
                return true;
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("分配任务失败", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 获取Jira任务的完整URL
        /// </summary>
        /// <param name="taskNumber">任务单号</param>
        /// <returns>完整URL</returns>
        public string GetTaskUrl(string taskNumber)
        {
            return $"{_jiraUrl}/browse/{taskNumber}";
        }

        /// <summary>
        /// 解析日期时间字符串
        /// </summary>
        /// <param name="dateTimeString">日期时间字符串</param>
        /// <returns>日期时间</returns>
        private DateTime ParseDateTime(string dateTimeString)
        {
            if (string.IsNullOrEmpty(dateTimeString))
                return DateTime.MinValue;

            if (long.TryParse(dateTimeString, out long timestamp))
            {
                // 如果是时间戳，转换为DateTime
                return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
            }
            else if (DateTime.TryParse(dateTimeString, out DateTime dateTime))
            {
                // 如果是标准日期时间格式，直接返回
                return dateTime;
            }

            return DateTime.MinValue;
        }
    }
}
