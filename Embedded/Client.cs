namespace Chromium.Embedded
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.IO;
	using System.Threading;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class Client: CefClient, IDisposable
	{
		private static readonly Logger Log;

		static Client()
		{
			Log = LogManager.GetCurrentClassLogger();
		}

		///////////////////////////////////////////////////////////////////////
		
		private readonly ClientHandlers Handlers;

		private readonly DirectoryInfo CookieDirectory;

		internal CefBrowser Browser;

		internal ManualResetEvent BrowserCreatedWaiter = new ManualResetEvent( false );

		public Client( DirectoryInfo cookieDirectory = null )
		{
			this.Handlers = new ClientHandlers( App.Instance, this );

			this.CookieDirectory = cookieDirectory;
			
			this.CreateBrowser();

			// this.Browser will be set by LifeSpanHandler.OnAfterCreated(), which will then also signal BrowserCreatedWaiter

			this.BrowserCreatedWaiter.WaitOne();
		}

		public Client( Client @this )
		{
			this.Handlers = new ClientHandlers( App.Instance, this );

			this.CookieDirectory = @this.CookieDirectory;

			this.Width = @this.WidthPriv;
			this.Height = @this.HeightPriv;

			this.CreateBrowser();

			// this.Browser will be set by LifeSpanHandler.OnAfterCreated(), which will then also signal BrowserCreatedWaiter

			this.BrowserCreatedWaiter.WaitOne();
		}

		private void CreateBrowser()
		{
			var windowInfo = CefWindowInfo.Create();

			windowInfo.SetTransparentPainting( true );

			windowInfo.SetAsOffScreen( IntPtr.Zero );
			windowInfo.Width = this.Width;
			windowInfo.Height = this.Height;

			var browserSettings = new CefBrowserSettings
			{
				AcceleratedCompositing = CefState.Disabled,
				FileAccessFromFileUrls = CefState.Enabled,
				Java = CefState.Disabled,
				JavaScriptAccessClipboard = CefState.Disabled,
				JavaScriptCloseWindows = CefState.Disabled,
				WebGL = CefState.Disabled,
			};
			
			CefBrowserHost.CreateBrowser(
				client : this,
				windowInfo : windowInfo,
				settings : browserSettings,
				requestContext: CefRequestContext.CreateContext( new RequestContextHandler( this.CookieDirectory ) ) );
		}

		public void Dispose()
		{
			this.BrowserCreatedWaiter.Dispose();

			if( this.Browser != null ) this.Browser.GetHost().CloseBrowser();

			if( this.Browser != null ) this.Browser.Dispose();

			this.Browser = null;
		}
		
		///////////////////////////////////////////////////////////////////////
		
		[EditorBrowsable( EditorBrowsableState.Never )]
		internal class DimensionChangedEventArgs: EventArgs
		{
			public DimensionChangedEventArgs( Int32? width, Int32? height )
			{
				this.Width = width;
				this.Height = height;
			}

			public Int32? Width;
			public Int32? Height;
		}

		///////////////////////////////////////////////////////////////////////
		
		public event EventHandler<DimensionChangedEventArgs> WidthChanged;

		private UInt16 WidthPriv = 1280;

		public UInt16 Width
		{
			get { return this.WidthPriv; }
			set
			{
				this.WidthPriv = value;

				this.Browser.GetHost().WasResized();

				if( this.WidthChanged != null )
				{
					this.WidthChanged( this, new DimensionChangedEventArgs( value, null ) );
				}
			}
		}

		///////////////////////////////////////////////////////////////////////

		public event EventHandler<DimensionChangedEventArgs> HeightChanged;

		private UInt16 HeightPriv = 869;

		public UInt16 Height
		{
			get { return this.HeightPriv; }
			set
			{
				this.HeightPriv = value;

				this.Browser.GetHost().WasResized();

				if( this.HeightChanged != null )
				{
					this.HeightChanged( this, new DimensionChangedEventArgs( null, value ) );
				}
			}
		}

		///////////////////////////////////////////////////////////////////////

		public void Click( Int32 x, Int32 y )
		{
			var bhost = this.Browser.GetHost();

			bhost.SendMouseMoveEvent( new CefMouseEvent( x, y, CefEventFlags.None ), mouseLeave : false );

			bhost.SendMouseClickEvent( new CefMouseEvent( x, y, CefEventFlags.LeftMouseButton ), CefMouseButtonType.Left, false, 1 );

			bhost.SendMouseClickEvent( new CefMouseEvent( x, y, CefEventFlags.LeftMouseButton ), CefMouseButtonType.Left, true, 1 );
		}

		public void Type( String text )
		{
			var bhost = this.Browser.GetHost();

			foreach( var c in text )
			{
				bhost.SendKeyEvent(
					new CefKeyEvent
					{
						WindowsKeyCode = c,
						NativeKeyCode = c,
						Character = c,
						UnmodifiedCharacter = c,
						EventType = CefKeyEventType.Char,
						Modifiers = CefEventFlags.None,
					} );
			}
		}

		// @todo commeting out until CEF3 has a way to not show the print dialog
		//public void Print()
		//{
		//	this.Browser.GetHost().Print();
		//}

		public Bitmap Render()
		{
			return this.Handlers.RenderHandler.Render();
		}

		///////////////////////////////////////////////////////////////////////

		internal delegate String[] FileDialogHandler( String title, String defaultFileName, String[] acceptTypes );

		internal FileDialogHandler HandleOpenFileDialog;

		internal FileDialogHandler HandleSaveFileDialog;

		internal delegate void AddressChangeHandler( String newUrl );

		internal AddressChangeHandler HandleAddressChange;

		internal delegate void ConsoleMessageHandler( String message, String source, Int32 line );

		internal ConsoleMessageHandler HandleConsoleMessage;

		internal delegate void LoadingStateChangeHandler();

		internal LoadingStateChangeHandler HandleLoadStarted;

		internal LoadingStateChangeHandler HandleLoadFinished;

		internal LoadingStateChangeHandler HandleLoadError;

		internal delegate void StatusMessageChangeHandler( String message );

		internal StatusMessageChangeHandler HandleStatusMessageChange;

		internal delegate void TitleChangeHandler( String title );

		internal TitleChangeHandler HandleTitleChange;

		internal delegate void ToolTipShownHandler( String toolTip );

		internal ToolTipShownHandler HandleToolTipShown;

		internal delegate String DownloadStartingHandler( String suggestedFileName );

		internal delegate void DownloadProgressHandler( String fileName, Int64 receivedBytes, Int64 totalBytes, ref Boolean cancelled );

		internal DownloadStartingHandler HandleDownloadStarting;

		internal DownloadProgressHandler HandleDownloadProgress;

		internal delegate void JavascriptAlertDialogHandler( String origin, String message );

		internal delegate void JavascriptConfirmDialogHandler( String origin, String message, out Boolean confirmed );

		internal delegate void JavascriptPromptDialogHandler( String origin, String message, out String responseText );

		internal JavascriptAlertDialogHandler HandleJavascriptAlertDialog;

		internal JavascriptConfirmDialogHandler HandleJavascriptConfirmDialog;

		internal JavascriptPromptDialogHandler HandleJavascriptPromptDialog;

		internal delegate void BrowserAlohaHandler();

		internal BrowserAlohaHandler HandleBrowserOpened;

		internal BrowserAlohaHandler HandleBrowserClosed;

		internal delegate Boolean UnauthorizedHandler( String host, Int32 port, String realm, String scheme, out String username, out String password );

		internal UnauthorizedHandler HandleUnauthorized;

		///////////////////////////////////////////////////////////////////////
		
		protected override Boolean OnProcessMessageReceived( CefBrowser browser, CefProcessId sourceProcess, CefProcessMessage message )
		{
			Log.Trace( "Client.OnProcessMessageReceived( browser: {0}, sourceProcess: {1} )",
				browser.Identifier,
				Enum.GetName( typeof( CefProcessId ), sourceProcess ) );

			//if( sourceProcess == CefProcessId.Browser ) // in Render process
			//{
			//	RpcBroker.DispatchRequest( message.Arguments );
			//}
			if( sourceProcess == CefProcessId.Renderer ) // in Browser process
			{
				RpcBroker.DispatchReply( message.Arguments );
			}
			else
			{
				return false;
			}

			return true;
		}

		///////////////////////////////////////////////////////////////////////
		
		#region Client Handlers

		private struct ClientHandlers
		{
			public ClientHandlers( App app, Client client )
			{
				this.ContextMenuHandler = new ContextMenuHandler( app, client );
				this.DialogHandler = new DialogHandler( app, client );
				this.DisplayHandler = new DisplayHandler( app, client );
				this.DownloadHandler = new DownloadHandler( app, client );
				this.DragHandler = new DragHandler( app, client );
				this.FocusHandler = new FocusHandler( app, client );
				this.GeolocationHandler = new GeolocationHandler( app, client );
				this.JSDialogHandler = new JSDialogHandler( app, client );
				this.KeyboardHandler = new KeyboardHandler( app, client );
				this.LifeSpanHandler = new LifeSpanHandler( app, client );
				this.LoadHandler = new LoadHandler( app, client );
				this.RenderHandler = new RenderHandler( app, client );
				this.RequestHandler = new RequestHandler( app, client );
			}

			public readonly ContextMenuHandler ContextMenuHandler;
			public readonly DialogHandler DialogHandler;
			public readonly DisplayHandler DisplayHandler;
			public readonly DownloadHandler DownloadHandler;
			public readonly DragHandler DragHandler;
			public readonly FocusHandler FocusHandler;
			public readonly GeolocationHandler GeolocationHandler;
			public readonly JSDialogHandler JSDialogHandler;
			public readonly KeyboardHandler KeyboardHandler;
			public readonly LifeSpanHandler LifeSpanHandler;
			public readonly LoadHandler LoadHandler;
			public readonly RenderHandler RenderHandler;
			public readonly RequestHandler RequestHandler;
		}

		protected override CefContextMenuHandler GetContextMenuHandler()
		{
			return this.Handlers.ContextMenuHandler;
		}

		protected override CefDialogHandler GetDialogHandler()
		{
			return this.Handlers.DialogHandler;
		}

		protected override CefDisplayHandler GetDisplayHandler()
		{
			return this.Handlers.DisplayHandler;
		}

		protected override CefDownloadHandler GetDownloadHandler()
		{
			return this.Handlers.DownloadHandler;
		}

		protected override CefDragHandler GetDragHandler()
		{
			return this.Handlers.DragHandler;
		}

		protected override CefFocusHandler GetFocusHandler()
		{
			return this.Handlers.FocusHandler;
		}

		protected override CefGeolocationHandler GetGeolocationHandler()
		{
			return this.Handlers.GeolocationHandler;
		}

		protected override CefJSDialogHandler GetJSDialogHandler()
		{
			return this.Handlers.JSDialogHandler;
		}

		protected override CefKeyboardHandler GetKeyboardHandler()
		{
			return this.Handlers.KeyboardHandler;
		}

		protected override CefLifeSpanHandler GetLifeSpanHandler()
		{
			return this.Handlers.LifeSpanHandler;
		}

		protected override CefLoadHandler GetLoadHandler()
		{
			return this.Handlers.LoadHandler;
		}

		protected override CefRenderHandler GetRenderHandler()
		{
			return this.Handlers.RenderHandler;
		}

		protected override CefRequestHandler GetRequestHandler()
		{
			return this.Handlers.RequestHandler;
		}

		#endregion
	}
}
