// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;

    public class MainViewBindingModel : BindingModelBase
    {
        private string message;

        private bool isProgressRingActive = true;

        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                if (!string.Equals(this.message, value, StringComparison.CurrentCultureIgnoreCase))
                {
                    this.message = value;
                    this.RaiseCurrenntPropertyChanged();
                }
            }
        }

        public bool IsProgressRingActive
        {
            get
            {
                return this.isProgressRingActive;
            }

            set
            {
                if (this.isProgressRingActive != value)
                {
                    this.isProgressRingActive = value;
                    this.RaiseCurrenntPropertyChanged();
                }
            }
        }
    }
}