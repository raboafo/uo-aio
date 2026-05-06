namespace UOAIO;

public class GItemCounters : GEmpty
{
	private static GItemCounter[] m_List;

	private static bool m_Valid;

	private static bool m_Active;

	public static bool Active
	{
		get
		{
			return GItemCounters.m_Active;
		}
		set
		{
			GItemCounters.m_Active = value;
		}
	}

	public GItemCounters(params ItemIDValidator[] list)
	{
		this.Y = -1;
		GItemCounters.m_List = new GItemCounter[list.Length];
		for (int i = 0; i < GItemCounters.m_List.Length; i++)
		{
			base.m_Children.Add(GItemCounters.m_List[i] = new GItemCounter(list[i]));
		}
		Timer timer = new Timer(Update_OnTick, 250);
		timer.Start(Now: true);
	}

	protected internal override void Render(int x, int y)
	{
		if (GItemCounters.m_Active && GItemCounters.m_Valid)
		{
			base.Render(x, y);
		}
	}

	private void Update_OnTick(Timer t)
	{
		if (Engine.m_Ingame && GItemCounters.m_Active)
		{
			Mobile player = World.Player;
			if (player != null)
			{
				Item backpack = player.Backpack;
				if (backpack != null)
				{
					GItemCounters.m_Valid = true;
					int num = 0;
					int num2 = 0;
					for (int i = 0; i < GItemCounters.m_List.Length; i++)
					{
						GItemCounters.m_List[i].Update(backpack);
						GItemCounters.m_List[i].Y = num;
						num += GItemCounters.m_List[i].Height - 1;
						if (GItemCounters.m_List[i].Width > num2)
						{
							num2 = GItemCounters.m_List[i].Width;
						}
					}
					for (int j = 0; j < GItemCounters.m_List.Length; j++)
					{
						GItemCounters.m_List[j].Width = num2;
					}
					this.X = Engine.ScreenWidth - num2 + 1;
					return;
				}
			}
		}
		GItemCounters.m_Valid = false;
	}
}
