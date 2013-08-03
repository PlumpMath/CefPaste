namespace Chromium.Embedded
{
	using System;
	using System.Collections.Concurrent;
	using System.Threading;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class RpcClient
	{
		private static readonly Logger Log;

		static RpcClient()
		{
			Log = LogManager.GetCurrentClassLogger();
		}

		public readonly CefBrowser Browser;

		private readonly ConcurrentDictionary<String, RpcBroker.Message> Replies =
			new ConcurrentDictionary<String, RpcBroker.Message>();

		public RpcClient( CefBrowser browser )
		{
			Log.Trace( "RpcClient.RpcClient" );

			if( browser == null ) throw new ArgumentNullException( "browser" );

			this.Browser = browser;
		}

		public JSValue Invoke( String procedure, Object[] arguments, Int32 timeoutMsec = 3000 )
		{
			Log.Trace( "RpcClient.Invoke( procedure: {0} )", procedure );

			if( String.IsNullOrWhiteSpace( procedure ) ) throw new ArgumentException( "procedure" );
			if( arguments == null ) throw new ArgumentNullException( "arguments" );

			var request = new RpcBroker.Message
			{
				Procedure = procedure,
				Data = new JSValue( arguments ),
			};

			this.SendRequest( request );

			// spin until a reply has been received or timeout

			RpcBroker.Message reply;

			//var spinCount = 0;
			while( this.Replies.TryGetValue( request.Token, out reply ) == false )
			{
				//if( spinCount > ( timeoutMsec / 100 ) ) break;
				Thread.Sleep( 100 );
				//spinCount++;
			}

			if( reply == null ) throw new TimeoutException( "A remote invocation has timed out before receiving a reply" );

			return reply.Data;
		}

		public void OnReply( RpcBroker.Message message )
		{
			Log.Trace( "RpcClient.OnReply" );

			if( message == null ) throw new ArgumentNullException( "message" );

			if( this.Replies.TryAdd( message.Token, message ) == false )
			{
				throw new InvalidOperationException( "A reply to this message has already been dispatched" );
			}
		}
	}
}
