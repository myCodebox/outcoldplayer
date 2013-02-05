// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.BindingModels;

    using Windows.Foundation;
    using Windows.UI;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;

    internal class PlaylistsGridView : Panel
    {
        public static readonly DependencyProperty GroupsProperty = DependencyProperty.Register(
            "Groups",
            typeof(ICollection<PlaylistsGroupBindingModel>),
            typeof(PlaylistsGridView),
            new PropertyMetadata(null, (o, args) => ((PlaylistsGridView)o).Populate()));

        private const double PlaylistItemWidth = 294d;
        private const double PlaylistItemFullWidth = PlaylistItemWidth + PlaylistItemMargin;
        private const double PlaylistItemHeight = 146d;
        private const double PlaylistItemFullHeight = PlaylistItemHeight + PlaylistItemMargin;
        private const double PlaylistItemMargin = 10d;
        private const double GroupItemMargin = 20d;

        private const double GroupHeaderHeight = 30d;

        public PlaylistsGridView()
        {
            this.Background = new SolidColorBrush(Colors.Chartreuse);
        }

        public ICollection<PlaylistsGroupBindingModel> Groups
        {
            get { return (ICollection<PlaylistsGroupBindingModel>)this.GetValue(GroupsProperty); }
            set { this.SetValue(GroupsProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.Groups == null)
            {
                return base.MeasureOverride(availableSize);
            }

            int elementsInColumn = Math.Max((int)((availableSize.Height + PlaylistItemMargin - (GroupHeaderHeight + GroupItemMargin)) / PlaylistItemFullHeight), 1);

            var width = Math.Max(
                    this.Groups.Sum(g => ((this.GetExpectedColumns(g.Playlists.Count, elementsInColumn) * PlaylistItemFullWidth) - PlaylistItemMargin) + GroupItemMargin) - GroupItemMargin,
                    0.0);

            return new Size(
                width, 
                GroupHeaderHeight + GroupItemMargin + (elementsInColumn * (PlaylistItemHeight + PlaylistItemMargin)) - PlaylistItemMargin);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this.Groups == null)
            {
                return finalSize;
            }

            int elementsInColumn = Math.Max((int)((finalSize.Height + PlaylistItemMargin - (GroupHeaderHeight + GroupItemMargin)) / PlaylistItemFullHeight), 1);

            double width = 0.0;

            int itemIndex = 0;
            foreach (var group in this.Groups)
            {
                if (width > 0.00001)
                {
                    width += GroupItemMargin;
                }

                double itemsWidth = (this.GetExpectedColumns(group.Playlists.Count, elementsInColumn) * PlaylistItemFullWidth) - PlaylistItemMargin;

                ((Button)this.Children[itemIndex]).Width = itemsWidth;
                this.Children[itemIndex].Arrange(new Rect(width, 0.0, itemsWidth, GroupHeaderHeight));

                itemIndex++;

                for (int i = 0; i < group.Playlists.Count; i++)
                {
                    var border = (ContentControl)this.Children[itemIndex + i];
                    border.Arrange(new Rect(
                        width + ((int)(i / elementsInColumn) * PlaylistItemFullWidth),
                        GroupHeaderHeight + GroupItemMargin + ((i % elementsInColumn) * PlaylistItemFullHeight), 
                        PlaylistItemWidth, 
                        PlaylistItemHeight));
                }

                width += itemsWidth;
                itemIndex += group.Playlists.Count;
            }

            return new Size(width, (elementsInColumn * PlaylistItemFullHeight) - PlaylistItemMargin);
        }

        private int GetExpectedColumns(int playlists, int elementsInColumn)
        {
            return (playlists / elementsInColumn) + (playlists % elementsInColumn > 0 ? 1 : 0);
        }

        private void Populate()
        {
            this.Children.Clear();

            if (this.Groups != null)
            {
                foreach (var group in this.Groups)
                {
                    this.Children.Add(
                        new Button()
                            {
                                Style = (Style)App.Current.Resources["HeaderButton"],
                                Content = string.Format("{0} ({1})", group.Title, group.PlaylistsCount),
                                Width = 80,
                                Height = GroupHeaderHeight
                            });

                    foreach (var playlist in group.Playlists)
                    {
                        this.Children.Add(
                            new ContentControl()
                                {
                                    Width = PlaylistItemWidth,
                                    Height = PlaylistItemHeight,
                                    Background = new SolidColorBrush(Colors.Blue),
                                    DataContext = playlist,
                                    Template =
                                        (ControlTemplate)App.Current.Resources["PlaylistControlTemplate"]
                                });
                    }
                }
            }
        }
    }
}
