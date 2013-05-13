// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;
    using System.Globalization;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;

    using Windows.Storage;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Media.Imaging;

    public class AlbumArtUrlToImageConverter : IValueConverter
    {
        private const string UnknownAlbumArtFormat = "ms-appx:///Resources/UnknownArt-{0}.png";

        private readonly Lazy<ILogger> logger = new Lazy<ILogger>(() => ApplicationBase.Container.Resolve<ILogManager>().CreateLogger("AlbumArtUrlToImageConverter"));
        private readonly Lazy<IAlbumArtCacheService> cacheService = new Lazy<IAlbumArtCacheService>(() => ApplicationBase.Container.Resolve<IAlbumArtCacheService>());
        private readonly Lazy<IApplicationStateService> stateService = new Lazy<IApplicationStateService>(() => ApplicationBase.Container.Resolve<IApplicationStateService>());

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

                var result = new BitmapImage();
                uint size = AlbumArtUrlExtensions.DefaultAlbumArtSize;
                if (parameter != null)
                {
                    size = uint.Parse(parameter.ToString());
                }

                this.GetImageAsync(result, uri, size);

                return result;
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

        private async void GetImageAsync(BitmapImage image, Uri uri, uint size)
        {
            try
            {
                string path = await this.cacheService.Value.GetCachedImageAsync(uri, size);
                
                StorageFile file;
                if (string.IsNullOrEmpty(path))
                {
                    file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(string.Format(CultureInfo.InvariantCulture, UnknownAlbumArtFormat, size)));
                }
                else
                {
                    file = await ApplicationData.Current.LocalFolder.GetFileAsync(path);
                }

                image.SetSource(await file.OpenReadAsync());
            }
            catch (Exception e)
            {
                this.logger.Value.Error(e, "Exception while tried to load album art with GetImageAsync.");
            }
        }
    }
}