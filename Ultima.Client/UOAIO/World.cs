using System;
using System.Collections.Generic;
using Ultima.Client;

namespace UOAIO;

public static class World
{
	private static readonly Viewport viewport;

	private static WorldAgent _agent;

	private static Dictionary<int, Mobile> mobiles;

	private static Dictionary<int, Item> items;

	private static int m_Serial;

	private static List<StaticMessage> m_Messages;

	private static bool hasIdentified;

	private static Mobile m_Player;

	private static int m_Range;

	private static int m_X;

	private static int m_Y;

	private static int m_Z;

	private static Mobile m_Opponent;

	public static Viewport Viewport => World.viewport;

	public static WorldAgent Agent => World._agent;

	public static bool HasIdentified
	{
		get
		{
			return World.hasIdentified;
		}
		set
		{
			World.hasIdentified = value;
		}
	}

	public static Mobile Opponent
	{
		get
		{
			return World.m_Opponent;
		}
		set
		{
			World.m_Opponent = value;
		}
	}

	public static int X
	{
		get
		{
			return World.m_X;
		}
		set
		{
			World.SetLocation(value, World.m_Y, World.m_Z);
		}
	}

	public static int Y
	{
		get
		{
			return World.m_Y;
		}
		set
		{
			World.SetLocation(World.m_X, value, World.m_Z);
		}
	}

	public static int Z
	{
		get
		{
			return World.m_Z;
		}
		set
		{
			World.m_Z = value;
			if (DesignContext.Current != null)
			{
				DesignContext.Current.Designer.UpdateLevelButtons();
			}
		}
	}

	public static int Range
	{
		get
		{
			return World.m_Range;
		}
		set
		{
			World.m_Range = value;
		}
	}

	public static int Serial
	{
		get
		{
			return World.m_Serial;
		}
		set
		{
			World.m_Serial = value;
			World.m_Player = World.FindMobile(World.m_Serial);
			World.hasIdentified = false;
			Renderer.SetText(Engine.m_Text);
		}
	}

	public static Dictionary<int, Mobile> Mobiles => World.mobiles;

	public static Dictionary<int, Item> Items => World.items;

	public static Mobile Player => World.m_Player;

	public static void SetLocation(int x, int y, int z)
	{
		World.m_Z = z;
		if (World.m_X == x && World.m_Y == y)
		{
			return;
		}
		World.m_X = x;
		World.m_Y = y;
		foreach (Mobile value in World.mobiles.Values)
		{
			if (!value.Player && !World.InUpdateRange(value))
			{
				World.Remove(value);
			}
		}
		foreach (Item value2 in World.items.Values)
		{
			if (value2.InWorld && !World.InUpdateRange(value2))
			{
				if (!value2.IsMulti)
				{
					World.Remove(value2);
				}
				else if (!World.InUpdateRange(value2, 24))
				{
					value2.Revision = -1;
				}
			}
		}
	}

	public static IEnumerable<Item> GetItems(IItemValidator validator)
	{
		if (validator == null)
		{
			throw new ArgumentNullException("validator");
		}
		return World.GetItems(validator.IsValid);
	}

	public static IEnumerable<Item> GetItems(Predicate<Item> validator)
	{
		if (validator == null)
		{
			throw new ArgumentNullException("validator");
		}
		foreach (Item item in World.items.Values)
		{
			if (item.Visible && item.InWorld && !item.IsMulti && World.InRange(item) && validator(item))
			{
				yield return item;
			}
		}
	}

	public static Item[] FindItems(IItemValidator validator)
	{
		return ScratchList<Item>.ToArray(World.GetItems(validator));
	}

	public static Item[] FindItems(Predicate<Item> validator)
	{
		return ScratchList<Item>.ToArray(World.GetItems(validator));
	}

	public static Item FindItem(params IItemValidator[] validators)
	{
		if (validators == null)
		{
			throw new ArgumentNullException("validators");
		}
		return World.FindItem(delegate(Item item)
		{
			IItemValidator[] array = validators;
			foreach (IItemValidator itemValidator in array)
			{
				if (itemValidator.IsValid(item))
				{
					return true;
				}
			}
			return false;
		});
	}

	public static Item FindItem(IItemValidator validator)
	{
		if (validator == null)
		{
			throw new ArgumentNullException("validator");
		}
		return World.FindItem(validator.IsValid);
	}

	public static Item FindItem(Predicate<Item> validator)
	{
		if (validator == null)
		{
			throw new ArgumentNullException("validator");
		}
		foreach (Item value in World.items.Values)
		{
			if (value.Visible && value.InWorld && !value.IsMulti && World.InRange(value) && validator(value))
			{
				return value;
			}
		}
		return null;
	}

	public static bool InRange(Point3D p1, Point3D p2, int range)
	{
		return p1.X >= p2.X - range && p1.X <= p2.X + range && p1.Y >= p2.Y - range && p1.Y <= p2.Y + range;
	}

	public static bool InUpdateRange(IPoint2D p)
	{
		return World.InUpdateRange(p, World.m_Range);
	}

	public static bool InUpdateRange(IPoint2D p, int range)
	{
		if (p is Mobile mobile)
		{
			return World.IsWithinRange(mobile.XReal, mobile.YReal, range);
		}
		if (p is Item { IsMulti: not false, Multi: not null } item)
		{
			item.Multi.GetBounds(out var xMin, out var yMin, out var xMax, out var yMax);
			xMin += item.X;
			xMax += item.X;
			yMin += item.Y;
			yMax += item.Y;
			if (World.m_X >= xMin - range && World.m_X <= xMax + range && World.m_Y >= yMin - range)
			{
				return World.m_Y <= yMax + range;
			}
			return false;
		}
		return World.IsWithinRange(p.X, p.Y, range);
	}

	public static bool InRange(IPoint2D p)
	{
		return World.m_Player == null || (p.X >= World.m_Player.X - World.m_Range && p.X <= World.m_Player.X + World.m_Range && p.Y >= World.m_Player.Y - World.m_Range && p.Y <= World.m_Player.Y + World.m_Range);
	}

	public static void Offset(int X, int Y)
	{
		int count = World.m_Messages.Count;
		for (int i = 0; i < count; i++)
		{
			World.m_Messages[i].Offset(X, Y);
		}
	}

	public static void AddStaticMessage(int Serial, string Message)
	{
		int count = World.m_Messages.Count;
		for (int i = 0; i < count; i++)
		{
			StaticMessage staticMessage = World.m_Messages[i];
			if (staticMessage.Serial == Serial)
			{
				return;
			}
		}
		World.m_Messages.Add(new StaticMessage(Engine.m_xClick - Engine.GameX, Engine.m_yClick - Engine.GameY, Serial, Message));
	}

	public static void DrawAllMessages()
	{
		int num = World.m_Messages.Count;
		if (num == 0)
		{
			return;
		}
		int num2 = 0;
		while (num2 < num)
		{
			StaticMessage staticMessage = World.m_Messages[num2];
			if (staticMessage.Alpha <= 0f)
			{
				World.m_Messages.RemoveAt(num2);
				num--;
				continue;
			}
			if (staticMessage.Elapsed && !staticMessage.Disposing)
			{
				staticMessage.Dispose();
			}
			Renderer.m_TextToDraw.Add(staticMessage);
			num2++;
		}
	}

	public static void Clear()
	{
		World.m_Serial = 0;
		World.m_Player = null;
		World.hasIdentified = false;
		if (World.mobiles != null)
		{
			World.mobiles.Clear();
		}
		if (World.items != null)
		{
			World.items.Clear();
		}
		if (World.m_Messages != null)
		{
			World.m_Messages.Clear();
		}
		Engine.Multis.Clear();
		Engine.m_Display.Text = "Ultima Online";
	}

	public static int GetAmount(params Item[] items)
	{
		int num = 0;
		for (int i = 0; i < items.Length; i++)
		{
			num += (ushort)items[i].Amount;
		}
		return num;
	}

	static World()
	{
		World.viewport = new Viewport();
		World._agent = new WorldAgent();
		World.m_Range = 18;
		World.mobiles = new Dictionary<int, Mobile>();
		World.items = new Dictionary<int, Item>();
		World.m_Messages = new List<StaticMessage>();
	}

	public static Item WantItem(int serial)
	{
		if (!World.items.TryGetValue(serial, out var value))
		{
			value = new Item(serial);
			World.items.Add(serial, value);
		}
		return value;
	}

	public static Item WantItem(int serial, ref bool wasFound)
	{
		if (!World.items.TryGetValue(serial, out var value))
		{
			value = new Item(serial);
			World.items.Add(serial, value);
			wasFound = false;
		}
		else
		{
			wasFound = true;
		}
		return value;
	}

	public static Mobile WantMobile(int serial, ref bool wasFound)
	{
		if (!World.mobiles.TryGetValue(serial, out var value))
		{
			value = new Mobile(serial);
			World.mobiles.Add(serial, value);
			wasFound = false;
		}
		else
		{
			wasFound = true;
		}
		return value;
	}

	public static Mobile WantMobile(int serial)
	{
		if (!World.mobiles.TryGetValue(serial, out var value))
		{
			value = new Mobile(serial);
			World.mobiles.Add(serial, value);
		}
		return value;
	}

	public static Item FindItem(int serial)
	{
		World.items.TryGetValue(serial, out var value);
		return value;
	}

	public static Mobile FindMobile(int serial)
	{
		World.mobiles.TryGetValue(serial, out var value);
		return value;
	}

	public static void Remove(Item item)
	{
		item?.Delete();
	}

	public static void Remove(Mobile m)
	{
		if (m != null && !m.Player)
		{
			m.Delete();
		}
	}

	private static bool IsWithinRange(int x, int y, int range)
	{
		if (World.m_Player != null)
		{
			if (x >= World.m_X - range && x <= World.m_X + range && y >= World.m_Y - range)
			{
				return y <= World.m_Y + range;
			}
			return false;
		}
		return true;
	}
}
