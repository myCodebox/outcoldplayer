// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.WebServices;

    public class ProgressLoadingPresenter : ViewPresenterBase<IView>
    {
        private readonly ISongsService songsService;
        private readonly IClientLoginService loginService;
        private readonly INavigationService navigationService;

        public ProgressLoadingPresenter(
            IDependencyResolverContainer container, 
            IView view,
            ISongsService songsService,
            IClientLoginService loginService,
            INavigationService navigationService)
            : base(container, view)
        {
            this.songsService = songsService;
            this.loginService = loginService;
            this.navigationService = navigationService;
            this.BindingModel = new ProgressLoadingBindingModel();
        }

        public ProgressLoadingBindingModel BindingModel { get; private set; }

        public override void OnNavigatedTo(object parameter)
        {
            base.OnNavigatedTo(parameter);

            this.LoadSongs();
        }

        public void LoadSongs()
        {
            this.BindingModel.Progress = 0;
            this.BindingModel.Message = "Initializing...";
            this.BindingModel.IsFailed = false;

            this.loginService.GetStatusAsync().ContinueWith(
                tStatus =>
                {
                    if (tStatus.IsCompleted)
                    {
                        this.BindingModel.Maximum = tStatus.Result.AvailableTracks;
                        this.BindingModel.Message = "Loading playlists...";

                        this.songsService.GetAllPlaylistsAsync().ContinueWith(
                            tPlaylists =>
                            {
                                if (tStatus.IsCompleted)
                                {
                                    this.BindingModel.Message = "Loading songs...";
                                    Progress<int> progress = new Progress<int>();
                                    progress.ProgressChanged += (sender, i) =>
                                    {
                                        this.BindingModel.Progress = i;
                                    };

                                    this.songsService.GetAllGoogleSongsAsync(progress).ContinueWith(
                                        tSongs =>
                                        {
                                            if (tSongs.IsCompleted)
                                            {
                                                this.navigationService.NavigateTo<IStartView>();
                                            }
                                            else
                                            {
                                                this.BindingModel.Message = "Cannot load data...";
                                                this.BindingModel.IsFailed = true;
                                            }
                                        },
                                        TaskScheduler.FromCurrentSynchronizationContext());
                                }
                                else
                                {
                                    this.BindingModel.Message = "Cannot load data...";
                                    this.BindingModel.IsFailed = true;
                                }
                            },
                            TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    else
                    {
                        this.BindingModel.Message = "Cannot load data...";
                        this.BindingModel.IsFailed = true;
                    }
                },
                TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}