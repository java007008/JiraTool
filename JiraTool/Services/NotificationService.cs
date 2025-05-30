using JiraTool.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace JiraTool.Services
{
    /// <summary>
    /// 通知服务实现类
    /// </summary>
    public class NotificationService : INotificationService
    {
        /// <summary>
        /// 当前活动的通知字典，用于防止重复通知
        /// Key: 唯一标识（如任务ID + 通知类型），Value: 通知窗口实例
        /// </summary>
        private readonly Dictionary<string, Window> _activeNotifications = new Dictionary<string, Window>();

        /// <summary>
        /// 班车名变更确认事件
        /// </summary>
        public event EventHandler<string>? BanCheNameChangeConfirmed;

        /// <summary>
        /// 描述变更确认事件
        /// </summary>
        public event EventHandler<string>? DescriptionChangeConfirmed;

        /// <summary>
        /// 显示班车名变更通知
        /// </summary>
        /// <param name="taskNumber">任务单号</param>
        /// <param name="oldBanCheName">旧班车名</param>
        /// <param name="newBanCheName">新班车名</param>
        /// <returns>用户是否点击了确认</returns>
        public async Task<bool> ShowBanCheNameChangeNotificationAsync(string taskNumber, string oldBanCheName, string newBanCheName)
        {
            // 检查是否已有相同通知正在显示
            string notificationKey = $"BanCheChange_{taskNumber}";
            if (_activeNotifications.ContainsKey(notificationKey))
            {
                return false; // 已有相同通知正在显示，返回false
            }

            // 创建一个TaskCompletionSource来异步等待用户响应
            var tcs = new TaskCompletionSource<bool>();

            // 在UI线程中执行
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 创建通知窗口
                var notification = new Views.Controls.NotificationPopup
                {
                    Title = "班车名变更通知",
                    NotificationType = Views.Controls.NotificationType.BanCheNameChange,
                    TaskNumber = taskNumber,
                    Message = $"{taskNumber} 班车名发生变更从 {oldBanCheName} 变更为 {newBanCheName}",
                    CanClickTaskNumber = true
                };

                // 注册事件处理
                notification.Confirmed += (sender, e) => 
                {
                    BanCheNameChangeConfirmed?.Invoke(this, taskNumber);
                    tcs.SetResult(true);
                    _activeNotifications.Remove(notificationKey);
                };

                notification.Closed += (sender, e) => 
                {
                    if (!tcs.Task.IsCompleted)
                    {
                        tcs.SetResult(false);
                    }
                    _activeNotifications.Remove(notificationKey);
                };

                // 保存到活动通知字典
                _activeNotifications[notificationKey] = notification;

                // 显示通知
                notification.Show();
            });

            // 等待用户响应
            return await tcs.Task;
        }

        /// <summary>
        /// 显示主任务描述变更通知
        /// </summary>
        /// <param name="taskNumber">任务单号</param>
        /// <returns>用户是否点击了确认</returns>
        public async Task<bool> ShowDescriptionChangeNotificationAsync(string taskNumber)
        {
            // 检查是否已有相同通知正在显示
            string notificationKey = $"DescChange_{taskNumber}";
            if (_activeNotifications.ContainsKey(notificationKey))
            {
                return false; // 已有相同通知正在显示，返回false
            }

            // 创建一个TaskCompletionSource来异步等待用户响应
            var tcs = new TaskCompletionSource<bool>();

            // 在UI线程中执行
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 创建通知窗口
                var notification = new Views.Controls.NotificationPopup
                {
                    Title = "任务描述变更通知",
                    NotificationType = Views.Controls.NotificationType.DescriptionChange,
                    TaskNumber = taskNumber,
                    Message = $"{taskNumber} 任务描述已发生变更",
                    CanClickTaskNumber = true
                };

                // 注册事件处理
                notification.Confirmed += (sender, e) => 
                {
                    DescriptionChangeConfirmed?.Invoke(this, taskNumber);
                    tcs.SetResult(true);
                    _activeNotifications.Remove(notificationKey);
                };

                notification.Closed += (sender, e) => 
                {
                    if (!tcs.Task.IsCompleted)
                    {
                        tcs.SetResult(false);
                    }
                    _activeNotifications.Remove(notificationKey);
                };

                // 保存到活动通知字典
                _activeNotifications[notificationKey] = notification;

                // 显示通知
                notification.Show();
            });

            // 等待用户响应
            return await tcs.Task;
        }

        /// <summary>
        /// 显示一般信息通知
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">消息内容</param>
        public void ShowInformation(string title, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        /// <summary>
        /// 显示错误通知
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">错误信息</param>
        public void ShowError(string title, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
        
        /// <summary>
        /// 显示成功通知
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">成功信息</param>
        public void ShowSuccess(string title, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        /// <summary>
        /// 显示确认对话框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">消息内容</param>
        /// <returns>用户是否确认</returns>
        public Task<bool> ShowConfirmationAsync(string title, string message)
        {
            return Task.Run(() =>
            {
                bool? result = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
                });
                return result == true;
            });
        }

        /// <summary>
        /// 显示SQL输入对话框
        /// </summary>
        /// <param name="taskNumber">任务单号</param>
        /// <param name="existingSql">现有SQL内容</param>
        /// <returns>用户输入的SQL，如果取消则返回null</returns>
        public async Task<string?> ShowSqlInputAsync(string taskNumber, string existingSql)
        {
            var tcs = new TaskCompletionSource<string?>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                // 在实际应用中，这里会创建并显示一个自定义的SQL输入对话框
                // 现在暂时使用简单的输入框代替
                var result = MessageBox.Show(
                    $"是否要为任务 {taskNumber} 添加SQL脚本？",
                    "SQL脚本",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    tcs.SetResult("SELECT * FROM users;\n-- 这里是示例SQL");
                }
                else
                {
                    tcs.SetResult(null);
                }
            });

            return await tcs.Task;
        }

        /// <summary>
        /// 显示配置输入对话框
        /// </summary>
        /// <param name="taskNumber">任务单号</param>
        /// <param name="existingConfig">现有配置内容</param>
        /// <returns>用户输入的配置，如果取消则返回null</returns>
        public async Task<string?> ShowConfigInputAsync(string taskNumber, string existingConfig)
        {
            var tcs = new TaskCompletionSource<string?>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                // 在实际应用中，这里会创建并显示一个自定义的配置输入对话框
                // 现在暂时使用简单的输入框代替
                var result = MessageBox.Show(
                    $"是否要为任务 {taskNumber} 添加配置信息？",
                    "配置信息",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    tcs.SetResult("{\n  \"key\": \"value\",\n  \"enabled\": true\n}");
                }
                else
                {
                    tcs.SetResult(null);
                }
            });

            return await tcs.Task;
        }
    }
}
