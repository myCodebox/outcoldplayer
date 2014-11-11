// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using Windows.Storage;

    public class WindowsStoreFile : IFile
    {
        public WindowsStoreFile(StorageFile file)
        {
            this.File = file;
        }

        public StorageFile File { get; private set; }
    }
}
