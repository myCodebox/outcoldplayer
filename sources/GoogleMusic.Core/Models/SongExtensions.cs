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

        public static bool IsExplicit(this Song @this)
        {
            return @this.ContentType == 1;
        }

        public static bool IsDeleted(this Song @this)
        {
            return !@this.TrackAvailableForSubscription && !string.IsNullOrEmpty(@this.StoreId) && @this.TrackType == StreamType.AllAccess;
        }

        public static bool IsAllAccess(this Song @this)
        {
            return @this.TrackType != StreamType.Uploaded && @this.TrackType != StreamType.Free && @this.TrackType != StreamType.Purchased;
        }
    }
}
