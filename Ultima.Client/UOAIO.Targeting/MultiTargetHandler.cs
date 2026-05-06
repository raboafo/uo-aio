namespace UOAIO.Targeting;

internal class MultiTargetHandler : ServerTargetHandler
{
	public MultiTargetHandler(int targetID)
		: base(targetID, allowGround: true, AggressionType.Neutral)
	{
	}

	protected override bool OnTarget(Mobile mob)
	{
		return false;
	}

	protected override bool OnTarget(Item item)
	{
		return false;
	}

	protected override void Dispatch(int type, int serial, int x, int y, int z, int id)
	{
		Network.Send(new PMultiTarget_Response(base.targetID, x, y, z, id));
		Engine.m_MultiPreview = false;
	}

	protected override void OnCancel()
	{
		Engine.m_MultiPreview = false;
		Network.Send(new PMultiTarget_Cancel(base.targetID));
	}
}
