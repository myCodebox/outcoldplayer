﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public enum RepeatType
    {
        None = 0,
        One = 1,
        All = 2
    }

    public interface IPlayQueueService
    {
        event EventHandler QueueChanged;

        event EventHandler<StateChangedEventArgs> StateChanged;

        event EventHandler<double> DownloadProgress;

        bool IsShuffled { get; set; }

        RepeatType Repeat { get; set; }

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

        Task AddRangeAsync(IEnumerable<Song> songs, bool playNext = false);

        Task AddRangeAsync(IPlaylist playlist, IEnumerable<Song> songs, bool playNext = false);

        Task RemoveAsync(IEnumerable<int> index);

        IEnumerable<Song> GetQueue();

        int GetCurrentSongIndex();

        Song GetCurrentSong();
    }
}