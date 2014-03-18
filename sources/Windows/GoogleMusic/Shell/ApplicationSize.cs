// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Shell
{
    using Windows.Foundation;

    using OutcoldSolutions.GoogleMusic.BindingModels;

    public class ApplicationSize : BindingModelBase
    {
        private double width;
        private double height;

        public double Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.SetValue(ref this.width, value);
            }
        }

        public double Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.SetValue(ref this.height, value);
            }
        }

        public bool IsLarge
        {
            get
            {
                return this.width >= 1024;
            }   
        }

        public bool IsMedium
        {
            get
            {
                return !this.IsLarge && this.width >= 767;
            }
        }

        public bool IsSmall
        {
            get
            {
                return !this.IsLarge && !this.IsMedium;
            }
        }

        public bool IsMediumOrLarge
        {
            get
            {
                return this.IsMedium || this.IsLarge;
            }
        }

        public bool IsSmallOrMedium
        {
            get
            {
                return this.IsSmall || this.IsMedium;
            }
        }

        public ApplicationSize Instance
        {
            get
            {
                return this;
            }
        }

        public bool OnSizeChanged(Size newSizes)
        {
            bool isLargeOld = this.IsLarge;
            bool isSmallOld = this.IsSmall;
            bool isMediumOld = this.IsMedium;

            this.Width = newSizes.Width;
            this.Height = newSizes.Height;

            this.RaisePropertyChanged(() => this.IsLarge);
            this.RaisePropertyChanged(() => this.IsSmall);
            this.RaisePropertyChanged(() => this.IsMedium);
            this.RaisePropertyChanged(() => this.IsMediumOrLarge);
            this.RaisePropertyChanged(() => this.IsSmallOrMedium);
            this.RaisePropertyChanged(() => this.Instance);

            return this.IsLarge == isLargeOld && this.IsSmall == isSmallOld && this.IsMedium == isMediumOld;
        }
    }
}
