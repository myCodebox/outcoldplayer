// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public enum CreationCollisionOption
    {
        ReplaceExisting = 1,
        FailIfExists = 2,
        OpenIfExists = 3
    }

    public interface IFolder
    {
        string Path { get; }

        Task<IFile> GetFileAsync(string fileName);

        Task<IList<IFolder>> GetFoldersAsync();

        Task<IList<IFile>> GetFilesAsync();

        Task<IFolder> CreateOrOpenFolderAsync(string folderName);

        Task<IFile> CreateFileAsync(string fileName, CreationCollisionOption options = CreationCollisionOption.ReplaceExisting);

        Task<bool> ExistsAsync(string fileName);

        Task DeleteAsync();
    }
}
