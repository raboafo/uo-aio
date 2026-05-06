using System.IO;

namespace UOAIO;

public sealed class GameCrypto : BaseCrypto
{
	private UnpackLeaf m_Leaf;

	private byte[] _unpackBuffer;

	public GameCrypto(uint seed)
		: base(seed)
	{
	}

	protected override void InitKeys(uint seed)
	{
		this.m_Leaf = Compression.m_Tree;
	}

	public unsafe override int Decrypt(byte[] input, int inputStart, int count, byte[] output, int outputStart)
	{
		fixed (byte* ptr = output)
		{
			byte* ptr2 = ptr + outputStart;
			byte* ptr3 = ptr + output.Length;
			fixed (UnpackCacheEntry* cacheEntries = Compression.m_CacheEntries)
			{
				fixed (byte* outputBuffer = Compression.m_OutputBuffer)
				{
					fixed (byte* ptr4 = input)
					{
						UnpackLeaf unpackLeaf = this.m_Leaf;
						UnpackLeaf[] leaves = Compression.m_Leaves;
						byte* ptr5 = ptr4;
						byte* ptr6 = ptr5 + count;
						while (ptr5 < ptr6)
						{
							int num;
							UnpackCacheEntry unpackCacheEntry = cacheEntries[num = unpackLeaf.m_Cache[*(ptr5++)]];
							byte* ptr7 = outputBuffer + unpackCacheEntry.m_ByteIndex;
							byte* ptr8 = ptr2 + unpackCacheEntry.m_ByteCount;
							if (ptr8 > ptr3)
							{
								throw new InternalBufferOverflowException("Network::Decompress(): Buffer overflow.");
							}
							while (ptr2 < ptr8)
							{
								*(ptr2++) = *(ptr7++);
							}
							unpackLeaf = leaves[unpackCacheEntry.m_NextIndex];
						}
						this.m_Leaf = unpackLeaf;
					}
				}
			}
			return (int)(ptr2 - outputStart - ptr);
		}
	}

	public override void Encrypt(byte[] buffer, int start, int count)
	{
	}

	public override void Decrypt(byte[] buffer, int offset, int length, IConsolidator output)
	{
		if (this._unpackBuffer == null)
		{
			this._unpackBuffer = new byte[65536];
		}
		int length2 = this.Decrypt(buffer, offset, length, this._unpackBuffer, 0);
		output.Enqueue(this._unpackBuffer, 0, length2);
	}

	public override void Encrypt(byte[] buffer, int offset, int length, IConsolidator output)
	{
		output.Enqueue(buffer, offset, length);
	}
}
