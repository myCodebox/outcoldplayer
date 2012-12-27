// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;
    using System.Globalization;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Input;

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

        private bool progressBarManipulating = false;

        private MediaElement mediaElement;

        public PlayerView()
        {
            this.InitializePresenter<PlayerViewPresenter>();
            this.InitializeComponent();
           
            this.timer.Interval = TimeSpan.FromSeconds(1);
            this.timer.Start();
        }

        public void SetMediaElement(MediaElement mediaElement)
        {
            this.mediaElement = mediaElement;

            this.mediaElement.MediaOpened += (sender, args) =>
            {
                this.ProgressBar.Value = 0;
                this.ProgressBar.Maximum = this.mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            };

            this.mediaElement.DownloadProgressChanged += (sender, args) =>
            {
                this.DownloadProgressPanel.Visibility = this.mediaElement.DownloadProgress > 0.001 && this.mediaElement.DownloadProgress <= 0.999
                                                       ? Visibility.Visible
                                                       : Visibility.Collapsed;
                this.DownloadProgressText.Text = string.Format(CultureInfo.CurrentCulture, "{0:P0}", this.mediaElement.DownloadProgress);
            };

            this.mediaElement.MediaEnded += (sender, args) => this.Presenter<PlayerViewPresenter>().OnMediaEnded();

            this.timer.Tick += (sender, o) =>
            {
                if (this.Presenter<PlayerViewPresenter>().BindingModel.IsPlaying)
                {
                    if (!this.progressBarManipulating)
                    {
                        this.ProgressBar.Value = this.mediaElement.Position.TotalSeconds;
                    }
                    
                    this.CurrentTime.Text = string.Format("{0:N0}:{1:00}", this.mediaElement.Position.Subtract(TimeSpan.FromSeconds(this.mediaElement.Position.Seconds)).TotalMinutes, this.mediaElement.Position.Seconds);
                }
            };
        }

        public void PlaySong(Uri songUri)
        {
            this.Dispatcher.RunAsync(
                CoreDispatcherPriority.High,
                () =>
                    {
                        if (this.mediaElement.Source != null)
                        {
                            this.mediaElement.Stop();
                        }

                        this.mediaElement.Source = songUri;
                        this.mediaElement.Play();

                        // this.SongsList.ScrollIntoView(this.Presenter<PlayerViewPresenter>().BindingModel.CurrentSong);
                    });
        }

        public void Play()
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () => this.mediaElement.Play());
        }

        public void Pause()
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () => this.mediaElement.Pause());
        }

        public void Stop()
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () => this.mediaElement.Stop());
        }

        private void ProgressBarValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Math.Abs(this.mediaElement.Position.TotalSeconds - this.ProgressBar.Value) > 2)
            {
                this.mediaElement.Position = TimeSpan.FromSeconds(this.ProgressBar.Value);
            }
        }

        private void ProgressBarManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            this.progressBarManipulating = true;
        }

        private void ProgressBarManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            this.progressBarManipulating = false;
        }

        private void SongsListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (SongBindingModel removedItem in e.RemovedItems)
            {
                removedItem.IsSelected = false;
            }

            foreach (SongBindingModel removedItem in e.AddedItems)
            {
                removedItem.IsSelected = true;
            }
        }

        private void ItemPlayClick(object sender, RoutedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)e.OriginalSource;
            var songBindingModel = frameworkElement.DataContext as SongBindingModel;
            if (songBindingModel != null)
            {
                this.Presenter<PlayerViewPresenter>().Stop();
                this.Presenter<PlayerViewPresenter>().BindingModel.CurrentSongIndex = songBindingModel.Index - 1;
                this.Presenter<PlayerViewPresenter>().Play();
            }
        }
    }
}
