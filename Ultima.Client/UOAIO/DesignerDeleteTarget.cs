using UOAIO.Targeting;

namespace UOAIO;

internal class DesignerDeleteTarget : ClientTargetHandler
{
	private DesignContext m_Context;

	public DesignerDeleteTarget(DesignContext context)
	{
		this.m_Context = context;
	}

	protected override bool OnTarget(StaticTarget staticTarget)
	{
		int x = staticTarget.X - this.m_Context.House.X;
		int y = staticTarget.Y - this.m_Context.House.Y;
		int z = staticTarget.Z - this.m_Context.House.Z;
		if (this.m_Context.Multi.Remove(x, y, z, staticTarget.RealID))
		{
			Network.Send(new PDesigner_Remove(this.m_Context.House, x, y, z, staticTarget.RealID));
			Map.Invalidate();
		}
		return false;
	}
}
