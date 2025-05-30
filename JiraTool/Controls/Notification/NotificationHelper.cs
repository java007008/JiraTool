using System;
using System.Collections.Generic;

namespace JiraTool.Controls.Notification
{
    /// <summary>
    /// 通知辅助类，提供简化的通知方法
    /// </summary>
    public static class NotificationHelper
    {
        /// <summary>
        /// 显示班车号变更询问通知
        /// </summary>
        /// <param name="oldBusNumber">旧班车号</param>
        /// <param name="newBusNumber">新班车号</param>
        /// <param name="ticketId">相关工单号（可选）</param>
        /// <param name="onConfirm">确认回调（可选）</param>
        /// <param name="onCancel">取消回调（可选）</param>
        /// <returns>通知ID</returns>
        public static string ShowBusChangeQuestion(string oldBusNumber, string newBusNumber, string ticketId = null, Action<NotificationData> onConfirm = null, Action<NotificationData> onCancel = null)
        {
            string message = $"班车号已从 {oldBusNumber} 变更为 {newBusNumber}，是否确认已知悉？";
            
            // 如果有工单号，添加到消息中并创建超链接
            if (!string.IsNullOrEmpty(ticketId))
            {
                var hyperlinks = new Dictionary<string, string>
                {
                    { ticketId, ticketId } // JIRA单号会自动转换为URL
                };
                
                message += $"\n\n相关工单: {ticketId}";
                
                var notificationId = HandyNotificationManager.ShowWithHyperlinks(
                    message,
                    hyperlinks,
                    "班车号变更确认",
                    NotificationType.Question,
                    0  // 不自动关闭
                );
                
                // 手动设置询问回调
                if (onConfirm != null || onCancel != null)
                {
                    var notification = HandyNotificationManager.GetNotification(notificationId);
                    if (notification != null)
                    {
                        notification.SetConfirmAction(onConfirm);
                        notification.SetCancelAction(onCancel);
                    }
                }
                
                return notificationId;
            }
            else
            {
                // 没有工单号，使用简单通知
                var notificationId = HandyNotificationManager.Question(
                    message, 
                    "班车号变更确认",
                    0  // 不自动关闭
                );
                
                // 手动设置询问回调
                if (onConfirm != null || onCancel != null)
                {
                    var notification = HandyNotificationManager.GetNotification(notificationId);
                    if (notification != null)
                    {
                        notification.SetConfirmAction(onConfirm);
                        notification.SetCancelAction(onCancel);
                    }
                }
                
                return notificationId;
            }
        }
        
        /// <summary>
        /// 显示班车号变更通知
        /// </summary>
        /// <param name="busNumber">新班车号</param>
        /// <param name="ticketId">相关工单号（可选）</param>
        /// <returns>通知ID</returns>
        public static string ShowBusChangeNotification(string busNumber, string ticketId = null)
        {
            string message = $"班车号已变更为：{busNumber}";
            
            // 如果有工单号，添加到消息中并创建超链接
            if (!string.IsNullOrEmpty(ticketId))
            {
                var hyperlinks = new Dictionary<string, string>
                {
                    { ticketId, ticketId } // JIRA单号会自动转换为URL
                };
                
                message += $"，相关工单: {ticketId}";
                
                return HandyNotificationManager.ShowWithHyperlinks(
                    message,
                    hyperlinks,
                    "班车号变更通知",
                    NotificationType.Info
                );
            }
            else
            {
                // 没有工单号，使用简单通知
                return HandyNotificationManager.Info(
                    message, 
                    "班车号变更通知"
                );
            }
        }

        /// <summary>
        /// 显示警告类型的班车号变更通知
        /// </summary>
        /// <param name="busNumber">新班车号</param>
        /// <param name="ticketId">相关工单号（可选）</param>
        /// <returns>通知ID</returns>
        public static string ShowBusChangeWarning(string busNumber, string ticketId = null)
        {
            string message = $"警告：班车号已变更为：{busNumber}";
            
            if (!string.IsNullOrEmpty(ticketId))
            {
                var hyperlinks = new Dictionary<string, string>
                {
                    { ticketId, ticketId }
                };
                
                message += $"，相关工单: {ticketId}";
                
                return HandyNotificationManager.ShowWithHyperlinks(
                    message,
                    hyperlinks,
                    "班车号变更警告",
                    NotificationType.Warning
                );
            }
            else
            {
                return HandyNotificationManager.Warning(
                    message, 
                    "班车号变更警告"
                );
            }
        }

        /// <summary>
        /// 显示错误类型的班车号变更通知
        /// </summary>
        /// <param name="busNumber">新班车号</param>
        /// <param name="ticketId">相关工单号（可选）</param>
        /// <returns>通知ID</returns>
        public static string ShowBusChangeError(string busNumber, string ticketId = null)
        {
            string message = $"错误：班车号已变更为：{busNumber}";
            
            if (!string.IsNullOrEmpty(ticketId))
            {
                var hyperlinks = new Dictionary<string, string>
                {
                    { ticketId, ticketId }
                };
                
                message += $"，相关工单: {ticketId}";
                
                return HandyNotificationManager.ShowWithHyperlinks(
                    message,
                    hyperlinks,
                    "班车号变更错误",
                    NotificationType.Error
                );
            }
            else
            {
                return HandyNotificationManager.Error(
                    message, 
                    "班车号变更错误"
                );
            }
        }
    }
}
