// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class ExplorePageViewPresenter : PagePresenterBase<IExplorePageView>
    {
        private readonly IAllAccessService allAccessService;

        private readonly INavigationService navigationService;

        private readonly IRadioStationsService radioStationsService;

        private readonly IPlayQueueService playQueueService;

        private readonly IAnalyticsService analyticsService;

        private ExploreTab tab;

        private string subtitle;

        public ExplorePageViewPresenter(
            IAllAccessService allAccessService,
            INavigationService navigationService,
            IRadioStationsService radioStationsService,
            IPlayQueueService playQueueService,
            IAnalyticsService analyticsService)
        {
            this.allAccessService = allAccessService;
            this.navigationService = navigationService;
            this.radioStationsService = radioStationsService;
            this.playQueueService = playQueueService;
            this.analyticsService = analyticsService;

            this.NavigateToGroupCommand = new DelegateCommand(this.NavigateToGroup);
            this.NavigateToGenresCommand = new DelegateCommand(this.NavigateToGenres);
            this.StartRadioCommand = new DelegateCommand(this.StartRadio);
        }

        public string Subtitle
        {
            get
            {
                return this.subtitle;
            }

            set
            {
                this.SetValue(ref this.subtitle, value);
            }
        }

        public ExploreTab Tab
        {
            get
            {
                return this.tab;
            }

            set
            {
                this.SetValue(ref this.tab, value);
            }
        }

        public string GenresTitle
        {
            get
            {
                return this.Tab != null && this.Tab.ParentGenre != null ? "Subgenres" : "Genres";
            }
        }

        public DelegateCommand NavigateToGroupCommand { get; set; }

        public DelegateCommand NavigateToGenresCommand { get; set; }

        public DelegateCommand StartRadioCommand { get; set; }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            this.Tab = null;
            this.Subtitle = null;
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs, CancellationToken cancellationToken)
        {
            var playlistNavigationRequest = navigatedToEventArgs.Parameter as PlaylistNavigationRequest;

            AllAccessGenre parent = null;
            if (playlistNavigationRequest != null)
            {
                parent = (AllAccessGenre)playlistNavigationRequest.Playlist;

                await this.Dispatcher.RunAsync(
                    () =>
                    {
                        this.Subtitle = parent.Title;
                    });
            }

            var loadedTab = await this.allAccessService.GetExploreTabAsync(parent, cancellationToken);

            await this.Dispatcher.RunAsync(
                () =>
                {
                    this.Tab = loadedTab;
                });
        }

        protected override IEnumerable<CommandMetadata> GetViewCommands()
        {
            if (this.Tab != null && this.Tab.ParentGenre != null)
            {
                yield return new CommandMetadata(CommandIcon.Radio, "Start radio", this.StartRadioCommand);
            }
        }

        private void NavigateToGroup(object obj)
        {
            var exploreTabGroup = obj as ExploreTabGroup;
            if (exploreTabGroup != null)
            {
                this.analyticsService.SendEvent("Explore", "Navigate", exploreTabGroup.Title);

                if (exploreTabGroup.Songs != null)
                {
                    this.navigationService.NavigateTo<IPlaylistPageView>(
                        new PlaylistNavigationRequest(
                            this.Tab.ParentGenre,
                            string.IsNullOrEmpty(this.Subtitle) ? "Explore" : ("Explore - " + this.Subtitle),
                            exploreTabGroup.Title,
                            exploreTabGroup.Songs)
                        {
                            PlaylistType = PlaylistType.AllAccessGenre
                        });
                }
                else if (exploreTabGroup.Playlists != null)
                {
                    this.navigationService.NavigateTo<IPlaylistsPageView>(
                        new PlaylistNavigationRequest(
                            this.Tab.ParentGenre,
                            string.IsNullOrEmpty(this.Subtitle) ? "Explore" : ("Explore - " + this.Subtitle),
                            exploreTabGroup.Title,
                            exploreTabGroup.Playlists)
                        {
                            PlaylistType = PlaylistType.AllAccessGenre
                        });
                }
            }
        }

        private void NavigateToGenres()
        {
            if (this.Tab.Genres != null)
            {
                this.analyticsService.SendEvent("Explore", "Navigate", "Genres");

                this.navigationService.NavigateTo<IPlaylistsPageView>(
                    new PlaylistNavigationRequest(
                        this.Tab.ParentGenre,
                        string.IsNullOrEmpty(this.Subtitle) ? "Explore" : ("Explore - " + this.Subtitle),
                        this.GenresTitle,
                        this.Tab.Genres.Cast<IPlaylist>().ToList()));
            }
        }

        private async void StartRadio()
        {
            if (this.Tab.ParentGenre != null && !this.IsDataLoading)
            {
                this.analyticsService.SendEvent("Explore", "Execute", "StartRadio");

                await this.Dispatcher.RunAsync(() => this.IsDataLoading = true);

                var radio = await this.radioStationsService.CreateAsync(this.Tab.ParentGenre);

                if (radio != null)
                {
                    if (await this.playQueueService.PlayAsync(radio.Item1, radio.Item2, -1))
                    {
                        await this.Dispatcher.RunAsync(() => this.IsDataLoading = false);

                        this.navigationService.NavigateToPlaylist(radio.Item1);
                    }
                }
            }
        }
    }
}
