// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Specialized;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.Views;

    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public interface IPlaylistPageView : IPageView
    {
    }

    public sealed partial class PlaylistPageView : PageViewBase, IPlaylistPageView
    {
        private PlaylistPageViewPresenter presenter;

        public PlaylistPageView()
        {
            this.InitializeComponent();
            this.TrackItemsControl(this.ListView);
        }
        
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<PlaylistPageViewPresenter>();
            this.presenter.BindingModel.SongsBindingModel.SelectedItems.CollectionChanged += this.SelectedItemsOnCollectionChanged;
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

        private async void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            CollectionExtensions.UpdateCollection(this.ListView.SelectedItems, notifyCollectionChangedEventArgs.NewItems, notifyCollectionChangedEventArgs.OldItems);

            await Task.Yield();

            this.Dispatcher.RunAsync(
                CoreDispatcherPriority.Low,
                () =>
                {
                    if (notifyCollectionChangedEventArgs.NewItems != null
                        && notifyCollectionChangedEventArgs.NewItems.Count > 0)
                    {
                        this.ListView.ScrollIntoView(notifyCollectionChangedEventArgs.NewItems[0]);
                    }
                });
        }

        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CollectionExtensions.UpdateCollection(this.presenter.BindingModel.SongsBindingModel.SelectedItems, e.AddedItems, e.RemovedItems);
        }
    }
}
