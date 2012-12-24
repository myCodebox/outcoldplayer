// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;

    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.System.Display;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls.Primitives;

    public interface IPlayerView : IView
    {
        void PlaySong(Uri songUri);

        void Play();

        void Pause();

        void Stop();
    }

    public sealed partial class PlayerView : ViewBase, IPlayerView
    {
        private readonly DispatcherTimer timer = new DispatcherTimer();

        public PlayerView()
        {
            this.InitializePresenter<PlayerViewPresenter>();
            this.InitializeComponent();
            this.MediaElement.MediaOpened += (sender, args) =>
                {
                    this.ProgressBar.Value = 0;
                    this.ProgressBar.Maximum = this.MediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                };

            this.timer.Interval = TimeSpan.FromSeconds(1);
            this.timer.Tick += (sender, o) =>
                {
                    if (this.Presenter<PlayerViewPresenter>().BindingModel.IsPlaying)
                    {
                        this.ProgressBar.Value = this.MediaElement.Position.TotalSeconds;
                        this.CurrentTime.Text = string.Format("{0:N0}:{1:00}", this.MediaElement.Position.TotalMinutes, this.MediaElement.Position.Seconds);
                    }
                };

            this.timer.Start();
        }

        public void PlaySong(Uri songUri)
        {
            this.Dispatcher.RunAsync(
                CoreDispatcherPriority.High,
                () =>
                    {
                        if (this.MediaElement.Source != null)
                        {
                            this.MediaElement.Stop();
                        }

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

        private void ProgressBar_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Math.Abs(this.MediaElement.Position.TotalSeconds - this.ProgressBar.Value) > 2)
            {
                this.MediaElement.Position = TimeSpan.FromSeconds(this.ProgressBar.Value);
            }
        }
    }
}
