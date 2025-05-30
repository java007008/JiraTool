using Hardcodet.Wpf.TaskbarNotification;
using JiraTool.Services.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;

namespace JiraTool.Services
{
    /// <summary>
    /// 系统托盘图标服务实现类
    /// </summary>
    public class TrayIconService : ITrayIconService
    {
        private TaskbarIcon? _trayIcon;
        private bool _isExitFromTray = false;

        /// <summary>
        /// 窗口显示事件
        /// </summary>
        public event EventHandler? ShowWindowRequested;

        /// <summary>
        /// 窗口隐藏事件
        /// </summary>
        public event EventHandler? HideWindowRequested;

        /// <summary>
        /// 应用退出事件
        /// </summary>
        public event EventHandler? ExitRequested;

        /// <summary>
        /// 用户是否选择从托盘退出
        /// </summary>
        public bool IsExitFromTray => _isExitFromTray;

        /// <summary>
        /// 初始化托盘图标
        /// </summary>
        public void Initialize()
        {
            if (_trayIcon != null) return;

            // 在UI线程中创建托盘图标
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    // 尝试加载托盘图标
                    _trayIcon = new TaskbarIcon
                    {
                        // 创建一个默认图标代替资源图标
                        Icon = System.Drawing.SystemIcons.Application,
                        ToolTipText = "Jira客户端工具",
                        Visibility = Visibility.Visible,
                        ContextMenu = CreateContextMenu()
                    };

                    // 双击托盘图标显示主窗口
                    _trayIcon.TrayMouseDoubleClick += (sender, e) => OnShowWindowRequested();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"创建托盘图标失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        /// <summary>
        /// 显示托盘图标
        /// </summary>
        public void Show()
        {
            if (_trayIcon != null)
            {
                _trayIcon.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 隐藏托盘图标
        /// </summary>
        public void Hide()
        {
            if (_trayIcon != null)
            {
                _trayIcon.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 设置托盘图标工具提示文本
        /// </summary>
        /// <param name="text">提示文本</param>
        public void SetToolTip(string text)
        {
            if (_trayIcon != null)
            {
                _trayIcon.ToolTipText = text;
            }
        }
        
        /// <summary>
        /// 更新托盘图标提示文本
        /// </summary>
        /// <param name="toolTip">提示文本</param>
        public void UpdateToolTip(string toolTip)
        {
            // 直接调用现有的SetToolTip方法
            SetToolTip(toolTip);
        }

        /// <summary>
        /// 最小化主窗口到托盘
        /// </summary>
        public void MinimizeToTray()
        {
            OnHideWindowRequested();
        }

        /// <summary>
        /// 从托盘还原主窗口
        /// </summary>
        public void RestoreFromTray()
        {
            OnShowWindowRequested();
        }

        /// <summary>
        /// 触发窗口显示事件
        /// </summary>
        private void OnShowWindowRequested()
        {
            ShowWindowRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 触发窗口隐藏事件
        /// </summary>
        private void OnHideWindowRequested()
        {
            HideWindowRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 触发应用退出事件
        /// </summary>
        private void OnExitRequested()
        {
            _isExitFromTray = true;
            ExitRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 创建托盘图标的上下文菜单
        /// </summary>
        /// <returns>上下文菜单</returns>
        private ContextMenu CreateContextMenu()
        {
            var menu = new ContextMenu();

            // 显示主窗口菜单项
            var showMenuItem = new MenuItem { Header = "显示主窗口" };
            showMenuItem.Click += (sender, e) => OnShowWindowRequested();
            menu.Items.Add(showMenuItem);

            // 分隔符
            menu.Items.Add(new Separator());

            // 退出菜单项
            var exitMenuItem = new MenuItem { Header = "退出" };
            exitMenuItem.Click += (sender, e) => OnExitRequested();
            menu.Items.Add(exitMenuItem);

            return menu;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_trayIcon != null)
            {
                _trayIcon.Dispose();
                _trayIcon = null;
            }
        }
    }
}
