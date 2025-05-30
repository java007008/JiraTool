using JiraTool.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JiraTool.Services.Interfaces
{
    /// <summary>
    /// Jira服务接口
    /// </summary>
    public interface IJiraService
    {
        /// <summary>
        /// 用户名
        /// </summary>
        string Username { get; }

        /// <summary>
        /// 是否已登录
        /// </summary>
        bool IsLoggedIn { get; }

        /// <summary>
        /// 登录Jira
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="jiraUrl">Jira服务器地址</param>
        /// <returns>登录是否成功</returns>
        Task<bool> LoginAsync(string username, string password, string jiraUrl);

        /// <summary>
        /// 登出Jira
        /// </summary>
        /// <returns>操作任务</returns>
        Task LogoutAsync();

        /// <summary>
        /// 获取用户的任务列表
        /// </summary>
        /// <returns>任务列表</returns>
        Task<List<JiraSubTask>> GetUserTasksAsync();
        
        /// <summary>
        /// 获取所有任务
        /// </summary>
        /// <returns>任务列表</returns>
        Task<List<JiraSubTask>> GetTasksAsync();

        /// <summary>
        /// 开始开发子任务
        /// </summary>
        /// <param name="taskNumber">子任务单号</param>
        /// <returns>操作是否成功</returns>
        Task<bool> StartDevelopmentAsync(string taskNumber);

        /// <summary>
        /// 完成子任务开发
        /// </summary>
        /// <param name="taskNumber">子任务单号</param>
        /// <returns>操作是否成功</returns>
        Task<bool> CompleteDevelopmentAsync(string taskNumber);

        /// <summary>
        /// 分配任务
        /// </summary>
        /// <param name="taskNumber">子任务单号</param>
        /// <param name="username">用户名</param>
        /// <returns>操作是否成功</returns>
        Task<bool> AssignTaskAsync(string taskNumber, string username);

        /// <summary>
        /// 获取Jira任务的完整URL
        /// </summary>
        /// <param name="taskNumber">任务单号</param>
        /// <returns>完整URL</returns>
        string GetTaskUrl(string taskNumber);

        /// <summary>
        /// 最后一次数据获取时间
        /// </summary>
        DateTime LastUpdateTime { get; }

        /// <summary>
        /// 任务数据更新事件
        /// </summary>
        event EventHandler TasksUpdated;
    }
}
