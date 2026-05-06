namespace System.IO;

internal static class StreamEx
{
	public static void Fill(this Stream stream, byte[] buffer, int offset, int length)
	{
		while (length > 0)
		{
			int read = stream.Read(buffer, offset, length);
			if (read <= 0)
			{
				throw new EndOfStreamException();
			}
			offset += read;
			length -= read;
		}
	}
}
