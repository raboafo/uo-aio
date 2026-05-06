namespace UOAIO;

internal class PBuyItems : Packet
{
	public PBuyItems(int serial, BuyInfo[] info)
		: base(59)
	{
		base.m_Stream.Write(serial);
		base.m_Stream.Write((byte)2);
		for (int i = 0; i < info.Length; i++)
		{
			if (info[i].ToBuy > 0)
			{
				base.m_Stream.Write((byte)26);
				base.m_Stream.Write(info[i].Item.Serial);
				base.m_Stream.Write((short)info[i].ToBuy);
			}
		}
	}
}
