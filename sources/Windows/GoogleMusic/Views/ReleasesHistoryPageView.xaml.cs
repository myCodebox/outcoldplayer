// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.Views;

    public interface IReleasesHistoryPageView : IPageView
    {
    }

    public sealed partial class ReleasesHistoryPageView : PageViewBase, IReleasesHistoryPageView
    {
        public ReleasesHistoryPageView()
        {
            this.InitializeComponent();
        }
    }
}
