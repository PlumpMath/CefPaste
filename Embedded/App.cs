namespace Chromium.Embedded
{
	using System;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class App : CefApp
	{
		private static readonly Logger Log;

		static App()
		{
			Log = LogManager.GetCurrentClassLogger();
		}

		///////////////////////////////////////////////////////////////////////

		private readonly AppHandlers Handlers;

		private readonly CefMainArgs Args;

		private App( String[] args )
		{
			this.Handlers = new AppHandlers( this );
			this.Args = new CefMainArgs( args ?? new String[0] );
		}

		///////////////////////////////////////////////////////////////////////

		public static App Instance;

		public static Boolean Initialize( out Int32 exitCode, String[] args = null )
		{
			if( Instance != null ) throw new InvalidOperationException( "Chromium has already been initialized" );

			Log.Trace( "Initializing Chromium..." );

			CefRuntime.Load();

			App.Instance = new App( args );

			exitCode = CefRuntime.ExecuteProcess( App.Instance.Args, App.Instance );

			Log.Trace( "Chromium exit code: {0}", exitCode );

			return exitCode == -1;
		}

		///////////////////////////////////////////////////////////////////////

		private static Boolean Started;

		///////////////////////////////////////////////////////////////////////

		private static String UserAgentPriv;

		public static String UserAgent
		{
			get { return App.UserAgentPriv; }
			set
			{
				if( App.Started ) throw new InvalidOperationException( "Chromium runtime has already been started" );
				App.UserAgentPriv = value;
			}
		}

		///////////////////////////////////////////////////////////////////////
		
		private static String DebugLevelPriv;

		public static String DebugLevel
		{
			get { return App.DebugLevelPriv; }
			set
			{
				if( App.Started ) throw new InvalidOperationException( "Chromium runtime has already been started" );
				App.DebugLevelPriv = value;
			}
		}

		///////////////////////////////////////////////////////////////////////
		
		private static String DebugFilePriv;

		public static String DebugFile
		{
			get { return App.DebugFilePriv; }
			set
			{
				if( App.Started ) throw new InvalidOperationException( "Chromium runtime has already been started" );
				App.DebugFilePriv = value;
			}
		}

		///////////////////////////////////////////////////////////////////////
		
		private static Int32 DebugPortPriv;

		public static Int32 DebugPort
		{
			get { return App.DebugPortPriv; }
			set
			{
				if( App.Started ) throw new InvalidOperationException( "Chromium runtime has already been started" );
				App.DebugPortPriv = value;
			}
		}

		///////////////////////////////////////////////////////////////////////
		
		private static Boolean IgnoreCertificateErrorsPriv;

		public static Boolean IgnoreCertificateErrors
		{
			get { return App.IgnoreCertificateErrorsPriv; }
			set
			{
				if( App.Started ) throw new InvalidOperationException( "Chromium runtime has already been started" );
				App.IgnoreCertificateErrorsPriv = value;
			}
		}

		///////////////////////////////////////////////////////////////////////
		
		public static void Start()
		{
			if( Instance == null ) throw new InvalidOperationException( "Chromium runtime has not been initialized" );

			if( Started ) throw new InvalidOperationException( "Chromium runtime has already been started" );

			Log.Trace( "Initializing Chromium runtime..." );

			CefRuntime.Initialize(
				application : Instance,
				args : Instance.Args,
				settings : new CefSettings
				{
					MultiThreadedMessageLoop = true,
					IgnoreCertificateErrors = App.IgnoreCertificateErrors,
					RemoteDebuggingPort = App.DebugPort,
					LogFile = App.DebugFile,
					LogSeverity = (CefLogSeverity)Enum.Parse( typeof( CefLogSeverity ), App.DebugLevel ),
					ReleaseDCheckEnabled = false,
					UncaughtExceptionStackSize = 128,
					UserAgent = App.UserAgent,
				} );

			Log.Trace( "Chromium runtime has been initialized" );

			Started = true;
		}

		///////////////////////////////////////////////////////////////////////
		
		public static void Stop()
		{
			if( Started == false ) throw new InvalidOperationException( "Chromium runtime has not yet been started" );

			Log.Trace( "Shutting down Chromium runtime..." );

			CefRuntime.Shutdown();

			Log.Trace( "Chromium runtime shut down" );

			Instance = null;

			Started = false;
		}

		///////////////////////////////////////////////////////////////////////
		
		protected override void OnBeforeCommandLineProcessing( String processType, CefCommandLine commandLine )
		{
			Log.Trace( "App.OnBeforeCommandLineProcessing( processType: {0}, commandLine: {1} )", processType, commandLine.ToString() );
			base.OnBeforeCommandLineProcessing( processType, commandLine );
		}

		protected override void OnRegisterCustomSchemes( CefSchemeRegistrar registrar )
		{
			Log.Trace( "App.OnRegisterCustomSchemes()" );
			base.OnRegisterCustomSchemes( registrar );
		}

		///////////////////////////////////////////////////////////////////////
		
		#region App Handlers

		private struct AppHandlers
		{
			public AppHandlers( App app )
			{
				this.BrowserProcessHandler = new BrowserProcessHandler( app );
				this.RenderProcessHandler = new RenderProcessHandler( app );
				this.ResourceBundleHandler = new ResourceBundleHandler( app );
			}

			public readonly BrowserProcessHandler BrowserProcessHandler;
			public readonly RenderProcessHandler RenderProcessHandler;
			public readonly ResourceBundleHandler ResourceBundleHandler;
		}
		
		protected override CefBrowserProcessHandler GetBrowserProcessHandler()
		{
			return this.Handlers.BrowserProcessHandler;
		}

		protected override CefRenderProcessHandler GetRenderProcessHandler()
		{
			return this.Handlers.RenderProcessHandler;
		}

		protected override CefResourceBundleHandler GetResourceBundleHandler()
		{
			return this.Handlers.ResourceBundleHandler;
		}

		#endregion
	}
}
