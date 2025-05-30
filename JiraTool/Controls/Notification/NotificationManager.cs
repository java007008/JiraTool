using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using JiraTool.Controls.Notification.ViewModels;

namespace JiraTool.Controls.Notification
{
    /// <summary>
    /// 通知管理器
    /// </summary>
    public static class NotificationManager
    {
        // 活动通知列表
        private static readonly Dictionary<string, DesktopNotificationWindow> _activeNotifications = new Dictionary<string, DesktopNotificationWindow>();
        
        // 通知队列，当所有位置都被占用时，新通知进入队列
        private static readonly Queue<NotificationData> _notificationQueue = new Queue<NotificationData>();
        
        // 最多显示5个通知
        private const int MaxVisibleNotifications = 5;
        
        // 固定的五个位置（位置编号从1到5，1在最下面，5在最上面）
        private static readonly int[] _positions = new int[] { 1, 2, 3, 4, 5 };
        
        // 存储每个通知ID对应的位置编号
        private static readonly Dictionary<string, int> _notificationPositions = new Dictionary<string, int>();
        
        // 存储每个位置编号对应的通知ID，用于快速查找
        private static readonly Dictionary<int, string> _positionNotifications = new Dictionary<int, string>();
        
        /// <summary>
        /// 显示桌面通知
        /// </summary>
        /// <param name="message">通知内容</param>
        /// <param name="title">通知标题</param>
        /// <param name="type">通知类型</param>
        /// <param name="duration">显示时长（毫秒）</param>
        /// <param name="onClose">关闭回调</param>
        /// <param name="onClick">点击回调</param>
        /// <returns>通知ID</returns>
        public static string ShowDesktopNotification(
            string message,
            string title = "",
            NotificationType type = NotificationType.Info,
            int duration = 4000,
            Action<NotificationData> onClose = null,
            Action<NotificationData> onClick = null)
        {
            var data = new NotificationData
            {
                Message = message,
                Title = title,
                Type = type,
                Duration = duration,
                OnClose = onClose,
                OnClick = onClick
            };
            
            return ShowDesktopNotification(data);
        }
        
        /// <summary>
        /// 显示桌面通知
        /// </summary>
        /// <param name="data">通知数据</param>
        /// <returns>通知ID</returns>
        public static string ShowDesktopNotification(NotificationData data)
        {
            // 如果已经有最大数量的通知，则将通知加入队列等待
            if (_activeNotifications.Count >= MaxVisibleNotifications)
            {
                _notificationQueue.Enqueue(data);
                return data.Id;
            }
            
            // 创建并显示通知窗口
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 创建通知窗口
                var window = new DesktopNotificationWindow(data);
                window.NotificationClosed += NotificationWindow_Closed;
                
                // 检查是否是询问类型的通知
                bool isQuestion = data.Type == NotificationType.Question;
                int position;
                
                if (isQuestion)
                {
                    // 询问类型的通知始终放在最底部（位置1）
                    position = 1;
                    
                    // 如果位置1已被占用，则先移动占用它的通知
                    if (_positionNotifications.TryGetValue(1, out string existingId))
                    {
                        // 获取当前占用位置1的通知
                        if (_activeNotifications.TryGetValue(existingId, out var existingWindow))
                        {
                            // 检查占用位置1的是否也是询问类型
                            bool isExistingQuestion = false;
                            if (existingWindow.DataContext is DesktopNotificationViewModel existingViewModel)
                            {
                                isExistingQuestion = existingViewModel.Data.Type == NotificationType.Question;
                            }
                            
                            // 如果不是询问类型，则将其移动到位置2
                            if (!isExistingQuestion)
                            {
                                _positionNotifications.Remove(1);
                                _notificationPositions[existingId] = 2;
                                _positionNotifications[2] = existingId;
                                
                                // 重新调整通知窗口的位置
                                SetNotificationPosition(existingWindow, 2);
                            }
                        }
                    }
                }
                else
                {
                    // 非询问类型的通知从顶部开始放置
                    position = GetNextAvailablePosition();
                    
                    // 如果没有可用位置，则将通知加入队列等待
                    if (position == 0)
                    {
                        _notificationQueue.Enqueue(data);
                        return;
                    }
                }
                
                // 记录通知位置
                _notificationPositions[data.Id] = position;
                _positionNotifications[position] = data.Id;
                
                // 添加到活动通知列表
                _activeNotifications[data.Id] = window;
                
                // 设置通知窗口的位置
                SetNotificationPosition(window, position);
                
                // 显示通知窗口
                window.Show();
                
                // 设置通知窗口的动画和计时器
                if (window.DataContext is DesktopNotificationViewModel viewModel)
                {
                    // 如果不是询问类型的通知，则启动自动关闭计时器
                    if (data.Type != NotificationType.Question && data.Duration > 0)
                    {
                        viewModel.StartTimer();
                    }
                }
            });
            
            return data.Id;
        }
        
        /// <summary>
        /// 设置通知窗口的位置
        /// </summary>
        /// <param name="window">通知窗口</param>
        /// <param name="position">位置编号（1-5）</param>
        private static void SetNotificationPosition(DesktopNotificationWindow window, int position)
        {
            try
            {
                // 保存当前内容和数据
                var content = window.Content;
                var dataContext = window.DataContext;
                var notificationContent = window.NotificationContent?.Content;
                
                // 如果是DesktopNotificationViewModel，保存其数据
                NotificationData notificationData = null;
                if (dataContext is DesktopNotificationViewModel desktopViewModel)
                {
                    notificationData = desktopViewModel.Data;
                }
                
                var workArea = SystemParameters.WorkArea;
                
                // 设置绿色区域的高度范围（根据图片中的绿色区域）
                const int notificationHeight = 120; // 通知窗口的高度
                const int margin = 10; // 距离屏幕边缘的距离
                double greenAreaTop = workArea.Bottom - 600; // 绿色区域的顶部位置
                double greenAreaBottom = workArea.Bottom - 100; // 绿色区域的底部位置
                
                // 计算五个固定位置的坐标
                double availableHeight = greenAreaBottom - greenAreaTop;
                double positionSpacing = availableHeight / 4; // 四个间隔分配五个位置
                
                // 计算位置对应的顶部坐标（位置1在底部，位置5在顶部）
                double top = greenAreaBottom - (position - 1) * positionSpacing - notificationHeight;
                
                // 设置窗口的位置
                window.Top = top;
                window.Left = workArea.Right - window.Width - margin; // 设置为右侧
                
                // 确保内容不丢失
                if (window.Content == null && content != null)
                {
                    window.Content = content;
                }
                
                // 确保 NotificationContent 内容不丢失
                if (window.NotificationContent != null && window.NotificationContent.Content == null && notificationContent != null)
                {
                    window.NotificationContent.Content = notificationContent;
                }
                
                // 确保 DataContext 不丢失
                if (window.DataContext == null && dataContext != null)
                {
                    window.DataContext = dataContext;
                }
                
                // 如果有通知数据，确保内容被正确设置
                if (notificationData != null)
                {
                    // 如果内容为空，重新创建通知控件
                    if (window.NotificationContent != null && window.NotificationContent.Content == null)
                    {
                        window.NotificationContent.Content = new NotificationControl(notificationData);
                    }
                    
                    // 强制刷新属性
                    if (window.DataContext is DesktopNotificationViewModel viewModel && viewModel.NotificationViewModel != null)
                    {
                        viewModel.NotificationViewModel.RefreshProperties();
                        
                        // 确保数据完整
                        if (notificationData != null)
                        {
                            viewModel.NotificationViewModel.Data = notificationData;
                            viewModel.NotificationViewModel.Title = notificationData.Title;
                            viewModel.NotificationViewModel.Message = notificationData.Message;
                            
                            if (notificationData.Hyperlinks != null && notificationData.Hyperlinks.Count > 0)
                            {
                                viewModel.NotificationViewModel.Hyperlinks = new Dictionary<string, string>(notificationData.Hyperlinks);
                            }
                        }
                    }
                }
                
                // 强制刷新布局
                window.UpdateLayout();
                
                // 如果有NotificationContent，确保其内容可见
                if (window.NotificationContent != null && window.NotificationContent.Content is NotificationControl notificationControl)
                {
                    notificationControl.UpdateLayout();
                    
                    // 强制刷新视图模型属性
                    if (notificationControl.ViewModel != null)
                    {
                        notificationControl.ViewModel.RefreshProperties();
                    }
                }
                else if (window.Content is NotificationViewModel contentViewModel)
                {
                    contentViewModel.RefreshProperties();
                }
                
                // 设置窗口为最前面，确保可见
                window.Topmost = true;
                window.Topmost = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"设置通知位置时出错: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取下一个可用的位置
        /// </summary>
        /// <returns>位置编号（1-5）</returns>
        private static int GetNextAvailablePosition()
        {
            // 如果有询问类型的通知，则从位置2开始分配
            bool hasQuestionNotification = false;
            foreach (var kvp in _activeNotifications)
            {
                if (kvp.Value.DataContext is DesktopNotificationViewModel viewModel && 
                    viewModel.Data.Type == NotificationType.Question)
                {
                    hasQuestionNotification = true;
                    break;
                }
            }
            
            // 从位置5开始向下查找第一个可用的位置
            for (int i = 5; i >= (hasQuestionNotification ? 2 : 1); i--)
            {
                if (!_positionNotifications.ContainsKey(i))
                {
                    return i;
                }
            }
            
            return 0; // 没有可用位置
        }
        
        /// <summary>
        /// 关闭通知
        /// </summary>
        /// <param name="id">通知ID</param>
        public static void CloseNotification(string id)
        {
            if (_activeNotifications.TryGetValue(id, out var window))
            {
                window.Close();
            }
        }
        
        /// <summary>
        /// 关闭所有通知
        /// </summary>
        public static void CloseAllNotifications()
        {
            foreach (var window in _activeNotifications.Values.ToList())
            {
                window.Close();
            }
            
            _notificationQueue.Clear();
        }
        
        /// <summary>
        /// 通知窗口关闭回调
        /// </summary>
        private static void NotificationWindow_Closed(object sender, NotificationData e)
        {
            if (sender is DesktopNotificationWindow window)
            {
                // 删除活动通知
                _activeNotifications.Remove(e.Id);
                
                // 获取关闭的通知位置
                if (!_notificationPositions.TryGetValue(e.Id, out int closedPosition))
                {
                    // 如果找不到关闭的通知位置，可能是通知已经被关闭过了
                    return;
                }
                
                // 删除通知位置映射
                _notificationPositions.Remove(e.Id);
                _positionNotifications.Remove(closedPosition);
                
                // 触发关闭回调
                e.OnClose?.Invoke(e);
                
                // 重新整理所有通知的位置，确保位置连续且没有空位
                AdjustNotificationPositions();
                
                // 如果队列中有等待的通知，则显示下一个
                if (_notificationQueue.Count > 0)
                {
                    var nextData = _notificationQueue.Dequeue();
                    ShowDesktopNotification(nextData);
                }
            }
        }
        
        /// <summary>
        /// 调整通知位置
        /// </summary>
        private static void AdjustNotificationPositions()
        {
            try
            {
                // 先保存所有通知的内容
                var contentBackup = new Dictionary<string, object>();
                foreach (var kvp in _activeNotifications)
                {
                    string notificationId = kvp.Key;
                    var window = kvp.Value;
                    if (window.Content != null)
                    {
                        contentBackup[notificationId] = window.Content;
                    }
                }
                
                // 清理位置映射，确保没有无效的映射
                CleanupPositionMappings();
                
                // 重新分配所有通知的位置
                ReassignNotificationPositions();
                
                // 更新所有通知窗口的位置
                var workArea = SystemParameters.WorkArea;
                const int margin = 10; // 距离屏幕边缘的距离
                
                // 遍历所有活动通知，根据其分配的位置设置坐标
                foreach (var kvp in _activeNotifications)
                {
                    string notificationId = kvp.Key;
                    var window = kvp.Value;
                    
                    // 获取通知的位置
                    if (_notificationPositions.TryGetValue(notificationId, out int position))
                    {
                        // 设置通知窗口的位置
                        SetNotificationPosition(window, position);
                        
                        // 确保内容不丢失
                        if (window.Content == null && contentBackup.TryGetValue(notificationId, out var content) && content != null)
                        {
                            window.Content = content;
                            
                            // 强制刷新属性
                            if (content is NotificationViewModel viewModel)
                            {
                                viewModel.RefreshProperties();
                            }
                        }
                        
                        // 设置窗口的OnScreenPosition属性（动画结束位置）
                        if (window.DataContext is DesktopNotificationViewModel desktopViewModel)
                        {
                            desktopViewModel.OnScreenPosition = workArea.Right - window.Width - margin; // 设置为右侧
                        }
                        
                        // 如果窗口已经显示（不在动画中），直接调整位置
                        if (window.Opacity > 0.9)
                        {
                            window.Left = workArea.Right - window.Width - margin; // 设置为右侧
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"调整通知位置时出错: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 清理位置映射，移除无效的映射
        /// </summary>
        private static void CleanupPositionMappings()
        {
            // 清理通知位置映射
            var invalidNotificationIds = _notificationPositions.Keys
                .Where(id => !_activeNotifications.ContainsKey(id))
                .ToList();
            
            foreach (var id in invalidNotificationIds)
            {
                if (_notificationPositions.TryGetValue(id, out int position))
                {
                    _positionNotifications.Remove(position);
                }
                
                _notificationPositions.Remove(id);
            }
            
            // 清理位置通知映射
            var invalidPositions = _positionNotifications.Keys
                .Where(position => !_activeNotifications.ContainsKey(_positionNotifications[position]))
                .ToList();
            
            foreach (var position in invalidPositions)
            {
                _positionNotifications.Remove(position);
            }
            
            // 强制刷新所有活动通知的内容
            foreach (var kvp in _activeNotifications)
            {
                var window = kvp.Value;
                if (window.Content is NotificationViewModel viewModel)
                {
                    viewModel.RefreshProperties();
                }
            }
        }
        
        /// <summary>
        /// 重新分配所有通知的位置
        /// </summary>
        private static void ReassignNotificationPositions()
        {
            // 清空位置映射
            _notificationPositions.Clear();
            _positionNotifications.Clear();
            
            // 将所有活动通知按类型分组
            var questionNotifications = new List<string>();
            var normalNotifications = new List<string>();
            
            foreach (var kvp in _activeNotifications)
            {
                string notificationId = kvp.Key;
                var window = kvp.Value;
                
                // 检查是否是询问类型通知
                bool isQuestion = false;
                if (window.DataContext is DesktopNotificationViewModel viewModel)
                {
                    isQuestion = viewModel.Data.Type == NotificationType.Question;
                }
                
                if (isQuestion)
                {
                    questionNotifications.Add(notificationId);
                }
                else
                {
                    normalNotifications.Add(notificationId);
                }
            }
            
            // 按照从下到上的顺序分配位置（位置1在最底部，位置5在最顶部）
            int position = 1;
            
            // 先处理询问类型的通知，放在底部
            foreach (var notificationId in questionNotifications)
            {
                if (position <= 5)
                {
                    _notificationPositions[notificationId] = position;
                    _positionNotifications[position] = notificationId;
                    position++;
                }
            }
            
            // 再处理普通通知，放在上面
            foreach (var notificationId in normalNotifications)
            {
                if (position <= 5)
                {
                    _notificationPositions[notificationId] = position;
                    _positionNotifications[position] = notificationId;
                    position++;
                }
            }
        }
        
        /// <summary>
        /// 更新所有通知窗口的位置
        /// </summary>
        private static void UpdateAllNotificationPositions()
        {
            try
            {
                // 先保存所有通知的内容和数据
                var contentBackup = new Dictionary<string, object>();
                var dataBackup = new Dictionary<string, NotificationData>();
                var viewModelBackup = new Dictionary<string, NotificationViewModel>();
                
                foreach (var kvp in _activeNotifications)
                {
                    string notificationId = kvp.Key;
                    var window = kvp.Value;
                    
                    // 保存内容
                    if (window.Content != null)
                    {
                        contentBackup[notificationId] = window.Content;
                        
                        // 保存视图模型和数据
                        if (window.Content is NotificationViewModel contentViewModel)
                        {
                            viewModelBackup[notificationId] = contentViewModel;
                            if (contentViewModel.Data != null)
                            {
                                dataBackup[notificationId] = contentViewModel.Data;
                            }
                        }
                    }
                    
                    // 如果窗口是DesktopNotificationWindow类型，保存其Data
                    if (window is DesktopNotificationWindow notificationWindow && notificationWindow.DataContext is DesktopNotificationViewModel viewModel && viewModel.Data != null)
                    {
                        dataBackup[notificationId] = viewModel.Data;
                    }
                }
                
                // 更新所有通知的位置
                foreach (var kvp in _activeNotifications)
                {
                    string notificationId = kvp.Key;
                    var window = kvp.Value;
                    
                    if (_notificationPositions.TryGetValue(notificationId, out int position))
                    {
                        // 先保存当前内容
                        var currentContent = window.Content;
                        
                        // 设置位置
                        SetNotificationPosition(window, position);
                        
                        // 确保内容不丢失
                        if (window.Content == null || (window.Content is NotificationViewModel vm && string.IsNullOrEmpty(vm.Message)))
                        {
                            // 尝试恢复内容
                            if (contentBackup.TryGetValue(notificationId, out var content) && content != null)
                            {
                                window.Content = content;
                                
                                // 强制刷新属性
                                if (content is NotificationViewModel notificationViewModel)
                                {
                                    notificationViewModel.RefreshProperties();
                                    
                                    // 确保数据完整
                                    if (dataBackup.TryGetValue(notificationId, out var data))
                                    {
                                        notificationViewModel.Data = data;
                                        notificationViewModel.Title = data.Title;
                                        notificationViewModel.Message = data.Message;
                                        if (data.Hyperlinks != null && data.Hyperlinks.Count > 0)
                                        {
                                            notificationViewModel.Hyperlinks = new Dictionary<string, string>(data.Hyperlinks);
                                        }
                                    }
                                }
                            }
                            // 如果内容仍然为空，尝试创建新的视图模型
                            else if (dataBackup.TryGetValue(notificationId, out var data))
                            {
                                var newViewModel = new NotificationViewModel
                                {
                                    Data = data,
                                    Title = data.Title,
                                    Message = data.Message
                                };
                                
                                if (data.Hyperlinks != null && data.Hyperlinks.Count > 0)
                                {
                                    newViewModel.Hyperlinks = new Dictionary<string, string>(data.Hyperlinks);
                                }
                                
                                window.Content = newViewModel;
                                newViewModel.RefreshProperties();
                            }
                        }
                        // 如果窗口是DesktopNotificationWindow类型，确保其内容被正确初始化
                        if (window is DesktopNotificationWindow notificationWindow && notificationWindow.DataContext is DesktopNotificationViewModel desktopViewModel)
                        {
                            // 强制触发属性变更通知
                            if (desktopViewModel.Data != null && desktopViewModel.NotificationViewModel != null)
                            {
                                // 使用NotificationViewModel属性来更新内容
                                var notificationVM = desktopViewModel.NotificationViewModel;
                                notificationVM.RefreshProperties();
                                
                                // 确保数据完整
                                if (dataBackup.TryGetValue(notificationId, out var data))
                                {
                                    notificationVM.Data = data;
                                    notificationVM.Title = data.Title;
                                    notificationVM.Message = data.Message;
                                    if (data.Hyperlinks != null && data.Hyperlinks.Count > 0)
                                    {
                                        notificationVM.Hyperlinks = new Dictionary<string, string>(data.Hyperlinks);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新通知位置时出错: {ex.Message}");
            }
        }
        
        #region 快捷方法
        
        /// <summary>
        /// 显示信息通知
        /// </summary>
        public static string Info(string message, string title = "", int duration = 4000, Action<NotificationData> onClose = null, Action<NotificationData> onClick = null)
        {
            return ShowDesktopNotification(message, title, NotificationType.Info, duration, onClose, onClick);
        }
        
        /// <summary>
        /// 显示成功通知
        /// </summary>
        public static string Success(string message, string title = "", int duration = 4000, Action<NotificationData> onClose = null, Action<NotificationData> onClick = null)
        {
            return ShowDesktopNotification(message, title, NotificationType.Success, duration, onClose, onClick);
        }
        
        /// <summary>
        /// 显示警告通知
        /// </summary>
        public static string Warning(string message, string title = "", int duration = 4000, Action<NotificationData> onClose = null, Action<NotificationData> onClick = null)
        {
            return ShowDesktopNotification(message, title, NotificationType.Warning, duration, onClose, onClick);
        }
        
        /// <summary>
        /// 显示错误通知
        /// </summary>
        public static string Error(string message, string title = "", int duration = 4000, Action<NotificationData> onClose = null, Action<NotificationData> onClick = null)
        {
            return ShowDesktopNotification(message, title, NotificationType.Error, duration, onClose, onClick);
        }
        
        /// <summary>
        /// 显示询问通知
        /// </summary>
        public static string Question(string message, string title = "", int duration = 0, Action<NotificationData> onClose = null, Action<NotificationData> onClick = null)
        {
            // 询问类型的通知默认不会自动消失（duration=0）
            return ShowDesktopNotification(message, title, NotificationType.Question, duration, onClose, onClick);
        }
        
        #endregion
    }
}
