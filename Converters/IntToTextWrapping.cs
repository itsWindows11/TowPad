using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Rich_Text_Editor.Converters
{
    public class IntToTextWrapping : IValueConverter
    {
        public bool Reverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int val = (int)value;

            if (Reverse)
            {
                return val;
            }

            return val switch
            {
                0 => TextWrapping.NoWrap,
                1 => TextWrapping.Wrap,
                2 => TextWrapping.WrapWholeWords,
                _ => TextWrapping.NoWrap,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
