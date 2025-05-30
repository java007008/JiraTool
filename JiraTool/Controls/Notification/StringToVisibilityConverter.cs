using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JiraTool.Controls.Notification
{
    /// <summary>
    /// 字符串转Visibility转换器
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// 转换
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return string.IsNullOrEmpty(str) ? Visibility.Collapsed : Visibility.Visible;
            }
            
            return Visibility.Collapsed;
        }

        /// <summary>
        /// 反向转换
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
