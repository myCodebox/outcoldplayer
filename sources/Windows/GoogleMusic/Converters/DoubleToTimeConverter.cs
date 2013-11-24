// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;

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

            var doubleValue = (double) value;

            if (0 <= doubleValue && doubleValue <= TimeSpan.MaxValue.TotalSeconds)
            {
                return TimeSpan.FromSeconds((double)value).ToPresentString();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}