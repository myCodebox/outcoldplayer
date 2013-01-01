// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
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

        public override void OnNavigatedTo(object parameter)
        {
            base.OnNavigatedTo(parameter);

            this.BindingModel.Playlists.Clear();
            this.BindingModel.Count = 0;
            this.BindingModel.IsEditable = false;

            if (parameter is PlaylistsRequest)
            {
                this.BindingModel.IsLoading = true;
                var playlistsRequest = (PlaylistsRequest)parameter;

                if (playlistsRequest == PlaylistsRequest.Albums)
                {
                    this.BindingModel.Title = "Albums";
                    this.songsService.GetAllAlbumsAsync().ContinueWith(
                        t =>
                            {
                                this.BindingModel.Count = t.Result.Count;
                                this.BindingModel.IsLoading = false;

                                foreach (var album in t.Result)
                                {
                                    this.BindingModel.Playlists.Add(new PlaylistBindingModel(album));
                                }
                            },
                        TaskScheduler.FromCurrentSynchronizationContext());
                }
                else if (playlistsRequest == PlaylistsRequest.Playlists)
                {
                    this.BindingModel.Title = "Playlists";
                    this.songsService.GetAllPlaylistsAsync().ContinueWith(
                        t =>
                            {
                                this.BindingModel.Count = t.Result.Count;
                                this.BindingModel.IsLoading = false;
                                this.BindingModel.IsEditable = true;

                                foreach (var playlist in t.Result)
                                {
                                    this.BindingModel.Playlists.Add(new PlaylistBindingModel(playlist));
                                }
                            },
                        TaskScheduler.FromCurrentSynchronizationContext());
                }
                else if (playlistsRequest == PlaylistsRequest.Genres)
                {
                    this.BindingModel.Title = "Genres";
                    this.songsService.GetAllGenresAsync().ContinueWith(
                        t =>
                        {
                            this.BindingModel.Count = t.Result.Count;
                            this.BindingModel.IsLoading = false;

                            foreach (var playlist in t.Result)
                            {
                                this.BindingModel.Playlists.Add(new PlaylistBindingModel(playlist));
                            }
                        },
                        TaskScheduler.FromCurrentSynchronizationContext());
                }
                else
                {
                    this.BindingModel.Title = "Artists";
                    this.songsService.GetAllArtistsAsync().ContinueWith(
                        t =>
                        {
                            this.BindingModel.Count = t.Result.Count;
                            this.BindingModel.IsLoading = false;

                            foreach (var playlist in t.Result)
                            {
                                this.BindingModel.Playlists.Add(new PlaylistBindingModel(playlist));
                            }
                        },
                        TaskScheduler.FromCurrentSynchronizationContext());
                }
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

        private void AddPlaylist()
        {
            if (!this.BindingModel.IsLoading && this.BindingModel.IsEditable)
            {
                this.BindingModel.IsLoading = true;
                this.BindingModel.IsEditable = false;

                this.songsService.CreatePlaylistAsync().ContinueWith(
                    t =>
                        {
                            if (t.Result != null)
                            {
                                this.BindingModel.Playlists.Add(new PlaylistBindingModel(t.Result));
                                this.BindingModel.IsLoading = false;
                                this.BindingModel.IsEditable = true;
                            }
                        },
                    TaskScheduler.FromCurrentSynchronizationContext());
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
                                t =>
                                    {
                                        this.Dispatcher.RunAsync(
                                            () =>
                                                {
                                                    if (t.IsCompleted && t.Result)
                                                    {
                                                        this.BindingModel.Playlists.Remove(playlistBindingModel);
                                                    }

                                                    this.BindingModel.IsLoading = false;
                                                    this.BindingModel.IsEditable = true;
                                                });
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
                    dialog.ShowAsync();
                }
            }
        }

        private void EditPlaylist()
        {
            this.View.EditPlaylist(this.BindingModel.SelectedItem);
        }
    }
}