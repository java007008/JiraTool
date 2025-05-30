using JiraTool.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace JiraTool.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="viewModel">主窗口视图模型</param>
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        /// <summary>
        /// 窗口加载事件处理
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel.WindowLoadedCommand.CanExecute(null))
            {
                _viewModel.WindowLoadedCommand.Execute(null);
            }
        }

        /// <summary>
        /// 窗口关闭事件处理
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_viewModel.WindowClosingCommand.CanExecute(e))
            {
                _viewModel.WindowClosingCommand.Execute(e);
            }
        }

        /// <summary>
        /// 窗口状态改变事件处理
        /// </summary>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (_viewModel.WindowStateChangedCommand.CanExecute(this))
            {
                _viewModel.WindowStateChangedCommand.Execute(this);
            }
        }
        
        /// <summary>
        /// 打开浏览器标签页或窗口来显示URL
        /// </summary>
        /// <param name="url">要打开的URL</param>
        public void OpenBrowserTab(string url)
        {
            try
            {
                // 如果有内置浏览器标签页，可以在这里打开
                if (_viewModel.OpenBrowserCommand != null && _viewModel.OpenBrowserCommand.CanExecute(url))
                {
                    _viewModel.OpenBrowserCommand.Execute(url);
                    return;
                }
                
                // 如果没有内置浏览器，则使用系统默认浏览器打开
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开URL失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 窗口大小改变事件处理
        /// </summary>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_viewModel.WindowSizeChangedCommand.CanExecute(e))
            {
                _viewModel.WindowSizeChangedCommand.Execute(e);
            }
        }

        /// <summary>
        /// 标题栏鼠标左键按下事件处理
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeButton_Click(sender, e);
            }
            else
            {
                DragMove();
            }
        }

        /// <summary>
        /// 最大化/还原按钮点击事件处理
        /// </summary>
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        /// <summary>
        /// 关闭按钮点击事件处理
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
