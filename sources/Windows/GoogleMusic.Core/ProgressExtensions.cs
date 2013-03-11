// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Threading.Tasks;

    public static class ProgressExtensions
    {
        public static async Task SafeReportAsync<T>(this IProgress<T> progress, T value)
        {
            if (progress != null)
            {
                progress.Report(value);
                await Task.Yield();
            }
        }
    }
}
