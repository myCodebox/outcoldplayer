// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    /// <summary>
    /// UI Converter between <see cref="System.Boolean"/> and <see cref="Visibility"/>.
    /// By default it converts <value>true</value> to <value>System.Visible</value> and 
    /// <value>false</value> to <value>System.Collapsed</value>, but you can change this behavior with 
    /// <see cref="Invert"/> property.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets a value indicating whether the value should be inverted.
        /// By default (<value>false</value> value) it converts <value>true</value> to <value>System.Visible</value> 
        /// and <value>false</value> to <value>System.Collapsed</value>. You can set <value>false</value> to change this.
        /// </summary>
        public bool Invert { get; set; }

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is bool))
            {
                return DependencyProperty.UnsetValue;
            }

            bool result = System.Convert.ToBoolean(value);

            if (this.Invert)
            {
                result = !result;
            }

            return result ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (this.Invert)
            {
                return value.Equals(Visibility.Collapsed);
            }
            return value.Equals(Visibility.Visible);
        }
    }
}