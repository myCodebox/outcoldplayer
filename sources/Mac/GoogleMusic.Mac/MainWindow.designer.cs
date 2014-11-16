// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace OutcoldSolutions.GoogleMusic
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSButton btnLogin { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField txtLogin { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField txtPassword { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (txtLogin != null) {
				txtLogin.Dispose ();
				txtLogin = null;
			}

			if (txtPassword != null) {
				txtPassword.Dispose ();
				txtPassword = null;
			}

			if (btnLogin != null) {
				btnLogin.Dispose ();
				btnLogin = null;
			}
		}
	}

	[Register ("MainWindow")]
	partial class MainWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
