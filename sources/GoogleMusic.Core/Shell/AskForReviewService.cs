// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class AskForReviewService
    {
        private const int AskForReviewStarts = 100;

        private const string DoNotAskToReviewKey = "DoNotAskToReviewKey-v3";
        private const string CountOfStartsBeforeReview = "CountOfStartsBeforeReview-v3";

        private readonly INavigationService navigationService;
        private readonly ISettingsService settingsService;
        private readonly IMainFrame mainFrame;

        private readonly IDispatcher dispatcher;

        private readonly ILogger logger;

        public AskForReviewService(
            INavigationService navigationService,
            ISettingsService settingsService,
            IMainFrame mainFrame,
            IDispatcher dispatcher,
            ILogManager logManager)
        {
            this.navigationService = navigationService;
            this.settingsService = settingsService;
            this.mainFrame = mainFrame;
            this.dispatcher = dispatcher;
            this.logger = logManager.CreateLogger("AskForReviewService");

            this.navigationService.NavigatedTo += this.NavigationServiceOnNavigatedTo;
        }

        private void NavigationServiceOnNavigatedTo(object sender, NavigatedToEventArgs navigatedToEventArgs)
        {
            bool dontAsk = this.settingsService.GetApplicationValue<bool>(DoNotAskToReviewKey);
            if (dontAsk)
            {
                this.navigationService.NavigatedTo -= this.NavigationServiceOnNavigatedTo;
            }
            else
            {
                int startsCount = this.settingsService.GetApplicationValue<int>(CountOfStartsBeforeReview);
                if (startsCount >= AskForReviewStarts)
                {
                    try
                    {
                        this.AskForReview();
                    }
                    catch (Exception e)
                    {
                        this.logger.Error(e, "AskForReview failed");
                    }
                }
                else
                {
                    this.settingsService.SetApplicationValue<int>(CountOfStartsBeforeReview, startsCount + 1);
                }
            }
        }

        private void AskForReview()
        {
            this.dispatcher.RunAsync(
                () =>
                {
                    var donatePopupView = this.mainFrame.ShowPopup<IDonatePopupView>(PopupRegion.Full);
                    donatePopupView.Closed += this.DonateOnClosed;
                });
        }

        private void DonateOnClosed(object sender, EventArgs eventArgs)
        {
            var donatePopupView = sender as IDonatePopupView;
            if (donatePopupView != null)
            {
                donatePopupView.Closed -= this.DonateOnClosed;
            }

            var donateCloseEventArgs = eventArgs as DonateCloseEventArgs;
            if (donateCloseEventArgs != null)
            {
                if (donateCloseEventArgs.Later)
                {
                    this.settingsService.SetApplicationValue<int>(CountOfStartsBeforeReview, 0);
                }
                else
                {
                    this.settingsService.SetApplicationValue<bool>(DoNotAskToReviewKey, true);
                }
            }
        }
    }
}
