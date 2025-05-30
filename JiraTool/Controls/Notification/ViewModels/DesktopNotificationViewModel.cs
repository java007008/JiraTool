using JiraTool.ViewModels.Base;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace JiraTool.Controls.Notification.ViewModels
{
    /// <summary>
    /// 桌面通知视图模型
    /// </summary>
    public class DesktopNotificationViewModel : ViewModelBase
    {
        private readonly DispatcherTimer _timer;
        private readonly NotificationData _data;
        private bool _isClosing;
        private bool _isMouseOver;
        private double _offScreenPosition;
        private double _onScreenPosition;
        private NotificationViewModel _notificationViewModel;
        
        /// <summary>
        /// 通知数据
        /// </summary>
        public NotificationData Data => _data;

        /// <summary>
        /// 屏幕外位置（动画起始位置）
        /// </summary>
        public double OffScreenPosition
        {
            get => _offScreenPosition;
            set => SetProperty(ref _offScreenPosition, value);
        }

        /// <summary>
        /// 屏幕内位置（动画结束位置）
        /// </summary>
        public double OnScreenPosition
        {
            get => _onScreenPosition;
            set => SetProperty(ref _onScreenPosition, value);
        }

        /// <summary>
        /// 通知视图模型
        /// </summary>
        public NotificationViewModel NotificationViewModel
        {
            get => _notificationViewModel;
            private set => SetProperty(ref _notificationViewModel, value);
        }

        /// <summary>
        /// 鼠标进入命令
        /// </summary>
        public ICommand MouseEnterCommand { get; }

        /// <summary>
        /// 鼠标离开命令
        /// </summary>
        public ICommand MouseLeaveCommand { get; }

        /// <summary>
        /// 加载完成命令
        /// </summary>
        public ICommand LoadedCommand { get; }

        /// <summary>
        /// 通知关闭事件
        /// </summary>
        public event EventHandler<NotificationData> NotificationClosed;

        /// <summary>
        /// 动画完成事件
        /// </summary>
        public event EventHandler AnimationCompleted;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="data">通知数据</param>
        public DesktopNotificationViewModel(NotificationData data)
        {
            _data = data;
            
            // 创建通知视图模型
            NotificationViewModel = new NotificationViewModel(data);
            NotificationViewModel.NotificationClosed += (s, e) => Close();
            
            // 创建自动关闭计时器
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(data.Duration)
            };
            _timer.Tick += Timer_Tick;
            
            // 初始化命令
            MouseEnterCommand = new RelayCommand<object>(_ => OnMouseEnter());
            MouseLeaveCommand = new RelayCommand<object>(_ => OnMouseLeave());
            LoadedCommand = new RelayCommand<object>(_ => OnLoaded());
        }

        /// <summary>
        /// 设置窗口位置
        /// </summary>
        /// <param name="windowWidth">窗口宽度</param>
        /// <param name="windowHeight">窗口高度</param>
        public void SetWindowPosition(double windowWidth, double windowHeight)
        {
            // 获取工作区域
            var workArea = SystemParameters.WorkArea;
            
            // 设置动画位置
            const int margin = 10; // 距离屏幕边缘的距离
            OnScreenPosition = workArea.Right - windowWidth - margin; // 屏幕内位置（右侧显示）
            OffScreenPosition = workArea.Right; // 屏幕外位置（初始在右侧屏幕外）
        }
        
        /// <summary>
        /// 窗口加载完成
        /// </summary>
        private void OnLoaded()
        {
            // 如果设置了自动关闭，启动计时器
            if (_data.Duration > 0)
            {
                _timer.Start();
            }
        }
        
        /// <summary>
        /// 计时器回调
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            
            // 如果鼠标悬停在窗口上，不自动关闭
            if (!_isMouseOver)
            {
                Close();
            }
        }
        
        /// <summary>
        /// 鼠标进入窗口
        /// </summary>
        public void OnMouseEnter()
        {
            _isMouseOver = true;
            _timer.Stop();
        }
        
        /// <summary>
        /// 鼠标离开窗口
        /// </summary>
        public void OnMouseLeave()
        {
            _isMouseOver = false;
            
            // 如果设置了自动关闭，重新启动计时器
            if (_data.Duration > 0 && !_isClosing)
            {
                _timer.Start();
            }
        }
        
        /// <summary>
        /// 启动计时器
        /// </summary>
        public void StartTimer()
        {
            // 如果是询问类型的通知，不启动自动关闭计时器
            if (_data.Type == NotificationType.Question)
            {
                return;
            }
            
            if (_data.Duration > 0 && !_isClosing)
            {
                _timer.Start();
            }
        }
        
        /// <summary>
        /// 关闭通知
        /// </summary>
        public void Close()
        {
            if (_isClosing) return;
            
            _isClosing = true;
            _timer.Stop();
            
            // 触发关闭事件
            NotificationClosed?.Invoke(this, _data);
        }
        
        /// <summary>
        /// 动画完成回调
        /// </summary>
        public void OnAnimationCompleted()
        {
            // 触发动画完成事件
            AnimationCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
