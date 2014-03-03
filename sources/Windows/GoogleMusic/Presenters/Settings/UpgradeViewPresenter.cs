// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Settings
{
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views;

    public class UpgradeViewPresenter : ViewPresenterBase<IView>
    {
        private readonly IApplicationSettingViewsService applicationSettingViewsService;

        private bool buying = false;

        public UpgradeViewPresenter(
            IApplicationSettingViewsService applicationSettingViewsService)
        {
            this.applicationSettingViewsService = applicationSettingViewsService;

            this.BuyPackageCommand = new DelegateCommand(this.BuyPackage, this.CanBuyPackage);
        }

        public DelegateCommand BuyPackageCommand { get; set; }

        public void BuyPackage(object arg)
        {
            if (this.buying)
            {
                return;
            }

            var packageName = arg.ToString();

            this.buying = true;

            this.Logger.LogTask(InAppPurchases.RequestPurchase(packageName));
            this.applicationSettingViewsService.Close();
        }

        private bool CanBuyPackage(object arg)
        {
            var packageName = arg.ToString();
            return !InAppPurchases.IsActive(packageName);
        }
    }
}