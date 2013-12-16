namespace Chromium.Embedded
{
	using System;
	using System.IO;
	using System.Linq;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class RenderProcessHandler: CefRenderProcessHandler
	{
		private static readonly Logger Log;

		static RenderProcessHandler()
		{
			Log = LogManager.GetCurrentClassLogger();
		}

		private readonly CefApp App;

		public RenderProcessHandler( CefApp app )
		{
			this.App = app;
		}

		protected override Boolean OnBeforeNavigation( CefBrowser browser, CefFrame frame, CefRequest request, CefNavigationType navigationType, Boolean isRedirect )
		{
			Log.Trace( "RenderProcessHandler.OnBeforeNavigation( browser: {0}, frame: {1}, request: {2}, navigationType: {3}, isRedirect: {4} )",
				browser.Identifier,
				frame.Identifier,
				request.Method,
				Enum.GetName( typeof( CefNavigationType ), navigationType ),
				isRedirect );
			return base.OnBeforeNavigation( browser, frame, request, navigationType, isRedirect );
		}

		protected override void OnBrowserCreated( CefBrowser browser )
		{
			Log.Trace( "RenderProcessHandler.OnBrowserCreated( browser: {0} )", browser.Identifier );
			base.OnBrowserCreated( browser );
		}

		protected override void OnBrowserDestroyed( CefBrowser browser )
		{
			Log.Trace( "RenderProcessHandler.OnBrowserDestroyed( browser: {0} )", browser.Identifier );
			base.OnBrowserDestroyed( browser );
		}

		private RpcServer m_javascriptInvokeServer;

		private RpcServer m_javascriptEvalServer;

		protected override void OnContextCreated( CefBrowser browser, CefFrame frame, CefV8Context context )
		{
			Log.Trace( "RenderProcessHandler.OnContextCreated( browser: {0}, frame: {1} )",
				browser.Identifier,
				frame.Identifier );

			m_javascriptInvokeServer = new RpcServer( browser, "javascript:invoke" );

			m_javascriptInvokeServer.RequestHandler = request =>
			{
				var ctx = m_javascriptInvokeServer.Browser.GetMainFrame().V8Context;
				var global = ctx.GetGlobal();

				var funcName = request[0] as String ?? String.Empty;
				var args = request[1] as Object[] ?? new Object[0];

				Log.Trace( "m_javascriptInvokeServer.RequestHandler( funcName: {0} )", funcName );

				if( ctx.Enter() == false ) throw new InvalidOperationException( "Could not acquire the V8 context" );

				var func = global.GetValue( funcName );

				var rawArgs = (Object[])( new JSValue( args ).Value );

				var v8Args = rawArgs.Select( arg => new JSValue( arg ).AsV8Value() ).ToArray();

				var v8Return = func.ExecuteFunctionWithContext( ctx, global, v8Args );

				var result = new JSValue( v8Return );

				global.Dispose();

				ctx.Exit();

				ctx.Dispose();

				return result;
			};

			m_javascriptEvalServer = new RpcServer( browser, "javascript:eval" );

			this.m_javascriptEvalServer.RequestHandler = request =>
			{
				Log.Trace( "m_javascriptEvalServer.RequestHandler( funcName: eval )" );

				m_javascriptEvalServer.Browser
					.GetMainFrame()
					.ExecuteJavaScript( request[0] as String ?? String.Empty, "about:blank", 0 );

				return null;
			};

			base.OnContextCreated( browser, frame, context );
		}

		protected override void OnContextReleased( CefBrowser browser, CefFrame frame, CefV8Context context )
		{
			Log.Trace( "RenderProcessHandler.OnContextReleased( browser: {0}, frame: {1} )",
				browser.Identifier,
				frame.Identifier );

			m_javascriptInvokeServer.Dispose();

			this.m_javascriptEvalServer.Dispose();

			base.OnContextReleased( browser, frame, context );
		}

		protected override void OnFocusedNodeChanged( CefBrowser browser, CefFrame frame, CefDomNode node )
		{
			Log.Trace( "RenderProcessHandler.OnFocusedNodeChanged( browser: {0}, frame: {1}, node: {2} )",
				browser.Identifier,
				frame.Identifier,
				node != null ? node.ElementTagName : "HTML" );
			base.OnFocusedNodeChanged( browser, frame, node );
		}

		protected override Boolean OnProcessMessageReceived( CefBrowser browser, CefProcessId sourceProcess, CefProcessMessage message )
		{
			Log.Trace( "RenderProcessHandler.OnProcessMessageReceived( browser: {0}, sourceProcess: {1} )",
				browser.Identifier,
				Enum.GetName( typeof( CefProcessId ), sourceProcess ) );

			if( sourceProcess == CefProcessId.Browser )
			{
				RpcBroker.DispatchRequest( message.Arguments );
			}
			else if( sourceProcess == CefProcessId.Renderer )
			{
				RpcBroker.DispatchReply( message.Arguments );
			}
			else
			{
				return false;
			}

			return true;
		}

		protected override void OnRenderThreadCreated( CefListValue extraInfo )
		{
			Log.Trace( "RenderProcessHandler.OnRenderThreadCreated( extraInfo: CefListValue[{0}] )", extraInfo.Count );
			base.OnRenderThreadCreated( extraInfo );
		}

		protected override CefLoadHandler GetLoadHandler()
		{
			Log.Trace( "RenderProcessHandler.GetLoadHandler()" );
			return base.GetLoadHandler(); // @todo- what about CefClient.GetLoadHandler()?
		}

		protected override void OnUncaughtException( CefBrowser browser, CefFrame frame, CefV8Context context, CefV8Exception exception, CefV8StackTrace stackTrace )
		{
			Log.Trace( "RenderProcessHandler.OnUncaughtException( browser: {0}, frame: {1}, exception: {2} )",
				browser.Identifier,
				frame.Identifier,
				"\"" + exception.Message + "\" at line " + exception.LineNumber + " in script \"" + exception.ScriptResourceName + "\"" );
			base.OnUncaughtException( browser, frame, context, exception, stackTrace );
		}

		protected override void OnWebKitInitialized()
		{
			Log.Trace( "RenderProcessHandler.OnWebKitInitialized()" );
			base.OnWebKitInitialized();
		}
	}
}
