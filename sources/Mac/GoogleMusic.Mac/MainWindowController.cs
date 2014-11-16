// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using OutcoldSolutions.GoogleMusic.Views;
using OutcoldSolutions.GoogleMusic.Services;
using OutcoldSolutions.GoogleMusic.Models;
using OutcoldSolutions.GoogleMusic.Web.Synchronization;
using OutcoldSolutions.GoogleMusic.Repositories;

namespace OutcoldSolutions.GoogleMusic
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController, IMainFrame 
	{
		#region Constructors

		// Called when created from unmanaged code
		public MainWindowController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MainWindowController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public MainWindowController () : base ("MainWindow")
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{

		}

		#endregion

		//strongly typed window accessor
		public new MainWindow Window {
			get {
				return (MainWindow)base.Window;
			}
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			this.btnLogin.Activated += this.LoginClick;
		}

		private async void LoginClick(object sender, EventArgs e)
		{
			String value = this.txtLogin.StringValue;
			String password = this.txtPassword.StringValue;

			var container = ApplicationContext.Container;
			var authenticationService = container.Resolve<IAuthentificationService>();
			var userInfo = new UserInfo(value, password);

			var result = await authenticationService.CheckAuthentificationAsync(userInfo);

			DbContext dbContext = new DbContext();
			await dbContext.InitializeAsync();

			var sync = container.Resolve<IInitialSynchronization>();
			await sync.InitializeAsync();

			Console.WriteLine("Done!");
		}

		#region IMainFrame implementation

		public bool IsCurretView(IPageView view)
		{
			throw new NotImplementedException();
		}

		public void SetViewCommands(IEnumerable<CommandMetadata> commands)
		{
			throw new NotImplementedException();
		}

		public void ClearViewCommands()
		{
			throw new NotImplementedException();
		}

		public void SetContextCommands(IEnumerable<CommandMetadata> commands)
		{
			throw new NotImplementedException();
		}

		public void ClearContextCommands()
		{
			throw new NotImplementedException();
		}

		public TPopup ShowPopup<TPopup>(PopupRegion popupRegion, params object[] injections) where TPopup : IPopupView
		{
			throw new NotImplementedException();
		}

		public void ShowMessage(string text)
		{
			throw new NotImplementedException();
		}

		public object GetRectForSecondaryTileRequest()
		{
			throw new NotImplementedException();
		}

		public string Title {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public string Subtitle {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		#endregion

		#region IView implementation

		public TPresenter GetPresenter<TPresenter>()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}

