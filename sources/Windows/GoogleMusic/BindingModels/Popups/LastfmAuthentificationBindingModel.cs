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
                this.SetValue(ref this.message, value);
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
                this.SetValue(ref this.isLoading, value);
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
                this.SetValue(ref this.link, value);
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
                this.SetValue(ref this.isLinkVisible, value);
            }
        }
    }
}