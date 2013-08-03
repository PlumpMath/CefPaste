namespace Chromium.Embedded
{
	using System;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class RpcServer: IDisposable
	{
		private static readonly Logger Log;

		static RpcServer()
		{
			Log = LogManager.GetCurrentClassLogger();
		}

		public readonly CefBrowser Browser;

		public readonly String Procedure;

		public RpcServer( CefBrowser browser, String procedure )
		{
			Log.Trace( "RpcServer.RpcServer" );

			if( browser == null ) throw new ArgumentNullException( "browser" );
			if( String.IsNullOrWhiteSpace( procedure ) ) throw new ArgumentException( "procedure" );

			this.Browser = browser;

			this.Procedure = procedure;

			this.Register( this.Procedure );
		}

		public Func<Object[], JSValue> RequestHandler;

		public void OnRequest( RpcBroker.Message message )
		{
			Log.Trace( "RpcServer.OnRequest" );

			if( message == null ) throw new ArgumentNullException( "message" );

			if( this.RequestHandler == null ) return;

			var replyData = this.RequestHandler( message.Data.Value as Object[] );

			var reply = new RpcBroker.Message( message.Token )
			{
				Procedure = message.Procedure,
				Data = replyData,
			};

			this.SendReply( reply );
		}

		public void Dispose()
		{
			Log.Trace( "RpcServer.Dispose" );

			this.Unregister( this.Procedure );
		}
	}
}
