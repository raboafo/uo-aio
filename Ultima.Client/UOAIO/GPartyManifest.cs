namespace UOAIO;

public class GPartyManifest : GDragable
{
	private static Mobile m_Player;

	private static GPartyManifest m_Instance;

	private static int m_PlayerIndex;

	private static bool m_CanLoot;

	public static void Open()
	{
		if (GPartyManifest.m_Instance == null)
		{
			GPartyManifest.m_Instance = new GPartyManifest();
		}
		Gumps.Desktop.Children.Add(GPartyManifest.m_Instance);
	}

	protected internal override void OnDispose()
	{
		base.OnDispose();
		GPartyManifest.m_Instance = null;
	}

	public GPartyManifest()
		: base(2604, Engine.GameWidth / 2, Engine.GameHeight / 2)
	{
		GPartyManifest.m_Player = World.Player;
		base.m_Width = 450;
		base.m_Height = 475;
		GImage toAdd = new GImage(2604, 0, 150)
		{
			m_CanDrag = true
		};
		base.m_Children.Add(toAdd);
		toAdd = new GImage(2603, 0, 0)
		{
			m_CanDrag = true
		};
		base.m_Children.Add(toAdd);
		toAdd = new GImage(2603, 0, 125)
		{
			m_CanDrag = true
		};
		base.m_Children.Add(toAdd);
		toAdd = new GImage(2601, 20, -20)
		{
			m_CanDrag = true
		};
		base.m_Children.Add(toAdd);
		toAdd = new GImage(2607, 20, base.m_Height - 35)
		{
			m_CanDrag = true
		};
		base.m_Children.Add(toAdd);
		toAdd = new GImage(2605, base.m_Width - 25, 0)
		{
			m_CanDrag = true
		};
		base.m_Children.Add(toAdd);
		toAdd = new GImage(2605, base.m_Width - 25, 125)
		{
			m_CanDrag = true
		};
		base.m_Children.Add(toAdd);
		toAdd = new GImage(2606, 0, base.m_Height - 35)
		{
			m_CanDrag = true
		};
		base.m_Children.Add(toAdd);
		toAdd = new GImage(2600, 0, -20)
		{
			m_CanDrag = true
		};
		base.m_Children.Add(toAdd);
		toAdd = new GImage(2602, base.m_Width - 25, -20)
		{
			m_CanDrag = true
		};
		base.m_Children.Add(toAdd);
		toAdd = new GImage(2608, base.m_Width - 25, base.m_Height - 35)
		{
			m_CanDrag = true
		};
		base.m_Children.Add(toAdd);
		GLabel toAdd2 = new GLabel("Party Manifest", Engine.GetFont(3), Hues.Default, 105, 0);
		base.m_Children.Add(toAdd2);
		toAdd2 = new GLabel("Kick", Engine.GetFont(9), Hues.Load(1), 34, 31);
		base.m_Children.Add(toAdd2);
		toAdd2 = new GLabel("Tell", Engine.GetFont(9), Hues.Load(1), 84, 31);
		base.m_Children.Add(toAdd2);
		toAdd2 = new GLabel("Member Name", Engine.GetFont(9), Hues.Load(1), 150, 31);
		base.m_Children.Add(toAdd2);
		int num = 0;
		bool isLeader = Party.IsLeader;
		for (int i = 0; i < 10; i++)
		{
			if (i < Party.Members.Length)
			{
				GPartyManifest.m_PlayerIndex = i;
				if (Party.Members[i].Serial != World.Player.Serial)
				{
					GButton gButton;
					if (isLeader)
					{
						gButton = new GButton(4017, 4018, 4019, 35, 50 + num, Kick_OnClick);
						gButton.SetTag(Party.Members[i].Name, i);
						base.m_Children.Add(gButton);
					}
					gButton = new GButton(4029, 4030, 4031, 85, 50 + num, Tell_OnClick);
					gButton.SetTag(Party.Members[i].Name, i);
					base.m_Children.Add(gButton);
				}
				toAdd = new GImage(3000, 130, 60 + num);
				base.m_Children.Add(toAdd);
				toAdd2 = new GLabel(Party.Members[i].Name, Engine.DefaultFont, Hues.Load(1), 150, 50 + num);
				base.m_Children.Add(toAdd2);
			}
			else
			{
				toAdd = new GImage(3000, 130, 60 + num);
				base.m_Children.Add(toAdd);
			}
			num += 30;
		}
		if (GPartyManifest.m_Player.IsInParty)
		{
			toAdd2 = new GLabel(GPartyManifest.m_CanLoot ? "Party CAN loot me" : "Party CANNOT loot me", Engine.DefaultFont, GPartyManifest.m_CanLoot ? Hues.Load(68) : Hues.Load(38), 150, 75 + num);
			base.m_Children.Add(toAdd2);
			GButton gButton = new GButton(4008, 4009, 4010, 100, 75 + num, Loot_OnClick);
			base.m_Children.Add(gButton);
			num += 25;
			toAdd2 = new GLabel(isLeader ? "Disband the party" : "Leave the party", Engine.DefaultFont, Hues.Load(1), 150, 75 + num);
			base.m_Children.Add(toAdd2);
			gButton = new GButton(4005, 4006, 4007, 100, 75 + num, Leave_OnClick);
			base.m_Children.Add(gButton);
			num += 25;
			if (Party.IsLeader)
			{
				toAdd2 = new GLabel("Add new member", Engine.DefaultFont, Hues.Load(1), 150, 75 + num);
				base.m_Children.Add(toAdd2);
				gButton = new GButton(4008, 4009, 4010, 100, 75 + num, AddMember_OnClick);
				base.m_Children.Add(gButton);
			}
		}
		else
		{
			toAdd2 = new GLabel("Create a party", Engine.DefaultFont, Hues.Load(1), 100, 75 + num);
			base.m_Children.Add(toAdd2);
		}
	}

	private void AddMember_OnClick(Gump g)
	{
		Engine.commandEntered("/add");
	}

	private void Leave_OnClick(Gump g)
	{
		Network.Send(new PParty_Quit());
	}

	private void Kick_OnClick(Gump g)
	{
		for (int i = 0; i < Party.Members.Length; i++)
		{
			if (Party.IsLeader && g.HasTag(Party.Members[i].Name))
			{
				Network.Send(new PParty_RemoveMember(Party.Members[i].Serial));
			}
		}
	}

	private void Tell_OnClick(Gump g)
	{
		if (Party.IsLeader)
		{
			for (int i = 0; i < Party.Members.Length; i++)
			{
				if (g.HasTag(Party.Members[i].Name))
				{
					Engine.m_Text = "/" + (i + 1);
					Renderer.SetText("/" + (i + 1));
				}
			}
			return;
		}
		for (int j = 0; j < Party.Members.Length; j++)
		{
			if (g.HasTag(Party.Members[j].Name))
			{
				Engine.m_Text = "/" + (j + 1);
				Renderer.SetText("/" + (j + 1));
			}
		}
	}

	private void Loot_OnClick(Gump g)
	{
		GPartyManifest.m_CanLoot = !GPartyManifest.m_CanLoot;
		Network.Send(new PParty_SetCanLoot(GPartyManifest.m_CanLoot));
	}
}
