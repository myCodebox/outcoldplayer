//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Specialized;
    using System.Diagnostics;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public interface IStartPageView : IPageView
    {
    }

    public sealed partial class StartPageView : PageViewBase, IStartPageView
    {
        private StartPageViewPresenter presenter;

        public StartPageView()
        {
            this.InitializeComponent();
            this.TrackItemsControl(this.GridView);
        }

        public override void OnUnfreeze(NavigatedToEventArgs eventArgs)
        {
            base.OnUnfreeze(eventArgs);

            this.Groups.Source = this.presenter.BindingModel.Groups;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<StartPageViewPresenter>();
            this.presenter.BindingModel.SelectedItems.CollectionChanged += this.SelectedItemsOnCollectionChanged;
        }

        private void PlaylistItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as PlaylistBindingModel;

            Debug.Assert(album != null, "album != null");
            if (album != null)
            {
                this.NavigationService.NavigateToPlaylist(album.Playlist);
            }
        }
        
        private void NavigateTo(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement != null)
            {
                var groupBindingModel = frameworkElement.DataContext as PlaylistsGroupBindingModel;
                if (groupBindingModel != null)
                {
                    if (groupBindingModel.Request == PlaylistType.UserPlaylist)
                    {
                        this.NavigationService.NavigateTo<IUserPlaylistsPageView>(PlaylistType.UserPlaylist);
                    }
                    else
                    {
                        this.NavigationService.NavigateTo<IPlaylistsPageView>(groupBindingModel.Request);
                    }
                }
            }
        }

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionExtensions.UpdateCollection(this.GridView.SelectedItems, e.NewItems, e.OldItems);
        }

        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CollectionExtensions.UpdateCollection(this.presenter.BindingModel.SelectedItems, e.AddedItems, e.RemovedItems);
        }
    }
}
