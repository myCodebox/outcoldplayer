// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Storage;
    using OutcoldSolutions.GoogleMusic.Repositories;

    public class ApplicationDbFile : IDbFile
    {
        public string Location
        {
            get { return ApplicationData.Current.LocalFolder.Path; }
        }

        public string FileName
        {
            get { return "db.sqlite"; }
        }

        public string FullPath
        {
            get
            {
                return Path.Combine(this.Location, this.FileName);
            }
        }

        public async Task<bool> Exists()
        {
            return (await ApplicationData.Current.LocalFolder.GetFilesAsync())
                .Any(f => string.Equals(f.Name, this.FileName));

        }

        public async Task Delete()
        {
            var dbFile = (await ApplicationData.Current.LocalFolder.GetFilesAsync())
                .FirstOrDefault(f => string.Equals(f.Name, this.FileName));

            if (dbFile != null)
            {
                await dbFile.DeleteAsync();
            }
        }
    }
}
