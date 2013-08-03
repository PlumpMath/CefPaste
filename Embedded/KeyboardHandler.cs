namespace Chromium.Embedded
{
	using System;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class KeyboardHandler: CefKeyboardHandler
	{
		private static readonly Logger Log;

		static KeyboardHandler()
		{
			Log = LogManager.GetCurrentClassLogger();
		}
		
		private readonly App App;

		private readonly Client Client;

		public KeyboardHandler( App app, Client client )
		{
			this.App = app;
			this.Client = client;
		}

		protected override Boolean OnKeyEvent( CefBrowser browser, CefKeyEvent keyEvent, IntPtr osEvent )
		{
			Log.Trace( "KeyboardHandler.OnKeyEvent( browser: {0}, keyEvent: {1} )", browser.Identifier, Enum.GetName( typeof( CefKeyEventType ), keyEvent.EventType ) + ": " + keyEvent.Character );
			return base.OnKeyEvent( browser, keyEvent, osEvent );
		}

		protected override Boolean OnPreKeyEvent( CefBrowser browser, CefKeyEvent keyEvent, System.IntPtr os_event, out Boolean isKeyboardShortcut )
		{
			Log.Trace( "KeyboardHandler.OnPreKeyEvent( browser: {0}, keyEvent: {1} )", browser.Identifier, Enum.GetName( typeof( CefKeyEventType ), keyEvent.EventType ) + ": " + keyEvent.Character );
			return base.OnPreKeyEvent( browser, keyEvent, os_event, out isKeyboardShortcut );
		}
	}
}
