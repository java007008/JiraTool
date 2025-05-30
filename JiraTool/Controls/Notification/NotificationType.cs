using System;

namespace JiraTool.Controls.Notification
{
    /// <summary>
    /// 通知类型枚举
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// 普通消息
        /// </summary>
        Info,
        
        /// <summary>
        /// 成功消息
        /// </summary>
        Success,
        
        /// <summary>
        /// 警告消息
        /// </summary>
        Warning,
        
        /// <summary>
        /// 错误消息
        /// </summary>
        Error,
        
        /// <summary>
        /// 询问消息
        /// </summary>
        Question
    }
}
