using System;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

namespace UOAIO;

[SuppressUnmanagedCodeSecurity]
public sealed class KernelUnsafeMethods : IUnsafeMethods
{
	private static class Kernel32
	{
		public const string DllName = "kernel32";

		[DllImport("kernel32", SetLastError = true)]
		public unsafe static extern int ReadFile(SafeHandle handle, void* buffer, int size, out int read, IntPtr zero);

		[DllImport("kernel32", SetLastError = true)]
		public unsafe static extern int WriteFile(SafeHandle handle, void* buffer, int size, out int written, IntPtr zero);

		[DllImport("kernel32", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public unsafe static extern void ZeroMemory(void* buffer, int size);

		[DllImport("kernel32", EntryPoint = "RtlMoveMemory", ExactSpelling = true)]
		public unsafe static extern void CopyMemory(void* target, void* source, int size);
	}

	public unsafe int ReadFile(FileStream file, void* buffer, int size)
	{
		if (Kernel32.ReadFile(file.SafeFileHandle, buffer, size, out var read, IntPtr.Zero) == 0)
		{
			file.SafeFileHandle.SetHandleAsInvalid();
			return -1;
		}
		return read;
	}

	public unsafe int WriteFile(FileStream file, void* buffer, int size)
	{
		if (Kernel32.WriteFile(file.SafeFileHandle, buffer, size, out var written, IntPtr.Zero) == 0)
		{
			file.SafeFileHandle.SetHandleAsInvalid();
			return -1;
		}
		return written;
	}

	public unsafe void ZeroMemory(void* buffer, int size)
	{
		Kernel32.ZeroMemory(buffer, size);
	}

	public unsafe void CopyMemory(void* target, void* source, int size)
	{
		Kernel32.CopyMemory(target, source, size);
	}
}
