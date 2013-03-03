// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.BindingModels;

    public class InitPageViewBindingModel : BindingModelBase
    {
        private string message;

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
    }
}