using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;


namespace farmmanager.Helpers
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool boolValue = value is bool b && b;
            bool invert = parameter?.ToString() == "Invert";
            return (boolValue ^ invert) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => value is Visibility v && v == Visibility.Visible;
    }

    public class DecimalToCurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal d) return d.ToString("N0") + " FCFA";
            if (value is double dbl) return dbl.ToString("N0") + " FCFA";
            return "0 FCFA";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var str = value?.ToString()?.Replace("FCFA", "").Replace(",", "").Trim();
            return decimal.TryParse(str, out var result) ? result : 0m;
        }
    }

    public class CompletionRateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            decimal rate = value is decimal d ? d : 0;
            if (rate >= 100) return new SolidColorBrush(Colors.Green);
            if (rate >= 70) return new SolidColorBrush(Colors.DarkOrange);
            return new SolidColorBrush(Colors.Crimson);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isNull = value == null;
            bool invert = parameter?.ToString() == "Invert";
            return (isNull ^ invert) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dt)
            {
                string format = parameter?.ToString() ?? "dd/MM/yyyy";
                return dt.ToString(format);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => DateTime.TryParse(value?.ToString(), out var dt) ? dt : DateTime.Today;
    }

    public class ZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int count = value is int i ? i : 0;
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

}
