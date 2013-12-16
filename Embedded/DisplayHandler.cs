namespace Chromium.Embedded
{
	using System;
	using System.IO;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class DisplayHandler : CefDisplayHandler
	{
		private static readonly Logger Log;

		static DisplayHandler()
		{
			Log = LogManager.GetCurrentClassLogger();
		}
		
		private readonly App App;

		private readonly Client Client;

		public DisplayHandler( App app, Client client )
		{
			this.App = app;
			this.Client = client;
		}
		
		protected override void OnAddressChange( CefBrowser browser, CefFrame frame, String url )
		{
			if( frame.IsMain && this.Client.HandleAddressChange != null )
			{
				this.Client.HandleAddressChange( url );
			}

			Log.Trace( "DisplayHandler.OnAddressChange( browser: {0}, frame: {1}, url: {2} )", browser.Identifier, frame.Identifier, url );
			//base.OnAddressChange( browser, frame, url );
		}

		protected override Boolean OnConsoleMessage( CefBrowser browser, String message, String source, Int32 line )
		{
			if( this.Client.HandleConsoleMessage != null )
			{
				this.Client.HandleConsoleMessage( message, source, line );
			}

			Log.Trace( "DisplayHandler.OnConsoleMessage( browser: {0}, message: {1}, source: {2}, line: {3} )", browser.Identifier, message, source, line );

			return true;
			//return base.OnConsoleMessage( browser, message, source, line );
		}

		protected override void OnStatusMessage( CefBrowser browser, String value )
		{
			if( this.Client.HandleStatusMessageChange != null )
			{
				this.Client.HandleStatusMessageChange( value );
			}
			
			Log.Trace( "DisplayHandler.OnStatusMessage( browser: {0}, value: {1} )", browser.Identifier, value );
			
			//base.OnStatusMessage( browser, value );
		}

		protected override void OnTitleChange( CefBrowser browser, String title )
		{
			if( this.Client.HandleTitleChange != null )
			{
				this.Client.HandleTitleChange( title );
			}
			
			Log.Trace( "DisplayHandler.OnTitleChange( browser: {0}, title: {1} )", browser.Identifier, title );
			
			//base.OnTitleChange( browser, title );
		}

		protected override Boolean OnTooltip( CefBrowser browser, String text )
		{
			if( this.Client.HandleToolTipShown != null )
			{
				this.Client.HandleToolTipShown( text );
			}

			Log.Trace( "DisplayHandler.OnTooltip( browser: {0}, text: {1} )", browser.Identifier, text );

			return true;
			//return base.OnTooltip( browser, text );
		}
	}
}
