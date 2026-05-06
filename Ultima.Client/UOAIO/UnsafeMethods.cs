using System.IO;

namespace UOAIO;

public static class UnsafeMethods
{
	private static IUnsafeMethods implementation;

	static UnsafeMethods()
	{
		UnsafeMethods.implementation = new KernelUnsafeMethods();
	}

	public unsafe static int ReadFile(FileStream file, byte[] buffer, int offset, int size)
	{
		fixed (byte* ptr = buffer)
		{
			return UnsafeMethods.ReadFile(file, ptr + offset, size);
		}
	}

	public unsafe static int ReadFile(FileStream file, void* buffer, int size)
	{
		return UnsafeMethods.implementation.ReadFile(file, buffer, size);
	}

	public unsafe static int WriteFile(FileStream file, void* buffer, int size)
	{
		return UnsafeMethods.implementation.WriteFile(file, buffer, size);
	}

	public unsafe static void ZeroMemory(byte* buffer, int size)
	{
		UnsafeMethods.implementation.ZeroMemory(buffer, size);
	}

	public unsafe static void CopyMemory(void* target, void* source, int size)
	{
		UnsafeMethods.implementation.CopyMemory(target, source, size);
	}
}
