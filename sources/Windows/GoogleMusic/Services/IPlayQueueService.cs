// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface IPlayQueueService
    {
        event EventHandler QueueChanged;

        event EventHandler<StateChangedEventArgs> StateChanged;

        event EventHandler<double> DownloadProgress;

        bool IsShuffled { get; set; }

        bool IsRepeatAll { get; set; }

        QueueState State { get; }

        Task PlayAsync(IPlaylist playlist);

        Task PlayAsync(IPlaylist playlist, int songIndex);

        Task PlayAsync(IPlaylist playlist, IEnumerable<Song> songs, int songIndex);

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

        IEnumerable<Song> GetQueue();

        int GetCurrentSongIndex();
    }
}