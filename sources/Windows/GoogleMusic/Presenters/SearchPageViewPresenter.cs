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

        public SearchPageViewPresenter(
            IApplicationResources resources,
            ISongsRepository songsRepository,
            IPlaylistsService playlistsService)
        {
            this.resources = resources;
            this.songsRepository = songsRepository;
            this.playlistsService = playlistsService;
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