using SunbirdMB.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SunbirdMB.Gui
{
    public class SelectedStyleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool? dataValue = values[0] as bool?;
            Style firstStyle = values[1] as Style;
            Style secondStyle = values[2] as Style;

            return dataValue.Equals(false) ? firstStyle : secondStyle;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class AuthorizationStyleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Authorization dataValue = (Authorization)values[0];
            Style firstStyle = values[1] as Style;
            Style secondStyle = values[2] as Style;

            return dataValue.Equals(Authorization.None) ? firstStyle : secondStyle;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
