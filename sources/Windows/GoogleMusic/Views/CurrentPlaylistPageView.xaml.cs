// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.Views;

    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Input;

    public interface ICurrentPlaylistPageView : IPageView
    {
        Task ScrollIntoCurrentSongAsync(SongBindingModel songBindingModel);
    }

    public sealed partial class CurrentPlaylistPageView : PageViewBase, ICurrentPlaylistPageView
    {
        private CurrentPlaylistPageViewPresenter presenter;

        public CurrentPlaylistPageView()
        {
            this.InitializeComponent();
            this.TrackItemsControl(this.ListView);
        }

        public async Task ScrollIntoCurrentSongAsync(SongBindingModel songBindingModel)
        {
            await Task.Run(
                async () =>
                    {
                        if (this.presenter != null && songBindingModel != null)
                        {
                            SongsBindingModel songsBindingModel = this.presenter.BindingModel;
                            if (songsBindingModel != null 
                                && songsBindingModel.Songs != null)
                            {
                                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, this.UpdateLayout);
                                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.ListView.ScrollIntoView(songBindingModel));
                            }
                        }
                    });
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<CurrentPlaylistPageViewPresenter>();
        }

        private void ListDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                var songBindingModel = frameworkElement.DataContext as SongBindingModel;
                if (songBindingModel != null)
                {
                    this.presenter.PlaySong(songBindingModel);
                }
            }
        }
    }
}
