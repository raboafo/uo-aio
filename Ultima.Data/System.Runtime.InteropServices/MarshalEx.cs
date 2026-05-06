namespace System.Runtime.InteropServices;

internal static class MarshalEx
{
	public unsafe static string PtrToStringAnsiFixed(byte* src, int len)
	{
		byte* end = src;
		while (len > 0 && *end != 0)
		{
			end++;
			len--;
		}
		return Marshal.PtrToStringAnsi((IntPtr)src, checked((int)(end - src)));
	}
}
