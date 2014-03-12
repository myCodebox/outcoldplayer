// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Controls
{
    using System.Windows.Input;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    using OutcoldSolutions.GoogleMusic.Presenters;

    [TemplateVisualState(GroupName = "SortDirection", Name = "Unknown")]
    [TemplateVisualState(GroupName = "SortDirection", Name = "Up")]
    [TemplateVisualState(GroupName = "SortDirection", Name = "Down")] 
    public class SortableHeaderControl : Control
    {
        public static readonly DependencyProperty AscendingSortingProperty = DependencyProperty.Register(
            "AscendingSorting", typeof(SongsSorting), typeof(SortableHeaderControl), new PropertyMetadata(SongsSorting.Unknown));

        public static readonly DependencyProperty DescensingSortingProperty = DependencyProperty.Register(
            "DescensingSorting", typeof(SongsSorting), typeof(SortableHeaderControl), new PropertyMetadata(SongsSorting.Unknown));

        public static readonly DependencyProperty CurrentSortingProperty = DependencyProperty.Register(
            "CurrentSorting", typeof(SongsSorting), typeof(SortableHeaderControl), new PropertyMetadata(SongsSorting.Unknown, (o, args) => ((SortableHeaderControl)o).OnCurrentSortingChanged()));

        public static readonly DependencyProperty SortCommandProperty = DependencyProperty.Register(
            "SortCommand", typeof(ICommand), typeof(SortableHeaderControl), new PropertyMetadata(null));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(SortableHeaderControl), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TitleAlignmentProperty =
            DependencyProperty.Register("TitleAlignment", typeof(HorizontalAlignment), typeof(SortableHeaderControl), new PropertyMetadata(HorizontalAlignment.Left));

        public SongsSorting AscendingSorting
        {
            get { return (SongsSorting)this.GetValue(AscendingSortingProperty); }
            set { this.SetValue(AscendingSortingProperty, value); }
        }

        public SongsSorting DescensingSorting
        {
            get { return (SongsSorting)this.GetValue(DescensingSortingProperty); }
            set { this.SetValue(DescensingSortingProperty, value); }
        }

        public SongsSorting CurrentSorting
        {
            get { return (SongsSorting)this.GetValue(CurrentSortingProperty); }
            set { this.SetValue(CurrentSortingProperty, value); }
        }

        public string Title
        {
            get { return (string)this.GetValue(TitleProperty); }
            set { this.SetValue(TitleProperty, value); }
        }

        public HorizontalAlignment TitleAlignment
        {
            get { return (HorizontalAlignment)GetValue(TitleAlignmentProperty); }
            set { SetValue(TitleAlignmentProperty, value); }
        }

        public ICommand SortCommand 
        {
            get { return (ICommand)this.GetValue(SortCommandProperty); }
            set { this.SetValue(SortCommandProperty, value); }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var button = this.GetTemplateChild("Part_Button") as Button;
            if (button != null)
            {
                button.Click += this.ButtonOnClick;
            }

            this.OnCurrentSortingChanged();
        }

        private void ButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.SortCommand != null)
            {
                if (this.CurrentSorting == this.AscendingSorting)
                {
                    if (this.SortCommand.CanExecute(this.DescensingSorting))
                    {
                        this.SortCommand.Execute(this.DescensingSorting);
                    }
                }
                else
                {
                    if (this.SortCommand.CanExecute(this.AscendingSorting))
                    {
                        this.SortCommand.Execute(this.AscendingSorting);
                    }
                }
            }
        }

        private void OnCurrentSortingChanged()
        {
            if (this.CurrentSorting == this.AscendingSorting)
            {
                VisualStateManager.GoToState(this, "Up", true);
            }
            else if (this.CurrentSorting == this.DescensingSorting)
            {
                VisualStateManager.GoToState(this, "Down", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "Unknown", true);
            }
        }
    }
}
