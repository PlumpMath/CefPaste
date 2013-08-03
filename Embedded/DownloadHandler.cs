namespace Chromium.Embedded
{
	using System;
	using System.IO;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class DownloadHandler: CefDownloadHandler
	{
		private static readonly Logger Log;

		static DownloadHandler()
		{
			Log = LogManager.GetCurrentClassLogger();
		}
		
		private readonly App App;

		private readonly Client Client;

		public DownloadHandler( App app, Client client )
		{
			this.App = app;
			this.Client = client;
		}

		protected override void OnBeforeDownload( CefBrowser browser, CefDownloadItem downloadItem, String suggestedName, CefBeforeDownloadCallback callback )
		{
			if( this.Client.HandleDownloadStarting != null )
			{
				var fileName = this.Client.HandleDownloadStarting( suggestedName );
				if( fileName != null ) callback.Continue( fileName, false );
			}

			Log.Trace( "DownloadHandler.OnBeforeDownload( browser: {0}, downloadItem: {1}, suggestedName: {2} )", browser.Identifier, downloadItem.Id, suggestedName );
			
			//base.OnBeforeDownload( browser, downloadItem, suggestedName, callback );
		}

		protected override void OnDownloadUpdated( CefBrowser browser, CefDownloadItem downloadItem, CefDownloadItemCallback callback )
		{
			if( this.Client.HandleDownloadProgress != null )
			{
				var cancelled = downloadItem.IsCanceled;
				this.Client.HandleDownloadProgress( downloadItem.FullPath, downloadItem.ReceivedBytes, downloadItem.TotalBytes, ref cancelled );
				if( cancelled && downloadItem.IsCanceled == false ) callback.Cancel();
			}
			
			Log.Trace( "DownloadHandler.OnDownloadUpdated( browser: {0}, downloadItem: {1} )", browser.Identifier, downloadItem.Id );
			
			//base.OnDownloadUpdated( browser, downloadItem, callback );
		}
	}
}
