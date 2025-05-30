using JiraTool.Services.Interfaces;
using JiraTool.ViewModels.Base;
using System;
using System.Windows;
using System.Windows.Input;

namespace JiraTool.ViewModels
{
    /// <summary>
    /// 系统托盘图标视图模型
    /// </summary>
    public class TrayIconViewModel : ViewModelBase
    {
        private readonly ITrayIconService _trayIconService;
        private string _toolTipText = "Jira客户端工具";

        /// <summary>
        /// 托盘图标提示文本
        /// </summary>
        public string ToolTipText
        {
            get => _toolTipText;
            set
            {
                if (SetProperty(ref _toolTipText, value))
                {
                    _trayIconService.SetToolTip(value);
                }
            }
        }

        /// <summary>
        /// 显示窗口命令
        /// </summary>
        public ICommand ShowWindowCommand { get; }

        /// <summary>
        /// 退出应用命令
        /// </summary>
        public ICommand ExitApplicationCommand { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="trayIconService">托盘图标服务</param>
        public TrayIconViewModel(ITrayIconService trayIconService)
        {
            _trayIconService = trayIconService;

            // 初始化命令
            ShowWindowCommand = new RelayCommand<object>(_ => _trayIconService.RestoreFromTray());
            ExitApplicationCommand = new RelayCommand<object>(_ => Application.Current.Shutdown());

            // 初始化托盘图标
            _trayIconService.Initialize();
            _trayIconService.SetToolTip(_toolTipText);
        }

        /// <summary>
        /// 设置托盘图标提示文本
        /// </summary>
        /// <param name="text">提示文本</param>
        public void SetToolTip(string text)
        {
            ToolTipText = text;
        }

        /// <summary>
        /// 最小化主窗口到托盘
        /// </summary>
        public void MinimizeToTray()
        {
            _trayIconService.MinimizeToTray();
        }

        /// <summary>
        /// 从托盘还原主窗口
        /// </summary>
        public void RestoreFromTray()
        {
            _trayIconService.RestoreFromTray();
        }
    }
}
