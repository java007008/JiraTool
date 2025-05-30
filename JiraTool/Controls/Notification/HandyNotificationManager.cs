using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using JiraTool.Utils;
using JiraTool.Views;
using JiraTool.Controls.Notification.ViewModels;
using Microsoft.Extensions.Logging;

namespace JiraTool.Controls.Notification
{
    /// <summary>
    /// 仿 HandyControl 的通知管理器
    /// </summary>
    public static class HandyNotificationManager
    {
        // 活动通知列表
        private static readonly List<HandyNotification> _activeNotifications = new List<HandyNotification>();

        // 通知队列，当所有位置都被占用时，新通知进入队列
        private static readonly Queue<NotificationData> _notificationQueue = new Queue<NotificationData>();

        // 最多显示的通知数量
        private const int MaxVisibleNotifications = 5;

        // 通知窗口之间的间距
        private const int NotificationSpacing = 16;
        
        // CEF浏览器实例
        private static CefWebScraper _cefBrowser = new CefWebScraper();

        /// <summary>
        /// 显示桌面通知
        /// </summary>
        /// <param name="message">通知内容</param>
        /// <param name="title">通知标题</param>
        /// <param name="type">通知类型</param>
        /// <param name="duration">显示时长（毫秒）</param>
        /// <param name="onClose">关闭回调</param>
        /// <param name="onClick">点击回调</param>
        /// <returns>通知ID</returns>
        public static string ShowDesktopNotification(
            string message,
            string title = "",
            NotificationType type = NotificationType.Info,
            int duration = 4000,
            Action<NotificationData> onClose = null,
            Action<NotificationData> onClick = null)
        {
            var data = new NotificationData
            {
                Message = message,
                Title = title,
                Type = type,
                Duration = duration,
                OnClose = onClose,
                OnClick = onClick
            };

            return ShowDesktopNotification(data);
        }

        /// <summary>
        /// 显示桌面通知
        /// </summary>
        /// <param name="data">通知数据</param>
        /// <returns>通知ID</returns>
        public static string ShowDesktopNotification(NotificationData data)
        {
            // 如果已经有最大数量的通知，则将通知加入队列等待
            if (_activeNotifications.Count >= MaxVisibleNotifications)
            {
                _notificationQueue.Enqueue(data);
                return data.Id;
            }

            // 在UI线程上创建并显示通知
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 确定是否保持打开（询问类型的通知不自动关闭）
                bool staysOpen = data.Type == NotificationType.Question || data.Duration <= 0;

                try
                {
                    // 创建通知窗口
                    var notification = HandyNotification.Show(data, ShowAnimation.HorizontalMove, staysOpen);
                    notification.NotificationClosed += NotificationWindow_Closed;
                    notification.HyperlinkClicked += Notification_HyperlinkClicked;

                    // 确保通知内容正确显示
                    if (notification.Content is NotificationViewModel viewModel)
                    {
                        // 强制刷新属性
                        viewModel.RefreshProperties();
                        
                        // 设置通知标题和消息
                        viewModel.Title = data.Title;
                        viewModel.Message = data.Message;
                        
                        // 如果有超链接，确保它们被正确初始化
                        if (data.Hyperlinks.Count > 0)
                        {
                            viewModel.Hyperlinks = new Dictionary<string, string>(data.Hyperlinks);
                        }
                    }

                    // 添加到活动通知列表
                    _activeNotifications.Add(notification);

                    // 调整所有通知的位置
                    AdjustNotificationPositions();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"显示通知时出错: {ex.Message}");
                }
            });

            return data.Id;
        }

        /// <summary>
        /// 调整所有通知的位置
        /// </summary>
        private static void AdjustNotificationPositions()
        {
            try
            {
                // 获取工作区域
                var workArea = SystemParameters.WorkArea;
                
                // 计算通知窗口的位置
                int index = 0;
                foreach (var notification in _activeNotifications.ToList())
                {
                    // 先保存当前内容
                    var content = notification.Content;
                    
                    // 计算通知窗口的位置
                    double top = workArea.Bottom - (index + 1) * (notification.ActualHeight + NotificationSpacing);
                    
                    // 设置通知窗口的位置
                    notification.Top = top;
                    notification.Left = workArea.Right - notification.ActualWidth - 20;
                    
                    // 确保内容不丢失
                    if (notification.Content == null && content != null)
                    {
                        notification.Content = content;
                    }
                    
                    // 确保通知内容可见
                    notification.EnsureContentVisible();
                    
                    index++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"调整通知位置时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 通知窗口关闭回调
        /// </summary>
        private static void NotificationWindow_Closed(object sender, NotificationData e)
        {
            if (sender is HandyNotification notification)
            {
                // 从活动通知列表中移除
                _activeNotifications.Remove(notification);

                // 调整其他通知的位置
                AdjustNotificationPositions();

                // 如果队列中有等待的通知，则显示下一个
                if (_notificationQueue.Count > 0)
                {
                    var nextData = _notificationQueue.Dequeue();
                    ShowDesktopNotification(nextData);
                }
            }
        }

        /// <summary>
        /// 关闭通知
        /// </summary>
        /// <param name="id">通知ID</param>
        public static void CloseNotification(string id)
        {
            foreach (var notification in _activeNotifications.ToArray())
            {
                if (notification.Data.Id == id)
                {
                    notification.Close();
                    break;
                }
            }
        }
        
        /// <summary>
        /// 获取通知实例
        /// </summary>
        /// <param name="id">通知ID</param>
        /// <returns>通知实例，如果未找到则返回null</returns>
        public static HandyNotification GetNotification(string id)
        {
            return _activeNotifications.FirstOrDefault(n => n.Data.Id == id);
        }

        /// <summary>
        /// 关闭所有通知
        /// </summary>
        public static void CloseAllNotifications()
        {
            foreach (var notification in _activeNotifications.ToArray())
            {
                notification.Close();
            }

            _notificationQueue.Clear();
        }

        #region 快捷方法

        /// <summary>
        /// 显示信息通知
        /// </summary>
        public static string Info(string message, string title = "", int duration = 4000, Action<NotificationData> onClose = null, Action<NotificationData> onClick = null)
        {
            return ShowDesktopNotification(message, title, NotificationType.Info, duration, onClose, onClick);
        }

        /// <summary>
        /// 显示成功通知
        /// </summary>
        public static string Success(string message, string title = "", int duration = 4000, Action<NotificationData> onClose = null, Action<NotificationData> onClick = null)
        {
            return ShowDesktopNotification(message, title, NotificationType.Success, duration, onClose, onClick);
        }

        /// <summary>
        /// 显示警告通知
        /// </summary>
        public static string Warning(string message, string title = "", int duration = 4000, Action<NotificationData> onClose = null, Action<NotificationData> onClick = null)
        {
            return ShowDesktopNotification(message, title, NotificationType.Warning, duration, onClose, onClick);
        }

        /// <summary>
        /// 显示错误通知
        /// </summary>
        public static string Error(string message, string title = "", int duration = 4000, Action<NotificationData> onClose = null, Action<NotificationData> onClick = null)
        {
            return ShowDesktopNotification(message, title, NotificationType.Error, duration, onClose, onClick);
        }

        /// <summary>
        /// 显示询问通知
        /// </summary>
        public static string Question(string message, string title = "", int duration = 0, Action<NotificationData> onClose = null, Action<NotificationData> onClick = null)
        {
            // 设置默认的关闭回调，确保取消后关闭通知
            Action<NotificationData> defaultOnClose = (data) => 
            {
                // 调用用户提供的关闭回调（如果有）
                onClose?.Invoke(data);
                
                // 关闭通知
                CloseNotification(data.Id);
            };
            
            // 询问类型的通知默认不会自动消失（duration=0）
            return ShowDesktopNotification(message, title, NotificationType.Question, duration, defaultOnClose, onClick);
        }

        #endregion
        
        #region 超链接处理
        
        /// <summary>
        /// 初始化CEF浏览器
        /// </summary>
        private static void InitializeCefBrowser()
        {
            if (_cefBrowser == null)
            {
                // 创建CEF浏览器实例
                _cefBrowser = new CefWebScraper();
            }
            
            // 注意：CEF浏览器已在构造函数中初始化
        }
        
        /// <summary>
        /// 处理超链接点击事件
        /// </summary>
        private static async void Notification_HyperlinkClicked(object sender, string url)
        {
            try
            {
                // 初始化CEF浏览器
                InitializeCefBrowser();
                
                // 使用CEF浏览器打开URL
                await OpenUrlInCefBrowser(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开链接失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 在CEF浏览器中打开URL
        /// </summary>
        private static async Task OpenUrlInCefBrowser(string url)
        {
            try
            {
                // 判断是否是单号链接（例如“JIRA-1234”格式）
                if (IsJiraTicketId(url))
                {
                    // 将单号转换为Jira URL
                    url = ConvertToJiraUrl(url);
                }
                
                // 在主线程上打开浏览器窗口
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // 使用主程序的浏览器打开URL
                    if (Application.Current.MainWindow is MainWindow mainWindow)
                    {
                        mainWindow.OpenBrowserTab(url);
                    }
                    else
                    {
                        // 如果主窗口不可用，则使用系统浏览器打开
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"打开URL失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 判断是否是Jira单号
        /// </summary>
        private static bool IsJiraTicketId(string text)
        {
            // 判断是否符合Jira单号格式，例如“JIRA-1234”
            return System.Text.RegularExpressions.Regex.IsMatch(text, @"^[A-Z]+-\d+$");
        }
        
        /// <summary>
        /// 将Jira单号转换为URL
        /// </summary>
        private static string ConvertToJiraUrl(string ticketId)
        {
            // 这里可以配置为实际的Jira URL
            return $"https://jira.example.com/browse/{ticketId}";
        }
        
        /// <summary>
        /// 创建带有超链接的通知
        /// </summary>
        public static string ShowWithHyperlinks(string message, Dictionary<string, string> hyperlinks, string title = "", NotificationType type = NotificationType.Info, int duration = 4000)
        {
            var data = new NotificationData
            {
                Message = message,
                Title = title,
                Type = type,
                Duration = duration,
                Hyperlinks = hyperlinks
            };
            
            return ShowDesktopNotification(data);
        }
        
        #endregion
    }
}
