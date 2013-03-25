// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Collections.Generic;
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
        private readonly ISongsRepository songsRepository;
        private readonly IPlaylistsService playlistsService;

        public SearchPageViewPresenter(
            ISongsRepository songsRepository,
            IPlaylistsService playlistsService)
        {
            this.songsRepository = songsRepository;
            this.playlistsService = playlistsService;
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var query = navigatedToEventArgs.Parameter as string;

            this.BindingModel.Query = query;

            var searchGroupBindingModels = string.IsNullOrEmpty(query) ? new List<SearchGroupBindingModel>() : await this.Search(query);
            searchGroupBindingModels.Insert(0, new SearchGroupBindingModel("All", searchGroupBindingModels.SelectMany(x => x.Results).ToList()));
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
                   .Select(x => new PlaylistResultBindingModel(query, x))
                   .Cast<SearchResultBindingModel>()
                   .ToList();

                if (playlists.Count > 0)
                {
                    if (playlistType == PlaylistType.UserPlaylist)
                    {
                        hasUserPlaylistsResults = true;
                    }

                    results.Add(new SearchGroupBindingModel(playlistType.ToTitle(), playlists));
                }
            }

            var songs = (await this.songsRepository.SearchAsync(query))
                        .Select(x => new SongResultBindingModel(query, new SongBindingModel(x)))
                        .Cast<SearchResultBindingModel>()
                        .ToList();

            if (songs.Count > 0)
            {
                var item = new SearchGroupBindingModel("Songs", songs);

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