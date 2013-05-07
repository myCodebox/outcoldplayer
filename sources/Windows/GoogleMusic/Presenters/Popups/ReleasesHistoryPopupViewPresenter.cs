// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Presenters;

    using Windows.ApplicationModel;

    public class ReleasesHistoryPopupViewPresenter : ViewPresenterBase<IReleasesHistoryPopupView>
    {
        public ReleasesHistoryPopupViewPresenter()
        {
            this.LeavePageCommand = new DelegateCommand(this.LeavePage);
            this.Version = Package.Current.Id.Version.ToVersionString();
        }

        public DelegateCommand LeavePageCommand { get; private set; }

        public string Version { get; private set; }

        private void LeavePage()
        {
            this.View.Close();
        }
    }
}