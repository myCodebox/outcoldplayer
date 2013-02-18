// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.GoogleMusic.Controls;

    using Windows.UI.Xaml.Controls;

    public class DataPageViewBase : PageViewBase, IDataPageView
    {
        private const string HorizontalScrollOffset = "ListView_HorizontalScrollOffset";
        private ListViewBase trackingListView;

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            if (this.trackingListView != null)
            {
                eventArgs.State[HorizontalScrollOffset] = 
                    this.trackingListView.GetScrollViewerHorizontalOffset();
            }
        }

        public virtual void OnDataLoading(NavigatedToEventArgs eventArgs)
        {
            if (this.trackingListView != null)
            {
                this.trackingListView.ScrollToHorizontalZero();
            }
        }

        public virtual void OnUnfreeze(NavigatedToEventArgs eventArgs)
        {
        }

        public virtual void OnDataLoaded(NavigatedToEventArgs eventArgs)
        {
            if (this.trackingListView != null)
            {
                if (eventArgs.IsNavigationBack)
                {
                    object offset;
                    if (eventArgs.State.TryGetValue(HorizontalScrollOffset, out offset))
                    {
                        this.trackingListView.ScrollToHorizontalOffset((double)offset);
                    }
                }
            }
        }

        protected void TrackListViewBase(ListViewBase listViewBase)
        {
            this.trackingListView = listViewBase;
        }
    }
}