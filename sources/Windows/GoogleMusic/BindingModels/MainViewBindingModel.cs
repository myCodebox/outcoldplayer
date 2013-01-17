// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;

    using OutcoldSolutions.GoogleMusic.Presentation;

    public class MainViewBindingModel : BindingModelBase
    {
        private string message;
        private bool isProgressRingActive = true;
        private bool canGoBack = false;
        private bool isAuthenticated = false;

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
                    this.RaiseCurrentPropertyChanged();
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
                    this.RaiseCurrentPropertyChanged();
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
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return this.isAuthenticated;
            }
            
            set
            {
                if (this.isAuthenticated != value)
                {
                    this.isAuthenticated = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }
    }
}