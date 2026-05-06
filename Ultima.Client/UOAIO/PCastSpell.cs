namespace UOAIO;

internal class PCastSpell : Packet
{
	public static int m_LastSpellID;

	public static void SendLast()
	{
		if (PCastSpell.m_LastSpellID >= 0)
		{
			Network.Send(new PCastSpell(PCastSpell.m_LastSpellID));
		}
	}

	public PCastSpell(int spellID)
		: base(191)
	{
		PCastSpell.m_LastSpellID = spellID;
		base.m_Stream.Write((ushort)28);
		base.m_Stream.Write((ushort)0);
		base.m_Stream.Write((ushort)spellID);
	}

	static PCastSpell()
	{
		PCastSpell.m_LastSpellID = -1;
	}
}
