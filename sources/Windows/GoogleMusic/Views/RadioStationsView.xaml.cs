// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.Views;

    public interface IRadioStationsView : IPageView
    {
    }

    public sealed partial class RadioStationsView : PageViewBase, IRadioStationsView
    {
        public RadioStationsView()
        {
            this.InitializeComponent();
        }
    }
}
