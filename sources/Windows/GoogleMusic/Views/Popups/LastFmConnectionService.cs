// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;

    public interface ILastFmConnectionService
    {
        void Connect();
    }

    public class LastFmConnectionService : ILastFmConnectionService
    {
        private readonly IDependencyResolverContainer container;

        public LastFmConnectionService(IDependencyResolverContainer container)
        {
            this.container = container;
        }

        public void Connect()
        {
            var settingsPopup = new Popup
                                    {
                                        IsLightDismissEnabled = false,
                                        Width = Window.Current.Bounds.Width,
                                        Height = Window.Current.Bounds.Height
                                    };

            var frameworkElement = (FrameworkElement)this.container.Resolve<ILastfmAuthentificationView>();

            frameworkElement.Height = settingsPopup.Height;
            frameworkElement.Width = settingsPopup.Width;

            settingsPopup.Child = frameworkElement;
            settingsPopup.SetValue(Canvas.LeftProperty, 0);
            settingsPopup.SetValue(Canvas.TopProperty, 0);
            settingsPopup.IsOpen = true;
        }
    }
}