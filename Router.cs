namespace Chromium
{
	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;
	using System.Timers;
	using NLog;

	public sealed class Router: IDisposable
	{
		private static readonly Logger Log;

		static Router()
		{
			Log = LogManager.GetCurrentClassLogger();
		}

		private readonly Timer TimeoutWaiter;

		public Action<Router> OnTimeout;

		public Int32 TimeoutSeconds
		{
			get { return (Int32)this.TimeoutWaiter.Interval; }
			set { this.TimeoutWaiter.Interval = value * 1000; }
		}

		public Router( Int32 timeoutSeconds = 10 )
		{
			this.TimeoutWaiter = new Timer
			{
				AutoReset = true,
				Enabled = false,
			};

			this.TimeoutWaiter.Elapsed += ( s, a ) =>
			{
				Log.Warn( "Timeout" );

				if( this.OnTimeout != null )
				{
					this.OnTimeout( this );
				}
			};

			this.TimeoutSeconds = timeoutSeconds;
		}

		public void Dispose()
		{
			this.TimeoutWaiter.Dispose();
		}

		///////////////////////////////////////////////////////////////////////

		private readonly List<Tuple<Regex, Controller>> Routes = new List<Tuple<Regex, Controller>>();

		public void IgnoreRoute( Regex regex )
		{
			if( regex == null ) throw new ArgumentNullException( "regex" );

			this.Routes.Add( new Tuple<Regex, Controller>( regex, null ) );
		}

		public void IgnoreRoute( String regex )
		{
			if( String.IsNullOrWhiteSpace( regex ) ) throw new ArgumentException( "regex" );

			this.IgnoreRoute( new Regex( regex, RegexOptions.Compiled | RegexOptions.CultureInvariant ) );
		}

		public void MapRoute( Regex regex, Controller controller )
		{
			if( regex == null ) throw new ArgumentNullException( "regex" );
			if( controller == null ) throw new ArgumentNullException( "controller" );

			this.Routes.Add( new Tuple<Regex, Controller>( regex, controller ) );
		}

		public void MapRoute( String regex, Controller controller )
		{
			if( String.IsNullOrWhiteSpace( regex ) ) throw new ArgumentException( "regex" );

			this.MapRoute( new Regex( regex, RegexOptions.Compiled | RegexOptions.CultureInvariant ), controller );
		}

		///////////////////////////////////////////////////////////////////////
		
		internal Controller Route( String url )
		{
			Controller lastMatchedRoute = null;

			foreach( var route in this.Routes )
			{
				if( route.Item1.IsMatch( url ) )
				{
					lastMatchedRoute = route.Item2;
				}
			}

			if( lastMatchedRoute == null )
			{
				throw new UnroutedUriException( url );
			}

			if( this.TimeoutWaiter.Enabled == false )
			{
				this.TimeoutWaiter.Enabled = true;
			}
			else
			{
				this.TimeoutWaiter.Stop();
				this.TimeoutWaiter.Start();
			}
			
			return lastMatchedRoute;
		}
	}
}
