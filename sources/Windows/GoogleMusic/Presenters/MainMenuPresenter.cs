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
        private bool isQueueSelected = false;
        private bool isPlaylistsSelected = false;
        private bool isRadioSelected = false;
        private bool isArtistsSelected = false;
        private bool isAlbumsSelected = false;
        private bool isGenresSelected = false;

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

            this.HomeCommand = new DelegateCommand(() => this.navigationService.NavigateTo<IStartPageView>());
            this.QueueCommand = new DelegateCommand(() => this.navigationService.NavigateTo<ICurrentPlaylistPageView>());
            this.UserPlaylistsCommand = new DelegateCommand(() => this.navigationService.NavigateTo<IUserPlaylistsPageView>(PlaylistType.UserPlaylist));
            this.RadioStationsCommand = new DelegateCommand(() => this.navigationService.NavigateTo<IRadioPageView>(PlaylistType.Radio));
            this.PlaylistsCommand = new DelegateCommand(this.NavigatePlaylistsView);

            eventAggregator.GetEvent<ApplicationStateChangeEvent>()
                .Subscribe((e) => this.RaisePropertyChanged(() => this.IsRadioVisible));
            eventAggregator.GetEvent<SettingsChangeEvent>()
                .Where(x => string.Equals(x.Key, GoogleMusicCoreSettingsServiceExtensions.IsAllAccessAvailableKey, StringComparison.OrdinalIgnoreCase))
                .Subscribe((e) => this.RaisePropertyChanged(() => this.RadioText));

            this.navigationService.NavigatedTo += this.NavigationServiceOnNavigatedTo;

            this.ViewCommands = new ObservableCollection<CommandMetadata>();
        }

        public DelegateCommand HomeCommand { get; set; }

        public DelegateCommand QueueCommand { get; set; }

        public DelegateCommand UserPlaylistsCommand { get; set; }

        public DelegateCommand RadioStationsCommand { get; set; }

        public DelegateCommand PlaylistsCommand { get; set; }

        public ObservableCollection<CommandMetadata> ViewCommands { get; set; } 

        public bool IsRadioVisible
        {
            get
            {
                return this.applicationStateService.IsOnline();
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

        public bool IsQueueSelected
        {
            get
            {
                return this.isQueueSelected;
            }

            set
            {
                this.SetValue(ref this.isQueueSelected, value);
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

        private void NavigationServiceOnNavigatedTo(object sender, NavigatedToEventArgs args)
        {
            this.FreezeNotifications();

            this.IsHomeSelected = args.View is IStartPageView;
            this.IsQueueSelected = args.View is ICurrentPlaylistPageView;
            this.IsPlaylistsSelected = (args.View is IUserPlaylistsPageView) || 
                (args.View is IPlaylistPageView && ((PlaylistNavigationRequest)args.Parameter).PlaylistType == PlaylistType.UserPlaylist);
            this.IsRadioSelected = (args.View is IRadioPageView) ||
                (args.View is IPlaylistPageView && ((PlaylistNavigationRequest)args.Parameter).PlaylistType == PlaylistType.UserPlaylist);
            this.IsArtistsSelected = (args.View is IPlaylistsPageView && (PlaylistType)args.Parameter == PlaylistType.Artist) ||
                (args.View is IPlaylistPageView && ((PlaylistNavigationRequest)args.Parameter).PlaylistType == PlaylistType.UserPlaylist);
            this.IsAlbumsSelected = (args.View is IPlaylistsPageView && (PlaylistType)args.Parameter == PlaylistType.Album) ||
                (args.View is IPlaylistPageView && ((PlaylistNavigationRequest)args.Parameter).PlaylistType == PlaylistType.UserPlaylist);
            this.IsGenresSelected = (args.View is IPlaylistsPageView && (PlaylistType)args.Parameter == PlaylistType.Genre) ||
                (args.View is IPlaylistPageView && ((PlaylistNavigationRequest)args.Parameter).PlaylistType == PlaylistType.UserPlaylist);

            this.UnfreezeNotifications();
        }

        private void NavigatePlaylistsView(object obj)
        {
            this.navigationService.NavigateTo<IPlaylistsPageView>((PlaylistType)obj);
        }
    }
}
