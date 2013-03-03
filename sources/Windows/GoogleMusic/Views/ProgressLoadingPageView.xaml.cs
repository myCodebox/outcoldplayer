// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.Views;

    public interface IProgressLoadingView : IPageView
    {
    }

    public sealed partial class ProgressLoadingPageView : PageViewBase, IProgressLoadingView
    {
        public ProgressLoadingPageView()
        {
            this.InitializeComponent();
        }
    }
}
