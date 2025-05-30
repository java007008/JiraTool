using System;
using System.Collections.Generic;
using System.Windows;

namespace JiraTool.Controls.Notification
{
    /// <summary>
    /// 通知数据模型
    /// </summary>
    public class NotificationData
    {
        /// <summary>
        /// 通知ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 通知类型
        /// </summary>
        public NotificationType Type { get; set; } = NotificationType.Info;
        
        /// <summary>
        /// 通知标题
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// 通知内容
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// 超链接列表 - 键为链接文本，值为URL
        /// </summary>
        public Dictionary<string, string> Hyperlinks { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// 是否包含富文本内容
        /// </summary>
        public bool HasRichContent => Hyperlinks.Count > 0;
        
        /// <summary>
        /// 显示时长（毫秒）
        /// </summary>
        public int Duration { get; set; } = 4000;
        
        /// <summary>
        /// 是否显示关闭按钮
        /// </summary>
        public bool ShowCloseButton { get; set; } = true;
        
        /// <summary>
        /// 是否可以通过点击关闭
        /// </summary>
        public bool CanClickToClose { get; set; } = true;
        
        /// <summary>
        /// 关闭回调
        /// </summary>
        public Action<NotificationData> OnClose { get; set; }
        
        /// <summary>
        /// 点击回调
        /// </summary>
        public Action<NotificationData> OnClick { get; set; }
    }
}
