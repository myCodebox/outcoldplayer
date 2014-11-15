// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Threading.Tasks;

    using Windows.UI.Xaml.Controls;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface IAlbumPageView : IPlaylistPageViewBase
    {
    }

    public interface IArtistPageView : IPageView
    {
    }

    public interface ICurrentPlaylistPageView : IPageView
    {
        ISongsListView GetSongsListView();
    }

    public interface IExplorePageView : IPageView
    {
    }

    public interface IMainMenu : IView
    {
    }

    public interface IPlayerView : IView
    {
    }

    public interface IPlaylistPageViewBase : IPageView
    {
        ISongsListView GetSongsListView();
    }

    public interface IPlaylistPageView : IPlaylistPageViewBase
    {
    }

    public interface IRadioStationPageView : IPlaylistPageViewBase
    {
    }

    public interface IPlaylistsListView : IView
    {
        ListView GetListView();

        Task ScrollIntoCurrentSongAsync(IPlaylist song);
    }

    public interface IUserPlaylistsPageView : IPageView
    {
    }

    public interface IPlaylistsPageView : IPageView
    {
    }

    public interface IRadioPageView : IPageView
    {
    }

    public interface IGenrePageView : IPageView
    {
    }

    public interface IHomePageView : IPageView
    {
    }

    public interface ISearchPageView : IPageView
    {
    }

    public interface ISituationStationsPageView : IPageView
    {
    }

    public interface ISituationsPageView : IPageView
    {
    }

    public interface ISongsListView : IView
    {
        ListView GetListView();

        Task ScrollIntoCurrentSongAsync(Song song);
    }

    public interface IApplicationSettingFrame : IPopupView
    {
        void SetContent(string title, object content);
    }
}
