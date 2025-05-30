using JiraTool.Models;
using JiraTool.Services;
using JiraTool.Services.Interfaces;
using JiraTool.ViewModels.Base;
using System;
using System.Windows;
using System.Windows.Input;
using JiraTool.Controls.Notification;

namespace JiraTool.ViewModels
{
    /// <summary>
    /// 主窗口视图模型
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly INotificationService _notificationService;
        private readonly ITrayIconService _trayIconService;
        private readonly DatabaseService _databaseService;
        private readonly IJiraService _jiraService;
        private readonly IJiraLoginService _jiraLoginService;
        private readonly IJiraDataScraperService _jiraDataScraperService;

        private ViewModelBase _currentViewModel;
        private bool _isLoggedIn = false;
        private string _statusMessage = "未登录";
        private bool _isLoading = false;
        private double _windowWidth = 1280;
        private double _windowHeight = 720;

        /// <summary>
        /// 当前显示的子视图模型
        /// </summary>
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        /// <summary>
        /// 用户是否已登录
        /// </summary>
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }

        /// <summary>
        /// 状态消息
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// 是否正在加载数据
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// 窗口宽度
        /// </summary>
        public double WindowWidth
        {
            get => _windowWidth;
            set => SetProperty(ref _windowWidth, value);
        }

        /// <summary>
        /// 窗口高度
        /// </summary>
        public double WindowHeight
        {
            get => _windowHeight;
            set => SetProperty(ref _windowHeight, value);
        }

        /// <summary>
        /// 登录命令
        /// </summary>
        public ICommand LoginCommand { get; }

        /// <summary>
        /// 登出命令
        /// </summary>
        public ICommand LogoutCommand { get; }

        /// <summary>
        /// 退出命令
        /// </summary>
        public ICommand ExitCommand { get; }

        /// <summary>
        /// 窗口加载命令
        /// </summary>
        public ICommand WindowLoadedCommand { get; }
        
        /// <summary>
        /// 打开浏览器命令
        /// </summary>
        public ICommand OpenBrowserCommand { get; }

        /// <summary>
        /// 窗口关闭命令
        /// </summary>
        public ICommand WindowClosingCommand { get; }

        /// <summary>
        /// 窗口大小改变命令
        /// </summary>
        public ICommand WindowSizeChangedCommand { get; }

        /// <summary>
        /// 最小化命令
        /// </summary>
        public ICommand MinimizeCommand { get; }

        /// <summary>
        /// 窗口状态改变命令
        /// </summary>
        public ICommand WindowStateChangedCommand { get; }

        /// <summary>
        /// 测试班车号变更通知命令
        /// </summary>
        public ICommand TestBusNotificationCommand { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="notificationService">通知服务</param>
        /// <param name="trayIconService">托盘服务</param>
        /// <param name="databaseService">数据库服务</param>
        /// <param name="jiraService">Jira服务</param>
        /// <param name="loginViewModel">登录视图模型</param>
        /// <param name="taskListViewModel">任务列表视图模型</param>
        /// <param name="testViewModel">测试视图模型</param>
        public MainViewModel(
            INotificationService notificationService,
            ITrayIconService trayIconService,
            DatabaseService databaseService,
            IJiraService jiraService,
            IJiraLoginService jiraLoginService,
            IJiraDataScraperService jiraDataScraperService,
            LoginViewModel loginViewModel,
            TaskListViewModel taskListViewModel,
            TestViewModel testViewModel)
        {
            _notificationService = notificationService;
            _trayIconService = trayIconService;
            _databaseService = databaseService;
            _jiraService = jiraService;
            _jiraLoginService = jiraLoginService;
            _jiraDataScraperService = jiraDataScraperService;

            // 订阅登录成功事件
            _jiraLoginService.LoginSuccessful += OnJiraLoginSuccessful;

            // 初始化命令
            LoginCommand = new RelayCommand<object>(OnLogin);
            LogoutCommand = new RelayCommand<object>(OnLogout, _ => IsLoggedIn);
            ExitCommand = new RelayCommand<object>(_ => Application.Current.Shutdown());
            WindowLoadedCommand = new RelayCommand<object>(OnWindowLoaded);
            WindowClosingCommand = new RelayCommand<object>(OnWindowClosing);
            WindowSizeChangedCommand = new RelayCommand<System.Windows.SizeChangedEventArgs>(OnWindowSizeChanged);
            MinimizeCommand = new RelayCommand<Window>(w => w.WindowState = WindowState.Minimized);
            WindowStateChangedCommand = new RelayCommand<Window>(OnWindowStateChanged);
            TestBusNotificationCommand = new RelayCommand<object>(_ => ShowBusChangeNotification());
            OpenBrowserCommand = new RelayCommand<string>(OnOpenBrowser);

            // 初始化托盘图标
            _trayIconService.Initialize();
            _trayIconService.ShowWindowRequested += (_, _) => OnShowWindow();
            _trayIconService.HideWindowRequested += (_, _) => OnHideWindow();
            _trayIconService.ExitRequested += (_, _) => Application.Current.Shutdown();

            //// 设置初始视图模型 - 直接显示任务列表视图
            //IsLoggedIn = true; // 模拟已登录状态
            //StatusMessage = "已登录: 模拟用户";
            //CurrentViewModel = taskListViewModel;

            //// 订阅登录视图模型的事件
            //loginViewModel.LoginSuccessful += (_, _) =>
            //{
            //    IsLoggedIn = true;
            //    StatusMessage = $"已登录: {_jiraService.Username}";
            //    CurrentViewModel = taskListViewModel;
            //};

            // 加载设置
            LoadSettings();
        }

        /// <summary>
        /// Jira登录成功事件处理
        /// </summary>
        private async void OnJiraLoginSuccessful(object? sender, EventArgs e)
        {
            IsLoggedIn = true;
            StatusMessage = "已登录";

            // 重新加载窗口，初始化任务列表
            await LoadSettings();

            // 如果配置了URL，自动启动抽取任务
            var config = new AppConfigService().GetConfig();
            if (!string.IsNullOrEmpty(config.MainTaskListUrl) && !string.IsNullOrEmpty(config.SubTaskListUrl))
            {
                await _jiraDataScraperService.StartScrapingTask(
                    config.MainTaskListUrl,
                    config.SubTaskListUrl,
                    config.RefreshInterval);
            }
        }

        /// <summary>
        /// 登录处理
        /// </summary>
        private async void OnLogin(object? _)
        {
            // 使用IJiraLoginService检查登录状态
            var isLoggedIn = await _jiraLoginService.IsLoggedIn();
            if (isLoggedIn)
            {
                _notificationService.ShowInformation("提示", "您已经登录");
                return;
            }

            // 直接使用CEF打开登录页面
            try
            {
                StatusMessage = "正在打开登录页面...";

                // 从配置中获取BaseUrl
                var config = new AppConfigService().GetConfig();
                string baseUrl = config.BaseUrl;

                if (string.IsNullOrEmpty(baseUrl))
                {
                    _notificationService.ShowError("登录失败", "请先在设置中配置Jira基础URL");
                    return;
                }

                // 打开登录页面
                var success = await _jiraLoginService.OpenLoginPage(baseUrl);

                if (success)
                {
                    StatusMessage = "登录成功";
                    IsLoggedIn = true;
                }
                else
                {
                    StatusMessage = "登录取消或失败";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "登录失败";
                _notificationService.ShowError("登录失败", ex.Message);
            }
            // 登录成功后，会在JiraLoginService中触发事件，并在那里更新状态
        }

        /// <summary>
        /// 登出处理
        /// </summary>
        private async void OnLogout(object? _)
        {
            if (!_jiraService.IsLoggedIn) return;

            var result = await _notificationService.ShowConfirmationAsync("确认", "确定要退出登录吗？");
            if (result)
            {
                await _jiraService.LogoutAsync();
                IsLoggedIn = false;
                StatusMessage = "未登录";
                CurrentViewModel = App.AppHost?.Services.GetService(typeof(LoginViewModel)) as ViewModelBase
                    ?? throw new InvalidOperationException("无法获取登录视图模型");
            }
        }

        /// <summary>
        /// 窗口加载处理
        /// </summary>
        private async void OnWindowLoaded(object? _)
        {
            // 加载用户设置
            await LoadSettings();

            // 设置托盘图标提示文本
            _trayIconService.SetToolTip("Jira客户端工具");

            // 切换到任务列表视图，无论是否登录都加载任务列表
            var taskListViewModel = App.AppHost?.Services.GetService(typeof(TaskListViewModel)) as TaskListViewModel
                ?? throw new InvalidOperationException("无法获取任务列表视图模型");

            // 设置任务列表视图的登录状态
            if (taskListViewModel is ILoginAwareViewModel loginAware)
            {
                loginAware.IsLoggedIn = IsLoggedIn;
            }

            CurrentViewModel = taskListViewModel;

            // 读取配置文件中的Jira URL
            var config = new AppConfigService().GetConfig();
            if (!string.IsNullOrEmpty(config.BaseUrl))
            {
                try
                {
                    // 尝试检查是否已登录
                    var isLoggedIn = await _jiraLoginService.IsLoggedIn();

                    if (isLoggedIn)
                    {
                        StatusMessage = "已登录";
                        IsLoggedIn = true;

                        // 更新任务列表视图的登录状态
                        if (taskListViewModel is ILoginAwareViewModel loginAwareViewModel)
                        {
                            loginAwareViewModel.IsLoggedIn = true;
                        }

                        // 如果配置了URL，自动启动抽取任务
                        if (!string.IsNullOrEmpty(config.MainTaskListUrl) && !string.IsNullOrEmpty(config.SubTaskListUrl))
                        {
                            await _jiraDataScraperService.StartScrapingTask(
                                config.MainTaskListUrl,
                                config.SubTaskListUrl,
                                config.RefreshInterval);
                        }
                    }
                    else
                    {
                        StatusMessage = "未登录";
                        IsLoggedIn = false;
                    }
                }
                catch (Exception ex)
                {
                    _notificationService.ShowError("登录失败", ex.Message);
                    StatusMessage = "登录失败";
                }
            }
        }

        /// <summary>
        /// 窗口关闭处理
        /// </summary>
        private async void OnWindowClosing(object? obj)
        {
            if (_trayIconService.IsExitFromTray) return;

            if (obj is System.ComponentModel.CancelEventArgs e)
            {
                var settings = await _databaseService.GetUserSettingsAsync();
                if (settings.MinimizeToTrayOnClose)
                {
                    e.Cancel = true;
                    _trayIconService.MinimizeToTray();
                }
            }
        }

        /// <summary>
        /// 窗口大小改变处理
        /// </summary>
        private async void OnWindowSizeChanged(object? obj)
        {
            if (obj is System.Windows.SizeChangedEventArgs e)
            {
                if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
                {
                    WindowWidth = e.NewSize.Width;
                    WindowHeight = e.NewSize.Height;

                    var settings = await _databaseService.GetUserSettingsAsync();
                    settings.WindowWidth = WindowWidth;
                    settings.WindowHeight = WindowHeight;
                    await _databaseService.SaveUserSettingsAsync(settings);
                }
            }
        }

        /// <summary>
        /// 窗口状态改变处理
        /// </summary>
        private void OnWindowStateChanged(object? obj)
        {
            if (obj is Window window && window.WindowState == WindowState.Minimized)
            {
                _trayIconService.MinimizeToTray();
            }
        }

        /// <summary>
        /// 显示窗口处理
        /// </summary>
        private void OnShowWindow()
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.WindowState = WindowState.Normal;
                Application.Current.MainWindow.Activate();
            }
        }

        /// <summary>
        /// 隐藏窗口处理
        /// </summary>
        private void OnHideWindow()
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Hide();
            }
        }

        /// <summary>
        /// 加载用户设置
        /// </summary>
        private async System.Threading.Tasks.Task LoadSettings()
        {
            try
            {
                var settings = await _databaseService.GetUserSettingsAsync();
                WindowWidth = settings.WindowWidth;
                WindowHeight = settings.WindowHeight;
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"打开URL失败: {ex.Message}", "错误");
            }
        }
        

        
        /// <summary>
        /// 处理打开浏览器的命令
        /// </summary>
        /// <param name="url">要打开的URL</param>
        private void OnOpenBrowser(string url)
        {
            try
            {
                // 如果是JIRA单号格式，则转换为URL
                if (System.Text.RegularExpressions.Regex.IsMatch(url, @"^[A-Z]+-\d+$"))
                {
                    // 从配置中获取BaseUrl
                    var config = new AppConfigService().GetConfig();
                    string baseUrl = config.BaseUrl;
                    
                    if (!string.IsNullOrEmpty(baseUrl))
                    {
                        url = $"{baseUrl.TrimEnd('/')}/browse/{url}";
                    }
                    else
                    {
                        // 如果没有配置基础URL，使用默认的Jira URL
                        url = $"https://jira.example.com/browse/{url}";
                    }
                }
                
                // 使用系统默认浏览器打开URL
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("错误", $"打开URL失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 显示班车号变更通知
        /// </summary>
        private void ShowBusChangeNotification()
        {
            // 示例1：班车变更询问通知 - K888变更为G999
            NotificationHelper.ShowBusChangeQuestion(
                "K888",  // 旧班车号
                "G999",  // 新班车号
                "JIRA-5678",  // 相关工单号
                onConfirm: (data) =>
                {
                    // 用户确认后显示成功通知
                    HandyNotificationManager.Success("感谢您的确认", "操作完成");
                },
                onCancel: (data) =>
                {
                    // 用户取消时的处理
                    Console.WriteLine("用户关闭了班车变更询问通知");
                }
            );

            // 延迟1秒后显示第二个通知
            System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // 示例2：普通信息通知
                    NotificationHelper.ShowBusChangeNotification("K456");
                });
            });

            // 延迟2秒后显示第三个通知
            System.Threading.Tasks.Task.Delay(2000).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // 示例3：带工单号的警告通知
                    NotificationHelper.ShowBusChangeWarning("K789", "JIRA-1234");
                });
            });

            // 延迟3秒后显示第四个通知（将进入队列）
            System.Threading.Tasks.Task.Delay(3000).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // 示例4：错误类型通知
                    NotificationHelper.ShowBusChangeError("K000");
                });
            });
        }
    }
}
