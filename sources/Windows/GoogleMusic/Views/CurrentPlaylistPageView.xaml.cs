// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public interface ICurrentPlaylistPageView : IPageView
    {
    }

    public sealed partial class CurrentPlaylistPageView : PageViewBase, ICurrentPlaylistPageView
    {
        private CurrentPlaylistPageViewPresenter presenter;

        public CurrentPlaylistPageView()
        {
            this.InitializeComponent();
            this.TrackItemsControl(this.ListView);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<CurrentPlaylistPageViewPresenter>();
            this.presenter.BindingModel.SelectedItems.CollectionChanged += this.SelectedItemsOnCollectionChanged;
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

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            this.UpdateCollection(this.ListView.SelectedItems, notifyCollectionChangedEventArgs.NewItems, notifyCollectionChangedEventArgs.OldItems);
        }

        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateCollection(this.presenter.BindingModel.SelectedItems, e.AddedItems, e.RemovedItems);
        }

        private void UpdateCollection<T>(IList<T> collection, IEnumerable newItems, IEnumerable oldItems)
        {
            if (oldItems != null)
            {
                foreach (T songBindingModel in oldItems)
                {
                    if (collection.Contains(songBindingModel))
                    {
                        collection.Remove(songBindingModel);
                    }
                }
            }

            if (newItems != null)
            {
                foreach (T songBindingModel in newItems)
                {
                    if (!collection.Contains(songBindingModel))
                    {
                        collection.Add(songBindingModel);
                    }
                }
            }
        }
    }
}
