using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace JiraTool.Converters
{
    /// <summary>
    /// 布尔值到可见性转换器
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// 布尔值到SQL图标转换器
    /// </summary>
    public class BooleanToSqlIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? PackIconKind.Database : PackIconKind.DatabaseOff;
            }
            return PackIconKind.DatabaseOff;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 布尔值到配置图标转换器
    /// </summary>
    public class BooleanToConfigIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? PackIconKind.Cog : PackIconKind.CogOff;
            }
            return PackIconKind.CogOff;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
