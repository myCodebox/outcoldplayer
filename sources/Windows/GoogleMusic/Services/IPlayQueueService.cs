// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    public interface IPlayQueueService
    {
        event EventHandler QueueChanged;

        event EventHandler<StateChangedEventArgs> StateChanged;

        event EventHandler<double> DownloadProgress;
        
        bool IsShuffled { get; }

        bool IsRepeatAll { get; }

        QueueState State { get; }

        Task PlayAsync(IPlaylist playlist, IList<Song> songs);

        Task PlayAsync(IPlaylist playlist, IList<Song> songs, int songIndex);

        Task PlayAsync(int index);

        Task PlayAsync();

        Task PauseAsync();

        Task StopAsync();

        Task NextSongAsync();

        bool CanSwitchToNext();

        Task PreviousSongAsync();

        bool CanSwitchToPrevious();

        Task AddRangeAsync(IEnumerable<Song> songs);

        Task RemoveAsync(int index);

        Task SetRepeatAllAsync(bool repeatAll);

        Task SetShuffledAsync(bool isShuffled);

        IEnumerable<Song> GetQueue();

        int GetCurrentSongIndex();
    }
}