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

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    using Windows.UI.Popups;

    public class PlaylistsPageViewPresenter : PagePresenterBase<IPlaylistsPageView, PlaylistsPageViewBindingModel>
    {
        private readonly IPlaylistCollectionsService playlistCollectionsService;

        private readonly IMusicPlaylistRepository musicPlaylistRepository;

        private PlaylistsRequest currentRequest;

        public PlaylistsPageViewPresenter(
            IDependencyResolverContainer container,
            IPlaylistCollectionsService playlistCollectionsService,
            IMusicPlaylistRepository musicPlaylistRepository)
            : base(container)
        {
            this.playlistCollectionsService = playlistCollectionsService;
            this.musicPlaylistRepository = musicPlaylistRepository;

            this.AddPlaylistCommand = new DelegateCommand(this.AddPlaylist);
            this.DeletePlaylistCommand = new DelegateCommand(this.DetelePlaylist, () => this.BindingModel.SelectedItem != null);
            this.EditPlaylistCommand = new DelegateCommand(this.EditPlaylist, () => this.BindingModel.SelectedItem != null);
        }

        public DelegateCommand AddPlaylistCommand { get; private set; }

        public DelegateCommand DeletePlaylistCommand { get; private set; }

        public DelegateCommand EditPlaylistCommand { get; private set; }

        public void ChangePlaylistName(string newName)
        {
            if (!this.IsDataLoading && this.BindingModel.IsEditable)
            {
                this.IsDataLoading = true;
                this.BindingModel.IsEditable = false;

                var playlistBindingModel = this.BindingModel.SelectedItem;

                if (playlistBindingModel != null)
                {
                    var playlist = (MusicPlaylist)playlistBindingModel.Playlist;

                    this.musicPlaylistRepository.ChangeName(playlist.Id, newName).ContinueWith(
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

        protected override void LoadData(NavigatedToEventArgs navigatedToEventArgs)
        {
            this.currentRequest = (PlaylistsRequest)navigatedToEventArgs.Parameter;
            this.BindingModel.Title = this.currentRequest.ToString();

            this.RefreshPlaylists();

            this.BindingModel.IsEditable = this.currentRequest == PlaylistsRequest.Playlists;
        }

        protected override IEnumerable<CommandMetadata> GetViewCommands()
        {
            if (this.currentRequest == PlaylistsRequest.Playlists)
            {
                yield return new CommandMetadata(CommandIcon.Add, "Add", this.AddPlaylistCommand);
            }
        }

        private IEnumerable<CommandMetadata> GetContextCommands()
        {
            if (this.currentRequest == PlaylistsRequest.Playlists)
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

                this.musicPlaylistRepository.CreateAsync(string.Format(CultureInfo.CurrentCulture, "Playlist - {0}", DateTime.Now)).ContinueWith(
                    async t =>
                    {
                        if (t.Result != null)
                        {
                            this.BindingModel.FreezeNotifications();

                            this.RefreshPlaylists();

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

        private async void RefreshPlaylists()
        {
            var playlists = await this.LoadPlaylistsAsync();
            this.BindingModel.Groups = playlists;
            this.BindingModel.Count = this.BindingModel.Groups.Sum(x => x.Playlists.Count);
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
                            command => this.musicPlaylistRepository.DeleteAsync(((MusicPlaylist)playlist).Id).ContinueWith(
                                async t =>
                                {
                                    if (t.IsCompleted && !t.IsFaulted && t.Result)
                                    {
                                        this.BindingModel.FreezeNotifications();
                                        this.RefreshPlaylists();
                                        
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