// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;

    using OutcoldSolutions.GoogleMusic.Presenters;
    
    using Windows.UI.Core;
    using Windows.UI.Xaml;

    public interface IPlayerView : IView
    {
        void PlaySong(Uri songUri);

        void Play();

        void Pause();

        void Stop();
    }

    public sealed partial class PlayerView : ViewBase, IPlayerView
    {
        public PlayerView()
        {
            this.InitializePresenter<PlayerViewPresenter>();
            this.InitializeComponent();
        }

        public void PlaySong(Uri songUri)
        {
            this.Dispatcher.RunAsync(
                CoreDispatcherPriority.High,
                () =>
                    {
                        this.MediaElement.Source = songUri;
                        this.MediaElement.Play();
                    });
        }

        public void Play()
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () => this.MediaElement.Play());
        }

        public void Pause()
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () => this.MediaElement.Pause());
        }

        public void Stop()
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () => this.MediaElement.Stop());
        }

        private void MediaEnded(object sender, RoutedEventArgs e)
        {
            this.Presenter<PlayerViewPresenter>().OnMediaEnded();
        }
    }
}
