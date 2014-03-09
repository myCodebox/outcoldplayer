﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;
    using System.Diagnostics;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media.Animation;

    using OutcoldSolutions.GoogleMusic.Controls;
    using OutcoldSolutions.GoogleMusic.Presenters;

    /// <summary>
    /// The page view base.
    /// </summary>
    public class PageViewBase : ViewBase, IPageView
    {
        /// <summary>
        /// The title dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = 
            DependencyProperty.Register("Title", typeof(string), typeof(PageViewBase), new PropertyMetadata(null));

        /// <summary>
        /// The subtitle dependency property.
        /// </summary>
        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register("Subtitle", typeof(string), typeof(PageViewBase), new PropertyMetadata(null));

        /// <summary>
        /// The is title visible dependency property.
        /// </summary>
        public static readonly DependencyProperty IsTitleVisibleProperty =
            DependencyProperty.Register("IsTitleVisible", typeof(bool), typeof(PageViewBase), new PropertyMetadata(true));

        /// <summary>
        /// The is store logo visible dependency property.
        /// </summary>
        public static readonly DependencyProperty IsStoreLogoVisibleProperty =
            DependencyProperty.Register("IsStoreLogoVisible", typeof(bool), typeof(PageViewBase), new PropertyMetadata(false));

        private const string HorizontalScrollOffset = "ListView_HorizontalScrollOffset";
        private const string VerticalScrollOffset = "ListView_VerticalScrollOffset";

        private FrameworkElement trackingControl;
        private Storyboard trackingListStoryboard;

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title
        {
            get { return (string)this.GetValue(TitleProperty); }
            set { this.SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the subtitle.
        /// </summary>
        public string Subtitle
        {
            get { return (string)this.GetValue(SubtitleProperty); }
            set { this.SetValue(SubtitleProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether is title visible.
        /// </summary>
        public bool IsTitleVisible
        {
            get { return (bool)this.GetValue(IsTitleVisibleProperty); }
            set { this.SetValue(IsTitleVisibleProperty, value); }
        }

        /// <summary>
        /// Gets the navigation service.
        /// </summary>
        protected INavigationService NavigationService { get; private set; }

        /// <inheritdoc />
        public virtual void OnNavigatedTo(NavigatedToEventArgs eventArgs)
        {
            ((IPagePresenterBase)this.DataContext).OnNavigatedTo(eventArgs);
        }

        /// <inheritdoc />
        public virtual void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            ((IPagePresenterBase)this.DataContext).OnNavigatingFrom(eventArgs);

            if (this.trackingControl != null)
            {
                eventArgs.State[HorizontalScrollOffset] =
                    this.trackingControl.GetScrollViewerHorizontalOffset();
                eventArgs.State[VerticalScrollOffset] =
                    this.trackingControl.GetScrollViewerVerticalOffset();

                this.trackingControl.Opacity = 0;
            }
        }
        
        /// <summary>
        /// On data loading.
        /// </summary>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        public virtual void OnDataLoading(NavigatedToEventArgs eventArgs)
        {
            if (this.trackingControl != null)
            {
                this.trackingControl.ScrollToHorizontalZero();
                this.trackingControl.ScrollToVerticalZero();
            }
        }

        /// <summary>
        /// On unfreeze.
        /// </summary>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        public virtual void OnUnfreeze(NavigatedToEventArgs eventArgs)
        {
        }

        /// <summary>
        /// On data loaded.
        /// </summary>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        public virtual void OnDataLoaded(NavigatedToEventArgs eventArgs)
        {
            if (this.trackingControl != null)
            {
                if (eventArgs.IsNavigationBack)
                {
                    this.trackingControl.UpdateLayout();

                    object offset;
                    if (eventArgs.State.TryGetValue(HorizontalScrollOffset, out offset))
                    {
                        this.trackingControl.ScrollToHorizontalOffset((double)offset);
                    }

                    if (eventArgs.State.TryGetValue(VerticalScrollOffset, out offset))
                    {
                        this.trackingControl.ScrollToVerticalOffset((double)offset);
                    }
                }

                this.trackingListStoryboard.Begin();
            }
        }

        /// <summary>
        /// Track list view base.
        /// </summary>
        /// <param name="frameworkElement">
        /// The list view base.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="frameworkElement"/> is null.
        /// </exception>
        protected void TrackScrollViewer(FrameworkElement frameworkElement)
        {
            if (frameworkElement == null)
            {
                throw new ArgumentNullException("frameworkElement");
            }

            Debug.Assert(this.trackingControl == null, "this.trackingControl == null. Only one list view tracking supported.");
            this.trackingControl = frameworkElement;
            if (this.trackingControl.Transitions != null)
            {
                this.trackingControl.Transitions.Clear();
            }

            this.trackingControl.Opacity = 0;

            this.trackingListStoryboard = new Storyboard();
            DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTarget(doubleAnimationUsingKeyFrames, this.trackingControl);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames, "Opacity");
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new LinearDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0)), Value = 0 });
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new LinearDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100)), Value = 0 });
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new LinearDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300)), Value = 1 });
            this.trackingListStoryboard.Children.Add(doubleAnimationUsingKeyFrames);
            this.Resources.Add("TrackingListStoryboard", this.trackingListStoryboard);
        }

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.NavigationService = this.Container.Resolve<INavigationService>();
        }
    }
}
