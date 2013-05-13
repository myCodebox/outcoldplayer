// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels.Settings
{
    using OutcoldSolutions.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public class CachedSongBindingModel : BindingModelBase
    {
        private bool isDownloading;

        private double downloadProgress;

        public CachedSongBindingModel(CachedSong cachedSong)
        {
            this.CachedSong = cachedSong;
        }

        public CachedSong CachedSong { get; private set; }

        public bool IsDownloading
        {
            get
            {
                return this.isDownloading;
            }

            set
            {
                this.SetValue(ref this.isDownloading, value);
            }
        }

        public double DownloadProgress
        {
            get
            {
                return this.downloadProgress;
            }

            set
            {
                this.SetValue(ref this.downloadProgress, value);
            }
        }
    }
}