// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    public class StartPageViewPresenter : PagePresenterBase<IStartPageView, StartViewBindingModel>
    {
        private const int MaxItems = 12;

        private readonly IPlayQueueService playQueueService;

        private readonly INavigationService navigationService;

        private readonly IPlaylistsService playlistsService;

        public StartPageViewPresenter(
            INavigationService navigationService,
            IPlayQueueService playQueueService,
            IPlaylistsService playlistsService)
        {
            this.playQueueService = playQueueService;
            this.navigationService = navigationService;
            this.playlistsService = playlistsService;

            this.PlayCommand = new DelegateCommand(this.Play);

            // TODO: Make helper method in Framework
            var mainFrameRegionProvider = ApplicationBase.Container.Resolve<IMainFrameRegionProvider>();
            mainFrameRegionProvider.SetContent(MainFrameRegion.Links, ApplicationBase.Container.Resolve<LinksRegionView>());

            this.QueueCommand = new DelegateCommand(this.Queue, () => this.BindingModel.SelectedItems.Count > 0);
        }

        public DelegateCommand PlayCommand { get; set; }

        public DelegateCommand QueueCommand { get; private set; }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var types = new[]
                            {
                                PlaylistType.SystemPlaylist, 
                                PlaylistType.UserPlaylist, 
                                PlaylistType.Artist,
                                PlaylistType.Album, 
                                PlaylistType.Genre
                            };

            var groups = await Task.WhenAll(types.Select((t) => Task.Run(async () =>
                {
                    var countTask = this.playlistsService.GetCountAsync(t);
                    var getAllTask = this.playlistsService.GetAllAsync(t, Order.LastPlayed, MaxItems);

                    await Task.WhenAll(countTask, getAllTask);

                    int count = await countTask;
                    IEnumerable<IPlaylist> playlists = await getAllTask;

                    return this.CreateGroup(t.ToPluralTitle(), count, playlists, t);
                })));

            this.BindingModel.Groups = groups.ToList();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel.SelectedItems.CollectionChanged += this.SelectedItemsOnCollectionChanged;
        }

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnSelectedItemsChanged();
        }

        private void OnSelectedItemsChanged()
        {
            if (this.BindingModel.SelectedItems.Count > 0)
            {
                this.MainFrame.SetContextCommands(this.GetContextCommands());
            }
            else
            {
                this.MainFrame.ClearContextCommands();
            }
        }

        private IEnumerable<CommandMetadata> GetContextCommands()
        {
            yield return new CommandMetadata(CommandIcon.OpenWith, "Queue", this.QueueCommand);
        }

        private PlaylistsGroupBindingModel CreateGroup(string title, int playlistsCount, IEnumerable<IPlaylist> playlists, PlaylistType type)
        {
            List<PlaylistBindingModel> groupItems =
                playlists.Select(
                    playlist =>
                    new PlaylistBindingModel(playlist)
                        {
                            PlayCommand = this.PlayCommand
                        }).ToList();

            return new PlaylistsGroupBindingModel(
                title,
                playlistsCount,
                groupItems,
                type);
        }

        private void Play(object commandParameter)
        {
            IPlaylist playlist = commandParameter as IPlaylist;
            if (playlist != null)
            {
                this.MainFrame.IsBottomAppBarOpen = true;
                this.Logger.LogTask(this.playQueueService.PlayAsync(playlist));
                this.navigationService.NavigateToPlaylist(playlist);
            }
        }

        private void Queue()
        {
            this.MainFrame.ShowPopup<IQueueActionsPopupView>(PopupRegion.AppToolBarLeft, new SelectedItems(this.BindingModel.SelectedItems.Select(bm => bm.Playlist).ToList()));
        }
    }
}