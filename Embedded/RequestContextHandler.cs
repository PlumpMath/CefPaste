namespace Chromium.Embedded
{
	using System;
	using System.IO;
	using System.Linq;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class RequestContextHandler: CefRequestContextHandler
	{
		private static readonly Logger Log;

		static RequestContextHandler()
		{
			Log = LogManager.GetCurrentClassLogger();
		}

		private readonly CefCookieManager CookieManager;

		public RequestContextHandler( DirectoryInfo cookieDirectory = null )
		{
			if( cookieDirectory != null )
			{
				this.CookieManager = CefCookieManager.Create( cookieDirectory.FullName, false );
			}
			else
			{
				this.CookieManager = CefCookieManager.Create( null, false );
			}
		}

		protected override CefCookieManager GetCookieManager()
		{
			Log.Trace( "RequestContextHandler.GetCookieManager()" );
			
			return this.CookieManager ?? CefCookieManager.Global;
		}
	}
}
