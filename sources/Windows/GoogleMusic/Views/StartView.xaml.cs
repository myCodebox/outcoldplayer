//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using Windows.UI.Xaml.Controls;

    public interface IStartView : IView
    {
    }

    public sealed partial class StartView : UserControl, IStartView
    {
        public StartView()
        {
            this.InitializeComponent();
        }
    }
}
