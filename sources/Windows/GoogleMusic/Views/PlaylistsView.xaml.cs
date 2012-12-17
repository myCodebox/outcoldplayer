// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.GoogleMusic.Presenters;

    public interface IPlaylistsView : IView
    {
    }

    public sealed partial class PlaylistsView : ViewBase, IPlaylistsView
    {
        public PlaylistsView()
        {
            this.InitializePresenter<PlaylistsViewPresenter>();
            this.InitializeComponent();
        }
    }
}
