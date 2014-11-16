// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
using System;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using OutcoldSolutions.GoogleMusic.InversionOfControl;
using OutcoldSolutions.GoogleMusic.EventAggregator;
using OutcoldSolutions.GoogleMusic.Diagnostics;
using OutcoldSolutions.GoogleMusic.Services;
using OutcoldSolutions.GoogleMusic.Web;
using OutcoldSolutions.GoogleMusic.Web.Lastfm;
using OutcoldSolutions.GoogleMusic.Services.Publishers;
using OutcoldSolutions.GoogleMusic.Repositories;
using OutcoldSolutions.GoogleMusic.Models;
using OutcoldSolutions.GoogleMusic.Web.Synchronization;
using OutcoldSolutions.GoogleMusic.Services.Actions;
using OutcoldSolutions.GoogleMusic.Shell;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutcoldSolutions.GoogleMusic
{
	public class ApplicationResources : IApplicationResources
	{
		#region IApplicationResources implementation

		public string GetString(string name)
		{
			return name;
		}

		#endregion


	}

	public class SecurityStorageProvider : ISecureStorageService
	{
		private IDictionary<string, Tuple<string, string>> storage = new Dictionary<string, Tuple<string, string>>();

		#region ISecureStorageService implementation

		public void Save(string resource, string username, string password)
		{
			this.storage[resource] = Tuple.Create(username, password);
		}

		public bool Get(string resource, out string username)
		{
			username = null;
			Tuple<string, string> value;
			if (this.storage.TryGetValue(resource, out value))
			{
				username = value.Item1;
				return true;
			}
			return false;
		}

		public bool Get(string resource, out string username, out string password)
		{
			username = null;
			password = null;
			Tuple<string, string> value;
			if (this.storage.TryGetValue(resource, out value))
			{
				username = value.Item1;
				password = value.Item2;
				return true;
			}
			return false;
		}

		public void Delete(string resource)
		{
			this.storage.Remove(resource);
		}

		#endregion


	}

	public class DataProtectService : IDataProtectService
	{
		#region IDataProtectService implementation

		public Task<string> ProtectStringAsync(string unprotectedString)
		{
			return Task.Run(() => unprotectedString);
		}

		public Task<string> UnprotectStringAsync(string protectedString)
		{
			return Task.Run(() => protectedString);
		}

		public byte[] GetMd5Hash(string content)
		{
			throw new NotImplementedException();
		}

		public string GetMd5HashStringAsBase64(string content)
		{
			throw new NotImplementedException();
		}

		public string GetHMacStringAsBase64(string key, string value)
		{
			throw new NotImplementedException();
		}

		#endregion


	}

	public class SettingsService : ISettingsService
	{
		private IDictionary<string, IDictionary<string, object>> storage = new Dictionary<string, IDictionary<string, object>>();

		#region ISettingsService implementation

		public void SetApplicationValue<T>(string key, T value)
		{
			this.SetValue("Application", key, value);
		}

		public T GetApplicationValue<T>(string key, T defaultValue = default(T))
		{
			return this.GetValue("Application", key, defaultValue);
		}

		public void RemoveApplicationValue(string key)
		{
			this.RemoveValue("Application", key);
		}

		public void SetValue<T>(string containerName, string key, T value)
		{
			if (!this.storage.ContainsKey(containerName))
			{
				this.storage[containerName] = new Dictionary<string, object>();
			}

			this.storage[containerName][key] = (object)value;
		}

		public T GetValue<T>(string containerName, string key, T defaultValue = default(T))
		{
			if (!this.storage.ContainsKey(containerName) || !this.storage[containerName].ContainsKey(key))
			{
				return defaultValue;
			}

			return (T)this.storage[containerName][key];
		}

		public bool TryGetValue<T>(string containerName, string key, out T value)
		{
			if (!this.storage.ContainsKey(containerName) || !this.storage[containerName].ContainsKey(key))
			{
				value = default(T);
				return false;
			}

			value = (T)this.storage[containerName][key];
			return true;
		}

		public void RemoveValue(string containerName, string key)
		{
			if (this.storage.ContainsKey(containerName))
			{
				this.storage[containerName].Remove(key);
			}
		}

		public void SetRoamingValue<T>(string key, T value)
		{
			this.SetValue("ApplicationRoamin", key, value);
		}

		public T GetRoamingValue<T>(string key, T defaultValue = default(T))
		{
			return this.GetValue("ApplicationRoamin", key, defaultValue);
		}

		public void RemoveRoamingValue(string key)
		{
			this.RemoveValue("ApplicationRoamin", key);
		}

		#endregion


	}

	public class MediaDownloadService : IMediaStreamDownloadService
	{
		#region IMediaStreamDownloadService implementation

		public Task<INetworkRandomAccessStream> GetStreamAsync(string url, System.Threading.CancellationToken token)
		{
			throw new NotImplementedException();
		}

		public Task<INetworkRandomAccessStream> GetStreamAsync(string[] urls, System.Threading.CancellationToken token)
		{
			throw new NotImplementedException();
		}

		public Task<IStream> GetCachedStreamAsync(IFile storageFile, System.Threading.CancellationToken token)
		{
			throw new NotImplementedException();
		}

		#endregion


	}

	public class NotificationService : INotificationService
	{
		#region INotificationService implementation

		public Task ShowMessageAsync(string message)
		{
			return Task.Run(() => {
				Console.WriteLine(message);
			});
		}

		public Task<bool?> ShowQuestionAsync(string question, Action yesAction = null, Action noAction = null, Action cancelAction = null, string yesButton = null, string noButton = null, string cancelButton = null)
		{
			throw new NotImplementedException();
		}

		#endregion


	}


	public partial class AppDelegate : NSApplicationDelegate
	{
		MainWindowController mainWindowController;

		public AppDelegate()
		{
		}

		public override void FinishedLaunching(NSObject notification)
		{
			if (ApplicationContext.ApplicationLocalFolder == null)
			{
				ApplicationContext.ApplicationLocalFolder = new StorageFolder(Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Library", "outcoldplayer"));
				if (!Directory.Exists(ApplicationContext.ApplicationLocalFolder.Path))
				{
					Directory.CreateDirectory(ApplicationContext.ApplicationLocalFolder.Path);
				}
			}

			if (ApplicationContext.Container == null)
			{
				ApplicationContext.ApplicationVersion = new Version(1, 0, 0, 0);

				ApplicationContext.Container = new DependencyResolverContainer();

				using (var registration = ApplicationContext.Container.Registration())
				{
					registration.Register<IEventAggregator>().AsSingleton<EventAggregator.EventAggregator>();
					registration.Register<ILogManager>().AsSingleton<LogManager>();
					registration.Register<INavigationService>().AsSingleton<NavigationService>();
					registration.Register<IApplicationResources>().AsSingleton<ApplicationResources>();
					registration.Register<ISecureStorageService>().AsSingleton<SecurityStorageProvider>();
					registration.Register<IDataProtectService>().AsSingleton<DataProtectService>();
					registration.Register<ISettingsService>().AsSingleton<SettingsService>();
					registration.Register<IMediaStreamDownloadService>().AsSingleton<MediaDownloadService>();
					registration.Register<INotificationService>().AsSingleton<NotificationService>();

#if DEBUG
					registration.Register<IDebugConsole>().AsSingleton<DebugConsole>();
#endif
					// Services
					//registration.Register<IDataProtectService>().AsSingleton<DataProtectService>();
					registration.Register<IGoogleAccountWebService>().As<GoogleAccountWebService>();
					registration.Register<IGoogleMusicWebService>().AsSingleton<GoogleMusicWebService>();
					registration.Register<IGoogleMusicApisService>().AsSingleton<GoogleMusicApisService>();
					registration.Register<IGoogleAccountService>().AsSingleton<GoogleAccountService>();
					registration.Register<IAuthentificationService>().As<AuthentificationService>();
					registration.Register<IPlaylistsWebService>().As<PlaylistsWebService>();
					registration.Register<ISongsWebService>().AsSingleton<SongsWebService>();
					registration.Register<IRadioWebService>().AsSingleton<RadioWebService>();
					registration.Register<IAllAccessWebService>().AsSingleton<AllAccessWebService>();
					registration.Register<IAllAccessService>().AsSingleton<AllAccessService>();
					//registration.Register<ISettingsService>().AsSingleton<SettingsService>();
					registration.Register<IGoogleMusicSessionService>().AsSingleton<GoogleMusicSessionService>();
					registration.Register<IConfigWebService>().AsSingleton<ConfigWebService>();

					//registration.Register<IMediaStreamDownloadService>().AsSingleton<MediaStreamDownloadService>();

					registration.Register<ILastfmWebService>().AsSingleton<LastfmWebService>();
					registration.Register<ILastfmAccountWebService>().As<LastfmAccountWebService>();

					// Publishers
					registration.Register<ICurrentSongPublisherService>().AsSingleton<CurrentSongPublisherService>();
					registration.Register<GoogleMusicCurrentSongPublisher>().AsSingleton();
					//registration.Register<MediaControlCurrentSongPublisher>().AsSingleton();
					//registration.Register<TileCurrentSongPublisher>().AsSingleton();
					registration.Register<LastFmCurrentSongPublisher>().AsSingleton();

					// Songs Repositories and Services
					registration.Register<ICachedSongsRepository>().AsSingleton<CachedSongsRepository>();
					registration.Register<ISongsRepository>().AsSingleton<SongsRepository>();
					registration.Register<IUserPlaylistsRepository>().And<IPlaylistRepository<UserPlaylist>>().AsSingleton<UserPlaylistsRepository>();
					registration.Register<IArtistsRepository>().And<IPlaylistRepository<Artist>>().AsSingleton<ArtistsRepository>();
					registration.Register<IAlbumsRepository>().And<IPlaylistRepository<Album>>().AsSingleton<AlbumsRepository>();
					registration.Register<IGenresRepository>().And<IPlaylistRepository<Genre>>().AsSingleton<GenresRepository>();
					registration.Register<ISystemPlaylistsRepository>().And<IPlaylistRepository<SystemPlaylist>>().AsSingleton<SystemPlaylistsRepository>();
					registration.Register<IPlaylistsService>().AsSingleton<PlaylistsService>();
					registration.Register<IUserPlaylistsService>().AsSingleton<UserPlaylistsService>();
					registration.Register<IAlbumArtCacheService>().AsSingleton<AlbumArtCacheService>();
					registration.Register<ICachedAlbumArtsRepository>().AsSingleton<CachedAlbumArtsRepository>();
					registration.Register<IRadioStationsRepository>().And<IPlaylistRepository<Radio>>().AsSingleton<RadioStationsRepository>();
					registration.Register<IRadioStationsService>().AsSingleton<RadioStationsService>();

					registration.Register<IInitialSynchronization>().As<InitialSynchronization>();

					registration.Register<ISongsService>().AsSingleton<SongsService>();

					//registration.Register<ApplicationLogManager>().AsSingleton();

					registration.Register<ISongsCachingService>().AsSingleton<SongsCachingService>();

					registration.Register<IApplicationStateService>().AsSingleton<ApplicationStateService>();

//					registration.Register<MediaElement>()
//						.AsSingleton(
//							new MediaElement()
//							{
//								IsLooping = false,
//								AutoPlay = true,
//								AudioCategory = AudioCategory.BackgroundCapableMedia
//							});

//					registration.Register<IMediaElementContainer>()
//						.AsSingleton<MediaElementContainer>();

					registration.Register<IPlayQueueService>()
						.AsSingleton<PlayQueueService>();

//					registration.Register<INotificationService>()
//						.AsSingleton<NotificationService>();
//
//					registration.Register<IMediaControlIntegration>()
//						.AsSingleton<MediaControlIntegration>();

					registration.Register<IGoogleMusicSynchronizationService>()
						.AsSingleton<GoogleMusicSynchronizationService>();

//					registration.Register<ScreenLocker>();
					registration.Register<ApplicationStateChangeHandler>();

					// Actions
					registration.Register<ISelectedObjectsService>()
						.AsSingleton<SelectedObjectsService>();

					registration.Register<QueueAction>().AsSingleton();
					registration.Register<StartRadioAction>().AsSingleton();
					registration.Register<AddToPlaylistAction>().AsSingleton();
					registration.Register<EditPlaylistAction>().AsSingleton();
					registration.Register<DownloadAction>().AsSingleton();
					registration.Register<RemoveLocalAction>().AsSingleton();
					registration.Register<RemoveFromPlaylistAction>().AsSingleton();
					registration.Register<RemoveSelectedSongAction>().AsSingleton();
					registration.Register<DeletePlaylistAction>().AsSingleton();
					registration.Register<DeleteRadioStationsAction>().AsSingleton();
					registration.Register<AddToLibraryAction>().AsSingleton();
					registration.Register<RemoveFromLibraryAction>().AsSingleton();
//					registration.Register<PinToStartAction>().AsSingleton();

//					registration.Register<ApplicationSize>().AsSingleton(this.Resources["ApplicationSize"]);

					registration.Register<AskForReviewService>().AsSingleton();

					registration.Register<IRatingCacheService>().AsSingleton<RatingCacheService>();

#if DEBUG
					registration.Register<IAnalyticsService>().AsSingleton<FakeAnalyticsService>();
#else
					registration.Register<IAnalyticsService>().AsSingleton<AnalyticsService>();
#endif
				}

				ApplicationContext.Container.Resolve<ILogManager>().Writers.AddOrUpdate(typeof(DebugLogWriter), type => new DebugLogWriter(ApplicationContext.Container), (type, writer) => writer);
				ApplicationContext.Container.Resolve<ILogManager>().Writers.AddOrUpdate(typeof(FileLogWriter), type => new FileLogWriter(), (type, writer) => writer);


			}


			mainWindowController = new MainWindowController();
			mainWindowController.Window.MakeKeyAndOrderFront(this);


		}
	}
}

