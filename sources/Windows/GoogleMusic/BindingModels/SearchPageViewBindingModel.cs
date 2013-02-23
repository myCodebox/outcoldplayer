// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    public class SearchPageViewBindingModel : BindingModelBase
    {
        private List<SearchGroupBindingModel> groups;

        private string query;

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

        public List<SearchGroupBindingModel> Groups
        {
            get
            {
                return this.groups;
            }
            
            set
            {
                this.groups = value;
                this.RaiseCurrentPropertyChanged();
            }
        }
    }
}