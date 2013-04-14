// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    using OutcoldSolutions.Views;

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
