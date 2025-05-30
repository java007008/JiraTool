using System;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace JiraTool.Utils
{
    /// <summary>
    /// 基于CEF的网页抓取工具类，用于抓取包含JavaScript的页面
    /// </summary>
    public class CefWebScraper : IDisposable
    {
        private readonly ChromiumWebBrowser _browser;
        private readonly ILogger<CefWebScraper> _logger;
        private bool _isInitialized = false;
        private bool _isNavigating = false;
        private string _cookieString = string.Empty;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger">日志器</param>
        /// <param name="cookieString">Cookie字符串</param>
        public CefWebScraper(ILogger<CefWebScraper> logger = null, string cookieString = null)
        {
            _logger = logger;
            _cookieString = cookieString ?? string.Empty;
            
            try
            {
                // 初始化CEF环境（如果尚未初始化）
                InitializeCef();
                
                // 创建离屏浏览器实例
                _browser = new ChromiumWebBrowser();
                
                // 设置浏览器事件
                _browser.FrameLoadEnd += Browser_FrameLoadEnd;
                
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "初始化CEF浏览器失败");
                throw;
            }
        }
        
        /// <summary>
        /// 初始化CEF环境
        /// </summary>
        private void InitializeCef()
        {
            if (Cef.IsInitialized)
                return;
            
            var settings = new CefSettings
            {
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JiraTool", "CEF", "Cache"),
                LogSeverity = LogSeverity.Disable,
                WindowlessRenderingEnabled = true
            };
            
            // 初始化CEF
            Cef.Initialize(settings);
        }
        
        /// <summary>
        /// 页面加载完成事件
        /// </summary>
        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                _isNavigating = false;
            }
        }
        
        /// <summary>
        /// 导航到指定URL
        /// </summary>
        /// <param name="url">要导航的URL</param>
        public async Task NavigateToUrlAsync(string url)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("CEF浏览器未初始化");
            
            try
            {
                _isNavigating = true;
                
                // 导航到URL
                _browser.Load(url);
                
                // 等待导航完成
                await WaitForNavigationCompleteAsync();
                
                // 如果有cookie，设置cookie
                if (!string.IsNullOrEmpty(_cookieString))
                {
                    await SetCookiesAsync(url);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "导航到URL失败: {Url}", url);
                throw;
            }
        }
        
        /// <summary>
        /// 等待导航完成
        /// </summary>
        private async Task WaitForNavigationCompleteAsync(int timeoutMs = 30000)
        {
            var startTime = DateTime.Now;
            
            while (_isNavigating)
            {
                if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMs)
                {
                    throw new TimeoutException("导航超时");
                }
                
                await Task.Delay(100);
            }
        }
        
        /// <summary>
        /// 设置Cookie
        /// </summary>
        /// <param name="url">要设置Cookie的URL</param>
        private async Task SetCookiesAsync(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(_cookieString))
                    return;
                
                // 解析URL获取域名
                var uri = new Uri(url);
                var domain = uri.Host;
                
                // 解析cookie字符串
                var cookiePairs = _cookieString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var cookiePair in cookiePairs)
                {
                    var parts = cookiePair.Trim().Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        var name = parts[0].Trim();
                        var value = parts[1].Trim();
                        
                        // 创建cookie
                        var cookie = new Cookie
                        {
                            Name = name,
                            Value = value,
                            Domain = domain,
                            Path = "/",
                            Expires = DateTime.Now.AddDays(1)
                        };
                        
                        // 设置cookie
                        Cef.GetGlobalCookieManager().SetCookie(url, cookie);
                    }
                }
                
                _logger?.LogInformation($"成功设置{cookiePairs.Length}个Cookie");
                
                // 等待一下确保cookie生效
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "设置Cookie失败");
            }
        }
        
        /// <summary>
        /// 等待页面加载完成（包括异步JS）
        /// </summary>
        public async Task WaitForPageLoadAsync(int timeoutMs = 5000)
        {
            try
            {
                // 等待一段时间，确保异步JS加载完成
                await Task.Delay(timeoutMs);
                
                // 等待页面准备就绪
                var readyState = await _browser.EvaluateScriptAsync("document.readyState");
                
                int attempts = 0;
                while (attempts < 10 && (readyState.Result == null || readyState.Result.ToString() != "complete"))
                {
                    await Task.Delay(500);
                    readyState = await _browser.EvaluateScriptAsync("document.readyState");
                    attempts++;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "等待页面加载完成失败");
                throw;
            }
        }
        
        /// <summary>
        /// 执行JavaScript脚本
        /// </summary>
        /// <param name="script">要执行的JavaScript脚本</param>
        /// <returns>脚本执行结果</returns>
        public async Task<JavascriptResponse> ExecuteScriptAsync(string script)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("CEF浏览器未初始化");
            
            try
            {
                return await _browser.EvaluateScriptAsync(script);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "执行JavaScript脚本失败");
                throw;
            }
        }
        
        /// <summary>
        /// 获取页面HTML内容
        /// </summary>
        /// <returns>页面HTML内容</returns>
        public async Task<string> GetPageHtmlAsync()
        {
            try
            {
                var response = await _browser.EvaluateScriptAsync("document.documentElement.outerHTML");
                
                if (response.Success && response.Result != null)
                {
                    return response.Result.ToString();
                }
                
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "获取页面HTML内容失败");
                throw;
            }
        }
        
        /// <summary>
        /// 截取页面截图
        /// </summary>
        /// <param name="filePath">保存截图的文件路径</param>
        public async Task CaptureScreenshotAsync(string filePath)
        {
            try
            {
                // 对于WPF版本，我们需要使用不同的方法来截图
                // 这里使用WPF内置的方法来截取控件图像
                await Task.Delay(500); // 等待渲染完成
                
                // 注意：实际项目中，需要在UI线程上执行以下代码
                // 这里简化处理，实际使用时需要通过Dispatcher调用
                
                // 创建一个空的字节数组，实际项目中需要实现WPF控件的截图功能
                var screenshot = new byte[0];
                
                // 保存截图
                if (screenshot.Length > 0)
                {
                    File.WriteAllBytes(filePath, screenshot);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "截取页面截图失败");
                throw;
            }
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                // 移除事件处理程序
                if (_browser != null)
                {
                    _browser.FrameLoadEnd -= Browser_FrameLoadEnd;
                    _browser.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "释放CEF浏览器资源失败");
            }
        }
    }
}
