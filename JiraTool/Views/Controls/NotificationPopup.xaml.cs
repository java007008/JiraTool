using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace JiraTool.Views.Controls
{
    /// <summary>
    /// 通知类型枚举
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// 班车名变更通知
        /// </summary>
        BanCheNameChange,

        /// <summary>
        /// 描述变更通知
        /// </summary>
        DescriptionChange,

        /// <summary>
        /// 一般信息通知
        /// </summary>
        Information
    }

    /// <summary>
    /// NotificationPopup.xaml 的交互逻辑
    /// </summary>
    public partial class NotificationPopup : Window
    {
        /// <summary>
        /// 通知类型
        /// </summary>
        public NotificationType NotificationType { get; set; } = NotificationType.Information;

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; } = "通知";

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 任务单号
        /// </summary>
        public string TaskNumber { get; set; } = string.Empty;

        /// <summary>
        /// 任务URL
        /// </summary>
        public string TaskUrl { get; set; } = string.Empty;

        /// <summary>
        /// 是否可以点击任务单号
        /// </summary>
        public bool CanClickTaskNumber { get; set; } = false;

        /// <summary>
        /// 确认事件
        /// </summary>
        public event EventHandler? Confirmed;

        /// <summary>
        /// 构造函数
        /// </summary>
        public NotificationPopup()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 窗口加载事件处理
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 计算窗口位置（右下角）
            var workingArea = Screen.PrimaryScreen.WorkingArea;
            Left = workingArea.Right - Width - 20;
            Top = workingArea.Bottom - Height - 20;

            // 设置任务单号点击样式
            if (CanClickTaskNumber && !string.IsNullOrEmpty(TaskNumber))
            {
                TaskNumberTextBlock.Cursor = System.Windows.Input.Cursors.Hand;
                TaskNumberTextBlock.TextDecorations = TextDecorations.Underline;
            }
        }

        /// <summary>
        /// 任务单号点击事件处理
        /// </summary>
        private void TaskNumber_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CanClickTaskNumber && !string.IsNullOrEmpty(TaskUrl))
            {
                try
                {
                    // 打开任务URL
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = TaskUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"无法打开任务链接: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 确认按钮点击事件处理
        /// </summary>
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Confirmed?.Invoke(this, EventArgs.Empty);
            Close();
        }

        /// <summary>
        /// 关闭按钮点击事件处理
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
