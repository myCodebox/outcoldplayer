// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;
    using System.Globalization;

    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class TutorialPopupViewPresenter : ViewPresenterBase<ITutorialPopupView>
    {
        private int selectedPage;

        public TutorialPopupViewPresenter()
        {
            this.NextCommand = new DelegateCommand(() => { this.SelectedPage ++; }, () => this.SelectedPage < 5);
            this.PreviousCommand = new DelegateCommand(() => { this.SelectedPage--; }, () => this.SelectedPage > 1);

            this.SelectedPage = 1;
        }

        public DelegateCommand NextCommand { get; set; }

        public DelegateCommand PreviousCommand { get; set; }

        public int SelectedPage
        {
            get
            {
                return this.selectedPage;
            }

            set
            {
                this.SetValue(ref this.selectedPage, value);
                this.NextCommand.RaiseCanExecuteChanged();
                this.PreviousCommand.RaiseCanExecuteChanged();
                this.RaisePropertyChanged(() => this.TutorialImageUri);
            }
        }

        public Uri TutorialImageUri
        {
            get
            {
                return new Uri(string.Format(CultureInfo.InvariantCulture, "ms-appx:///Resources/tutorial-page-{0}.png", this.SelectedPage));
            }
        }
    }
}
