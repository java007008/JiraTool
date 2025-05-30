using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JiraTool.Models
{
    /// <summary>
    /// Jira主任务模型
    /// </summary>
    public class JiraTask : TaskVisibility
    {
        /// <summary>
        /// 主任务ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 主任务单号
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TaskNumber { get; set; } = string.Empty;

        /// <summary>
        /// 主任务URL
        /// </summary>
        [MaxLength(500)]
        public string TaskUrl { get; set; } = string.Empty;

        /// <summary>
        /// 主任务描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 上一次主任务描述，用于检测变更
        /// </summary>
        public string PreviousDescription { get; set; } = string.Empty;

        /// <summary>
        /// 主任务状态
        /// </summary>
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 班车名
        /// </summary>
        [MaxLength(50)]
        public string BanCheName { get; set; } = string.Empty;

        /// <summary>
        /// 上一次班车名，用于检测变更
        /// </summary>
        [MaxLength(50)]
        public string PreviousBanCheName { get; set; } = string.Empty;

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
        /// 是否已经通知班车名变更
        /// </summary>
        public bool IsBanCheNameChangeNotified { get; set; } = false;

        /// <summary>
        /// 是否已经通知描述变更
        /// </summary>
        public bool IsDescriptionChangeNotified { get; set; } = false;

        /// <summary>
        /// 子任务列表
        /// </summary>
        [InverseProperty("ParentTask")]
        public ICollection<JiraSubTask> SubTasks { get; set; } = new List<JiraSubTask>();
    }
}
