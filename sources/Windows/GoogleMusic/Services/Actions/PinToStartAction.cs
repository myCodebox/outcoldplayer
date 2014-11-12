// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Foundation;
    using Windows.UI.Popups;
    using Windows.UI.StartScreen;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Views;

    public class PinToStartAction : ISelectedObjectAction
    {
        private readonly ILogger logger;
        private readonly IMainFrame mainFrame;
        private readonly IAlbumArtCacheService albumArtCacheService;

        public PinToStartAction(
            IMainFrame mainFrame,
            IAlbumArtCacheService albumArtCacheService,
            ILogManager logManager)
        {
            this.mainFrame = mainFrame;
            this.albumArtCacheService = albumArtCacheService;
            this.logger = logManager.CreateLogger("PinToStartAction");
        }

        public string Icon
        {
            get
            {
                return CommandIcon.Pin;
            }
        }

        public string Title
        {
            get
            {
                return "Pin on start screen";
            }
        }

        public ActionGroup Group
        {
            get
            {
                return ActionGroup.Navigation;
            }
        }

        public int Priority
        {
            get
            {
                return 5;
            }
        }

        public bool CanExecute(IList<object> selectedObjects)
        {
            foreach (var selectedObject in selectedObjects)
            {
                var playlist = selectedObject as IPlaylist;
                if (playlist == null)
                {
                    return false;
                }

                if ((playlist.PlaylistType != PlaylistType.Album
                     && playlist.PlaylistType != PlaylistType.Artist
                     && playlist.PlaylistType != PlaylistType.Genre
                     && playlist.PlaylistType != PlaylistType.Radio
                     && playlist.PlaylistType != PlaylistType.SystemPlaylist
                     && playlist.PlaylistType != PlaylistType.UserPlaylist)
                    || string.IsNullOrEmpty(playlist.Id))
                {
                    return false;
                }
            }

            return selectedObjects.Count > 0;
        }

        public async Task<bool?> Execute(IList<object> selectedObjects)
        {
            IPlaylist playlist = selectedObjects.First() as IPlaylist;
            if (playlist != null)
            {
                Uri uri = await GetAlbumArtUri(playlist.ArtUrl);

                var secondaryTile = new SecondaryTile(
                    (playlist.PlaylistType + "_" + playlist.Id).Replace('-', '_'),
                    playlist.Title,
                    playlist.PlaylistType + "_" + playlist.Id,
                    uri,
                    TileSize.Square150x150);

                secondaryTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                secondaryTile.VisualElements.ShowNameOnSquare310x310Logo = true;
                secondaryTile.VisualElements.ShowNameOnWide310x150Logo = true;
                secondaryTile.VisualElements.Square30x30Logo = uri;
                secondaryTile.VisualElements.Square70x70Logo = uri;
                secondaryTile.VisualElements.Square150x150Logo = uri;
                secondaryTile.VisualElements.Wide310x150Logo = uri;

                bool isPinned = await secondaryTile.RequestCreateForSelectionAsync((Rect)this.mainFrame.GetRectForSecondaryTileRequest(), Placement.Above);

                return isPinned ? (bool?)true : null;
            }

            return null;
        }

        private async Task<Uri> GetAlbumArtUri(Uri albumArt)
        {
            Uri albumArtUri = new Uri("ms-appx:///Resources/UnknownArt-160.png");

            try
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("Album album art: {0}.", albumArt);
                }

                if (albumArt != null)
                {
                    string cachedFile = await this.albumArtCacheService.GetCachedImageAsync(albumArt, size: 160);
                    if (!string.IsNullOrEmpty(cachedFile))
                    {
                        albumArtUri = AlbumArtUrlExtensions.ToLocalUri(cachedFile);
                    }
                }
            }
            catch (OperationCanceledException exception)
            {
                this.logger.Debug(exception, "GetAlbumArtUri:: operation canceled.");
                throw;
            }
            catch (Exception exception)
            {
                this.logger.Error(exception, "Cannot download album art.");
            }

            return albumArtUri;
        }
    }
}
