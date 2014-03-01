// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;

    public interface IRadioStationsService
    {
        Task<IList<Song>> GetRadioSongsAsync(string radio, IList<Song> currentSongs = null);

        Task<bool> DeleteAsync(IList<Radio> radios);

        Task<Tuple<Radio, IList<Song>>> CreateAsync(Song song);

        Task<Tuple<Radio, IList<Song>>> CreateAsync(Artist artist);

        Task<Tuple<Radio, IList<Song>>> CreateAsync(Album album);

        Task<bool> RenameStationAsync(Radio radio, string title);
    }

    public class RadioStationsService : IRadioStationsService
    {
        private readonly IRadioWebService radioWebService;

        private readonly IRadioStationsRepository radioStationsRepository;

        private readonly ISongsRepository songsRepository;

        private readonly INotificationService notificationService;

        private readonly IApplicationResources applicationResources;

        private readonly IEventAggregator eventAggregator;

        private ILogger logger;

        public RadioStationsService(
            IRadioWebService radioWebService,
            IRadioStationsRepository radioStationsRepository,
            ISongsRepository songsRepository,
            INotificationService notificationService,
            IApplicationResources applicationResources,
            IEventAggregator eventAggregator,
            ILogManager logManager)
        {
            this.radioWebService = radioWebService;
            this.radioStationsRepository = radioStationsRepository;
            this.songsRepository = songsRepository;
            this.notificationService = notificationService;
            this.applicationResources = applicationResources;
            this.eventAggregator = eventAggregator;

            this.logger = logManager.CreateLogger("RadioStationsService");
        }

        public async Task<IList<Song>> GetRadioSongsAsync(string id, IList<Song> currentSongs = null)
        {
            var googleRadio = await this.radioWebService.GetRadioSongsAsync(id, currentSongs ?? new List<Song>());

            if (googleRadio != null && googleRadio.Data != null && googleRadio.Data.Stations != null &&
                googleRadio.Data.Stations.Length == 1)
            {
                return await this.GetSongsAsync(googleRadio.Data.Stations[0].Tracks);
            }

            return new List<Song>();
        }

        public async Task<bool> DeleteAsync(IList<Radio> radios)
        {
            var response = await this.radioWebService.DeleteStationsAsync(radios.Select(x => x.Id));
            if (response.MutateResponse != null)
            {
                IList<Radio> deletedRadios = new List<Radio>();
                foreach (var mutateResponse in response.MutateResponse)
                {
                    if (string.Equals(mutateResponse.ResponseCode, "OK", StringComparison.OrdinalIgnoreCase))
                    {
                        Radio radio =
                            radios.FirstOrDefault(r => string.Equals(r.Id, mutateResponse.Id, StringComparison.OrdinalIgnoreCase));
                        if (radio != null)
                        {
                            deletedRadios.Add(radio);
                        }
                    }
                }
                await this.radioStationsRepository.DeleteAsync(deletedRadios);
                this.eventAggregator.Publish(PlaylistsChangeEvent.New(PlaylistType.Radio).AddRemovedPlaylists(deletedRadios.Cast<IPlaylist>().ToArray()));
                return true;
            }

            return false;
        }

        public async Task<Tuple<Radio, IList<Song>>> CreateAsync(Song song)
        {
            var radio = await this.radioStationsRepository.FindByGoogleSongId(song.SongId);
            if (radio != null)
            {
                return Tuple.Create(radio, await this.GetRadioSongsAsync(radio.RadioId));
            }

            return await this.HandleCreationResponse(await this.radioWebService.CreateStationAsync(song));
        }

        public async Task<Tuple<Radio, IList<Song>>> CreateAsync(Artist artist)
        {
            var radio = await this.radioStationsRepository.FindByGoogleArtistId(artist.GoogleArtistId);
            if (radio != null)
            {
                return Tuple.Create(radio, await this.GetRadioSongsAsync(radio.RadioId));
            }

            return await this.HandleCreationResponse(await this.radioWebService.CreateStationAsync(artist));
        }

        public async Task<Tuple<Radio, IList<Song>>> CreateAsync(Album album)
        {
            var radio = await this.radioStationsRepository.FindByGoogleAlbumId(album.GoogleAlbumId);
            if (radio != null)
            {
                return Tuple.Create(radio, await this.GetRadioSongsAsync(radio.RadioId));
            }

            return await this.HandleCreationResponse(await this.radioWebService.CreateStationAsync(album));
        }

        public async Task<bool> RenameStationAsync(Radio radio, string title)
        {
            var resp = await this.radioWebService.ChangeStationNameAsync(radio, title);
            if (resp != null && resp.MutateResponse != null && resp.MutateResponse.Length == 1)
            {
                if (string.Equals(resp.MutateResponse[0].ResponseCode, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    radio.Title = title;
                    radio.TitleNorm = title.Normalize();

                    await this.radioStationsRepository.UpdateAsync(new[] { radio });
                    this.eventAggregator.Publish(PlaylistsChangeEvent.New(PlaylistType.Radio).AddUpdatedPlaylists(radio));
                    return true;
                }
            }

            return false;
        }

        private async Task<Tuple<Radio, IList<Song>>> HandleCreationResponse(GoogleMusicMutateResponse response)
        {
            if (response.MutateResponse != null && response.MutateResponse.Length == 1)
            {
                var mutateResponse = response.MutateResponse[0];
                if (string.Equals(mutateResponse.ResponseCode, "OK", StringComparison.OrdinalIgnoreCase) &&
                     mutateResponse.Station != null)
                {
                    var radio = mutateResponse.Station.ToRadio();
                    await this.radioStationsRepository.InsertAsync(new[] { radio });
                    this.eventAggregator.Publish(PlaylistsChangeEvent.New(PlaylistType.Radio).AddAddedPlaylists(radio));
                    IList<Song> tracks = await this.GetSongsAsync(mutateResponse.Station.Tracks);
                    return Tuple.Create(radio, tracks);
                }
                else
                {
                    this.logger.Error("Could not create radio, because of {0}", mutateResponse.ResponseCode);
                }
            }

            return null;
        }

        private async Task<IList<Song>> GetSongsAsync(IEnumerable<GoogleMusicSong> radioTracks)
        {
            List<Song> songs = new List<Song>();

            if (radioTracks != null)
            {
                foreach (var track in radioTracks)
                {
                    Song song = await this.songsRepository.FindSongAsync(track.Id);

                    if (song == null)
                    {
                        song = track.ToSong();
                        song.IsLibrary = false;
                    }

                    songs.Add(song);
                }
            }

            if (songs.Count == 0)
            {
                await this.notificationService.ShowMessageAsync(this.applicationResources.GetString("Radio_CouldNotPrepare"));
            }

            return songs;
        }
    }
}
