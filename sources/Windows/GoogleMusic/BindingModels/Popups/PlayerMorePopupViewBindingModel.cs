// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.BindingModels.Popups
{
    using OutcoldSolutions.BindingModels;
    using OutcoldSolutions.GoogleMusic.Shell;

    public class PlayerMorePopupViewBindingModel : BindingModelBase
    {
        private readonly IMediaElementContainer mediaElementContainer;

        private bool isShuffleEnabled;
        private bool isRepeatAllEnabled;

        public PlayerMorePopupViewBindingModel(
            IMediaElementContainer mediaElementContainer)
        {
            this.mediaElementContainer = mediaElementContainer;
        }

        public bool IsShuffleEnabled
        {
            get
            {
                return this.isShuffleEnabled;
            }

            set
            {
                this.SetValue(ref this.isShuffleEnabled, value);
            }
        }

        public bool IsRepeatAllEnabled
        {
            get
            {
                return this.isRepeatAllEnabled;
            }

            set
            {
                this.SetValue(ref this.isRepeatAllEnabled, value);
            }
        }

        public double Volume
        {
            get
            {
                return this.mediaElementContainer.Volume;
            }
        }
    }
}
