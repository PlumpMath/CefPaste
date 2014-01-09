namespace Chromium.Embedded
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class LoadHandler: CefLoadHandler
	{
		private static readonly Logger Log;

		static LoadHandler()
		{
			Log = LogManager.GetCurrentClassLogger();
		}
		
		private readonly App App;

		private readonly Client Client;

		public LoadHandler( App app, Client client )
		{
			this.App = app;
			this.Client = client;
		}

		protected override void OnLoadStart( CefBrowser browser, CefFrame frame )
		{
			Log.Trace( "LoadHandler.OnLoadStart( browser: {0}, frame: {1} )", browser.Identifier, frame.Identifier );
			base.OnLoadStart( browser, frame );
		}

		protected override void OnLoadEnd( CefBrowser browser, CefFrame frame, Int32 httpStatusCode )
		{
			Log.Trace( "LoadHandler.OnLoadEnd( browser: {0}, frame: {1}, httpStatusCode: {2} )",
				browser.Identifier,
				frame.Identifier,
				httpStatusCode );
			base.OnLoadEnd( browser, frame, httpStatusCode );
		}

		protected override void OnLoadError( CefBrowser browser, CefFrame frame, CefErrorCode errorCode, String errorText, String failedUrl )
		{
			Log.Trace( "LoadHandler.OnLoadError( browser: {0}, frame: {1}, errorText: {2}, failedUrl: {3} )",
				browser.Identifier,
				frame.Identifier,
				errorText,
				failedUrl );
			base.OnLoadError( browser, frame, errorCode, errorText, failedUrl );
		}

		protected override void OnLoadingStateChange( CefBrowser browser, Boolean isLoading, Boolean canGoBack, Boolean canGoForward )
		{
			Log.Trace( "LoadHandler.OnLoadingStateChange( browser: {0}, isLoading: {1}, canGoBack: {2}, canGoForward: {3} )", browser.Identifier, isLoading, canGoBack, canGoForward );

			if( isLoading && this.Client.HandleLoadStarted != null )
			{
				if( this.Client.Browser != null ) this.Client.HandleLoadStarted();
			}

			if( isLoading == false && this.Client.HandleLoadFinished != null )
			{
				if( this.Client.Browser != null ) this.Client.HandleLoadFinished();
			}

			base.OnLoadingStateChange( browser, isLoading, canGoBack, canGoForward );
		}
	}
}
