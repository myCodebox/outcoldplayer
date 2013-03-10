namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Threading.Tasks;

    public interface IGoogleMusicSynchronizationService
    {
        Task InitializeAsync(IProgress<double> progress);

        Task RefreshAsync(IProgress<double> progress);

        Task SynchronizeAsync(IProgress<double> progress);

        Task ClearLocalDatabaseAsync();
    }
}