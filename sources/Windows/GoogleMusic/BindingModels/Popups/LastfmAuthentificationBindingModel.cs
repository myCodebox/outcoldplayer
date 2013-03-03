// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels.Popups
{
    using OutcoldSolutions.BindingModels;

    public class LastfmAuthentificationBindingModel : BindingModelBase
    {
        private string message;
        private bool isLoading;
        private string link;

        private bool isLinkVisible;

        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                this.message = value;
                this.RaiseCurrentPropertyChanged();
            }
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

        public string LinkUrl
        {
            get
            {
                return this.link;
            }

            set
            {
                this.link = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public bool IsLinkVisible
        {
            get
            {
                return this.isLinkVisible;
            }

            set
            {
                this.isLinkVisible = value;
                this.RaiseCurrentPropertyChanged();
            }
        }
    }
}