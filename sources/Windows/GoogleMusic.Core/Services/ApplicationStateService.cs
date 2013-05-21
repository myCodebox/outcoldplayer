// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;

    public enum ApplicationState
    {
        Unknown = 0,

        Online = 1,

        Offline = 2
    }

    public class ApplicationStateChangeEvent
    {
        public ApplicationStateChangeEvent(ApplicationState currentState)
        {
            this.CurrentState = currentState;
        }

        public ApplicationState CurrentState { get; set; }
    }

    public interface IApplicationStateService
    {
        ApplicationState CurrentState { get; set; }
    }

    public static class ApplicationStateServiceExtensions
    {
        public static bool IsOffline(this IApplicationStateService @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            return @this.CurrentState == ApplicationState.Offline;
        }

        public static bool IsOnline(this IApplicationStateService @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            return @this.CurrentState == ApplicationState.Online;
        }
    }

    public class ApplicationStateService : IApplicationStateService
    {
        private const string LastApplicationStateKey = "LastApplicationState";

        private readonly ISettingsService settingsService;
        private readonly IEventAggregator eventAggregator;

        private ApplicationState? applicationState;

        public ApplicationStateService(
            ISettingsService settingsService,
            IEventAggregator eventAggregator)
        {
            this.settingsService = settingsService;
            this.eventAggregator = eventAggregator;
        }

        public ApplicationState CurrentState
        {
            get
            {
                return this.applicationState.HasValue ? this.applicationState.Value : (ApplicationState)(this.applicationState = this.settingsService.GetValue(LastApplicationStateKey, ApplicationState.Online));
            }

            set
            {
                this.applicationState = value;
                this.settingsService.SetValue(LastApplicationStateKey, value);
                this.eventAggregator.Publish(new ApplicationStateChangeEvent(value));
            }
        }
    }
}