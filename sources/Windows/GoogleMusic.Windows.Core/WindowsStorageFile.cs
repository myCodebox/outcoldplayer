// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Storage;

    public class WindowsStorageFile : IFile
    {
        public WindowsStorageFile(StorageFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            this.File = file;
        }

        public StorageFile File { get; private set; }

        public string Name
        {
            get { return this.File.Name; }
        }

        public string Path
        {
            get { return this.File.Path; }
        }

        public async Task<ulong> GetSizeAsync()
        {
            var properties = await this.File.GetBasicPropertiesAsync();
            return properties.Size;
        }

        public Task DeleteAsync()
        {
            return this.File.DeleteAsync(StorageDeleteOption.PermanentDelete).AsTask();
        }

        public async Task MoveAsync(IFolder destination)
        {
            await this.File.MoveAsync(((WindowsStorageFolder)destination).Folder);
        }

        public async Task<Stream> OpenReadWriteAsync()
        {
            var stream = await this.File.OpenAsync(FileAccessMode.ReadWrite).AsTask();
            return stream.AsStream();
        }
    }
}
