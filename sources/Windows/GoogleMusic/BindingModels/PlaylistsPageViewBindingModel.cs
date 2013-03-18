// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    using OutcoldSolutions.BindingModels;

    public class PlaylistsPageViewBindingModel : BindingModelBase
    {
        private string title;
        private bool isEditable;
        private PlaylistBindingModel selectedItem;

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

        public bool IsEditable
        {
            get
            {
                return this.isEditable;
            }

            set
            {
                this.SetValue(ref this.isEditable, value);
            }
        }

        public PlaylistBindingModel SelectedItem
        {
            get
            {
                return this.selectedItem;
            }

            set
            {
                this.SetValue(ref this.selectedItem, value);
            }
        }

        public List<PlaylistsGroupBindingModel> Groups { get; set; }
    }
}