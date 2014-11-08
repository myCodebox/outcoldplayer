// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class QueueActionsPopupViewPresenter : ViewPresenterBase<IQueueActionsPopupView>
    {
        private readonly SelectedItems selectedItems;
        private readonly IPlayQueueService playQueueService;
        private readonly IPlaylistsService playlistsService;

        private readonly IAnalyticsService analyticsService;

        public QueueActionsPopupViewPresenter(
            SelectedItems selectedItems,
            IPlayQueueService playQueueService,
            IPlaylistsService playlistsService,
            IAnalyticsService analyticsService)
        {
            this.selectedItems = selectedItems;
            this.playQueueService = playQueueService;
            this.playlistsService = playlistsService;
            this.analyticsService = analyticsService;
            this.ShuffleAllCommand = new DelegateCommand(this.ShuffleAll);
            this.PlayCommand = new DelegateCommand(this.Play);
            this.AddCommand = new DelegateCommand(this.Add);
            this.PlayNextCommand = new DelegateCommand(this.PlayNext);
        }

        public DelegateCommand PlayCommand { get; private set; }

        public DelegateCommand ShuffleAllCommand { get; private set; }

        public DelegateCommand AddCommand { get; private set; }

        public DelegateCommand PlayNextCommand { get; private set; }

        private async void Play()
        {
            this.analyticsService.SendEvent("QueuePopup", "Execute", "Play");
            this.View.Close(new QueueActionsCompletedEventArgs());

            this.playQueueService.IsShuffled = false;
            await this.PlaySelectedItemsAsync();
        }

        private async void ShuffleAll()
        {
            this.analyticsService.SendEvent("QueuePopup", "Execute", "ShuffleAll");
            this.View.Close(new QueueActionsCompletedEventArgs());

            this.playQueueService.IsShuffled = true;
            await this.PlaySelectedItemsAsync();
        }

        private async void Add()
        {
            this.analyticsService.SendEvent("QueuePopup", "Execute", "Add");
            this.View.Close(new QueueActionsCompletedEventArgs());

            var songs = await this.GetAllSongsAsync();
            await this.playQueueService.AddRangeAsync(songs);
        }

        private async void PlayNext()
        {
            this.analyticsService.SendEvent("QueuePopup", "Execute", "PlayNext");
            this.View.Close(new QueueActionsCompletedEventArgs());

            var songs = await this.GetAllSongsAsync();
            await this.playQueueService.AddRangeAsync(songs, true);
        }

        private async Task PlaySelectedItemsAsync()
        {
            IPlaylist selectedPlaylist = this.GetSelectedPlaylist();
            if (selectedPlaylist != null)
            {
                await this.playQueueService.PlayAsync(selectedPlaylist);
            }
            else
            {
                var songs = await this.GetAllSongsAsync();
                await this.playQueueService.PlayAsync(songs);
            }
        }

        private async Task<IList<Song>> GetAllSongsAsync()
        {
            List<Song> songs = new List<Song>();

            if (this.selectedItems.Playlists != null)
            {
                foreach (var playlist in this.selectedItems.Playlists)
                {
                    var list = await this.playlistsService.GetSongsAsync(playlist);
                    songs.AddRange(list);
                }
            }

            if (this.selectedItems.Songs != null)
            {
                songs.AddRange(this.selectedItems.Songs);
            }

            return songs;
        }

        private IPlaylist GetSelectedPlaylist()
        {
            if (this.selectedItems.Playlists != null && this.selectedItems.Playlists.Count == 1)
            {
                return this.selectedItems.Playlists[0];
            }

            return null;
        }
    }
}