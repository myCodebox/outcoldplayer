// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.BindingModels;

    public class LinksRegionBindingModel : BindingModelBase 
    {
        private bool isSynchronizing;
        private string updatingText;

        public bool IsSynchronizing
        {
            get
            {
                return this.isSynchronizing;
            }

            set
            {
                this.SetValue(ref this.isSynchronizing, value);
            }
        }

        public string UpdatingText
        {
            get
            {
                return this.updatingText;
            }

            set
            {
                this.SetValue(ref this.updatingText, value);
            }
        }
    }
}
