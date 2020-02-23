using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MemEditGUI.Converters
{
    public class NullConverter<T> : IValueConverter
    {
        public NullConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }
    }
    public sealed class NullToVisibilityConverter : NullConverter<Visibility>
    {
        public NullToVisibilityConverter() :
            base(Visibility.Collapsed, Visibility.Visible)
        { }
    }
    public sealed class NullToBoolConverter : NullConverter<bool>
    {
        public NullToBoolConverter() : base(true, false)
        { }
    }
    public sealed class NullToBoolInverseConverter : NullConverter<bool>
    {
        public NullToBoolInverseConverter() : base(false, true)
        { }
    }
}
