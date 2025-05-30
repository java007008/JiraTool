using JiraTool.ViewModels.Base;
using System.Windows;
using System.Windows.Input;

namespace JiraTool.Views
{
    /// <summary>
    /// ColumnSettingsDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ColumnSettingsDialog : Window
    {
        private readonly ViewModels.ColumnSettingsViewModel _viewModel = new ViewModels.ColumnSettingsViewModel();
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public ColumnSettingsDialog()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
        
        /// <summary>
        /// 标题栏拖动事件
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                DragMove();
            }
        }
        
        /// <summary>
        /// 关闭按钮点击事件
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #region 列显示属性

        /// <summary>
        /// 是否显示子任务名称列
        /// </summary>
        public bool ShowTaskNameColumn
        {
            get => _viewModel.ShowTaskNameColumn;
            set => _viewModel.ShowTaskNameColumn = value;
        }

        /// <summary>
        /// 是否显示子任务单号列
        /// </summary>
        public bool ShowTaskNumberColumn
        {
            get => _viewModel.ShowTaskNumberColumn;
            set => _viewModel.ShowTaskNumberColumn = value;
        }

        /// <summary>
        /// 是否显示主任务单号列
        /// </summary>
        public bool ShowParentTaskNumberColumn
        {
            get => _viewModel.ShowParentTaskNumberColumn;
            set => _viewModel.ShowParentTaskNumberColumn = value;
        }

        /// <summary>
        /// 是否显示子任务状态列
        /// </summary>
        public bool ShowTaskStatusColumn
        {
            get => _viewModel.ShowTaskStatusColumn;
            set => _viewModel.ShowTaskStatusColumn = value;
        }

        /// <summary>
        /// 是否显示主任务状态列
        /// </summary>
        public bool ShowParentTaskStatusColumn
        {
            get => _viewModel.ShowParentTaskStatusColumn;
            set => _viewModel.ShowParentTaskStatusColumn = value;
        }

        /// <summary>
        /// 是否显示预计工时列
        /// </summary>
        public bool ShowEstimatedHoursColumn
        {
            get => _viewModel.ShowEstimatedHoursColumn;
            set => _viewModel.ShowEstimatedHoursColumn = value;
        }

        /// <summary>
        /// 是否显示预计完成时间列
        /// </summary>
        public bool ShowEstimatedCompletionTimeColumn
        {
            get => _viewModel.ShowEstimatedCompletionTimeColumn;
            set => _viewModel.ShowEstimatedCompletionTimeColumn = value;
        }

        /// <summary>
        /// 是否显示实际完成时间列
        /// </summary>
        public bool ShowActualCompletionTimeColumn
        {
            get => _viewModel.ShowActualCompletionTimeColumn;
            set => _viewModel.ShowActualCompletionTimeColumn = value;
        }

        /// <summary>
        /// 是否显示创建时间列
        /// </summary>
        public bool ShowCreatedAtColumn
        {
            get => _viewModel.ShowCreatedAtColumn;
            set => _viewModel.ShowCreatedAtColumn = value;
        }

        /// <summary>
        /// 是否显示更新时间列
        /// </summary>
        public bool ShowUpdatedAtColumn
        {
            get => _viewModel.ShowUpdatedAtColumn;
            set => _viewModel.ShowUpdatedAtColumn = value;
        }

        /// <summary>
        /// 是否显示优先级列
        /// </summary>
        public bool ShowPriorityColumn
        {
            get => _viewModel.ShowPriorityColumn;
            set => _viewModel.ShowPriorityColumn = value;
        }

        /// <summary>
        /// 是否显示班车名列
        /// </summary>
        public bool ShowBanCheNameColumn
        {
            get => _viewModel.ShowBanCheNameColumn;
            set => _viewModel.ShowBanCheNameColumn = value;
        }

        /// <summary>
        /// 是否显示代码已合并列
        /// </summary>
        public bool ShowCodeMergedColumn
        {
            get => _viewModel.ShowCodeMergedColumn;
            set => _viewModel.ShowCodeMergedColumn = value;
        }

        /// <summary>
        /// 是否显示SQL脚本列
        /// </summary>
        public bool ShowSqlColumn
        {
            get => _viewModel.ShowSqlColumn;
            set => _viewModel.ShowSqlColumn = value;
        }

        /// <summary>
        /// 是否显示配置信息列
        /// </summary>
        public bool ShowConfigurationColumn
        {
            get => _viewModel.ShowConfigurationColumn;
            set => _viewModel.ShowConfigurationColumn = value;
        }

        #endregion

        /// <summary>
        /// 确定按钮点击事件
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
