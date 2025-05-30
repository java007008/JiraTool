using JiraTool.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JiraTool.Views
{
    /// <summary>
    /// TaskListView.xaml 的交互逻辑
    /// </summary>
    public partial class TaskListView : UserControl
    {
        private TaskListViewModel? _viewModel;

        /// <summary>
        /// 无参构造函数 - 用于XAML创建实例
        /// </summary>
        public TaskListView()
        {
            InitializeComponent();
            
            // 尝试从服务容器获取ViewModel
            if (App.AppHost != null)
            {
                try
                {
                    _viewModel = App.AppHost.Services.GetRequiredService<TaskListViewModel>();
                    if (_viewModel != null)
                    {
                        DataContext = _viewModel;
                    }
                }
                catch
                {
                    // 忽略异常
                }
            }
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="viewModel">任务列表视图模型</param>
        public TaskListView(TaskListViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            
            // 添加控件卸载事件处理
            Unloaded += TaskListView_Unloaded;
        }
        
        /// <summary>
        /// 控件卸载事件处理
        /// </summary>
        private void TaskListView_Unloaded(object sender, RoutedEventArgs e)
        {
            // 取消事件订阅，避免内存泄漏
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            
            // 移除控件卸载事件处理
            Unloaded -= TaskListView_Unloaded;
        }

        /// <summary>
        /// 用户控件加载事件处理
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel?.LoadedCommand != null && _viewModel.LoadedCommand.CanExecute(null))
            {
                _viewModel.LoadedCommand.Execute(null);
            }
            
            // 监听列设置属性的变化
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                UpdateColumnVisibility(); // 初始化列可见性
            }
        }
        
        /// <summary>
        /// ViewModel属性变化事件处理
        /// </summary>
        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // 如果是列设置属性变化，则直接更新列可见性
            if (e.PropertyName?.StartsWith("Show") == true && e.PropertyName.EndsWith("Column"))
            {
                // 直接更新对应列的可见性
                UpdateColumnVisibility(e.PropertyName);
            }
        }
        
        /// <summary>
        /// 更新列可见性
        /// </summary>
        /// <param name="propertyName">变化的属性名，如果为空则更新所有列</param>
        private void UpdateColumnVisibility(string? propertyName = null)
        {
            if (_viewModel == null) return;
            
            // 手动设置每个列的可见性
            foreach (var column in TaskDataGrid.Columns)
            {
                if (column.Header?.ToString() == "ID") continue; // ID列始终显示
                
                string? columnProperty = null;
                
                switch (column.Header?.ToString())
                {
                    case "任务名称":
                        columnProperty = "ShowTaskNameColumn";
                        break;
                    case "任务编号":
                        columnProperty = "ShowTaskNumberColumn";
                        break;
                    case "状态":
                        columnProperty = "ShowTaskStatusColumn";
                        break;
                    case "优先级":
                        columnProperty = "ShowPriorityColumn";
                        break;
                    case "创建时间":
                        columnProperty = "ShowCreatedAtColumn";
                        break;
                    case "更新时间":
                        columnProperty = "ShowUpdatedAtColumn";
                        break;
                    case "预计工时":
                        columnProperty = "ShowEstimatedHoursColumn";
                        break;
                    case "预计完成时间":
                        columnProperty = "ShowEstimatedCompletionTimeColumn";
                        break;
                    case "实际完成时间":
                        columnProperty = "ShowActualCompletionTimeColumn";
                        break;
                    case "主任务编号":
                        columnProperty = "ShowParentTaskNumberColumn";
                        break;
                    case "主任务状态":
                        columnProperty = "ShowParentTaskStatusColumn";
                        break;
                    case "班车名":
                        columnProperty = "ShowBanCheNameColumn";
                        break;
                    case "代码已合并":
                        columnProperty = "ShowCodeMergedColumn";
                        break;
                    case "SQL脚本":
                        columnProperty = "ShowSqlColumn";
                        break;
                    case "配置信息":
                        columnProperty = "ShowConfigurationColumn";
                        break;
                }
                
                // 如果指定了属性名，只更新该属性对应的列
                if (propertyName != null && columnProperty != propertyName) continue;
                
                // 获取属性值并设置列可见性
                if (columnProperty != null)
                {
                    var propertyInfo = _viewModel.GetType().GetProperty(columnProperty);
                    if (propertyInfo != null)
                    {
                        bool isVisible = (bool)propertyInfo.GetValue(_viewModel)!;
                        column.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
        }


        /// <summary>
        /// 子任务单号点击事件处理
        /// </summary>
        private void TaskNumber_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Tag is string taskNumber)
            {
                if (_viewModel.OpenTaskCommand != null && _viewModel.OpenTaskCommand.CanExecute(taskNumber))
                {
                    _viewModel.OpenTaskCommand.Execute(taskNumber);
                }
            }
        }

        /// <summary>
        /// 主任务单号点击事件处理
        /// </summary>
        private void ParentTaskNumber_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Tag is string taskNumber)
            {
                if (_viewModel.OpenParentTaskCommand != null && _viewModel.OpenParentTaskCommand.CanExecute(taskNumber))
                {
                    _viewModel.OpenParentTaskCommand.Execute(taskNumber);
                }
            }
        }
    }
}
