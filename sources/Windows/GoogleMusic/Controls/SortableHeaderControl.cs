// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Controls
{
    using System.Windows.Input;

    using OutcoldSolutions.GoogleMusic.BindingModels;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    [TemplateVisualState(GroupName = "SortDirection", Name = "Unknown")]
    [TemplateVisualState(GroupName = "SortDirection", Name = "Up")]
    [TemplateVisualState(GroupName = "SortDirection", Name = "Down")] 
    public class SortableHeaderControl : Control
    {
        public static readonly DependencyProperty UpSortingProperty = DependencyProperty.Register(
            "UpSorting", typeof(SongsSorting), typeof(SortableHeaderControl), new PropertyMetadata(SongsSorting.Unknown));

        public static readonly DependencyProperty DownSortingProperty = DependencyProperty.Register(
            "DownSorting", typeof(SongsSorting), typeof(SortableHeaderControl), new PropertyMetadata(SongsSorting.Unknown));

        public static readonly DependencyProperty CurrentSortingProperty = DependencyProperty.Register(
            "CurrentSorting", typeof(SongsSorting), typeof(SortableHeaderControl), new PropertyMetadata(SongsSorting.Unknown, (o, args) => ((SortableHeaderControl)o).OnCurrentSortingChanged()));

        public static readonly DependencyProperty SortCommandProperty = DependencyProperty.Register(
            "SortCommand", typeof(ICommand), typeof(SortableHeaderControl), new PropertyMetadata(null));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(SortableHeaderControl), new PropertyMetadata(string.Empty));

        public SongsSorting UpSorting
        {
            get { return (SongsSorting)this.GetValue(UpSortingProperty); }
            set { this.SetValue(UpSortingProperty, value); }
        }

        public SongsSorting DownSorting
        {
            get { return (SongsSorting)this.GetValue(DownSortingProperty); }
            set { this.SetValue(DownSortingProperty, value); }
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
                if (this.CurrentSorting == this.DownSorting)
                {
                    if (this.SortCommand.CanExecute(this.UpSorting))
                    {
                        this.SortCommand.Execute(this.UpSorting);
                    }
                }
                else
                {
                    if (this.SortCommand.CanExecute(this.DownSorting))
                    {
                        this.SortCommand.Execute(this.DownSorting);
                    }
                }
            }
        }

        private void OnCurrentSortingChanged()
        {
            if (this.CurrentSorting == this.UpSorting)
            {
                VisualStateManager.GoToState(this, "Up", true);
            }
            else if (this.CurrentSorting == this.DownSorting)
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
