// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Diagnostics;

    using OutcoldSolutions.GoogleMusic.Controls;

    using Windows.UI.Xaml.Controls;

    public class DataPageViewBase : PageViewBase, IDataPageView
    {
        private const string HorizontalScrollOffset = "ListView_HorizontalScrollOffset";
        private const string VerticalScrollOffset = "ListView_VerticalScrollOffset";

        private ListViewBase trackingListView;

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            if (this.trackingListView != null)
            {
                eventArgs.State[HorizontalScrollOffset] = 
                    this.trackingListView.GetScrollViewerHorizontalOffset();
                eventArgs.State[VerticalScrollOffset] =
                    this.trackingListView.GetScrollViewerVerticalOffset();
            }
        }

        public virtual void OnDataLoading(NavigatedToEventArgs eventArgs)
        {
            if (this.trackingListView != null)
            {
                this.trackingListView.ScrollToHorizontalZero();
                this.trackingListView.ScrollToVerticalZero();
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

                    if (eventArgs.State.TryGetValue(VerticalScrollOffset, out offset))
                    {
                        this.trackingListView.ScrollToVerticalOffset((double)offset);
                    }
                }
            }
        }

        protected void TrackListViewBase(ListViewBase listViewBase)
        {
            if (listViewBase == null)
            {
                throw new ArgumentNullException("listViewBase");
            }

            Debug.Assert(this.trackingListView == null, "this.trackingListView == null. Only one list view tracking supported.");
            this.trackingListView = listViewBase;
        }
    }
}