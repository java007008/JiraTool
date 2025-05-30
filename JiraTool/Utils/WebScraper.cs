using HtmlAgilityPack;
using JiraTool.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JiraTool.Utils
{
    /// <summary>
    /// 网页抓取工具类
    /// </summary>
    public class WebScraper
    {
        private readonly CookieContainer _cookieContainer;
        private readonly HttpClientHandler _handler;
        private readonly HttpClient _httpClient;
        private string _jiraUrl = string.Empty;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WebScraper()
        {
            _cookieContainer = new CookieContainer();
            _handler = new HttpClientHandler { CookieContainer = _cookieContainer };
            _httpClient = new HttpClient(_handler);
            // 设置默认请求头，模拟浏览器行为
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
        }

        /// <summary>
        /// 初始化爬虫
        /// </summary>
        /// <param name="jiraUrl">Jira服务器URL</param>
        public void Initialize(string jiraUrl)
        {
            _jiraUrl = jiraUrl.TrimEnd('/');
        }

        /// <summary>
        /// 模拟登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>登录是否成功</returns>
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                // 第一步：获取登录页面，通常包含CSRF令牌等信息
                var loginPageResponse = await _httpClient.GetAsync($"{_jiraUrl}/login.jsp");
                loginPageResponse.EnsureSuccessStatusCode();
                var loginPageContent = await loginPageResponse.Content.ReadAsStringAsync();

                // 解析登录页面，提取必要的表单字段（如CSRF令牌）
                var doc = new HtmlDocument();
                doc.LoadHtml(loginPageContent);

                // 示例：提取CSRF令牌（实际应用中需要根据真实页面结构调整）
                var csrfToken = doc.DocumentNode.SelectSingleNode("//input[@name='atl_token']")?.GetAttributeValue("value", string.Empty);

                // 构建登录表单数据
                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("os_username", username),
                    new KeyValuePair<string, string>("os_password", password),
                    new KeyValuePair<string, string>("os_destination", ""),
                    new KeyValuePair<string, string>("atl_token", csrfToken ?? string.Empty),
                    new KeyValuePair<string, string>("login", "登录")
                });

                // 发送登录请求
                var loginResponse = await _httpClient.PostAsync($"{_jiraUrl}/login.jsp", formData);
                
                // 检查登录是否成功（通常通过重定向到dashboard或其他页面判断）
                return loginResponse.IsSuccessStatusCode && !loginResponse.RequestMessage.RequestUri.ToString().Contains("login.jsp");
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取用户任务列表
        /// </summary>
        /// <returns>解析后的任务列表</returns>
        public async Task<List<JiraSubTask>> GetUserTasksAsync()
        {
            try
            {
                // 示例：访问用户任务列表页面
                var taskPageResponse = await _httpClient.GetAsync($"{_jiraUrl}/secure/Dashboard.jspa");
                taskPageResponse.EnsureSuccessStatusCode();
                var taskPageContent = await taskPageResponse.Content.ReadAsStringAsync();

                // 解析任务列表HTML
                return ParseTasksFromHtml(taskPageContent);
            }
            catch (Exception)
            {
                return new List<JiraSubTask>();
            }
        }

        /// <summary>
        /// 解析HTML内容，提取任务信息
        /// </summary>
        /// <param name="html">HTML内容</param>
        /// <returns>解析后的任务列表</returns>
        private List<JiraSubTask> ParseTasksFromHtml(string html)
        {
            var result = new List<JiraSubTask>();
            
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // 示例：解析任务列表（实际应用中需要根据真实页面结构调整）
                // 这里仅作为示例代码，实际实现需要根据Jira系统的HTML结构进行调整
                var taskNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'issue-list')]//div[contains(@class, 'issue')]");
                
                if (taskNodes != null)
                {
                    foreach (var taskNode in taskNodes)
                    {
                        // 提取主任务信息
                        var parentTaskNumber = taskNode.SelectSingleNode(".//div[contains(@class, 'parent-issue')]")?.InnerText.Trim() ?? string.Empty;
                        var parentTaskUrl = $"{_jiraUrl}/browse/{parentTaskNumber}";
                        
                        // 创建主任务对象
                        var parentTask = new JiraTask
                        {
                            TaskNumber = parentTaskNumber,
                            TaskUrl = parentTaskUrl,
                            Description = taskNode.SelectSingleNode(".//div[contains(@class, 'description')]")?.InnerText.Trim() ?? string.Empty,
                            Status = taskNode.SelectSingleNode(".//span[contains(@class, 'status')]")?.InnerText.Trim() ?? string.Empty,
                            BanCheName = taskNode.SelectSingleNode(".//div[contains(@class, 'banche')]")?.InnerText.Trim() ?? string.Empty,
                            CreatedAt = ParseDateTime(taskNode.SelectSingleNode(".//div[contains(@class, 'created')]")?.InnerText.Trim()),
                            UpdatedAt = ParseDateTime(taskNode.SelectSingleNode(".//div[contains(@class, 'updated')]")?.InnerText.Trim()),
                            Priority = taskNode.SelectSingleNode(".//div[contains(@class, 'priority')]")?.InnerText.Trim() ?? string.Empty
                        };

                        // 提取子任务信息
                        var subTaskNumber = taskNode.SelectSingleNode(".//div[contains(@class, 'issue-key')]")?.InnerText.Trim() ?? string.Empty;
                        var subTaskUrl = $"{_jiraUrl}/browse/{subTaskNumber}";
                        
                        // 创建子任务对象
                        var subTask = new JiraSubTask
                        {
                            TaskName = taskNode.SelectSingleNode(".//div[contains(@class, 'summary')]")?.InnerText.Trim() ?? string.Empty,
                            TaskNumber = subTaskNumber,
                            TaskUrl = subTaskUrl,
                            Status = taskNode.SelectSingleNode(".//span[contains(@class, 'status')]")?.InnerText.Trim() ?? string.Empty,
                            EstimatedHours = ParseDouble(taskNode.SelectSingleNode(".//div[contains(@class, 'hours')]")?.InnerText.Trim()),
                            EstimatedCompletionTime = ParseDateTime(taskNode.SelectSingleNode(".//div[contains(@class, 'duedate')]")?.InnerText.Trim()),
                            ActualCompletionTime = ParseDateTime(taskNode.SelectSingleNode(".//div[contains(@class, 'resolutiondate')]")?.InnerText.Trim()),
                            CreatedAt = ParseDateTime(taskNode.SelectSingleNode(".//div[contains(@class, 'created')]")?.InnerText.Trim()),
                            UpdatedAt = ParseDateTime(taskNode.SelectSingleNode(".//div[contains(@class, 'updated')]")?.InnerText.Trim()),
                            Priority = taskNode.SelectSingleNode(".//div[contains(@class, 'priority')]")?.InnerText.Trim() ?? string.Empty,
                            IsCodeMerged = false, // 默认值，实际应用中可能需要从页面提取
                            HasSqlScript = false, // 默认值，实际应用中可能需要从页面提取
                            HasConfiguration = false, // 默认值，实际应用中可能需要从页面提取
                            ParentTask = parentTask
                        };

                        result.Add(subTask);
                    }
                }
            }
            catch (Exception)
            {
                // 解析错误处理
            }

            return result;
        }

        /// <summary>
        /// 解析日期时间字符串
        /// </summary>
        /// <param name="dateString">日期时间字符串</param>
        /// <returns>解析后的日期时间，如果解析失败则返回当前时间</returns>
        private DateTime ParseDateTime(string? dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return DateTime.Now;

            if (DateTime.TryParse(dateString, out var result))
                return result;

            return DateTime.Now;
        }

        /// <summary>
        /// 解析双精度浮点数字符串
        /// </summary>
        /// <param name="numberString">数字字符串</param>
        /// <returns>解析后的双精度浮点数，如果解析失败则返回0</returns>
        private double ParseDouble(string? numberString)
        {
            if (string.IsNullOrEmpty(numberString))
                return 0;

            if (double.TryParse(numberString, out var result))
                return result;

            return 0;
        }
    }
}
