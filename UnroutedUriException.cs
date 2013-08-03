namespace Chromium
{
	using System;

	internal sealed class UnroutedUriException: Exception
	{
		public UnroutedUriException( String url )
			: base( "No controller was able to handle this URL: " + url )
		{
		}
	}
}
