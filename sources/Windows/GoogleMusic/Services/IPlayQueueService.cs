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

        bool IsRadio { get; }

        IPlaylist CurrentPlaylist { get; }

        QueueState State { get; }

        Task<bool> PlayAsync(IPlaylist playlist);

        Task<bool> PlayAsync(IPlaylist playlist, int songIndex);

        Task<bool> PlayAsync(IPlaylist playlist, IEnumerable<Song> songs, int songIndex);

        Task<bool> PlayAsync(int index);

        Task<bool> PlayAsync(IEnumerable<Song> songs);

        Task<bool> PlayAsync();

        Task PauseAsync();

        Task StopAsync();

        Task<bool> NextSongAsync();

        bool CanSwitchToNext();

        Task<bool> PreviousSongAsync();

        bool CanSwitchToPrevious();

        Task AddRangeAsync(IEnumerable<Song> songs);

        Task AddRangeAsync(IPlaylist playlist, IEnumerable<Song> songs);

        Task RemoveAsync(IEnumerable<int> index);

        IEnumerable<Song> GetQueue();

        int GetCurrentSongIndex();

        Song GetCurrentSong();
    }
}