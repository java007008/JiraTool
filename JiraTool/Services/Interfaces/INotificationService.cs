using System;
using System.Threading.Tasks;

namespace JiraTool.Services.Interfaces
{
    /// <summary>
    /// 通知服务接口
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// 显示班车名变更通知
        /// </summary>
        /// <param name="taskNumber">任务单号</param>
        /// <param name="oldBanCheName">旧班车名</param>
        /// <param name="newBanCheName">新班车名</param>
        /// <returns>用户是否点击了确认</returns>
        Task<bool> ShowBanCheNameChangeNotificationAsync(string taskNumber, string oldBanCheName, string newBanCheName);

        /// <summary>
        /// 显示主任务描述变更通知
        /// </summary>
        /// <param name="taskNumber">任务单号</param>
        /// <returns>用户是否点击了确认</returns>
        Task<bool> ShowDescriptionChangeNotificationAsync(string taskNumber);

        /// <summary>
        /// 显示一般信息通知
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">消息内容</param>
        void ShowInformation(string title, string message);

        /// <summary>
        /// 显示错误通知
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">错误信息</param>
        void ShowError(string title, string message);
        
        /// <summary>
        /// 显示成功通知
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">成功信息</param>
        void ShowSuccess(string title, string message);

        /// <summary>
        /// 显示确认对话框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">消息内容</param>
        /// <returns>用户是否确认</returns>
        Task<bool> ShowConfirmationAsync(string title, string message);

        /// <summary>
        /// 显示SQL输入对话框
        /// </summary>
        /// <param name="taskNumber">任务单号</param>
        /// <param name="existingSql">现有SQL内容</param>
        /// <returns>用户输入的SQL，如果取消则返回null</returns>
        Task<string?> ShowSqlInputAsync(string taskNumber, string existingSql);

        /// <summary>
        /// 显示配置输入对话框
        /// </summary>
        /// <param name="taskNumber">任务单号</param>
        /// <param name="existingConfig">现有配置内容</param>
        /// <returns>用户输入的配置，如果取消则返回null</returns>
        Task<string?> ShowConfigInputAsync(string taskNumber, string existingConfig);

        /// <summary>
        /// 班车名变更确认事件
        /// </summary>
        event EventHandler<string> BanCheNameChangeConfirmed;

        /// <summary>
        /// 描述变更确认事件
        /// </summary>
        event EventHandler<string> DescriptionChangeConfirmed;
    }
}
