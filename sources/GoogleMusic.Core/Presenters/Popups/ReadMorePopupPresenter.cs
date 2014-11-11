// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class ReadMorePopupPresenter : ViewPresenterBase<IReadMorePopup>
    {
        public ReadMorePopupPresenter(string text)
        {
            this.Text = text;

            this.LeavePageCommand = new DelegateCommand(() => this.View.Close());
        }

        public string Text { get; set; }

        public DelegateCommand LeavePageCommand { get; set; }
    }
}
