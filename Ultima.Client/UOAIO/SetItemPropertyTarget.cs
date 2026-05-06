using UOAIO.Targeting;

namespace UOAIO;

internal class SetItemPropertyTarget : ClientTargetHandler
{
	private GPropertyEntry m_Entry;

	public SetItemPropertyTarget(GPropertyEntry entry)
	{
		this.m_Entry = entry;
	}

	protected override bool OnTarget(Item item)
	{
		this.m_Entry.SetValue(item);
		return true;
	}
}
