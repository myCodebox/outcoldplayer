// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    using System;

    using OutcoldSolutions.GoogleMusic.Presenters.Popups;

    using Windows.System;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls.Primitives;

    public interface ILastfmAuthentificationView : IView
    {
        void Close();
    }

    public sealed partial class LastfmAuthentificationView : ViewBase, ILastfmAuthentificationView
    {
        public LastfmAuthentificationView()
        {
            this.InitializeComponent();
            this.InitializePresenter<LastfmAuthentificationPresenter>();

            Window.Current.Activated += this.CurrentOnActivated;
        }

        public void Close()
        {
            Window.Current.Activated -= this.CurrentOnActivated;

            var popup = this.Parent as Popup;
            if (popup != null)
            {
                popup.IsOpen = false;
            }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CurrentOnActivated(object sender, WindowActivatedEventArgs windowActivatedEventArgs)
        {
            if (windowActivatedEventArgs.WindowActivationState != CoreWindowActivationState.Deactivated)
            {
                Window.Current.Activated -= this.CurrentOnActivated; 
                this.Presenter<LastfmAuthentificationPresenter>().GetSession();
            }
        }

        private void NavigateToLastfm(object sender, RoutedEventArgs e)
        {
            Window.Current.Activated += this.CurrentOnActivated; 
            var task = Launcher.LaunchUriAsync(new Uri(this.Presenter<LastfmAuthentificationPresenter>().BindingModel.LinkUrl));
        }
    }
}
