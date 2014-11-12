// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;

    public class DonatePopupViewPresenter : DonatePresenterBase
    {
        public DonatePopupViewPresenter(IAnalyticsService analyticsService, IInAppPurchasesService inAppPurchasesService)
            : base(analyticsService, inAppPurchasesService)
        {
        }
    }
}
