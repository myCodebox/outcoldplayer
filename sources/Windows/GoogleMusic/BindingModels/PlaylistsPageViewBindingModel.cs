// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    public class PlaylistsPageViewBindingModel : BindingModelBase
    {
        private int count;
        private string title;
        private bool isEditable;
        private PlaylistBindingModel selectedItem;

        public int Count
        {
            get
            {
                return this.count;
            }

            set
            {
                if (this.count != value)
                {
                    this.count = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                if (this.title != value)
                {
                    this.title = value;
                    this.RaiseCurrentPropertyChanged();
                }
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
                this.isEditable = value;
                this.RaiseCurrentPropertyChanged();
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
                this.selectedItem = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public List<PlaylistsGroupBindingModel> Groups { get; set; }
    }
}