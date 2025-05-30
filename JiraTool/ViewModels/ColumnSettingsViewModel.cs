using JiraTool.ViewModels.Base;

namespace JiraTool.ViewModels
{
    /// <summary>
    /// 列设置视图模型
    /// </summary>
    public class ColumnSettingsViewModel : ViewModelBase
    {
        private bool _showTaskNameColumn = true;
        private bool _showTaskNumberColumn = true;
        private bool _showParentTaskNumberColumn = true;
        private bool _showTaskStatusColumn = true;
        private bool _showParentTaskStatusColumn = true;
        private bool _showEstimatedHoursColumn = true;
        private bool _showEstimatedCompletionTimeColumn = false;
        private bool _showActualCompletionTimeColumn = false;
        private bool _showCreatedAtColumn = true;
        private bool _showUpdatedAtColumn = true;
        private bool _showPriorityColumn = true;
        private bool _showBanCheNameColumn = true;
        private bool _showCodeMergedColumn = true;
        private bool _showSqlColumn = true;
        private bool _showConfigurationColumn = true;

        /// <summary>
        /// 是否显示子任务名称列
        /// </summary>
        public bool ShowTaskNameColumn
        {
            get => _showTaskNameColumn;
            set => SetProperty(ref _showTaskNameColumn, value);
        }

        /// <summary>
        /// 是否显示子任务单号列
        /// </summary>
        public bool ShowTaskNumberColumn
        {
            get => _showTaskNumberColumn;
            set => SetProperty(ref _showTaskNumberColumn, value);
        }

        /// <summary>
        /// 是否显示主任务单号列
        /// </summary>
        public bool ShowParentTaskNumberColumn
        {
            get => _showParentTaskNumberColumn;
            set => SetProperty(ref _showParentTaskNumberColumn, value);
        }

        /// <summary>
        /// 是否显示子任务状态列
        /// </summary>
        public bool ShowTaskStatusColumn
        {
            get => _showTaskStatusColumn;
            set => SetProperty(ref _showTaskStatusColumn, value);
        }

        /// <summary>
        /// 是否显示主任务状态列
        /// </summary>
        public bool ShowParentTaskStatusColumn
        {
            get => _showParentTaskStatusColumn;
            set => SetProperty(ref _showParentTaskStatusColumn, value);
        }

        /// <summary>
        /// 是否显示预计工时列
        /// </summary>
        public bool ShowEstimatedHoursColumn
        {
            get => _showEstimatedHoursColumn;
            set => SetProperty(ref _showEstimatedHoursColumn, value);
        }

        /// <summary>
        /// 是否显示预计完成时间列
        /// </summary>
        public bool ShowEstimatedCompletionTimeColumn
        {
            get => _showEstimatedCompletionTimeColumn;
            set => SetProperty(ref _showEstimatedCompletionTimeColumn, value);
        }

        /// <summary>
        /// 是否显示实际完成时间列
        /// </summary>
        public bool ShowActualCompletionTimeColumn
        {
            get => _showActualCompletionTimeColumn;
            set => SetProperty(ref _showActualCompletionTimeColumn, value);
        }

        /// <summary>
        /// 是否显示创建时间列
        /// </summary>
        public bool ShowCreatedAtColumn
        {
            get => _showCreatedAtColumn;
            set => SetProperty(ref _showCreatedAtColumn, value);
        }

        /// <summary>
        /// 是否显示更新时间列
        /// </summary>
        public bool ShowUpdatedAtColumn
        {
            get => _showUpdatedAtColumn;
            set => SetProperty(ref _showUpdatedAtColumn, value);
        }

        /// <summary>
        /// 是否显示优先级列
        /// </summary>
        public bool ShowPriorityColumn
        {
            get => _showPriorityColumn;
            set => SetProperty(ref _showPriorityColumn, value);
        }

        /// <summary>
        /// 是否显示班车名列
        /// </summary>
        public bool ShowBanCheNameColumn
        {
            get => _showBanCheNameColumn;
            set => SetProperty(ref _showBanCheNameColumn, value);
        }

        /// <summary>
        /// 是否显示代码已合并列
        /// </summary>
        public bool ShowCodeMergedColumn
        {
            get => _showCodeMergedColumn;
            set => SetProperty(ref _showCodeMergedColumn, value);
        }

        /// <summary>
        /// 是否显示SQL脚本列
        /// </summary>
        public bool ShowSqlColumn
        {
            get => _showSqlColumn;
            set => SetProperty(ref _showSqlColumn, value);
        }

        /// <summary>
        /// 是否显示配置信息列
        /// </summary>
        public bool ShowConfigurationColumn
        {
            get => _showConfigurationColumn;
            set => SetProperty(ref _showConfigurationColumn, value);
        }
    }
}
