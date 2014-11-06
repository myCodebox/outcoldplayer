// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml;
    using OutcoldSolutions.GoogleMusic.Services;

    public class RepeatToStyleConverter : IValueConverter
    {
        public Style RepeatOneStyle { get; set; }
        public Style RepeatAllStyle { get; set; }
        public Style RepeatNoneStyle { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var repeatType = (RepeatType)value;

            if (repeatType == RepeatType.One)
            {
                return this.RepeatOneStyle;
            } 
            else if (repeatType == RepeatType.All)
            {
                return this.RepeatAllStyle;
            }
            else
            {
                return this.RepeatNoneStyle;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
