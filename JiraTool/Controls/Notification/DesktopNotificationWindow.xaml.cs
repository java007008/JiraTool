using System;
using System.Windows;
using System.Windows.Media.Animation;
using JiraTool.Controls.Notification.ViewModels;

namespace JiraTool.Controls.Notification
{
    /// <summary>
    /// DesktopNotificationWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DesktopNotificationWindow : Window
    {
        /// <summary>
        /// 通知关闭事件
        /// </summary>
        public event EventHandler<NotificationData> NotificationClosed;
        /// <summary>
        /// 视图模型
        /// </summary>
        public DesktopNotificationViewModel ViewModel
        {
            get => DataContext as DesktopNotificationViewModel;
            set => DataContext = value;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="data">通知数据</param>
        public DesktopNotificationWindow(NotificationData data)
        {
            InitializeComponent();
            
            // 创建视图模型
            ViewModel = new DesktopNotificationViewModel(data);
            
            // 设置窗口内容
            NotificationContent.Content = new NotificationControl(data);
            
            // 不能直接将GUID设置为窗口名称，因为WPF对Name属性有特定的格式要求
            // 我们将使用Tag属性来存储通知ID
            Tag = data.Id;
            
            // 设置窗口位置
            ViewModel.SetWindowPosition(Width, Height);
            Left = ViewModel.OffScreenPosition; // 初始在屏幕左侧外
            // 不需要在这里设置Top位置，由HandyNotificationManager的AdjustNotificationPositions方法设置
            
            // 订阅事件
            ViewModel.NotificationClosed += (s, e) => CloseNotification();
            ViewModel.AnimationCompleted += (s, e) => Close();
            
            // 如果设置了点击关闭，添加点击事件
            if (data.CanClickToClose)
            {
                MouseLeftButtonUp += (s, e) =>
                {
                    // 执行点击回调
                    data.OnClick?.Invoke(data);
                    ViewModel.Close();
                };
            }
            
            // 窗口加载完成后显示动画
            Loaded += (s, e) =>
            {
                try
                {
                    // 确保通知内容已设置
                    if (NotificationContent.Content == null)
                    {
                        NotificationContent.Content = new NotificationControl(data);
                    }
                    
                    // 开始显示动画
                    var storyboard = (Storyboard)FindResource("ShowAnimation");
                    storyboard.Begin(this);
                    
                    // 如果是询问类型的通知，添加特殊样式
                    if (data.Type == NotificationType.Question)
                    {
                        // 为询问类型的通知添加特殊样式，提示用户需要点击
                        var control = NotificationContent.Content as NotificationControl;
                        if (control != null && control.MessageText != null)
                        {
                            control.MessageText.FontWeight = System.Windows.FontWeights.Bold;
                        }
                    }
                    
                    // 如果设置了自动关闭，启动计时器
                    if (data.Duration > 0)
                    {
                        ViewModel.StartTimer();
                    }
                    
                    // 强制刷新内容
                    UpdateLayout();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"通知加载时出错: {ex.Message}");
                }
            };
            
            // 鼠标进入窗口
            MouseEnter += (s, e) =>
            {
                ViewModel.OnMouseEnter();
            };
            
            // 鼠标离开窗口
            MouseLeave += (s, e) =>
            {
                ViewModel.OnMouseLeave();
            };
        }
        
        /// <summary>
        /// 关闭通知
        /// </summary>
        public void CloseNotification()
        {
            var storyboard = (Storyboard)FindResource("HideAnimation");
            storyboard.Begin(this);
            
            // 确保通知完全关闭
            storyboard.Completed += (s, e) => 
            {
                // 如果通知还没有关闭，强制关闭
                if (this.IsVisible)
                {
                    this.Close();
                }
            };
        }
        
        /// <summary>
        /// 隐藏动画完成回调
        /// </summary>
        private void HideAnimation_Completed(object sender, EventArgs e)
        {
            // 触发关闭事件
            NotificationClosed?.Invoke(this, ViewModel.Data);
            
            // 强制关闭窗口
            this.Close();
            
            // 通知视图模型动画完成
            ViewModel.OnAnimationCompleted();
        }
    }
}
