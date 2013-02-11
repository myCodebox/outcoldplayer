// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml.Controls;

    public interface ISearchView : IPageView
    {
        int SelectedFilterIndex { set; }
    }

    public sealed partial class SearchPageView : PageViewBase, ISearchView
    {
        public SearchPageView()
        {
            this.InitializePresenter<SearchViewPresenter>();
            this.InitializeComponent();
        }

        public int SelectedFilterIndex
        {
            set
            {
                this.Groups.SelectedIndex = value;
            }
        }

        private void ListViewOnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is SongResultBindingModel)
            {
                App.Container.Resolve<INavigationService>()
                   .NavigateTo<IPlaylistView>(((SongResultBindingModel)e.ClickedItem).Result);
            }
            else if (e.ClickedItem is PlaylistResultBindingModel)
            {
                App.Container.Resolve<INavigationService>()
                    .NavigateTo<IPlaylistView>(((PlaylistResultBindingModel)e.ClickedItem).Result);
            }
        }

        private void GroupsOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ListView.Items != null && this.ListView.Items.Count > 0)
            {
                this.ListView.ScrollIntoView(this.ListView.Items[0]);
            }
        }
    }
}
