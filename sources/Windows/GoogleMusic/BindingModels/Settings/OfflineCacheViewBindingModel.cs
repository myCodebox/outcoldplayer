// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels.Settings
{
    using OutcoldSolutions.BindingModels;

    public class OfflineCacheViewBindingModel : BindingModelBase
    {
        private long albumArtCacheSize;
        private bool isLoading;

        private long songsCacheSize;

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
    }
}