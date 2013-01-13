// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;
    using System.Globalization;

    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Media.Imaging;

    public class AlbumArtUrlToImageConverter : IValueConverter
    {
        private const string AlbumArtUrlParameter = "=s130-c-e100";

        private readonly Lazy<BitmapImage> unknownAlbumArt256 = new Lazy<BitmapImage>(() => new BitmapImage(new Uri("ms-appx:///Resources/UnknownArt-256.png")));
        private readonly Lazy<BitmapImage> unknownAlbumArt116 = new Lazy<BitmapImage>(() => new BitmapImage(new Uri("ms-appx:///Resources/UnknownArt-116.png")));
        private readonly Lazy<BitmapImage> unknownAlbumArt90 = new Lazy<BitmapImage>(() => new BitmapImage(new Uri("ms-appx:///Resources/UnknownArt-90.png")));

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var url = value as string;
            if (string.IsNullOrEmpty(url))
            {
                if (parameter != null)
                {
                    switch (parameter.ToString())
                    {
                        case "256":
                            return this.unknownAlbumArt256.Value;
                        case "116":
                            return this.unknownAlbumArt116.Value;
                        case "90":
                            return this.unknownAlbumArt90.Value;
                    }
                }

                return this.unknownAlbumArt116.Value;
            }
            else
            {
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

                // TODO: Use converter parameter to set image size
                return new BitmapImage(new Uri("http:" + url));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}