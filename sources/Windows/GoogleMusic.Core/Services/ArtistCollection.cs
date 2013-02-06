// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;

    public class ArtistCollection : PlaylistCollectionBase<Artist>, IArtistCollection
    {
        public ArtistCollection(ISongsRepository songsRepository)
            : base(songsRepository)
        {
        }

        protected override IEnumerable<Artist> GetForSearch()
        {
            var artists = base.GetForSearch().ToList();

            var groupBy = this.SongsRepository.GetAll().GroupBy(x => x.GoogleMusicMetadata.ArtistNorm);
            foreach (var group in groupBy)
            {
                var artist = artists.FirstOrDefault(
                    x => string.Equals(group.Key, x.Title, StringComparison.CurrentCultureIgnoreCase));

                if (artist != null)
                {
                    foreach (Song song in group)
                    {
                        if (!artist.Songs.Contains(song))
                        {
                            artist.Songs.Add(song);
                        }
                    }
                }
                else
                {
                    artists.Add(new Artist(group.ToList(), useArtist: true));
                }
            }

            return this.OrderCollection(artists, Order.Name);
        }

        protected override List<Artist> Generate()
        {
            return this.SongsRepository.GetAll()
                .GroupBy(x => string.IsNullOrWhiteSpace(x.GoogleMusicMetadata.AlbumArtistNorm) ? x.GoogleMusicMetadata.ArtistNorm : x.GoogleMusicMetadata.AlbumArtistNorm)
                .OrderBy(x => x.Key)
                .Select(x => new Artist(x.ToList())).ToList();
        }
    }
}