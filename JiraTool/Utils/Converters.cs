using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JiraTool.Utils
{
    /// <summary>
    /// 布尔值转可见性转换器
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// 转换方法
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue && boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 反向转换方法
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }

    /// <summary>
    /// 布尔值转SQL图标转换器
    /// </summary>
    public class BooleanToSqlIconConverter : IValueConverter
    {
        /// <summary>
        /// 转换方法
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue && boolValue ? "Database" : "DatabasePlus";
        }

        /// <summary>
        /// 反向转换方法
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string stringValue && stringValue == "Database";
        }
    }

    /// <summary>
    /// 布尔值转配置图标转换器
    /// </summary>
    public class BooleanToConfigIconConverter : IValueConverter
    {
        /// <summary>
        /// 转换方法
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue && boolValue ? "Cog" : "CogPlus";
        }

        /// <summary>
        /// 反向转换方法
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string stringValue && stringValue == "Cog";
        }
    }
}
