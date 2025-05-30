using JiraTool.Data;
using JiraTool.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiraTool.Services
{
    /// <summary>
    /// 数据库服务接口
    /// </summary>
    public interface IDatabaseService
    {
        Task<List<JiraSubTask>> GetAllTasksAsync();
        Task<List<JiraTask>> GetAllParentTasksAsync();
        Task<bool> SaveTasksAsync(List<JiraSubTask> subTasks);
        Task<bool> SaveParentTasksAsync(List<JiraTask> parentTasks);
        Task<bool> UpdateTasksAsync(List<JiraSubTask> subTasks);
        Task<UserSettings> GetUserSettingsAsync();
        Task<bool> SaveUserSettingsAsync(UserSettings settings);
        Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);
    }

    /// <summary>
    /// 数据库服务类，提供对数据库的操作
    /// </summary>
    public class DatabaseService : IDatabaseService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="contextFactory">数据库上下文工厂</param>
        public DatabaseService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// 获取用户设置
        /// </summary>
        /// <returns>用户设置对象</returns>
        public async Task<UserSettings> GetUserSettingsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var settings = await context.UserSettings.FirstOrDefaultAsync();
            return settings ?? new UserSettings { Id = 1 };
        }

        /// <summary>
        /// 保存用户设置
        /// </summary>
        /// <param name="settings">用户设置对象</param>
        /// <returns>保存操作的任务</returns>
        public async Task<bool> SaveUserSettingsAsync(UserSettings settings)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var existingSettings = await context.UserSettings.FirstOrDefaultAsync();

            if (existingSettings == null)
            {
                context.UserSettings.Add(settings);
            }
            else
            {
                existingSettings.Username = settings.Username;
                existingSettings.EncryptedPassword = settings.EncryptedPassword;
                existingSettings.JiraServerUrl = settings.JiraServerUrl;
                existingSettings.RememberPassword = settings.RememberPassword;
                existingSettings.AutoLogin = settings.AutoLogin;
                existingSettings.WindowWidth = settings.WindowWidth;
                existingSettings.WindowHeight = settings.WindowHeight;
                existingSettings.MinimizeToTrayOnStart = settings.MinimizeToTrayOnStart;
                existingSettings.MinimizeToTrayOnClose = settings.MinimizeToTrayOnClose;
                existingSettings.RefreshInterval = settings.RefreshInterval;
                
                // 列设置
                existingSettings.ShowTaskNameColumn = settings.ShowTaskNameColumn;
                existingSettings.ShowTaskNumberColumn = settings.ShowTaskNumberColumn;
                existingSettings.ShowParentTaskNumberColumn = settings.ShowParentTaskNumberColumn;
                existingSettings.ShowTaskStatusColumn = settings.ShowTaskStatusColumn;
                existingSettings.ShowParentTaskStatusColumn = settings.ShowParentTaskStatusColumn;
                existingSettings.ShowEstimatedHoursColumn = settings.ShowEstimatedHoursColumn;
                existingSettings.ShowEstimatedCompletionTimeColumn = settings.ShowEstimatedCompletionTimeColumn;
                existingSettings.ShowActualCompletionTimeColumn = settings.ShowActualCompletionTimeColumn;
                existingSettings.ShowCreatedAtColumn = settings.ShowCreatedAtColumn;
                existingSettings.ShowUpdatedAtColumn = settings.ShowUpdatedAtColumn;
                existingSettings.ShowPriorityColumn = settings.ShowPriorityColumn;
                existingSettings.ShowBanCheNameColumn = settings.ShowBanCheNameColumn;
                existingSettings.ShowCodeMergedColumn = settings.ShowCodeMergedColumn;
                existingSettings.ShowSqlColumn = settings.ShowSqlColumn;
                existingSettings.ShowConfigurationColumn = settings.ShowConfigurationColumn;
            }

            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 获取所有子任务，包括父任务信息
        /// </summary>
        /// <returns>子任务列表</returns>
        public async Task<List<JiraSubTask>> GetAllTasksAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 使用Include来加载父任务信息，并使用AsNoTracking来避免实体跟踪冲突
                var subTasks = await context.SubTasks
                    .AsNoTracking()
                    .Include(st => st.ParentTask)
                    .OrderByDescending(st => st.UpdatedAt)
                    .ToListAsync();
                
                // 创建新的实例以避免实体跟踪冲突
                var result = new List<JiraSubTask>();
                foreach (var subTask in subTasks)
                {
                    var newSubTask = new JiraSubTask
                    {
                        Id = subTask.Id,
                        TaskNumber = subTask.TaskNumber,
                        TaskName = subTask.TaskName,
                        Status = subTask.Status,
                        Priority = subTask.Priority,
                        EstimatedHours = subTask.EstimatedHours,
                        EstimatedCompletionTime = subTask.EstimatedCompletionTime,
                        ActualCompletionTime = subTask.ActualCompletionTime,
                        CreatedAt = subTask.CreatedAt,
                        UpdatedAt = subTask.UpdatedAt,
                        IsCodeMerged = subTask.IsCodeMerged,
                        HasSqlScript = subTask.HasSqlScript,
                        SqlScript = subTask.SqlScript,
                        HasConfiguration = subTask.HasConfiguration,
                        Configuration = subTask.Configuration,
                        TaskUrl = subTask.TaskUrl,
                        ParentTaskId = subTask.ParentTaskId
                    };
                    
                    // 如果有父任务，创建一个新的父任务实例
                    if (subTask.ParentTask != null)
                    {
                        newSubTask.ParentTask = new JiraTask
                        {
                            Id = subTask.ParentTask.Id,
                            TaskNumber = subTask.ParentTask.TaskNumber,
                            Description = subTask.ParentTask.Description,
                            Status = subTask.ParentTask.Status,
                            BanCheName = subTask.ParentTask.BanCheName,
                            PreviousBanCheName = subTask.ParentTask.PreviousBanCheName,
                            TaskUrl = subTask.ParentTask.TaskUrl,
                            CreatedAt = subTask.ParentTask.CreatedAt,
                            UpdatedAt = subTask.ParentTask.UpdatedAt
                        };
                    }
                    
                    result.Add(newSubTask);
                }
                
                System.Diagnostics.Debug.WriteLine($"从数据库中获取了 {result.Count} 条子任务数据");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取子任务失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 执行原始SQL语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    var result = await context.Database.ExecuteSqlRawAsync(sql, parameters);
                    await transaction.CommitAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine($"执行SQL失败: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"内部异常: {ex.InnerException.Message}");
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"执行SQL失败 (未知异常): {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取所有父任务
        /// </summary>
        /// <returns>父任务列表</returns>
        public async Task<List<JiraTask>> GetAllParentTasksAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var tasks = await context.Tasks
                    .AsNoTracking()
                    .ToListAsync();

                // 创建新的实例以避免实体跟踪冲突
                var result = tasks.Select(task => new JiraTask
                {
                    Id = task.Id,
                    TaskNumber = task.TaskNumber,
                    Description = task.Description,
                    Status = task.Status,
                    BanCheName = task.BanCheName,
                    PreviousBanCheName = task.PreviousBanCheName,
                    PreviousDescription = task.PreviousDescription,
                    IsBanCheNameChangeNotified = task.IsBanCheNameChangeNotified,
                    IsDescriptionChangeNotified = task.IsDescriptionChangeNotified,
                    TaskUrl = task.TaskUrl,
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt,
                    Priority = task.Priority,
                    Visibility = task.Visibility
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取父任务失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 保存父任务
        /// </summary>
        /// <param name="parentTasks">父任务列表</param>
        /// <returns>是否保存成功</returns>
        public async Task<bool> SaveParentTasksAsync(List<JiraTask> parentTasks)
        {
            if (parentTasks == null || !parentTasks.Any())
            {
                return true;
            }

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                await using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    await context.Tasks.AddRangeAsync(parentTasks);
                    await context.SaveChangesAsync();
                    
                    // 提交事务
                    await transaction.CommitAsync();
                    
                    // 清除变更跟踪器，避免实体跟踪冲突
                    context.ChangeTracker.Clear();
                    
                    return true;
                }
                catch (DbUpdateException dbEx)
                {
                    await transaction.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine($"保存父任务失败 (DbUpdateException): {dbEx.Message}");
                    
                    if (dbEx.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"内部异常: {dbEx.InnerException.Message}");
                    }
                    
                    throw;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine($"保存父任务失败: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存父任务失败 (未知异常): {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 保存任务列表到数据库
        /// </summary>
        /// <param name="subTasks">任务列表</param>
        /// <returns>是否保存成功</returns>
        public async Task<bool> SaveTasksAsync(List<JiraSubTask> subTasks)
        {
            if (subTasks == null || !subTasks.Any())
            {
                return true;
            }

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                await using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    await context.SubTasks.AddRangeAsync(subTasks);
                    await context.SaveChangesAsync();
                    
                    // 提交事务
                    await transaction.CommitAsync();
                    
                    // 清除变更跟踪器，避免实体跟踪冲突
                    context.ChangeTracker.Clear();
                    
                    System.Diagnostics.Debug.WriteLine($"保存了 {subTasks.Count} 条子任务数据到数据库");
                    return true;
                }
                catch (DbUpdateException dbEx)
                {
                    await transaction.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine($"保存任务失败 (DbUpdateException): {dbEx.Message}");
                    
                    if (dbEx.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"内部异常: {dbEx.InnerException.Message}");
                    }
                    
                    throw;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine($"保存任务失败: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存任务失败 (未知异常): {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 更新任务列表到数据库
        /// </summary>
        /// <param name="subTasks">任务列表</param>
        /// <returns>是否更新成功</returns>
        public async Task<bool> UpdateTasksAsync(List<JiraSubTask> subTasks)
        {
            if (subTasks == null || !subTasks.Any())
            {
                return true;
            }

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                await using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    foreach (var subTask in subTasks)
                    {
                        var existingTask = await context.SubTasks
                            .AsNoTracking()
                            .FirstOrDefaultAsync(t => t.Id == subTask.Id);
                            
                        if (existingTask != null)
                        {
                            context.Entry(subTask).State = EntityState.Modified;
                        }
                        else
                        {
                            context.SubTasks.Add(subTask);
                        }
                    }
                    
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    context.ChangeTracker.Clear();
                    
                    System.Diagnostics.Debug.WriteLine($"更新了 {subTasks.Count} 条子任务数据到数据库");
                    return true;
                }
                catch (DbUpdateException dbEx)
                {
                    await transaction.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine($"更新任务失败 (DbUpdateException): {dbEx.Message}");
                    
                    if (dbEx.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"内部异常: {dbEx.InnerException.Message}");
                    }
                    
                    throw;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine($"更新任务失败: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新任务失败 (未知异常): {ex.Message}");
                throw;
            }
        }
    }
}
