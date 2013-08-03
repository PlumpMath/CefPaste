namespace Chromium.Embedded
{
	using System;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class DialogHandler: CefDialogHandler
	{
		private static readonly Logger Log;

		static DialogHandler()
		{
			Log = LogManager.GetCurrentClassLogger();
		}
		
		private readonly App App;

		private readonly Client Client;

		public DialogHandler( App app, Client client )
		{
			this.App = app;
			this.Client = client;
		}

		protected override Boolean OnFileDialog( CefBrowser browser, CefFileDialogMode mode, String title, String defaultFileName, String[] acceptTypes, CefFileDialogCallback callback )
		{
			if( mode == CefFileDialogMode.Open || mode == CefFileDialogMode.OpenMultiple )
			{
				if( this.Client.HandleOpenFileDialog != null )
				{
					callback.Continue( this.Client.HandleOpenFileDialog( title, defaultFileName, acceptTypes ) );
					return true;
				}
			}
			else if( mode == CefFileDialogMode.Save )
			{
				if( this.Client.HandleSaveFileDialog != null )
				{
					callback.Continue( this.Client.HandleSaveFileDialog( title, defaultFileName, acceptTypes ) );
					return true;
				}
			}

			Log.Trace( "DialogHandler.OnFileDialog( browser: {0}, mode: {1}, title: {2}, defaultFileName: {3}, acceptTypes: {4} )",
				browser.Identifier,
				Enum.GetName( typeof( CefFileDialogMode ), mode ),
				title,
				defaultFileName,
				String.Join( " ", acceptTypes ) );

			return false;
			//return base.OnFileDialog( browser, mode, title, defaultFileName, acceptTypes, callback );
		}
	}
}
