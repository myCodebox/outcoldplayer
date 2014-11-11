// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading.Tasks;

    using Windows.ApplicationModel;

    using Microsoft.Xaml.Interactivity;

    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    using OutcoldSolutions.GoogleMusic.Diagnostics;

    public class ListViewBaseSelectedItemsBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            "SelectedItems",
            typeof(object),
            typeof(ListViewBaseSelectedItemsBehavior),
            new PropertyMetadata(null, (o, args) =>
            {
                ((ListViewBaseSelectedItemsBehavior)o).OnSelectedItemsChanged(args);
            }));

        public static readonly DependencyProperty ForceToShowProperty = DependencyProperty.Register(
            "ForceToShow", typeof(bool), typeof(ListViewBaseSelectedItemsBehavior), new PropertyMetadata(false));

        private static readonly Lazy<ILogger> Logger = new Lazy<ILogger>(() => ApplicationContext.Container.Resolve<ILogManager>().CreateLogger(typeof(ListViewBaseSelectedItemsBehavior).Name));

        private bool freezed = false;

        public object SelectedItems
        {
            get { return (object)this.GetValue(SelectedItemsProperty); }
            set { this.SetValue(SelectedItemsProperty, value); }
        }

        public bool ForceToShow
        {
            get { return (bool)this.GetValue(ForceToShowProperty); }
            set { this.SetValue(ForceToShowProperty, value); }
        }

        public DependencyObject AssociatedObject { get; private set; }

        public ListViewBase AssociatedListViewBase
        {
            get
            {
                return (ListViewBase)this.AssociatedObject;
            }
        }

        public void Attach(DependencyObject associatedObject)
        {
            if (!(associatedObject is ListViewBase))
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

                this.AssociatedListViewBase.SelectionChanged += this.OnSelectionChanged;
                this.Synchronize();
            }
        }

        public void Detach()
        {
            if (this.AssociatedListViewBase != null)
            {
                this.AssociatedListViewBase.SelectionChanged -= this.OnSelectionChanged;
            }
        }

        private void OnSelectedItemsChanged(DependencyPropertyChangedEventArgs args)
        {
            var oldNotifyCollectionChanged = args.OldValue as INotifyCollectionChanged;
            if (oldNotifyCollectionChanged != null)
            {
                oldNotifyCollectionChanged.CollectionChanged -= this.OnCollectionChanged;
            }

            var newNotifyCollectionChanged = args.NewValue as INotifyCollectionChanged;
            if (newNotifyCollectionChanged != null)
            {
                newNotifyCollectionChanged.CollectionChanged += this.OnCollectionChanged;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.freezed)
            {
                return;
            }

            var collection = this.SelectedItems as IList;
            if (collection != null)
            {
                if (e.AddedItems == null && e.RemovedItems == null)
                {
                    collection.Clear();
                }
                else
                {
                    this.freezed = true;

                    if (e.RemovedItems != null)
                    {
                        var removedItems = e.RemovedItems.Where(collection.Contains).ToList();

                        foreach (object item in removedItems)
                        {
                            collection.Remove(item);
                        }
                    }

                    if (e.AddedItems != null)
                    {
                        var addedItems = e.AddedItems.Where(x => !collection.Contains(x)).ToList();

                        foreach (object item in addedItems)
                        {
                            collection.Add(item);
                        }
                    }

                    this.freezed = false;
                }
            }
        }

        private async void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.freezed)
            {
                return;
            }

            if (this.AssociatedListViewBase != null)
            {
                if (e.NewItems == null && e.OldItems == null)
                {
                    if (this.AssociatedListViewBase.SelectedItems != null)
                    {
                        this.AssociatedListViewBase.SelectedItems.Clear();
                    }
                }
                else
                {
                    this.freezed = true;

                    if (e.OldItems != null)
                    {
                        if (this.AssociatedListViewBase.SelectedItems != null)
                        {
                            foreach (object item in e.OldItems)
                            {
                                if (this.AssociatedListViewBase.SelectedItems.Contains(item))
                                {
                                    this.AssociatedListViewBase.SelectedItems.Remove(item);
                                }
                            }
                        }
                    }

                    if (e.NewItems != null)
                    {
                        if (this.AssociatedListViewBase.SelectedItems != null)
                        {
                            foreach (object item in e.NewItems)
                            {
                                if (!this.AssociatedListViewBase.SelectedItems.Contains(item))
                                {
                                    this.AssociatedListViewBase.SelectedItems.Add(item);
                                }
                            }
                        }
                    }

                    this.freezed = false;
                }

                if (this.ForceToShow && e.NewItems != null && this.AssociatedListViewBase.SelectedItems != null && this.AssociatedListViewBase.SelectedItems.Count == 1)
                {
                    await Task.Yield();

                    await this.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Low, () =>
                            {
                                try
                                {
                                    if (this.AssociatedListViewBase != null && e.NewItems.Count > 0)
                                    {
                                        this.AssociatedListViewBase.ScrollIntoView(e.NewItems[0]);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Logger.Value.Debug(exception, "OnCollectionChanged");
                                }
                            });
                }
            }
        }

        private async void Synchronize()
        {
            if (this.AssociatedListViewBase != null)
            {
                this.AssociatedListViewBase.SelectedItems.Clear();

                var collection = this.SelectedItems as IList;
                if (collection != null)
                {
                    foreach (var selectedItem in collection)
                    {
                        this.AssociatedListViewBase.SelectedItems.Add(selectedItem);
                    }
                }

                if (this.ForceToShow && this.AssociatedListViewBase.SelectedItems.Count == 1)
                {
                    await Task.Yield();

                    await this.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Low, () =>
                            {
                                try
                                {
                                    if (this.AssociatedListViewBase != null && this.AssociatedListViewBase.SelectedItems.Count > 0)
                                    {
                                        this.AssociatedListViewBase.ScrollIntoView(this.AssociatedListViewBase.SelectedItems[0]);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Logger.Value.Debug(exception, "Synchronize");
                                }
                            });
                }
            }
        }
    }
}