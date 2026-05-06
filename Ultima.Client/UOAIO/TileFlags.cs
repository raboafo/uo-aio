using Ultima.Data;

namespace UOAIO;

public struct TileFlags
{
	private TileFlag value;

	public TileFlag Value
	{
		get
		{
			return this.value;
		}
		set
		{
			this.value = value;
		}
	}

	public ulong Value64 => (ulong)this.value;

	public bool this[TileFlag flag]
	{
		get
		{
			return (this.value & flag) != 0;
		}
		set
		{
			if (value)
			{
				this.value |= flag;
			}
			else
			{
				this.value &= ~flag;
			}
		}
	}

	public TileFlags(TileFlag value)
	{
		this.value = value;
	}

	public override string ToString()
	{
		TileFlag tileFlag = this.value;
		return tileFlag.ToString();
	}
}
