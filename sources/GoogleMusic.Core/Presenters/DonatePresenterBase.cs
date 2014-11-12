// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public abstract class DonatePresenterBase : ViewPresenterBase<IView>
    {
        private readonly IAnalyticsService analyticsService;
        private readonly IInAppPurchasesService inAppPurchasesService;

        private bool buying = false;

        protected DonatePresenterBase(IAnalyticsService analyticsService, IInAppPurchasesService inAppPurchasesService)
        {
            this.analyticsService = analyticsService;
            this.inAppPurchasesService = inAppPurchasesService;
            this.BuyPackageCommand = new DelegateCommand(this.BuyPackage, this.CanBuyPackage);
        }

        public DelegateCommand BuyPackageCommand { get; set; }

        public async void BuyPackage(object arg)
        {
            if (this.buying)
            {
                return;
            }

            var packageName = arg.ToString();

            this.buying = true;

            await this.inAppPurchasesService.RequestPurchase(packageName);

            this.analyticsService.SendEvent("Donate", "purchase", packageName);

            this.BuyPackageCommand.RaiseCanExecuteChanged();
        }

        private bool CanBuyPackage(object arg)
        {
            var packageName = arg.ToString();
            return !this.inAppPurchasesService.IsActive(packageName);
        }
    }
}
