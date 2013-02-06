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

    public class AlbumCollection : PlaylistCollectionBase<Album>, IAlbumCollection
    {
        public AlbumCollection(ISongsRepository songsRepository)
            : base(songsRepository)
        {
        }

        protected override List<Album> Generate()
        {
            return this.SongsRepository.GetAll()
                .GroupBy(
                    x =>
                    new
                        {
                            x.GoogleMusicMetadata.AlbumNorm,
                            ArtistNorm = string.IsNullOrWhiteSpace(x.GoogleMusicMetadata.AlbumArtistNorm)
                                            ? x.GoogleMusicMetadata.ArtistNorm
                                            : x.GoogleMusicMetadata.AlbumArtistNorm
                        })
                .Select(
                    x =>
                    new Album(
                        x.OrderBy(s => Math.Max(s.GoogleMusicMetadata.Disc, 1))
                         .ThenBy(s => s.GoogleMusicMetadata.Track)
                         .ToList()))
                .ToList();
        }
    }
}