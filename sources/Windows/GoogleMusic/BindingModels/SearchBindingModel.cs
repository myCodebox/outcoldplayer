// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.ObjectModel;

    using OutcoldSolutions.GoogleMusic.Presentation;

    public class SearchBindingModel : BindingModelBase
    {
        private readonly ObservableCollection<SearchGroupBindingModel> groups;

        private bool isLoading;

        private string query;

        public SearchBindingModel()
        {
            this.groups = new ObservableCollection<SearchGroupBindingModel>();
        }

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

        public ObservableCollection<SearchGroupBindingModel> Groups
        {
            get
            {
                return this.groups;
            }
        }
    }
}