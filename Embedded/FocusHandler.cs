namespace Chromium.Embedded
{
	using System;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class FocusHandler: CefFocusHandler
	{
		private static readonly Logger Log;

		static FocusHandler()
		{
			Log = LogManager.GetCurrentClassLogger();
		}
		
		private readonly App App;

		private readonly Client Client;

		public FocusHandler( App app, Client client )
		{
			this.App = app;
			this.Client = client;
		}

		protected override void OnGotFocus( CefBrowser browser )
		{
			Log.Trace( "FocusHandler.OnGotFocus( browser: {0} )", browser.Identifier );
			base.OnGotFocus( browser );
		}

		protected override Boolean OnSetFocus( CefBrowser browser, CefFocusSource source )
		{
			Log.Trace( "FocusHandler.OnSetFocus( browser: {0}, source: {1} )", browser.Identifier, Enum.GetName( typeof( CefFocusSource ), source ) );
			return base.OnSetFocus( browser, source );
		}

		protected override void OnTakeFocus( CefBrowser browser, Boolean next )
		{
			Log.Trace( "FocusHandler.OnTakeFocus( browser: {0}, next: {1} )", browser.Identifier, next );
			base.OnTakeFocus( browser, next );
		}
	}
}
