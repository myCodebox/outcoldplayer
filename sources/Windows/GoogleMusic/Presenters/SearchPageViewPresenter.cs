// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.Presenters;

    public class SearchPageViewPresenter : PagePresenterBase<ISearchPageView, SearchPageViewBindingModel>
    {
        private readonly IApplicationResources resources;
        private readonly ISongsRepository songsRepository;
        private readonly IPlaylistsService playlistsService;
        private readonly INavigationService navigationService;
        private readonly IUserPlaylistsRepository userPlaylistsRepository;

        public SearchPageViewPresenter(
            IApplicationResources resources,
            ISongsRepository songsRepository,
            IPlaylistsService playlistsService,
            INavigationService navigationService,
            IUserPlaylistsRepository userPlaylistsRepository)
        {
            this.resources = resources;
            this.songsRepository = songsRepository;
            this.playlistsService = playlistsService;
            this.navigationService = navigationService;
            this.userPlaylistsRepository = userPlaylistsRepository;
        }

        public async void NavigateToView(object clickedItem)
        {
            if (clickedItem is SongResultBindingModel)
            {
                var song = ((SongResultBindingModel)clickedItem).Result.Metadata;
                if (song.IsLibrary)
                {
                    this.navigationService.NavigateTo<IAlbumPageView>(song.SongId);
                }
                else
                {
                    var playlist = await this.userPlaylistsRepository.FindUserPlaylistAsync(song);
                    if (playlist != null)
                    {
                        await this.Dispatcher.RunAsync(() => this.navigationService.NavigateTo<IPlaylistPageView>(new PlaylistNavigationRequest(PlaylistType.UserPlaylist, playlist.Id, song.SongId)));
                    }
                }
                
            }
            else if (clickedItem is PlaylistResultBindingModel)
            {
                this.navigationService.NavigateToPlaylist(((PlaylistResultBindingModel)clickedItem).Result);
            }
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var query = navigatedToEventArgs.Parameter as string;

            this.BindingModel.Title = string.Format(CultureInfo.CurrentCulture, this.resources.GetString("SearchPageView_SubtitleFormat"), query);

            var searchGroupBindingModels = string.IsNullOrEmpty(query) ? new List<SearchGroupBindingModel>() : await this.Search(query);
            searchGroupBindingModels.Insert(0, new SearchGroupBindingModel(this.resources.GetString("SearchPageView_AllTitle"), searchGroupBindingModels.SelectMany(x => x.Results).ToList()));
            this.BindingModel.Groups = searchGroupBindingModels;
        }

        private async Task<List<SearchGroupBindingModel>> Search(string query)
        {
            var results = new List<SearchGroupBindingModel>();

            var types = new[] { PlaylistType.Artist, PlaylistType.Album, PlaylistType.Genre, PlaylistType.UserPlaylist };
            bool hasUserPlaylistsResults = false;

            foreach (var playlistType in types)
            {
                var playlists = (await this.playlistsService.SearchAsync(playlistType, query))
                   .Select(x => new PlaylistResultBindingModel(
                       query, 
                       x, 
                       this.resources.GetTitle(playlistType), 
                       string.Format(CultureInfo.CurrentCulture, this.resources.GetString("SearchItem_SongsFormat"), x.SongsCount)))
                   .Cast<SearchResultBindingModel>()
                   .ToList();

                if (playlists.Count > 0)
                {
                    if (playlistType == PlaylistType.UserPlaylist)
                    {
                        hasUserPlaylistsResults = true;
                    }

                    results.Add(new SearchGroupBindingModel(this.resources.GetPluralTitle(playlistType), playlists));
                }
            }

            var songs = (await this.songsRepository.SearchAsync(query))
                        .Select(x => new SongResultBindingModel(query, new SongBindingModel(x), this.resources.GetString("Model_Song_Title")))
                        .Cast<SearchResultBindingModel>()
                        .ToList();

            if (songs.Count > 0)
            {
                var item = new SearchGroupBindingModel(this.resources.GetString("Model_Song_Plural_Title"), songs);

                if (hasUserPlaylistsResults)
                {
                    results.Insert(results.Count - 1, item);
                }
                else
                {
                    results.Add(item);
                }
            }

            return results;
        }
    }
}