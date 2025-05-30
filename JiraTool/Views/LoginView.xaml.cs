using JiraTool.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace JiraTool.Views
{
    /// <summary>
    /// LoginView.xaml 的交互逻辑
    /// </summary>
    public partial class LoginView : UserControl
    {
        private LoginViewModel? _viewModel;

        /// <summary>
        /// 无参构造函数 - 用于XAML创建实例
        /// </summary>
        public LoginView()
        {
            InitializeComponent();
            
            // 尝试从服务容器获取ViewModel
            if (App.AppHost != null)
            {
                try
                {
                    _viewModel = App.AppHost.Services.GetRequiredService<LoginViewModel>();
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
        /// <param name="viewModel">登录视图模型</param>
        public LoginView(LoginViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        /// <summary>
        /// 用户控件加载事件处理
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel.LoadedCommand.CanExecute(null))
            {
                _viewModel.LoadedCommand.Execute(null);
            }

            // 给密码框设置焦点
            if (string.IsNullOrEmpty(UsernameTextBox.Text))
            {
                UsernameTextBox.Focus();
            }
            else
            {
                PasswordBox.Focus();
            }
        }
    }
}
