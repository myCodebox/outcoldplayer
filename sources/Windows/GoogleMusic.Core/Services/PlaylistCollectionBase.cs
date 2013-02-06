// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;

    public abstract class PlaylistCollectionBase<TPlaylist> : IPlaylistCollection<TPlaylist>
        where TPlaylist : Playlist
    {
        private readonly object locker = new object();
        private List<TPlaylist> playlists;

        protected PlaylistCollectionBase(ISongsRepository songsRepository)
        {
            this.SongsRepository = songsRepository;
            this.SongsRepository.Updated += () =>
                {
                    lock (locker)
                    {
                        this.playlists = null;
                    }
                };
        }

        protected ISongsRepository SongsRepository { get; private set; }

        public Task<int> CountAsync()
        {
            return Task.Factory.StartNew(() => this.GetAll().Count);
        }

        public Task<IEnumerable<TPlaylist>> GetAllAsync(Order order = Order.Name, int takeCount = int.MaxValue)
        {
            return Task.Factory.StartNew(() => this.OrderCollection(this.GetAll(), order).Take(takeCount));
        }

        public Task<IEnumerable<TPlaylist>> SearchAsync(string query, int takeCount = Int32.MaxValue)
        {
            return Task.Factory.StartNew(() => this.GetForSearch()
                                                   .Select(x => Tuple.Create(Search.IndexOf(x.Title, query), x))
                                                   .Where(x => x.Item1 >= 0)
                                                   .OrderBy(x => x.Item1)
                                                   .Take(takeCount)
                                                   .Select(x => x.Item2));
        }

        protected virtual IEnumerable<TPlaylist> GetForSearch()
        {
            return this.GetAll();
        }

        protected abstract List<TPlaylist> Generate();

        protected IEnumerable<TPlaylist> OrderCollection(IEnumerable<TPlaylist> enumerable, Order order)
        {
            if (order == Order.LastPlayed)
            {
                enumerable = enumerable.OrderByDescending(x => x.Songs.Count > 0 ? x.Songs.Max(s => s.GoogleMusicMetadata.LastPlayed) : double.MinValue);
            }
            else if (order == Order.Name)
            {
                enumerable = enumerable.OrderBy(x => (x.Title ?? string.Empty).ToUpper());
            }

            return enumerable;
        }

        private List<TPlaylist> GetAll()
        {
            lock (this.locker)
            {
                if (this.playlists == null)
                {
                    this.playlists = this.Generate();
                }

                return this.playlists.ToList();
            }
        }
    }
}