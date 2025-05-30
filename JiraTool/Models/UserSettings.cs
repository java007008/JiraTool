using System.ComponentModel.DataAnnotations;

namespace JiraTool.Models
{
    /// <summary>
    /// 用户设置模型
    /// </summary>
    public class UserSettings
    {
        /// <summary>
        /// 设置ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 加密后的密码（如果保存）
        /// </summary>
        [MaxLength(200)]
        public string? EncryptedPassword { get; set; }

        /// <summary>
        /// 是否记住密码
        /// </summary>
        public bool RememberPassword { get; set; } = false;

        /// <summary>
        /// 是否自动登录
        /// </summary>
        public bool AutoLogin { get; set; } = false;

        /// <summary>
        /// Jira服务器地址
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string JiraServerUrl { get; set; } = string.Empty;

        /// <summary>
        /// 数据刷新间隔（分钟）
        /// </summary>
        public int RefreshInterval { get; set; } = 1;

        /// <summary>
        /// 是否显示子任务名称列
        /// </summary>
        public bool ShowTaskNameColumn { get; set; } = true;

        /// <summary>
        /// 是否显示子任务单号列
        /// </summary>
        public bool ShowTaskNumberColumn { get; set; } = true;

        /// <summary>
        /// 是否显示主任务单号列
        /// </summary>
        public bool ShowParentTaskNumberColumn { get; set; } = true;

        /// <summary>
        /// 是否显示子任务状态列
        /// </summary>
        public bool ShowTaskStatusColumn { get; set; } = true;

        /// <summary>
        /// 是否显示主任务状态列
        /// </summary>
        public bool ShowParentTaskStatusColumn { get; set; } = true;

        /// <summary>
        /// 是否显示预计工时列
        /// </summary>
        public bool ShowEstimatedHoursColumn { get; set; } = true;

        /// <summary>
        /// 是否显示预计完成时间列
        /// </summary>
        public bool ShowEstimatedCompletionTimeColumn { get; set; } = false;

        /// <summary>
        /// 是否显示实际完成时间列
        /// </summary>
        public bool ShowActualCompletionTimeColumn { get; set; } = false;

        /// <summary>
        /// 是否显示创建时间列
        /// </summary>
        public bool ShowCreatedAtColumn { get; set; } = true;

        /// <summary>
        /// 是否显示更新时间列
        /// </summary>
        public bool ShowUpdatedAtColumn { get; set; } = true;

        /// <summary>
        /// 是否显示优先等级列
        /// </summary>
        public bool ShowPriorityColumn { get; set; } = true;

        /// <summary>
        /// 是否显示班车名列
        /// </summary>
        public bool ShowBanCheNameColumn { get; set; } = true;

        /// <summary>
        /// 是否显示代码合并列
        /// </summary>
        public bool ShowCodeMergedColumn { get; set; } = true;

        /// <summary>
        /// 是否显示SQL列
        /// </summary>
        public bool ShowSqlColumn { get; set; } = true;

        /// <summary>
        /// 是否显示配置列
        /// </summary>
        public bool ShowConfigurationColumn { get; set; } = true;

        /// <summary>
        /// 窗口宽度
        /// </summary>
        public double WindowWidth { get; set; } = 1280;

        /// <summary>
        /// 窗口高度
        /// </summary>
        public double WindowHeight { get; set; } = 720;

        /// <summary>
        /// 是否启动时最小化到托盘
        /// </summary>
        public bool MinimizeToTrayOnStart { get; set; } = false;

        /// <summary>
        /// 是否关闭时最小化到托盘
        /// </summary>
        public bool MinimizeToTrayOnClose { get; set; } = true;
    }
}
