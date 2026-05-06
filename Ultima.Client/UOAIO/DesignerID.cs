using System;

namespace UOAIO;

public class DesignerID
{
	private int[] m_ItemIDs;

	public int[] ItemIDs => this.m_ItemIDs;

	public int DisplayID => this.m_ItemIDs[0];

	public DesignerID(int itemID)
	{
		this.m_ItemIDs = new int[1] { itemID };
	}

	public DesignerID(int[] itemIDs)
	{
		this.m_ItemIDs = itemIDs;
	}

	public int GetRandomID()
	{
		return this.GetRandomID(Engine.Random);
	}

	public int GetRandomID(Random random)
	{
		return this.m_ItemIDs[random.Next(this.m_ItemIDs.Length)];
	}

	public static implicit operator DesignerID(int itemID)
	{
		return new DesignerID(itemID);
	}

	public static implicit operator DesignerID(int[] itemIDs)
	{
		return new DesignerID(itemIDs);
	}

	public static implicit operator int(DesignerID id)
	{
		return id.GetRandomID();
	}
}
