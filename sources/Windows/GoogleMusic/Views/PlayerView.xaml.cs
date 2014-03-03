// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    public interface IPlayerView : IView
    {
    }

    public sealed partial class PlayerView : ViewBase, IPlayerView
    {
        public PlayerView()
        {
            this.InitializeComponent();
        }
    }
}
