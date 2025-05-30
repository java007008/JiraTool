using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;

namespace JiraTool.Utils
{
    /// <summary>
    /// CEF浏览器辅助类，用于打开网页
    /// </summary>
    public static class CefBrowserHelper
    {
        private static bool _initialized = false;

        /// <summary>
        /// 初始化CEF环境
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            try
            {
                var settings = new CefSettings
                {
                    CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JiraTool", "CEF", "Cache"),
                    LogSeverity = LogSeverity.Disable
                };

                // 初始化CEF
                Cef.Initialize(settings);
                _initialized = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CEF初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 打开URL
        /// </summary>
        /// <param name="url">要打开的URL</param>
        public static void OpenUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return;

            try
            {
                // 使用系统默认浏览器打开
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"打开URL失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 在新窗口中打开URL
        /// </summary>
        /// <param name="url">要打开的URL</param>
        /// <param name="title">窗口标题</param>
        public static void OpenUrlInNewWindow(string url, string title)
        {
            if (string.IsNullOrEmpty(url)) return;
            if (!_initialized)
            {
                Initialize();
            }

            try
            {
                // 创建CEF浏览器窗口
                var window = new CefBrowserWindow(url, title);
                window.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"打开URL失败: {ex.Message}");
                // 降级为使用系统默认浏览器
                OpenUrl(url);
            }
        }

        /// <summary>
        /// 关闭CEF环境
        /// </summary>
        public static void Shutdown()
        {
            if (!_initialized) return;

            try
            {
                Cef.Shutdown();
                _initialized = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CEF关闭失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// CEF浏览器窗口
    /// </summary>
    public class CefBrowserWindow : System.Windows.Window
    {
        private readonly ChromiumWebBrowser _browser;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="url">要打开的URL</param>
        /// <param name="title">窗口标题</param>
        public CefBrowserWindow(string url, string title)
        {
            Title = title;
            Width = 1024;
            Height = 768;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            // 创建浏览器控件
            _browser = new ChromiumWebBrowser(url);
            Content = _browser;

            // 窗口关闭时处理资源
            Closed += (sender, e) =>
            {
                _browser.Dispose();
            };
        }
    }
}
