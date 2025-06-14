using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace TaskManagerApp.Converters
{
    public sealed class TagsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is List<string> tags)
            {
                return string.Join(", ", tags);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
