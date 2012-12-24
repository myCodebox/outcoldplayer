// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    public class ValueToVisibilityConverter : IValueConverter 
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool result;
            if (parameter == null)
            {
                result = value == null;
            }
            else if (value is double)
            {
                var d = System.Convert.ToDouble(parameter);
                result = Math.Abs(((double)value) - d) < 0.001;
            }
            else
            {
                throw new NotImplementedException();
            }

            if (this.Invert)
            {
                result = !result;
            }

            return result ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}