// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    public interface IQueueActionsPopupView : IPopupView
    {
    }

    public sealed partial class QueueActionsPopupView : PopupViewBase, IQueueActionsPopupView
    {
        public QueueActionsPopupView()
        {
            this.InitializeComponent();
        }
    }
}
