// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Diagnostics;

    using OutcoldSolutions.GoogleMusic.Controls;

    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media.Animation;

    public class DataPageViewBase : PageViewBase, IDataPageView
    {
        private const string HorizontalScrollOffset = "ListView_HorizontalScrollOffset";
        private const string VerticalScrollOffset = "ListView_VerticalScrollOffset";

        private ListViewBase trackingListView;
        private Storyboard trackingListStoryboard;

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            if (this.trackingListView != null)
            {
                eventArgs.State[HorizontalScrollOffset] = 
                    this.trackingListView.GetScrollViewerHorizontalOffset();
                eventArgs.State[VerticalScrollOffset] =
                    this.trackingListView.GetScrollViewerVerticalOffset();

                this.trackingListView.Opacity = 0;
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

                this.trackingListStoryboard.Begin();
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
            if (this.trackingListView.Transitions != null)
            {
                this.trackingListView.Transitions.Clear();
            }

            this.trackingListView.Opacity = 0;

            this.trackingListStoryboard = new Storyboard();
            DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTarget(doubleAnimationUsingKeyFrames, this.trackingListView);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames, "Opacity");
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new LinearDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0)), Value = 0 });
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new LinearDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100)), Value = 0 });
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new LinearDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300)), Value = 1 });
            this.trackingListStoryboard.Children.Add(doubleAnimationUsingKeyFrames);
            this.Resources.Add("TrackingListStoryboard", this.trackingListStoryboard);
        }
    }
}