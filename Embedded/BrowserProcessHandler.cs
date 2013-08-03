namespace Chromium.Embedded
{
	using NLog;
	using Xilium.CefGlue;

	internal sealed class BrowserProcessHandler: CefBrowserProcessHandler
	{
		private static readonly Logger Log;

		static BrowserProcessHandler()
		{
			Log = LogManager.GetCurrentClassLogger();
		}

		private readonly CefApp App;

		public BrowserProcessHandler( CefApp app )
		{
			this.App = app;
		}
		
		protected override void OnBeforeChildProcessLaunch( CefCommandLine commandLine )
		{
			Log.Trace( "BrowserProcessHandler.OnBeforeChildProcessLaunch( commandLine: {0} )", commandLine.ToString() );
			base.OnBeforeChildProcessLaunch( commandLine );
		}

		protected override void OnContextInitialized()
		{
			Log.Trace( "BrowserProcessHandler.OnContextInitialized()" );
			base.OnContextInitialized();
		}

		protected override void OnRenderProcessThreadCreated( CefListValue extraInfo )
		{
			Log.Trace( "BrowserProcessHandler.OnRenderProcessThreadCreated( extraInfo: CefListValue[{0}] )", extraInfo.Count );
			base.OnRenderProcessThreadCreated( extraInfo );
		}
	}
}
