// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.BindingModels;

    public class ProgressLoadingPageViewBindingModel : BindingModelBase
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
                this.SetValue(ref this.message, value);
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
                this.SetValue(ref this.progress, value);
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
                this.SetValue(ref this.maximum, value);
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
                this.SetValue(ref this.isFailed, value);
            }
        }
    }
}