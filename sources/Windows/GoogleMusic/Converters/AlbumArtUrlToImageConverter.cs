// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;
    using System.Globalization;

    using OutcoldSolutions.Diagnostics;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    public class AlbumArtUrlToImageConverter : IValueConverter
    {
        private const string AlbumArtUrlParameter = "=s130-c-e100";

        private const string UnknownAlbumArtFormat = "ms-appx:///Resources/UnknownArt-{0}.png";

        private readonly Lazy<ILogger> logger = new Lazy<ILogger>(() => ApplicationBase.Container.Resolve<ILogManager>().CreateLogger("AlbumArtUrlToImageConverter"));

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var uri = value as Uri;
                if (uri == null)
                {
                    if (parameter != null)
                    {
                        return string.Format(CultureInfo.InvariantCulture, UnknownAlbumArtFormat, parameter);
                    }

                    return string.Format(CultureInfo.InvariantCulture, UnknownAlbumArtFormat, 116);
                }

                string url = uri.ToString();

                if (parameter != null)
                {
                    if (url.LastIndexOf(AlbumArtUrlParameter, StringComparison.OrdinalIgnoreCase) == (url.Length - AlbumArtUrlParameter.Length))
                    {
                        url = string.Format(
                            CultureInfo.InvariantCulture,
                            "{0}=s{1}-c-e100",
                            url.Substring(0, url.Length - AlbumArtUrlParameter.Length),
                            parameter);
                    }
                }

                return url;
            }
            catch (Exception e)
            {
                this.logger.Value.Error(e, "Exception while tried to load album art.");
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}