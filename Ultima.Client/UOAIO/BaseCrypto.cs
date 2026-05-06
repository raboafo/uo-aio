namespace UOAIO;

public abstract class BaseCrypto
{
	private uint m_Seed;

	public uint Seed => this.m_Seed;

	public BaseCrypto(uint seed)
	{
		this.m_Seed = seed;
		this.InitKeys(seed);
	}

	protected abstract void InitKeys(uint seed);

	public abstract int Decrypt(byte[] input, int inputStart, int count, byte[] output, int outputStart);

	public abstract void Encrypt(byte[] buffer, int start, int count);

	public abstract void Decrypt(byte[] buffer, int offset, int length, IConsolidator output);

	public abstract void Encrypt(byte[] buffer, int offset, int length, IConsolidator output);
}
