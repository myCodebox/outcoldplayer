// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class SituationStationsPageViewPresenter : PagePresenterBase<ISituationStationsPageView, SituationStationsPageViewBindingModel>
    {
        private readonly IRadioStationsService radioStationsService;
        private readonly INavigationService navigationService;
        private readonly IPlayQueueService playQueueService;

        public SituationStationsPageViewPresenter(
            IRadioStationsService radioStationsService,
            INavigationService navigationService,
            IPlayQueueService playQueueService)
        {
            this.radioStationsService = radioStationsService;
            this.navigationService = navigationService;
            this.playQueueService = playQueueService;
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            this.BindingModel.Title = null;
            this.BindingModel.Subtitle = null;
            this.BindingModel.Playlists = null;
            this.BindingModel.SituationStations = null;
        }

        protected override Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                var playlistNavigationRequest = navigatedToEventArgs.Parameter as PlaylistNavigationRequest;
                if (playlistNavigationRequest != null)
                {
                    var situationStations = playlistNavigationRequest.Playlist as SituationStations;
                    if (situationStations != null)
                    {
                        await this.Dispatcher.RunAsync(() =>
                        {
                            this.BindingModel.Title = situationStations.Title;
                            this.BindingModel.Subtitle = situationStations.Description;
                            this.BindingModel.PlaylistType = PlaylistType.Radio;
                            this.BindingModel.Playlists = situationStations.Stations.Cast<IPlaylist>().ToList();
                            this.BindingModel.SituationStations = situationStations;
                        });
                    }
                }
            }, cancellationToken);
        }

        public async void NavigateToRadio(SituationRadio situationRadio)
        {
            if (!this.IsDataLoading)
            {
                this.IsDataLoading = true;
                try
                {
                    if (situationRadio != null)
                    {
                        Tuple<Radio, IList<Song>> radio = await this.radioStationsService.CreateAsync(situationRadio);
                        await this.playQueueService.PlayAsync(radio.Item1, radio.Item2, -1);
                        this.navigationService.NavigateToPlaylist(radio.Item1);
                    }
                }
                finally
                {
                    this.IsDataLoading = false;
                }
            }
        }
    }
}
