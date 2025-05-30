using System;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using JiraTool.Models;
using JiraTool.Services.Interfaces;
using JiraTool.Utils;
using JiraTool.Views;
using Microsoft.Extensions.Logging;

namespace JiraTool.Services
{
    /// <summary>
    /// Jira登录服务接口
    /// </summary>
    public interface IJiraLoginService
    {
        /// <summary>
        /// 登录成功事件
        /// </summary>
        event EventHandler LoginSuccessful;
        
        /// <summary>
        /// 打开Jira登录页面
        /// </summary>
        /// <param name="jiraUrl">Jira服务器URL</param>
        /// <returns>登录是否成功</returns>
        Task<bool> OpenLoginPage(string jiraUrl);
        
        /// <summary>
        /// 检查是否已登录
        /// </summary>
        /// <returns>是否已登录</returns>
        Task<bool> IsLoggedIn();
        
        /// <summary>
        /// 获取当前登录的Jira URL
        /// </summary>
        /// <returns>Jira URL</returns>
        string GetCurrentJiraUrl();
        
        /// <summary>
        /// 获取当前登录的Cookie
        /// </summary>
        /// <returns>Cookie字符串</returns>
        string GetCookies();
    }
    
    /// <summary>
    /// Jira登录服务实现
    /// </summary>
    public class JiraLoginService : IJiraLoginService
    {
        private readonly ILogger<JiraLoginService> _logger;
        private readonly IDatabaseService _databaseService;
        private readonly INotificationService _notificationService;
        private readonly AppConfigService _configService;
        
        private string _currentJiraUrl = string.Empty;
        private bool _isLoggedIn = false;
        private string _cookieString = string.Empty;
        private JiraLoginWindow _loginWindow;
        
        /// <summary>
        /// 登录成功事件
        /// </summary>
        public event EventHandler LoginSuccessful;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public JiraLoginService(ILogger<JiraLoginService> logger, IDatabaseService databaseService, INotificationService notificationService, AppConfigService configService)
        {
            _logger = logger;
            _databaseService = databaseService;
            _notificationService = notificationService;
            _configService = configService;
            
            // 从配置中加载Jira URL
            var config = _configService.GetConfig();
            if (!string.IsNullOrEmpty(config.BaseUrl))
            {
                _currentJiraUrl = config.BaseUrl;
            }
        }
        
        /// <summary>
        /// 打开Jira登录页面
        /// </summary>
        /// <param name="jiraUrl">Jira服务器URL</param>
        /// <returns>登录是否成功</returns>
        public async Task<bool> OpenLoginPage(string jiraUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(jiraUrl))
                {
                    _notificationService.ShowError("登录失败", "请输入有效的Jira服务器地址");
                    return false;
                }
                
                // 确保URL格式正确
                if (!jiraUrl.StartsWith("http://") && !jiraUrl.StartsWith("https://"))
                {
                    jiraUrl = "https://" + jiraUrl;
                }
                
                _currentJiraUrl = jiraUrl;
                
                // 保存Jira URL到配置
                var config = _configService.GetConfig();
                config.BaseUrl = jiraUrl;
                await _configService.SaveConfigAsync(config);
                
                // 初始化CEF
                CefBrowserHelper.Initialize();
                
                // 使用配置文件中的LoginUrl
                string loginUrl = config.LoginUrl;
                if (string.IsNullOrEmpty(loginUrl))
                {
                    // 如果没有配置登录URL，则使用默认的登录页面
                    loginUrl = jiraUrl + "/login.jsp";
                }
                
                // 创建登录窗口
                _loginWindow = new JiraLoginWindow(loginUrl);
                
                // 监听登录成功事件
                _loginWindow.LoginSuccessful += OnLoginSuccessful;
                
                // 显示登录窗口
                var result = _loginWindow.ShowDialog();
                
                return _isLoggedIn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "打开Jira登录页面失败");
                _notificationService.ShowError("登录失败", ex.Message);
                return false;
            }
        }
        
        /// <summary>
        /// 检查是否已登录
        /// </summary>
        /// <returns>是否已登录</returns>
        public Task<bool> IsLoggedIn()
        {
            return Task.FromResult(_isLoggedIn);
        }
        
        /// <summary>
        /// 获取当前登录的Jira URL
        /// </summary>
        /// <returns>Jira URL</returns>
        public string GetCurrentJiraUrl()
        {
            return _currentJiraUrl;
        }
        
        /// <summary>
        /// 获取当前登录的Cookie
        /// </summary>
        /// <returns>Cookie字符串</returns>
        public string GetCookies()
        {
            return _cookieString;
        }
        
        /// <summary>
        /// 登录成功事件处理
        /// </summary>
        private void OnLoginSuccessful(object sender, EventArgs e)
        {
            _isLoggedIn = true;
            
            // 获取并保存cookie
            if (sender is JiraLoginWindow loginWindow)
            {
                _cookieString = loginWindow.CookieString;
                _logger?.LogInformation($"成功获取并保存Cookie: {_cookieString.Length} 字符");
            }
            
            // 保存Jira URL到设置
            SaveJiraUrlToSettings(_currentJiraUrl);
            
            // 触发登录成功事件，通知主窗口更新状态
            LoginSuccessful?.Invoke(this, EventArgs.Empty);
            
            // 显示登录成功通知
            _notificationService.ShowInformation("登录成功", "已成功登录到Jira");
            
            // 关闭登录窗口
            _loginWindow?.Close();
        }
        
        /// <summary>
        /// 保存Jira URL到设置
        /// </summary>
        /// <param name="jiraUrl">Jira URL</param>
        private async void SaveJiraUrlToSettings(string jiraUrl)
        {
            try
            {
                // 保存到数据库设置
                var settings = await _databaseService.GetUserSettingsAsync();
                settings.JiraServerUrl = jiraUrl;
                await _databaseService.SaveUserSettingsAsync(settings);
                
                // 保存到配置文件
                var config = _configService.GetConfig();
                config.BaseUrl = jiraUrl;
                await _configService.SaveConfigAsync(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存Jira URL到设置失败");
            }
        }
    }
}
