// // --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace OutcoldSolutions.GoogleMusic
{
	public class StorageFolder : IFolder
	{
		private string folder;

		public StorageFolder(string folder)
		{
			this.folder = folder;
		}

		public Task<IFile> GetFileAsync(string fileName)
		{
			return Task.Run(() => 
				{
					if (File.Exists(System.IO.Path.Combine(this.Path, fileName)))
					{
						return (IFile)new StorageFile(this.Path, fileName);
					}

					return null;
				});
		}

		public Task<IList<IFolder>> GetFoldersAsync()
		{
			return Task.Run(() => 
				{
					return (IList<IFolder>)Directory.GetDirectories(this.Path).Select(x => new StorageFolder(x)).Cast<IFolder>().ToList();
				});
		}

		public Task<System.Collections.Generic.IList<IFile>> GetFilesAsync()
		{
			return Task.Run(() => 
				{
					return (IList<IFile>)Directory.GetFiles(this.Path).Select(x => new StorageFile(this.Path, x)).Cast<IFile>().ToList();
				});
		}

		public Task<IFolder> CreateOrOpenFolderAsync(string folderName)
		{
			return Task.Run(() => 
				{
					string fullPath = System.IO.Path.Combine(this.Path, folderName);
					if (!Directory.Exists(fullPath))
					{
						Directory.CreateDirectory(fullPath);
					}

					return (IFolder)new StorageFolder(fullPath);
				});
		}

		public Task<IFile> CreateFileAsync(string fileName, CreationCollisionOption options = CreationCollisionOption.ReplaceExisting)
		{
			return Task.Run(() => 
				{
					string fullPath = System.IO.Path.Combine(this.Path, fileName);

					if (File.Exists(fullPath))
					{
						if (options == CreationCollisionOption.ReplaceExisting)
						{
							File.Delete(fullPath);
							using(File.Create(fullPath));
						}
					}
					else
					{
						using(File.Create(fullPath));
					}

					return (IFile)new StorageFile(this.Path, fileName);
				});
		}

		public Task<bool> ExistsAsync(string fileName)
		{
			return Task.Run(() => 
				{
					string fullPath = System.IO.Path.Combine(this.Path, fileName);

					return File.Exists(fullPath);
				});
		}

		public Task DeleteAsync()
		{
			return Task.Run(() => 
				{
					Directory.Delete(this.Path, recursive: true);
				});
		}

		public string Path {
			get {
				return this.folder;
			}
		}

		public string Name {
			get {
				return System.IO.Path.GetDirectoryName(this.folder);
			}
		}
	}
}

