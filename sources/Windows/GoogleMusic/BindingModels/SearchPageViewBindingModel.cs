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
        private string title;

        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.SetValue(ref this.title, value);
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