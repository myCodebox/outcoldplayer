// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class SongsRepository : ISongsRepository
    {
        private readonly ILogger logger;

        private readonly Dictionary<Guid, Song> songs = new Dictionary<Guid, Song>();

        public SongsRepository(
            ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("SongsRepository");
        }

        public event Action Updated;

        public void Clear()
        {
            lock (this.songs)
            {
                this.songs.Clear();
            }
        }

        public Song AddOrUpdate(GoogleMusicSong songInfo)
        {
            lock (this.songs)
            {
                return this.AddOrUpdatePrivate(songInfo);
            }
        }

        public void AddRange(IEnumerable<GoogleMusicSong> songInfos)
        {
            bool updated = false;

            lock (this.songs)
            {
                foreach (var songInfo in songInfos)
                {
                    updated = true;

                    this.AddOrUpdatePrivate(songInfo);
                }
            }

            if (updated)
            {
                this.RaiseUpdated();
            }
        }

        public void Remove(Guid id)
        {
            lock (this.songs)
            {
                Song song;
                if (this.songs.TryGetValue(id, out song))
                {
                    this.songs.Remove(id);
                }
            }
        }

        public IEnumerable<Song> GetAll()
        {
            lock (this.songs)
            {
                return this.songs.Values.ToList();
            }
        }

        private Song AddOrUpdatePrivate(GoogleMusicSong songInfo)
        {
            Song song;
            if (this.songs.TryGetValue(songInfo.Id, out song))
            {
                song.GoogleMusicMetadata = songInfo;
            }
            else
            {
                this.songs.Add(songInfo.Id, song = new Song(songInfo));
            }

            return song;
        }

        private void RaiseUpdated()
        {
            var handler = this.Updated;
            if (handler != null)
            {
                handler();
            }
        }
    }
}