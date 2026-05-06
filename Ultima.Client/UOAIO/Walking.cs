using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Ultima.Data;
using UOAIO.Profiles;

namespace UOAIO;

public static class Walking
{
	private const float DefaultSpeed = 0.4f;

	private static float m_Speed;

	private const int PersonHeight = 16;

	private const int StepHeight = 2;

	private static DateTime m_LastLiftBlocker;

	private static bool m_Diag;

	public static float Speed
	{
		get
		{
			if (Walking.IsDefaultSpeed)
			{
				return 0.4f;
			}
			return Walking.m_Speed;
		}
		set
		{
			Walking.m_Speed = value;
		}
	}

	public static float WalkSpeed
	{
		get
		{
			return Walking.Speed;
		}
		set
		{
			Walking.m_Speed = value;
		}
	}

	public static float RunSpeed
	{
		get
		{
			return Walking.Speed / 2f;
		}
		set
		{
			Walking.m_Speed = value * 2f;
		}
	}

	public static bool IsDefaultSpeed
	{
		get
		{
			if (!Engine.GMPrivs)
			{
				return true;
			}
			return Walking.m_Speed == 0.4f;
		}
	}

	private static bool IsOk(bool ignoreMobs, bool ignoreDoors, int ourZ, int ourTop, List<ICell> tiles)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			ICell cell = tiles[i];
			if (cell is StaticItem)
			{
				StaticItem staticItem = (StaticItem)cell;
				TileFlags tileFlags = Map.m_ItemFlags[staticItem.m_RealID];
				if (tileFlags[TileFlag.Impassable | TileFlag.Surface])
				{
					int z = staticItem.m_Z;
					int num = z + staticItem.CalcHeight;
					if (num > ourZ && ourTop > z)
					{
						return false;
					}
				}
			}
			else if (cell is DynamicItem)
			{
				Item item = ((DynamicItem)cell).m_Item;
				TileFlags tileFlags2 = Map.m_ItemFlags[item.ID];
				if (!tileFlags2[TileFlag.Impassable | TileFlag.Surface] || (item.IsDoor && (ignoreDoors || !Walking.m_Diag)))
				{
					continue;
				}
				int z2 = item.Z;
				int num2 = z2;
				num2 = ((!tileFlags2[TileFlag.Bridge]) ? (num2 + item.Height) : (num2 + item.Height / 2));
				if (num2 > ourZ && ourTop > z2)
				{
					if (!item.IsMovable || (Control.ModifierKeys & Keys.Shift) == 0 || !(Walking.m_LastLiftBlocker + TimeSpan.FromSeconds(0.6) < DateTime.Now))
					{
						return false;
					}
					Walking.m_LastLiftBlocker = DateTime.Now;
					Network.Send(new PPickupItem(item, item.Amount));
					Network.Send(new PDropItem(item.Serial, -1, -1, 0, World.Serial));
				}
			}
			else
			{
				if (ignoreMobs || !(cell is MobileCell))
				{
					continue;
				}
				Mobile mobile = ((MobileCell)cell).m_Mobile;
				if (!mobile.Ghost && !mobile.IsDeadPet)
				{
					int z3 = mobile.Z;
					int num3 = z3 + 16;
					if (num3 > ourZ && ourTop > z3)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public static bool CheckMovement(int xStart, int yStart, int zStart, int dir, out int zNew)
	{
		Mobile player = World.Player;
		if (player == null)
		{
			zNew = 0;
			return false;
		}
		if (DesignContext.Current != null)
		{
			zNew = zStart;
			return true;
		}
		int x = xStart;
		int y = yStart;
		Walking.Offset(dir, ref x, ref y);
		MapPackage cache = Map.GetCache();
		int num = x - cache.CellX;
		int num2 = y - cache.CellY;
		if (!Map.IsValid(num, num2))
		{
			zNew = 0;
			return false;
		}
		List<ICell> list = cache.cells[num, num2];
		LandTile landTile = World.Viewport.GetLandTile(x, y, Engine.m_World);
		LandTile landTile2 = World.Viewport.GetLandTile(xStart, yStart, Engine.m_World);
		try
		{
			if (player.Notoriety == Notoriety.Murderer && landTile.m_Guarded && !Options.Current.SiegeRuleset && !landTile2.m_Guarded && (Control.ModifierKeys & (Keys.Shift | Keys.Control)) != (Keys.Shift | Keys.Control))
			{
				zNew = 0;
				return false;
			}
		}
		catch
		{
		}
		bool impassable = landTile.Impassable;
		bool flag = landTile.m_ID != 2 && landTile.m_ID != 475 && (landTile.m_ID < 430 || landTile.m_ID > 437);
		int z = 0;
		int avg = 0;
		int top = 0;
		Map.GetAverageZ(x, y, ref z, ref avg, ref top);
		Walking.GetStartZ(xStart, yStart, zStart, out var zLow, out var zTop);
		zNew = zLow;
		bool flag2 = false;
		int num3 = zTop + 2;
		int num4 = zLow + 16;
		bool flag3 = player.Ghost || player.Body == 987;
		bool ignoreMobs = flag3 || player.CurrentStamina == player.MaximumStamina || Engine.m_World != 0;
		if (Engine.m_Stealth)
		{
			ignoreMobs = false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			ICell cell = list[i];
			if (cell is StaticItem)
			{
				StaticItem staticItem = (StaticItem)cell;
				TileFlags tileFlags = Map.m_ItemFlags[staticItem.m_RealID];
				if (!tileFlags[TileFlag.Surface] || tileFlags[TileFlag.Impassable])
				{
					continue;
				}
				int z2 = staticItem.m_Z;
				int num5 = z2;
				int num6 = z2 + staticItem.CalcHeight;
				int num7 = num6 + 16;
				int num8 = num4;
				if (flag2)
				{
					int num9 = Math.Abs(num6 - player.Z) - Math.Abs(zNew - player.Z);
					if (num9 > 0 || (num9 == 0 && num6 > zNew))
					{
						continue;
					}
				}
				if (num6 + 16 > num8)
				{
					num8 = num6 + 16;
				}
				if (!tileFlags[TileFlag.Bridge])
				{
					num5 += staticItem.Height;
				}
				if (num3 >= num5)
				{
					int num10 = z2;
					num10 = ((staticItem.Height < 2) ? (num10 + staticItem.Height) : (num10 + 2));
					if ((!flag || num10 >= avg || avg <= num6 || num8 <= z) && Walking.IsOk(ignoreMobs, flag3, num6, num8, list))
					{
						zNew = num6;
						flag2 = true;
					}
				}
			}
			else
			{
				if (!(cell is DynamicItem))
				{
					continue;
				}
				Item item = ((DynamicItem)cell).m_Item;
				TileFlags tileFlags2 = Map.m_ItemFlags[item.ID];
				if (!tileFlags2[TileFlag.Surface] || tileFlags2[TileFlag.Impassable])
				{
					continue;
				}
				int z3 = item.Z;
				int num11 = z3;
				int num12 = z3;
				int height = item.Height;
				num12 = ((!tileFlags2[TileFlag.Bridge]) ? (num12 + height) : (num12 + height / 2));
				if (flag2)
				{
					int num13 = Math.Abs(num12 - player.Z) - Math.Abs(zNew - player.Z);
					if (num13 > 0 || (num13 == 0 && num12 > zNew))
					{
						continue;
					}
				}
				int num14 = num12 + 16;
				int num15 = num4;
				if (num12 + 16 > num15)
				{
					num15 = num12 + 16;
				}
				if (!tileFlags2[TileFlag.Bridge])
				{
					num11 += height;
				}
				if (num3 >= num11)
				{
					int num16 = z3;
					num16 = ((height < 2) ? (num16 + height) : (num16 + 2));
					if ((!flag || num16 >= avg || avg <= num12 || num15 <= z) && Walking.IsOk(ignoreMobs, flag3, num12, num15, list))
					{
						zNew = num12;
						flag2 = true;
					}
				}
			}
		}
		if (flag && !impassable && num3 >= z)
		{
			int num17 = avg;
			int num18 = num17 + 16;
			int num19 = num4;
			if (num17 + 16 > num19)
			{
				num19 = num17 + 16;
			}
			bool flag4 = true;
			if (flag2)
			{
				int num20 = Math.Abs(num17 - player.Z) - Math.Abs(zNew - player.Z);
				if (num20 > 0 || (num20 == 0 && num17 > zNew))
				{
					flag4 = false;
				}
			}
			if (flag4 && Walking.IsOk(ignoreMobs, flag3, num17, num19, list))
			{
				zNew = num17;
				flag2 = true;
			}
		}
		return flag2;
	}

	private unsafe static void GetStartZ(int xStart, int yStart, int zStart, out int zLow, out int zTop)
	{
		MapPackage cache = Map.GetCache();
		int num = xStart - cache.CellX;
		int num2 = yStart - cache.CellY;
		if (!Map.IsValid(num, num2))
		{
			zLow = zStart;
			zTop = zStart;
			return;
		}
		LandTile landTile = World.Viewport.GetLandTile(xStart, yStart, Engine.m_World);
		List<ICell> list = cache.cells[num, num2];
		bool impassable = landTile.Impassable;
		bool flag = landTile.m_ID != 2 && landTile.m_ID != 475 && (landTile.m_ID < 430 || landTile.m_ID > 437);
		int z = 0;
		int avg = 0;
		int top = 0;
		Map.GetAverageZ(xStart, yStart, ref z, ref avg, ref top);
		int num3 = (zLow = (zTop = 0));
		bool flag2 = false;
		if (flag && !impassable && zStart >= avg && (!flag2 || avg >= num3))
		{
			zLow = z;
			num3 = avg;
			if (!flag2 || top > zTop)
			{
				zTop = top;
			}
			flag2 = true;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (!(list[i] is IItem item))
			{
				continue;
			}
			ItemData* itemDataPointer = Map.GetItemDataPointer(item.ItemId);
			TileFlag flags = itemDataPointer->Flags;
			if ((flags & TileFlag.Surface) == 0)
			{
				continue;
			}
			bool flag3 = (flags & TileFlag.Bridge) != 0;
			byte height = itemDataPointer->Height;
			int z2 = item.Z;
			int num4 = z2 + (flag3 ? (height / 2) : height);
			int num5 = z2 + itemDataPointer->Height;
			if (zStart >= num4 && (!flag2 || num4 >= num3))
			{
				num3 = num4;
				if (!flag2 || num5 > zTop)
				{
					zTop = num5;
				}
				zLow = z2;
				flag2 = true;
			}
		}
		if (!flag2)
		{
			zLow = (zTop = zStart);
		}
		else if (zStart > zTop)
		{
			zTop = zStart;
		}
	}

	public static bool Calculate(int x, int y, int z, Direction dir, out int newZ, out int newDir)
	{
		int walkDirection = Engine.GetWalkDirection(dir);
		newZ = z;
		newDir = walkDirection;
		if (!Walking.IsDiagonal(walkDirection))
		{
			int num = Walking.Turn(walkDirection, 1);
			int num2 = Walking.Turn(walkDirection, -1);
			Walking.m_Diag = true;
			int zNew;
			bool flag = Walking.CheckMovement(x, y, z, num, out zNew);
			int zNew2;
			bool flag2 = Walking.CheckMovement(x, y, z, num2, out zNew2);
			Walking.m_Diag = false;
			int zNew3;
			bool flag3 = Walking.CheckMovement(x, y, z, walkDirection, out zNew3);
			Mobile player = World.Player;
			if (flag3 && ((player.Body == 987) ? (flag || flag2) : (flag && flag2)))
			{
				newZ = zNew3;
			}
			else if (flag)
			{
				newZ = zNew;
				newDir = num;
			}
			else
			{
				if (!flag2)
				{
					return false;
				}
				newZ = zNew2;
				newDir = num2;
			}
			return true;
		}
		if (Walking.CheckMovement(x, y, z, walkDirection, out newZ))
		{
			return true;
		}
		return false;
	}

	public static bool IsDiagonal(int dir)
	{
		return (dir & 1) == 0;
	}

	public static int Turn(int dir, int offset)
	{
		return (((dir & 7) + offset) & 7) | (dir & 0x80);
	}

	public static void Offset(int dir, ref int x, ref int y)
	{
		switch (dir & 7)
		{
		case 0:
			y--;
			break;
		case 1:
			x++;
			y--;
			break;
		case 2:
			x++;
			break;
		case 3:
			x++;
			y++;
			break;
		case 4:
			y++;
			break;
		case 5:
			x--;
			y++;
			break;
		case 6:
			x--;
			break;
		case 7:
			x--;
			y--;
			break;
		}
	}

	static Walking()
	{
		Walking.m_Speed = 0.4f;
	}
}
