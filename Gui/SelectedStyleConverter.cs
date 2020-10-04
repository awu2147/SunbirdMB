using SharpDX.MediaFoundation;
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
            SelectionMode dataValue = (SelectionMode)values[0];
            Style defaultStyle = values[1] as Style;
            Style selectedStyle = values[2] as Style;
            Style activeStyle = values[3] as Style;

            switch (dataValue)
            {
                case SelectionMode.None:
                    return defaultStyle;
                case SelectionMode.Selected:
                    return selectedStyle;
                case SelectionMode.Active:
                    return activeStyle;
                default:
                    return defaultStyle;
            }
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


    public class BuildModeStyleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BuildMode dataValue = (BuildMode)values[0];
            Style firstStyle = values[1] as Style;
            Style secondStyle = values[2] as Style;

            return dataValue.Equals(BuildMode.Cube) ? firstStyle : secondStyle;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
