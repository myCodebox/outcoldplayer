// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels.Settings
{
    public class AccountViewBindingModel : BindingModelBase
    {
        private string accountName;
        private string message;

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