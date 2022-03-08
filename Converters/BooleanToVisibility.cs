using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Rich_Text_Editor.Converters
{
    public class BooleanToVisibility : IValueConverter
    {
        public bool Reverse { get; set; }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool boolean = (bool)value;

            if (Reverse)
            {
                return boolean ? Visibility.Collapsed : Visibility.Visible;
            }

            return boolean ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
