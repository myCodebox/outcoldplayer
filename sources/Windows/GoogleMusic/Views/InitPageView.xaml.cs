// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.Views;

    public interface IInitPageView : IPageView
    {
    }

    public sealed partial class InitPageView : PageViewBase, IInitPageView
    {
        public InitPageView()
        {
            this.InitializeComponent();
        }
    }
}
