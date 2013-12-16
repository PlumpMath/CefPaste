namespace Chromium.Embedded
{
	using System;
	using System.IO;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class DragHandler: CefDragHandler
	{
		private static readonly Logger Log;

		static DragHandler()
		{
			Log = LogManager.GetCurrentClassLogger();
		}

		private readonly App App;

		private readonly Client Client;

		public DragHandler( App app, Client client )
		{
			this.App = app;
			this.Client = client;
		}

		protected override Boolean OnDragEnter( CefBrowser browser, CefDragData dragData, CefDragOperationsMask mask )
		{
			Log.Trace( "DownloadHandler.OnDragEnter( browser: {0} )", browser.Identifier );

			return false;
		}
	}
}
