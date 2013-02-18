// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    public interface IAlbumPageView : IDataPageView
    {
    }

    public sealed partial class AlbumPageView : DataPageViewBase, IAlbumPageView
    {
        public AlbumPageView()
        {
            this.InitializeComponent();
        }
    }
}
