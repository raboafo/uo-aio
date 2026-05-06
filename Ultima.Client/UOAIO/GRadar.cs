using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using UOAIO.Profiles;
using UOAIO.Targeting;

namespace UOAIO;

public class GRadar : Gump, IResizable
{
	protected int m_Width;

	protected int m_Height;

	private static List<IRadarTrackable> _trackables;

	private static bool m_ToClose;

	private static bool m_Open;

	private static int m_xBlock;

	private static int m_yBlock;

	private static int m_xWidth;

	private static int m_yHeight;

	private static int m_World;

	private static short[] m_Colors;

	private const short BLACK = short.MinValue;

	private static Texture m_Image;

	private static Texture m_Swap;

	private static MapBlock[] m_StrongReferences;

	private static BitArray m_Guarded;

	public static Mobile m_FocusMob;

	private static VertexCache m_vCache;

	public int MinWidth => 68;

	public int MinHeight => 68;

	public int MaxWidth => 340;

	public int MaxHeight => 340;

	public override int Width
	{
		get
		{
			return this.m_Width;
		}
		set
		{
			this.m_Width = value;
		}
	}

	public override int Height
	{
		get
		{
			return this.m_Height;
		}
		set
		{
			this.m_Height = value;
		}
	}

	public static void RegisterTrackable(IRadarTrackable trackable)
	{
		if (!GRadar._trackables.Contains(trackable))
		{
			GRadar._trackables.Add(trackable);
		}
	}

	public static void Open()
	{
		if (!GRadar.m_Open)
		{
			Gumps.Desktop.Children.Add(new GRadar());
		}
		else
		{
			GRadar.m_FocusMob = null;
		}
	}

	protected internal override void OnDispose()
	{
		GRadar.m_Open = false;
	}

	public GRadar()
		: base(25, 25)
	{
		GRadar.m_Open = true;
		this.m_Width = 260;
		this.m_Height = 260;
		base.m_Children.Add(new GVResizer(this));
		base.m_Children.Add(new GHResizer(this));
		base.m_Children.Add(new GLResizer(this));
		base.m_Children.Add(new GTResizer(this));
		base.m_Children.Add(new GHVResizer(this));
		base.m_Children.Add(new GLTResizer(this));
		base.m_Children.Add(new GHTResizer(this));
		base.m_Children.Add(new GLVResizer(this));
		base.m_CanDrag = true;
		base.m_QuickDrag = false;
		GRadar.m_FocusMob = null;
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return !Engine.amMoving && !TargetManager.IsActive;
	}

	protected internal override void OnMouseDown(int X, int Y, MouseButtons mb)
	{
		if (mb == MouseButtons.Right)
		{
			GRadar.m_ToClose = true;
		}
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if (GRadar.m_ToClose)
		{
			Gumps.Destroy(this);
		}
	}

	protected internal override void OnMouseEnter(int X, int Y, MouseButtons mb)
	{
		if (GRadar.m_ToClose && mb != MouseButtons.Right)
		{
			GRadar.m_ToClose = false;
		}
	}

	public static void Swap()
	{
		Texture image = GRadar.m_Image;
		GRadar.m_Image = GRadar.m_Swap;
		GRadar.m_Swap = image;
	}

	public static short GetRadarColor(int tid)
	{
		if (GRadar.m_Colors == null)
		{
			GRadar.LoadColors();
		}
		return GRadar.m_Colors[tid & 0x3FFF];
	}

	public static short GetRealColor(int tid)
	{
		if (GRadar.m_Colors == null)
		{
			GRadar.LoadColors();
		}
		return GRadar.m_Colors[tid];
	}

	private unsafe static void Load(int x, int y, int w, int h, int world, Texture tex)
	{
		if (GRadar.m_Colors == null)
		{
			GRadar.LoadColors();
		}
		if (GRadar.m_StrongReferences == null || GRadar.m_StrongReferences.Length != w * h)
		{
			GRadar.m_StrongReferences = new MapBlock[w * h];
		}
		if (GRadar.m_Guarded == null || GRadar.m_Guarded.Length != w * h * 64)
		{
			GRadar.m_Guarded = new BitArray(w * h * 64);
		}
		else
		{
			GRadar.m_Guarded.SetAll(value: false);
		}
		Region[] guardedRegions = Region.GuardedRegions;
		int num = x * 8;
		int num2 = y * 8;
		int num3 = w * 8;
		int num4 = h * 8;
		foreach (Region region in guardedRegions)
		{
			RegionWorld world2 = region.World;
			bool flag = false;
			switch (world2)
			{
			case RegionWorld.Britannia:
				flag = world == 0 || world == 1;
				break;
			case RegionWorld.Felucca:
				flag = world == 0;
				break;
			case RegionWorld.Trammel:
				flag = world == 1;
				break;
			case RegionWorld.Ilshenar:
				flag = world == 2;
				break;
			case RegionWorld.Malas:
				flag = world == 3;
				break;
			case RegionWorld.Tokuno:
				flag = world == 4;
				break;
			case RegionWorld.TerMur:
				flag = world == 5;
				break;
			}
			if (!flag)
			{
				continue;
			}
			int num5 = region.X - num;
			int num6 = region.Y - num2;
			if (num5 >= num3 || num6 >= num4 || num5 <= -region.Width || num6 <= -region.Height)
			{
				continue;
			}
			int num7 = num5 + region.Width;
			int num8 = num6 + region.Height;
			if (num5 < 0)
			{
				num5 = 0;
			}
			if (num6 < 0)
			{
				num6 = 0;
			}
			for (int j = num5; j < num7 && j < num3; j++)
			{
				for (int k = num6; k < num8 && k < num4; k++)
				{
					GRadar.m_Guarded[k * num3 + j] = true;
				}
			}
		}
		TileMatrix matrix = Map.GetMatrix(world);
		LockData lockData = tex.Lock(LockFlags.WriteOnly);
		int num9 = lockData.Pitch >> 1;
		fixed (short* colors = GRadar.m_Colors)
		{
			for (int l = 0; l < w; l++)
			{
				short* ptr = (short*)lockData.pvSrc + (l << 3);
				for (int m = 0; m < h; m++)
				{
					MapBlock block = matrix.GetBlock(x + l, y + m);
					GRadar.m_StrongReferences[m * w + l] = block;
					HuedTile[][][] array = ((block == null) ? matrix.EmptyStaticBlock : block.m_StaticTiles);
					Tile[] array2 = ((block == null) ? matrix.InvalidLandBlock : block.m_LandTiles);
					int num10 = 0;
					int num11 = 0;
					while (num10 < 8)
					{
						for (int n = 0; n < 8; n++)
						{
							int num12 = -255;
							int num13 = -255;
							int num14 = 0;
							int num15 = 0;
							int num18;
							for (int num16 = 0; num16 < array[n][num10].Length; num16++)
							{
								HuedTile huedTile = array[n][num10][num16];
								int num17 = (int)(16384 + huedTile.itemId);
								if (num17 != 16385 && num17 != 22422 && num17 != 24996 && num17 != 24984 && num17 != 25020 && num17 != 24985)
								{
									int z = huedTile.z;
									num18 = z + Map.GetItemHeight(huedTile.itemId);
									if (num18 > num12 || (z > num13 && num18 >= num12))
									{
										num12 = num18;
										num13 = z;
										num14 = num17;
										num15 = huedTile.hueId;
									}
								}
							}
							num18 = array2[num11 + n].z;
							if (num18 > num12 && array2[num11 + n].Visible)
							{
								num14 = (int)array2[num11 + n].landId;
								num15 = 0;
							}
							int num19 = ((m << 3) + num10) * num3 + (l << 3) + n;
							if (num15 == 0)
							{
								ptr[n] = colors[num14];
							}
							else
							{
								ptr[n] = (short)Hues.Load((num15 & 0x3FFF) | 0x8000).Pixel((ushort)colors[num14]);
							}
						}
						ptr += num9;
						num10++;
						num11 += 8;
					}
				}
			}
			List<Item> items = Engine.Multis.Items;
			for (int num20 = 0; num20 < items.Count; num20++)
			{
				Item item = items[num20];
				if (!item.InWorld)
				{
					continue;
				}
				CustomMultiEntry customMulti = CustomMultiLoader.GetCustomMulti(item.Serial, item.Revision);
				Multi multi = null;
				if (customMulti != null)
				{
					multi = customMulti.Multi;
				}
				if (multi == null)
				{
					multi = item.Multi;
				}
				if (multi == null)
				{
					continue;
				}
				ushort[][] radar = multi.Radar;
				if (radar == null)
				{
					continue;
				}
				multi.GetBounds(out var xMin, out var yMin, out var _, out var _);
				int num21 = 0;
				int num22 = item.Y - (y << 3) + yMin;
				while (num21 < radar.Length)
				{
					if (num22 >= 0 && num22 < h << 3)
					{
						short* ptr2 = (short*)lockData.pvSrc + num22 * num9;
						ushort[] array3 = radar[num21];
						int num23 = 0;
						int num24 = item.X - (x << 3) + xMin;
						while (num23 < array3.Length)
						{
							if (num24 >= 0 && num24 < w << 3 && array3[num23] != 0)
							{
								ptr2[num24] = colors[16384 + array3[num23]];
							}
							num23++;
							num24++;
						}
					}
					num21++;
					num22++;
				}
			}
		}
		tex.Unlock();
	}

	protected internal override void Draw(int X, int Y)
	{
		Mobile mobile = ((GRadar.m_FocusMob == null) ? World.Player : GRadar.m_FocusMob);
		if (mobile != null)
		{
			GRadar.DrawImage(X + 2, Y + 2, this.m_Width - 4, this.m_Height - 4, (mobile.Visible || mobile.Player) ? mobile.X : mobile.m_KUOC_X, (mobile.Visible || mobile.Player) ? mobile.Y : mobile.m_KUOC_Y, (mobile.Visible || mobile.Player) ? Engine.m_World : mobile.m_KUOC_F);
		}
		Renderer.SetTexture(null);
		Renderer.PushAlpha(0.25f);
		Renderer.TransparentRect(0, X + 4, Y + 4, this.m_Width - 8, this.m_Height - 8);
		Renderer.DrawLine(X, Y + 2, X, Y + this.m_Height - 2);
		Renderer.DrawLine(X + 2, Y, X + this.m_Width - 2, Y);
		Renderer.DrawLine(X + this.m_Width - 1, Y + 2, X + this.m_Width - 1, Y + this.m_Height - 2);
		Renderer.DrawLine(X + 2, Y + this.m_Height - 1, X + this.m_Width - 2, Y + this.m_Height - 1);
		Renderer.DrawPoints(new Point(X + 1, Y + 1), new Point(X + 1, Y + this.m_Height - 2), new Point(X + this.m_Width - 2, Y + 1), new Point(X + this.m_Width - 2, Y + this.m_Height - 2));
		Renderer.SetAlpha(0.5f);
		Renderer.DrawLine(X + 1, Y + 2, X + 1, Y + this.m_Height - 2);
		Renderer.DrawLine(X + 2, Y + 1, X + this.m_Width - 2, Y + 1);
		Renderer.DrawLine(X + this.m_Width - 2, Y + 2, X + this.m_Width - 2, Y + this.m_Height - 2);
		Renderer.DrawLine(X + 2, Y + this.m_Height - 2, X + this.m_Width - 2, Y + this.m_Height - 2);
		Renderer.TransparentRect(0, X + 3, Y + 3, this.m_Width - 6, this.m_Height - 6);
		Renderer.PopAlpha();
		Renderer.TransparentRect(0, X + 2, Y + 2, this.m_Width - 4, this.m_Height - 4);
	}

	public static void Dispose()
	{
		if (GRadar.m_Image != null)
		{
			GRadar.m_Image.Dispose();
			GRadar.m_Image = null;
		}
		if (GRadar.m_Swap != null)
		{
			GRadar.m_Swap.Dispose();
			GRadar.m_Swap = null;
		}
		GRadar.m_Colors = null;
	}

	private static Point GetPoint(int xTile, int yTile, int xCenter, int yCenter, int xDotCenter, int yDotCenter, double xScale, double yScale)
	{
		int num = xTile - xCenter;
		int num2 = yTile - yCenter;
		int num3 = xDotCenter;
		int num4 = yDotCenter;
		num3 += (int)((double)(num - num2) * xScale);
		num4 += (int)((double)(num + num2) * yScale);
		return new Point(num3, num4);
	}

	protected internal override void OnDoubleClick(int X, int Y)
	{
		Mobile mobile = ((GRadar.m_FocusMob == null) ? World.Player : GRadar.m_FocusMob);
		if (mobile == null)
		{
			return;
		}
		int num = ((mobile.Visible || mobile.Player) ? mobile.X : mobile.m_KUOC_X);
		int num2 = ((mobile.Visible || mobile.Player) ? mobile.Y : mobile.m_KUOC_Y);
		int num3 = this.m_Width - 4;
		int num4 = this.m_Height - 4;
		int num5 = (num3 >> 1) - 1;
		int num6 = (GRadar.m_Image.Height >> 1) - 16;
		num6 = (int)((double)num6 / (double)GRadar.m_Image.Height * (double)num4);
		double num7 = (double)num3 / 256.0;
		double num8 = (double)num4 / 256.0;
		if (Engine.GMPrivs)
		{
			int num9 = (int)((double)(X - num5) / num7 / 2.0);
			int num10 = (int)((double)(Y - num6) / num8 / 2.0);
			int num11 = num + (num10 + num9);
			int num12 = num2 + (num10 - num9);
			Engine.commandEntered($"[go {num11} {num12}");
			return;
		}
		RuneInfoEx runeInfoEx = null;
		int num13 = 0;
		foreach (RuneInfoEx rune in Player.Runes)
		{
			GRadar.GetDotPoint(cap: false, rune.Point.X, rune.Point.Y, num3, num4, num, num2, num7, num8, num5, num6, out var xDot, out var yDot);
			xDot -= X;
			yDot -= Y;
			xDot += 2;
			yDot += 2;
			int num14 = Math.Max(Math.Abs(xDot), Math.Abs(yDot));
			if (runeInfoEx == null || num14 < num13)
			{
				runeInfoEx = rune;
				num13 = num14;
			}
		}
		if (runeInfoEx != null)
		{
			bool isGate = Control.ModifierKeys == (Keys.Shift | Keys.Control);
			ClientFormatEx.Recall(runeInfoEx, isGate);
		}
	}

	private static void DrawDot(bool onScreen, int color, int xLoc, int yLoc, int x, int y, int width, int height, int xCenter, int yCenter, double xScale, double yScale, int xDotCenter, int yDotCenter, out int xDot, out int yDot)
	{
		bool dotPoint = GRadar.GetDotPoint(cap: true, xLoc, yLoc, width, height, xCenter, yCenter, xScale, yScale, xDotCenter, yDotCenter, out xDot, out yDot);
		if (!(!dotPoint && onScreen))
		{
			int num = xDot + x;
			int num2 = yDot + y;
			Renderer.SetTexture(null);
			Renderer.SolidRect(color, num, num2, 1, 1);
			Renderer.PushAlpha(0.5f);
			Renderer.SolidRect(color, num - 1, num2, 1, 1);
			Renderer.SolidRect(color, num + 1, num2, 1, 1);
			Renderer.SolidRect(color, num, num2 - 1, 1, 1);
			Renderer.SolidRect(color, num, num2 + 1, 1, 1);
			Renderer.SetAlpha(0.25f);
			Renderer.SolidRect(color, num - 2, num2, 1, 1);
			Renderer.SolidRect(color, num + 2, num2, 1, 1);
			Renderer.SolidRect(color, num, num2 - 2, 1, 1);
			Renderer.SolidRect(color, num, num2 + 2, 1, 1);
			Renderer.SetAlpha(0.15f);
			Renderer.SolidRect(color, num - 1, num2 - 1, 1, 1);
			Renderer.SolidRect(color, num + 1, num2 - 1, 1, 1);
			Renderer.SolidRect(color, num - 1, num2 + 1, 1, 1);
			Renderer.SolidRect(color, num + 1, num2 + 1, 1, 1);
			Renderer.PopAlpha();
			if (onScreen)
			{
				Texture travelIcon = Engine.ImageCache.TravelIcon;
				travelIcon.DrawClipped(num - travelIcon.Width / 2 - 1, num2 - travelIcon.Height / 2 - 1, Clipper.TemporaryInstance(x, y, width, height));
			}
		}
	}

	private static bool GetDotPoint(bool cap, int xLoc, int yLoc, int width, int height, int xCenter, int yCenter, double xScale, double yScale, int xDotCenter, int yDotCenter, out int xDot, out int yDot)
	{
		bool result = true;
		int num = xLoc - xCenter;
		int num2 = yLoc - yCenter;
		xDot = xDotCenter;
		yDot = yDotCenter;
		xDot += (int)((double)(num - num2) * xScale);
		yDot += (int)((double)(num + num2) * yScale);
		if (cap)
		{
			if (xDot <= 1)
			{
				xDot = 2;
				result = false;
			}
			else if (xDot >= width - 2)
			{
				xDot = width - 3;
				result = false;
			}
			if (yDot <= 1)
			{
				yDot = 2;
				result = false;
			}
			else if (yDot >= height - 2)
			{
				yDot = height - 3;
				result = false;
			}
		}
		return result;
	}

	private static void DrawTags(int x, int y, int f, int width, int height, int xCenter, int yCenter)
	{
		int num = (width >> 1) - 1;
		int num2 = (GRadar.m_Image.Height >> 1) - 16;
		num2 = (int)((double)num2 / (double)GRadar.m_Image.Height * (double)height);
		double xScale = (double)width / 256.0;
		double yScale = (double)height / 256.0;
		if (World.Player != null)
		{
			foreach (RuneInfoEx rune in Player.Runes)
			{
				GRadar.DrawDot(onScreen: true, 16777215, rune.Point.X, rune.Point.Y, x, y, width, height, xCenter, yCenter, xScale, yScale, num, num2, out var _, out var _);
			}
		}
		if (GRadar.m_FocusMob != World.Player && GRadar.m_FocusMob != null)
		{
			Mobile player = World.Player;
			if (Engine.m_World == f)
			{
				GRadar.DrawDot(onScreen: false, 16777215, player.X, player.Y, x, y, width, height, xCenter, yCenter, xScale, yScale, num, num2, out var xDot2, out var yDot2);
				Texture texture = Engine.GetUniFont(2).GetString("You", Hues.Bright);
				if (xDot2 < num && yDot2 < num2)
				{
					GRadar.m_vCache.Draw(texture, xDot2 + x - texture.xMin + 2, yDot2 + y - texture.yMin + 2);
				}
				else if (xDot2 >= num && yDot2 < num2)
				{
					GRadar.m_vCache.Draw(texture, xDot2 + x - texture.xMax - 2, yDot2 + y - texture.yMin + 2);
				}
				else if (xDot2 < num && yDot2 >= num2)
				{
					GRadar.m_vCache.Draw(texture, xDot2 + x - texture.xMin + 2, yDot2 + y - texture.yMax - 2);
				}
				else if (xDot2 >= num && yDot2 >= num2)
				{
					GRadar.m_vCache.Draw(texture, xDot2 + x - texture.xMax - 2, yDot2 + y - texture.yMax - 2);
				}
			}
		}
		foreach (IRadarTrackable trackable in GRadar._trackables)
		{
			if (trackable.Facet != f || trackable.HasExpired)
			{
				continue;
			}
			GRadar.DrawDot(onScreen: false, trackable.Color, trackable.X, trackable.Y, x, y, width, height, xCenter, yCenter, xScale, yScale, num, num2, out var xDot3, out var yDot3);
			string name = trackable.Name;
			if (!string.IsNullOrEmpty(name))
			{
				Texture texture2 = Engine.GetUniFont(2).GetString(name, Hues.Bright);
				if (xDot3 < num && yDot3 < num2)
				{
					GRadar.m_vCache.Draw(texture2, xDot3 + x - texture2.xMin + 2, yDot3 + y - texture2.yMin + 2);
				}
				else if (xDot3 >= num && yDot3 < num2)
				{
					GRadar.m_vCache.Draw(texture2, xDot3 + x - texture2.xMax - 2, yDot3 + y - texture2.yMin + 2);
				}
				else if (xDot3 < num && yDot3 >= num2)
				{
					GRadar.m_vCache.Draw(texture2, xDot3 + x - texture2.xMin + 2, yDot3 + y - texture2.yMax - 2);
				}
				else if (xDot3 >= num && yDot3 >= num2)
				{
					GRadar.m_vCache.Draw(texture2, xDot3 + x - texture2.xMax - 2, yDot3 + y - texture2.yMax - 2);
				}
			}
		}
	}

	public static void Invalidate()
	{
		GRadar.m_xBlock = -1;
	}

	protected static void DrawImage(int X, int Y, int Width, int Height, int xCenter, int yCenter, int world)
	{
		if (GRadar.m_Image == null)
		{
			GRadar.m_Image = new Texture(Width, Height, TextureTransparency.None);
		}
		int num = xCenter >> 3;
		int num2 = yCenter >> 3;
		int num3 = xCenter & 7;
		int num4 = yCenter & 7;
		int num5 = num3;
		int num6 = num4;
		int num7 = 0;
		int num8 = 0;
		double num9 = 0.0;
		if (GRadar.m_xBlock == num && GRadar.m_yBlock == num2 && GRadar.m_World == world && GRadar.m_Image != null)
		{
			Renderer.FilterEnable = true;
			GRadar.m_Image.Draw(X, Y, Width, Height, 0f + (float)((double)num5 / (double)GRadar.m_Image.Width), 0.5f + (float)((double)num6 / (double)GRadar.m_Image.Height), 0.5f + (float)((double)num5 / (double)GRadar.m_Image.Width), 0f + (float)((double)num6 / (double)GRadar.m_Image.Height), 1f + (float)((double)num5 / (double)GRadar.m_Image.Width), 0.5f + (float)((double)num6 / (double)GRadar.m_Image.Height), 0.5f + (float)((double)num5 / (double)GRadar.m_Image.Width), 1f + (float)((double)num6 / (double)GRadar.m_Image.Height));
			Renderer.FilterEnable = false;
			num7 = X + (Width >> 1) - 1;
			num8 = (GRadar.m_Image.Height >> 1) - 16;
			num9 = (double)num8 / (double)GRadar.m_Image.Height;
			num8 = (int)(num9 * (double)Height);
			num8 += Y;
			GRadar.DrawTags(X, Y, world, Width, Height, xCenter, yCenter);
			Renderer.SetTexture(null);
			Renderer.SolidRect(16777215, num7, num8, 1, 1);
			Renderer.PushAlpha(0.5f);
			Renderer.SolidRect(16777215, num7 - 1, num8, 1, 1);
			Renderer.SolidRect(16777215, num7 + 1, num8, 1, 1);
			Renderer.SolidRect(16777215, num7, num8 - 1, 1, 1);
			Renderer.SolidRect(16777215, num7, num8 + 1, 1, 1);
			Renderer.SetAlpha(0.25f);
			Renderer.SolidRect(16777215, num7 - 2, num8, 1, 1);
			Renderer.SolidRect(16777215, num7 + 2, num8, 1, 1);
			Renderer.SolidRect(16777215, num7, num8 - 2, 1, 1);
			Renderer.SolidRect(16777215, num7, num8 + 2, 1, 1);
			Renderer.SetAlpha(0.15f);
			Renderer.SolidRect(16777215, num7 - 1, num8 - 1, 1, 1);
			Renderer.SolidRect(16777215, num7 + 1, num8 - 1, 1, 1);
			Renderer.SolidRect(16777215, num7 - 1, num8 + 1, 1, 1);
			Renderer.SolidRect(16777215, num7 + 1, num8 + 1, 1, 1);
			Renderer.PopAlpha();
			return;
		}
		int x = num - 15;
		int y = num2 - 15;
		int num10 = 32;
		int num11 = 32;
		GRadar.m_xWidth = num10;
		GRadar.m_yHeight = num11;
		GRadar.m_xBlock = num;
		GRadar.m_yBlock = num2;
		GRadar.m_World = world;
		GRadar.Load(x, y, num10, num11, world, GRadar.m_Image);
		if (GRadar.m_Image != null && !GRadar.m_Image.IsEmpty())
		{
			Renderer.FilterEnable = true;
			GRadar.m_Image.Draw(X, Y, Width, Height, 0f + (float)((double)num5 / (double)GRadar.m_Image.Width), 0.5f + (float)((double)num6 / (double)GRadar.m_Image.Height), 0.5f + (float)((double)num5 / (double)GRadar.m_Image.Width), 0f + (float)((double)num6 / (double)GRadar.m_Image.Height), 1f + (float)((double)num5 / (double)GRadar.m_Image.Width), 0.5f + (float)((double)num6 / (double)GRadar.m_Image.Height), 0.5f + (float)((double)num5 / (double)GRadar.m_Image.Width), 1f + (float)((double)num6 / (double)GRadar.m_Image.Height));
			Renderer.FilterEnable = false;
		}
		num7 = X + (Width >> 1) - 1;
		num8 = (GRadar.m_Image.Height >> 1) - 16;
		num9 = (double)num8 / (double)GRadar.m_Image.Height;
		num8 = (int)(num9 * (double)Height);
		num8 += Y;
		GRadar.DrawTags(X, Y, world, Width, Height, xCenter, yCenter);
		Renderer.SetTexture(null);
		Renderer.SolidRect(16777215, num7, num8, 1, 1);
		Renderer.PushAlpha(0.5f);
		Renderer.SolidRect(16777215, num7 - 1, num8, 1, 1);
		Renderer.SolidRect(16777215, num7 + 1, num8, 1, 1);
		Renderer.SolidRect(16777215, num7, num8 - 1, 1, 1);
		Renderer.SolidRect(16777215, num7, num8 + 1, 1, 1);
		Renderer.SetAlpha(0.25f);
		Renderer.SolidRect(16777215, num7 - 2, num8, 1, 1);
		Renderer.SolidRect(16777215, num7 + 2, num8, 1, 1);
		Renderer.SolidRect(16777215, num7, num8 - 2, 1, 1);
		Renderer.SolidRect(16777215, num7, num8 + 2, 1, 1);
		Renderer.SetAlpha(0.15f);
		Renderer.SolidRect(16777215, num7 - 1, num8 - 1, 1, 1);
		Renderer.SolidRect(16777215, num7 + 1, num8 - 1, 1, 1);
		Renderer.SolidRect(16777215, num7 - 1, num8 + 1, 1, 1);
		Renderer.SolidRect(16777215, num7 + 1, num8 + 1, 1, 1);
		Renderer.PopAlpha();
	}

	private unsafe static void LoadColors()
	{
		Debug.TimeBlock("Initializing Radar");
		GRadar.m_Colors = new short[81920];
		byte[] array = new byte[163840];
		Stream stream = Engine.FileManager.OpenMUL("RadarCol.mul");
		UnsafeMethods.ReadFile((FileStream)stream, array, 0, array.Length);
		stream.Close();
		fixed (byte* ptr = array)
		{
			fixed (short* colors = GRadar.m_Colors)
			{
				ushort* ptr2 = (ushort*)ptr;
				ushort* ptr3 = (ushort*)colors;
				int num = 0;
				while (num++ < 81920)
				{
					*(ptr3++) = (ushort)(*(ptr2++) | 0x8000);
				}
				foreach (GraphicTranslation value in GraphicTranslators.Art.Table.Values)
				{
					if (value.UpdatedId < 16384 && value.FallbackId < 16384)
					{
						colors[value.UpdatedId] = colors[value.FallbackId];
					}
				}
			}
		}
		Debug.EndBlock();
	}

	static GRadar()
	{
		GRadar._trackables = new List<IRadarTrackable>();
		GRadar.m_vCache = new VertexCache();
	}
}
