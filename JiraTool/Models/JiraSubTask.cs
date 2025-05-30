using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JiraTool.Models
{
    /// <summary>
    /// Jira子任务模型
    /// </summary>
    public class JiraSubTask : TaskVisibility
    {
        /// <summary>
        /// 子任务ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 子任务名称
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string TaskName { get; set; } = string.Empty;

        /// <summary>
        /// 子任务单号
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TaskNumber { get; set; } = string.Empty;

        /// <summary>
        /// 子任务URL
        /// </summary>
        [MaxLength(500)]
        public string TaskUrl { get; set; } = string.Empty;

        /// <summary>
        /// 子任务状态
        /// </summary>
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 预计工时（小时）
        /// </summary>
        public double EstimatedHours { get; set; }

        /// <summary>
        /// 预计完成时间
        /// </summary>
        public DateTime? EstimatedCompletionTime { get; set; }

        /// <summary>
        /// 实际完成时间
        /// </summary>
        public DateTime? ActualCompletionTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 优先等级
        /// </summary>
        [MaxLength(20)]
        public string Priority { get; set; } = string.Empty;

        /// <summary>
        /// 是否已合并代码
        /// </summary>
        public bool IsCodeMerged { get; set; } = false;

        /// <summary>
        /// 是否有SQL脚本
        /// </summary>
        public bool HasSqlScript { get; set; } = false;

        /// <summary>
        /// SQL脚本内容
        /// </summary>
        public string SqlScript { get; set; } = string.Empty;

        /// <summary>
        /// 是否有配置
        /// </summary>
        public bool HasConfiguration { get; set; } = false;

        /// <summary>
        /// 配置信息
        /// </summary>
        public string Configuration { get; set; } = string.Empty;

        /// <summary>
        /// 是否班车名已变更（用于UI显示标记）
        /// </summary>
        public bool IsBanCheNameChanged { get; set; } = false;

        /// <summary>
        /// 外键 - 主任务ID
        /// </summary>
        public int ParentTaskId { get; set; }

        /// <summary>
        /// 导航属性 - 主任务
        /// </summary>
        [ForeignKey("ParentTaskId")]
        public JiraTask? ParentTask { get; set; }
    }
}
