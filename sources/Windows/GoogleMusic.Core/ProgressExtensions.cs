// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;

    public static class ProgressExtensions
    {
        public static void SafeReport<T>(this IProgress<T> progress, T value)
        {
            if (progress != null)
            {
                progress.Report(value);
            }
        }
    }
}
