// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using Windows.Storage;

    public class WindowsStoreFolder : IFolder
    {
        public WindowsStoreFolder(StorageFolder folder)
        {
            this.Folder = folder;
        }

        public StorageFolder Folder { get; set; }
    }
}
