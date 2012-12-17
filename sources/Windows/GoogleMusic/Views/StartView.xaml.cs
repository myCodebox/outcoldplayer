//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.GoogleMusic.Presenters;

    public interface IStartView : IView
    {
    }

    public sealed partial class StartView : ViewBase, IStartView
    {
        public StartView()
        {
            this.InitializePresenter<StartViewPresenter>();
            this.InitializeComponent();
        }
    }
}
