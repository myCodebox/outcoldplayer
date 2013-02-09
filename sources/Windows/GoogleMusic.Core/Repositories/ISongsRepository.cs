// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface ISongsRepository
    {
        event Action Updated;

        Task InitializeAsync(IProgress<int> progress);

        IEnumerable<Song> GetAll();

        Song AddOrUpdate(SongMetadata songInfo);

        void AddRange(IEnumerable<SongMetadata> songInfos);

        void Remove(Guid id);

        void Clear();
    }
}