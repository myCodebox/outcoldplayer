// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    using Windows.ApplicationModel.Search;
    using Windows.Storage.Streams;

    public class SearchService : ISearchService
    {
        private const int MaxResults = 5;

        private readonly IApplicationResources resources;
        private readonly INavigationService navigationService;
        private readonly IDispatcher dispatcher;
        private readonly IPlaylistsService playlistsService;
        private readonly ISongsRepository songsRepository;
        private readonly IAlbumArtCacheService albumArtCacheService;

        public SearchService(
            IApplicationResources resources,
            INavigationService navigationService, 
            IDispatcher dispatcher,
            IPlaylistsService playlistsService,
            ISongsRepository songsRepository,
            IAlbumArtCacheService albumArtCacheService)
        {
            this.resources = resources;
            this.navigationService = navigationService;
            this.dispatcher = dispatcher;
            this.playlistsService = playlistsService;
            this.songsRepository = songsRepository;
            this.albumArtCacheService = albumArtCacheService;
        }
        
        public void Activate()
        {
            var searchPane = SearchPane.GetForCurrentView();
            searchPane.Show();
        }

        public void SetShowOnKeyboardInput(bool value)
        {
            var searchPane = SearchPane.GetForCurrentView();
            searchPane.ShowOnKeyboardInput = value;
        }

        public void Register()
        {
            var searchPane = SearchPane.GetForCurrentView();
            searchPane.ShowOnKeyboardInput = true;
            searchPane.SuggestionsRequested += this.OnSuggestionsRequested;
            searchPane.ResultSuggestionChosen += this.SearchPaneOnResultSuggestionChosen;
            searchPane.QuerySubmitted += this.SearchPaneOnQuerySubmitted;
        }

        public void Unregister()
        {
            var searchPane = SearchPane.GetForCurrentView();
            searchPane.ShowOnKeyboardInput = false;
            searchPane.SuggestionsRequested -= this.OnSuggestionsRequested;
            searchPane.ResultSuggestionChosen -= this.SearchPaneOnResultSuggestionChosen;
            searchPane.QuerySubmitted -= this.SearchPaneOnQuerySubmitted;
        }

        private async void SearchPaneOnQuerySubmitted(SearchPane sender, SearchPaneQuerySubmittedEventArgs args)
        {
            await this.dispatcher.RunAsync(() => this.navigationService.NavigateTo<ISearchPageView>(args.QueryText));
        }

        private void OnSuggestionsRequested(SearchPane sender, SearchPaneSuggestionsRequestedEventArgs args)
        {
            var searchPaneSuggestionsRequestDeferral = args.Request.GetDeferral();
            this.SearchAsync(args)
                .ContinueWith(
                async x =>
                {
                    foreach (var item in x.Result)
                    {
                        if (args.Request.IsCanceled)
                        {
                            break;
                        }

                        if (item.ItemType == SearchItemType.Title)
                        {
                            args.Request.SearchSuggestionCollection.AppendSearchSeparator(((SearchTitle)item).Title);
                        }
                        else
                        {
                            var searchResult = (SearchResult)item;

                            IRandomAccessStreamReference randomAccessStreamReference = null;
                            if (searchResult.AlbumArtUrl != null)
                            {
                                var pathToImage = await this.albumArtCacheService.GetCachedImageAsync(searchResult.AlbumArtUrl, size: 116);
                                if (string.IsNullOrEmpty(pathToImage))
                                {
                                    randomAccessStreamReference = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/SmallLogo.png"));
                                }
                                else
                                {
                                    randomAccessStreamReference = RandomAccessStreamReference.CreateFromUri(AlbumArtUrlExtensions.ToLocalUri(pathToImage));
                                }
                            }
                            else
                            {
                                randomAccessStreamReference =
                                    RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/SmallLogo.png"));
                            }

                            args.Request.SearchSuggestionCollection.AppendResultSuggestion(
                                searchResult.Title, searchResult.Details, searchResult.Tag, randomAccessStreamReference, "gMusicW");
                        }
                    }

                    searchPaneSuggestionsRequestDeferral.Complete();
                },
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void SearchPaneOnResultSuggestionChosen(SearchPane sender, SearchPaneResultSuggestionChosenEventArgs args)
        {
            this.NavigateToTagAsync(args.Tag);
        }

        private async void NavigateToTagAsync(string tag)
        {
            var strings = tag.Split(new[] { ':' }, 2);
            if (strings.Length == 2)
            {
                PlaylistType playlistType;
                int playlistId;
                if (Enum.TryParse(strings[0], out playlistType)
                    && int.TryParse(strings[1], out playlistId))
                {
                    await this.dispatcher.RunAsync(() => this.navigationService.NavigateToPlaylist(
                        new PlaylistNavigationRequest(playlistType, playlistId)));
                }
            }
            else
            {
                int songId;
                if (int.TryParse(tag, out songId))
                {
                    await this.dispatcher.RunAsync(() => this.navigationService.NavigateTo<IAlbumPageView>(songId));
                }
            }
        }

        private async Task<List<ISearchItem>> SearchAsync(SearchPaneSuggestionsRequestedEventArgs args)
        {
            var result = new List<ISearchItem>();

            var types = new[] { PlaylistType.Artist, PlaylistType.Album, PlaylistType.Genre };
            foreach (var playlistType in types)
            {
                var playlists = (await this.playlistsService.SearchAsync(playlistType, args.QueryText, (uint?)(MaxResults - result.Count))).ToList();
                if (playlists.Count > 0)
                {
                    this.AddResults(result, playlists, playlistType);
                }

                if (result.Count >= MaxResults)
                {
                    break;
                }
            }
            
            if (result.Count < MaxResults)
            {
                var songs = await this.songsRepository.SearchAsync(args.QueryText, (uint?)(MaxResults - result.Count));

                if (songs.Count > 0)
                {
                    result.Add(new SearchTitle(this.resources.GetString("Model_Song_Plural_Title")));
                    foreach (var song in songs)
                    {
                        result.Add(new SearchResult(
                            song.Title,
                            song.GetSongArtist(),
                            song.SongId.ToString(),
                            song.AlbumArtUrl));
                    }
                }
            }

            if (result.Count < MaxResults)
            {
                var playlists = (await this.playlistsService.SearchAsync(PlaylistType.UserPlaylist, args.QueryText, (uint?)(MaxResults - result.Count))).ToList();
                if (playlists.Count > 0)
                {
                    this.AddResults(result, playlists, PlaylistType.UserPlaylist);
                }
            }

            return result;
        }

        private void AddResults(List<ISearchItem> result, IEnumerable<IPlaylist> playlists, PlaylistType playlistType)
        {
            bool titleAdded = false;

            foreach (var playlist in playlists)
            {
                if (!titleAdded)
                {
                    result.Add(new SearchTitle(this.resources.GetTitle(playlist.PlaylistType)));
                    titleAdded = true;
                }

                result.Add(new SearchResult(
                    playlist.Title,
                    string.Format(CultureInfo.CurrentCulture, this.resources.GetString("SearchItem_SongsFormat"), playlist.SongsCount),
                    string.Format(CultureInfo.CurrentCulture, "{0}:{1}", playlistType, playlist.Id),
                    playlist.ArtUrl));
            }
        }

        private enum SearchItemType
        {
            Title = 1,

            Result = 2
        }

        private interface ISearchItem
        {
            SearchItemType ItemType { get; }
        }

        private class SearchTitle : ISearchItem
        {
            public SearchTitle(string title)
            {
                this.Title = title;
            }

            public string Title { get; private set; }

            public SearchItemType ItemType
            {
                get
                {
                    return SearchItemType.Title;
                }
            }
        }

        private class SearchResult : ISearchItem
        {
            public SearchResult(string title, string details, string tag, Uri albumArtUrl)
            {
                this.Title = title;
                this.Details = details;
                this.Tag = tag;
                this.AlbumArtUrl = albumArtUrl;
            }

            public string Title { get; private set; }

            public string Details { get; private set; }

            public string Tag { get; private set; }

            public Uri AlbumArtUrl { get; private set; }

            public SearchItemType ItemType
            {
                get
                {
                    return SearchItemType.Result;
                }
            }
        }
    }
}