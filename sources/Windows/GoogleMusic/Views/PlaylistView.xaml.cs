// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.GoogleMusic.Presenters;

    public interface IPlaylistView : IView
    {
    }

    public sealed partial class PlaylistView : ViewBase, IPlaylistView
    {
        public PlaylistView()
        {
            this.InitializePresenter<PlaylistViewPresenter>();
            this.InitializeComponent();
        }
    }
}
