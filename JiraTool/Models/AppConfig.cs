using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace JiraTool.Models
{
    /// <summary>
    /// 应用程序配置类
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// Jira基础URL
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Jira登录页面URL
        /// </summary>
        public string LoginUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// 主单列表URL模板，包含{0}项目参数
        /// </summary>
        public string MainTaskListUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// 子单列表URL模板，包含{0}项目和{1}父任务参数
        /// </summary>
        public string SubTaskListUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// 数据刷新间隔（分钟）
        /// </summary>
        public int RefreshInterval { get; set; } = 30;
        
        /// <summary>
        /// 是否启动时自动登录
        /// </summary>
        public bool AutoLogin { get; set; } = false;
        
        /// <summary>
        /// 是否启动时最小化到托盘
        /// </summary>
        public bool MinimizeToTrayOnStart { get; set; } = false;
        
        /// <summary>
        /// 是否关闭时最小化到托盘
        /// </summary>
        public bool MinimizeToTrayOnClose { get; set; } = true;
    }
    
    /// <summary>
    /// 应用程序配置服务
    /// </summary>
    public class AppConfigService
    {
        private readonly ILogger<AppConfigService> _logger;
        private readonly string _configFilePath;
        private readonly IConfiguration _appSettings;
        private AppConfig _config;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="configuration">应用程序配置</param>
        public AppConfigService(ILogger<AppConfigService> logger = null, IConfiguration configuration = null)
        {
            _logger = logger;
            _appSettings = configuration;
            
            // 配置文件路径
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JiraTool");
            
            // 确保目录存在
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            
            _configFilePath = Path.Combine(appDataPath, "config.json");
            
            // 加载配置
            LoadConfig();
            
            // 从 AppSettings.json 加载默认配置
            LoadDefaultConfigFromAppSettings();
        }
        
        /// <summary>
        /// 获取配置
        /// </summary>
        public AppConfig GetConfig()
        {
            return _config;
        }
        
        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="config">要保存的配置</param>
        public async Task SaveConfigAsync(AppConfig config)
        {
            try
            {
                _config = config;
                
                // 序列化配置
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_config, options);
                
                // 写入文件
                await File.WriteAllTextAsync(_configFilePath, json);
                
                _logger?.LogInformation("配置已保存到: {ConfigFilePath}", _configFilePath);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "保存配置失败");
                throw;
            }
        }
        
        /// <summary>
        /// 加载配置
        /// </summary>
        private void LoadConfig()
        {
            try
            {
                // 检查配置文件是否存在
                if (File.Exists(_configFilePath))
                {
                    // 读取配置文件
                    var json = File.ReadAllText(_configFilePath);
                    
                    // 反序列化配置
                    _config = JsonSerializer.Deserialize<AppConfig>(json);
                    
                    _logger?.LogInformation("配置已从 {ConfigFilePath} 加载", _configFilePath);
                }
                else
                {
                    // 创建默认配置
                    _config = new AppConfig();
                    
                    _logger?.LogInformation("创建默认配置");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "加载配置失败，使用默认配置");
                _config = new AppConfig();
            }
        }
        
        /// <summary>
        /// 从 AppSettings.json 加载默认配置
        /// </summary>
        private void LoadDefaultConfigFromAppSettings()
        {
            try
            {
                if (_appSettings == null)
                {
                    _logger?.LogWarning("未提供 IConfiguration，无法从 AppSettings.json 加载默认配置");
                    return;
                }
                
                // 如果用户配置中没有设置这些值，则从 AppSettings.json 加载默认值
                if (string.IsNullOrEmpty(_config.BaseUrl))
                {
                    _config.BaseUrl = _appSettings["AppSettings:BaseUrl"] ?? string.Empty;
                }
                
                if (string.IsNullOrEmpty(_config.LoginUrl))
                {
                    _config.LoginUrl = _appSettings["AppSettings:LoginUrl"] ?? string.Empty;
                }
                
                if (string.IsNullOrEmpty(_config.MainTaskListUrl))
                {
                    _config.MainTaskListUrl = _appSettings["AppSettings:MainTaskListUrl"] ?? string.Empty;
                }
                
                if (string.IsNullOrEmpty(_config.SubTaskListUrl))
                {
                    _config.SubTaskListUrl = _appSettings["AppSettings:SubTaskListUrl"] ?? string.Empty;
                }
                
                // 如果用户没有设置刷新间隔，使用默认值
                if (_config.RefreshInterval <= 0)
                {
                    var refreshIntervalStr = _appSettings["AppSettings:RefreshInterval"];
                    if (!string.IsNullOrEmpty(refreshIntervalStr) && int.TryParse(refreshIntervalStr, out int refreshInterval))
                    {
                        _config.RefreshInterval = refreshInterval / 60000; // 转换为分钟
                    }
                }
                
                // 加载其他设置
                var autoStartWithWindowsStr = _appSettings["AppSettings:AutoStartWithWindows"];
                if (!string.IsNullOrEmpty(autoStartWithWindowsStr) && bool.TryParse(autoStartWithWindowsStr, out bool autoStartWithWindows))
                {
                    _config.AutoLogin = autoStartWithWindows;
                }
                
                var minimizeToTrayOnStartStr = _appSettings["AppSettings:MinimizeToTrayOnStart"];
                if (!string.IsNullOrEmpty(minimizeToTrayOnStartStr) && bool.TryParse(minimizeToTrayOnStartStr, out bool minimizeToTrayOnStart))
                {
                    _config.MinimizeToTrayOnStart = minimizeToTrayOnStart;
                }
                
                var minimizeToTrayOnCloseStr = _appSettings["AppSettings:MinimizeToTrayOnClose"];
                if (!string.IsNullOrEmpty(minimizeToTrayOnCloseStr) && bool.TryParse(minimizeToTrayOnCloseStr, out bool minimizeToTrayOnClose))
                {
                    _config.MinimizeToTrayOnClose = minimizeToTrayOnClose;
                }
                
                _logger?.LogInformation("从 AppSettings.json 加载默认配置成功");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "从 AppSettings.json 加载默认配置失败");
            }
        }
    }
}
