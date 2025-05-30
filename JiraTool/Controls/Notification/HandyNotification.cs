using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using JiraTool.Controls.Notification.ViewModels;

namespace JiraTool.Controls.Notification
{
    /// <summary>
    /// 仿 HandyControl 的通知窗口
    /// </summary>
    public sealed class HandyNotification : Window
    {
        private const int WaitTime = 6; // 默认等待时间（秒）

        /// <summary>
        /// 计数
        /// </summary>
        private int _tickCount;

        /// <summary>
        /// 关闭计时器
        /// </summary>
        private DispatcherTimer _timerClose;

        /// <summary>
        /// 显示动画类型
        /// </summary>
        public ShowAnimation ShowAnimation { get; set; }

        /// <summary>
        /// 是否应该关闭
        /// </summary>
        private bool _shouldBeClosed;

        /// <summary>
        /// 通知数据
        /// </summary>
        public NotificationData Data { get; private set; }

        /// <summary>
        /// 通知关闭事件
        /// </summary>
        public event EventHandler<NotificationData> NotificationClosed;
        
        /// <summary>
        /// 设置确认操作回调
        /// </summary>
        /// <param name="action">确认操作回调</param>
        public void SetConfirmAction(Action<NotificationData> action)
        {
            if (Data != null)
            {
                Data.OnClick = action;
            }
        }
        
        /// <summary>
        /// 设置取消操作回调
        /// </summary>
        /// <param name="action">取消操作回调</param>
        public void SetCancelAction(Action<NotificationData> action)
        {
            if (Data != null)
            {
                Data.OnClose = action;
            }
        }
        
        /// <summary>
        /// 触发超链接点击事件
        /// </summary>
        /// <param name="url">点击的URL</param>
        protected void OnHyperlinkClicked(string url)
        {
            HyperlinkClicked?.Invoke(this, url);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public HandyNotification()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            ShowInTaskbar = false;
            Topmost = true;
            Width = 360; // 固定宽度
            Height = 120; // 固定高度
            SizeToContent = SizeToContent.Manual; // 不自适应大小
            ResizeMode = ResizeMode.NoResize;
            Background = System.Windows.Media.Brushes.Transparent; // 设置窗口背景为透明
            BorderThickness = new Thickness(0); // 移除边框
            Padding = new Thickness(0); // 移除内边距
            Margin = new Thickness(0); // 移除外边距
            
            // 设置模板
            ContentTemplate = (DataTemplate)Application.Current.Resources["HandyNotificationTemplate"];
            
            // 加载完成后初始化超链接
            Loaded += HandyNotification_Loaded;
        }

        /// <summary>
        /// 显示通知
        /// </summary>
        /// <param name="data">通知数据</param>
        /// <param name="showAnimation">显示动画</param>
        /// <param name="staysOpen">是否保持打开</param>
        /// <returns>通知窗口</returns>
        public static HandyNotification Show(NotificationData data, ShowAnimation showAnimation = ShowAnimation.Fade, bool staysOpen = false)
        {
            var notification = new HandyNotification
            {
                Data = data,
                ShowAnimation = showAnimation,
                Content = new NotificationViewModel(data)
            };
            
            // 确保通知内容被正确初始化
            if (notification.Content is NotificationViewModel viewModel)
            {
                viewModel.Title = data.Title;
                viewModel.Message = data.Message;
                
                if (data.Hyperlinks != null && data.Hyperlinks.Count > 0)
                {
                    viewModel.Hyperlinks = new Dictionary<string, string>(data.Hyperlinks);
                }
                
                viewModel.RefreshProperties();
            }
            
            notification.Show();
            
            // 强制刷新布局
            notification.UpdateLayout();
            
            var desktopWorkingArea = SystemParameters.WorkArea;
            var leftMax = desktopWorkingArea.Width - notification.ActualWidth;
            var topMax = desktopWorkingArea.Height - notification.ActualHeight;

            // 根据动画类型设置初始位置和动画
            switch (showAnimation)
            {
                case ShowAnimation.None:
                    notification.Opacity = 1;
                    notification.Left = leftMax;
                    notification.Top = topMax;
                    break;
                case ShowAnimation.HorizontalMove:
                    notification.Opacity = 1;
                    notification.Left = desktopWorkingArea.Width;
                    notification.Top = topMax;
                    notification.BeginAnimation(LeftProperty, CreateAnimation(leftMax));
                    break;
                case ShowAnimation.VerticalMove:
                    notification.Opacity = 1;
                    notification.Left = leftMax;
                    notification.Top = desktopWorkingArea.Height;
                    notification.BeginAnimation(TopProperty, CreateAnimation(topMax));
                    break;
                case ShowAnimation.Fade:
                    notification.Left = leftMax;
                    notification.Top = topMax;
                    notification.BeginAnimation(OpacityProperty, CreateAnimation(1));
                    break;
                default:
                    notification.Opacity = 1;
                    notification.Left = leftMax;
                    notification.Top = topMax;
                    break;
            }

            // 如果不是保持打开，则启动自动关闭计时器
            if (!staysOpen && data.Duration > 0)
            {
                notification.StartTimer();
            }

            return notification;
        }

        /// <summary>
        /// 创建动画
        /// </summary>
        /// <param name="toValue">目标值</param>
        /// <returns>动画</returns>
        private static DoubleAnimation CreateAnimation(double toValue)
        {
            return new DoubleAnimation(toValue, new Duration(TimeSpan.FromMilliseconds(300)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
        }

        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_shouldBeClosed)
            {
                // 触发通知关闭事件
                NotificationClosed?.Invoke(this, Data);
                return;
            }

            // 获取工作区域
            var desktopWorkingArea = SystemParameters.WorkArea;

            // 根据动画类型设置关闭动画
            switch (ShowAnimation)
            {
                case ShowAnimation.None:
                    // 触发通知关闭事件
                    NotificationClosed?.Invoke(this, Data);
                    break;
                case ShowAnimation.HorizontalMove:
                    {
                        var animation = CreateAnimation(desktopWorkingArea.Width);
                        animation.Completed += Animation_Completed;
                        BeginAnimation(LeftProperty, animation);
                        e.Cancel = true;
                        _shouldBeClosed = true;
                    }
                    break;
                case ShowAnimation.VerticalMove:
                    {
                        var animation = CreateAnimation(desktopWorkingArea.Height);
                        animation.Completed += Animation_Completed;
                        BeginAnimation(TopProperty, animation);
                        e.Cancel = true;
                        _shouldBeClosed = true;
                    }
                    break;
                case ShowAnimation.Fade:
                    {
                        var animation = CreateAnimation(0);
                        animation.Completed += Animation_Completed;
                        BeginAnimation(OpacityProperty, animation);
                        e.Cancel = true;
                        _shouldBeClosed = true;
                    }
                    break;
            }
        }

        /// <summary>
        /// 动画完成事件
        /// </summary>
        private void Animation_Completed(object sender, EventArgs e)
        {
            _shouldBeClosed = false;
            Close();
        }

        /// <summary>
        /// 开始计时器
        /// </summary>
        private void StartTimer()
        {
            _timerClose = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timerClose.Tick += delegate
            {
                if (IsMouseOver)
                {
                    _tickCount = 0;
                    return;
                }

                _tickCount++;
                if (_tickCount >= WaitTime)
                {
                    Close();
                }
            };
            _timerClose.Start();
        }
        
/// <summary>
/// 窗口加载完成事件
/// </summary>
private void HandyNotification_Loaded(object sender, RoutedEventArgs e)
{
    try
    {
        if (Content is NotificationViewModel viewModel)
        {
            // 强制触发属性更新，确保数据绑定正常工作
            viewModel.RefreshProperties();
            
            // 确保标题和消息内容被设置
            if (Data != null)
            {
                viewModel.Title = Data.Title;
                viewModel.Message = Data.Message;
                
                if (Data.Hyperlinks != null && Data.Hyperlinks.Count > 0)
                {
                    viewModel.Hyperlinks = new Dictionary<string, string>(Data.Hyperlinks);
                }
            }
            
            // 如果有超链接，初始化RichTextBox
            if (viewModel.HasRichContent)
            {
                InitializeRichTextWithHyperlinks(viewModel);
                
                // 注册超链接点击事件
                viewModel.HyperlinkClicked += ViewModel_HyperlinkClicked;
            }
            else
            {
                // 对于普通通知，直接设置文本内容
                InitializeTextContent(viewModel);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"通知加载时出错: {ex.Message}");
    }
}

/// <summary>
/// 初始化普通通知的文本内容
/// </summary>
private void InitializeTextContent(NotificationViewModel viewModel)
{
    try
    {
        // 查找文本块
        var textBlock = FindTextBlock(this);
        if (textBlock == null)
        {
            Console.WriteLine("未找到TextBlock元素");
            return;
        }
        
        // 直接设置文本内容
        if (!string.IsNullOrEmpty(viewModel.Message))
        {
            textBlock.Text = viewModel.Message;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Foreground = new SolidColorBrush(Color.FromRgb(221, 221, 221)); // #DDDDDD
        }
        else if (Data != null && !string.IsNullOrEmpty(Data.Message))
        {
            // 如果视图模型中的消息为空，则使用Data中的消息
            textBlock.Text = Data.Message;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Foreground = new SolidColorBrush(Color.FromRgb(221, 221, 221)); // #DDDDDD
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"初始化文本内容时出错: {ex.Message}");
    }
}

private void InitializeRichTextWithHyperlinks(NotificationViewModel viewModel)
{
    // 查找富文本框
    var richTextBox = FindRichTextBox(this);
    if (richTextBox == null) return;
    
    // 创建文档
    var document = new FlowDocument();
    var paragraph = new Paragraph();
    
    // 设置超链接点击事件
    viewModel.HyperlinkClicked += (sender, url) => OnHyperlinkClicked(url);
    
    // 处理消息文本，并插入超链接
    string message = viewModel.Message;
    
    // 遍历所有超链接
    foreach (var link in viewModel.Hyperlinks)
    {
        string linkText = link.Key;
        string url = link.Value;
        
        // 如果消息中包含链接文本
        int index = message.IndexOf(linkText);
        if (index >= 0)
        {
            // 添加链接前的文本
            if (index > 0)
            {
                paragraph.Inlines.Add(new Run(message.Substring(0, index)));
            }
            
            // 添加超链接
            var hyperlink = new Hyperlink(new Run(linkText))
            {
                Tag = url
            };
            hyperlink.Click += Hyperlink_Click;
            paragraph.Inlines.Add(hyperlink);
            
            // 更新消息，移除已处理的部分
            message = message.Substring(index + linkText.Length);
        }
    }
    
    // 添加剩余文本
    if (!string.IsNullOrEmpty(message))
    {
        paragraph.Inlines.Add(new Run(message));
    }
    
    document.Blocks.Add(paragraph);
    richTextBox.Document = document;
    
    // 添加文档加载完成事件处理
    richTextBox.Loaded += (s, e) => {
        // 为所有超链接添加点击事件
        foreach (var block in richTextBox.Document.Blocks)
        {
            if (block is Paragraph para)
            {
                foreach (var inline in para.Inlines)
                {
                    if (inline is Hyperlink link && link.Tag is string)
                    {
                        link.Click += Hyperlink_Click;
                    }
                }
            }
        }
    };
        }
        
        /// <summary>
        /// 递归查找富文本框
        /// </summary>
        private RichTextBox FindRichTextBox(DependencyObject parent)
        {
            // 安全检查
            if (parent == null) return null;
            
            // 先检查当前元素
            if (parent is RichTextBox richTextBox && richTextBox.Name == "RichTextContent")
            {
                return richTextBox;
            }

            // 获取子元素数量
            int childCount = 0;
            try
            {
                childCount = VisualTreeHelper.GetChildrenCount(parent);
            }
            catch
            {
                // 如果发生异常，可能是可视化树还没有构建完成
                return null;
            }
            
            // 递归查找子元素
            RichTextBox result = null;
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                
                // 递归查找
                result = FindRichTextBox(child);
                if (result != null)
                {
                    return result;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 查找普通文本块
        /// </summary>
        private TextBlock FindTextBlock(DependencyObject parent)
        {
            // 安全检查
            if (parent == null) return null;
            
            // 检查当前元素是否为TextBlock
            if (parent is TextBlock textBlock)
            {
                return textBlock;
            }
            
            // 获取子元素数量
            int childCount = 0;
            try
            {
                childCount = VisualTreeHelper.GetChildrenCount(parent);
            }
            catch
            {
                // 如果发生异常，可能是可视化树还没有构建完成
                return null;
            }
            
            if (childCount == 0) return null;

            // 递归查找子元素
            TextBlock result = null;
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                
                // 递归查找
                result = FindTextBlock(child);
                if (result != null)
                {
                    return result;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 超链接点击事件
        /// </summary>
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink hyperlink && hyperlink.Tag is string url)
            {
                // 如果内容是通知视图模型，触发超链接点击事件
                if (Content is NotificationViewModel viewModel)
                {
                    viewModel.HandleHyperlinkClick(url);
                }
            }
        }
        
        /// <summary>
        /// 视图模型超链接点击事件
        /// </summary>
        private void ViewModel_HyperlinkClicked(object sender, string url)
        {
            HyperlinkClicked?.Invoke(this, url);
        }
        
        /// <summary>
        /// 初始化通知内容
        /// </summary>
        public void InitializeContent()
        {
            try
            {
                if (Content is NotificationViewModel viewModel)
                {
                    // 确保数据完整
                    if (Data != null)
                    {
                        viewModel.Data = Data;
                        viewModel.Title = Data.Title;
                        viewModel.Message = Data.Message;
                        
                        if (Data.Hyperlinks != null && Data.Hyperlinks.Count > 0)
                        {
                            viewModel.Hyperlinks = new Dictionary<string, string>(Data.Hyperlinks);
                        }
                    }
                    
                    // 强制刷新属性
                    viewModel.RefreshProperties();
                    
                    // 根据内容类型初始化
                    if (viewModel.HasRichContent)
                    {
                        InitializeRichTextWithHyperlinks(viewModel);
                    }
                    else
                    {
                        InitializeTextContent(viewModel);
                    }
                }
                
                // 强制刷新布局
                UpdateLayout();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化通知内容时出错: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 确保通知内容始终可见
        /// </summary>
        public void EnsureContentVisible()
        {
            try
            {
                // 如果内容为空，重新初始化
                if (Content == null && Data != null)
                {
                    Content = new NotificationViewModel(Data);
                    InitializeContent();
                }
                else if (Content is NotificationViewModel viewModel)
                {
                    // 检查消息是否为空
                    if (string.IsNullOrEmpty(viewModel.Message) && Data != null)
                    {
                        viewModel.Message = Data.Message;
                        viewModel.Title = Data.Title;
                        
                        if (Data.Hyperlinks != null && Data.Hyperlinks.Count > 0)
                        {
                            viewModel.Hyperlinks = new Dictionary<string, string>(Data.Hyperlinks);
                        }
                        
                        viewModel.RefreshProperties();
                        
                        // 重新初始化内容
                        if (viewModel.HasRichContent)
                        {
                            InitializeRichTextWithHyperlinks(viewModel);
                        }
                        else
                        {
                            InitializeTextContent(viewModel);
                        }
                    }
                }
                
                // 强制刷新布局
                UpdateLayout();
                
                // 设置窗口为最前面
                Topmost = true;
                Topmost = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"确保通知内容可见时出错: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 超链接点击事件
        /// </summary>
        public event EventHandler<string> HyperlinkClicked;
    }

    /// <summary>
    /// 显示动画类型
    /// </summary>
    public enum ShowAnimation
    {
        /// <summary>
        /// 无动画
        /// </summary>
        None,

        /// <summary>
        /// 水平移动
        /// </summary>
        HorizontalMove,

        /// <summary>
        /// 垂直移动
        /// </summary>
        VerticalMove,

        /// <summary>
        /// 淡入淡出
        /// </summary>
        Fade
    }
}
