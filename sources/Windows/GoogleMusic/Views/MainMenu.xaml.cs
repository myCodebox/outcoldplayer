// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    public interface IMainMenu : IView
    {
    }

    public sealed partial class MainMenu : ViewBase, IMainMenu
    {
        public MainMenu()
        {
            this.InitializeComponent();
        }
    }
}
