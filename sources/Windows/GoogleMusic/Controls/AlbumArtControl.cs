// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Controls
{
    using System;

    using OutcoldSolutions.GoogleMusic.Converters;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Media.Imaging;

    [TemplateVisualState(GroupName = "State", Name = "BackgroundImage")]
    [TemplateVisualState(GroupName = "State", Name = "AlbumArtImage")] 
    public class AlbumArtControl : Control
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource", typeof(Uri), typeof(AlbumArtControl), new PropertyMetadata(null, (o, args) => ((AlbumArtControl)o).Update()));

        public static readonly DependencyProperty ImageSizeProperty = DependencyProperty.Register(
            "ImageSize", typeof(double), typeof(AlbumArtControl), new PropertyMetadata(0d, (o, args) => ((AlbumArtControl)o).Update()));

        private static readonly AlbumArtUrlToImageConverter Converter = new AlbumArtUrlToImageConverter();

        private Image backgroundImage;
        private Image albumArtImage;

        public Uri ImageSource
        {
            get { return (Uri)this.GetValue(ImageSourceProperty); }
            set { this.SetValue(ImageSourceProperty, value); }
        }

        public double ImageSize
        {
            get { return (double)this.GetValue(ImageSizeProperty); }
            set { this.SetValue(ImageSizeProperty, value); }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.backgroundImage = (Image)this.GetTemplateChild("Part_BackgroundImage");
            this.albumArtImage = (Image)this.GetTemplateChild("Part_AlbumArtImage");
            
            if (this.albumArtImage != null)
            {
                this.albumArtImage.ImageOpened += this.AlbumArtImage_ImageOpened;
            }

            if (this.backgroundImage != null)
            {
                this.backgroundImage.Source = new BitmapImage(new Uri((string)Converter.Convert(null, typeof(ImageSource), this.ImageSize, string.Empty)));
            }

            this.Update();
        }

        private void Update()
        {
            if (this.albumArtImage != null)
            {
                if (this.ImageSource != null)
                {
                    object convert = Converter.Convert(this.ImageSource, typeof(ImageSource), this.ImageSize, string.Empty);
                    BitmapImage source = convert as BitmapImage;
                    if (source == null)
                    {
                        source = new BitmapImage(new Uri((string)convert));
                    }

                    this.albumArtImage.Source = source;
                }
                else
                {
                    this.albumArtImage.Source = null;
                    VisualStateManager.GoToState(this, "BackgroundImage", true);
                }
            }
        }

        private void AlbumArtImage_ImageOpened(object sender, RoutedEventArgs routedEventArgs)
        {
            VisualStateManager.GoToState(this, "AlbumArtImage", true);
        }
    }
}
