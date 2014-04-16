// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;

    public class PlaylistBindingModel : BindingModelBase
    {
        private static readonly Lazy<IPlaylistsService> PlaylistsService = new Lazy<IPlaylistsService>(() => ApplicationBase.Container.Resolve<IPlaylistsService>());
        private static readonly Lazy<ILogger> Logger = new Lazy<ILogger>(() => ApplicationBase.Container.Resolve<ILogManager>().CreateLogger("PlaylistBindingModel"));
        private static readonly Lazy<IDispatcher> Dispatcher = new Lazy<IDispatcher>(() => ApplicationBase.Container.Resolve<IDispatcher>()); 

        private readonly IPlaylist playlist;

        public PlaylistBindingModel(IPlaylist playlist)
        {
            if (playlist == null)
            {
                throw new ArgumentNullException("playlist");
            }

            this.playlist = playlist;
        }

        public DelegateCommand PlayCommand { get; set; }

        public IPlaylist Playlist
        {
            get
            {
                return this.playlist;
            }
        }

        public Uri[] ArtUris
        {
            get
            {
                var mixedPlaylist = this.Playlist as IMixedPlaylist;
                if (mixedPlaylist != null)
                {
                    if (mixedPlaylist.ArtUrls != null)
                    {
                        return mixedPlaylist.ArtUrls;
                    }
                    else if (mixedPlaylist.PlaylistType != PlaylistType.AllAccessGenre)
                    {
                        LoadArtUris(mixedPlaylist);
                    }
                }

                return null;
            }
        }

        public bool IsMixedList { get; set; }

        private async void LoadArtUris(IMixedPlaylist mixedPlaylist)
        {
            try
            {
                await PlaylistsService.Value.GetArtUrisAsync(mixedPlaylist);
                await Dispatcher.Value.RunAsync(() => this.RaisePropertyChanged(() => this.ArtUris));
            }
            catch (Exception e)
            {
                Logger.Value.Debug(e, "Could not load art uris");
            }
        }
    }
}