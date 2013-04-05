// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using OutcoldSolutions.BindingModels;

    public class StartViewBindingModel : BindingModelBase
    {
        private readonly ObservableCollection<PlaylistBindingModel> selectedItems;
        private List<PlaylistsGroupBindingModel> groups;

        public StartViewBindingModel()
        {
            this.selectedItems = new ObservableCollection<PlaylistBindingModel>();
        }

        public List<PlaylistsGroupBindingModel> Groups
        {
            get
            {
                return this.groups;
            }

            set
            {
                this.SetValue(ref this.groups, value);
            }
        }

        public ObservableCollection<PlaylistBindingModel> SelectedItems
        {
            get
            {
                return this.selectedItems;
            }
        }

        public void ClearSelectedItems()
        {
            if (this.selectedItems.Count > 0)
            {
                this.selectedItems.Clear();
            }
        }
    }
}