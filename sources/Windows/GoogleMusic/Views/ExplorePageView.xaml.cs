// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    public interface IExplorePageView : IPageView
    {
        
    }

    public sealed partial class ExplorePageView : PageViewBase, IExplorePageView
    {
        public ExplorePageView()
        {
            this.InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.TrackScrollViewer(this);
        }
    }
}
