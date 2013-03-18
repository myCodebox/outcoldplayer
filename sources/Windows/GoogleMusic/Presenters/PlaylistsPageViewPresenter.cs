// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.Presenters;

    using Windows.UI.Popups;

    public class PlaylistsPageViewPresenter : DataPagePresenterBase<IPlaylistsPageView, PlaylistsPageViewBindingModel>
    {
        private readonly IPlaylistCollectionsService playlistCollectionsService;
        private readonly IUserPlaylistsRepository userPlaylistsRepository;
        private readonly INavigationService navigationService;
        private readonly IPlayQueueService playQueueService;

        private PlaylistType currentRequest;

        public PlaylistsPageViewPresenter(
            IPlaylistCollectionsService playlistCollectionsService,
            IUserPlaylistsRepository userPlaylistsRepository,
            INavigationService navigationService,
            IPlayQueueService playQueueService)
        {
            this.playlistCollectionsService = playlistCollectionsService;
            this.userPlaylistsRepository = userPlaylistsRepository;
            this.navigationService = navigationService;
            this.playQueueService = playQueueService;

            this.AddPlaylistCommand = new DelegateCommand(this.AddPlaylist);
            this.DeletePlaylistCommand = new DelegateCommand(this.DetelePlaylist, () => this.BindingModel.SelectedItem != null);
            this.EditPlaylistCommand = new DelegateCommand(this.EditPlaylist, () => this.BindingModel.SelectedItem != null);
            this.PlayCommand = new DelegateCommand(this.Play);
        }

        public DelegateCommand AddPlaylistCommand { get; private set; }

        public DelegateCommand DeletePlaylistCommand { get; private set; }

        public DelegateCommand EditPlaylistCommand { get; private set; }

        public DelegateCommand PlayCommand { get; private set; }

        public void ChangePlaylistName(string newName)
        {
            if (!this.IsDataLoading && this.BindingModel.IsEditable)
            {
                this.IsDataLoading = true;
                this.BindingModel.IsEditable = false;

                var playlistBindingModel = this.BindingModel.SelectedItem;

                if (playlistBindingModel != null)
                {
                    var playlist = (UserPlaylistBindingModel)playlistBindingModel.Playlist;

                    this.userPlaylistsRepository.ChangeName(playlist.Metadata, newName).ContinueWith(
                        t =>
                            {
                                this.IsDataLoading = false;
                                this.BindingModel.IsEditable = true;
                            },
                        TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel.Subscribe(() => this.BindingModel.SelectedItem, this.SelectedItemChanged);
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            this.currentRequest = (PlaylistType)navigatedToEventArgs.Parameter;

            await this.RefreshPlaylists();

            this.BindingModel.IsEditable = this.currentRequest == PlaylistType.UserPlaylist;
        }

        protected override IEnumerable<CommandMetadata> GetViewCommands()
        {
            if (this.currentRequest == PlaylistType.UserPlaylist)
            {
                yield return new CommandMetadata(CommandIcon.Add, "Add", this.AddPlaylistCommand);
            }
        }

        private IEnumerable<CommandMetadata> GetContextCommands()
        {
            if (this.currentRequest == PlaylistType.UserPlaylist)
            {
                yield return new CommandMetadata(CommandIcon.Edit, "Rename", this.EditPlaylistCommand);
                yield return new CommandMetadata(CommandIcon.Delete, "Delete", this.DeletePlaylistCommand);
            }
        }

        private void SelectedItemChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            this.AddPlaylistCommand.RaiseCanExecuteChanged();
            this.DeletePlaylistCommand.RaiseCanExecuteChanged();
            this.EditPlaylistCommand.RaiseCanExecuteChanged();

            if (this.BindingModel.IsEditable && this.BindingModel.SelectedItem != null)
            {
                this.Toolbar.SetContextCommands(this.GetContextCommands());
            }
            else
            {
                this.Toolbar.ClearContextCommands();
            }
        }

        private void AddPlaylist()
        {
            if (!this.IsDataLoading && this.BindingModel.IsEditable)
            {
                this.IsDataLoading = true;
                this.BindingModel.IsEditable = false;

                this.userPlaylistsRepository.CreateAsync(string.Format(CultureInfo.CurrentCulture, "Playlist - {0}", DateTime.Now)).ContinueWith(
                    async t =>
                    {
                        if (t.Result != null)
                        {
                            this.BindingModel.FreezeNotifications();

                            await this.RefreshPlaylists();

                            await this.Dispatcher.RunAsync(
                                () =>
                                {
                                    this.BindingModel.UnfreezeNotifications();
                                    this.BindingModel.IsEditable = true;
                                    this.View.Refresh();
                                    var playlistBindingModel =
                                        this.BindingModel.Groups.SelectMany(x => x.Playlists)
                                                 .FirstOrDefault(x => x.Playlist == t.Result);
                                    this.View.ShowPlaylist(playlistBindingModel);
                                    this.IsDataLoading = false;
                                });
                        }
                    });
            }
        }

        private async Task RefreshPlaylists()
        {
            var playlists = await this.LoadPlaylistsAsync();
            this.BindingModel.Groups = playlists;
            this.BindingModel.Title = string.Format("{0} ({1})", this.currentRequest.ToString(), this.BindingModel.Groups.Sum(x => x.Playlists.Count));
        }

        private void DetelePlaylist()
        {
            if (!this.IsDataLoading && this.BindingModel.IsEditable)
            {
                this.IsDataLoading = true;
                this.BindingModel.IsEditable = false;

                var playlistBindingModel = this.BindingModel.SelectedItem;

                if (playlistBindingModel != null)
                {
                    var playlist = playlistBindingModel.Playlist;

                    MessageDialog dialog = new MessageDialog("Are you sure want to delete playlist?");
                    dialog.Commands.Add(
                        new UICommand(
                            "Yes",
                            command => this.userPlaylistsRepository.DeleteAsync(((UserPlaylistBindingModel)playlist).Metadata).ContinueWith(
                                async t =>
                                {
                                    if (t.IsCompleted && !t.IsFaulted && t.Result)
                                    {
                                        this.BindingModel.FreezeNotifications();
                                        await this.RefreshPlaylists();
                                        
                                        await this.Dispatcher.RunAsync(
                                                       () =>
                                                       {
                                                           this.BindingModel.UnfreezeNotifications();
                                                           this.BindingModel.IsEditable = true;
                                                           this.View.Refresh();
                                                           this.IsDataLoading = false;
                                                       });
                                    }
                                })));

                    dialog.Commands.Add(
                        new UICommand(
                            "No",
                            command => this.Dispatcher.RunAsync(
                                () =>
                                {
                                    this.IsDataLoading = false;
                                    this.BindingModel.IsEditable = true;
                                })));

                    dialog.DefaultCommandIndex = 0;
                    dialog.CancelCommandIndex = 1;
                    this.Logger.LogTask(dialog.ShowAsync().AsTask());
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
            IEnumerable<PlaylistBaseBindingModel> queue = null;
            switch (this.currentRequest)
            {
                case PlaylistType.Album:
                    queue = await this.playlistCollectionsService.GetCollection<AlbumBindingModel>().GetAllAsync(Order.Name);
                    break;
                case PlaylistType.UserPlaylist:
                    queue = await this.playlistCollectionsService.GetCollection<UserPlaylistBindingModel>().GetAllAsync(Order.Name);
                    break;
                case PlaylistType.Genre:
                    queue = await this.playlistCollectionsService.GetCollection<GenreBindingModel>().GetAllAsync(Order.Name);
                    break;
                case PlaylistType.Artist:
                    queue = await this.playlistCollectionsService.GetCollection<ArtistBindingModel>().GetAllAsync(Order.Name);
                    break;
            }

            List<PlaylistsGroupBindingModel> groups = new List<PlaylistsGroupBindingModel>();

            if (queue != null)
            {
                foreach (var group in queue.GroupBy(x => string.IsNullOrEmpty(x.Title) ? string.Empty : x.Title.Substring(0, 1), StringComparer.CurrentCultureIgnoreCase))
                {
                    var playlistBindingModels = group.Select(p => new PlaylistBindingModel(p) { PlayCommand = this.PlayCommand });
                    groups.Add(new PlaylistsGroupBindingModel(group.Key, 0, playlistBindingModels));
                }
            }

            return groups;
        }

        private void Play(object commandParameter)
        {
            PlaylistBaseBindingModel playlist = commandParameter as PlaylistBaseBindingModel;
            if (playlist != null)
            {
                this.navigationService.NavigateTo<IPlaylistPageView>(playlist);
                this.playQueueService.PlayAsync(null, playlist.Songs.Select(x => x.Metadata).ToList(), songIndex: -1);
                this.Toolbar.IsBottomAppBarOpen = true;
            }
        }
    }
}