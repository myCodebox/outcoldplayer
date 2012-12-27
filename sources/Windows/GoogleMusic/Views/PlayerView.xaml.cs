// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;

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
                        if (!this.progressBarManipulating)
                        {
                            this.ProgressBar.Value = this.MediaElement.Position.TotalSeconds;
                        }

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

                        this.SongsList.ScrollIntoView(this.Presenter<PlayerViewPresenter>().BindingModel.CurrentSong);
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

        private void ProgressBarValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Math.Abs(this.MediaElement.Position.TotalSeconds - this.ProgressBar.Value) > 2)
            {
                this.MediaElement.Position = TimeSpan.FromSeconds(this.ProgressBar.Value);
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
