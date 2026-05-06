using System;
using System.Runtime.InteropServices;

namespace UOAIO;

public class Memory
{
	public unsafe static void* Alloc(int Size)
	{
		return (void*)Marshal.AllocHGlobal(Size);
	}

	public unsafe static void Free(void* Data)
	{
		Marshal.FreeHGlobal((IntPtr)Data);
	}
}
