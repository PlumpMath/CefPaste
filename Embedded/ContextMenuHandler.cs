namespace Chromium.Embedded
{
	using System;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class ContextMenuHandler: CefContextMenuHandler
	{
		private static readonly Logger Log;

		static ContextMenuHandler()
		{
			Log = LogManager.GetCurrentClassLogger();
		}

		private readonly App App;

		private readonly Client Client;

		public ContextMenuHandler( App app, Client client )
		{
			this.App = app;
			this.Client = client;
		}

		protected override void OnBeforeContextMenu( CefBrowser browser, CefFrame frame, CefContextMenuParams state, CefMenuModel model )
		{
			Log.Trace( "ContextMenuHandler.OnBeforeContextMenu( browser: {0}, frame: {1}, state: {2}, model: CefMenuModel[{3}] )",
				browser.Identifier,
				frame.Identifier,
				"(" + state.X + ", " + state.Y + ")",
				model.Count );
			base.OnBeforeContextMenu( browser, frame, state, model );
		}

		protected override Boolean OnContextMenuCommand( CefBrowser browser, CefFrame frame, CefContextMenuParams state, Int32 commandId, CefEventFlags eventFlags )
		{
			Log.Trace( "ContextMenuHandler.OnContextMenuCommand( browser: {0}, frame: {1}, state: {2}, commandId: {3}, eventFlags: {4} )",
				browser.Identifier,
				frame.Identifier,
				"(" + state.X + ", " + state.Y + ")",
				commandId,
				eventFlags );
			return base.OnContextMenuCommand( browser, frame, state, commandId, eventFlags );
		}

		protected override void OnContextMenuDismissed( CefBrowser browser, CefFrame frame )
		{
			Log.Trace( "ContextMenuHandler.OnContextMenuDismissed( browser: {0}, frame: {1} )", browser.Identifier, frame.Identifier );
			base.OnContextMenuDismissed( browser, frame );
		}
	}
}
