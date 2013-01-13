// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class SearchViewPresenter : ViewPresenterBase<ISearchView>
    {
        private readonly ISongsService songsService;

        public SearchViewPresenter(
            IDependencyResolverContainer container,
            ISearchView view,
            ISongsService songsService)
            : base(container, view)
        {
            this.songsService = songsService;
            this.BindingModel = new SearchBindingModel();
        }

        public SearchBindingModel BindingModel { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);

            var query = eventArgs.Parameter as string;

            this.BindingModel.Groups.Clear();
            this.BindingModel.Query = query;
            

            if (!string.IsNullOrEmpty(query))
            {
                this.BindingModel.IsLoading = true;

                this.Search(query).ContinueWith(
                    t =>
                        {
                            this.BindingModel.IsLoading = false; 
                            this.BindingModel.Groups.Add(new SearchGroupBindingModel("All", t.Result.SelectMany(x => x.Results).ToList()));
                            foreach (var searchGroupBindingModel in t.Result)
                            {
                                this.BindingModel.Groups.Add(searchGroupBindingModel);   
                            }

                            this.View.SelectedFilterIndex = 0;
                        },
                    TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                this.BindingModel.Groups.Add(new SearchGroupBindingModel("All", new List<SearchResultBindingModel>()));
                this.View.SelectedFilterIndex = 0;
            }
        }

        public async Task<List<SearchGroupBindingModel>> Search(string query)
        {
            var results = new List<SearchGroupBindingModel>();

            var artists =
                this.SearchPlaylists(await this.songsService.GetAllArtistsAsync(), query)
                    .Select(x => new PlaylistResultBindingModel(query, x))
                    .Cast<SearchResultBindingModel>()
                    .ToList();

            if (artists.Count > 0)
            {
                results.Add(new SearchGroupBindingModel("Artists", artists));
            }

            var albums =
                this.SearchPlaylists(await this.songsService.GetAllAlbumsAsync(), query)
                    .Select(x => new PlaylistResultBindingModel(query, x))
                    .Cast<SearchResultBindingModel>()
                    .ToList();

            if (albums.Count > 0)
            {
                results.Add(new SearchGroupBindingModel("Albums", albums));
            }

            var genres =
                this.SearchPlaylists(await this.songsService.GetAllGenresAsync(), query)
                    .Select(x => new PlaylistResultBindingModel(query, x))
                    .Cast<SearchResultBindingModel>()
                    .ToList();

            if (genres.Count > 0)
            {
                results.Add(new SearchGroupBindingModel("Genres", genres));
            }

            var songs = (await this.songsService.GetAllGoogleSongsAsync()).Where(
                x => Models.Search.Contains(x.Title, query)).Select(x => new SongResultBindingModel(query, x)).Cast<SearchResultBindingModel>().ToList();

            if (songs.Count > 0)
            {
                results.Add(new SearchGroupBindingModel("Songs", songs));
            }

            var playlists =
                this.SearchPlaylists(await this.songsService.GetAllPlaylistsAsync(), query)
                    .Select(x => new PlaylistResultBindingModel(query, x))
                    .Cast<SearchResultBindingModel>()
                    .ToList();

            if (playlists.Count > 0)
            {
                results.Add(new SearchGroupBindingModel("Playlists", playlists));
            }

            return results;
        }

        private IEnumerable<Playlist> SearchPlaylists(IEnumerable<Playlist> playlists, string search)
        {
            return playlists.Where(x => Models.Search.Contains(x.Title, search));
        }
    }
}