// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels.Settings
{
    public class AccountViewBindingModel : BindingModelBase
    {
        private string accountName;
        private string message;
        private bool isRemembered;

        public string AccountName
        {
            get
            {
                return this.accountName;
            }

            set
            {
                this.accountName = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public bool IsRemembered
        {
            get
            {
                return this.isRemembered;
            }

            set
            {
                this.isRemembered = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

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
    }
}