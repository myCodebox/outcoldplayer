// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    public class SnappedPlayerBindingModel : PlayerBindingModel
    {
        private bool isShuffleEnabled;
        private bool isRepeatAllEnabled;
        private bool isQueueEmpty;

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

        public bool IsQueueEmpty
        {
            get
            {
                return this.isQueueEmpty;
            }

            set
            {
                this.SetValue(ref this.isQueueEmpty, value);
            }
        }
    }
}
