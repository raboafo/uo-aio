using System.Collections;

namespace UOAIO;

internal class PGumpButton : Packet
{
	public PGumpButton(int serial, int dialogID, int buttonID)
		: base(177)
	{
		base.m_Stream.Write(serial);
		base.m_Stream.Write(dialogID);
		base.m_Stream.Write(buttonID);
		base.m_Stream.Write(0);
		base.m_Stream.Write(0);
	}

	public PGumpButton(GServerGump gump, int buttonID)
		: base(177)
	{
		base.m_Stream.Write(gump.Serial);
		base.m_Stream.Write(gump.DialogID);
		base.m_Stream.Write(buttonID);
		ArrayList dataStore = Engine.GetDataStore();
		ArrayList dataStore2 = Engine.GetDataStore();
		Gump[] array = gump.Children.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is IRelayedSwitch)
			{
				IRelayedSwitch relayedSwitch = (IRelayedSwitch)array[i];
				if (relayedSwitch.Active)
				{
					dataStore.Add(relayedSwitch.RelayID);
				}
			}
			else if (array[i] is GServerTextBox)
			{
				dataStore2.Add(array[i]);
			}
		}
		base.m_Stream.Write(dataStore.Count);
		for (int j = 0; j < dataStore.Count; j++)
		{
			base.m_Stream.Write((int)dataStore[j]);
		}
		base.m_Stream.Write(dataStore2.Count);
		for (int k = 0; k < dataStore2.Count; k++)
		{
			GServerTextBox gServerTextBox = (GServerTextBox)dataStore2[k];
			base.m_Stream.Write((short)gServerTextBox.RelayID);
			base.m_Stream.Write((short)gServerTextBox.String.Length);
			base.m_Stream.WriteUnicode(gServerTextBox.String);
		}
		Engine.ReleaseDataStore(dataStore2);
		Engine.ReleaseDataStore(dataStore);
	}
}
