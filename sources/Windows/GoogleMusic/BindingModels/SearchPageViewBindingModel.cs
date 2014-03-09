// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    public class SearchPageViewBindingModel : BindingModelBase
    {
        private List<SearchGroupBindingModel> groups;
        private string searchText;

        public string SearchText
        {
            get
            {
                return this.searchText;
            }

            set
            {
                this.SetValue(ref this.searchText, value);
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