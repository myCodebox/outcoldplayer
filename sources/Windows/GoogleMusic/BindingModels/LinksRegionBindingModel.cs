// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.BindingModels;

    public class LinksRegionBindingModel : BindingModelBase 
    {
        private bool showProgressRing;
        private string messageText;

        public bool ShowProgressRing
        {
            get
            {
                return this.showProgressRing;
            }

            set
            {
                this.SetValue(ref this.showProgressRing, value);
            }
        }

        public string MessageText
        {
            get
            {
                return this.messageText;
            }

            set
            {
                this.SetValue(ref this.messageText, value);
            }
        }
    }
}
