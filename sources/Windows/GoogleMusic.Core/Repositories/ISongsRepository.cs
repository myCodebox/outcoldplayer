// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface ISongsRepository
    {
        IEnumerable<Song> GetAll();

        Song AddOrUpdate(GoogleMusicSong songInfo);

        void AddRange(IEnumerable<GoogleMusicSong> songInfos);

        void Remove(Guid id);

        void Clear();
    }
}