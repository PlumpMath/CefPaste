namespace Chromium.Embedded
{
	using System;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class LifeSpanHandler : CefLifeSpanHandler
	{
		private static readonly Logger Log;

		static LifeSpanHandler()
		{
			Log = LogManager.GetCurrentClassLogger();
		}
		
		private readonly App App;

		private readonly Client Client;

		public LifeSpanHandler( App app, Client client )
		{
			this.App = app;
			this.Client = client;
		}

		protected override Boolean DoClose( CefBrowser browser )
		{
			Log.Trace( "LifeSpanHandler.DoClose( browser: {0} )", browser.Identifier );
			return base.DoClose( browser );
		}

		protected override void OnAfterCreated( CefBrowser browser )
		{
			this.Client.Browser = browser;

			this.Client.BrowserCreatedWaiter.Set();

			if( this.Client.HandleBrowserOpened != null )
			{
				this.Client.HandleBrowserOpened();
			}

			Log.Trace( "LifeSpanHandler.OnAfterCreated( browser: {0} )", browser.Identifier );
			//base.OnAfterCreated( browser );
		}

		protected override void OnBeforeClose( CefBrowser browser )
		{
			if( this.Client.HandleBrowserClosed != null )
			{
				this.Client.HandleBrowserClosed();
			}

			Log.Trace( "LifeSpanHandler.OnBeforeClose( browser: {0} )", browser.Identifier );
			//base.OnBeforeClose( browser );
		}

		protected override Boolean OnBeforePopup( CefBrowser browser, CefFrame frame, String targetUrl, String targetFrameName, CefPopupFeatures popupFeatures, CefWindowInfo windowInfo, ref CefClient client, CefBrowserSettings settings, ref Boolean noJavascriptAccess )
		{
			Log.Trace( "LifeSpanHandler.OnBeforePopup( browser: {0}, frame: {1}, targetUrl: {2}, targetFrameName: {3} )",
				browser.Identifier,
				frame.Identifier,
				targetUrl,
				targetFrameName );
			return base.OnBeforePopup( browser, frame, targetUrl, targetFrameName, popupFeatures, windowInfo, ref client, settings, ref noJavascriptAccess );
		}

		protected override Boolean RunModal( CefBrowser browser )
		{
			Log.Trace( "LifeSpanHandler.RunModal( browser: {0} )", browser.Identifier );
			return base.RunModal( browser );
		}
	}
}
