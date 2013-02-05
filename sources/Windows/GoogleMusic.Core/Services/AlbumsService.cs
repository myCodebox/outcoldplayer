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

    public class AlbumsService : PlaylistsServiceBase<Album>, IAlbumsService
    {
        private readonly ISongsRepository songsRepository;
        private readonly object locker = new object();
        private List<Album> albums;

        public AlbumsService(ISongsRepository songsRepository)
        {
            this.songsRepository = songsRepository;
            this.songsRepository.Updated += () =>
                {
                    lock (this.albums)
                    {
                        this.albums.Clear();
                    }
                };
        }

        protected override List<Album> GetPlaylists()
        {
            lock (this.locker)
            {
                if (this.albums == null)
                {
                    this.albums = this.songsRepository.GetAll()
                        .GroupBy(x => new
                        {
                            x.GoogleMusicMetadata.AlbumNorm,
                            ArtistNorm = string.IsNullOrWhiteSpace(x.GoogleMusicMetadata.AlbumArtistNorm)
                                                  ? x.GoogleMusicMetadata.ArtistNorm
                                                  : x.GoogleMusicMetadata.AlbumArtistNorm
                        })
                        .Select(x => new Album(x.OrderBy(s => Math.Max(s.GoogleMusicMetadata.Disc, 1)).ThenBy(s => s.GoogleMusicMetadata.Track).ToList())).ToList();
                }

                return this.albums.ToList();
            }
        }
    }
}