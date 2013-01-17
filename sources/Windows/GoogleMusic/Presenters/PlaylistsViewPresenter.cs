// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    using Windows.Foundation;
    using Windows.UI.Core;
    using Windows.UI.Popups;

    public class PlaylistsViewPresenter : PlaylistsViewPresenterBase<IPlaylistsView>
    {
        private readonly ISongsService songsService;
        private PlaylistsRequest currentRequest;

        private Playlist clickedPlaylist = null;

        public PlaylistsViewPresenter(
            IDependencyResolverContainer container,
            IPlaylistsView view,
            ISongsService songsService)
            : base(container, view)
        {
            this.songsService = songsService;
            this.BindingModel = new PlaylistsViewBindingModel();

            this.AddPlaylistCommand = new DelegateCommand(this.AddPlaylist, () => !this.BindingModel.IsLoading && this.BindingModel.IsEditable);
            this.DeletePlaylistCommand = new DelegateCommand(this.DetelePlaylist);
            this.EditPlaylistCommand = new DelegateCommand(this.EditPlaylist);

            this.BindingModel.PropertyChanged += (sender, args) =>
                {
                    this.AddPlaylistCommand.RaiseCanExecuteChanged();
                    this.DeletePlaylistCommand.RaiseCanExecuteChanged();
                    this.EditPlaylistCommand.RaiseCanExecuteChanged();
                };
        }

        public PlaylistsViewBindingModel BindingModel { get; private set; }

        public DelegateCommand AddPlaylistCommand { get; private set; }

        public DelegateCommand DeletePlaylistCommand { get; private set; }

        public DelegateCommand EditPlaylistCommand { get; private set; }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);
            eventArgs.State["LastViewedPlaylist"] = this.clickedPlaylist;
            this.clickedPlaylist = null;
        }

        public override void OnNavigatedTo(NavigatedToEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);

            this.View.SetGroups(null);
            this.BindingModel.Count = 0;
            this.BindingModel.IsEditable = false;

            if (eventArgs.Parameter is PlaylistsRequest)
            {
                this.currentRequest = (PlaylistsRequest)eventArgs.Parameter;
                this.BindingModel.IsLoading = true;
                this.BindingModel.Title = this.currentRequest.ToString();

                this.LoadPlaylistsAsync().ContinueWith(
                    async (t) =>
                        {
                            this.BindingModel.IsEditable = this.currentRequest == PlaylistsRequest.Playlists;

                            this.View.SetGroups(t.Result);

                            if (eventArgs.IsBack)
                            {
                                object lastPlaylist;
                                if (eventArgs.State.TryGetValue("LastViewedPlaylist", out lastPlaylist)
                                    && lastPlaylist is Playlist)
                                {
                                    foreach (var group in t.Result)
                                    {
                                        foreach (var playlist in group.Playlists)
                                        {
                                            if (playlist.Playlist.Equals(lastPlaylist))
                                            {
                                                await this.Dispatcher.RunAsync(() => this.View.ShowPlaylist(playlist));
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            this.BindingModel.IsLoading = false;
                            this.BindingModel.Count = t.Result.Sum(x => x.Playlists.Count);
                        },
                    TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        public void ChangePlaylistName(string newName)
        {
            if (!this.BindingModel.IsLoading && this.BindingModel.IsEditable)
            {
                this.BindingModel.IsLoading = true;
                this.BindingModel.IsEditable = false;

                var playlistBindingModel = this.BindingModel.SelectedItem;

                if (playlistBindingModel != null)
                {
                    var playlist = (MusicPlaylist)playlistBindingModel.Playlist;

                    this.songsService.ChangePlaylistNameAsync(playlist, newName).ContinueWith(
                        t =>
                        {
                            this.BindingModel.IsLoading = false;
                            this.BindingModel.IsEditable = true;
                        },
                        TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }

        public override void ItemClick(PlaylistBindingModel playlistBindingModel)
        {
            this.clickedPlaylist = playlistBindingModel.Playlist;
            base.ItemClick(playlistBindingModel);
        }

        private void AddPlaylist()
        {
            if (!this.BindingModel.IsLoading && this.BindingModel.IsEditable)
            {
                this.BindingModel.IsLoading = true;
                this.BindingModel.IsEditable = false;

                this.songsService.CreatePlaylistAsync().ContinueWith(
                    async t =>
                        {
                            if (t.Result != null)
                            {
                                var playlists = await this.LoadPlaylistsAsync();

                                await this.Dispatcher.RunAsync(
                                    () =>
                                        {
                                            this.BindingModel.IsEditable = true;
                                            this.View.SetGroups(playlists);
                                            var playlistBindingModel =
                                                playlists.SelectMany(x => x.Playlists)
                                                         .FirstOrDefault(x => x.Playlist == t.Result);
                                            this.View.ShowPlaylist(playlistBindingModel);
                                            this.BindingModel.IsLoading = false;
                                        });
                            }
                        });
            }
        }

        private void DetelePlaylist()
        {
            if (!this.BindingModel.IsLoading && this.BindingModel.IsEditable)
            {
                this.BindingModel.IsLoading = true;
                this.BindingModel.IsEditable = false;

                var playlistBindingModel = this.BindingModel.SelectedItem;

                if (playlistBindingModel != null)
                {
                    var playlist = playlistBindingModel.Playlist;

                    CoreWindowDialog dialog = new CoreWindowDialog("Are you sure want to delete playlist?");
                    dialog.Showing += (sender, args) => args.SetDesiredSize(new Size(sender.Bounds.Width, 0));
                    dialog.Commands.Add(
                        new UICommand(
                            "Yes",
                            command => this.songsService.DeletePlaylistAsync((MusicPlaylist)playlist).ContinueWith(
                                async t =>
                                    {
                                        if (t.IsCompleted && !t.IsFaulted && t.Result)
                                        {
                                            var playlists = await this.LoadPlaylistsAsync();
                                            await this.Dispatcher.RunAsync(
                                                           () =>
                                                           {
                                                               this.BindingModel.IsEditable = true;
                                                               this.View.SetGroups(playlists);
                                                               this.BindingModel.IsLoading = false;
                                                           });
                                        }
                                    })));

                    dialog.Commands.Add(
                        new UICommand(
                            "No",
                            command => this.Dispatcher.RunAsync(
                                () =>
                                    {
                                        this.BindingModel.IsLoading = false;
                                        this.BindingModel.IsEditable = true;
                                    })));

                    dialog.DefaultCommandIndex = 0;
                    dialog.CancelCommandIndex = 1;
                    var taskResult = dialog.ShowAsync();
                }
            }
        }

        private void EditPlaylist()
        {
            var playlistBindingModel = this.BindingModel.SelectedItem;
            if (playlistBindingModel != null)
            {
                this.View.EditPlaylist(playlistBindingModel);
            }
        }

        private async Task<List<PlaylistsGroupBindingModel>> LoadPlaylistsAsync()
        {
            IEnumerable<Playlist> playlists = null;

            if (this.currentRequest == PlaylistsRequest.Albums)
            {
                playlists = await this.songsService.GetAllAlbumsAsync(Order.Name);
            }
            else if (this.currentRequest == PlaylistsRequest.Playlists)
            {
                playlists = await this.songsService.GetAllPlaylistsAsync(Order.Name, canReload: true);
            }
            else if (this.currentRequest == PlaylistsRequest.Genres)
            {
                playlists = await this.songsService.GetAllGenresAsync(Order.Name);
            }
            else
            {
                playlists = await this.songsService.GetAllArtistsAsync(Order.Name);
            }

            return playlists.GroupBy(x => string.IsNullOrEmpty(x.Title) ? ' ' : char.ToUpper(x.Title[0]))
                         .OrderBy(x => x.Key)
                         .Select(x => new PlaylistsGroupBindingModel(x.Key.ToString(), 0, x.Select(p => new PlaylistBindingModel(p)))).ToList();
        }
    }
}