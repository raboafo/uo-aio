using System;

namespace UOAIO.Videos;

public sealed class HashCodeMismatchException : Exception
{
	public HashCodeMismatchException()
		: base("Hash code mismatch.")
	{
	}
}
