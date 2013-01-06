// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml.Controls;

    public interface ISearchView : IView
    {
        int SelectedFilterIndex { set; }
    }

    public sealed partial class SearchView : ViewBase, ISearchView
    {
        public SearchView()
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
    }
}
