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

    public class SearchPageViewPresenter : DataPagePresenterBase<ISearchPageView, SearchPageViewBindingModel>
    {
        private readonly ISongsRepository songsRepository;
        private readonly IPlaylistCollectionsService collectionsService;

        public SearchPageViewPresenter(
            IDependencyResolverContainer container,
            ISongsRepository songsRepository,
            IPlaylistCollectionsService collectionsService)
            : base(container)
        {
            this.songsRepository = songsRepository;
            this.collectionsService = collectionsService;
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var query = navigatedToEventArgs.Parameter as string;

            this.BindingModel.Query = query;

            var searchGroupBindingModels = string.IsNullOrEmpty(query) ? new List<SearchGroupBindingModel>() : await this.Search(query);
            searchGroupBindingModels.Insert(0, new SearchGroupBindingModel("All", searchGroupBindingModels.SelectMany(x => x.Results).ToList()));
            this.BindingModel.Groups = searchGroupBindingModels;
        }

        public async Task<List<SearchGroupBindingModel>> Search(string query)
        {
            var results = new List<SearchGroupBindingModel>();

            var artists = (await this.collectionsService.GetCollection<Artist>().SearchAsync(query))
                    .Select(x => new PlaylistResultBindingModel(query, x))
                    .Cast<SearchResultBindingModel>()
                    .ToList();

            if (artists.Count > 0)
            {
                results.Add(new SearchGroupBindingModel("Artists", artists));
            }

            var albums = (await this.collectionsService.GetCollection<Album>().SearchAsync(query))
                    .Select(x => new PlaylistResultBindingModel(query, x))
                    .Cast<SearchResultBindingModel>()
                    .ToList();

            if (albums.Count > 0)
            {
                results.Add(new SearchGroupBindingModel("Albums", albums));
            }

            var genres = (await this.collectionsService.GetCollection<Genre>().SearchAsync(query))
                    .Select(x => new PlaylistResultBindingModel(query, x))
                    .Cast<SearchResultBindingModel>()
                    .ToList();

            if (genres.Count > 0)
            {
                results.Add(new SearchGroupBindingModel("Genres", genres));
            }

            var songs = this.songsRepository.GetAll().Where(
                x => Models.Search.Contains(x.Title, query)).Select(x => new SongResultBindingModel(query, x)).Cast<SearchResultBindingModel>().ToList();

            if (songs.Count > 0)
            {
                results.Add(new SearchGroupBindingModel("Songs", songs));
            }

            var playlists =
                (await this.collectionsService.GetCollection<MusicPlaylist>().SearchAsync(query))
                    .Select(x => new PlaylistResultBindingModel(query, x))
                    .Cast<SearchResultBindingModel>()
                    .ToList();

            if (playlists.Count > 0)
            {
                results.Add(new SearchGroupBindingModel("Playlists", playlists));
            }

            return results;
        }
    }
}