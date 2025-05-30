using System.Windows;
using JiraTool.Models.Base;

namespace JiraTool.Models
{
    /// <summary>
    /// 任务可见性基类
    /// </summary>
    public class TaskVisibility : NotifyPropertyChanged
    {
        private Visibility _visibility = Visibility.Visible;

        /// <summary>
        /// 可见性
        /// </summary>
        public Visibility Visibility
        {
            get => _visibility;
            set => SetProperty(ref _visibility, value);
        }
    }
}
