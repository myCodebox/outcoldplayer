// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.EventAggregator;
    using OutcoldSolutions.GoogleMusic.Services.Actions;
    using OutcoldSolutions.GoogleMusic.Views;

    public interface ISelectedObjectsService
    {
        void ClearSelection();

        void AddActions(IEnumerable<ISelectedObjectAction> actions);

        void Update(IEnumerable<object> newItems, IEnumerable<object> oldItems);

        bool HasSelectedObjects();
    }

    public interface ISelectedObjectAction
    {
        string Icon { get; }

        string Title { get; }

        ActionGroup Group { get; }

        int Priority { get; }

        bool CanExecute(IList<object> selectedObjects);

        Task<bool?> Execute(IList<object> selectedObjects);
    }

    public class SelectionClearedEvent
    {
    }

    public class SelectedObjectsService : ISelectedObjectsService
    {
        private readonly INavigationService navigationService;
        private readonly IMainFrame mainFrame;
        private readonly IEventAggregator eventAggregator;

        private readonly INotificationService notificationService;

        private readonly List<object> selectedObjects = new List<object>();
        private readonly List<SelectedObjectActionContainer> objectActions = new List<SelectedObjectActionContainer>();

        private readonly ILogger logger;

        private bool isBusy = false;

        private class SelectedObjectActionContainer
        {
            public DelegateCommand Command { get; set; }

            public ISelectedObjectAction Action { get; set; }
        }

        public SelectedObjectsService(
            INavigationService navigationService,
            IMainFrame mainFrame,
            IEventAggregator eventAggregator,
            INotificationService notificationService,
            ILogManager logManager)
        {
            this.navigationService = navigationService;
            this.mainFrame = mainFrame;
            this.eventAggregator = eventAggregator;
            this.notificationService = notificationService;
            this.navigationService.NavigatedTo += this.OnNavigatedTo;
            this.logger = logManager.CreateLogger("SelectedObjectsService");
        }

        public void AddActions(IEnumerable<ISelectedObjectAction> actions)
        {
            var actionObjects = actions.Select(x => new SelectedObjectActionContainer
                    {
                        Action = x,
                        Command = new DelegateCommand(
                            async () =>
                            {
                                if (!this.isBusy)
                                {
                                    this.isBusy = true;
                                    this.UpdateCommandsStatus();

                                    bool failed = true;

                                    try
                                    {
                                        var result = await x.Execute(this.selectedObjects);
                                        if (result.HasValue)
                                        {
                                            failed = !result.Value;
                                            if (result.Value)
                                            {
                                                this.ClearSelection();
                                                this.navigationService.RefreshCurrentView();
                                            }
                                        }
                                        else
                                        {
                                            failed = false;
                                        }
                                    }
                                    catch (Exception exception)
                                    {
                                        this.logger.Error(exception, "Could not execute action {0} - {1}", x.Title, x.Icon);
                                    }

                                    if (failed)
                                    {
                                        await this.notificationService
                                            .ShowMessageAsync("Could not execute selected action. Please try again.");
                                    }

                                    this.isBusy = false;
                                    this.UpdateCommandsStatus();
                                }
                            },
                            () => !this.isBusy)
                    });
            this.objectActions.AddRange(actionObjects);
        }

        public void Update(IEnumerable<object> newItems, IEnumerable<object> oldItems)
        {
            bool updated = false;

            if (this.selectedObjects.Count > 0 && oldItems != null)
            {
                foreach (var oldItem in oldItems)
                {
                    updated = true;
                    this.selectedObjects.Remove(oldItem);
                }
            }

            if (newItems != null)
            {
                foreach (var newItem in newItems)
                {
                    updated = true;
                    this.selectedObjects.Add(newItem);
                }
            }

            if (updated)
            {
                this.UpdateContextCommands();
            }
        }

        public bool HasSelectedObjects()
        {
            return this.selectedObjects.Any();
        }

        public void ClearSelection()
        {
            this.selectedObjects.Clear();
            this.mainFrame.ClearContextCommands();
            this.eventAggregator.Publish(new SelectionClearedEvent());
        }

        private void UpdateContextCommands()
        {
            if (this.selectedObjects.Count > 0)
            {
                this.mainFrame.SetContextCommands(
                    this.objectActions.Where(x => x.Action.CanExecute(this.selectedObjects))
                        .OrderBy(x => x.Action.Group).ThenByDescending(x => x.Action.Priority)
                        .Select(x => new CommandMetadata(x.Action.Icon, x.Action.Title, x.Action.Group, x.Command)));
            }
            else
            {
                this.mainFrame.ClearContextCommands();
            }
        }

        private void UpdateCommandsStatus()
        {
            foreach (var action in this.objectActions)
            {
                action.Command.RaiseCanExecuteChanged();
            }
        }

        private void OnNavigatedTo(object sender, NavigatedToEventArgs navigatedToEventArgs)
        {
            this.ClearSelection();
        }
    }
}
