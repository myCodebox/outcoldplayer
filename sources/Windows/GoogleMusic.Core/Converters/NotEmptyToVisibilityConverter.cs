// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class NotEmptyToVisibilityConverter : VisibilityConverterBase 
    {
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            var enumerable = value as IEnumerable<object>;
            return this.ConvertToVisibility(enumerable != null && enumerable.Any());
        }

        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
