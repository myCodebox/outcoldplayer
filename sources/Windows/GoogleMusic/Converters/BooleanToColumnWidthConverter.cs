// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    public class BooleanToColumnWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value)
            {
                return new GridLength(1, GridUnitType.Star);
            }

            return new GridLength(0, GridUnitType.Pixel);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
