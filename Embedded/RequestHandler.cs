namespace Chromium.Embedded
{
	using System;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class RequestHandler: CefRequestHandler
	{
		private static readonly Logger Log;

		static RequestHandler()
		{
			Log = LogManager.GetCurrentClassLogger();
		}
		
		private readonly App App;

		private readonly Client Client;

		public RequestHandler( App app, Client client )
		{
			this.App = app;
			this.Client = client;
		}

		protected override Boolean GetAuthCredentials( CefBrowser browser, CefFrame frame, Boolean isProxy,
			String host, Int32 port, String realm, String scheme, CefAuthCallback callback )
		{
			if( this.Client.HandleUnauthorized != null )
			{
				String username, password;

				if( this.Client.HandleUnauthorized( host, port, realm, scheme, out username, out password ) )
				{
					callback.Continue( username, password );
					return true;
				}
			}

			Log.Trace( "RequestHandler.GetAuthCredentials( browser: {0}, frame: {1}, isProxy: {2}, host: {3}, port: {4}, realm: {5}, scheme: {6} )",
				browser.Identifier,
				frame.Identifier,
				isProxy,
				host,
				port,
				realm,
				scheme);

			return false;
			//return base.GetAuthCredentials( browser, frame, isProxy, host, port, realm, scheme, callback );
		}

		protected override CefCookieManager GetCookieManager( CefBrowser browser, String mainUrl )
		{
			Log.Trace( "RequestHandler.GetCookieManager( browser: {0}, mainUrl: {1} )", browser.Identifier, mainUrl );
			
			return this.Client.CookieManager;
			//return base.GetCookieManager( browser, mainUrl );
		}

		protected override CefResourceHandler GetResourceHandler( CefBrowser browser, CefFrame frame, CefRequest request )
		{
			Log.Trace( "RequestHandler.GetResourceHandler( browser: {0}, frame: {1}, request: {2} )",
				browser.Identifier,
				frame.Identifier,
				request.Method + " " + request.Url );
			return base.GetResourceHandler( browser, frame, request );
		}

		protected override Boolean OnBeforePluginLoad( CefBrowser browser, String url, String policyUrl, CefWebPluginInfo info )
		{
			Log.Trace( "RequestHandler.OnBeforePluginLoad( browser: {0}, url: {1}, policyUrl: {2}, info: {3} )",
				browser.Identifier,
				url,
				policyUrl,
				info.Name + " version " + info.Version );
			return base.OnBeforePluginLoad( browser, url, policyUrl, info );
		}

		protected override Boolean OnBeforeResourceLoad( CefBrowser browser, CefFrame frame, CefRequest request )
		{
			Log.Trace( "RequestHandler.OnBeforeResourceLoad( browser: {0}, frame: {1}, request: {2} )",
				browser.Identifier,
				frame.Identifier,
				request.Method + " " + request.Url );
			return base.OnBeforeResourceLoad( browser, frame, request );
		}

		protected override Boolean OnCertificateError( CefErrorCode certError, String requestUrl, CefAllowCertificateErrorCallback callback )
		{
			Log.Trace( "RequestHandler.OnCertificateError( certError: {0}, requestUrl: {1} )",
				Enum.GetName( typeof( CefErrorCode ), certError ),
				requestUrl );
			return base.OnCertificateError( certError, requestUrl, callback );
		}

		protected override void OnProtocolExecution( CefBrowser browser, String url, out Boolean allowOSExecution )
		{
			Log.Trace( "RequestHandler.OnProtocolExecution( browser: {0}, url: {1} )", browser.Identifier, url );
			base.OnProtocolExecution( browser, url, out allowOSExecution );
		}

		protected override Boolean OnQuotaRequest( CefBrowser browser, String originUrl, Int64 newSize, CefQuotaCallback callback )
		{
			Log.Trace( "RequestHandler.OnQuotaRequest( browser: {0}, originUrl: {1}, newSize: {2}b )",
				browser.Identifier,
				originUrl,
				newSize );
			return false;
		}

		protected override void OnResourceRedirect( CefBrowser browser, CefFrame frame, String oldUrl, ref String newUrl )
		{
			Log.Trace( "RequestHandler.OnResourceRedirect( browser: {0}, frame: {1}, oldUrl: {2}, newUrl: {3} )",
				browser.Identifier,
				frame.Identifier,
				oldUrl,
				newUrl );
			base.OnResourceRedirect( browser, frame, oldUrl, ref newUrl );
		}
	}
}
