// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Views;

    using Windows.ApplicationModel.Search;
    using Windows.Storage.Streams;

    public class SearchService : ISearchService
    {
        private const int MaxResults = 5;

        private const string Artists = "Artists";
        private const string Albums = "Albums";
        private const string Genres = "Genres";
        private const string Playlists = "Playlists";
        private const string Songs = "Songs";

        private readonly ISongsService songsService;
        private readonly INavigationService navigationService;
        private readonly IDispatcher dispatcher;

        private readonly IPlaylistCollectionsService playlistCollectionsService;

        public SearchService(
            ISongsService songsService, 
            INavigationService navigationService, 
            IDispatcher dispatcher,
            IPlaylistCollectionsService playlistCollectionsService)
        {
            this.songsService = songsService;
            this.navigationService = navigationService;
            this.dispatcher = dispatcher;
            this.playlistCollectionsService = playlistCollectionsService;
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

        public void SetShowOnKeyboardInput(bool value)
        {
            var searchPane = SearchPane.GetForCurrentView();
            searchPane.ShowOnKeyboardInput = value;
        }

        private async void SearchPaneOnQuerySubmitted(SearchPane sender, SearchPaneQuerySubmittedEventArgs args)
        {
            await this.dispatcher.RunAsync(() => this.navigationService.NavigateTo<ISearchView>(args.QueryText));
        }

        private void OnSuggestionsRequested(SearchPane sender, SearchPaneSuggestionsRequestedEventArgs args)
        {
            var searchPaneSuggestionsRequestDeferral = args.Request.GetDeferral();
            this.SearchAsync(args, searchPaneSuggestionsRequestDeferral)
                .ContinueWith(x => searchPaneSuggestionsRequestDeferral.Complete());
        }

        private void SearchPaneOnResultSuggestionChosen(SearchPane sender, SearchPaneResultSuggestionChosenEventArgs args)
        {
            this.NavigateToTagAsync(args.Tag);
        }

        private async void NavigateToTagAsync(string tag)
        {
            var separator = tag.IndexOf(":", StringComparison.CurrentCulture);
            if (separator > 0)
            {
                var strings = tag.Split(new[] { ':' }, 2);
                if (strings.Length == 2)
                {
                    IEnumerable<Playlist> playlists = null;
                    switch (strings[0])
                    {
                        case Artists:
                            {
                                playlists = await this.playlistCollectionsService.GetCollection<Artist>().SearchAsync(strings[1]);
                                break;
                            }

                        case Albums:
                            {
                                playlists = await this.playlistCollectionsService.GetCollection<Album>().SearchAsync(strings[1]);
                                break; 
                            }

                        case Genres:
                            {
                                playlists = await this.playlistCollectionsService.GetCollection<Genre>().SearchAsync(strings[1]);
                                break;
                            }

                        case Playlists:
                            {
                                playlists = await this.songsService.GetAllPlaylistsAsync();
                                break;
                            }

                        case Songs:
                            {
                                var songs = await this.songsService.GetAllGoogleSongsAsync();
                                var song = songs.Where(
                                    x =>
                                    {
                                        if (x.Title == null)
                                        {
                                            return false;
                                        }

                                        return string.Equals(strings[1], string.Format(CultureInfo.CurrentCulture, "{0} - {1}", x.Artist, x.Title), StringComparison.CurrentCultureIgnoreCase);
                                    }).FirstOrDefault();

                                if (song != null)
                                {
                                    await this.dispatcher.RunAsync(() => this.navigationService.NavigateTo<IPlaylistView>(song));
                                }

                                return;
                            }
                    }

                    if (playlists != null)
                    {
                        var playlist = playlists.FirstOrDefault(x => string.Equals(x.Title, strings[1], StringComparison.CurrentCultureIgnoreCase));
                        if (playlist != null)
                        {
                            await this.dispatcher.RunAsync(() => this.navigationService.NavigateTo<IPlaylistView>(playlist));
                        }
                    }
                }
            }
        }

        private async Task SearchAsync(SearchPaneSuggestionsRequestedEventArgs args, SearchPaneSuggestionsRequestDeferral deferral)
        {
            var artistsSearch = (await this.playlistCollectionsService.GetCollection<Artist>().SearchAsync(args.QueryText, MaxResults)).ToList();

            if (artistsSearch.Count > 0)
            {
                this.AddResults(args, artistsSearch, Artists);

                if (artistsSearch.Count >= MaxResults)
                {
                    return;
                }
            }

            var albumsSearch = (await this.playlistCollectionsService.GetCollection<Album>().SearchAsync(args.QueryText, MaxResults - artistsSearch.Count)).ToList();

            if (albumsSearch.Count > 0)
            {
                this.AddResults(args, albumsSearch, Albums);

                if (artistsSearch.Count + albumsSearch.Count >= MaxResults)
                {
                    return;
                }
            }

            var genresSearch = (await this.playlistCollectionsService.GetCollection<Genre>().SearchAsync(args.QueryText, MaxResults - (artistsSearch.Count + albumsSearch.Count))).ToList();

            if (genresSearch.Count > 0)
            {
                this.AddResults(args, genresSearch, Genres);
                
                if (genresSearch.Count + artistsSearch.Count + albumsSearch.Count >= MaxResults)
                {
                    return;
                }
            }

            var songs = await this.songsService.GetAllGoogleSongsAsync();
            var songsSearch = songs.Where(x => Search.Contains(x.Title, args.QueryText)).Take(Math.Max(MaxResults - (genresSearch.Count + artistsSearch.Count + albumsSearch.Count), 0)).ToList();

            if (songsSearch.Count > 0)
            {
                args.Request.SearchSuggestionCollection.AppendSearchSeparator(Songs);

                foreach (var song in songsSearch)
                {
                    IRandomAccessStreamReference randomAccessStreamReference = null;
                    if (!string.IsNullOrEmpty(song.GoogleMusicMetadata.AlbumArtUrl))
                    {
                        randomAccessStreamReference =
                            RandomAccessStreamReference.CreateFromUri(new Uri("http:" + song.GoogleMusicMetadata.AlbumArtUrl));
                    }
                    else
                    {
                        randomAccessStreamReference =
                            RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/SmallLogo.png"));
                    }

                    args.Request.SearchSuggestionCollection.AppendResultSuggestion(
                        song.Title,
                        song.Artist,
                        string.Format(CultureInfo.CurrentCulture, "{0}:{1} - {2}", Songs, song.Artist, song.Title),
                        randomAccessStreamReference,
                        "gMusic");
                }
            }

            var playlistsSearch = this.SearchPlaylists(
                                await this.songsService.GetAllPlaylistsAsync(Order.Name), 
                                args.QueryText,
                                Math.Max(MaxResults - artistsSearch.Count - albumsSearch.Count - genresSearch.Count /* - songsSearch.Count */, 0))
                                .ToList();

            if (playlistsSearch.Count > 0)
            {
                this.AddResults(args, playlistsSearch, Playlists);
            }

            deferral.Complete();
        }

        private void AddResults(SearchPaneSuggestionsRequestedEventArgs args, IEnumerable<Playlist> playlists, string title)
        {
            bool titleAdded = false;

            foreach (var playlist in playlists)
            {
                if (!titleAdded)
                {
                    args.Request.SearchSuggestionCollection.AppendSearchSeparator(title);
                    titleAdded = true;
                }

                IRandomAccessStreamReference randomAccessStreamReference = null;
                if (!string.IsNullOrEmpty(playlist.AlbumArtUrl))
                {
                    randomAccessStreamReference =
                        RandomAccessStreamReference.CreateFromUri(new Uri("http:" + playlist.AlbumArtUrl));
                }
                else
                {
                    randomAccessStreamReference =
                        RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/SmallLogo.png"));
                }

                args.Request.SearchSuggestionCollection.AppendResultSuggestion(
                    playlist.Title,
                    string.Format(CultureInfo.CurrentCulture, "{0} songs", playlist.Songs.Count),
                    string.Format(CultureInfo.CurrentCulture, "{0}:{1}", title, playlist.Title),
                    randomAccessStreamReference,
                    "gMusic");
            }
        }

        private IEnumerable<Playlist> SearchPlaylists(IEnumerable<Playlist> playlists, string search, int take)
        {
            return playlists.Select(x => Tuple.Create(Search.IndexOf(x.Title, search), x))
                     .Where(x => x.Item1 >= 0)
                     .OrderBy(x => x.Item1)
                     .Take(take)
                     .Select(x => x.Item2);
        }
    }
}