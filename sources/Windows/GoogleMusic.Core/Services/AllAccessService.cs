// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;

    public interface IAllAccessService
    {
        Task<ArtistInfo> GetArtistInfoAsync(Artist artist);

        Task<Tuple<Album, IList<Song>>> GetAlbumAsync(Album album);
    }

    public class AllAccessService : IAllAccessService
    {
        private readonly IAllAccessWebService allAccessWebService;
        private readonly IArtistsRepository artistsRepository;
        private readonly IAlbumsRepository albumsRepository;
        private readonly ISongsRepository songsRepository;
        private readonly ILogger logger;

        public AllAccessService(
            IAllAccessWebService allAccessWebService,
            IArtistsRepository artistsRepository,
            IAlbumsRepository albumsRepository,
            ISongsRepository songsRepository,
            ILogManager logManager)
        {
            this.allAccessWebService = allAccessWebService;
            this.artistsRepository = artistsRepository;
            this.albumsRepository = albumsRepository;
            this.songsRepository = songsRepository;
            this.logger = logManager.CreateLogger("AllAccessService");
        }

        public async Task<ArtistInfo> GetArtistInfoAsync(Artist artist)
        {
            ArtistInfo info = new ArtistInfo() { Artist = artist };

            if (string.IsNullOrEmpty(artist.GoogleArtistId))
            {
                return info;
            }

            GoogleMusicArtist googleMusicArtist = null;

            try
            {
                googleMusicArtist = await this.allAccessWebService.FetchArtistAsync(artist.GoogleArtistId);
            }
            catch (Exception e)
            {
                this.logger.Error(e, "Could not fetch artist");
            }

            if (googleMusicArtist == null)
            {
                return info;
            }

            if (!string.Equals(artist.Bio, googleMusicArtist.ArtistBio))
            {
                artist.Bio = googleMusicArtist.ArtistBio;
                if (artist.ArtistId > 0)
                {
                    await this.artistsRepository.UpdateBioAsync(artist, artist.Bio);
                }
            }

            if (googleMusicArtist.Albums != null)
            {
                info.GoogleAlbums = new List<Album>();
                foreach (var googleMusicAlbum in googleMusicArtist.Albums)
                {
                    info.GoogleAlbums.Add(this.GetAlbum(googleMusicAlbum, artist));
                }
            }

            if (googleMusicArtist.TopTracks != null)
            {
                info.TopSongs = new List<Song>();
                foreach (var googleMusicSong in googleMusicArtist.TopTracks)
                {
                    Song song = await this.songsRepository.FindSongAsync(googleMusicSong.Id);

                    if (song == null)
                    {
                        song = googleMusicSong.ToSong();
                        song.IsLibrary = false;
                        song.UnknownSong = true;
                    }

                    info.TopSongs.Add(song);
                }
            }

            if (googleMusicArtist.RelatedArtists != null)
            {
                info.RelatedArtists = new List<Artist>();
                foreach (var realtedArtist in googleMusicArtist.RelatedArtists)
                {
                    info.RelatedArtists.Add(new Artist()
                                            {
                                                GoogleArtistId = realtedArtist.ArtistId,
                                                Title = realtedArtist.Name,
                                                TitleNorm = realtedArtist.Name.Normalize(),
                                                ArtUrl = new Uri(realtedArtist.ArtistArtRef)
                                            });
                }
            }

            return info;
        }

        public async Task<Tuple<Album, IList<Song>>> GetAlbumAsync(Album album)
        {
            if (string.IsNullOrEmpty(album.GoogleAlbumId))
            {
                return Tuple.Create<Album, IList<Song>>(album, null);
            }

            var googleMusicAlbum = await this.allAccessWebService.FetchAlbumAsync(album.GoogleAlbumId);

            if (album.AlbumId == 0)
            {
                album = (await this.albumsRepository.FindByGoogleMusicAlbumIdAsync(album.GoogleAlbumId)) ?? album;
            }

            if (!string.Equals(album.Description, googleMusicAlbum.Description))
            {
                album.Description = googleMusicAlbum.Description;
                if (album.AlbumId > 0)
                {
                    await this.albumsRepository.UpdateDescriptionAsync(album.AlbumId, album.Description);
                }
            }

            IList<Song> songs = null;

            if (googleMusicAlbum.Tracks != null)
            {
                songs = new List<Song>();
                foreach (var googleMusicSong in googleMusicAlbum.Tracks)
                {
                    Song song = await this.songsRepository.FindSongAsync(googleMusicSong.Id);

                    if (song == null)
                    {
                        song = googleMusicSong.ToSong();
                        song.IsLibrary = false;
                        song.UnknownSong = true;
                    }

                    songs.Add(song);
                }
            }

            return Tuple.Create(album, songs);
        }

        private Album GetAlbum(GoogleMusicAlbum googleMusicAlbum, Artist artist)
        {
            return new Album()
                   {
                       GoogleAlbumId = googleMusicAlbum.AlbumId,
                       Title = googleMusicAlbum.Name,
                       TitleNorm = googleMusicAlbum.Name.Normalize(),
                       Year = (ushort?)googleMusicAlbum.Year,
                       ArtUrl = new Uri(googleMusicAlbum.AlbumArtRef),
                       Artist = artist
                   };
        }
    }
}
