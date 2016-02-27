using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace AstroSimulator
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            if ((value == null) || (parameter == null))
                return false;
            return value.ToString().Equals(parameter.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            if ((value == null) || (parameter == null))
                return false;
            return ((Boolean)value ? Enum.Parse(targetType, parameter.ToString()) : null);
        }
    }
}
