// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.Presenters;

    public class RadioStationsViewPresenter : PagePresenterBase<IRadioStationsView>
    {
        private readonly IRadioWebService radioWebService;

        public RadioStationsViewPresenter(IRadioWebService radioWebService)
        {
            this.radioWebService = radioWebService;
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var radioStations = await this.radioWebService.GetAllAsync();
            await this.radioWebService.GetRadioSongsAsync(radioStations[0].Id);
        }
    }
}