// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public sealed partial class PlayerView : UserControl
    {
        public PlayerView()
        {
            this.InitializeComponent();

            this.SizeChanged += (sender, args) =>
                {
                    if (ApplicationView.Value == ApplicationViewState.Snapped)
                    {
                        this.ProgressBarArea.Visibility = Visibility.Collapsed;
                        this.MoreButton.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        var width = this.ActualWidth - 104 /* Image */ - this.ButtonsArea.ActualWidth;
                        if (width > 0)
                        {
                            this.ProgressBarArea.Width = width;
                            this.ProgressBarArea.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this.ProgressBarArea.Visibility = Visibility.Collapsed;
                        }

                        this.MoreButton.Visibility = Visibility.Visible;
                    }
                };
        }

        private void MoreClick(object sender, RoutedEventArgs e)
        {
            this.MorePopup.IsOpen = true;
        }
    }
}
