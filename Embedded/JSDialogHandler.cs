namespace Chromium.Embedded
{
	using System;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class JSDialogHandler: CefJSDialogHandler
	{
		private static readonly Logger Log;

		static JSDialogHandler()
		{
			Log = LogManager.GetCurrentClassLogger();
		}
		
		private readonly App App;

		private readonly Client Client;

		public JSDialogHandler( App app, Client client )
		{
			this.App = app;
			this.Client = client;
		}

		protected override Boolean OnJSDialog( CefBrowser browser, String originUrl, String acceptLang, CefJSDialogType dialogType, String messageText, String defaultPromptText, CefJSDialogCallback callback, out Boolean suppressMessage )
		{
			Log.Trace( "JSDialogHandler.OnJSDialog( browser: {0}, originUrl: {1}, acceptLang: {2}, dialogType: {3}, messageText: {4}, defaultPromptText: {5} )",
				browser.Identifier,
				originUrl,
				acceptLang,
				Enum.GetName( typeof( CefJSDialogType ), dialogType ),
				messageText,
				defaultPromptText );

			suppressMessage = true;

			switch( dialogType )
			{
			case CefJSDialogType.Alert:

				if( this.Client.HandleJavascriptAlertDialog != null )
				{
					this.Client.HandleJavascriptAlertDialog( originUrl, messageText );
					return true;
				}

				break;

			case CefJSDialogType.Confirm:

				if( this.Client.HandleJavascriptConfirmDialog != null )
				{
					Boolean confirmed;
					this.Client.HandleJavascriptConfirmDialog( originUrl, messageText, out confirmed );
					callback.Continue( confirmed, null );
					return true;
				}

				break;

			case CefJSDialogType.Prompt:

				if( this.Client.HandleJavascriptPromptDialog != null )
				{
					String responseText;
					this.Client.HandleJavascriptPromptDialog( originUrl, messageText, out responseText );
					callback.Continue( responseText != null, responseText );
					return true;
				}

				break;
			}

			return false;
		}

		protected override Boolean OnBeforeUnloadDialog( CefBrowser browser, String messageText, Boolean isReload, CefJSDialogCallback callback )
		{
			Log.Trace( "JSDialogHandler.OnBeforeUnloadDialog" );
			Log.Trace( "JSDialogHandler.OnBeforeUnloadDialog( browser: {0}, messageText: {1}, isReload: {2} )",
				browser.Identifier,
				messageText,
				isReload );
			callback.Continue( true, null );
			return true;
		}

		protected override void OnDialogClosed( CefBrowser browser )
		{
			Log.Trace( "JSDialogHandler.OnDialogClosed( browser: {0} )", browser.Identifier );
		}

		protected override void OnResetDialogState( CefBrowser browser )
		{
			Log.Trace( "JSDialogHandler.OnResetDialogState( browser: {0} )", browser.Identifier );
		}
	}
}
