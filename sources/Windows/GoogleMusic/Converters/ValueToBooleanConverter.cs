// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;
    using System.Globalization;

    using Windows.UI.Xaml.Data;

    public class ValueToBooleanConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool result;
            if (parameter == null)
            {
                result = value == null;
            }
            else if (value == null)
            {
                result = false;
            }
            else if (value is double)
            {
                var d = System.Convert.ToDouble(parameter, this.GetCultureInfo(language));
                result = Math.Abs(((double)value) - d) < 0.00001;
            }
            else if (value is Enum)
            {
                result = value.Equals(Enum.ToObject(value.GetType(), parameter));
            }
            else
            {
                object parameterValue = System.Convert.ChangeType(parameter, value.GetType(), this.GetCultureInfo(language));
                result = value.Equals(parameterValue);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private CultureInfo GetCultureInfo(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                return CultureInfo.CurrentCulture;
            }

            return new CultureInfo(language);
        }
    }
}