// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Storage;

    public class WindowsStorageFolder : IFolder
    {
        public WindowsStorageFolder(StorageFolder folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }

            this.Folder = folder;
        }

        public StorageFolder Folder { get; set; }

        public string Path
        {
            get { return this.Folder.Path; }
            
        }

        public async Task<IFile> GetFileAsync(string fileName)
        {
            return new WindowsStorageFile(await this.Folder.GetFileAsync(fileName));
        }

        public async Task<IList<IFolder>> GetFoldersAsync()
        {
            return (await this.Folder.GetFoldersAsync()).Select(x => new WindowsStorageFolder(x)).Cast<IFolder>().ToList();
        }

        public async Task<IList<IFile>> GetFilesAsync()
        {
            return (await this.Folder.GetFilesAsync()).Select(x => new WindowsStorageFile(x)).Cast<IFile>().ToList();
        }

        public async Task<IFolder> CreateOrOpenFolderAsync(string folderName)
        {
            return new WindowsStorageFolder(await this.Folder.CreateFolderAsync(folderName, Windows.Storage.CreationCollisionOption.OpenIfExists));
        }

        public async Task<IFile> CreateFileAsync(string fileName, CreationCollisionOption options = CreationCollisionOption.ReplaceExisting)
        {
            return new WindowsStorageFile(await this.Folder.CreateFileAsync(fileName, (Windows.Storage.CreationCollisionOption) options));
        }

        public async Task<bool> ExistsAsync(string fileName)
        {
            return (await ApplicationData.Current.LocalFolder.GetFilesAsync())
               .Any(f => string.Equals(f.Name, fileName, StringComparison.OrdinalIgnoreCase));
        }

        public Task DeleteAsync()
        {
            return this.Folder.DeleteAsync(StorageDeleteOption.PermanentDelete).AsTask();
        }
    }
}
