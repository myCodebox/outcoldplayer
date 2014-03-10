// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IGoogleMusicApisService
    {
        Task<TResult> GetAsync<TResult>(
            string url,
            CancellationToken? cancellationToken = null);

        Task<TResult> PostAsync<TResult>(
            string url,
            dynamic json = null,
            bool signUrl = false,
            CancellationToken? cancellationToken = null);

        Task<IList<TData>> DownloadList<TData>(
            string url,
            DateTime? lastUpdate = null,
            IProgress<int> progress = null,
            Func<IList<TData>, Task> chunkHandler = null);
    }
}
