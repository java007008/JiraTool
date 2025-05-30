using JiraTool.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace JiraTool.Views
{
    /// <summary>
    /// TestView.xaml 的交互逻辑
    /// </summary>
    public partial class TestView : UserControl
    {
        private TestViewModel? _viewModel;

        /// <summary>
        /// 无参构造函数 - 用于XAML创建实例
        /// </summary>
        public TestView()
        {
            InitializeComponent();
            
            // 尝试从服务容器获取ViewModel
            if (App.AppHost != null)
            {
                try
                {
                    _viewModel = App.AppHost.Services.GetRequiredService<TestViewModel>();
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
        /// <param name="viewModel">测试视图模型</param>
        public TestView(TestViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}
