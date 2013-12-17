﻿namespace Chromium.Embedded
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
			return CefCookieManager.Create( String.Empty, false );
		}
	}
}