using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Data;
using ASCOM.Utilities;

namespace AstroSimulator
{
    public class HoursToHMSConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            Util ascomUtil = new Util();
            return ascomUtil.HoursToHMS((Double)value);
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            Util ascomUtil = new Util();
            return ascomUtil.HMSToHours((String)value);
        }
    }
}
