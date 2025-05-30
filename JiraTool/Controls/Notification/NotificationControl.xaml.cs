using System;
using System.Windows;
using System.Windows.Controls;
using JiraTool.Controls.Notification.ViewModels;

namespace JiraTool.Controls.Notification
{
    /// <summary>
    /// NotificationControl.xaml 的交互逻辑
    /// </summary>
    public partial class NotificationControl : UserControl
    {
        /// <summary>
        /// 视图模型
        /// </summary>
        public NotificationViewModel ViewModel
        {
            get => DataContext as NotificationViewModel;
            set => DataContext = value;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public NotificationControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="data">通知数据</param>
        public NotificationControl(NotificationData data) : this()
        {
            ViewModel = new NotificationViewModel(data);
            ViewModel.NotificationClosed += (s, e) => NotificationClosed?.Invoke(this, e);
        }

        /// <summary>
        /// 关闭事件
        /// </summary>
        public event EventHandler<NotificationData> NotificationClosed;
    }
}
