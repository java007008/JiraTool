# Jira客户端工具

一个用于跟踪和管理Jira任务的WPF客户端工具，支持通过网页抓取获取数据。

## 功能特点

- 通过网页抓取方式获取Jira任务数据
- 显示用户的待处理任务列表
- 支持班车名和任务描述变更自动通知
- 可自定义显示/隐藏列
- 支持SQL和配置信息管理
- 系统托盘功能
- MaterialDesign深色主题界面

## 系统要求

- Windows操作系统
- .NET 6.0 或更高版本
- Visual Studio 2022

## 如何使用

1. 使用Visual Studio 2022打开`JiraTool.sln`解决方案文件
2. 还原NuGet包
3. 编译并运行项目

## 项目结构

- **Models**: 数据模型类，如JiraTask、JiraSubTask等
- **ViewModels**: MVVM架构的视图模型类
- **Views**: WPF界面文件
- **Services**: 服务类，如JiraWebService、NotificationService等
- **Data**: 数据库相关类，包括EF Core上下文和仓储
- **Utils**: 工具类，如WebScraper、CefBrowserHelper等

## 注意事项

在使用前，您需要:

1. 添加应用图标和托盘图标到`Assets/Icons`目录:
   - app.ico
   - tray.ico
   
2. 在`JiraWebService.cs`中配置实际的Jira网站结构和API调用:
   - 根据实际的Jira系统结构修改网页抓取逻辑
   - 添加实际的登录处理逻辑

## 后续开发计划

- 实现开始开发、完成开发等任务状态管理功能
- 添加更多任务操作和自动化功能
- 完善SQL和配置信息的编辑界面
