namespace UOAIO;

public interface IBufferPolicy
{
	int BufferSize { get; }

	byte[] Acquire();

	void Release(byte[] buffer);
}
