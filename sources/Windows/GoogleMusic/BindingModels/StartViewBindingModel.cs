// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.GoogleMusic.Presentation;

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