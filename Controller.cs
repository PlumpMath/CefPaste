namespace Chromium
{
	using System;
	using System.Threading.Tasks;
	using NLog;

	/// <summary></summary>
	public abstract class Controller
	{
		private static readonly Logger Log;

		static Controller()
		{
			Log = LogManager.GetCurrentClassLogger();
		}

		internal readonly TaskFactory TaskFactory;

		/// <summary></summary>
		public Controller( TaskFactory taskFactory )
		{
			if( taskFactory == null ) throw new ArgumentNullException( "taskFactory" );

			this.TaskFactory = taskFactory;
		}

		///////////////////////////////////////////////////////////////////////
		
		/// <summary></summary>
		public abstract void OnLoad( Browser sender, String requestUri );

		/// <summary></summary>
		public virtual Tuple<String, String> OnUnauthorized( Browser sender, String requestUri, String host, Int32 port, String realm, String scheme )
		{
			throw new NotImplementedException();
		}

		/// <summary></summary>
		public virtual String OnDownload( Browser sender, String requestUri, String suggestedFileName )
		{
			throw new NotImplementedException();
		}

		/// <summary></summary>
		public virtual void OnDownloadProgress( Browser sender, String requestUri, String file, Int64 receivedBytes, Int64 totalBytes, out Boolean cancelled )
		{
			throw new NotImplementedException();
		}

		/// <summary></summary>
		public virtual String[] OnUpload( Browser sender, String requestUri, String[] acceptTypes )
		{
			throw new NotImplementedException();
		}
	}
}
