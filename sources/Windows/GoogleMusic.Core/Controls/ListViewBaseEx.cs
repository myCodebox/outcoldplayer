// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Controls
{
    using System;
    using System.Diagnostics;

    using Windows.UI.Xaml.Controls;

    public static class ListViewBaseEx
    {
        public static void ScrollToHorizontalOffset(this ListViewBase listViewBase, double horizontalOffset)
        {
            if (listViewBase == null)
            {
                throw new ArgumentNullException("listViewBase");
            }

            var scrollViewer = VisualTreeHelperEx.GetVisualChild<ScrollViewer>(listViewBase);
            Debug.Assert(scrollViewer != null, "scrollViewer != null");

            if (scrollViewer != null)
            {
                scrollViewer.ScrollToHorizontalOffset(horizontalOffset);
            }
        }

        public static void ScrollToHorizontalZero(this ListViewBase listViewBase)
        {
            ScrollToHorizontalOffset(listViewBase, 0.0d);
        }

        public static double GetScrollViewerHorizontalOffset(this ListViewBase listViewBase)
        {
            if (listViewBase == null)
            {
                throw new ArgumentNullException("listViewBase");
            }

            var scrollViewer = VisualTreeHelperEx.GetVisualChild<ScrollViewer>(listViewBase);
            
            Debug.Assert(scrollViewer != null, "scrollViewer != null");
            if (scrollViewer != null)
            {
                return scrollViewer.HorizontalOffset;
            }

            return 0.0d;
        }
    }
}