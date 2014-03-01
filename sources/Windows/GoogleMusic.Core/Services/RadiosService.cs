// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;

    public interface IRadiosService
    {
        Task<IList<Song>> GetRadioSongsAsync(string radio, IList<Song> currentSongs = null);

        Task<bool> DeleteAsync(IList<Radio> radios);

        Task<bool> RenameAsync(Radio playlist, string name);

        Task<Tuple<Radio, IList<Song>>> CreateAsync(Song song);
    }

    public class RadiosService : IRadiosService
    {
        private readonly IRadioWebService radioWebService;

        private readonly IRadioStationsRepository radioStationsRepository;

        private readonly ISongsRepository songsRepository;

        public RadiosService(
            IRadioWebService radioWebService,
            IRadioStationsRepository radioStationsRepository,
            ISongsRepository songsRepository)
        {
            this.radioWebService = radioWebService;
            this.radioStationsRepository = radioStationsRepository;
            this.songsRepository = songsRepository;
        }

        public async Task<IList<Song>> GetRadioSongsAsync(string id, IList<Song> currentSongs = null)
        {
            var googleRadio = await this.radioWebService.GetRadioSongsAsync(id, currentSongs ?? new List<Song>());

            if (googleRadio != null && googleRadio.Data != null && googleRadio.Data.Stations != null &&
                googleRadio.Data.Stations.Length == 1 && googleRadio.Data.Stations[0].Tracks != null)
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
                return true;
            }

            return false;
        }

        public Task<bool> RenameAsync(Radio playlist, string name)
        {
            throw new NotImplementedException();
        }

        public async Task<Tuple<Radio, IList<Song>>> CreateAsync(Song song)
        {
            var response = await this.radioWebService.CreateStationAsync(song);
            if (response.MutateResponse != null && response.MutateResponse.Length == 1)
            {
                var mutateResponse = response.MutateResponse[0];
                if (string.Equals(mutateResponse.ResponseCode, "OK", StringComparison.OrdinalIgnoreCase) &&
                     mutateResponse.Station != null)
                {
                    var radio = mutateResponse.Station.ToRadio();
                    await this.radioStationsRepository.InsertAsync(new[] { radio });
                    IList<Song> tracks = null;
                    if (mutateResponse.Station.Tracks != null)
                    {
                        tracks = await this.GetSongsAsync(mutateResponse.Station.Tracks);
                    }
                    return Tuple.Create(radio, tracks ?? new List<Song>());
                }
            }

            return null;
        }

        private async Task<IList<Song>> GetSongsAsync(IEnumerable<GoogleMusicSong> radioTracks)
        {
            List<Song> songs = new List<Song>();

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

            return songs;
        }
    }
}
