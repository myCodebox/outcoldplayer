// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Controls
{
    using System;

    using Windows.UI;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;

    public class Rating : Control
    {
        public static readonly DependencyProperty ValueProperty = 
            DependencyProperty.Register(
            "Value",
            typeof(int), 
            typeof(Rating), 
            new PropertyMetadata(
                (int)0, 
                (o, args) =>
                {
                    ((Rating)o).UpdateStars((int?)args.NewValue);
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

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            for (int i = 0; i < this.stars.Length; i++)
            {
                int value = i + 1;
                this.stars[i] = (Button)this.GetTemplateChild("Star" + value);
                this.stars[i].Click += (sender, args) =>
                    {
                        this.Value = value;
                        this.RaiseValueChanged(new ValueChangedEventArgs(Value));
                    };
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

        private void UpdateStars(int? newValue)
        {
            if (!newValue.HasValue)
            {
                newValue = 0;
            }

            for (int i = 0; i < this.stars.Length; i++)
            {
                if (this.stars[i] != null)
                {
                    if (i < newValue.Value)
                    {
                        this.stars[i].Foreground = this.FillBrush;
                    }
                    else
                    {
                        this.stars[i].Foreground = this.EmptyBrush;
                    }
                }
            }
        }
    }
}