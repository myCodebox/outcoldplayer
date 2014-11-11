// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web;

    public enum SongCachingChangeEventType
    {
        Unknown = 1,
        StartDownloading = 2,
        FinishDownloading = 3,
        FailedToDownload = 4,
        DownloadCanceled = 5,
        ClearCache = 6,
        RemoveLocalCopy = 7
    }

    public interface ISongsCachingService
    {
        Task<IStream> GetStreamAsync(Song song, CancellationToken token);

        Task PredownloadStreamAsync(Song song, CancellationToken token);

        Task QueueForDownloadAsync(IEnumerable<Song> song);

        Task<IFolder> GetCacheFolderAsync();

        Task ClearCacheAsync();

        Task ClearCachedAsync(IEnumerable<Song> songs);

        Task CancelTaskAsync(CachedSong cachedSong);

        Task<IList<CachedSong>> GetAllActiveTasksAsync();

        Task<Tuple<INetworkRandomAccessStream, Song>> GetCurrentTaskAsync();

        void StartDownloadTask();

        Task CancelDownloadTaskAsync();

        bool IsDownloading();

        Task RestoreCacheAsync();

        Task<IFolder> GetAppDataStorageFolderAsync();

        Task<IFolder> GetMusicLibraryStorageFolderAsync();
    }

    public class CachingChangeEvent
    {
        public CachingChangeEvent(SongCachingChangeEventType eventType)
        {
            this.EventType = eventType;
        }

        public SongCachingChangeEventType EventType { get; private set; }
    }

    public class SongCachingChangeEvent : CachingChangeEvent
    {
        public SongCachingChangeEvent(
            SongCachingChangeEventType eventType,
            INetworkRandomAccessStream stream,
            Song song)
            : base(eventType)
        {
            this.Stream = stream;
            this.Song = song;
        }

        public INetworkRandomAccessStream Stream { get; private set; }

        public Song Song { get; private set; }
    }
}