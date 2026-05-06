using System.Collections;

namespace UOAIO;

internal class PSellItems : Packet
{
	public PSellItems(int serial, SellInfo[] info)
		: base(159)
	{
		ArrayList dataStore = Engine.GetDataStore();
		for (int i = 0; i < info.Length; i++)
		{
			if (info[i].ToSell > 0)
			{
				dataStore.Add(info[i]);
			}
		}
		base.m_Stream.Write(serial);
		base.m_Stream.Write((ushort)dataStore.Count);
		for (int j = 0; j < dataStore.Count; j++)
		{
			SellInfo sellInfo = (SellInfo)dataStore[j];
			base.m_Stream.Write(sellInfo.Item.Serial);
			base.m_Stream.Write((ushort)sellInfo.ToSell);
		}
		Engine.ReleaseDataStore(dataStore);
	}
}
