// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    public class SearchGroupBindingModel
    {
        public SearchGroupBindingModel(string title, List<SearchResultBindingModel> results)
        {
            this.Title = title;
            this.Results = results;
        }

        public string Title { get; private set; }

        public List<SearchResultBindingModel> Results { get; private set; }
    }
}