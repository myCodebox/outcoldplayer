// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Controls
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls.Primitives;

    public class AppBarToggleButton : ToggleButton
    {
        public AppBarToggleButton()
        {
            this.Click += (sender, args) => VisualStateManager.GoToState(this, this.IsChecked.HasValue && this.IsChecked.Value ? "Checked" : "Unchecked", false);
        }
    }
}