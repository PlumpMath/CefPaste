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

		public RequestContextHandler( DirectoryInfo cookieDirectory )
		{
			if( cookieDirectory != null )
			{
				this.CookieManager = CefCookieManager.Create( cookieDirectory.FullName, false );
			}
		}

		protected override CefCookieManager GetCookieManager()
		{
			Log.Trace( "RequestContextHandler.GetCookieManager()" );
			
			return this.CookieManager ?? CefCookieManager.Global;
		}
	}
}
