// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
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

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                uint size = AlbumArtUrlExtensions.DefaultAlbumArtSize;
                Uri uri = null;

                var uris = value as Uri[];
                if (uris != null)
                {
                    int index = System.Convert.ToInt32(parameter);
                    if (uris.Length > index)
                    {
                        uri = uris[index];
                    }
                    else
                    {
                        uri = uris.LastOrDefault();
                    }

                    size = 79;
                }
                else
                {
                    uri = value as Uri;

                    if (parameter != null)
                    {
                        size = uint.Parse(parameter.ToString());
                    }
                }
                
                if (uri == null)
                {
                    if (parameter != null)
                    {
                        return string.Format(CultureInfo.InvariantCulture, UnknownAlbumArtFormat, size);
                    }

                    return string.Format(CultureInfo.InvariantCulture, UnknownAlbumArtFormat, 116);
                }

                var result = new BitmapImage();

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

                StorageFile file = null;

                if (!string.IsNullOrEmpty(path))
                {
                    try
                    {
                        file = await ApplicationData.Current.LocalFolder.GetFileAsync(path);
                    }
                    catch (FileNotFoundException e)
                    {
                        this.logger.Value.Debug(e, "File was removed.");
                        this.logger.Value.LogTask(this.cacheService.Value.DeleteBrokenLinkAsync(uri, size));
                    }
                }

                if (file == null)
                {
                    file =
                        await
                            StorageFile.GetFileFromApplicationUriAsync(
                                new Uri(string.Format(CultureInfo.InvariantCulture, UnknownAlbumArtFormat, size)));
                }

                if (file != null)
                {
                    image.SetSource(await file.OpenReadAsync());
                }
            }
            catch (OperationCanceledException e)
            {
                this.logger.Value.Debug(e, "Task was cancelled");
            }
            catch (WebException e)
            {
                this.logger.Value.Debug(e, "Web exception.");
            }
            catch (FileNotFoundException e)
            {
                this.logger.Value.Debug(e, "File not found.");
            }
            catch (Exception e)
            {
                this.logger.Value.Error(e, "Exception while tried to load album art with GetImageAsync.");
            }
        }
    }
}