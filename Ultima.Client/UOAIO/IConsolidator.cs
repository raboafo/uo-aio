namespace UOAIO;

public interface IConsolidator
{
	void Enqueue(byte[] buffer, int offset, int length);
}
