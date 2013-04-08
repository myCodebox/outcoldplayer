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
    using Windows.UI.Xaml.Media.Imaging;

    public class AlbumArtUrlToImageConverter : IValueConverter
    {
        private const string AlbumArtUrlParameter = "=s130-c-e100";

        private readonly Lazy<BitmapImage> unknownAlbumArt256 = new Lazy<BitmapImage>(() => new BitmapImage(new Uri("ms-appx:///Resources/UnknownArt-256.png")));
        private readonly Lazy<BitmapImage> unknownAlbumArt180 = new Lazy<BitmapImage>(() => new BitmapImage(new Uri("ms-appx:///Resources/UnknownArt-180.png")));
        private readonly Lazy<BitmapImage> unknownAlbumArt140 = new Lazy<BitmapImage>(() => new BitmapImage(new Uri("ms-appx:///Resources/UnknownArt-140.png")));
        private readonly Lazy<BitmapImage> unknownAlbumArt116 = new Lazy<BitmapImage>(() => new BitmapImage(new Uri("ms-appx:///Resources/UnknownArt-116.png")));
        private readonly Lazy<BitmapImage> unknownAlbumArt90 = new Lazy<BitmapImage>(() => new BitmapImage(new Uri("ms-appx:///Resources/UnknownArt-90.png")));
        private readonly Lazy<BitmapImage> unknownAlbumArt80 = new Lazy<BitmapImage>(() => new BitmapImage(new Uri("ms-appx:///Resources/UnknownArt-80.png")));
        private readonly Lazy<BitmapImage> unknownAlbumArt40 = new Lazy<BitmapImage>(() => new BitmapImage(new Uri("ms-appx:///Resources/UnknownArt-40.png")));

        private readonly ILogger logger;

        public AlbumArtUrlToImageConverter()
        {
            this.logger = ApplicationBase.Container.Resolve<ILogManager>().CreateLogger("AlbumArtUrlToImageConverter");
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var uri = value as Uri;
                if (uri == null)
                {
                    if (parameter != null)
                    {
                        switch (parameter.ToString())
                        {
                            case "256":
                                return this.unknownAlbumArt256.Value;
                            case "180":
                                return this.unknownAlbumArt180.Value;
                            case "140":
                                return this.unknownAlbumArt140.Value;
                            case "116":
                                return this.unknownAlbumArt116.Value;
                            case "90":
                                return this.unknownAlbumArt90.Value;
                            case "80":
                                return this.unknownAlbumArt80.Value;
                            case "40":
                                return this.unknownAlbumArt40.Value;
                        }
                    }

                    return this.unknownAlbumArt116.Value;
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

                return new BitmapImage(new Uri(url));
            }
            catch (Exception e)
            {
                this.logger.Error(e, "Exception while tried to load album art.");
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}