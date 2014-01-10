namespace Chromium
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Threading;
	using Embedded;
	using NLog;

	/// <summary></summary>
	public sealed class Browser: IDisposable
	{
		private static readonly Logger Log;

		static Browser()
		{
			Log = LogManager.GetCurrentClassLogger();
		}

		private readonly Router Router;

		private readonly DirectoryInfo CookieDirectory;

		private readonly Client Client;

		private readonly RpcClient RpcClient;

		/// <summary></summary>
		public Browser( Router router, DirectoryInfo cookieDirectory = null )
		{
			this.Router = router;

			this.CookieDirectory = cookieDirectory ?? new DirectoryInfo( Path.Combine( Path.GetTempPath(), Guid.NewGuid().ToString( "N" ) ) );

			this.Client = new Client( this.CookieDirectory );

			this.RpcClient = new RpcClient( this.Client.Browser );

			this.Wireup();

			Runtime.Register( this );
		}

		/// <summary></summary>
		public Browser( Browser @this )
		{
			this.Router = @this.Router;

			this.Client = new Client( @this.Client );

			this.RpcClient = new RpcClient( this.Client.Browser );

			this.Wireup();

			Runtime.Register( this );
		}

		private void Wireup()
		{
			this.Client.HandleLoadFinished = this.OnLoadFinished;

			this.Client.HandleUnauthorized = this.OnUnauthorized;

			this.Client.HandleDownloadStarting = this.OnDownloadStarting;

			this.Client.HandleDownloadProgress = this.OnDownloadProgress;

			this.Client.HandleOpenFileDialog = this.OnFileOpenDialog;

			this.Client.HandleConsoleMessage = ( message, source, line ) => Log.Debug( message );
		}

		public void Dispose()
		{
			this.Client.Dispose();

			Runtime.Unregister( this );
		}

		///////////////////////////////////////////////////////////////////////

		/// <summary></summary>
		public UInt16 Width
		{
			get { return this.Client.Width; }
			set { this.Client.Width = value; }
		}

		/// <summary></summary>
		public UInt16 Height
		{
			get { return this.Client.Height; }
			set { this.Client.Height = value; }
		}

		///////////////////////////////////////////////////////////////////////
		
		/// <summary></summary>
		public void Load( String url )
		{
			this.Client.Browser.GetMainFrame().LoadUrl( url );
		}

		/// <summary></summary>
		public void Reload()
		{
			this.Client.Browser.ReloadIgnoreCache();
		}

		/// <summary></summary>
		public void Stop()
		{
			this.Client.Browser.StopLoad();
		}

		///////////////////////////////////////////////////////////////////////
		
		/// <summary></summary>
		public Boolean Back()
		{
			var result = this.Client.Browser.CanGoBack;
			this.Client.Browser.GoBack();
			return result;
		}

		/// <summary></summary>
		public Boolean Forward()
		{
			var result = this.Client.Browser.CanGoForward;
			this.Client.Browser.GoForward();
			return result;
		}

		///////////////////////////////////////////////////////////////////////

		/// <summary></summary>
		public void EvalJS( String script )
		{
			if( String.IsNullOrWhiteSpace( script ) )
			{
				throw new ArgumentException( "Argument 'script' can not be empty", "script" );
			}

			var evalArgs = new List<Object>();

			evalArgs.Add( script );

			this.RpcClient.Invoke( "javascript:eval", evalArgs.ToArray() );
		}

		/// <summary></summary>
		public String CompileJS( String script )
		{
			if( String.IsNullOrWhiteSpace( script ) )
			{
				throw new ArgumentException( "Argument 'script' can not be empty", "script" );
			}

			var funcName = "$" + Guid.NewGuid().ToString( "N" );

			var funcSource =
"window." + funcName + @" = function() {
    return " + script + @"
}";

			this.EvalJS( funcSource );

			return funcName;
		}

		/// <summary></summary>
		public Object[] InvokeJS( String function, params Object[] args )
		{
			if( String.IsNullOrWhiteSpace( function ) )
			{
				throw new ArgumentException( "Argument 'function' can not be empty", "function" );
			}

			var invokeArgs = new List<Object>
			{
				function,
				args
			};

			var result = this.RpcClient.Invoke( "javascript:invoke", invokeArgs.ToArray() );

			return result.Value as Object[];
		}

		///////////////////////////////////////////////////////////////////////

		/// <summary></summary>
		/// <see cref="http://www.w3.org/TR/selectors-api/" />
		public void Click( String selector )
		{
			if( String.IsNullOrWhiteSpace( selector ) ) throw new ArgumentException( "selector" );

			var code = @"(function( selector ){
	var node = document.querySelector( selector );
//if( node == null ) console.log( 'Click() node = ' + node );
//if( node == null ) console.log( 'Click() selector = ' + selector );
	if( node == null ) return;
	node.scrollIntoView();
	var clientRect = node.getBoundingClientRect();
	var x = Math.round( clientRect.left + ( clientRect.width / 4 ) );
	var y =  Math.round( clientRect.top + ( clientRect.height / 4 ) );
	return [ x, y ];
}).apply( this, arguments );";

			var jsGetSelectorCoordinates = this.CompileJS( code );

			var coordinates = this.InvokeJS( jsGetSelectorCoordinates, selector );

			if( coordinates == null || coordinates.Length < 2 )
			{
				throw new Exception( "Could not find an element matching selector '" + selector + "'" );
			}

			this.Client.Click( (Int32)coordinates[0], (Int32)coordinates[1] );
		}

		/// <summary></summary>
		public void Type( String text )
		{
			this.Client.Type( text );
		}

		/// <summary></summary>
		// @todo commeting out until CEF3 has a way to not show the print dialog
		//public void Print()
		//{
		//	this.Client.Print();
		//}

		public Bitmap Render()
		{
			return this.Client.Render();
		}

		///////////////////////////////////////////////////////////////////////

		/// <summary></summary>
		public Action BeforeLoad;

		private void OnLoadFinished()
		{
			if( this.BeforeLoad != null ) this.BeforeLoad();

			var location = this.Client.Browser.GetMainFrame().Url;

			var controller = this.Router.Route( location );

			controller.TaskFactory.StartNew( () =>
			{
				if( this.Client.Browser != null ) controller.OnLoad( this, location );
			} );
		}

		private Boolean OnUnauthorized( String host, Int32 port, String realm, String scheme, out String username, out String password )
		{
			var location = this.Client.Browser.GetMainFrame().Url;

			var controller = this.Router.Route( location );

			Tuple<String, String> result = null;

			controller.TaskFactory.StartNew( () => { result = controller.OnUnauthorized( this, location, host, port, realm, scheme ); } ).Wait();

			username = result != null ? result.Item1 : null;
			password = result != null ? result.Item2 : null;

			return result == null;
		}

		private String OnDownloadStarting( String suggestedFileName )
		{
			var location = this.Client.Browser.GetMainFrame().Url;

			var controller = this.Router.Route( location );

			String result = null;

			controller.TaskFactory.StartNew( () => { result = controller.OnDownload( this, location, suggestedFileName ); } ).Wait();

			return result;
		}

		private void OnDownloadProgress( String fileName, Int64 receivedBytes, Int64 totalBytes, ref Boolean cancelled )
		{
			if( cancelled == false )
			{
				var location = this.Client.Browser.GetMainFrame().Url;

				var controller = this.Router.Route( location );

				var result = false;

				controller.TaskFactory.StartNew( () => controller.OnDownloadProgress( this, location, fileName, receivedBytes, totalBytes, out result ) );

				cancelled = result;
			}
		}

		private String[] OnFileOpenDialog( String title, String defaultFileName, String[] acceptTypes )
		{
			var location = this.Client.Browser.GetMainFrame().Url;

			var controller = this.Router.Route( location );

			String[] result = null;

			controller.TaskFactory.StartNew( () => { result = controller.OnUpload( this, location, acceptTypes ); } ).Wait();

			return result;
		}
	}
}
