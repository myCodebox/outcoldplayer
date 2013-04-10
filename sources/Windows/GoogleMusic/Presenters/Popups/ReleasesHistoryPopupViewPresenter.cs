// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Presenters;

    public class ReleasesHistoryPopupViewPresenter : ViewPresenterBase<IReleasesHistoryPopupView>
    {
        public ReleasesHistoryPopupViewPresenter()
        {
            this.LeavePageCommand = new DelegateCommand(this.LeavePage);
        }

        public DelegateCommand LeavePageCommand { get; private set; }

        private void LeavePage()
        {
            this.View.Close();
        }
    }
}