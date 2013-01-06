// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    public class SearchBindingModel : BindingModelBase
    {
        private bool isLoading;

        private string query;

        private List<SearchResultBindingModel> results;

        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }

            set
            {
                this.isLoading = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public string Query
        {
            get
            {
                return this.query;
            }

            set
            {
                this.query = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public List<SearchResultBindingModel> Results
        {
            get
            {
                return this.results;
            }

            set
            {
                this.results = value;
                this.RaiseCurrentPropertyChanged();
            }
        }
    }
}