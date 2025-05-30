using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace JiraTool.Views
{
    /// <summary>
    /// JiraLoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class JiraLoginWindow : Window
    {
        private readonly ILogger<JiraLoginWindow> _logger;
        private string _jiraUrl;
        
        /// <summary>
        /// 登录成功事件
        /// </summary>
        public event EventHandler LoginSuccessful;
        
        /// <summary>
        /// Cookie字符串
        /// </summary>
        public string CookieString { get; private set; }
        
        /// <summary>
        /// Jira URL
        /// </summary>
        public string JiraUrl
        {
            get => _jiraUrl;
            private set
            {
                _jiraUrl = value;
                Browser.Address = value;
            }
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="jiraUrl">Jira URL</param>
        public JiraLoginWindow(string jiraUrl)
        {
            InitializeComponent();
            
            _logger = App.AppHost?.Services.GetService(typeof(ILogger<JiraLoginWindow>)) as ILogger<JiraLoginWindow>;
            
            JiraUrl = jiraUrl;
            
            // 设置浏览器事件
            Browser.FrameLoadEnd += Browser_FrameLoadEnd;
            
            // 设置数据上下文
            DataContext = this;
        }
        
        /// <summary>
        /// 页面加载完成事件
        /// </summary>
        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (!e.Frame.IsMain)
                return;
            
            try
            {
                // 检查是否已登录
                Browser.EvaluateScriptAsync("document.body.innerText").ContinueWith(t =>
                {
                    if (t.Result.Success && t.Result.Result != null)
                    {
                        string pageContent = t.Result.Result.ToString();
                        
                        // 检查是否在登录页面
                        bool isLoginPage = pageContent.Contains("登录") || pageContent.Contains("Login") || 
                                          pageContent.Contains("用户名") || pageContent.Contains("Username") ||
                                          pageContent.Contains("密码") || pageContent.Contains("Password");
                        
                        // 检查是否已登录成功（在Jira主页面）
                        bool isLoggedIn = pageContent.Contains("仪表板") || pageContent.Contains("Dashboard") ||
                                         pageContent.Contains("项目") || pageContent.Contains("Projects") ||
                                         pageContent.Contains("问题") || pageContent.Contains("Issues");
                        
                        if (isLoggedIn)
                        {
                            // 获取cookie
                            GetCookies().ContinueWith(cookieTask => 
                            {
                                if (cookieTask.Result)
                                {
                                    // 登录成功，触发事件
                                    Dispatcher.Invoke(() =>
                                    {
                                        LoginSuccessful?.Invoke(this, EventArgs.Empty);
                                    });
                                }
                            });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "检查登录状态失败");
            }
        }
        
        /// <summary>
        /// 刷新按钮点击事件
        /// </summary>
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Browser.Reload();
        }
        
        /// <summary>
        /// 关闭按钮点击事件
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        /// <summary>
        /// 标题栏鼠标按下事件，实现拖动窗口
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 当鼠标在标题栏上按下时，开始拖动窗口
            DragMove();
        }
        
        /// <summary>
        /// 获取Cookie
        /// </summary>
        /// <returns>是否成功获取cookie</returns>
        private async Task<bool> GetCookies()
        {
            try
            {
                // 获取当前域名
                var host = new Uri(Browser.Address).Host;
                
                // 获取所有cookie
                var cookieManager = Cef.GetGlobalCookieManager();
                var visitor = new TaskCookieVisitor();
                cookieManager.VisitUrlCookies(Browser.Address, true, visitor);
                var cookies = await visitor.Task;
                
                if (cookies.Count > 0)
                {
                    // 将cookie转换为字符串格式
                    var cookieStrings = cookies.Select(c => $"{c.Name}={c.Value}");
                    CookieString = string.Join("; ", cookieStrings);
                    
                    _logger?.LogInformation($"成功获取{cookies.Count}个Cookie");
                    return true;
                }
                
                _logger?.LogWarning("未找到任何Cookie");
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "获取Cookie失败");
                return false;
            }
        }
        
        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            // 释放浏览器资源
            Browser.FrameLoadEnd -= Browser_FrameLoadEnd;
            Browser.Dispose();
            
            base.OnClosed(e);
        }
    }
}
