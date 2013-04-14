// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    using OutcoldSolutions.GoogleMusic.Presenters.Popups;
    using OutcoldSolutions.Views;

    using Windows.System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Input;

    public interface IPlaylistEditPopupView : IPopupView
    {
    }

    public sealed partial class PlaylistEditPopupView : PopupViewBase, IPlaylistEditPopupView
    {
        private PlaylistEditPopupViewPresenter presenter;

        public PlaylistEditPopupView()
        {
            this.InitializeComponent();
            this.Loaded += (sender, args) =>
                {
                    this.TextBoxPlaylistName.Focus(FocusState.Keyboard);
                    this.TextBoxPlaylistName.SelectAll();
                };
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<PlaylistEditPopupViewPresenter>();
        }

        private void TextBoxPlaylistNameKeyUp(object sender, KeyRoutedEventArgs e)
        {
            this.presenter.Title = this.TextBoxPlaylistName.Text;
        }

        private void TextBoxPlaylistNameOnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                this.presenter.SaveCommand.Execute();
                e.Handled = true;
            }
            else if (e.Key == VirtualKey.Escape)
            {
                this.presenter.CancelCommand.Execute();
                e.Handled = true;
            }
        }
    }
}
