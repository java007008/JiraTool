using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JiraTool.ViewModels.Base
{
    /// <summary>
    /// 视图模型基类，实现INotifyPropertyChanged接口以支持数据绑定
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性变更事件
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 触发属性变更通知
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 设置属性值并在值发生变化时触发属性变更通知
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">属性字段引用</param>
        /// <param name="value">新的属性值</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>如果值已更改则返回true，否则返回false</returns>
        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 设置属性值并在值发生变化时触发属性变更通知和自定义操作
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">属性字段引用</param>
        /// <param name="value">新的属性值</param>
        /// <param name="action">在属性变更后执行的操作</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>如果值已更改则返回true，否则返回false</returns>
        protected virtual bool SetProperty<T>(ref T field, T value, Action action, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            action();
            return true;
        }
    }
}
