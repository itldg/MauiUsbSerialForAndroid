using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiUsbSerialForAndroid.Converter
{
    public class IntToHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string hex = ((int)value).ToString("X");
            //如果长度是单数就补0
            if (hex.Length%2!=0)
            {
                hex = "0" + hex;
            }
            return hex;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(value);
        }
    }
}
