// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml.Input;

    public interface IAlbumPageView : IDataPageView
    {
    }

    public sealed partial class AlbumPageView : DataPageViewBase, IAlbumPageView
    {
        private AlbumPageViewPresenter presenter;

        public AlbumPageView()
        {
            this.InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<AlbumPageViewPresenter>();
        }

        private void ListDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (this.presenter.PlayCommand.CanExecute())
            {
                this.presenter.PlayCommand.Execute();
            }
        }
    }
}
