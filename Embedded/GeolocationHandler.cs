namespace Chromium.Embedded
{
	using System;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class GeolocationHandler: CefGeolocationHandler
	{
		private static readonly Logger Log;

		static GeolocationHandler()
		{
			Log = LogManager.GetCurrentClassLogger();
		}
		
		private readonly App App;

		private readonly Client Client;

		public GeolocationHandler( App app, Client client )
		{
			this.App = app;
			this.Client = client;
		}
		
		protected override void OnRequestGeolocationPermission( CefBrowser browser, String requestingUrl, Int32 requestId, CefGeolocationCallback callback )
		{
			Log.Trace( "GeolocationHandler.OnRequestGeolocationPermission( browser: {0}, requestingUrl: {1}, requestId: {2} )",
				browser.Identifier, requestingUrl, requestId );
			callback.Continue( false );
		}

		protected override void OnCancelGeolocationPermission( CefBrowser browser, String requestingUrl, Int32 requestId )
		{
			Log.Trace( "GeolocationHandler.OnCancelGeolocationPermission( browser: {0}, requestingUrl: {1}, requestId: {2} )",
				browser.Identifier, requestingUrl, requestId );
		}
	}
}
