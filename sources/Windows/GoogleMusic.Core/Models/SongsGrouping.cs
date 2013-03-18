// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.BindingModels;

    public class SongsGrouping
    {
        public static List<AlbumBindingModel> GroupByAlbums(IEnumerable<SongBindingModel> songs)
        {
            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            return songs
                .GroupBy(
                    x =>
                    new AlbumArtist
                    {
                        Album = x.Metadata.AlbumTitle,
                        Artist = string.IsNullOrWhiteSpace(x.Metadata.AlbumArtist) ? x.Metadata.ArtistTitle : x.Metadata.AlbumArtist
                    },
                        AlbumArtist.AlbumArtistComparer)
                .Select(
                    x =>
                    new AlbumBindingModel(
                        x.OrderBy(s => Math.Max(s.Metadata.Disc, (byte)1))
                         .ThenBy(s => s.Metadata.Track)
                         .ToList()))
                .ToList();
        }

        private class AlbumArtist
        {
            private static readonly IEqualityComparer<AlbumArtist> AlbumArtistComparerInstance = new AlbumArtistEqualityComparer();

            public static IEqualityComparer<AlbumArtist> AlbumArtistComparer
            {
                get
                {
                    return AlbumArtistComparerInstance;
                }
            }

            public string Album { get; set; }

            public string Artist { get; set; }

            private sealed class AlbumArtistEqualityComparer : IEqualityComparer<AlbumArtist>
            {
                public bool Equals(AlbumArtist x, AlbumArtist y)
                {
                    if (ReferenceEquals(x, y))
                    {
                        return true;
                    }

                    if (ReferenceEquals(x, null))
                    {
                        return false;
                    }

                    if (ReferenceEquals(y, null))
                    {
                        return false;
                    }

                    if (x.GetType() != y.GetType())
                    {
                        return false;
                    }

                    return string.Equals(x.Album, y.Album, StringComparison.CurrentCultureIgnoreCase) && string.Equals(x.Artist, y.Artist, StringComparison.CurrentCultureIgnoreCase);
                }

                public int GetHashCode(AlbumArtist obj)
                {
                    unchecked
                    {
                        return ((obj.Album != null ? obj.Album.ToUpper().GetHashCode() : 0) * 397) ^ (obj.Artist != null ? obj.Artist.ToUpper().GetHashCode() : 0);
                    }
                }
            }
        }
    }
}