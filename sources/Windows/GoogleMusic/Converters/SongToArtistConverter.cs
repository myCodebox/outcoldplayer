// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    using OutcoldSolutions.GoogleMusic.Models;

    public class SongToArtistConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var song = value as Song;
            if (song != null)
            {
                return string.IsNullOrEmpty(song.ArtistTitle) ? song.AlbumArtistTitle : song.ArtistTitle;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
