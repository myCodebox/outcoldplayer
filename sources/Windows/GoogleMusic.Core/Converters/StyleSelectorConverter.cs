// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    using OutcoldSolutions.GoogleMusic.Shell;

    public class ControlStyles
    {
        public Style Large { get; set; }

        public Style Medium { get; set; }

        public Style Small { get; set; }
    }

    public class StyleSelectorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var applicationSize = (ApplicationSize)value;
            var controlStyles = (ControlStyles)parameter;

            if (applicationSize.IsLarge)
            {
                return controlStyles.Large;
            }

            if (applicationSize.IsMedium)
            {
                return controlStyles.Medium;
            }

            return controlStyles.Small;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
