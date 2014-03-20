// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reactive.Linq;

    using OutcoldSolutions.GoogleMusic.EventAggregator;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class MainMenuPresenter : ViewPresenterBase<IMainMenu>
    {
        private readonly INavigationService navigationService;

        private readonly IApplicationStateService applicationStateService;

        private readonly ISettingsService settingsService;

        private readonly IApplicationResources applicationResources;

        private bool isHomeSelected = false;
        private bool isPlaylistsSelected = false;
        private bool isRadioSelected = false;
        private bool isArtistsSelected = false;
        private bool isAlbumsSelected = false;
        private bool isGenresSelected = false;
        private bool isSearchSelected = false;
        private bool isExploreSelected = false;

        public MainMenuPresenter(
            INavigationService navigationService,
            IApplicationStateService applicationStateService,
            IEventAggregator eventAggregator,
            ISettingsService settingsService,
            IApplicationResources applicationResources)
        {
            this.navigationService = navigationService;
            this.applicationStateService = applicationStateService;
            this.settingsService = settingsService;
            this.applicationResources = applicationResources;

            this.HomeCommand = new DelegateCommand(() => this.navigationService.NavigateTo<IHomePageView>());
            this.UserPlaylistsCommand = new DelegateCommand(() => this.navigationService.NavigateTo<IUserPlaylistsPageView>(PlaylistType.UserPlaylist));
            this.RadioStationsCommand = new DelegateCommand(() => this.navigationService.NavigateTo<IRadioPageView>(PlaylistType.Radio));
            this.SearchCommand = new DelegateCommand(() => this.navigationService.NavigateTo<ISearchPageView>());
            this.ExploreCommand = new DelegateCommand(() => this.navigationService.NavigateTo<IExplorePageView>());
            this.PlaylistsCommand = new DelegateCommand(this.NavigatePlaylistsView);

            eventAggregator.GetEvent<ApplicationStateChangeEvent>()
                .Subscribe(async (e) => await this.Dispatcher.RunAsync(
                    () =>
                    {
                        this.RaisePropertyChanged(() => this.IsOnline);
                        this.RaisePropertyChanged(() => this.IsExploreVisible);
                    }));
            eventAggregator.GetEvent<SettingsChangeEvent>()
                .Where(x => string.Equals(x.Key, GoogleMusicCoreSettingsServiceExtensions.IsAllAccessAvailableKey, StringComparison.OrdinalIgnoreCase))
                .Subscribe(async (e) => await this.Dispatcher.RunAsync(
                    () =>
                    {
                        this.RaisePropertyChanged(() => this.RadioText);
                        this.RaisePropertyChanged(() => this.IsExploreVisible);
                    }));

            this.navigationService.NavigatedTo += this.NavigationServiceOnNavigatedTo;

            this.ViewCommands = new ObservableCollection<CommandMetadata>();

            this.IsHomeSelected = true;
        }

        public DelegateCommand HomeCommand { get; set; }

        public DelegateCommand QueueCommand { get; set; }

        public DelegateCommand UserPlaylistsCommand { get; set; }

        public DelegateCommand RadioStationsCommand { get; set; }

        public DelegateCommand ExploreCommand { get; set; }

        public DelegateCommand PlaylistsCommand { get; set; }

        public DelegateCommand SearchCommand { get; set; }

        public ObservableCollection<CommandMetadata> ViewCommands { get; set; } 

        public bool IsOnline
        {
            get
            {
                return this.applicationStateService.IsOnline();
            }
        }

        public bool IsExploreVisible
        {
            get
            {
                return this.settingsService.GetIsAllAccessAvailable() && this.IsOnline;
            }
        }

        public string RadioText
        {
            get
            {
                return this.settingsService.GetIsAllAccessAvailable()
                    ? this.applicationResources.GetString("MainMenu_Radio")
                    : this.applicationResources.GetString("MainMenu_InstantMixes");
            }
        }

        public bool IsHomeSelected
        {
            get
            {
                return this.isHomeSelected;
            }

            set
            {
                this.SetValue(ref this.isHomeSelected, value);
            }
        }

        public bool IsPlaylistsSelected
        {
            get
            {
                return this.isPlaylistsSelected;
            }

            set
            {
                this.SetValue(ref this.isPlaylistsSelected, value);
            }
        }

        public bool IsRadioSelected
        {
            get
            {
                return this.isRadioSelected;
            }

            set
            {
                this.SetValue(ref this.isRadioSelected, value);
            }
        }

        public bool IsExploreSelected
        {
            get
            {
                return this.isExploreSelected;
            }

            set
            {
                this.SetValue(ref this.isExploreSelected, value);
            }
        }

        public bool IsArtistsSelected
        {
            get
            {
                return this.isArtistsSelected;
            }

            set
            {
                this.SetValue(ref this.isArtistsSelected, value);
            }
        }

        public bool IsAlbumsSelected
        {
            get
            {
                return this.isAlbumsSelected;
            }

            set
            {
                this.SetValue(ref this.isAlbumsSelected, value);
            }
        }

        public bool IsGenresSelected
        {
            get
            {
                return this.isGenresSelected;
            }

            set
            {
                this.SetValue(ref this.isGenresSelected, value);
            }
        }

        public bool IsSearchSelected
        {
            get
            {
                return this.isSearchSelected;
            }

            set
            {
                this.SetValue(ref this.isSearchSelected, value);
            }
        }

        private void NavigationServiceOnNavigatedTo(object sender, NavigatedToEventArgs args)
        {
            this.FreezeNotifications();

            this.IsHomeSelected = (args.View is IHomePageView) && (args.View.GetPresenter<IPlaylistsPageViewPresenterBase>().IsMixedList);
            this.IsExploreSelected = args.View is IExplorePageView ||
                (args.View is IPlaylistsPageView && args.Parameter is PlaylistNavigationRequest && ((PlaylistNavigationRequest)args.Parameter).PlaylistType == PlaylistType.AllAccessGenre);
            this.IsPlaylistsSelected = (args.View is IUserPlaylistsPageView && args.Parameter is PlaylistType && ((PlaylistType)args.Parameter) == PlaylistType.UserPlaylist) ||
                (args.View is IPlaylistPageView && args.Parameter is PlaylistNavigationRequest && ((PlaylistNavigationRequest)args.Parameter).PlaylistType == PlaylistType.UserPlaylist);
            this.IsRadioSelected = (args.View is IRadioPageView && args.Parameter is PlaylistType && ((PlaylistType)args.Parameter) == PlaylistType.Radio) ||
                (args.View is IPlaylistPageView && args.Parameter is PlaylistNavigationRequest && ((PlaylistNavigationRequest)args.Parameter).PlaylistType == PlaylistType.Radio);
            this.IsArtistsSelected = args.View is IArtistPageView || 
                (args.View is IPlaylistsPageView && args.Parameter is PlaylistType && (PlaylistType)args.Parameter == PlaylistType.Artist) ||
                (args.View is IPlaylistPageView && args.Parameter is PlaylistNavigationRequest && ((PlaylistNavigationRequest)args.Parameter).PlaylistType == PlaylistType.Artist);
            this.IsAlbumsSelected = args.View is IAlbumPageView ||
                (args.View is IPlaylistsPageView && args.Parameter is PlaylistType && (PlaylistType)args.Parameter == PlaylistType.Album) ||
                (args.View is IPlaylistPageView && args.Parameter is PlaylistNavigationRequest && ((PlaylistNavigationRequest)args.Parameter).PlaylistType == PlaylistType.Album);
            this.IsGenresSelected = (args.View is IPlaylistsPageView && args.Parameter is PlaylistType && (PlaylistType)args.Parameter == PlaylistType.Genre) ||
                (args.View is IPlaylistPageView && args.Parameter is PlaylistNavigationRequest && ((PlaylistNavigationRequest)args.Parameter).PlaylistType == PlaylistType.Genre);
            this.IsSearchSelected = args.View is ISearchPageView;

            this.UnfreezeNotifications();
        }

        private void NavigatePlaylistsView(object obj)
        {
            this.navigationService.NavigateTo<IPlaylistsPageView>((PlaylistType)obj);
        }
    }
}
