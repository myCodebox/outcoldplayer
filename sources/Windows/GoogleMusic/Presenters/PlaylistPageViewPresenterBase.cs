// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.InversionOfControl;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class PlaylistPageViewPresenterBase<TView, TPlaylist> 
        : PagePresenterBase<TView, PlaylistPageViewBindingModel<TPlaylist>>
        where TPlaylist : class, IPlaylist 
        where TView : IPageView
    {
        private readonly IPlaylistsService playlistsService;
        private readonly IApplicationResources resources;

        public PlaylistPageViewPresenterBase(IDependencyResolverContainer container)
        {
            this.playlistsService = container.Resolve<IPlaylistsService>();
            this.resources = container.Resolve<IApplicationResources>();
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            this.BindingModel.Songs = null;
            this.BindingModel.Playlist = default(TPlaylist);
            this.BindingModel.Type = null;
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var request = navigatedToEventArgs.Parameter as PlaylistNavigationRequest;
            if (request == null)
            {
                throw new NotSupportedException("Request parameter should be PlaylistNavigationRequest.");
            }

            var songs = await this.playlistsService.GetSongsAsync(request.PlaylistType, request.PlaylistId);
            var playlist = await this.playlistsService.GetAsync(request.PlaylistType, request.PlaylistId);

            await this.Dispatcher.RunAsync(
                () =>
                    {
                        this.BindingModel.Songs = songs;
                        this.BindingModel.Playlist = (TPlaylist)playlist;
                        this.BindingModel.Type = this.resources.GetTitle(playlist.PlaylistType);

                        if (!string.IsNullOrEmpty(request.SongId))
                        {
                            this.EventAggregator.Publish(new SelectSongByIdEvent(request.SongId));
                        }
                    });
        }
    }
}