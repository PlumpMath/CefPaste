namespace Chromium.Embedded
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Threading;
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
			if( frame.IsMain == false )
			{
				base.OnContextCreated( browser, frame, context );
				return;
			}

			Log.Trace( "RenderProcessHandler.OnContextCreated( browser: {0}, frame: {1} )",
				browser.Identifier,
				frame.Identifier );

			m_javascriptInvokeServer = new RpcServer( browser, "javascript:invoke" );

			m_javascriptInvokeServer.RequestHandler = request =>
			{
				var function = request[0] as String ?? "";
				var arguments = request[1] as Object[] ?? new Object[0];

				var ctx = m_javascriptInvokeServer.Browser.GetMainFrame().V8Context;

				if( ctx.Enter() == false ) throw new InvalidOperationException( "Could not acquire the V8 context" );

				JSValue result;

				try
				{
					var global = ctx.GetGlobal();

					var func = global.GetValue( function );

					var rawArgs = (Object[])( new JSValue( arguments ).Value );

					var v8Args = rawArgs.Select( arg => new JSValue( arg ).AsV8Value() ).ToArray();

					var v8Return = func.ExecuteFunctionWithContext( ctx, global, v8Args );

					result = new JSValue( v8Return );
				}
				finally
				{
					ctx.Exit();
				}

				return result;
			};

			m_javascriptEvalServer = new RpcServer( browser, "javascript:eval" );

			m_javascriptEvalServer.RequestHandler = request =>
			{
				var eval = request[0] as String ?? "";

				var ctx = m_javascriptEvalServer.Browser.GetMainFrame().V8Context;

				if( ctx.Enter() == false ) throw new InvalidOperationException( "Could not acquire the V8 context" );

				try
				{
					var global = ctx.GetGlobal();

					var func = global.GetValue( "eval" );

					func.ExecuteFunctionWithContext( ctx, global, new[] { CefV8Value.CreateString( eval ) } );
				}
				finally
				{
					ctx.Exit();
				}

				return null;
			};

			base.OnContextCreated( browser, frame, context );
		}

		protected override void OnContextReleased( CefBrowser browser, CefFrame frame, CefV8Context context )
		{
			if( frame.IsMain == false )
			{
				base.OnContextReleased( browser, frame, context );
				return;
			}

			Log.Trace( "RenderProcessHandler.OnContextReleased( browser: {0}, frame: {1} )",
				browser.Identifier,
				frame.Identifier );

			m_javascriptInvokeServer.Dispose();

			m_javascriptEvalServer.Dispose();

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

			if( sourceProcess == CefProcessId.Browser ) // in Render process
			{
				RpcBroker.DispatchRequest( message.Arguments );
			}
			//else if( sourceProcess == CefProcessId.Renderer ) // in Browser process
			//{
			//	RpcBroker.DispatchReply( message.Arguments );
			//}
			else
			{
				Log.Debug( "Message received from CefProcessId.{0}", Enum.GetName( typeof( CefProcessId ), sourceProcess ) );
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
			Log.Warn( "RenderProcessHandler.OnUncaughtException( browser: {0}, frame: {1}, exception: {2} )",
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
