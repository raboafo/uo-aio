namespace UOAIO;

public sealed class LoginCrypto : BaseCrypto
{
	public LoginCrypto(uint seed)
		: base(seed)
	{
	}

	protected override void InitKeys(uint seed)
	{
	}

	public override int Decrypt(byte[] input, int inputStart, int count, byte[] output, int outputStart)
	{
		for (int i = 0; i < count; i++)
		{
			output[i + outputStart] = input[i + inputStart];
		}
		return count;
	}

	public override void Encrypt(byte[] buffer, int start, int count)
	{
	}

	public override void Decrypt(byte[] buffer, int offset, int length, IConsolidator output)
	{
		output.Enqueue(buffer, offset, length);
	}

	public override void Encrypt(byte[] buffer, int offset, int length, IConsolidator output)
	{
		output.Enqueue(buffer, offset, length);
	}
}
