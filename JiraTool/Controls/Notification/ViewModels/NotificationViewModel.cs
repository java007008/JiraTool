using JiraTool.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using System.Windows.Documents;

namespace JiraTool.Controls.Notification.ViewModels
{
    /// <summary>
    /// 通知视图模型
    /// </summary>
    public class NotificationViewModel : ViewModelBase
    {
        private NotificationData _data;
        private PackIconKind _iconKind;
        private Brush _iconBrush;
        private Visibility _confirmButtonVisibility = Visibility.Collapsed;
        private Visibility _titleVisibility = Visibility.Collapsed;

        /// <summary>
        /// 通知数据
        /// </summary>
        public NotificationData Data
        {
            get => _data;
            set
            {
                if (SetProperty(ref _data, value))
                {
                    // 强制触发属性更新
                    UpdateProperties();
                    
                    // 确保触发所有相关属性的变更通知
                    OnPropertyChanged(nameof(Title));
                    OnPropertyChanged(nameof(Message));
                    OnPropertyChanged(nameof(HasRichContent));
                }
            }
        }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get => _data?.Title ?? string.Empty;
            set
            {
                if (_data != null)
                {
                    _data.Title = value;
                    OnPropertyChanged(nameof(Title));
                    OnPropertyChanged(nameof(TitleVisibility));
                }
            }
        }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message
        {
            get => _data?.Message ?? string.Empty;
            set
            {
                if (_data != null)
                {
                    _data.Message = value;
                    OnPropertyChanged(nameof(Message));
                }
            }
        }

        /// <summary>
        /// 是否包含富文本内容
        /// </summary>
        public bool HasRichContent => _data?.HasRichContent ?? false;

        /// <summary>
        /// 超链接列表
        /// </summary>
        public Dictionary<string, string> Hyperlinks
        {
            get => _data?.Hyperlinks ?? new Dictionary<string, string>();
            set
            {
                if (_data != null)
                {
                    _data.Hyperlinks = value;
                    OnPropertyChanged(nameof(Hyperlinks));
                    OnPropertyChanged(nameof(HasRichContent));
                }
            }
        }

        /// <summary>
        /// 处理超链接点击
        /// </summary>
        /// <param name="url">点击的URL</param>
        public void HandleHyperlinkClick(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            
            // 使用内置CEF打开URL
            try
            {
                // 触发打开URL的事件
                HyperlinkClicked?.Invoke(this, url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开链接失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 超链接点击事件
        /// </summary>
        public event EventHandler<string> HyperlinkClicked;

        /// <summary>
        /// 图标类型
        /// </summary>
        public PackIconKind IconKind
        {
            get => _iconKind;
            private set => SetProperty(ref _iconKind, value);
        }

        /// <summary>
        /// 图标颜色
        /// </summary>
        public Brush IconBrush
        {
            get => _iconBrush;
            private set => SetProperty(ref _iconBrush, value);
        }

        /// <summary>
        /// 确认按钮可见性
        /// </summary>
        public Visibility ConfirmButtonVisibility
        {
            get => _confirmButtonVisibility;
            private set => SetProperty(ref _confirmButtonVisibility, value);
        }

        /// <summary>
        /// 标题可见性
        /// </summary>
        public Visibility TitleVisibility
        {
            get => _titleVisibility;
            private set => SetProperty(ref _titleVisibility, value);
        }

        /// <summary>
        /// 关闭按钮可见性
        /// </summary>
        public Visibility CloseButtonVisibility => _data?.ShowCloseButton == true ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// 关闭命令
        /// </summary>
        public ICommand CloseCommand { get; }

        /// <summary>
        /// 确认命令
        /// </summary>
        public ICommand ConfirmCommand { get; }

        /// <summary>
        /// 通知关闭事件
        /// </summary>
        public event EventHandler<NotificationData> NotificationClosed;

        /// <summary>
        /// 构造函数
        /// </summary>
        public NotificationViewModel()
        {
            CloseCommand = new RelayCommand<object>(_ => Close());
            ConfirmCommand = new RelayCommand<object>(_ => Confirm());
        }
        
        /// <summary>
        /// 带参数的构造函数
        /// </summary>
        /// <param name="data">通知数据</param>
        public NotificationViewModel(NotificationData data) : this()
        {
            Data = data;
        }

        /// <summary>
        /// 更新属性
        /// </summary>
        private void UpdateProperties()
        {
            if (_data == null) return;

            // 设置标题可见性
            TitleVisibility = string.IsNullOrEmpty(_data.Title) ? Visibility.Collapsed : Visibility.Visible;

            // 根据通知类型设置图标和颜色
            switch (_data.Type)
            {
                case NotificationType.Info:
                    IconKind = PackIconKind.Information;
                    IconBrush = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Blue
                    break;
                case NotificationType.Success:
                    IconKind = PackIconKind.CheckCircle;
                    IconBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                    break;
                case NotificationType.Warning:
                    IconKind = PackIconKind.Alert;
                    IconBrush = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                    break;
                case NotificationType.Error:
                    IconKind = PackIconKind.CloseCircle;
                    IconBrush = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                    break;
                case NotificationType.Question:
                    IconKind = PackIconKind.HelpCircle;
                    IconBrush = new SolidColorBrush(Color.FromRgb(156, 39, 176)); // Purple
                    // 对于询问类型的通知，显示确认按钮
                    ConfirmButtonVisibility = Visibility.Visible;
                    break;
            }

            // 触发属性变更通知
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Message));
            OnPropertyChanged(nameof(CloseButtonVisibility));
            OnPropertyChanged(nameof(HasRichContent));
            OnPropertyChanged(nameof(Hyperlinks));
        }
        
        /// <summary>
        /// 强制刷新所有属性
        /// </summary>
        public void RefreshProperties()
        {
            // 触发所有属性的变更通知，确保数据绑定正常工作
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Message));
            OnPropertyChanged(nameof(TitleVisibility));
            OnPropertyChanged(nameof(IconKind));
            OnPropertyChanged(nameof(IconBrush));
            OnPropertyChanged(nameof(ConfirmButtonVisibility));
            OnPropertyChanged(nameof(CloseButtonVisibility));
            OnPropertyChanged(nameof(HasRichContent));
            OnPropertyChanged(nameof(Hyperlinks));
        }

        /// <summary>
        /// 关闭通知
        /// </summary>
        private void Close()
        {
            // 执行关闭回调
            _data?.OnClose?.Invoke(_data);

            // 触发关闭事件
            NotificationClosed?.Invoke(this, _data);
        }

        /// <summary>
        /// 确认操作
        /// </summary>
        private void Confirm()
        {
            // 执行点击回调
            _data?.OnClick?.Invoke(_data);

            // 关闭通知
            Close();
        }
    }
}
