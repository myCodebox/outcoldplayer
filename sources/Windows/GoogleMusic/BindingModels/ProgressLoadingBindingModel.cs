// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.BindingModels;

    public class ProgressLoadingBindingModel : BindingModelBase
    {
        private string message;
        private double progress = 0;
        private bool isFailed;
        private double maximum = 1;

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

        public double Progress
        {
            get
            {
                return this.progress;
            }

            set
            {
                this.progress = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public double Maximum
        {
            get
            {
                return this.maximum;
            }

            set
            {
                this.maximum = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public bool IsFailed
        {
            get
            {
                return this.isFailed;
            }

            set
            {
                this.isFailed = value;
                this.RaiseCurrentPropertyChanged();
            }
        }
    }
}