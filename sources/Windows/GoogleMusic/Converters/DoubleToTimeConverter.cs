// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    public class DoubleToTimeConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is double))
            {
                return DependencyProperty.UnsetValue;
            }

            var timeSpan = TimeSpan.FromSeconds((double)value);
            return string.Format("{0:N0}:{1:00}", timeSpan.Subtract(TimeSpan.FromSeconds(timeSpan.Seconds)).TotalMinutes, timeSpan.Seconds);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}