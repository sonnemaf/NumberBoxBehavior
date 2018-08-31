using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace NumberBoxDemo.Converters {
    class IntToNullableDoubleConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return new double?((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return (int)((double?)value);
        }
    }
}
