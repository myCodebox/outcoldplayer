// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Controls
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    [TemplateVisualState(GroupName = "PauseGroup", Name = "Paused")]
    [TemplateVisualState(GroupName = "PauseGroup", Name = "Active")] 
    public class FakeEqualizerControl : Control
    {
        public static readonly DependencyProperty IsPausedProperty = DependencyProperty.Register(
            "IsPaused",
            typeof(bool),
            typeof(FakeEqualizerControl),
            new PropertyMetadata(false, (o, args) => ((FakeEqualizerControl)o).OnPausedChanged()));

        public bool IsPaused
        {
            get { return (bool)this.GetValue(IsPausedProperty); }
            set { this.SetValue(IsPausedProperty, value); }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.OnPausedChanged();
        }

        private void OnPausedChanged()
        {
            if (this.IsPaused)
            {
                VisualStateManager.GoToState(this, "Paused", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "Active", true);
            }
        }
    }
}
