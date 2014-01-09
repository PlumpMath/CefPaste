namespace Chromium.Embedded
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.Runtime.InteropServices;
	using System.Threading;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class RenderHandler : CefRenderHandler
	{
		private static readonly Logger Log;

		static RenderHandler()
		{
			Log = LogManager.GetCurrentClassLogger();
		}
		
		private readonly App App;

		private readonly Client Client;

		public RenderHandler( App app, Client client )
		{
			this.App = app;
			this.Client = client;
		}

		protected override Boolean GetRootScreenRect( CefBrowser browser, ref CefRectangle rect )
		{
			rect.X = 0;
			rect.Y = 0;
			rect.Width = this.Client.Width;
			rect.Height = this.Client.Height;

			Log.Trace( "RenderHandler.GetRootScreenRect( X: {0}, Y: {1}, Width: {2}, Height: {3} )", rect.X, rect.Y, rect.Width, rect.Height );

			return true;
		}

		protected override Boolean GetScreenPoint( CefBrowser browser, Int32 viewX, Int32 viewY, ref Int32 screenX, ref Int32 screenY )
		{
			screenX = viewX;
			screenY = viewY;

			Log.Trace( "RenderHandler.GetScreenPoint( viewX: {0}, viewY: {1}, screenX: {2}, screenY: {3} )", viewX, viewY, screenX, screenY );

			return true;
		}

		protected override Boolean GetScreenInfo( CefBrowser browser, CefScreenInfo screenInfo )
		{
			Log.Trace( "RenderHandler.GetScreenInfo( browser: {0} )", browser.Identifier );
			return false;
		}

		protected override Boolean GetViewRect( CefBrowser browser, ref CefRectangle rect )
		{
			rect.X = 0;
			rect.Y = 0;
			rect.Width = this.Client.Width;
			rect.Height = this.Client.Height;

			Log.Trace( "RenderHandler.GetViewRect( X: {0}, Y: {1}, Width: {2}, Height: {3} )", rect.X, rect.Y, rect.Width, rect.Height );

			return true;
		}

		protected override void OnScrollOffsetChanged( CefBrowser browser )
		{
			Log.Trace( "RenderHandler.OnScrollOffsetChanged( browser: {0} )", browser.Identifier );
		}

		protected override void OnPopupSize( CefBrowser browser, CefRectangle rect )
		{
			Log.Trace( "RenderHandler.OnPopupSize( browser: {0} )", browser.Identifier );
		}

		protected override void OnPopupShow( CefBrowser browser, Boolean show )
		{
			Log.Trace( "RenderHandler.OnPopupShow( browser: {0}, show: {1} )", browser.Identifier, show );
			base.OnPopupShow( browser, show );
		}

		private Bitmap NextBitmap = null;

		public Bitmap Bitmap
		{
			get
			{
				while( this.NextBitmap == null ) Thread.Sleep( 10 );
				return this.NextBitmap;
			}
		}

		protected override void OnPaint( CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, Int32 width, Int32 height )
		{
			Log.Trace( "RenderHandler.OnPaint( browser: {0}, type: {1} )", browser.Identifier, Enum.GetName( typeof( CefPaintElementType ), type ) );

			this.NextBitmap = new Bitmap( width, height, width * 4, PixelFormat.Format32bppPArgb, buffer );
		}

		protected override void OnCursorChange( CefBrowser browser, IntPtr cursorHandle )
		{
			Log.Trace( "RenderHandler.OnCursorChange( browser: {0} )", browser.Identifier );
		}
	}
}
