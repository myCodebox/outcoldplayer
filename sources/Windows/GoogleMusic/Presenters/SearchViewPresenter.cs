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

        public override void OnNavigatedTo(object parameter)
        {
            base.OnNavigatedTo(parameter);

            var query = parameter as string;

            this.BindingModel.Query = query;
            

            if (!string.IsNullOrEmpty(query))
            {
                this.BindingModel.IsLoading = true;

                this.Search(query).ContinueWith(
                    t =>
                        {
                            this.BindingModel.IsLoading = false; 
                            this.BindingModel.Results = t.Result;
                        },
                    TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        public async Task<List<SearchResultBindingModel>> Search(string query)
        {
            var results = new List<SearchResultBindingModel>();

            results.AddRange(
                this.SearchPlaylists(await this.songsService.GetAllArtistsAsync(), query)
                    .Select(x => new PlaylistResultBindingModel(x)));

            results.AddRange(
                this.SearchPlaylists(await this.songsService.GetAllAlbumsAsync(), query)
                    .Select(x => new PlaylistResultBindingModel(x)));

            results.AddRange(
                this.SearchPlaylists(await this.songsService.GetAllGenresAsync(), query)
                    .Select(x => new PlaylistResultBindingModel(x)));

            results.AddRange(
                (await this.songsService.GetAllGoogleSongsAsync()).Where(
                    x =>
                        {
                            if (x.Title == null)
                            {
                                return false;
                            }

                            var found = x.Title.IndexOf(query.ToUpper(), StringComparison.CurrentCultureIgnoreCase);
                            return (found == 0) || (found > 0 && char.IsSeparator(x.Title[found - 1]));
                        }).Select(x => new SongResultBindingModel(x)));

            results.AddRange(
                this.SearchPlaylists(await this.songsService.GetAllPlaylistsAsync(), query)
                    .Select(x => new PlaylistResultBindingModel(x)));

            return results;
        }

        private IEnumerable<Playlist> SearchPlaylists(IEnumerable<Playlist> playlists, string search)
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
                });
        }
    }
}