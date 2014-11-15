// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;
    using System.Collections.Generic;
    using Windows.Storage;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class CacheMovePopupViewPresenter : ViewPresenterBase<ICacheMovePopupView>
    {
        private readonly ISongsCachingService songsCachingService;
        private readonly IPlayQueueService playQueueService;
        private readonly ISettingsService settingsService;
        private readonly IAnalyticsService analyticsService;
        private readonly INotificationService notificationService;
        private readonly bool isMovingToMusicLibrary;

        private int totalFiles;
        private int filesMoved;
        private string message;
        private bool isCounterVisible;

        public CacheMovePopupViewPresenter(
            ISongsCachingService songsCachingService,
            IPlayQueueService playQueueService,
            ISettingsService settingsService,
            IAnalyticsService analyticsService,
            INotificationService notificationService,
            bool isMovingToMusicLibrary)
        {
            this.songsCachingService = songsCachingService;
            this.playQueueService = playQueueService;
            this.settingsService = settingsService;
            this.analyticsService = analyticsService;
            this.notificationService = notificationService;
            this.isMovingToMusicLibrary = isMovingToMusicLibrary;
        }

        public int TotalFiles
        {
            get { return this.totalFiles; }
            set { this.SetValue(ref this.totalFiles, value); }
        }

        public int FilesMoved
        {
            get { return this.filesMoved; }
            set { this.SetValue(ref this.filesMoved, value); }
        }

        public string Message
        {
            get { return this.message; }
            set { this.SetValue(ref this.message, value); }
        }

        public bool IsCounterVisible
        {
            get { return this.isCounterVisible; }
            set { this.SetValue(ref this.isCounterVisible, value); }
        }

        protected override async void OnInitialized()
        {
            base.OnInitialized();

            await this.Dispatcher.RunAsync(() =>
            {
                this.Message = "Preparing new cache location...";
            });

            await this.playQueueService.StopAsync();
            await this.songsCachingService.CancelDownloadTaskAsync();

            StorageFolder appData = await this.songsCachingService.GetAppDataStorageFolderAsync();
            StorageFolder musicLibrary = null;

            try
            {
                musicLibrary = await this.songsCachingService.GetMusicLibraryStorageFolderAsync();
            }
            catch (Exception e)
            {
                this.Logger.Debug(e, "Cannot get access to the music library");
            }

            if (musicLibrary == null)
            {
                await this.notificationService.ShowMessageAsync("Application could not get access to the Music Library folder. Please verify that you have permissions to this folder.");
                this.View.Close();
            }
            else
            {
                StorageFolder to = this.isMovingToMusicLibrary ? musicLibrary : appData;
                StorageFolder from = this.isMovingToMusicLibrary ? appData : musicLibrary;

                IList<StorageFile> allFiles = new List<StorageFile>();

                foreach (var subfolder in await from.GetFoldersAsync())
                {
                    foreach (var file in await subfolder.GetFilesAsync())
                    {
                        allFiles.Add(file);
                    }
                }

                if (allFiles.Count > 0)
                {
                    await this.Dispatcher.RunAsync(() =>
                    {
                        this.Message = "Moving cache files...";
                        this.TotalFiles = allFiles.Count;
                        this.FilesMoved = 0;
                        this.IsCounterVisible = true;
                    });

                    foreach (var storageFile in allFiles)
                    {
                        var toFile =
                            await
                                to.CreateFolderAsync(storageFile.Name.Substring(0, 1), CreationCollisionOption.OpenIfExists);
                        await storageFile.MoveAsync(toFile);

                        await this.Dispatcher.RunAsync(() =>
                        {
                            this.FilesMoved++;
                        });
                    }
                }

                await from.DeleteAsync(StorageDeleteOption.PermanentDelete);

                this.settingsService.SetIsMusicLibraryForCache(this.isMovingToMusicLibrary);

                this.analyticsService.SendEvent("Settings", "ChangeIsMusicLibraryForCache", this.isMovingToMusicLibrary.ToString());

                this.songsCachingService.StartDownloadTask();

                this.View.Close();
            }
        }
    }
}
