// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Controls
{
    using System;

    using Windows.UI.Interactivity;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public class TextChangedBindingBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextChangedBindingBehavior), new PropertyMetadata(string.Empty, TextPropertyChangedCallback));

        private static void TextPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var behavior = dependencyObject as TextChangedBindingBehavior;
            if (behavior != null && behavior.AssociatedObject != null)
            {
                behavior.AssociatedObject.Text = eventArgs.NewValue as string ?? string.Empty;
            }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Text = this.Text;
            this.AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.AssociatedObject.TextChanged -= this.AssociatedObjectOnTextChanged;
        }

        private void AssociatedObjectOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            if (!string.Equals(this.Text, this.AssociatedObject.Text, StringComparison.CurrentCulture))
            {
                this.Text = this.AssociatedObject.Text;
            }
        }
    }
}
