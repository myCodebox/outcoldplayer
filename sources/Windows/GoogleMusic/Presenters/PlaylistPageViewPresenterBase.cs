﻿// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.InversionOfControl;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class PlaylistPageViewPresenterBase<TView> 
        : PagePresenterBase<TView, PlaylistPageViewBindingModel>
        where TView : IPlaylistPageViewBase
    {
        private readonly IPlaylistsService playlistsService;
        private readonly IApplicationResources resources;
        private readonly IPlayQueueService playQueueService;

        private NavigatedToEventArgs latestRequest = null;

        public PlaylistPageViewPresenterBase(IDependencyResolverContainer container)
        {
            this.playlistsService = container.Resolve<IPlaylistsService>();
            this.resources = container.Resolve<IApplicationResources>();
            this.playQueueService = container.Resolve<IPlayQueueService>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.EventAggregator
                .GetEvent<PlaylistsChangeEvent>()
                .Where(e => this.latestRequest != null &&
                    this.BindingModel.Playlist != null && 
                    e.PlaylistType == this.BindingModel.Playlist.PlaylistType &&
                    e.UpdatedPlaylists != null &&
                    e.UpdatedPlaylists.Any(x => string.Equals(x.Id, this.BindingModel.Playlist.Id, StringComparison.OrdinalIgnoreCase)))
                .Subscribe(async(e) => await this.LoadDataAsync(this.latestRequest));
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            this.BindingModel.Songs = null;
            this.BindingModel.Playlist = null;
            this.BindingModel.Title = null;
            this.BindingModel.Subtitle = null;
            this.latestRequest = null;
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var request = navigatedToEventArgs.Parameter as PlaylistNavigationRequest;
            if (request == null)
            {
                throw new NotSupportedException("Request parameter should be PlaylistNavigationRequest.");
            }

            if (request.Songs != null)
            {
                await this.Dispatcher.RunAsync(
                    () =>
                    {
                        this.BindingModel.Songs = request.Songs;
                        this.BindingModel.Playlist = request.Playlist;
                        this.BindingModel.Title = request.Title;
                        this.BindingModel.Subtitle = request.Subtitle;
                    });
            }
            else
            {
                IPlaylist playlist = null;
                IList<Song> songs = null;

                bool isRadio = request.PlaylistType == PlaylistType.Radio;
                bool startPlaying = false;
                bool isCurrentPlaylist = false;
                
                IPlaylist currentPlaylist = this.playQueueService.CurrentPlaylist;
                if (currentPlaylist != null && string.Equals(currentPlaylist.Id, request.PlaylistId))
                {
                    playlist = currentPlaylist;
                    songs = this.playQueueService.GetQueue().ToList();
                    isCurrentPlaylist = true;
                }
                else if (isRadio)
                {
                    await this.playQueueService.StopAsync();
                    startPlaying = true;
                }

                if (playlist == null && songs == null)
                {
                    playlist = await this.playlistsService.GetAsync(request.PlaylistType, request.PlaylistId);
                    songs = await this.playlistsService.GetSongsAsync(request.PlaylistType, request.PlaylistId);
                }

                await this.Dispatcher.RunAsync(
                    () =>
                    {
                        this.BindingModel.Songs = songs;
                        this.BindingModel.Playlist = playlist;
                        if (this.BindingModel.Playlist != null)
                        {
                            this.BindingModel.Title = this.BindingModel.Playlist.Title;
                            this.BindingModel.Subtitle = this.resources.GetTitle(playlist.PlaylistType);
                        }

                        if (isCurrentPlaylist)
                        {
                            this.View.GetSongsListView()
                                .ScrollIntoCurrentSongAsync(this.playQueueService.GetCurrentSong());
                        }
                    });

                if (startPlaying)
                {
                    this.Logger.LogTask(this.playQueueService.PlayAsync(playlist, songs, 0));
                }
            }

            this.latestRequest = navigatedToEventArgs;

            if (!string.IsNullOrEmpty(request.SongId))
            {
                await this.Dispatcher.RunAsync(
                    () => this.EventAggregator.Publish(new SelectSongByIdEvent(request.SongId)));
            }
        }
    }
}