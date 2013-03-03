﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    using System;

    using OutcoldSolutions.GoogleMusic.Presenters.Popups;
    using OutcoldSolutions.Views;

    using Windows.System;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls.Primitives;

    public interface ILastfmAuthentificationView : IView
    {
        void Close();
    }

    public sealed partial class LastfmAuthentificationPageView : PageViewBase, ILastfmAuthentificationView
    {
        private LastfmAuthentificationPresenter presenter;

        public LastfmAuthentificationPageView()
        {
            this.InitializeComponent();

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

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<LastfmAuthentificationPresenter>();
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
                this.presenter.GetSession();
            }
        }

        private void NavigateToLastfm(object sender, RoutedEventArgs e)
        {
            Window.Current.Activated += this.CurrentOnActivated;
            var task = Launcher.LaunchUriAsync(new Uri(this.presenter.BindingModel.LinkUrl));
        }
    }
}
