// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Settings
{
    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Shell;
    using OutcoldSolutions.Views;

    public class UpgradeViewPresenter : ViewPresenterBase<IView>
    {
        private readonly IApplicationSettingViewsService applicationSettingViewsService;

        private bool buying = false;

        public UpgradeViewPresenter(
            IDependencyResolverContainer container,
            IApplicationSettingViewsService applicationSettingViewsService)
            : base(container)
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