using System.IO;

namespace UOAIO;

public interface IUnsafeMethods
{
	unsafe int ReadFile(FileStream file, void* buffer, int size);

	unsafe int WriteFile(FileStream file, void* buffer, int size);

	unsafe void ZeroMemory(void* buffer, int size);

	unsafe void CopyMemory(void* target, void* source, int size);
}
