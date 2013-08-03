namespace Chromium
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using Embedded;
	using NLog;

	/// <summary></summary>
	public static class Runtime
	{
		private static readonly Logger Log;

		static Runtime()
		{
			Log = LogManager.GetCurrentClassLogger();
		}

		/// <summary></summary>
		public static void Initialize( String[] args )
		{
			var exitCode = 0;

			if( App.Initialize( out exitCode, args ) == false )
			{
				Environment.Exit( exitCode );
			}
		}

		private static Thread HostThread;

		/// <summary></summary>
		public static void Start()
		{
			if( Runtime.HostThread == null )
			{
				Runtime.HostThread = new Thread( Runtime.Start );

				Runtime.HostThread.Name = "Chromium Thread";

				Runtime.HostThread.SetApartmentState( ApartmentState.STA );

				Runtime.HostThread.Start();

				return;
			}

			try
			{
				App.Start();

				while( true )
				{
					if( Thread.Yield() == false ) Thread.Sleep( 1 );
				}
			}
			catch( ThreadAbortException )
			{
			}

			App.Stop();

			Runtime.HostThread = null;
		}

		/// <summary></summary>
		public static void Stop()
		{
			while( Runtime.Browsers.Count > 0 )
			{
				var browser = Runtime.Browsers[0];
				browser.Dispose(); // calls Unregister() below
			}

			Runtime.HostThread.Abort();
		}

		private static readonly List<Browser> Browsers = new List<Browser>();

		internal static void Register( Browser browser )
		{
			Runtime.Browsers.Add( browser );
		}

		internal static void Unregister( Browser browser )
		{
			Runtime.Browsers.Remove( browser );
		}

		/// <summary></summary>
		public static String UserAgent
		{
			get { return App.UserAgent; }
			set { App.UserAgent = value; }
		}

		/// <summary></summary>
		public static String DebugLevel
		{
			get { return App.DebugLevel; }
			set { App.DebugLevel = value; }
		}

		/// <summary></summary>
		public static String DebugFile
		{
			get { return App.DebugFile; }
			set { App.DebugFile = value; }
		}

		/// <summary></summary>
		public static Int32 DebugPort
		{
			get { return App.DebugPort; }
			set { App.DebugPort = value; }
		}

		/// <summary></summary>
		public static Boolean IgnoreCertificateErrors
		{
			get { return App.IgnoreCertificateErrors; }
			set { App.IgnoreCertificateErrors = value; }
		}

	}
}
