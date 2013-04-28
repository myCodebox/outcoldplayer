// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;
    using System.Globalization;

    using Windows.UI.Xaml.Data;

    public class SizeToStringConverter : IValueConverter
    {
        private static readonly string[] Scales = new string[]
                                                      {
                                                          "bytes",
                                                          "KB",
                                                          "MB",
                                                          "GB"
                                                      };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double size = (long)value;

            int scaleIndex = 0;
            while (size > 1024 && scaleIndex < Scales.Length)
            {
                scaleIndex++;
                size = size / 1024;
            }

            if (scaleIndex == 0)
            {
                return string.Format(new CultureInfo(language), "{0:N0} {1}", size, Scales[scaleIndex]);
            }
            else
            {
                return string.Format(new CultureInfo(language), "{0:N2} {1}", size, Scales[scaleIndex]);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}