// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.Views;

    public interface IAuthentificationPageView : IPageView
    {
    }

    public sealed partial class AuthentificationPageView : PageViewBase, IAuthentificationPageView
    {
        public AuthentificationPageView()
        {
            this.InitializeComponent();
        }
    }
}
