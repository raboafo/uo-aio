using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ultima.Client;
using Ultima.Data;

namespace UOAIO;

public class Map
{
	private static Vector m_vLight;

	private static int m_X;

	private static int m_Y;

	public static int m_Width;

	public static int m_Height;

	private static int m_World;

	private static MapPackage m_Cached;

	private static bool m_IsCached;

	private static readonly TileData tileData;

	private static AnimData[] m_Anim;

	public static TileFlags[] m_ItemFlags;

	public static Layer m_ItemLayer;

	private unsafe static sbyte* m_pAnims;

	private static int[] m_InvalidLandTiles;

	private static Point3DList m_PathList;

	private static int m_MaxLOSDistance;

	private const string RelativeApplicationDataPath = "Veritas/Ultima Online/Cache/TileData";

	private const string RelativeLegacyPath = "data/ultima/cache/tiledata.uoi";

	private static TileMatrix m_Felucca;

	private static TileMatrix m_Trammel;

	private static TileMatrix m_Ilshenar;

	private static TileMatrix m_Malas;

	private static TileMatrix m_Tokuno;

	private static TileMatrix m_TerMur;

	private static bool m_Locked;

	private static Queue<ILocked> m_LockQueue;

	private static bool m_QueueInvalidate;

	private static Type tLandTile;

	private static Type tDynamicItem;

	private static Type tStaticItem;

	private static Type tMobileCell;

	private static List<ICell>[,] m_CellPool;

	private static byte[,] m_FlagPool;

	private static byte[,] m_IndexPool;

	private static LandTile[,] m_LandTiles;

	private static MapBlock[] m_StrongReferences;

	public static int[] InvalidLandTiles
	{
		get
		{
			return Map.m_InvalidLandTiles;
		}
		set
		{
			Map.m_InvalidLandTiles = value;
		}
	}

	public static int MaxLOSDistance
	{
		get
		{
			return Map.m_MaxLOSDistance;
		}
		set
		{
			Map.m_MaxLOSDistance = value;
		}
	}

	public static TileMatrix Felucca
	{
		get
		{
			if (Map.m_Felucca == null)
			{
				Map.m_Felucca = new TileMatrix(0, 0, 7168, 4096);
			}
			return Map.m_Felucca;
		}
	}

	public static TileMatrix Trammel
	{
		get
		{
			if (Map.m_Trammel == null)
			{
				Map.m_Trammel = new TileMatrix(0, 1, 7168, 4096);
			}
			return Map.m_Trammel;
		}
	}

	public static TileMatrix Ilshenar
	{
		get
		{
			if (Map.m_Ilshenar == null)
			{
				Map.m_Ilshenar = new TileMatrix(2, 2, 2304, 1600);
			}
			return Map.m_Ilshenar;
		}
	}

	public static TileMatrix Malas
	{
		get
		{
			if (Map.m_Malas == null)
			{
				Map.m_Malas = new TileMatrix(3, 3, 2560, 2048);
			}
			return Map.m_Malas;
		}
	}

	public static TileMatrix Tokuno
	{
		get
		{
			if (Map.m_Tokuno == null)
			{
				Map.m_Tokuno = new TileMatrix(4, 4, 1448, 1448);
			}
			return Map.m_Tokuno;
		}
	}

	public static TileMatrix TerMur
	{
		get
		{
			if (Map.m_TerMur == null)
			{
				Map.m_TerMur = new TileMatrix(5, 5, 1280, 4096);
			}
			return Map.m_TerMur;
		}
	}

	public static bool InRange(IPoint2D p)
	{
		return p.X >= Map.m_Cached.CellX && p.X <= Map.m_Cached.CellX + Renderer.cellWidth && p.Y >= Map.m_Cached.CellY && p.Y <= Map.m_Cached.CellY + Renderer.cellHeight;
	}

	public static int GetZ(int x, int y, int w)
	{
		return Map.GetMatrix(w).GetLandTile(x, y).z;
	}

	public static int FloorAverage(int a, int b)
	{
		int num = a + b;
		if (num < 0)
		{
			num--;
		}
		return num / 2;
	}

	public static void GetAverageZ(int x, int y, ref int z, ref int avg, ref int top)
	{
		int z2 = Map.GetZ(x, y, Engine.m_World);
		int z3 = Map.GetZ(x, y + 1, Engine.m_World);
		int z4 = Map.GetZ(x + 1, y, Engine.m_World);
		int z5 = Map.GetZ(x + 1, y + 1, Engine.m_World);
		z = z2;
		if (z3 < z)
		{
			z = z3;
		}
		if (z4 < z)
		{
			z = z4;
		}
		if (z5 < z)
		{
			z = z5;
		}
		top = z2;
		if (z3 > top)
		{
			top = z3;
		}
		if (z4 > top)
		{
			top = z4;
		}
		if (z5 > top)
		{
			top = z5;
		}
		if (Math.Abs(z2 - z5) > Math.Abs(z3 - z4))
		{
			avg = Map.FloorAverage(z3, z4);
		}
		else
		{
			avg = Map.FloorAverage(z2, z5);
		}
	}

	public static int GetAverageZ(int x, int y)
	{
		int z = 0;
		int avg = 0;
		int top = 0;
		Map.GetAverageZ(x, y, ref z, ref avg, ref top);
		return avg;
	}

	public static AnimData GetAnim(int ItemID)
	{
		return Map.m_Anim[ItemID];
	}

	[Obsolete("etc", false)]
	public unsafe static short GetTexture(int landId)
	{
		return (short)Map.GetLandDataPointer(landId)->TextureId;
	}

	[Obsolete("etc", false)]
	private unsafe static LandData* GetLandDataPointer(int landId)
	{
		return Map.GetLandDataPointer((LandId)landId);
	}

	public unsafe static LandData* GetLandDataPointer(LandId landId)
	{
		return Map.tileData.GetLandDataPointer(landId);
	}

	[Obsolete("etc", false)]
	public unsafe static ItemData* GetItemDataPointer(int itemId)
	{
		return Map.GetItemDataPointer((ItemId)itemId);
	}

	public unsafe static ItemData* GetItemDataPointer(ItemId itemId)
	{
		return Map.tileData.GetItemDataPointer(itemId);
	}

	[Obsolete("etc", false)]
	public unsafe static TileFlags GetLandFlags(int landId)
	{
		return new TileFlags(Map.GetLandDataPointer(landId)->Flags);
	}

	[Obsolete("etc", false)]
	public unsafe static int GetWeight(int itemId)
	{
		return Map.GetItemDataPointer(itemId)->Weight;
	}

	[Obsolete("etc", false)]
	public static TileFlags GetTileFlags(int TileID)
	{
		if (TileID >= 16384)
		{
			return Map.m_ItemFlags[TileID];
		}
		return Map.GetLandFlags(TileID);
	}

	public unsafe static byte GetItemHeight(ItemId itemId)
	{
		return Map.GetItemDataPointer(itemId)->Height;
	}

	[Obsolete("etc", false)]
	public unsafe static byte GetHeight(int tileId)
	{
		if (tileId >= 16384)
		{
			return Map.GetItemDataPointer((ItemId)(tileId - 16384))->Height;
		}
		return 0;
	}

	[Obsolete("Use GetQuality( ItemId ) instead.", false)]
	public unsafe static byte GetQuality(int tileId)
	{
		return Map.GetItemDataPointer((ItemId)tileId)->quality_layer_light;
	}

	[Obsolete("Use GetAnimation( ItemId ) instead.", false)]
	public unsafe static short GetAnimation(int itemId)
	{
		return (short)Map.GetItemDataPointer(itemId)->AnimationId;
	}

	public unsafe static void Shutdown()
	{
		if (Map.m_pAnims != null)
		{
			Memory.Free(Map.m_pAnims);
		}
		for (int i = 0; i < 16384; i++)
		{
			Map.m_Anim[i].pvFrames = null;
		}
		Map.m_Anim = null;
		Map.m_CellPool = null;
		if (Map.tileData != null)
		{
			Map.tileData.Dispose();
		}
	}

	public static int GetDispID(int id, int amount, ref bool xDouble)
	{
		xDouble = amount > 1 && Map.m_ItemFlags[id][TileFlag.Generic];
		if (id >= 3818 && id <= 3826)
		{
			int num = (id - 3818) / 3;
			num *= 3;
			num += 3818;
			xDouble = false;
			id = ((amount <= 1) ? num : ((amount > 5) ? (num + 2) : (num + 1)));
		}
		return id;
	}

	public static bool LineOfSight(object from, object dest)
	{
		if (from == dest)
		{
			return true;
		}
		return Map.LineOfSight(Map.GetPoint(from, eye: true), Map.GetPoint(dest, eye: false));
	}

	public unsafe static Point3D GetPoint(object o, bool eye)
	{
		Point3D result;
		if (o is Mobile)
		{
			Mobile mobile = (Mobile)o;
			result = new Point3D(mobile.X, mobile.Y, mobile.Z);
			result.Z += 14;
		}
		else if (o is Item)
		{
			Item item = (Item)o;
			result = new Point3D(item.X, item.Y, item.Z);
			result.Z += item.ItemDataPointer->Height / 2 + 1;
		}
		else
		{
			if (o is Point3D result2)
			{
				return result2;
			}
			if (o is IPoint3D)
			{
				result = new Point3D((IPoint3D)o);
			}
			else
			{
				Console.WriteLine("Warning: Invalid object ({0}) in line of sight", o);
				result = new Point3D(0, 0, 0);
			}
		}
		return result;
	}

	public static bool LineOfSight(Mobile from, Point3D target)
	{
		Point3D org = new Point3D(from.X, from.Y, from.Z);
		org.Z += 14;
		return Map.LineOfSight(org, target);
	}

	public static bool CanFit(int x, int y, int z, int height, bool checkMobiles, bool requireSurface)
	{
		MapPackage cache = Map.GetCache();
		int num = x - cache.CellX;
		int num2 = y - cache.CellY;
		if (num < 0 || num2 < 0 || num >= Renderer.cellWidth || num2 >= Renderer.cellHeight)
		{
			return false;
		}
		bool result = !requireSurface;
		List<ICell> list = cache.cells[num, num2];
		for (int i = 0; i < list.Count; i++)
		{
			object obj = list[i];
			if (obj is LandTile)
			{
				LandTile landTile = obj as LandTile;
				int z2 = 0;
				int avg = 0;
				int top = 0;
				Map.GetAverageZ(x, y, ref z2, ref avg, ref top);
				TileFlags landFlags = Map.GetLandFlags(landTile.ID & 0x3FFF);
				if (landFlags[TileFlag.Impassable] && avg > z && z + height > z2)
				{
					return false;
				}
				if (!landFlags[TileFlag.Impassable] && z == avg && !landTile.Ignored)
				{
					result = true;
				}
			}
			else if (obj is StaticItem)
			{
				StaticItem staticItem = obj as StaticItem;
				TileFlags tileFlags = Map.m_ItemFlags[staticItem.ID];
				bool flag = tileFlags[TileFlag.Surface];
				bool flag2 = tileFlags[TileFlag.Impassable];
				if ((flag || flag2) && staticItem.Z + staticItem.CalcHeight > z && z + height > staticItem.Z)
				{
					return false;
				}
				if (flag && !flag2 && z == staticItem.Z + staticItem.CalcHeight)
				{
					result = true;
				}
			}
			else if (obj is DynamicItem)
			{
				DynamicItem dynamicItem = obj as DynamicItem;
				TileFlags tileFlags2 = Map.m_ItemFlags[dynamicItem.ID];
				bool flag3 = tileFlags2[TileFlag.Surface];
				bool flag4 = tileFlags2[TileFlag.Impassable];
				if ((flag3 || flag4) && dynamicItem.Z + dynamicItem.CalcHeight > z && z + height > dynamicItem.Z)
				{
					return false;
				}
				if (flag3 && !flag4 && z == dynamicItem.Z + dynamicItem.CalcHeight)
				{
					result = true;
				}
			}
			else if (checkMobiles && obj is MobileCell)
			{
				MobileCell mobileCell = obj as MobileCell;
				if (mobileCell.Z + 16 > z && z + mobileCell.Height > mobileCell.Z)
				{
					return false;
				}
			}
		}
		return result;
	}

	public static bool LineOfSight(Mobile from, Mobile to)
	{
		if (from == to)
		{
			return true;
		}
		Point3D org = new Point3D(from.X, from.Y, from.Z);
		Point3D dest = new Point3D(to.X, to.Y, to.Z);
		org.Z += 14;
		dest.Z += 14;
		return Map.LineOfSight(org, dest);
	}

	public static bool NumberBetween(double num, int bound1, int bound2, double allowance)
	{
		if (bound1 > bound2)
		{
			int num2 = bound1;
			bound1 = bound2;
			bound2 = num2;
		}
		return num < (double)bound2 + allowance && num > (double)bound1 - allowance;
	}

	public static void FixPoints(ref Point3D top, ref Point3D bottom)
	{
		if (bottom.X < top.X)
		{
			int x = top.X;
			top.X = bottom.X;
			bottom.X = x;
		}
		if (bottom.Y < top.Y)
		{
			int y = top.Y;
			top.Y = bottom.Y;
			bottom.Y = y;
		}
		if (bottom.Z < top.Z)
		{
			int z = top.Z;
			top.Z = bottom.Z;
			bottom.Z = z;
		}
	}

	public unsafe static bool LineOfSight(Point3D org, Point3D dest)
	{
		if (!World.InRange(org, dest, Map.m_MaxLOSDistance))
		{
			return false;
		}
		Point3D point3D = org;
		Point3D point3D2 = dest;
		if (org.X > dest.X || (org.X == dest.X && org.Y > dest.Y) || (org.X == dest.X && org.Y == dest.Y && org.Z > dest.Z))
		{
			Point3D point3D3 = org;
			org = dest;
			dest = point3D3;
		}
		Point3DList pathList = Map.m_PathList;
		if (org == dest)
		{
			return true;
		}
		if (pathList.Count > 0)
		{
			pathList.Clear();
		}
		int num = dest.X - org.X;
		int num2 = dest.Y - org.Y;
		int num3 = dest.Z - org.Z;
		double num4 = Math.Sqrt(num * num + num2 * num2);
		double num5 = ((num3 == 0) ? num4 : Math.Sqrt(num4 * num4 + (double)(num3 * num3)));
		double num6 = (double)(float)num2 / num5;
		double num7 = (double)(float)num / num5;
		num4 = (double)(float)num3 / num5;
		double num8 = org.Y;
		double num9 = org.Z;
		double num10 = org.X;
		Point3D last;
		while (Map.NumberBetween(num10, dest.X, org.X, 0.5) && Map.NumberBetween(num8, dest.Y, org.Y, 0.5) && Map.NumberBetween(num9, dest.Z, org.Z, 0.5))
		{
			int num11 = (int)Math.Round(num10);
			int num12 = (int)Math.Round(num8);
			int num13 = (int)Math.Round(num9);
			if (pathList.Count > 0)
			{
				last = pathList.Last;
				if (last.X != num11 || last.Y != num12 || last.Z != num13)
				{
					pathList.Add(num11, num12, num13);
				}
			}
			else
			{
				pathList.Add(num11, num12, num13);
			}
			num10 += num7;
			num8 += num6;
			num9 += num4;
		}
		if (pathList.Count == 0)
		{
			return true;
		}
		last = pathList.Last;
		if (last != dest)
		{
			pathList.Add(dest);
		}
		Point3D top = org;
		Point3D bottom = dest;
		Map.FixPoints(ref top, ref bottom);
		int count = pathList.Count;
		MapPackage cache = Map.GetCache();
		for (int i = 0; i < count; i++)
		{
			Point3D point3D4 = pathList[i];
			int num14 = point3D4.X - cache.CellX;
			int num15 = point3D4.Y - cache.CellY;
			if (num14 < 0 || num15 < 0 || num14 >= Renderer.cellWidth || num15 >= Renderer.cellHeight)
			{
				return false;
			}
			List<ICell> list = cache.cells[num14, num15];
			bool flag = false;
			bool flag2 = false;
			for (int j = 0; j < list.Count; j++)
			{
				ICell cell = list[j];
				if (cell is LandTile)
				{
					LandTile landTile = (LandTile)cell;
					for (int k = 0; k < Map.m_InvalidLandTiles.Length; k++)
					{
						if (landTile.ID == Map.m_InvalidLandTiles[k])
						{
							flag = true;
							break;
						}
					}
					int z = 0;
					int avg = 0;
					int top2 = 0;
					Map.GetAverageZ(point3D4.X, point3D4.Y, ref z, ref avg, ref top2);
					if (z <= point3D4.Z && top2 >= point3D4.Z && (point3D4.X != point3D2.X || point3D4.Y != point3D2.Y || z > point3D2.Z || top2 < point3D2.Z) && !landTile.Ignored)
					{
						return false;
					}
				}
				else if (cell is StaticItem)
				{
					flag2 = true;
					StaticItem staticItem = (StaticItem)cell;
					ItemData* itemDataPointer = staticItem.ItemDataPointer;
					TileFlag flags = itemDataPointer->Flags;
					int num16 = itemDataPointer->Height;
					if ((flags & TileFlag.Bridge) != 0)
					{
						num16 /= 2;
					}
					if (staticItem.m_Z <= point3D4.Z && staticItem.m_Z + num16 >= point3D4.Z && (flags & (TileFlag.Window | TileFlag.NoShoot)) != 0 && (point3D4.X != point3D2.X || point3D4.Y != point3D2.Y || staticItem.m_Z > point3D2.Z || staticItem.m_Z + num16 < point3D2.Z))
					{
						return false;
					}
				}
				else if (cell is DynamicItem)
				{
					flag2 = true;
					DynamicItem dynamicItem = (DynamicItem)cell;
					ItemData* itemDataPointer2 = Map.GetItemDataPointer(dynamicItem.ItemId);
					TileFlag flags = itemDataPointer2->Flags;
					int num16 = itemDataPointer2->Height;
					if ((flags & TileFlag.Bridge) != 0)
					{
						num16 /= 2;
					}
					if (dynamicItem.m_Z <= point3D4.Z && dynamicItem.m_Z + num16 >= point3D4.Z && (flags & (TileFlag.Window | TileFlag.NoShoot)) != 0 && (point3D4.X != point3D2.X || point3D4.Y != point3D2.Y || dynamicItem.m_Z > point3D2.Z || dynamicItem.m_Z + num16 < point3D2.Z))
					{
						return false;
					}
				}
			}
			if (flag && !flag2)
			{
				return false;
			}
		}
		return true;
	}

	private static string GetCachePath()
	{
		string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Veritas/Ultima Online/Cache/TileData");
		DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(text));
		if (!directoryInfo.Exists)
		{
			directoryInfo.Create();
		}
		return text;
	}

	unsafe static Map()
	{
		Map.m_InvalidLandTiles = new int[1] { 580 };
		Map.m_PathList = new Point3DList();
		Map.m_MaxLOSDistance = 18;
		Map.m_LockQueue = new Queue<ILocked>();
		Map.tLandTile = typeof(LandTile);
		Map.tDynamicItem = typeof(DynamicItem);
		Map.tStaticItem = typeof(StaticItem);
		Map.tMobileCell = typeof(MobileCell);
		Debug.TimeBlock("Initializing Map");
		Map.m_pAnims = (sbyte*)Memory.Alloc(4194304);
		string cachePath = Map.GetCachePath();
		if (!File.Exists(cachePath))
		{
			string text = Engine.FileManager.BasePath("data/ultima/cache/tiledata.uoi");
			if (File.Exists(text))
			{
				try
				{
					File.Move(text, cachePath);
				}
				catch
				{
					File.Copy(text, cachePath, overwrite: false);
				}
			}
		}
		Map.tileData = new TileData(Engine.FileManager.ResolveMUL(Files.Tiledata));
		Map.m_Anim = new AnimData[65536];
		int num = 4484832;
		byte[] array = new byte[num];
		Stream stream = Engine.FileManager.OpenMUL(Files.Animdata);
		stream.Read(array, 0, num);
		stream.Close();
		fixed (AnimData* anim = Map.m_Anim)
		{
			AnimData* ptr = anim;
			fixed (byte* ptr2 = array)
			{
				byte* ptr3 = ptr2;
				int num2 = 0;
				sbyte* ptr4 = Map.m_pAnims;
				while (num2++ < 8184)
				{
					ptr3 += 4;
					int num3 = 0;
					while (num3++ < 8)
					{
						ptr->pvFrames = ptr4;
						ptr4 += 64;
						sbyte* ptr5 = (sbyte*)ptr3;
						for (int i = 0; i < 64; i++)
						{
							ptr->pvFrames[i] = *(ptr5++);
						}
						ptr3 += 64;
						ptr->unknown = *(ptr3++);
						ptr->frameCount = *(ptr3++);
						ptr->frameInterval = *(ptr3++);
						ptr->frameStartInterval = *(ptr3++);
						ptr++;
					}
				}
			}
		}
		Map.Patch();
		Map.m_ItemFlags = new TileFlags[65500];
		for (int j = 0; j < Map.m_ItemFlags.Length; j++)
		{
			Map.m_ItemFlags[j] = new TileFlags(Map.GetItemDataPointer(j)->Flags);
		}
		Debug.EndBlock();
	}

	private static void Patch(IEnumerable<int> source, Action<int> thunk)
	{
		foreach (int item in source)
		{
			thunk(item);
		}
	}

	private static IEnumerable<int> GetNoDrawItemIds()
	{
		yield return 1;
		int itemId = 8600;
		while (itemId <= 8612)
		{
			yield return itemId;
			int num = itemId + 1;
			itemId = num;
		}
		yield return 8636;
		yield return 22160;
	}

	private unsafe static Action<int> SetTileFlag(TileFlag flag)
	{
		return delegate(int itemId)
		{
			Map.GetItemDataPointer((ItemId)itemId)->Flags |= flag;
		};
	}

	private unsafe static Action<int> ClearTileFlag(TileFlag flag)
	{
		return delegate(int itemId)
		{
			Map.GetItemDataPointer((ItemId)itemId)->Flags &= ~flag;
		};
	}

	private static void Patch(Action<int> thunk, int itemId)
	{
		thunk(itemId);
	}

	private static void Patch(Action<int> thunk, IEnumerable<int> itemIds)
	{
		foreach (int itemId in itemIds)
		{
			thunk(itemId);
		}
	}

	private static void Patch()
	{
		Map.Patch(Map.SetTileFlag(TileFlag.Internal), Map.GetNoDrawItemIds());
		Map.Patch(Map.ClearTileFlag(TileFlag.Internal), 8198);
		Map.Patch(Map.SetTileFlag(TileFlag.LightSource), Enumerable.Range(14612, 24));
		Map.Patch(Map.ClearTileFlag(TileFlag.Generic), 3853);
	}

	public static TileMatrix GetMatrix(int world)
	{
		return world switch
		{
			0 => Map.Felucca, 
			1 => Map.Trammel, 
			2 => Map.Ilshenar, 
			3 => Map.Malas, 
			4 => Map.Tokuno, 
			5 => Map.TerMur, 
			_ => Map.Felucca, 
		};
	}

	public static string ReplaceAmount(string Name, int Amount)
	{
		if (Name.IndexOf('%') == -1)
		{
			return Name;
		}
		Match match = Regex.Match(Name, "(?<1>[^%]*)%(?<2>[^%/]*)(?<3>/[^%]*)?%");
		if (Amount == 1)
		{
			return match.Groups[1].Value + ((match.Groups[3].Value.Length > 0) ? match.Groups[3].Value.Substring(1) : match.Groups[3].Value);
		}
		if (match.Groups[2].Success)
		{
			return match.Groups[1].Value + match.Groups[2].Value;
		}
		return match.Groups[1].Value;
	}

	public static string GetTileProperName(int TileID)
	{
		string tileName = Map.GetTileName(TileID);
		tileName = Map.ReplaceAmount(tileName, 1);
		TileFlags tileFlags = Map.GetTileFlags(TileID);
		bool flag = tileFlags[TileFlag.ArticleA];
		bool flag2 = tileFlags[TileFlag.ArticleAn];
		if (flag && flag2)
		{
			return $"the {tileName}";
		}
		if (flag)
		{
			return $"a {tileName}";
		}
		if (flag2)
		{
			return $"an {tileName}";
		}
		return tileName;
	}

	public unsafe static string GetItemName(ItemId itemId)
	{
		return Map.GetItemDataPointer(itemId)->Name;
	}

	public unsafe static string GetLandName(LandId landId)
	{
		return Map.GetLandDataPointer(landId)->Name;
	}

	[Obsolete("don't use me", false)]
	public static string GetTileName(int tileId)
	{
		if (tileId < 16384)
		{
			return Map.GetLandName((LandId)tileId);
		}
		return Map.GetItemName((ItemId)(tileId - 16384));
	}

	public static bool IsValid(int X, int Y)
	{
		return X >= 0 && X < Map.m_Width << 3 && Y >= 0 && Y < Map.m_Height << 3;
	}

	public static void Lock()
	{
		Map.m_Locked = true;
	}

	public static void Unlock()
	{
		Map.m_Locked = false;
		while (Map.m_LockQueue.Count > 0)
		{
			Map.m_LockQueue.Dequeue().Invoke();
		}
	}

	public static void Sort(int X, int Y)
	{
		if (Map.m_Locked)
		{
			Map.m_LockQueue.Enqueue(new SortLock(X, Y));
			return;
		}
		List<ICell>[,] cells = Map.m_Cached.cells;
		if (cells != null)
		{
			List<ICell> list = cells[X, Y];
			if (list.Count > 1)
			{
				list.Sort(TileSorter.Comparer);
			}
		}
	}

	private static List<ICell> GetList(int x, int y)
	{
		int num = Map.m_X << 3;
		int num2 = Map.m_Y << 3;
		x -= num;
		y -= num2;
		if (Map.IsValid(x, y))
		{
			List<ICell>[,] cells = Map.m_Cached.cells;
			if (cells != null)
			{
				return cells[x, y];
			}
		}
		return null;
	}

	public static void Update(PhysicalAgent agent)
	{
		if (!Map.m_Locked)
		{
			IAgentCell agentCell = agent.AcquireViewportCell();
			agentCell.Update();
			List<ICell> owner = agentCell.Owner;
			List<ICell> list = (agent.InWorld ? Map.GetList(agent.X, agent.Y) : null);
			if (list != owner)
			{
				owner?.Remove(agentCell);
				if (list != null)
				{
					int num = list.BinarySearch(agentCell, TileSorter.Comparer);
					if (num < 0)
					{
						num = ~num;
					}
					list.Insert(num, agentCell);
				}
				agentCell.Owner = list;
			}
			else
			{
				list?.Sort(TileSorter.Comparer);
			}
		}
		else
		{
			Map.m_LockQueue.Enqueue(new UpdateAgentLock(agent));
		}
	}

	public static void QueueInvalidate()
	{
		Map.m_QueueInvalidate = true;
	}

	public static void Invalidate()
	{
		Map.m_IsCached = false;
		Engine.Redraw();
	}

	public static MapPackage GetCache()
	{
		return Map.m_Cached;
	}

	private static void EnsureCellPoolCapacity(int width, int height)
	{
		if (Map.m_CellPool == null || Map.m_CellPool.GetLength(0) < width || Map.m_CellPool.GetLength(1) < height)
		{
			Map.m_CellPool = new List<ICell>[width, height];
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					Map.m_CellPool[i, j] = new List<ICell>(4);
				}
			}
			return;
		}
		int length = Map.m_CellPool.GetLength(0);
		int length2 = Map.m_CellPool.GetLength(1);
		for (int k = 0; k < length; k++)
		{
			for (int l = 0; l < length2; l++)
			{
				List<ICell> list = Map.m_CellPool[k, l];
				for (int m = 0; m < list.Count; m++)
				{
					if (list[m] is IAgentCell agentCell)
					{
						agentCell.Owner = null;
					}
				}
				list.Clear();
			}
		}
	}

	private static void EnsureLandTilePoolCapacity(int width, int height)
	{
		if (Map.m_LandTiles != null && Map.m_LandTiles.GetLength(0) >= width && Map.m_LandTiles.GetLength(1) >= height)
		{
			return;
		}
		Map.m_LandTiles = new LandTile[width, height];
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				Map.m_LandTiles[i, j] = new LandTile();
				Map.m_LandTiles[i, j].x = i;
				Map.m_LandTiles[i, j].y = j;
			}
		}
	}

	public static MapPackage GetMap(int X, int Y, int W, int H)
	{
		if (Map.m_X == X && Map.m_Y == Y && Map.m_Width == W && Map.m_Height == H && Map.m_World == Engine.m_World && Map.m_IsCached && !Map.m_QueueInvalidate)
		{
			return Map.m_Cached;
		}
		Map.m_QueueInvalidate = false;
		if (Map.m_Cached.cells != null)
		{
			int length = Map.m_Cached.cells.GetLength(0);
			int length2 = Map.m_Cached.cells.GetLength(1);
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < length2; j++)
				{
					List<ICell> list = Map.m_Cached.cells[i, j];
					if (list != null)
					{
						int count = list.Count;
						for (int k = 0; k < count; k++)
						{
							list[k].Dispose();
						}
					}
				}
			}
		}
		Map.m_X = X;
		Map.m_Y = Y;
		Map.m_Width = W;
		Map.m_Height = H;
		Map.m_World = Engine.m_World;
		if (Map.m_StrongReferences == null || Map.m_StrongReferences.Length < W * H)
		{
			Map.m_StrongReferences = new MapBlock[W * H];
		}
		int num = W << 3;
		int num2 = H << 3;
		Map.EnsureCellPoolCapacity(num, num2);
		Map.EnsureLandTilePoolCapacity(num, num2);
		if (Map.m_IndexPool == null || Map.m_IndexPool.GetLength(0) < num || Map.m_IndexPool.GetLength(1) < num2)
		{
			Map.m_IndexPool = new byte[num, num2];
		}
		if (Map.m_FlagPool == null || Map.m_FlagPool.GetLength(0) < num || Map.m_FlagPool.GetLength(1) < num2)
		{
			Map.m_FlagPool = new byte[num, num2];
		}
		List<ICell>[,] cellPool = Map.m_CellPool;
		IComparer<ICell> comparer = TileSorter.Comparer;
		MapPackage map = new MapPackage
		{
			cells = cellPool,
			CellX = X << 3,
			CellY = Y << 3
		};
		Engine.Multis.Update(map);
		int world = Engine.m_World;
		TileMatrix matrix = Map.GetMatrix(world);
		Viewport viewport = World.Viewport;
		int num7 = 0;
		int num8 = X;
		while (num7 < W)
		{
			int num9 = 0;
			int num10 = Y;
			while (num9 < H)
			{
				MapBlock block = matrix.GetBlock(num8, num10);
				Map.m_StrongReferences[num9 * W + num7] = block;
				HuedTile[][][] array = ((block == null) ? matrix.EmptyStaticBlock : block.m_StaticTiles);
				Tile[] array2 = ((block == null) ? matrix.InvalidLandBlock : block.m_LandTiles);
				int num11 = 0;
				int num12 = num8 << 3;
				int num13 = num7 << 3;
				while (num11 < 8)
				{
					int num14 = 0;
					int num15 = num10 << 3;
					int num16 = num9 << 3;
					while (num14 < 8)
					{
						HuedTile[] array3 = array[num11][num14];
						for (int num17 = 0; num17 < array3.Length; num17++)
						{
							cellPool[num13, num16].Add(StaticItem.Instantiate(array3[num17], num17, (num12 * matrix.Height + num15) | (num17 << 25)));
						}
						LandTile landTile = viewport.GetLandTile(num12, num15, world);
						landTile.x = num13;
						landTile.y = num16;
						cellPool[num13, num16].Add(landTile);
						num14++;
						num15++;
						num16++;
					}
					num11++;
					num12++;
					num13++;
				}
				num9++;
				num10++;
			}
			num7++;
			num8++;
		}
		int num18 = X << 3;
		int num19 = Y << 3;
		foreach (Item value in World.Items.Values)
		{
			if (value.InWorld && value.Visible && !value.IsMulti)
			{
				int num20 = value.X - num18;
				int num21 = value.Y - num19;
				if (num20 >= 0 && num20 < num && num21 >= 0 && num21 < num2)
				{
					IAgentCell agentCell2 = value.AcquireViewportCell();
					cellPool[num20, num21].Add(agentCell2);
					agentCell2.Owner = cellPool[num20, num21];
				}
			}
		}
		foreach (Mobile value2 in World.Mobiles.Values)
		{
			if (value2.InWorld && value2.Visible)
			{
				int num20 = value2.X - num18;
				int num21 = value2.Y - num19;
				if (num20 >= 0 && num20 < num && num21 >= 0 && num21 < num2)
				{
					IAgentCell agentCell3 = value2.AcquireViewportCell();
					cellPool[num20, num21].Add(agentCell3);
					agentCell3.Owner = cellPool[num20, num21];
				}
			}
		}
		for (int num22 = 0; num22 < num; num22++)
		{
			for (int num23 = 0; num23 < num2; num23++)
			{
				List<ICell> list3 = cellPool[num22, num23];
				if (list3.Count > 1)
				{
					list3.Sort(comparer);
				}
			}
		}
		map = (Map.m_Cached = new MapPackage
		{
			cells = cellPool,
			CellX = X << 3,
			CellY = Y << 3
		});
		Map.m_IsCached = true;
		for (int num24 = -1; num24 <= H; num24++)
		{
			Engine.QueueMapLoad(X - 1, Y + num24, matrix);
		}
		for (int num25 = 0; num25 < W; num25++)
		{
			Engine.QueueMapLoad(X + num25, Y - 1, matrix);
			Engine.QueueMapLoad(X + num25, Y + H, matrix);
		}
		for (int num26 = -1; num26 <= H; num26++)
		{
			Engine.QueueMapLoad(X + W, Y + num26, matrix);
		}
		return map;
	}
}
