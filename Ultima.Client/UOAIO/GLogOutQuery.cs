namespace UOAIO;

public class GLogOutQuery : GMessageBoxYesNo
{
	private static GLogOutQuery m_Instance;

	public static void Display()
	{
		if (GLogOutQuery.m_Instance == null)
		{
			GLogOutQuery.m_Instance = new GLogOutQuery();
			Gumps.Desktop.Children.Add(GLogOutQuery.m_Instance);
		}
	}

	protected internal override void OnDispose()
	{
		GLogOutQuery.m_Instance = null;
	}

	private GLogOutQuery()
		: base("Quit\nUltima Online?", modal: false, null)
	{
	}

	protected override void OnSignal(bool response)
	{
		if (response)
		{
			Engine.m_Ingame = false;
			Network.Send(new PDisconnect());
			Network.Disconnect();
			Engine.ShowAcctLogin();
		}
	}
}
