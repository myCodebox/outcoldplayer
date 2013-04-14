// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    using OutcoldSolutions.BindingModels;

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
                if (this.SetValue(ref this.query, value))
                {
                    this.RaisePropertyChanged(() => this.Title);
                }
            }
        }

        public string Title
        {
            get
            {
                return string.Format("Results for \"{0}\"", this.Query);
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