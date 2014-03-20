// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class CurrentPlaylistPageViewPresenter : PagePresenterBase<ICurrentPlaylistPageView>
    {
        private readonly IPlayQueueService playQueueService;

        private IList<Song> songs;

        private IPlaylist viewPlaylist;

        internal CurrentPlaylistPageViewPresenter(
            IPlayQueueService playQueueService)
        {
            this.playQueueService = playQueueService;

            this.playQueueService.QueueChanged += async (sender, args) => await this.Dispatcher.RunAsync(this.UpdateSongs);

            //this.SaveAsPlaylistCommand = new DelegateCommand(this.SaveAsPlaylist, () => this.BindingModel.Songs.Count > 0);

            this.playQueueService.StateChanged += async (sender, args) => await this.Dispatcher.RunAsync(async () => 
                {
                    if (this.View.GetSongsListView().GetPresenter<SongsListViewPresenter>().SelectedItems.Count == 0)
                    {
                        if (this.Songs != null && args.CurrentSong != null)
                        {
                            var currentSong = this.Songs.FirstOrDefault(x => string.Equals(x.SongId, args.CurrentSong.SongId, StringComparison.Ordinal));
                            if (currentSong != null)
                            {
                                await this.View.GetSongsListView().ScrollIntoCurrentSongAsync(currentSong);
                            }
                        }
                    }
                });
        }

        public DelegateCommand SaveAsPlaylistCommand { get; private set; }

        public IList<Song> Songs
        {
            get
            {
                return this.songs;
            }

            set
            {
                this.SetValue(ref this.songs, value);
            }
        }


        public IPlaylist ViewPlaylist
        {
            get
            {
                return this.viewPlaylist;
            }

            set
            {
                this.SetValue(ref this.viewPlaylist, value);
            }
        }

        //protected override IEnumerable<CommandMetadata> GetViewCommands()
        //{
        //    yield return new CommandMetadata(CommandIcon.Page, "Save", this.SaveAsPlaylistCommand);
        //}

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs, CancellationToken cancellationToken)
        {
            await this.Dispatcher.RunAsync(
                () =>
                    {
                        this.UpdateSongs();

                        if (navigatedToEventArgs.Parameter is bool && (bool)navigatedToEventArgs.Parameter)
                        {
                            this.EventAggregator.Publish(new SelectCurrentPlaylistSongEvent());
                        }
                        else
                        {
                            this.Logger.LogTask(Task.Factory.StartNew(
                                   async () =>
                                   {
                                       await Task.Delay(100, cancellationToken);
                                       await this.View.GetSongsListView().ScrollIntoCurrentSongAsync(this.playQueueService.GetCurrentSong());
                                   }, cancellationToken));
                        }
                    });
        }

        private void UpdateSongs()
        {
            IEnumerable<Song> enumerable = this.playQueueService.GetQueue();
            this.Songs = enumerable == null ? null : enumerable.ToList();
            this.ViewPlaylist = this.playQueueService.CurrentPlaylist;
        }
    }
}