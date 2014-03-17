// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public abstract class DonatePresenterBase : ViewPresenterBase<IView>
    {
        private bool buying = false;

        protected DonatePresenterBase()
        {
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

            await InAppPurchases.RequestPurchase(packageName);

            this.BuyPackageCommand.RaiseCanExecuteChanged();
        }

        private bool CanBuyPackage(object arg)
        {
            var packageName = arg.ToString();
            return !InAppPurchases.IsActive(packageName);
        }
    }
}
