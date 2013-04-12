// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Controls
{
    using System;
    using System.Globalization;
    using System.Windows.Input;

    using Windows.UI;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;

    [TemplateVisualState(GroupName = "Hover", Name = "Start1Hover")]
    [TemplateVisualState(GroupName = "Hover", Name = "Start2Hover")] 
    [TemplateVisualState(GroupName = "Hover", Name = "Start3Hover")] 
    [TemplateVisualState(GroupName = "Hover", Name = "Start4Hover")] 
    [TemplateVisualState(GroupName = "Hover", Name = "Start5Hover")] 
    [TemplateVisualState(GroupName = "Hover", Name = "NoHover")] 
    public class Rating : Control
    {
        public static readonly DependencyProperty ValueProperty = 
            DependencyProperty.Register(
            "Value",
            typeof(int), 
            typeof(Rating), 
            new PropertyMetadata(
                0, 
                async (o, args) =>
                    {
                        int newValue = 0;
                        if (args.NewValue is int)
                        {
                            newValue = (int)args.NewValue;
                        }

                        var rating = (Rating)o;
                        await rating.Dispatcher.RunAsync(
                            CoreDispatcherPriority.High,
                            () => rating.UpdateStars(newValue));
                    }));

        public static readonly DependencyProperty FillBrushProperty = 
            DependencyProperty.Register(
            "FillBrush",
            typeof(Brush),
            typeof(Rating),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF))));

        public static readonly DependencyProperty EmptyBrushProperty = 
            DependencyProperty.Register(
            "EmptyBrush",
            typeof(Brush),
            typeof(Rating),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x55, 0x55, 0x55))));

        public static readonly DependencyProperty CommandProperty = 
            DependencyProperty.Register(
            "Command", 
            typeof(ICommand), 
            typeof(Rating), 
            new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
            "CommandParameter", 
            typeof(object), 
            typeof(Rating), 
            new PropertyMetadata(null));

        private readonly Button[] stars = new Button[5];

        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        public Brush FillBrush
        {
            get { return (Brush)this.GetValue(FillBrushProperty); }
            set { this.SetValue(FillBrushProperty, value); }
        }

        public Brush EmptyBrush
        {
            get { return (Brush)this.GetValue(EmptyBrushProperty); }
            set { this.SetValue(EmptyBrushProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)this.GetValue(CommandProperty); }
            set { this.SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return this.GetValue(CommandParameterProperty); }
            set { this.SetValue(CommandParameterProperty, value); }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            for (int i = 0; i < this.stars.Length; i++)
            {
                int value = i + 1;
                this.stars[i] = (Button)this.GetTemplateChild("Star" + value);
                this.stars[i].Click += (sender, args) =>
                    {
                        var command = this.Command;

                        this.Value = value;
                        this.RaiseValueChanged(new ValueChangedEventArgs(Value));

                        if (command != null)
                        {
                            var eventArgs = new RatingEventArgs(this.CommandParameter, value);
                            if (command.CanExecute(eventArgs))
                            {
                                command.Execute(eventArgs);
                            }
                        }
                    };

                this.stars[i].PointerEntered += (sender, args) => VisualStateManager.GoToState(this, string.Format(CultureInfo.InvariantCulture, "Start{0}Hover", value), false);
                this.PointerExited += (sender, args) => VisualStateManager.GoToState(this, "NoHover", false);
            }

            this.UpdateStars(this.Value);
        }

        private void RaiseValueChanged(ValueChangedEventArgs e)
        {
            var handler = this.ValueChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void UpdateStars(int newValue)
        {
            for (int i = 0; i < this.stars.Length; i++)
            {
                if (this.stars[i] != null)
                {
                    this.stars[i].Foreground = i < newValue ? this.FillBrush : this.EmptyBrush;
                }
            }
        }
    }
}