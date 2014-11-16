// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;

namespace OutcoldSolutions.GoogleMusic
{
	public class StorageFile : IFile
	{
		public string folder;
		public string name;

		public StorageFile(string folder, string name)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}

			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			this.folder = folder;
			this.name = name;
		}

		public Task<ulong> GetSizeAsync()
		{
			return Task.Run(() => 
				{
					using (FileStream stream = File.OpenRead(this.Path))
					{
							return (ulong)stream.Length;
					}
				});
		}

		public Task DeleteAsync()
		{
			return Task.Run(() => 
				{
					File.Delete(this.Path);
				});
		}

		public Task MoveAsync(IFolder destination)
		{
			return Task.Run(() => 
				{
					File.Move(this.Path, destination.Path);
				});
		}

		public Task<Stream> OpenReadWriteAsync()
		{
			return Task.Run(() => 
				{
					return (Stream)File.OpenWrite(this.Path);
				});
		}

		public Task<Stream> OpenReadAsync()
		{
			return Task.Run(() => 
				{
					return (Stream)File.OpenRead(this.Path);
				});
		}

		public async Task WriteTextToFileAsync(string content)
		{
			using(FileStream stream = File.OpenWrite(this.Path))
			{
				using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
				{
					await writer.WriteAsync(content);
				}
			}
		}

		public async Task<string> ReadFileTextContentAsync()
		{
			using(FileStream stream = File.OpenRead(this.Path))
			{
				using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
				{
					return await reader.ReadToEndAsync();
				}
			}
		}

		public string Name 
		{
			get 
			{
				return this.name;
			}
		}

		public  string Path 
		{
			get 
			{
				return System.IO.Path.Combine(this.folder, this.name);
			}
		}
	}
}

