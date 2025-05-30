using JiraTool.Models;
using JiraTool.Services;
using JiraTool.Services.Interfaces;
using JiraTool.ViewModels.Base;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JiraTool.ViewModels
{
    /// <summary>
    /// 登录视图模型
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {
        private readonly IJiraLoginService _jiraLoginService;
        private readonly INotificationService _notificationService;
        private readonly IDatabaseService _databaseService;
        private readonly AppConfigService _configService;

        private string _jiraUrl = string.Empty;
        private bool _isLoading = false;
        private string _statusMessage = string.Empty;

        /// <summary>
        /// Jira服务器地址
        /// </summary>
        public string JiraUrl
        {
            get => _jiraUrl;
            set => SetProperty(ref _jiraUrl, value);
        }

        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
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
        /// 登录命令
        /// </summary>
        public ICommand LoginCommand { get; }

        /// <summary>
        /// 页面加载命令
        /// </summary>
        public ICommand LoadedCommand { get; }

        /// <summary>
        /// 登录成功事件
        /// </summary>
        public event EventHandler? LoginSuccessful;

        /// <summary>
        /// 构造函数
        /// </summary>
        public LoginViewModel(
            IJiraLoginService jiraLoginService,
            INotificationService notificationService,
            IDatabaseService databaseService,
            AppConfigService configService)
        {
            _jiraLoginService = jiraLoginService;
            _notificationService = notificationService;
            _databaseService = databaseService;
            _configService = configService;

            // 初始化命令
            LoginCommand = new RelayCommand<object>(OnLogin, CanLogin);
            LoadedCommand = new RelayCommand<object>(OnLoaded);
        }

        /// <summary>
        /// 页面加载处理
        /// </summary>
        private async void OnLoaded(object? _)
        {
            await LoadSettings();
            
            // 尝试自动登录
            if (!string.IsNullOrEmpty(JiraUrl))
            {
                // 如果已经有保存的URL，自动填充
                var isLoggedIn = await _jiraLoginService.IsLoggedIn();
                if (isLoggedIn)
                {
                    // 如果已经登录，触发登录成功事件
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 登录处理
        /// </summary>
        private async void OnLogin(object? _)
        {
            await LoginAsync();
        }

        /// <summary>
        /// 登录过程
        /// </summary>
        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "正在打开登录页面...";
                
                // 调用登录服务打开登录页面
                var isLoggedIn = await _jiraLoginService.OpenLoginPage(JiraUrl);
                
                if (isLoggedIn)
                {
                    StatusMessage = "登录成功";
                    
                    // 保存设置
                    var settings = await _databaseService.GetUserSettingsAsync();
                    settings.JiraServerUrl = JiraUrl;
                    await _databaseService.SaveUserSettingsAsync(settings);

                    // 保存到配置
                    var config = _configService.GetConfig();
                    config.BaseUrl = JiraUrl;
                    await _configService.SaveConfigAsync(config);

                    // 触发登录成功事件
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    StatusMessage = "登录取消或失败";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "登录异常";
                _notificationService.ShowError("登录异常", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 判断是否可以登录
        /// </summary>
        private bool CanLogin(object? _)
        {
            return !string.IsNullOrEmpty(JiraUrl) && !IsLoading;
        }

        /// <summary>
        /// 加载设置
        /// </summary>
        private async Task LoadSettings()
        {
            try
            {
                // 从数据库加载设置
                var settings = await _databaseService.GetUserSettingsAsync();
                if (!string.IsNullOrEmpty(settings.JiraServerUrl))
                {
                    JiraUrl = settings.JiraServerUrl;
                }
                else
                {
                    // 从配置文件加载设置
                    var config = _configService.GetConfig();
                    if (!string.IsNullOrEmpty(config.BaseUrl))
                    {
                        JiraUrl = config.BaseUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("加载设置失败", ex.Message);
            }
        }
    }
}
