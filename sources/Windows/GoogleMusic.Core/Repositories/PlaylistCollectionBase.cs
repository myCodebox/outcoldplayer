// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public abstract class PlaylistCollectionBase<TPlaylist> : IPlaylistCollection<TPlaylist>
        where TPlaylist : Playlist
    {
        private readonly SemaphoreSlim mutex = new SemaphoreSlim(1);
        private readonly bool useCache = false;

        private List<TPlaylist> playlists;

        protected PlaylistCollectionBase(
            ISongsRepository songsRepository, 
            bool useCache = true)
        {
            this.useCache = useCache;
            this.SongsRepository = songsRepository;
        }

        protected ISongsRepository SongsRepository { get; private set; }

        public async Task<int> CountAsync()
        {
            var collection = await this.GetCollectionAsync();
            return collection.Count;
        }

        public async Task<IEnumerable<TPlaylist>> GetAllAsync(Order order = Order.Name, int takeCount = int.MaxValue)
        {
            var collection = await this.GetCollectionAsync();
            return this.OrderCollection(collection, order).Take(takeCount);
        }

        public async Task<IEnumerable<TPlaylist>> SearchAsync(string query, int takeCount = Int32.MaxValue)
        {
            var collection = await this.GetForSearchAsync();
            return collection.Select(x => Tuple.Create(Search.IndexOf(x.Title, query), x))
                          .Where(x => x.Item1 >= 0)
                          .OrderBy(x => x.Item1)
                          .ThenBy(x => x.Item2.Title, StringComparer.CurrentCultureIgnoreCase)
                          .Take(takeCount)
                          .Select(x => x.Item2);
        }

        protected async virtual Task<List<TPlaylist>> GetForSearchAsync()
        {
            return await this.GetCollectionAsync();
        }

        protected abstract Task<List<TPlaylist>> LoadCollectionAsync();

        protected IEnumerable<TPlaylist> OrderCollection(IEnumerable<TPlaylist> enumerable, Order order)
        {
            if (order == Order.LastPlayed)
            {
                enumerable = enumerable.OrderByDescending(x => x.Songs.Count > 0 ? x.Songs.Max(s => s.Metadata.LastPlayed) : DateTime.MinValue);
            }
            else if (order == Order.Name)
            {
                enumerable = enumerable.OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase);
            }

            return enumerable;
        }

        private async Task<List<TPlaylist>> GetCollectionAsync()
        {
            try
            {
                if (this.useCache)
                {
                    await this.mutex.WaitAsync();
                }

                if (this.playlists != null)
                {
                    return this.playlists.ToList();
                }
                else
                {
                    var result = await this.LoadCollectionAsync();
                    if (this.useCache)
                    {
                        this.playlists = result;
                    }

                    return result.ToList();
                }
            }
            finally 
            {
                if (this.useCache)
                {
                    this.mutex.Release();
                }
            }
        }
    }
}