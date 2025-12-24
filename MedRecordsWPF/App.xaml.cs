using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MedRecordsWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    public class BoolToGridLengthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || !(values[0] is bool show)) return new GridLength(0);
            double max = System.Convert.ToDouble(parameter);          // 300 in XAML
            return show ? new GridLength(max) : new GridLength(0);
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    /// </summary>
    public partial class App : Application
    {
    }

}
