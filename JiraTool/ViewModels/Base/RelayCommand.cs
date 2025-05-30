using System;
using System.Windows.Input;

namespace JiraTool.ViewModels.Base
{
    /// <summary>
    /// 中继命令类，实现ICommand接口以支持MVVM命令绑定
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// 可执行状态变更事件
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="execute">执行方法</param>
        /// <param name="canExecute">可执行条件判断方法，可为null</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 判断命令是否可以执行
        /// </summary>
        /// <param name="parameter">命令参数</param>
        /// <returns>如果可以执行则返回true，否则返回false</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">命令参数</param>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
        
        /// <summary>
        /// 手动触发CanExecuteChanged事件
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// 泛型中继命令类，实现ICommand接口以支持MVVM命令绑定
    /// </summary>
    /// <typeparam name="T">命令参数类型</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Predicate<T?>? _canExecute;

        /// <summary>
        /// 可执行状态变更事件
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="execute">执行方法</param>
        /// <param name="canExecute">可执行条件判断方法，可为null</param>
        public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 判断命令是否可以执行
        /// </summary>
        /// <param name="parameter">命令参数</param>
        /// <returns>如果可以执行则返回true，否则返回false</returns>
        public bool CanExecute(object? parameter)
        {
            if (parameter == null && typeof(T).IsValueType && !IsNullableType(typeof(T)))
                return false;

            return _canExecute == null || _canExecute((T?)parameter);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">命令参数</param>
        public void Execute(object? parameter)
        {
            _execute((T?)parameter);
        }

        // 判断类型是否为可空类型
        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        
        /// <summary>
        /// 手动触发CanExecuteChanged事件
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
