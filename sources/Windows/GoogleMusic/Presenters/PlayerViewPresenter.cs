// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Views;

    public class PlayerViewPresenter : PlayerViewPresenterBase<PlayerBindingModel, IPlayerView> 
    {
        public PlayerViewPresenter(
            IMediaElementContainer mediaElementContainer, 
            IGoogleMusicSessionService sessionService, 
            IPlayQueueService queueService, 
            INavigationService navigationService, 
            PlayerBindingModel playerBindingModel)
            : base(mediaElementContainer, sessionService, queueService, navigationService, playerBindingModel)
        {
            this.ShowMoreCommand = new DelegateCommand(() => this.MainFrame.ShowPopup<IPlayerMorePopupView>(PopupRegion.AppToolBarRight));
        }

        public DelegateCommand ShowMoreCommand { get; set; }
    }
}