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
        private const int MaxResults = 4;

        private const string Artists = "Artists";
        private const string Albums = "Albums";
        private const string Genres = "Genres";
        private const string Playlists = "Playlists";
        private const string Songs = "Songs";

        private readonly ISongsService songsService;
        private readonly INavigationService navigationService;
        private readonly IDispatcher dispatcher;


        public SearchService(ISongsService songsService, INavigationService navigationService, IDispatcher dispatcher)
        {
            this.songsService = songsService;
            this.navigationService = navigationService;
            this.dispatcher = dispatcher;

            var searchPane = SearchPane.GetForCurrentView();
            searchPane.SuggestionsRequested += this.OnSuggestionsRequested;
            searchPane.ResultSuggestionChosen += this.SearchPaneOnResultSuggestionChosen;
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
                                playlists = await this.songsService.GetAllArtistsAsync();
                                break;
                            }

                        case Albums:
                            {
                                playlists = await this.songsService.GetAllAlbumsAsync();
                                break; 
                            }

                        case Genres:
                            {
                                playlists = await this.songsService.GetAllGenresAsync();
                                break;
                            }

                        case Playlists:
                            {
                                playlists = await this.songsService.GetAllPlaylistsAsync();
                                break;
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
            var artistsSearch = this.SearchPlaylists(
                               await this.songsService.GetAllArtistsAsync(Order.Name),
                               args.QueryText,
                               Math.Max(MaxResults, 0))
                               .ToList();

            if (artistsSearch.Count > 0)
            {
                this.AddResults(args, artistsSearch, Artists);

                if (artistsSearch.Count > MaxResults)
                {
                    return;
                }
            }

            var albumsSearch = this.SearchPlaylists(
                               await this.songsService.GetAllAlbumsAsync(Order.Name),
                               args.QueryText,
                               Math.Max(MaxResults - artistsSearch.Count, 0))
                               .ToList();

            if (albumsSearch.Count > 0)
            {
                this.AddResults(args, albumsSearch, Albums);

                if (artistsSearch.Count + albumsSearch.Count > MaxResults)
                {
                    return;
                }
            }

            var genresSearch = this.SearchPlaylists(
                                await this.songsService.GetAllGenresAsync(Order.Name),
                                args.QueryText,
                                Math.Max(MaxResults - artistsSearch.Count - albumsSearch.Count, 0))
                                .ToList();

            if (genresSearch.Count > 0)
            {
                this.AddResults(args, genresSearch, Genres);
                
                if (genresSearch.Count + artistsSearch.Count + albumsSearch.Count > MaxResults)
                {
                    return;
                }
            }

            //var songs = await this.songsService.GetAllGoogleSongsAsync();
            //var songsSearch = songs.Where( 
            //    x =>
            //        {
            //            if (x.Title == null)
            //            {
            //                return false;
            //            }

            //            var found = x.Title.IndexOf(args.QueryText.ToUpper(), StringComparison.CurrentCultureIgnoreCase);
            //            return (found == 0) || (found > 0 && char.IsSeparator(x.Title[found - 1]));
            //        }).Take(Math.Max(MaxResults - artistsSearch.Count - albumsSearch.Count, 0)).ToList();

            //if (songsSearch.Count > 0)
            //{
            //    args.Request.SearchSuggestionCollection.AppendSearchSeparator(Songs);

            //    foreach (var song in songsSearch)
            //    {
            //        IRandomAccessStreamReference randomAccessStreamReference = null;
            //        if (!string.IsNullOrEmpty(song.GoogleMusicMetadata.AlbumArtUrl))
            //        {
            //            randomAccessStreamReference =
            //                RandomAccessStreamReference.CreateFromUri(new Uri("https:" + song.GoogleMusicMetadata.AlbumArtUrl));
            //        }
            //        else
            //        {
            //            randomAccessStreamReference =
            //                RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/SmallLogo.png"));
            //        }

            //        args.Request.SearchSuggestionCollection.AppendResultSuggestion(
            //            song.Title,
            //            song.Artist,
            //            string.Format(CultureInfo.CurrentCulture, "{0}:{1} - {2}", Songs, song.Artist, song.Title),
            //            randomAccessStreamReference,
            //            "gMusic");
            //    }
            //}

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
                        RandomAccessStreamReference.CreateFromUri(new Uri("https:" + playlist.AlbumArtUrl));
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
            return playlists.Where(
                x =>
                    {
                        if (x.Title == null)
                        {
                            return false;
                        }

                        var found = x.Title.IndexOf(search.ToUpper(), StringComparison.CurrentCultureIgnoreCase);
                        return (found == 0) || (found > 0 && char.IsSeparator(x.Title[found - 1]));
                    }).Take(take);
        }
    }
}