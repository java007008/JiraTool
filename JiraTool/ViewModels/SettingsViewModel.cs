using System;
using System.Threading.Tasks;
using System.Windows.Input;
using JiraTool.Models;
using JiraTool.Services;
using JiraTool.Services.Interfaces;
using JiraTool.Utils;
using JiraTool.ViewModels.Base;
using Microsoft.Extensions.Logging;

namespace JiraTool.ViewModels
{
    /// <summary>
    /// 设置视图模型
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ILogger<SettingsViewModel> _logger;
        private readonly IDatabaseService _databaseService;
        private readonly INotificationService _notificationService;
        private readonly AppConfigService _configService;
        private readonly IJiraLoginService _jiraLoginService;
        private readonly IJiraDataScraperService _jiraDataScraperService;
        
        private string _baseUrl = string.Empty;
        private string _loginUrl = string.Empty;
        private string _mainTaskListUrl = string.Empty;
        private string _subTaskListUrl = string.Empty;
        private int _refreshInterval = 30;
        private bool _autoLogin = false;
        private bool _minimizeToTrayOnStart = false;
        private bool _minimizeToTrayOnClose = true;
        private bool _isBusy = false;
        
        /// <summary>
        /// Jira基础URL
        /// </summary>
        public string BaseUrl
        {
            get => _baseUrl;
            set => SetProperty(ref _baseUrl, value);
        }
        
        /// <summary>
        /// Jira登录页面URL
        /// </summary>
        public string LoginUrl
        {
            get => _loginUrl;
            set => SetProperty(ref _loginUrl, value);
        }
        
        /// <summary>
        /// 主单列表URL模板
        /// </summary>
        public string MainTaskListUrl
        {
            get => _mainTaskListUrl;
            set => SetProperty(ref _mainTaskListUrl, value);
        }
        
        /// <summary>
        /// 子单列表URL模板
        /// </summary>
        public string SubTaskListUrl
        {
            get => _subTaskListUrl;
            set => SetProperty(ref _subTaskListUrl, value);
        }
        
        /// <summary>
        /// 数据刷新间隔（分钟）
        /// </summary>
        public int RefreshInterval
        {
            get => _refreshInterval;
            set => SetProperty(ref _refreshInterval, value);
        }
        
        /// <summary>
        /// 是否启动时自动登录
        /// </summary>
        public bool AutoLogin
        {
            get => _autoLogin;
            set => SetProperty(ref _autoLogin, value);
        }
        
        /// <summary>
        /// 是否启动时最小化到托盘
        /// </summary>
        public bool MinimizeToTrayOnStart
        {
            get => _minimizeToTrayOnStart;
            set => SetProperty(ref _minimizeToTrayOnStart, value);
        }
        
        /// <summary>
        /// 是否关闭时最小化到托盘
        /// </summary>
        public bool MinimizeToTrayOnClose
        {
            get => _minimizeToTrayOnClose;
            set => SetProperty(ref _minimizeToTrayOnClose, value);
        }
        
        /// <summary>
        /// 是否忙碌
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        
        /// <summary>
        /// 保存命令
        /// </summary>
        public ICommand SaveCommand { get; }
        
        /// <summary>
        /// 登录命令
        /// </summary>
        public ICommand LoginCommand { get; }
        
        /// <summary>
        /// 开始抓取命令
        /// </summary>
        public ICommand StartScrapingCommand { get; }
        
        /// <summary>
        /// 停止抓取命令
        /// </summary>
        public ICommand StopScrapingCommand { get; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public SettingsViewModel(
            ILogger<SettingsViewModel> logger,
            IDatabaseService databaseService,
            INotificationService notificationService,
            AppConfigService configService,
            IJiraLoginService jiraLoginService,
            IJiraDataScraperService jiraDataScraperService)
        {
            _logger = logger;
            _databaseService = databaseService;
            _notificationService = notificationService;
            _configService = configService;
            _jiraLoginService = jiraLoginService;
            _jiraDataScraperService = jiraDataScraperService;
            
            // 初始化命令
            SaveCommand = new RelayCommand(async _ => await SaveSettingsAsync());
            LoginCommand = new RelayCommand(async _ => await LoginAsync());
            StartScrapingCommand = new RelayCommand(async _ => await StartScrapingAsync());
            StopScrapingCommand = new RelayCommand(_ => StopScraping());
            
            // 加载设置
            LoadSettings();
        }
        
        /// <summary>
        /// 加载设置
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                // 从配置文件加载设置
                var config = _configService.GetConfig();
                
                BaseUrl = config.BaseUrl;
                LoginUrl = config.LoginUrl;
                MainTaskListUrl = config.MainTaskListUrl;
                SubTaskListUrl = config.SubTaskListUrl;
                RefreshInterval = config.RefreshInterval;
                AutoLogin = config.AutoLogin;
                MinimizeToTrayOnStart = config.MinimizeToTrayOnStart;
                MinimizeToTrayOnClose = config.MinimizeToTrayOnClose;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载设置失败");
                _notificationService.ShowError("加载设置失败", ex.Message);
            }
        }
        
        /// <summary>
        /// 保存设置
        /// </summary>
        private async Task SaveSettingsAsync()
        {
            try
            {
                IsBusy = true;
                
                // 更新配置
                var config = _configService.GetConfig();
                
                config.BaseUrl = BaseUrl;
                config.LoginUrl = LoginUrl;
                config.MainTaskListUrl = MainTaskListUrl;
                config.SubTaskListUrl = SubTaskListUrl;
                config.RefreshInterval = RefreshInterval;
                config.AutoLogin = AutoLogin;
                config.MinimizeToTrayOnStart = MinimizeToTrayOnStart;
                config.MinimizeToTrayOnClose = MinimizeToTrayOnClose;
                
                // 保存配置
                await _configService.SaveConfigAsync(config);
                
                // 更新数据库设置
                var settings = await _databaseService.GetUserSettingsAsync();
                
                settings.JiraServerUrl = BaseUrl; // 使用BaseUrl作为数据库中的JiraServerUrl
                settings.RefreshInterval = RefreshInterval;
                settings.AutoLogin = AutoLogin;
                settings.MinimizeToTrayOnStart = MinimizeToTrayOnStart;
                settings.MinimizeToTrayOnClose = MinimizeToTrayOnClose;
                
                await _databaseService.SaveUserSettingsAsync(settings);
                
                _notificationService.ShowInformation("设置已保存", "应用程序设置已成功保存");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存设置失败");
                _notificationService.ShowError("保存设置失败", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        /// <summary>
        /// 登录
        /// </summary>
        private async Task LoginAsync()
        {
            try
            {
                IsBusy = true;
                
                // 检查Jira基础URL
                if (string.IsNullOrEmpty(BaseUrl))
                {
                    _notificationService.ShowError("登录失败", "请输入Jira基础地址");
                    return;
                }
                
                // 打开登录页面
                var success = await _jiraLoginService.OpenLoginPage(BaseUrl);
                
                if (success)
                {
                    _notificationService.ShowInformation("登录成功", "已成功登录到Jira");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登录失败");
                _notificationService.ShowError("登录失败", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        /// <summary>
        /// 开始抓取
        /// </summary>
        private async Task StartScrapingAsync()
        {
            try
            {
                IsBusy = true;
                
                // 检查URL
                if (string.IsNullOrEmpty(MainTaskListUrl) || string.IsNullOrEmpty(SubTaskListUrl))
                {
                    _notificationService.ShowError("启动抓取失败", "请输入主单和子单列表URL");
                    return;
                }
                
                // 检查是否已登录
                var isLoggedIn = await _jiraLoginService.IsLoggedIn();
                if (!isLoggedIn)
                {
                    // 尝试登录
                    var success = await _jiraLoginService.OpenLoginPage(BaseUrl);
                    if (!success)
                    {
                        _notificationService.ShowError("启动抓取失败", "请先登录Jira");
                        return;
                    }
                }
                
                // 启动抓取任务
                await _jiraDataScraperService.StartScrapingTask(MainTaskListUrl, SubTaskListUrl, RefreshInterval);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动抓取失败");
                _notificationService.ShowError("启动抓取失败", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        /// <summary>
        /// 停止抓取
        /// </summary>
        private void StopScraping()
        {
            try
            {
                _jiraDataScraperService.StopScrapingTask();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止抓取失败");
                _notificationService.ShowError("停止抓取失败", ex.Message);
            }
        }
    }
}
