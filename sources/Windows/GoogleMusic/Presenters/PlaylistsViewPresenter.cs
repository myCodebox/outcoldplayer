// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    using Windows.UI.Core;
    using Windows.UI.Popups;

    public class PlaylistsViewPresenter : PlaylistsViewPresenterBase<IPlaylistsView>
    {
        private readonly IPlaylistCollectionsService playlistCollectionsService;

        private readonly IMusicPlaylistRepository musicPlaylistRepository;

        private PlaylistsRequest currentRequest;

        public PlaylistsViewPresenter(
            IDependencyResolverContainer container,
            IPlaylistCollectionsService playlistCollectionsService,
            IMusicPlaylistRepository musicPlaylistRepository)
            : base(container)
        {
            this.playlistCollectionsService = playlistCollectionsService;
            this.musicPlaylistRepository = musicPlaylistRepository;
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
            eventArgs.State["ListViewHorizontalOffset"] = this.View.GetHorizontalOffset();
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
                            await this.Dispatcher.RunAsync(
                                CoreDispatcherPriority.High,
                                () =>
                                    {
                                        this.BindingModel.IsEditable = this.currentRequest == PlaylistsRequest.Playlists;
                                        this.BindingModel.IsLoading = false;
                                    });


                            await Task.Delay(100);

                            if (t.IsCompleted && !t.IsFaulted)
                            {
                                await this.Dispatcher.RunAsync(
                                    CoreDispatcherPriority.High,
                                    () =>
                                        {
                                            this.View.SetGroups(t.Result);
                                            this.BindingModel.Count = t.Result.Sum(x => x.Playlists.Count);
                                        });

                                if (eventArgs.IsNavigationBack)
                                {
                                    object lastPlaylist;
                                    if (eventArgs.State.TryGetValue("ListViewHorizontalOffset", out lastPlaylist)
                                        && lastPlaylist is double)
                                    {
                                        await Task.Delay(100);
                                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () => this.View.ScrollToHorizontalOffset((double)lastPlaylist));
                                    }
                                }
                            }
                        });
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

                    this.musicPlaylistRepository.ChangeName(playlist.Id, newName).ContinueWith(
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

                this.musicPlaylistRepository.CreateAsync(string.Format(CultureInfo.CurrentCulture, "Playlist - {0}", DateTime.Now)).ContinueWith(
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

                    MessageDialog dialog = new MessageDialog("Are you sure want to delete playlist?");
                    dialog.Commands.Add(
                        new UICommand(
                            "Yes",
                            command => this.musicPlaylistRepository.DeleteAsync(((MusicPlaylist)playlist).Id).ContinueWith(
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
                playlists = await this.playlistCollectionsService.GetCollection<Album>().GetAllAsync(Order.Name);
            }
            else if (this.currentRequest == PlaylistsRequest.Playlists)
            {
                playlists = await this.playlistCollectionsService.GetCollection<MusicPlaylist>().GetAllAsync(Order.Name);
            }
            else if (this.currentRequest == PlaylistsRequest.Genres)
            {
                playlists = await this.playlistCollectionsService.GetCollection<Genre>().GetAllAsync(Order.Name);
            }
            else
            {
                playlists = await this.playlistCollectionsService.GetCollection<Artist>().GetAllAsync(Order.Name);
            }

            return playlists.GroupBy(x => string.IsNullOrEmpty(x.Title) ? ' ' : char.ToUpper(x.Title[0]))
                         .OrderBy(x => x.Key)
                         .Select(x => new PlaylistsGroupBindingModel(x.Key.ToString(), 0, x.Select(p => new PlaylistBindingModel(p)))).ToList();
        }
    }
}