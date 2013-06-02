// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.Controls;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml.Controls;

    public interface ISearchPageView : IPageView
    {
    }

    public sealed partial class SearchPageView : PageViewBase, ISearchPageView
    {
        private const string SelectedIndex = "Groups_SelectedIndex";

        public SearchPageView()
        {
            this.InitializeComponent();
            this.TrackItemsControl(this.ListView);
        }

        public override void OnDataLoading(NavigatedToEventArgs eventArgs)
        {
            this.Groups.SelectedIndex = -1;

            base.OnDataLoading(eventArgs);
        }

        public override void OnDataLoaded(NavigatedToEventArgs eventArgs)
        {
            object index;
            if (eventArgs.IsNavigationBack && eventArgs.State.TryGetValue(SelectedIndex, out index))
            {
                this.Groups.SelectedIndex = (int)index;
                this.UpdateListViewItems(scrollToZero: false);
            }
            else
            {
                this.Groups.SelectedIndex = 0;
                this.UpdateListViewItems(scrollToZero: true);
            }

            base.OnDataLoaded(eventArgs);

            this.Groups.SelectionChanged -= this.GroupsOnSelectionChanged;
            this.Groups.SelectionChanged += this.GroupsOnSelectionChanged;
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            eventArgs.State[SelectedIndex] = this.Groups.SelectedIndex;

            this.Groups.SelectionChanged -= this.GroupsOnSelectionChanged;
        }

        private void ListViewOnItemClick(object sender, ItemClickEventArgs e)
        {
            this.GetPresenter<SearchPageViewPresenter>().NavigateToView(e.ClickedItem);
        }

        private void GroupsOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateListViewItems(scrollToZero: true);
        }

        private void UpdateListViewItems(bool scrollToZero)
        {
            if (this.Groups.Items != null)
            {
                var searchGroupBindingModel = this.Groups.SelectedValue as SearchGroupBindingModel;
                if (searchGroupBindingModel != null)
                {
                    this.ListView.ItemsSource = searchGroupBindingModel.Results;
                    if (scrollToZero)
                    {
                        this.ListView.ScrollToHorizontalZero();
                    }
                }
            }
        }
    }
}
