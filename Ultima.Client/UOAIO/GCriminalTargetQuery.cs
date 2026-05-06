using UOAIO.Targeting;

namespace UOAIO;

internal class GCriminalTargetQuery : GMessageBoxYesNo
{
	private Mobile m_Mobile;

	private ServerTargetHandler m_Handler;

	public GCriminalTargetQuery(Mobile m, ServerTargetHandler handler)
		: base("This may flag\nyou criminal!", modal: true, null)
	{
		this.m_Mobile = m;
		this.m_Handler = handler;
	}

	protected override void OnSignal(bool response)
	{
		if (response)
		{
			Network.Send(new PTarget_Response(0, this.m_Handler, this.m_Mobile.Serial, this.m_Mobile.X, this.m_Mobile.Y, this.m_Mobile.Z, this.m_Mobile.Body));
		}
		else
		{
			Network.Send(new PTarget_Cancel(this.m_Handler));
		}
	}
}
