// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface IAllAccessService
    {
        Task<ArtistInfo> GetArtistInfoAsync(Artist artist, CancellationToken cancellationToken);

        Task<Tuple<Album, IList<Song>>> GetAlbumAsync(Album album, CancellationToken cancellationToken);

        Task<SearchResult> SearchAsync(string search, CancellationToken cancellationToken);
    }

    public class AllAccessService : AllAccessServiceBase, IAllAccessService
    {
        private readonly IAllAccessWebService allAccessWebService;
        private readonly IArtistsRepository artistsRepository;
        private readonly IAlbumsRepository albumsRepository;
        private readonly ILogger logger;

        public AllAccessService(
            IAllAccessWebService allAccessWebService,
            IArtistsRepository artistsRepository,
            IAlbumsRepository albumsRepository,
            ISongsRepository songsRepository,
            ILogManager logManager)
            : base(songsRepository)
        {
            this.allAccessWebService = allAccessWebService;
            this.artistsRepository = artistsRepository;
            this.albumsRepository = albumsRepository;
            this.logger = logManager.CreateLogger("AllAccessService");
        }

        public async Task<ArtistInfo> GetArtistInfoAsync(Artist artist, CancellationToken cancellationToken)
        {
            ArtistInfo info = new ArtistInfo() { Artist = artist };

            if (string.IsNullOrEmpty(artist.GoogleArtistId))
            {
                return info;
            }

            GoogleMusicArtist googleMusicArtist = null;

            try
            {
                googleMusicArtist = await this.allAccessWebService.FetchArtistAsync(artist.GoogleArtistId, cancellationToken);
            }
            catch (OperationCanceledException e)
            {
                throw;
            }
            catch (Exception e)
            {
                this.logger.Error(e, "Could not fetch artist");
            }

            if (googleMusicArtist == null)
            {
                return info;
            }

            if (!string.Equals(artist.Bio, googleMusicArtist.ArtistBio, StringComparison.CurrentCulture))
            {
                artist.Bio = googleMusicArtist.ArtistBio;
                if (artist.ArtistId > 0)
                {
                    await this.artistsRepository.UpdateBioAsync(artist, artist.Bio);
                }
            }

            if (string.IsNullOrEmpty(artist.Title) && 
                !string.Equals(artist.Title, googleMusicArtist.Name, StringComparison.CurrentCulture))
            {
                artist.Title = googleMusicArtist.Name;
                artist.TitleNorm = googleMusicArtist.Name.Normalize();
            }

            if (!string.IsNullOrEmpty(googleMusicArtist.ArtistArtRef))
            {
                artist.ArtUrl = new Uri(googleMusicArtist.ArtistArtRef);
            }

            if (googleMusicArtist.Albums != null)
            {
                info.GoogleAlbums = new List<Album>();
                foreach (var googleMusicAlbum in googleMusicArtist.Albums)
                {
                    info.GoogleAlbums.Add(GetAlbum(googleMusicAlbum, artist));
                }
            }

            if (googleMusicArtist.TopTracks != null)
            {
                info.TopSongs = new List<Song>();
                foreach (var googleMusicSong in googleMusicArtist.TopTracks)
                {
                    info.TopSongs.Add(await this.GetSong(googleMusicSong));
                }
            }

            if (googleMusicArtist.RelatedArtists != null)
            {
                info.RelatedArtists = new List<Artist>();
                foreach (var realtedArtist in googleMusicArtist.RelatedArtists)
                {
                    info.RelatedArtists.Add(GetArtist(realtedArtist));
                }
            }

            return info;
        }

        public async Task<Tuple<Album, IList<Song>>> GetAlbumAsync(Album album, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(album.GoogleAlbumId))
            {
                return Tuple.Create<Album, IList<Song>>(album, null);
            }

            GoogleMusicAlbum googleMusicAlbum = null;

            try
            {
                googleMusicAlbum = await this.allAccessWebService.FetchAlbumAsync(album.GoogleAlbumId, cancellationToken);
            }
            catch (Exception e)
            {
                this.logger.Error(e, "Could not fetch google album");
                return Tuple.Create<Album, IList<Song>>(album, null);
            }
            
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

            if (!string.Equals(album.Title, googleMusicAlbum.Name, StringComparison.CurrentCulture))
            {
                album.Title = googleMusicAlbum.Name;
                album.TitleNorm = googleMusicAlbum.Name.Normalize();
            }

            if (album.ArtUrl == null && !string.IsNullOrEmpty(googleMusicAlbum.AlbumArtRef))
            {
                album.ArtUrl = new Uri(googleMusicAlbum.AlbumArtRef);
            }

            album.Year = (ushort?)googleMusicAlbum.Year;

            if (album.Artist == null)
            {
                album.Artist = new Artist()
                    {
                        GoogleArtistId = googleMusicAlbum.ArtistId == null ? null : googleMusicAlbum.ArtistId.FirstOrDefault(), 
                        Title = googleMusicAlbum.AlbumArtist, 
                        TitleNorm = googleMusicAlbum.AlbumArtist.Normalize()
                    };
            }

            IList<Song> songs = null;

            if (googleMusicAlbum.Tracks != null)
            {
                songs = new List<Song>();
                foreach (var googleMusicSong in googleMusicAlbum.Tracks)
                {
                    songs.Add(await this.GetSong(googleMusicSong));
                }
            }

            return Tuple.Create(album, songs);
        }

        public async Task<SearchResult> SearchAsync(string search, CancellationToken cancellationToken)
        {
            SearchResult result = new SearchResult(search);
            GoogleSearchResult googleSearchResult;
            try
            {
                googleSearchResult = await this.allAccessWebService.SearchAsync(search, cancellationToken);
            }
            catch (Exception exception)
            {
                this.logger.Debug(exception, "Search failed");
                return result;
            }

            if (googleSearchResult != null && googleSearchResult.Entries != null)
            {
                foreach (var entry in googleSearchResult.Entries)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (entry.Artist != null)
                    {
                        if (result.Artists == null)
                        {
                            result.Artists = new List<SearchResultEntity>();
                        }

                        result.Artists.Add(new SearchResultEntity() { Playlist = GetArtist(entry.Artist), Score = entry.Score });
                    }

                    if (entry.Album != null)
                    {
                        if (result.Albums == null)
                        {
                            result.Albums = new List<SearchResultEntity>();
                        }

                        result.Albums.Add(new SearchResultEntity() { Playlist = GetAlbum(entry.Album, null), Score = entry.Score });
                    }

                    if (entry.Track != null)
                    {
                        if (result.Songs == null)
                        {
                            result.Songs = new List<SearchResultEntity>();
                        }

                        result.Songs.Add(new SearchResultEntity() { Song = await this.GetSong(entry.Track), Score = entry.Score });
                    }
                }
            }

            return result;
        }

        private static Artist GetArtist(GoogleMusicArtist googleMusicArtist)
        {
            return new Artist()
            {
                GoogleArtistId = googleMusicArtist.ArtistId,
                Title = googleMusicArtist.Name,
                TitleNorm = googleMusicArtist.Name.Normalize(),
                ArtUrl = string.IsNullOrEmpty(googleMusicArtist.ArtistArtRef) ? null : new Uri(googleMusicArtist.ArtistArtRef)
            };
        }

        private static Album GetAlbum(GoogleMusicAlbum googleMusicAlbum, Artist artist)
        {
            return new Album()
                   {
                       GoogleAlbumId = googleMusicAlbum.AlbumId,
                       Title = googleMusicAlbum.Name,
                       TitleNorm = googleMusicAlbum.Name.Normalize(),
                       Year = (ushort?)googleMusicAlbum.Year,
                       ArtUrl = string.IsNullOrEmpty(googleMusicAlbum.AlbumArtRef) ? null : new Uri(googleMusicAlbum.AlbumArtRef),

                       Artist = artist ?? new Artist()
                                          {
                                              GoogleArtistId = googleMusicAlbum.ArtistId == null ? null : googleMusicAlbum.ArtistId.FirstOrDefault(), 
                                              Title = googleMusicAlbum.AlbumArtist, 
                                              TitleNorm = googleMusicAlbum.AlbumArtist.Normalize()
                                          }
                   };
        }
    }
}
