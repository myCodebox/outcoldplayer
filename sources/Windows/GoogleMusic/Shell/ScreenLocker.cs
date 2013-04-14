// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System;
    using System.Reactive.Linq;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;

    using Windows.System.Display;

    public class ScreenLocker
    {
        private readonly ILogger logger;
        private readonly IDispatcher dispatcher;
        private readonly ISettingsService settingsService;

        private readonly Lazy<DisplayRequest> displayRequest = new Lazy<DisplayRequest>();
        private bool isRequestActive = false;

        public ScreenLocker(
            ILogManager logManager,
            IDispatcher dispatcher,
            IEventAggregator eventAggregator,
            ISettingsService settingsService)
        {
            this.logger = logManager.CreateLogger("ScreenLocker");
            this.dispatcher = dispatcher;
            this.settingsService = settingsService;

            eventAggregator.GetEvent<SettingsChangeEvent>()
                           .Where(e => e.Key == GoogleMusicSettingsServiceExtensions.LockScreenEnabledKey)
                           .Subscribe(e => this.UpdateDisplayRequest());

            this.UpdateDisplayRequest();
        }

        private async void UpdateDisplayRequest()
        {
            await this.dispatcher.RunAsync(
                () =>
                    {
                        bool isLockScreenEnabled = this.settingsService.GetIsLockScreenEnabled();

                        try
                        {
                            if (isLockScreenEnabled)
                            {
                                if (!this.isRequestActive)
                                {
                                    this.displayRequest.Value.RequestActive();
                                    this.isRequestActive = true;
                                }
                            }
                            else
                            {
                                if (this.isRequestActive)
                                {
                                    this.displayRequest.Value.RequestRelease();
                                    this.isRequestActive = false;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            this.logger.Error(e, "Exception while tried to change screen request.");
                        }
                    });
        }
    }
}