// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class ArtistPageViewPresenter : PagePresenterBase<IArtistPageView, ArtistPageViewBindingModel>
    {
        private readonly IApplicationResources resources;
        private readonly IPlayQueueService playQueueService;
        private readonly INavigationService navigationService;
        private readonly IPlaylistsService playlistsService;
        private readonly IAlbumsRepository albumsRepository;
        private readonly IRadioStationsService radioStationsService;
        private readonly IApplicationStateService applicationStateService;

        internal ArtistPageViewPresenter(
            IApplicationResources resources,
            IPlayQueueService playQueueService,
            INavigationService navigationService,
            IPlaylistsService playlistsService,
            IAlbumsRepository albumsRepository,
            IRadioStationsService radioStationsService,
            IApplicationStateService applicationStateService)
        {
            this.resources = resources;
            this.playQueueService = playQueueService;
            this.navigationService = navigationService;
            this.playlistsService = playlistsService;
            this.albumsRepository = albumsRepository;
            this.radioStationsService = radioStationsService;
            this.applicationStateService = applicationStateService;
            this.ShowAllCommand = new DelegateCommand(this.ShowAll);
            this.StartRadioCommand = new DelegateCommand(this.StartRadio);
        }

        public DelegateCommand ShowAllCommand { get; set; }

        public DelegateCommand StartRadioCommand { get; set; }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            this.BindingModel.Artist = null;
            this.BindingModel.Albums = null;
            this.BindingModel.Collections = null;
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var request = navigatedToEventArgs.Parameter as PlaylistNavigationRequest;
            if (request == null || request.PlaylistType != PlaylistType.Artist)
            {
                throw new NotSupportedException("Request should be PlaylistNavigationRequest and playlist type should be artist.");
            }

            this.BindingModel.Artist = await this.playlistsService.GetRepository<Artist>().GetAsync(request.PlaylistId);
            this.BindingModel.Albums = (await this.albumsRepository.GetArtistAlbumsAsync(request.PlaylistId)).Cast<IPlaylist>().ToList();
            this.BindingModel.Collections = (await this.albumsRepository.GetArtistCollectionsAsync(request.PlaylistId)).Cast<IPlaylist>().ToList();
        }

        protected override IEnumerable<CommandMetadata> GetViewCommands()
        {
            yield return new CommandMetadata(CommandSymbol.List, this.resources.GetString("Toolbar_ShowAllButton"), this.ShowAllCommand);
            if (this.applicationStateService.IsOnline())
            {
                yield return new CommandMetadata(CommandSymbol.Radio, "Start radio", this.StartRadioCommand);
            }
        }

        private void ShowAll()
        {
            this.NavigateToShowAllArtistsSongs();
        }

        private async void StartRadio()
        {
            if (!this.IsDataLoading)
            {
                await this.Dispatcher.RunAsync(() => this.IsDataLoading = true);

                var radio = await this.radioStationsService.CreateAsync(this.BindingModel.Artist);

                if (radio != null)
                {
                    await this.playQueueService.PlayAsync(radio.Item1, radio.Item2, -1);

                    await this.Dispatcher.RunAsync(() => this.IsDataLoading = false);

                    this.navigationService.NavigateTo<ICurrentPlaylistPageView>();
                }
            }
        }

        private void NavigateToShowAllArtistsSongs()
        {
            if (this.BindingModel.Artist != null)
            {
                this.navigationService.NavigateTo<IPlaylistPageView>(new PlaylistNavigationRequest(PlaylistType.Artist, this.BindingModel.Artist.Id));
            }
        }
    }
}