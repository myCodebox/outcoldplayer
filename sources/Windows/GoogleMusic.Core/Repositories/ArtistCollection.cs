// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public class ArtistCollection : PlaylistCollectionBase<Artist>
    {
        public ArtistCollection(ISongsRepository songsRepository)
            : base(songsRepository)
        {
        }

        protected async override Task<List<Artist>> GetForSearchAsync()
        {
            var artists = await base.GetForSearchAsync();

            var songs = await this.SongsRepository.GetAllAsync();
            var groupBy = songs.GroupBy(x => x.Metadata.Artist, StringComparer.CurrentCultureIgnoreCase);
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

            return this.OrderCollection(artists, Order.Name).ToList();
        }

        protected override async Task<List<Artist>> LoadCollectionAsync()
        {
            return (await this.SongsRepository.GetAllAsync())
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Metadata.AlbumArtist) ? x.Metadata.Artist : x.Metadata.AlbumArtist, StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(x => x.Key, StringComparer.CurrentCultureIgnoreCase)
                .Select(x => new Artist(x.ToList())).ToList();
        }
    }
}