// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Controls
{
    using System;

    using Windows.ApplicationModel;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    using Microsoft.Xaml.Interactivity;

    public class TextChangedBindingBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextChangedBindingBehavior), new PropertyMetadata(string.Empty, TextPropertyChangedCallback));

        private static void TextPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var behavior = dependencyObject as TextChangedBindingBehavior;
            if (behavior != null && behavior.AssociatedObject != null)
            {
                ((TextBox)behavior.AssociatedObject).Text = eventArgs.NewValue as string ?? string.Empty;
            }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public DependencyObject AssociatedObject { get; private set; }

        public void Attach(DependencyObject associatedObject)
        {
           if (!(associatedObject is TextBox))
            {
                throw new ArgumentException("Behavior works only with ListView");
            }

            if ((associatedObject != this.AssociatedObject) && !DesignMode.DesignModeEnabled)
            {
                if (this.AssociatedObject != null)
                {
                    throw new InvalidOperationException("Cannot attach behavior multiple times.");
                }

                this.AssociatedObject = associatedObject;

                ((TextBox)this.AssociatedObject).Text = this.Text;
                ((TextBox)this.AssociatedObject).TextChanged += AssociatedObjectOnTextChanged;
            }
        }

        public void Detach()
        {
            if (this.AssociatedObject != null)
            {
                ((TextBox)this.AssociatedObject).TextChanged -= this.AssociatedObjectOnTextChanged;
            }
        }

        private void AssociatedObjectOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            if (this.AssociatedObject != null && !string.Equals(this.Text, ((TextBox)this.AssociatedObject).Text, StringComparison.CurrentCulture))
            {
                this.Text = ((TextBox)this.AssociatedObject).Text;
            }
        }
    }
}
