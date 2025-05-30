using System;

namespace JiraTool.Services.Interfaces
{
    /// <summary>
    /// 系统托盘图标服务接口
    /// </summary>
    public interface ITrayIconService
    {
        /// <summary>
        /// 初始化托盘图标
        /// </summary>
        void Initialize();

        /// <summary>
        /// 显示托盘图标
        /// </summary>
        void Show();

        /// <summary>
        /// 隐藏托盘图标
        /// </summary>
        void Hide();
        
        /// <summary>
        /// 更新托盘图标提示文本
        /// </summary>
        /// <param name="toolTip">提示文本</param>
        void UpdateToolTip(string toolTip);

        /// <summary>
        /// 设置托盘图标工具提示文本
        /// </summary>
        /// <param name="text">提示文本</param>
        void SetToolTip(string text);

        /// <summary>
        /// 最小化主窗口到托盘
        /// </summary>
        void MinimizeToTray();

        /// <summary>
        /// 从托盘还原主窗口
        /// </summary>
        void RestoreFromTray();

        /// <summary>
        /// 用户是否选择从托盘退出
        /// </summary>
        bool IsExitFromTray { get; }

        /// <summary>
        /// 窗口显示事件
        /// </summary>
        event EventHandler ShowWindowRequested;

        /// <summary>
        /// 窗口隐藏事件
        /// </summary>
        event EventHandler HideWindowRequested;

        /// <summary>
        /// 应用退出事件
        /// </summary>
        event EventHandler ExitRequested;
    }
}
