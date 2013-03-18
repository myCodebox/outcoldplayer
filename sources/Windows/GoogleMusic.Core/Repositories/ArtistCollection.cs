// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;

    public class ArtistCollection : PlaylistCollectionBase<ArtistBindingModel>
    {
        public ArtistCollection(ISongsRepository songsRepository)
            : base(songsRepository)
        {
        }

        protected async override Task<List<ArtistBindingModel>> GetForSearchAsync()
        {
            var artists = await base.GetForSearchAsync();

            var songs = await this.SongsRepository.GetAllAsync();
            var groupBy = songs.GroupBy(x => x.Metadata.ArtistTitle, StringComparer.CurrentCultureIgnoreCase);
            foreach (var group in groupBy)
            {
                var artist = artists.FirstOrDefault(
                    x => string.Equals(group.Key, x.Title, StringComparison.CurrentCultureIgnoreCase));

                if (artist != null)
                {
                    foreach (SongBindingModel song in group)
                    {
                        if (!artist.Songs.Contains(song))
                        {
                            artist.Songs.Add(song);
                        }
                    }
                }
                else
                {
                    artists.Add(new ArtistBindingModel(group.ToList(), useArtist: true));
                }
            }

            return this.OrderCollection(artists, Order.Name).ToList();
        }

        protected override async Task<List<ArtistBindingModel>> LoadCollectionAsync()
        {
            return (await this.SongsRepository.GetAllAsync())
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Metadata.AlbumArtist) ? x.Metadata.ArtistTitle : x.Metadata.AlbumArtist, StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(x => x.Key, StringComparer.CurrentCultureIgnoreCase)
                .Select(x => new ArtistBindingModel(x.ToList())).ToList();
        }
    }
}