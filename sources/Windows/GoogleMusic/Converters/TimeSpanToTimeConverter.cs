// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    public class TimeSpanToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is TimeSpan))
            {
                return DependencyProperty.UnsetValue;
            }

            return ((TimeSpan)value).ToPresentString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
