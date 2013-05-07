// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
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
            CurrentState = currentState;
        }

        public ApplicationState CurrentState { get; set; }
    }

    public interface IApplicationStateService
    {
        ApplicationState CurrentState { get; set; }
    }

    public class ApplicationStateService : IApplicationStateService
    {
        private const string LastApplicationStateKey = "LastApplicationState";

        private readonly ISettingsService settingsService;
        private readonly IEventAggregator eventAggregator;

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
                return this.settingsService.GetValue(LastApplicationStateKey, ApplicationState.Online);
            }

            set
            {
                this.settingsService.SetValue(LastApplicationStateKey, value);
                this.eventAggregator.Publish(new ApplicationStateChangeEvent(value));
            }
        }
    }
}