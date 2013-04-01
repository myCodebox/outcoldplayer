// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    public static class SongExtensions
    {
        public static string GetSongArtist(this Song @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            return string.IsNullOrEmpty(@this.ArtistTitle) ? @this.AlbumArtistTitle : @this.ArtistTitle;
        }
    }
}
