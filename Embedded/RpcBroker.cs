namespace Chromium.Embedded
{
	using System;
	using System.Collections.Concurrent;
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Threading;
	using NLog;
	using Xilium.CefGlue;

	internal static class RpcBroker
	{
		private static readonly Logger Log;

		static RpcBroker()
		{
			Log = LogManager.GetCurrentClassLogger();
		}

		public class Message
		{
			public readonly String Token;

			public Message()
			{
				this.Token = Guid.NewGuid().ToString( "N" );
			}

			public Message( String token )
			{
				if( String.IsNullOrWhiteSpace( token ) ) throw new ArgumentException( "token" );

				this.Token = token;
			}

			public String Procedure;

			public JSValue Data;
		}

		public class InvokeTask: CefTask
		{
			public readonly Action Act;

			public InvokeTask( Action action )
			{
				if( action == null ) throw new ArgumentNullException( "action" );
				this.Act = action;
			}

			protected override void Execute()
			{
				this.Act();
			}
		}

		// <Method, Server>
		private static readonly ConcurrentDictionary<String, RpcServer> s_serverMethods =
			new ConcurrentDictionary<String, RpcServer>();

		// <Token, Client>
		private static readonly ConcurrentDictionary<String, RpcClient> s_clientTokens =
			new ConcurrentDictionary<String, RpcClient>();

		public static void Register( this RpcServer server, String procedure )
		{
			Log.Trace( "RpcBroker.Register" );

			if( String.IsNullOrWhiteSpace( procedure ) ) throw new ArgumentException( "procedure" );

			if( s_serverMethods.TryAdd( procedure, server ) == false )
			{
				throw new InvalidOperationException( "Procedure '" + procedure + "' has already been registered" );
			}
		}

		public static void Unregister( this RpcServer server, String procedure )
		{
			Log.Trace( "RpcBroker.Unregister" );

			if( String.IsNullOrWhiteSpace( procedure ) ) throw new ArgumentException( "procedure" );

			RpcServer unused;

			if( s_serverMethods.TryRemove( procedure, out unused ) == false )
			{
				throw new InvalidOperationException( "Procedure '" + procedure + "' has is not registered" );
			}
		}

		public static void SendRequest( this RpcClient client, Message request )
		{
			Log.Trace( "RpcBroker.SendRequest" );

			if( s_clientTokens.TryAdd( request.Token, client ) == false )
			{
				throw new InvalidOperationException( "This message has already been sent" );
			}

			var processMessage = CefProcessMessage.Create( request.Token );
			
			processMessage.Arguments.SetString( 0, request.Token );
			processMessage.Arguments.SetString( 1, request.Procedure );
			if( request.Data != null ) processMessage.Arguments.SetList( 2, request.Data.AsCefListValue() );
			else processMessage.Arguments.SetList( 2, CefListValue.Create() );

			client.Browser.SendProcessMessage( CefProcessId.Renderer, processMessage );
		}

		public static void DispatchRequest( CefListValue data )
		{
			Log.Trace( "RpcBroker.DispatchRequest" );

			if( data == null ) throw new ArgumentNullException( "data" );

			var request = new Message( data.GetString( 0 ) )
			{
				Procedure = data.GetString( 1 ),
				Data = new JSValue( data.GetList( 2 ) ),
			};

			RpcServer server;

			if( s_serverMethods.TryGetValue( request.Procedure, out server ) == false )
			{
				throw new InvalidOperationException( "No server is registered to handle procedure '" + request.Procedure + "'" );
			}

			server.OnRequest( request );
		}

		public static void SendReply( this RpcServer server, Message reply )
		{
			Log.Trace( "RpcBroker.SendReply" );

			var processMessage = CefProcessMessage.Create( reply.Token );

			processMessage.Arguments.SetString( 0, reply.Token );
			processMessage.Arguments.SetString( 1, reply.Procedure );
			if( reply.Data != null ) processMessage.Arguments.SetList( 2, reply.Data.AsCefListValue() );
			else processMessage.Arguments.SetList( 2, CefListValue.Create() );

			server.Browser.SendProcessMessage( CefProcessId.Browser, processMessage );
		}
		 
		public static void DispatchReply( CefListValue data )
		{
			Log.Trace( "RpcBroker.DispatchReply" );

			if( data == null ) throw new ArgumentNullException( "data" );

			var reply = new Message( data.GetString( 0 ) )
			{
				Procedure = data.GetString( 1 ),
				Data = new JSValue( data.GetList( 2 ) ),
			};
			
			RpcClient client;

			if( s_clientTokens.TryRemove( reply.Token, out client ) == false )
			{
				throw new InvalidOperationException( "This message has already been replied to" );
			}

			client.OnReply( reply );
		}
	}
}
