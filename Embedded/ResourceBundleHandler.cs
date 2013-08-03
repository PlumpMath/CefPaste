namespace Chromium.Embedded
{
	using System;
	using NLog;
	using Xilium.CefGlue;

	internal sealed class ResourceBundleHandler: CefResourceBundleHandler
	{
		private static readonly Logger Log;

		static ResourceBundleHandler()
		{
			Log = LogManager.GetCurrentClassLogger();
		}

		private readonly App App;

		public ResourceBundleHandler( App app )
		{
			this.App = app;
		}

		// Would have to mark the assembly unsafe in order to override this
		//protected override unsafe Boolean GetDataResource( Int32 resourceID, void** data, UIntPtr* dataSize )
		//{
		//	return base.GetDataResource( resourceID, data, dataSize );
		//}

		protected override String GetLocalizedString( Int32 messageId )
		{
			Log.Trace( "ResourceBundleHandler.GetLocalizedString( messageId: {0} )", messageId );
			return base.GetLocalizedString( messageId );
		}
	}
}
