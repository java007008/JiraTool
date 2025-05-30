using System;

namespace JiraTool.Controls.Notification
{
    /// <summary>
    /// 通知示例类，展示如何使用新的通知系统
    /// </summary>
    public static class NotificationExample
    {
        /// <summary>
        /// 显示各种类型的通知示例
        /// </summary>
        public static void ShowExamples()
        {
            // 显示信息通知
            HandyNotificationManager.Info("这是一条信息通知", "信息");
            
            // 显示成功通知
            HandyNotificationManager.Success("操作已成功完成", "成功");
            
            // 显示警告通知
            HandyNotificationManager.Warning("请注意可能的问题", "警告");
            
            // 显示错误通知
            HandyNotificationManager.Error("操作失败，请重试", "错误");
            
            // 显示询问通知（带回调）
            HandyNotificationManager.Question(
                "您确定要执行此操作吗？", 
                "确认", 
                0, // 持续时间为0，表示不自动关闭
                data => Console.WriteLine("用户关闭了询问通知"), // 关闭回调
                data => Console.WriteLine("用户确认了操作") // 点击确认按钮回调
            );
        }
        
        /// <summary>
        /// 从旧的通知系统迁移到新的通知系统
        /// </summary>
        public static void MigrationGuide()
        {
            // 旧的通知系统调用
            // NotificationManager.Info("这是一条信息通知", "信息");
            
            // 新的通知系统调用
            HandyNotificationManager.Info("这是一条信息通知", "信息");
            
            // 两者的API完全兼容，只需将 NotificationManager 替换为 HandyNotificationManager 即可
        }
    }
}
