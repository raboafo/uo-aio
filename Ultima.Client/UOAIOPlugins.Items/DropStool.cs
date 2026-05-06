using System.Collections.Generic;
using Ultima.Data;
using UOAIO;
using UOAIO.Targeting;

namespace UOAIOPlugins.Items;

public class DropStool
{
	public static readonly ActionCallback Macro_Callback;

	public static bool Dropper(string args)
	{
		bool flag = DropStool.dropStool(World.Player.X, World.Player.Y);
		Console.Print("Stool dropped " + (flag ? "success" : "failed"), Console.MessageType.Information);
		return true;
	}

	public static bool SmartStool(string args)
	{
		if (TargetManager.LastOffensiveTarget != null && !World.Player.IsDead)
		{
			foreach (Mobile value in World.Mobiles.Values)
			{
				if (!value.Visible || value != TargetManager.LastOffensiveTarget || !World.Player.InRange(value, 2))
				{
					continue;
				}
				if (value.Notoriety == Notoriety.Vendor)
				{
					break;
				}
				bool flag = false;
				Direction[] array = new Direction[4]
				{
					Direction.South,
					Direction.West,
					Direction.North,
					Direction.East
				};
				foreach (Direction direction in array)
				{
					if (flag)
					{
						break;
					}
					switch (direction)
					{
					case Direction.Up:
						if (DropStool.Walkable(value.X - 1, value.Y))
						{
							flag = DropStool.dropStool(value.X - 1, value.Y);
						}
						else if (DropStool.Walkable(value.X, value.Y - 1))
						{
							flag = DropStool.dropStool(value.X, value.Y - 1);
						}
						break;
					case Direction.North:
						if (DropStool.Walkable(value.X, value.Y - 1))
						{
							flag = DropStool.dropStool(value.X, value.Y - 1);
						}
						break;
					case Direction.Right:
						if (DropStool.Walkable(value.X, value.Y - 1))
						{
							flag = DropStool.dropStool(value.X, value.Y - 1);
						}
						else if (DropStool.Walkable(value.X + 1, value.Y))
						{
							flag = DropStool.dropStool(value.X + 1, value.Y);
						}
						break;
					case Direction.East:
						if (DropStool.Walkable(value.X + 1, value.Y))
						{
							flag = DropStool.dropStool(value.X + 1, value.Y);
						}
						break;
					case Direction.Down:
						if (DropStool.Walkable(value.X, value.Y + 1))
						{
							flag = DropStool.dropStool(value.X, value.Y + 1);
						}
						else if (DropStool.Walkable(value.X + 1, value.Y))
						{
							flag = DropStool.dropStool(value.X + 1, value.Y);
						}
						break;
					case Direction.South:
						if (DropStool.Walkable(value.X, value.Y + 1))
						{
							flag = DropStool.dropStool(value.X, value.Y + 1);
						}
						break;
					case Direction.Left:
						if (DropStool.Walkable(value.X - 1, value.Y))
						{
							flag = DropStool.dropStool(value.X - 1, value.Y);
						}
						else if (DropStool.Walkable(value.X, value.Y + 1))
						{
							flag = DropStool.dropStool(value.X, value.Y + 1);
						}
						break;
					case Direction.West:
						if (DropStool.Walkable(value.X - 1, value.Y))
						{
							flag = DropStool.dropStool(value.X - 1, value.Y);
						}
						break;
					}
				}
				return true;
			}
			return true;
		}
		return true;
	}

	public static bool dropStool(int X, int Y)
	{
		foreach (Item value in World.Items.Values)
		{
			if (value.ID == 2910 && value.X == X && value.Y == Y)
			{
				return true;
			}
		}
		foreach (Item item in World.Player.Backpack.GetItems(new ItemIDValidator(2910)))
		{
			if (item != null)
			{
				if (Network.Send(new PPickupItem(item, item.Amount)) && Network.Send(new PDropItem(item.Serial, X, Y, World.Player.Z, -1)))
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	public static bool Walkable(int X, int Y)
	{
		bool result = true;
		MapPackage cache = Map.GetCache();
		int num = X - cache.CellX;
		int num2 = Y - cache.CellY;
		List<ICell> list = cache.cells[num, num2];
		for (int i = 0; i < list.Count; i++)
		{
			ICell cell = list[i];
			if (cell is DynamicItem && Map.GetTileFlags(((DynamicItem)cell).ID)[TileFlag.Impassable])
			{
				result = false;
			}
			if (cell is StaticItem && Map.GetTileFlags(((StaticItem)cell).ID)[TileFlag.Wall | TileFlag.Impassable])
			{
				result = false;
			}
		}
		return result;
	}

	private static Point Backwards()
	{
		return Engine.movingDir switch
		{
			Direction.Up => new Point(World.Player.X + 1, World.Player.Y + 1), 
			Direction.North => new Point(World.Player.X, World.Player.Y + 1), 
			Direction.Right => new Point(World.Player.X - 1, World.Player.Y + 1), 
			Direction.East => new Point(World.Player.X + 1, World.Player.Y), 
			Direction.Down => new Point(World.Player.X - 1, World.Player.Y - 1), 
			Direction.South => new Point(World.Player.X, World.Player.Y - 1), 
			Direction.Left => new Point(World.Player.X + 1, World.Player.Y - 1), 
			Direction.West => new Point(World.Player.X - 1, World.Player.Y), 
			_ => null, 
		};
	}

	static DropStool()
	{
		DropStool.Macro_Callback = Dropper;
	}
}
