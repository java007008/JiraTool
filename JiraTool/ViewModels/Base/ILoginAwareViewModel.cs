namespace JiraTool.ViewModels.Base
{
    /// <summary>
    /// 表示视图模型需要感知登录状态
    /// </summary>
    public interface ILoginAwareViewModel
    {
        /// <summary>
        /// 是否已登录
        /// </summary>
        bool IsLoggedIn { get; set; }
    }
}
