// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels.Settings
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;

    using OutcoldSolutions.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;

    public class OfflineCacheViewBindingModel : BindingModelBase
    {
        private readonly ISettingsService settingsService;

        private ObservableCollection<CachedSongBindingModel> queuedTasks;

        private bool isLoading;
        private long albumArtCacheSize;
        private long songsCacheSize;

        public OfflineCacheViewBindingModel(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }

            set
            {
                this.SetValue(ref this.isLoading, value);
            }
        }

        public long AlbumArtCacheSize
        {
            get
            {
                return this.albumArtCacheSize;
            }

            set
            {
                this.SetValue(ref this.albumArtCacheSize, value);
            }
        }

        public long SongsCacheSize
        {
            get
            {
                return this.songsCacheSize;
            }

            set
            {
                this.SetValue(ref this.songsCacheSize, value);
            }
        }

        public bool AutomaticCache
        {
            get
            {
                return this.settingsService.GetAutomaticCache();
            }

            set
            {
                this.settingsService.SetAutomaticCache(value);
            }
        }

        public uint MaximumCacheSize
        {
            get
            {
                return this.settingsService.GetMaximumCacheSize();
            }

            set
            {
                this.settingsService.SetMaximumCacheSize(value);
            }
        }

        public bool IsCacheEditable
        {
            get
            {
                return InAppPurchases.HasFeature(GoogleMusicFeatures.Offline);
            }
        }

        public ObservableCollection<CachedSongBindingModel> QueuedTasks
        {
            get
            {
                return this.queuedTasks;
            }

            set
            {
                if (this.queuedTasks != null)
                {
                    this.queuedTasks.CollectionChanged -= QueuedTasksOnCollectionChanged;
                }

                this.SetValue(ref this.queuedTasks, value);
                this.RaisePropertyChanged(() => this.IsQueueEmpty);

                if (this.queuedTasks != null)
                {
                    this.queuedTasks.CollectionChanged += QueuedTasksOnCollectionChanged;
                }
            }
        }

        public bool IsQueueEmpty
        {
            get
            {
                return this.queuedTasks != null && this.queuedTasks.Count == 0;
            }
        }

        private void QueuedTasksOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            this.RaisePropertyChanged(() => this.IsQueueEmpty);
        }
    }
}