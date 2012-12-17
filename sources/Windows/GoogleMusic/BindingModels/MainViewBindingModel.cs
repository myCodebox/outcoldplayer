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
        private bool canGoBack = false;

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

        public bool CanGoBack
        {
            get
            {
                return this.canGoBack;
            }

            set
            {
                if (this.canGoBack != value)
                {
                    this.canGoBack = value;
                    this.RaiseCurrenntPropertyChanged();
                }
            }
        }
    }
}