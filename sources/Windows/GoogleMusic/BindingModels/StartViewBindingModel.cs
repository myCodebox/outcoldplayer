// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    public class StartViewBindingModel : BindingModelBase
    {
        private bool isLoading;
        
        public StartViewBindingModel()
        {
        }

        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }

            set
            {
                if (this.isLoading != value)
                {
                    this.isLoading = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }
    }
}