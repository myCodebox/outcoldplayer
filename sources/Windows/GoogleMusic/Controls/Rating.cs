// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Controls
{
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
            new PropertyMetadata(0, (o, args) => ((Rating)o).UpdateStars()));

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

        private TextBlock[] stars = new TextBlock[5];

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
                this.stars[i] = (TextBlock)this.GetTemplateChild("Star" + (i + 1));
            }

            this.UpdateStars();
        }

        private void UpdateStars()
        {
            for (int i = 0; i < this.stars.Length; i++)
            {
                if (this.stars[i] != null)
                {
                    if (i < this.Value)
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