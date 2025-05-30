using HtmlAgilityPack;
using JiraTool.Models;
using JiraTool.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JiraTool.Services
{
    /// <summary>
    /// Jira网页服务实现类
    /// </summary>
    public class JiraWebService : IJiraService
    {
        private readonly HttpClient _httpClient;
        private readonly CookieContainer _cookieContainer;
        private readonly HttpClientHandler _handler;
        private bool _isLoggedIn = true; // 默认已登录
        private string _jiraUrl = string.Empty;
        private string _username = string.Empty;
        private DateTime _lastUpdateTime = DateTime.MinValue;
        
        /// <summary>
        /// 任务数据更新事件
        /// </summary>
        public event EventHandler? TasksUpdated;

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
        /// 构造函数
        /// </summary>
        public JiraWebService()
        {
            _cookieContainer = new CookieContainer();
            _handler = new HttpClientHandler { CookieContainer = _cookieContainer };
            _httpClient = new HttpClient(_handler);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "JiraTool/1.0");
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
            // 保存URL和用户名
            _jiraUrl = jiraUrl.TrimEnd('/');
            _username = username;

            // 登录请求参数
            var loginData = new
            {
                username,
                password
            };

            // 这里仅模拟登录，实际应用中需要替换为真实的登录API
            var loginContent = new StringContent(
                JsonConvert.SerializeObject(loginData),
                Encoding.UTF8,
                "application/json");

            try
            {
                // 模拟登录成功
                // 在实际应用中，需要替换为真实的登录API调用
                // var response = await _httpClient.PostAsync($"{_jiraUrl}/rest/auth/1/session", loginContent);
                // return response.IsSuccessStatusCode;

                // 模拟登录成功
                _isLoggedIn = true;
                return true;
            }
            catch (Exception)
            {
                _isLoggedIn = false;
                return false;
            }
        }

        /// <summary>
        /// 登出Jira
        /// </summary>
        /// <returns>操作任务</returns>
        public async Task LogoutAsync()
        {
            if (!_isLoggedIn) return;

            try
            {
                // 在实际应用中，需要替换为真实的登出API调用
                // await _httpClient.DeleteAsync($"{_jiraUrl}/rest/auth/1/session");
                _isLoggedIn = false;
            }
            catch (Exception)
            {
                // 忽略登出错误
            }
        }

        /// <summary>
        /// 获取所有任务
        /// </summary>
        /// <returns>任务列表</returns>
        public async Task<List<JiraSubTask>> GetTasksAsync()
        {
            // 调用现有的获取用户任务方法
            return await GetUserTasksAsync();
        }
        
        /// <summary>
        /// 获取用户的任务列表
        /// </summary>
        /// <returns>任务列表</returns>
        public async Task<List<JiraSubTask>> GetUserTasksAsync()
        {
            if (!_isLoggedIn)
                return new List<JiraSubTask>();

            try
            {
                // 这里是模拟数据，实际应用中需要替换为真实的API调用
                // 模拟不同状态的子任务
                var mockSubTasks = new List<JiraSubTask>();
                var statuses = new[] { "待处理", "处理中", "已完成" };
                var random = new Random();

                // 模拟3个主任务，每个主任务有2-4个子任务
                for (int i = 1; i <= 3; i++)
                {
                    var parentTask = new JiraTask
                    {
                        Id = i,
                        TaskNumber = $"JIRA-{1000 + i}",
                        TaskUrl = $"{_jiraUrl}/browse/JIRA-{1000 + i}",
                        Description = $"这是主任务{i}的描述，包含了一些业务需求和技术细节。需要完成相应的开发工作并按时交付。",
                        Status = statuses[random.Next(0, 3)],
                        BanCheName = $"20250{random.Next(10, 30)}",
                        CreatedAt = DateTime.Now.AddDays(-random.Next(5, 20)),
                        UpdatedAt = DateTime.Now.AddDays(-random.Next(0, 5)),
                        Priority = random.Next(1, 4).ToString()
                    };

                    // 为每个主任务创建2-4个子任务
                    int subTaskCount = random.Next(2, 5);
                    for (int j = 1; j <= subTaskCount; j++)
                    {
                        var subTask = new JiraSubTask
                        {
                            Id = i * 10 + j,
                            TaskName = $"子任务{i}-{j}",
                            TaskNumber = $"JIRA-{1000 + i}-{j}",
                            TaskUrl = $"{_jiraUrl}/browse/JIRA-{1000 + i}-{j}",
                            Status = statuses[random.Next(0, 3)],
                            EstimatedHours = random.Next(1, 24),
                            EstimatedCompletionTime = DateTime.Now.AddDays(random.Next(1, 10)),
                            ActualCompletionTime = random.Next(0, 2) == 0 ? (DateTime?)null : DateTime.Now.AddDays(-random.Next(1, 5)),
                            CreatedAt = DateTime.Now.AddDays(-random.Next(5, 20)),
                            UpdatedAt = DateTime.Now.AddDays(-random.Next(0, 5)),
                            Priority = random.Next(1, 4).ToString(),
                            IsCodeMerged = random.Next(0, 2) == 1,
                            HasSqlScript = random.Next(0, 2) == 1,
                            HasConfiguration = random.Next(0, 2) == 1,
                            ParentTaskId = parentTask.Id,
                            ParentTask = parentTask
                        };

                        // 如果有SQL脚本，则添加一些示例SQL
                        if (subTask.HasSqlScript)
                        {
                            subTask.SqlScript = "SELECT * FROM users WHERE id = 1;\nUPDATE products SET price = 99.99 WHERE product_id = 123;";
                        }

                        // 如果有配置，则添加一些示例配置
                        if (subTask.HasConfiguration)
                        {
                            subTask.Configuration = "{\n  \"serverUrl\": \"https://api.example.com\",\n  \"timeout\": 30000,\n  \"maxRetries\": 3\n}";
                        }

                        mockSubTasks.Add(subTask);
                    }
                }

                _lastUpdateTime = DateTime.Now;
                // 触发任务更新事件
                TasksUpdated?.Invoke(this, EventArgs.Empty);

                return mockSubTasks;
            }
            catch (Exception)
            {
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
            if (!_isLoggedIn) return false;

            try
            {
                // 在实际应用中，需要替换为真实的API调用
                // 模拟操作成功
                return true;
            }
            catch (Exception)
            {
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
            if (!_isLoggedIn) return false;

            try
            {
                // 在实际应用中，需要替换为真实的API调用
                // 模拟操作成功
                return true;
            }
            catch (Exception)
            {
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
            if (!_isLoggedIn) return false;

            try
            {
                // 在实际应用中，需要替换为真实的API调用
                // 模拟操作成功
                return true;
            }
            catch (Exception)
            {
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
        /// 解析HTML内容以提取数据（网页抓取示例）
        /// </summary>
        /// <param name="html">HTML内容</param>
        /// <returns>解析后的数据</returns>
        private List<JiraSubTask> ParseHtmlContent(string html)
        {
            var result = new List<JiraSubTask>();
            
            try
            {
                // 使用HtmlAgilityPack解析HTML
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // 这里是示例代码，实际应用中需要根据真实的HTML结构进行调整
                // var taskNodes = doc.DocumentNode.SelectNodes("//div[@class='issue-list']//div[@class='issue-item']");
                // if (taskNodes != null)
                // {
                //     foreach (var taskNode in taskNodes)
                //     {
                //         var subTask = new JiraSubTask
                //         {
                //             TaskNumber = taskNode.SelectSingleNode(".//div[@class='key']")?.InnerText.Trim() ?? string.Empty,
                //             TaskName = taskNode.SelectSingleNode(".//div[@class='summary']")?.InnerText.Trim() ?? string.Empty,
                //             // 更多字段解析...
                //         };
                //         result.Add(subTask);
                //     }
                // }
            }
            catch (Exception)
            {
                // 解析错误处理
            }

            return result;
        }
    }
}
