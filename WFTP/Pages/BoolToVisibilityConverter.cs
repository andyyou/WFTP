using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace WFTP.Pages
{
    /// <summary>
    /// Converter 給 xaml 轉換 Binding 資料 : Bool 轉 Width 60px
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null || !((bool)value)) return 0.0;

                return 60;
            }
            catch (InvalidCastException) { }
            return "<Unknown Value>";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    /// <summary>
    /// Converter 給 xaml 轉換 Binding 資料 : Bool 轉 Width 120px
    /// </summary>
    public class BoolToBigVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null || !((bool)value)) return 0.0;

                return 120;
            }
            catch (InvalidCastException) { }
            return "<Unknown Value>";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    /// <summary>
    /// Converter 給 xaml 轉換 Binding 資料 : Progress 使用 如果進度 100% 則隱藏掉.
    /// </summary>
    public class CancelToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null || ((int)value == 100)) return 0.0;

                return 55;
            }
            catch (InvalidCastException) { }
            return "<Unknown Value>";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
