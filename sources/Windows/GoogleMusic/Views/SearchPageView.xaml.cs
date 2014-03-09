// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Controls;
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml.Controls;

    public interface ISearchPageView : IPageView
    {
        void UpdateListViewItems(bool scrollToZero);
    }

    public sealed partial class SearchPageView : PageViewBase, ISearchPageView
    {
        private const string SelectedIndex = "Groups_SelectedIndex";

        public SearchPageView()
        {
            this.InitializeComponent();
            this.TrackScrollViewer(this.ListView);
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
            else if (this.Groups.Items != null && this.Groups.Items.Count > 0)
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

        public void UpdateListViewItems(bool scrollToZero)
        {
            if (this.Groups.Items != null)
            {
                if (this.Groups.SelectedValue == null && this.Groups.Items.Count > 0)
                {
                    this.Groups.SelectedIndex = 0;
                    return;
                }

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

        private void ListViewOnItemClick(object sender, ItemClickEventArgs e)
        {
            this.GetPresenter<SearchPageViewPresenter>().NavigateToView(e.ClickedItem);
        }

        private void GroupsOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateListViewItems(scrollToZero: true);
        }
    }
}
