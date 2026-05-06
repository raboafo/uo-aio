using System;

namespace UOAIO.Videos;

public sealed class VersionMismatchException : Exception
{
	public VersionMismatchException()
		: base("Version mismatch.")
	{
	}
}
