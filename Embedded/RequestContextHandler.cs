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

		public RequestContextHandler()
		{
		}

		protected override CefCookieManager GetCookieManager()
		{
			Log.Trace( "RequestContextHandler.GetCookieManager()" );

			return CefCookieManager.Global;
			return null; // "the default cookie manager will be returned if this method returns null"
		}
	}
}
