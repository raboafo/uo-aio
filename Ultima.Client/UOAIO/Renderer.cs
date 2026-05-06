using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D9;
using Ultima.Data;
using UOAIO.Profiles;
using UOAIO.Targeting;

namespace UOAIO;

public class Renderer
{
	private sealed class ScreenshotContext
	{
		private readonly string title;

		public ScreenshotContext(string title)
		{
			if (title == null)
			{
				throw new ArgumentNullException("title");
			}
			this.title = title;
		}

		public void Save()
		{
			Bitmap bitmap = ScreenshotContext.Capture();
			try
			{
				ThreadPool.QueueUserWorkItem(SaveCallback, bitmap);
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		private void SaveCallback(object state)
		{
			string tempFileName;
			using (Bitmap bitmap = (Bitmap)state)
			{
				tempFileName = Path.GetTempFileName();
				bitmap.Save(tempFileName, ImageFormat.Png);
			}
			DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Ultima Online"), ScreenshotContext.Normalize(World.Player.Name)));
			if (!directoryInfo.Exists)
			{
				directoryInfo.Create();
			}
			string normalizedFileName = ScreenshotContext.Normalize(this.title);
			for (int i = 1; i < 1000 && !ScreenshotContext.TrySave(directoryInfo, tempFileName, normalizedFileName, i); i++)
			{
			}
		}

		private static Bitmap Capture()
		{
			System.Drawing.Rectangle rectangle = Engine.m_Display.RectangleToScreen(new System.Drawing.Rectangle(Engine.GameX, Engine.GameY, Engine.GameWidth, Engine.GameHeight));
			Bitmap bitmap = new Bitmap(rectangle.Width, rectangle.Height);
			try
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					graphics.CopyFromScreen(rectangle.Location, System.Drawing.Point.Empty, rectangle.Size, CopyPixelOperation.SourceCopy);
				}
				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		private static string Normalize(string fileName)
		{
			return Regex.Replace(fileName, $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]", string.Empty);
		}

		private static bool TrySave(DirectoryInfo targetDirectory, string temporaryPath, string normalizedFileName, int attempt)
		{
			string targetPath = Path.Combine(targetDirectory.FullName, $"{normalizedFileName}-{attempt}.png");
			return ScreenshotContext.TryCopy(temporaryPath, targetPath);
		}

		private static bool TryCopy(string sourcePath, string targetPath)
		{
			if (!File.Exists(targetPath))
			{
				try
				{
					File.Copy(sourcePath, targetPath, overwrite: false);
					return true;
				}
				catch (PathTooLongException)
				{
					throw;
				}
				catch (DirectoryNotFoundException)
				{
					throw;
				}
				catch (FileNotFoundException)
				{
					throw;
				}
				catch (IOException)
				{
				}
			}
			return false;
		}
	}

	private class DrawQueueEntry
	{
		public Texture m_Texture;

		public int m_TileX;

		public int m_TileY;

		public int m_DrawX;

		public int m_DrawY;

		public bool m_Flip;

		public float m_fAlpha;

		public bool m_bAlpha;

		public DrawQueueEntry(Texture tex, int tx, int ty, int dx, int dy)
		{
			this.m_Texture = tex;
			this.m_TileX = tx;
			this.m_TileY = ty;
			this.m_DrawX = dx;
			this.m_DrawY = dy;
			this.m_Flip = this.m_Texture.Flip;
			this.m_fAlpha = Renderer._alphaValue;
			this.m_bAlpha = Renderer._alphaEnable;
		}
	}

	private class ObjectFormat
	{
		public PrimitiveType Type;

		public int PrimitiveCount;

		public int VertexCount;

		public IndexBuffer IndexBuffer;

		public ObjectFormat(PrimitiveType type, int primitiveCount, int vertexCount, IndexBuffer indexBuffer)
		{
			this.Type = type;
			this.PrimitiveCount = primitiveCount;
			this.VertexCount = vertexCount;
			this.IndexBuffer = indexBuffer;
		}
	}

	private class AlphaState
	{
		public int _type;

		public DrawBlendType m_BlendType;

		public Texture m_Texture;

		public TextureVB m_TextureVB;

		public bool m_Filter;

		public AlphaState()
		{
			this.m_TextureVB = new TextureVB();
		}
	}

	public static int m_Version;

	public const int FALSE = 0;

	public const int TRUE = 1;

	public static int m_Frames;

	public static int m_ActFrames;

	public static RenderProfile _profile;

	public static int blockWidth;

	public static int blockHeight;

	public static int cellWidth;

	public static int cellHeight;

	public const int CF_STRETCH = 1;

	private static ICell m_LastFind;

	private static int xwLast;

	private static int ywLast;

	private static int zwLast;

	private static int xLast;

	private static int yLast;

	private const double r21 = 1.0 / 21.0;

	private static System.Drawing.Point[] m_PointPool;

	private static System.Drawing.Point m_MousePoint;

	public static System.Drawing.Rectangle m_FoliageCheck;

	private static TransformedColoredTextured[] m_GeoPool;

	private const int A_FULL = -16777216;

	private static bool _alphaTestEnable;

	private static Texture m_Texture;

	private static bool _filterEnable;

	public static bool _alphaEnable;

	private static bool m_DrawPing;

	private static bool m_DrawFPS;

	private static bool m_DrawPCount;

	private static bool m_DrawGrid;

	private static bool m_Invalidate;

	public static float _alphaValue;

	public static int _alphaBits;

	private const int A_255 = -16777216;

	private static Stack<float> _alphaStack;

	public static Texture m_TextSurface;

	public static VertexCache m_vTextCache;

	private static int m_CharX;

	private static int m_CharY;

	private static int m_CharZ;

	public static bool m_DeathOverride;

	public static List<TextMessage> m_TextToDraw;

	private static bool m_WasDead;

	private static bool m_Transparency;

	private static bool m_CullLand;

	public static int m_xScroll;

	public static int m_yScroll;

	public static int m_xWorld;

	public static int m_yWorld;

	public static int m_zWorld;

	public static int m_xBaseLast;

	public static int m_yBaseLast;

	private static Type tLandTile;

	private static Type tDynamicItem;

	private static Type tStaticItem;

	private static Type tMobileCell;

	private static int m_AlwaysHighlight;

	public static bool m_Dead;

	public static int eOffsetX;

	public static int eOffsetY;

	private static ScreenshotContext screenshotContext;

	private static Queue<TransparentDraw> m_TransDrawQueue;

	private static Queue<MiniHealthEntry> m_MiniHealthQueue;

	private static Queue<Mobile> m_ToUpdateQueue;

	private static List<TextMessage> m_TextToDrawList;

	private static List<System.Drawing.Rectangle> m_RectsList;

	private static TransformedColoredTextured[] m_vMultiPool;

	private static TransformedColoredTextured[] m_vTransDrawPool;

	private static int m_xServerStart;

	private static int m_yServerStart;

	private static int m_xServerEnd;

	private static int m_yServerEnd;

	private static DesignerID[] m_DesignerIDs;

	private static MapSubgroup[] _mapSubgroups;

	public static bool _timeRefresh;

	private static Texture lightTexture;

	private static Surface lightSurface;

	private static int _drawGroup;

	public static int m_Count;

	private static List<Texture>[] m_Lists;

	private static DrawBlendType _blendType;

	public static int _renderCount;

	private static DrawBlendType m_CurBlendType;

	private static bool m_CurAlphaTest;

	private static BaseTexture _tex0;

	private static BaseTexture _tex1;

	private static PixelShader _psh;

	public const int VertexBufferLength = 32768;

	private static BufferedVertexStream m_VertexStream;

	private static List<AlphaState> m_AlphaStates;

	private static int m_AlphaStateCount;

	private static Surface _backBuffer;

	private static ObjectFormat[] _formats;

	internal static IndexBuffer _currentIndexBuffer;

	private static bool m_CurFilter;

	public static bool FilterEnable
	{
		get
		{
			return Renderer._filterEnable;
		}
		set
		{
			Renderer._filterEnable = value;
		}
	}

	public static bool DrawGrid
	{
		get
		{
			return Engine.GMPrivs && Renderer.m_DrawGrid;
		}
		set
		{
			Renderer.m_DrawGrid = value;
		}
	}

	public static bool DrawPCount
	{
		get
		{
			return Renderer.m_DrawPCount;
		}
		set
		{
			Renderer.m_DrawPCount = value;
		}
	}

	public static bool DrawPing
	{
		get
		{
			return Renderer.m_DrawPing;
		}
		set
		{
			Renderer.m_DrawPing = value;
		}
	}

	public static bool DrawFPS
	{
		get
		{
			return Renderer.m_DrawFPS;
		}
		set
		{
			Renderer.m_DrawFPS = value;
		}
	}

	public static bool Transparency
	{
		get
		{
			return Renderer.m_Transparency;
		}
		set
		{
			Renderer.m_Transparency = value;
		}
	}

	public static int AlwaysHighlight
	{
		get
		{
			return Renderer.m_AlwaysHighlight;
		}
		set
		{
			Renderer.m_AlwaysHighlight = value;
		}
	}

	public static bool ScreenshotMode => Renderer.screenshotContext != null;

	public static System.Drawing.Rectangle ServerBoundary
	{
		set
		{
			Renderer.m_xServerStart = value.X;
			Renderer.m_yServerStart = value.Y;
			Renderer.m_xServerEnd = value.Right;
			Renderer.m_yServerEnd = value.Bottom;
		}
	}

	public static ICell FindTileFromXY(int mx, int my, ref int TileX, ref int TileY)
	{
		return Renderer.FindTileFromXY(mx, my, ref TileX, ref TileY, onlyMobs: false);
	}

	public static void ResetHitTest()
	{
		Renderer.m_LastFind = null;
		Renderer.xwLast = 0;
		Renderer.ywLast = 0;
		Renderer.zwLast = 0;
		Renderer.xLast = 0;
		Renderer.yLast = 0;
	}

	public static bool LandTileHitTest(System.Drawing.Point[] points, System.Drawing.Point check)
	{
		int y = points[0].Y;
		int y2 = points[2].Y;
		if (check.Y >= points[0].Y && check.Y <= points[2].Y)
		{
			int num = check.X - points[3].X;
			int num3;
			int num4;
			if (num >= 0 && num < 22)
			{
				double num2 = 1.0 / 21.0 * (double)num;
				num3 = points[3].Y + (int)((double)(points[0].Y - points[3].Y) * num2);
				num4 = points[3].Y + (int)((double)(points[2].Y - points[3].Y) * num2);
			}
			else
			{
				if (num < 22 || num >= 44)
				{
					return false;
				}
				double num5 = 1.0 / 21.0 * (double)(num - 22);
				num3 = points[0].Y + (int)((double)(points[1].Y - points[0].Y) * num5);
				num4 = points[2].Y + (int)((double)(points[1].Y - points[2].Y) * num5);
			}
			return check.Y >= num3 && check.Y <= num4;
		}
		return false;
	}

	private static void Fix(ref int v, int cap)
	{
		if (v < 0)
		{
			v = 0;
		}
		else if (v > cap)
		{
			v = cap;
		}
	}

	public static bool SetViewport(int x, int y, int w, int h)
	{
		Renderer.PushAll();
		Viewport viewport = new Viewport
		{
			MinDepth = 0f,
			MaxDepth = 1f
		};
		int v = x;
		int v2 = y;
		int v3 = x + w;
		int v4 = y + h;
		Renderer.Fix(ref v, Engine.ScreenWidth);
		Renderer.Fix(ref v2, Engine.ScreenHeight);
		Renderer.Fix(ref v3, Engine.ScreenWidth);
		Renderer.Fix(ref v4, Engine.ScreenHeight);
		viewport.X = v;
		viewport.Y = v2;
		viewport.Width = v3 - v;
		viewport.Height = v4 - v2;
		if (viewport.Width == 0 || viewport.Height == 0)
		{
			return false;
		}
		Engine.m_Device.Viewport = viewport;
		return true;
	}

	public static ICell FindTileFromXY(int mx, int my, ref int TileX, ref int TileY, bool onlyMobs)
	{
		if (World.Serial == 0)
		{
			return null;
		}
		Renderer.m_MousePoint.X = mx;
		Renderer.m_MousePoint.Y = my;
		Mobile player = World.Player;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		if (player != null)
		{
			num = player.X;
			num2 = player.Y;
			num3 = player.Z;
		}
		int num4;
		int num5;
		List<ICell>[,] cells;
		int num8;
		int num9;
		int num10;
		int num11;
		int num12;
		int num13;
		if (Engine.m_Ingame && mx >= Engine.GameX && my >= Engine.GameY && mx < Engine.GameX + Engine.GameWidth && my < Engine.GameY + Engine.GameHeight)
		{
			MapPackage map = Map.GetMap((num >> 3) - (Renderer.blockWidth >> 1), (num2 >> 3) - (Renderer.blockHeight >> 1), Renderer.blockWidth, Renderer.blockHeight);
			num4 = (num >> 3) - (Renderer.blockWidth >> 1) << 3;
			num5 = (num2 >> 3) - (Renderer.blockHeight >> 1) << 3;
			cells = map.cells;
			int num6 = num & 7;
			int num7 = num2 & 7;
			num8 = Renderer.blockWidth / 2 * 8 + num6;
			num9 = Renderer.blockHeight / 2 * 8 + num7;
			num10 = 0;
			num11 = 0;
			num10 = Engine.GameWidth >> 1;
			num10 -= 22;
			num10 += (4 - num6) * 22;
			num10 -= (4 - num7) * 22;
			num11 += (4 - num6) * 22;
			num11 += (4 - num7) * 22;
			num11 += num3 << 2;
			num11 += (Engine.GameHeight >> 1) - (num8 + num9) * 22 - (4 - num7) * 22 - (4 - num6) * 22 - 22;
			num10--;
			num11--;
			num10 += Engine.GameX;
			num11 += Engine.GameY;
			num12 = int.MaxValue;
			num13 = int.MaxValue;
			int count = cells[num8 + 1, num9 + 1].Count;
			for (int i = 0; i < count; i++)
			{
				ICell cell = cells[num8 + 1, num9 + 1][i];
				Type cellType = cell.CellType;
				if (cellType == Renderer.tStaticItem || cellType == Renderer.tDynamicItem)
				{
					ITile tile = (ITile)cell;
					if (Map.m_ItemFlags[tile.ID][TileFlag.Roof] && tile.Z >= num3 + 15 && tile.Z < num13)
					{
						num13 = tile.Z;
					}
				}
			}
			count = cells[num8, num9].Count;
			for (int j = 0; j < count; j++)
			{
				ICell cell2 = cells[num8, num9][j];
				Type cellType2 = cell2.CellType;
				if (!(cellType2 == Renderer.tStaticItem) && !(cellType2 == Renderer.tDynamicItem) && !(cellType2 == Renderer.tLandTile))
				{
					continue;
				}
				ITile tile2 = (ITile)cell2;
				if (Map.GetTileFlags(tile2.ID)[TileFlag.Roof])
				{
					continue;
				}
				int num14 = ((cellType2 == Renderer.tLandTile) ? tile2.SortZ : tile2.Z);
				if (num14 < num3 + 15)
				{
					continue;
				}
				if (cellType2 == Renderer.tLandTile)
				{
					if (num14 < num13)
					{
						num13 = num14;
					}
					if (num3 + 16 < num12)
					{
						num12 = num3 + 16;
					}
				}
				else if (num14 < num13)
				{
					num13 = num14;
				}
			}
			ICell lastFind = Renderer.m_LastFind;
			if (lastFind != null && Renderer.xwLast == num && Renderer.ywLast == num2 && Renderer.zwLast == num3)
			{
				Type cellType3 = lastFind.CellType;
				if (!onlyMobs || cellType3 == Renderer.tMobileCell)
				{
					int num15 = (Renderer.xLast - Renderer.yLast) * 22;
					int num16 = (Renderer.xLast + Renderer.yLast) * 22;
					if (cellType3 == Renderer.tMobileCell)
					{
						if (num13 >= int.MaxValue || lastFind.Z < num13)
						{
							IAnimatedCell animatedCell = (IAnimatedCell)lastFind;
							int Body = 0;
							int Direction = 0;
							int Hue = 0;
							int Action = 0;
							int Frame = 0;
							animatedCell.GetPackage(ref Body, ref Action, ref Direction, ref Frame, ref Hue);
							int num17 = num15 + 22;
							int num18 = num16 - (animatedCell.Z << 2) + 22;
							num17++;
							num18 -= 2;
							Mobile mobile = ((MobileCell)lastFind).m_Mobile;
							if (mobile != null)
							{
								IHue h = (mobile.Flags[MobileFlag.Hidden] ? Hues.Grayscale : ((Engine.m_Highlight != mobile) ? Hues.Load(Hue) : Hues.GetNotoriety(mobile.Notoriety)));
								int TextureX = 0;
								int TextureY = 0;
								Frame frame = Engine.m_Animations.GetFrame(mobile, Body, Action, Direction, Frame, num17, num18, h, ref TextureX, ref TextureY, preserveHue: false);
								if (frame.Image != null && !frame.Image.IsEmpty())
								{
									TextureX += num10;
									TextureY += num11;
									int num19 = TextureX;
									int num20 = TextureY;
									if (mx >= num19 && my >= num20 && mx < num19 + frame.Image.Width && my < num20 + frame.Image.Height)
									{
										if (frame.Image.Flip)
										{
											if (frame.Image.HitTest(-(mx - num19), my - num20))
											{
												TileX = mobile.X;
												TileY = mobile.Y;
												return lastFind;
											}
										}
										else if (frame.Image.HitTest(mx - num19, my - num20))
										{
											TileX = mobile.X;
											TileY = mobile.Y;
											return lastFind;
										}
									}
								}
							}
						}
					}
					else if (cellType3 == Renderer.tStaticItem || cellType3 == Renderer.tDynamicItem)
					{
						IItem item = (IItem)lastFind;
						if (item.ID != 16385 && item.ID != 22422 && item.ID != 24996 && item.ID != 24984 && item.ID != 25020 && item.ID != 24985 && (num13 >= int.MaxValue || (lastFind.Z < num13 && !Map.m_ItemFlags[item.ID][TileFlag.Roof])) && (!Map.m_ItemFlags[item.ID][TileFlag.Foliage] || Renderer.xLast < num8 || Renderer.yLast < num9 || Renderer.xLast >= num8 + 8 || Renderer.yLast >= num8 + 8))
						{
							if (item.CellType == Renderer.tDynamicItem)
							{
								DynamicItem dynamicItem = (DynamicItem)item;
								Item item2 = dynamicItem.m_Item;
								if (item2 != null && item2.ID == 8198)
								{
									int amount = item2.Amount;
									amount = GraphicTranslators.Corpse.Convert(amount);
									int animDirection = Engine.GetAnimDirection(item2.Direction);
									int actionID = Engine.m_Animations.ConvertAction(amount, item2.Serial, item2.X, item2.Y, animDirection, GenericAction.Die, null);
									int frameCount = Engine.m_Animations.GetFrameCount(amount, actionID, animDirection);
									int xCenter = num15 + 23;
									int yCenter = num16 - (item2.Z << 2) + 20;
									IHue h2 = Hues.Default;
									int TextureX2 = 0;
									int TextureY2 = 0;
									Frame frame2 = Engine.m_Animations.GetFrame(item2, amount, actionID, animDirection, frameCount - 1, xCenter, yCenter, h2, ref TextureX2, ref TextureY2, preserveHue: true);
									TextureX2 += num10;
									TextureY2 += num11;
									int num21 = TextureX2;
									int num22 = TextureY2;
									if (mx >= num21 && my >= num22 && mx < num21 + frame2.Image.Width && my < num22 + frame2.Image.Height)
									{
										if (frame2.Image.Flip)
										{
											if (frame2.Image.HitTest(-(mx - num21), my - num22))
											{
												TileX = item2.X;
												TileY = item2.Y;
												return lastFind;
											}
										}
										else if (frame2.Image.HitTest(mx - num21, my - num22))
										{
											TileX = item2.X;
											TileY = item2.Y;
											return lastFind;
										}
									}
									goto IL_0d37;
								}
							}
							int iD = item.ID;
							bool xDouble = false;
							if (cellType3 == Renderer.tStaticItem)
							{
								iD = Map.GetDispID(iD, 0, ref xDouble);
							}
							else
							{
								Item item3 = ((DynamicItem)lastFind).m_Item;
								iD = ((item3 != null) ? Map.GetDispID(iD, item3.Amount, ref xDouble) : Map.GetDispID(iD, 0, ref xDouble));
							}
							AnimData anim = Map.GetAnim(iD);
							Texture texture = ((anim.frameCount != 0 && Map.m_ItemFlags[iD][TileFlag.Animation]) ? Hues.Default.GetItem(iD + anim[Renderer.m_Frames / (anim.frameInterval + 1) % anim.frameCount]) : Hues.Default.GetItem(iD));
							if (texture != null && !texture.IsEmpty())
							{
								int num23 = num15 + 22;
								int num24 = num16 - (lastFind.Z << 2) + 43;
								num23 -= texture.Width >> 1;
								num24 -= texture.Height;
								num23 += num10;
								num24 += num11;
								if (xDouble && mx >= num23 && my >= num24 && mx < num23 + texture.Width + 5 && my < num24 + texture.Height + 5)
								{
									mx -= num23;
									my -= num24;
									if ((mx < texture.Width && my < texture.Height && texture.HitTest(mx, my)) || (mx >= 5 && my >= 5 && texture.HitTest(mx - 5, my - 5)))
									{
										TileX = (short)(num4 + Renderer.xLast);
										TileY = (short)(num5 + Renderer.yLast);
										return lastFind;
									}
									mx += num23;
									my += num24;
								}
								else if (!xDouble && mx >= num23 && my >= num24 && mx < num23 + texture.Width && my < num24 + texture.Height && texture.HitTest(mx - num23, my - num24))
								{
									TileX = (short)(num4 + Renderer.xLast);
									TileY = (short)(num5 + Renderer.yLast);
									return lastFind;
								}
							}
						}
					}
					else if (cellType3 == Renderer.tLandTile)
					{
						LandTile landTile = (LandTile)lastFind;
						int z = landTile.m_Z;
						if ((num12 >= int.MaxValue || landTile.SortZ < num12) && landTile.m_ID != 2)
						{
							int num25 = num15 + num10;
							int num26 = num16 + num11;
							if (mx >= num25 && mx < num25 + 44)
							{
								Renderer.m_PointPool[0].X = num25 + 22;
								Renderer.m_PointPool[0].Y = num26 - 4 * landTile.z00;
								Renderer.m_PointPool[1].X = num25 + 44;
								Renderer.m_PointPool[1].Y = num26 + 22 - 4 * landTile.z10;
								Renderer.m_PointPool[2].X = num25 + 22;
								Renderer.m_PointPool[2].Y = num26 + 44 - 4 * landTile.z11;
								Renderer.m_PointPool[3].X = num25;
								Renderer.m_PointPool[3].Y = num26 + 22 - 4 * landTile.z01;
								if (Renderer.LandTileHitTest(Renderer.m_PointPool, Renderer.m_MousePoint))
								{
									TileX = (short)(num4 + Renderer.xLast);
									TileY = (short)(num5 + Renderer.yLast);
									return lastFind;
								}
							}
						}
					}
					goto IL_0d37;
				}
				for (int num27 = Renderer.xLast + 6; num27 >= Renderer.xLast - 6; num27--)
				{
					for (int num28 = Renderer.yLast + 6; num28 >= Renderer.yLast - 6; num28--)
					{
						if (num27 >= 0 && num28 >= 0 && num27 < Renderer.cellWidth - 1 && num28 < Renderer.cellHeight - 1)
						{
							int num29 = (num27 - num28) * 22;
							int num30 = (num27 + num28) * 22;
							int count2 = cells[num27, num28].Count;
							for (int num31 = count2 - 1; num31 >= 0; num31--)
							{
								ICell cell3 = cells[num27, num28][num31];
								Type cellType4 = cell3.CellType;
								if (cellType4 == Renderer.tMobileCell)
								{
									if (num13 < int.MaxValue && cell3.Z >= num13)
									{
										continue;
									}
									IAnimatedCell animatedCell2 = (IAnimatedCell)cell3;
									int Body2 = 0;
									int Direction2 = 0;
									int Hue2 = 0;
									int Action2 = 0;
									int Frame2 = 0;
									animatedCell2.GetPackage(ref Body2, ref Action2, ref Direction2, ref Frame2, ref Hue2);
									int num32 = num29 + 22;
									int num33 = num30 - (animatedCell2.Z << 2) + 22;
									num32++;
									num33 -= 2;
									Mobile mobile2 = ((MobileCell)cell3).m_Mobile;
									IHue h3 = (mobile2.Flags[MobileFlag.Hidden] ? Hues.Grayscale : ((Engine.m_Highlight != mobile2) ? Hues.Load(Hue2) : Hues.GetNotoriety(mobile2.Notoriety)));
									int TextureX3 = 0;
									int TextureY3 = 0;
									Frame frame3 = Engine.m_Animations.GetFrame(mobile2, Body2, Action2, Direction2, Frame2, num32, num33, h3, ref TextureX3, ref TextureY3, preserveHue: false);
									if (frame3.Image == null || frame3.Image.IsEmpty())
									{
										continue;
									}
									TextureX3 += num10;
									TextureY3 += num11;
									int num34 = TextureX3;
									int num35 = TextureY3;
									if (mx < num34 || my < num35 || mx >= num34 + frame3.Image.Width || my >= num35 + frame3.Image.Height)
									{
										continue;
									}
									if (frame3.Image.Flip)
									{
										if (frame3.Image.HitTest(-(mx - num34), my - num35))
										{
											TileX = (short)(num4 + num27);
											TileY = (short)(num5 + num28);
											Renderer.m_LastFind = cell3;
											Renderer.xLast = num27;
											Renderer.yLast = num28;
											return cell3;
										}
									}
									else if (frame3.Image.HitTest(mx - num34, my - num35))
									{
										TileX = (short)(num4 + num27);
										TileY = (short)(num5 + num28);
										Renderer.m_LastFind = cell3;
										Renderer.xLast = num27;
										Renderer.yLast = num28;
										return cell3;
									}
								}
								else if (cellType4 == Renderer.tStaticItem || cellType4 == Renderer.tDynamicItem)
								{
									IItem item4 = (IItem)cell3;
									if (item4.ID == 16385 || item4.ID == 22422 || item4.ID == 24996 || item4.ID == 24984 || item4.ID == 25020 || item4.ID == 24985 || (num13 < int.MaxValue && (cell3.Z >= num13 || Map.m_ItemFlags[item4.ID][TileFlag.Roof])) || (Map.m_ItemFlags[item4.ID][TileFlag.Foliage] && num27 >= num8 && num28 >= num9 && num27 < num8 + 8 && num28 < num8 + 8))
									{
										continue;
									}
									if (item4.CellType == Renderer.tDynamicItem)
									{
										DynamicItem dynamicItem2 = (DynamicItem)item4;
										Item item5 = dynamicItem2.m_Item;
										if (item5 != null && item5.ID == 8198)
										{
											int amount2 = item5.Amount;
											amount2 = GraphicTranslators.Corpse.Convert(amount2);
											int animDirection2 = Engine.GetAnimDirection(item5.Direction);
											int actionID2 = Engine.m_Animations.ConvertAction(amount2, item5.Serial, item5.X, item5.Y, animDirection2, GenericAction.Die, null);
											int frameCount2 = Engine.m_Animations.GetFrameCount(amount2, actionID2, animDirection2);
											int xCenter2 = num29 + 23;
											int yCenter2 = num30 - (item5.Z << 2) + 20;
											IHue h4 = Hues.Default;
											int TextureX4 = 0;
											int TextureY4 = 0;
											Frame frame4 = Engine.m_Animations.GetFrame(item5, amount2, actionID2, animDirection2, frameCount2 - 1, xCenter2, yCenter2, h4, ref TextureX4, ref TextureY4, preserveHue: true);
											TextureX4 += num10;
											TextureY4 += num11;
											int num36 = TextureX4;
											int num37 = TextureY4;
											if (mx < num36 || my < num37 || mx >= num36 + frame4.Image.Width || my >= num37 + frame4.Image.Height)
											{
												continue;
											}
											if (frame4.Image.Flip)
											{
												if (frame4.Image.HitTest(-(mx - num36), my - num37))
												{
													TileX = item5.X;
													TileY = item5.Y;
													Renderer.m_LastFind = cell3;
													Renderer.xLast = num27;
													Renderer.yLast = num28;
													return cell3;
												}
											}
											else if (frame4.Image.HitTest(mx - num36, my - num37))
											{
												TileX = item5.X;
												TileY = item5.Y;
												Renderer.m_LastFind = cell3;
												Renderer.xLast = num27;
												Renderer.yLast = num28;
												return cell3;
											}
											continue;
										}
									}
									int iD2 = item4.ID;
									bool xDouble2 = false;
									if (cellType4 == Renderer.tStaticItem)
									{
										iD2 = Map.GetDispID(iD2, 0, ref xDouble2);
									}
									else
									{
										Item item6 = ((DynamicItem)cell3).m_Item;
										iD2 = ((item6 != null) ? Map.GetDispID(iD2, item6.Amount, ref xDouble2) : Map.GetDispID(iD2, 0, ref xDouble2));
									}
									AnimData anim2 = Map.GetAnim(iD2);
									Texture texture2 = ((anim2.frameCount != 0 && Map.m_ItemFlags[iD2][TileFlag.Animation]) ? Hues.Default.GetItem(iD2 + anim2[Renderer.m_Frames / (anim2.frameInterval + 1) % anim2.frameCount]) : Hues.Default.GetItem(iD2));
									if (texture2 == null || texture2.IsEmpty())
									{
										continue;
									}
									int num38 = num29 + 22;
									int num39 = num30 - (cell3.Z << 2) + 43;
									num38 -= texture2.Width >> 1;
									num39 -= texture2.Height;
									num38 += num10;
									num39 += num11;
									if (xDouble2 && mx >= num38 && my >= num39 && mx < num38 + texture2.Width + 5 && my < num39 + texture2.Height + 5)
									{
										mx -= num38;
										my -= num39;
										if ((mx < texture2.Width && my < texture2.Height && texture2.HitTest(mx, my)) || (mx >= 5 && my >= 5 && texture2.HitTest(mx - 5, my - 5)))
										{
											TileX = (short)(num4 + num27);
											TileY = (short)(num5 + num28);
											Renderer.m_LastFind = cell3;
											Renderer.xLast = num27;
											Renderer.yLast = num28;
											return cell3;
										}
										mx += num38;
										my += num39;
									}
									else if (!xDouble2 && mx >= num38 && my >= num39 && mx < num38 + texture2.Width && my < num39 + texture2.Height && texture2.HitTest(mx - num38, my - num39))
									{
										TileX = (short)(num4 + num27);
										TileY = (short)(num5 + num28);
										Renderer.m_LastFind = cell3;
										Renderer.xLast = num27;
										Renderer.yLast = num28;
										return cell3;
									}
								}
								else
								{
									if (!(cellType4 == Renderer.tLandTile))
									{
										continue;
									}
									LandTile landTile2 = (LandTile)cell3;
									int z2 = landTile2.m_Z;
									if (landTile2.m_ID == 2 || (num12 < int.MaxValue && landTile2.SortZ >= num12))
									{
										continue;
									}
									int num40 = num29 + num10;
									int num41 = num30 + num11;
									if (mx >= num40 && mx < num40 + 44)
									{
										Renderer.m_PointPool[0].X = num40 + 22;
										Renderer.m_PointPool[0].Y = num41 - 4 * landTile2.z00;
										Renderer.m_PointPool[1].X = num40 + 44;
										Renderer.m_PointPool[1].Y = num41 + 22 - 4 * landTile2.z10;
										Renderer.m_PointPool[2].X = num40 + 22;
										Renderer.m_PointPool[2].Y = num41 + 44 - 4 * landTile2.z11;
										Renderer.m_PointPool[3].X = num40;
										Renderer.m_PointPool[3].Y = num41 + 22 - 4 * landTile2.z01;
										if (Renderer.LandTileHitTest(Renderer.m_PointPool, Renderer.m_MousePoint))
										{
											TileX = (short)(num4 + num27);
											TileY = (short)(num5 + num28);
											Renderer.m_LastFind = cell3;
											Renderer.xLast = num27;
											Renderer.yLast = num28;
											return cell3;
										}
									}
								}
							}
						}
					}
				}
			}
			goto IL_179d;
		}
		goto IL_23d9;
		IL_23d9:
		TileX = -1;
		TileY = -1;
		Renderer.m_LastFind = null;
		Renderer.xLast = -1;
		Renderer.yLast = -1;
		return null;
		IL_0d37:
		bool flag = false;
		goto IL_179d;
		IL_179d:
		Renderer.m_LastFind = null;
		Renderer.xLast = -100;
		Renderer.yLast = -100;
		Renderer.xwLast = num;
		Renderer.ywLast = num2;
		Renderer.zwLast = num3;
		try
		{
			for (int num42 = Renderer.cellWidth - 2; num42 >= 0; num42--)
			{
				for (int num43 = Renderer.cellHeight - 2; num43 >= 0; num43--)
				{
					int num44 = (num42 - num43) * 22;
					int num45 = (num42 + num43) * 22;
					int count3 = cells[num42, num43].Count;
					for (int num46 = count3 - 1; num46 >= 0; num46--)
					{
						ICell cell4 = cells[num42, num43][num46];
						Type cellType5 = cell4.CellType;
						if (cellType5 == Renderer.tMobileCell)
						{
							if (num13 < int.MaxValue && cell4.Z >= num13)
							{
								continue;
							}
							IAnimatedCell animatedCell3 = (IAnimatedCell)cell4;
							int Body3 = 0;
							int Direction3 = 0;
							int Hue3 = 0;
							int Action3 = 0;
							int Frame3 = 0;
							animatedCell3.GetPackage(ref Body3, ref Action3, ref Direction3, ref Frame3, ref Hue3);
							int num47 = num44 + 22;
							int num48 = num45 - (animatedCell3.Z << 2) + 22;
							num47++;
							num48 -= 2;
							Mobile mobile3 = ((MobileCell)cell4).m_Mobile;
							IHue h5 = (mobile3.Flags[MobileFlag.Hidden] ? Hues.Grayscale : ((Engine.m_Highlight != mobile3) ? Hues.Load(Hue3) : Hues.GetNotoriety(mobile3.Notoriety)));
							int TextureX5 = 0;
							int TextureY5 = 0;
							Frame frame5 = Engine.m_Animations.GetFrame(mobile3, Body3, Action3, Direction3, Frame3, num47, num48, h5, ref TextureX5, ref TextureY5, preserveHue: false);
							if (frame5.Image == null || frame5.Image.IsEmpty())
							{
								continue;
							}
							TextureX5 += num10;
							TextureY5 += num11;
							int num49 = TextureX5;
							int num50 = TextureY5;
							if (mx < num49 || my < num50 || mx >= num49 + frame5.Image.Width || my >= num50 + frame5.Image.Height)
							{
								continue;
							}
							if (frame5.Image.Flip)
							{
								if (frame5.Image.HitTest(-(mx - num49), my - num50))
								{
									TileX = (short)(num4 + num42);
									TileY = (short)(num5 + num43);
									Renderer.m_LastFind = cell4;
									Renderer.xLast = num42;
									Renderer.yLast = num43;
									return cell4;
								}
							}
							else if (frame5.Image.HitTest(mx - num49, my - num50))
							{
								TileX = (short)(num4 + num42);
								TileY = (short)(num5 + num43);
								Renderer.m_LastFind = cell4;
								Renderer.xLast = num42;
								Renderer.yLast = num43;
								return cell4;
							}
						}
						else if (cellType5 == Renderer.tStaticItem || cellType5 == Renderer.tDynamicItem)
						{
							IItem item7 = (IItem)cell4;
							if (item7.ID == 16385 || item7.ID == 22422 || item7.ID == 24996 || item7.ID == 24984 || item7.ID == 25020 || item7.ID == 24985 || (num13 < int.MaxValue && (cell4.Z >= num13 || Map.m_ItemFlags[item7.ID][TileFlag.Roof])) || (Map.m_ItemFlags[item7.ID][TileFlag.Foliage] && num42 >= num8 && num43 >= num9 && num42 < num8 + 8 && num43 < num8 + 8))
							{
								continue;
							}
							if (item7.CellType == Renderer.tDynamicItem)
							{
								DynamicItem dynamicItem3 = (DynamicItem)item7;
								Item item8 = dynamicItem3.m_Item;
								if (item8 != null && item8.ID == 8198)
								{
									int amount3 = item8.Amount;
									amount3 = GraphicTranslators.Corpse.Convert(amount3);
									int animDirection3 = Engine.GetAnimDirection(item8.Direction);
									int actionID3 = Engine.m_Animations.ConvertAction(amount3, item8.Serial, item8.X, item8.Y, animDirection3, GenericAction.Die, null);
									int frameCount3 = Engine.m_Animations.GetFrameCount(amount3, actionID3, animDirection3);
									int xCenter3 = num44 + 23;
									int yCenter3 = num45 - (item8.Z << 2) + 20;
									IHue h6 = Hues.Default;
									int TextureX6 = 0;
									int TextureY6 = 0;
									Frame frame6 = Engine.m_Animations.GetFrame(item8, amount3, actionID3, animDirection3, frameCount3 - 1, xCenter3, yCenter3, h6, ref TextureX6, ref TextureY6, preserveHue: true);
									TextureX6 += num10;
									TextureY6 += num11;
									int num51 = TextureX6;
									int num52 = TextureY6;
									if (mx < num51 || my < num52 || mx >= num51 + frame6.Image.Width || my >= num52 + frame6.Image.Height)
									{
										continue;
									}
									if (frame6.Image.Flip)
									{
										if (frame6.Image.HitTest(-(mx - num51), my - num52))
										{
											TileX = item8.X;
											TileY = item8.Y;
											Renderer.m_LastFind = cell4;
											Renderer.xLast = num42;
											Renderer.yLast = num43;
											return cell4;
										}
									}
									else if (frame6.Image.HitTest(mx - num51, my - num52))
									{
										TileX = item8.X;
										TileY = item8.Y;
										Renderer.m_LastFind = cell4;
										Renderer.xLast = num42;
										Renderer.yLast = num43;
										return cell4;
									}
									continue;
								}
							}
							int iD3 = item7.ID;
							bool xDouble3 = false;
							if (cellType5 == Renderer.tStaticItem)
							{
								iD3 = Map.GetDispID(iD3, 0, ref xDouble3);
							}
							else
							{
								Item item9 = ((DynamicItem)cell4).m_Item;
								iD3 = ((item9 != null) ? Map.GetDispID(iD3, item9.Amount, ref xDouble3) : Map.GetDispID(iD3, 0, ref xDouble3));
							}
							AnimData anim3 = Map.GetAnim(iD3);
							int num53 = iD3;
							if (anim3.frameCount != 0 && Map.m_ItemFlags[iD3][TileFlag.Animation])
							{
								num53 += anim3[Renderer.m_Frames / (anim3.frameInterval + 1) % anim3.frameCount];
							}
							Texture item10 = Hues.Default.GetItem(num53);
							if (item10 == null || item10.IsEmpty())
							{
								continue;
							}
							int num54 = num44 + 22;
							int num55 = num45 - (cell4.Z << 2) + 43;
							num54 -= item10.Width >> 1;
							num55 -= item10.Height;
							num54 += num10;
							num55 += num11;
							if (xDouble3 && mx >= num54 && my >= num55 && mx < num54 + item10.Width + 5 && my < num55 + item10.Height + 5)
							{
								mx -= num54;
								my -= num55;
								if ((mx < item10.Width && my < item10.Height && item10.HitTest(mx, my)) || (mx >= 5 && my >= 5 && item10.HitTest(mx - 5, my - 5)))
								{
									TileX = (short)(num4 + num42);
									TileY = (short)(num5 + num43);
									Renderer.m_LastFind = cell4;
									Renderer.xLast = num42;
									Renderer.yLast = num43;
									return cell4;
								}
								mx += num54;
								my += num55;
							}
							else if (!xDouble3 && mx >= num54 && my >= num55 && mx < num54 + item10.Width && my < num55 + item10.Height && item10.HitTest(mx - num54, my - num55))
							{
								TileX = (short)(num4 + num42);
								TileY = (short)(num5 + num43);
								Renderer.m_LastFind = cell4;
								Renderer.xLast = num42;
								Renderer.yLast = num43;
								return cell4;
							}
						}
						else
						{
							if (!(cellType5 == Renderer.tLandTile))
							{
								continue;
							}
							LandTile landTile3 = (LandTile)cell4;
							int z3 = landTile3.m_Z;
							if (landTile3.m_ID == 2 || (num12 < int.MaxValue && landTile3.SortZ > num12))
							{
								continue;
							}
							int num56 = num44 + num10;
							int num57 = num45 + num11;
							if (mx >= num56 && mx < num56 + 44)
							{
								Renderer.m_PointPool[0].X = num56 + 22;
								Renderer.m_PointPool[0].Y = num57 - 4 * landTile3.z00;
								Renderer.m_PointPool[1].X = num56 + 44;
								Renderer.m_PointPool[1].Y = num57 + 22 - 4 * landTile3.z10;
								Renderer.m_PointPool[2].X = num56 + 22;
								Renderer.m_PointPool[2].Y = num57 + 44 - 4 * landTile3.z11;
								Renderer.m_PointPool[3].X = num56;
								Renderer.m_PointPool[3].Y = num57 + 22 - 4 * landTile3.z01;
								if (Renderer.LandTileHitTest(Renderer.m_PointPool, Renderer.m_MousePoint))
								{
									TileX = (short)(num4 + num42);
									TileY = (short)(num5 + num43);
									Renderer.m_LastFind = cell4;
									Renderer.xLast = num42;
									Renderer.yLast = num43;
									return cell4;
								}
							}
						}
					}
				}
			}
		}
		catch
		{
		}
		goto IL_23d9;
	}

	private static TransformedColoredTextured[] GeoPool(int count)
	{
		if (Renderer.m_GeoPool == null || Renderer.m_GeoPool.Length < count)
		{
			Renderer.m_GeoPool = new TransformedColoredTextured[count];
			for (int i = 0; i < count; i++)
			{
				Renderer.m_GeoPool[i].Rhw = 1f;
				Renderer.m_GeoPool[i].Color = Renderer.GetQuadColor(0);
			}
		}
		else
		{
			for (int j = 0; j < count; j++)
			{
				Renderer.m_GeoPool[j].Color = Renderer.GetQuadColor(0);
			}
		}
		return Renderer.m_GeoPool;
	}

	public unsafe static void TransparentRect(int Color, int X, int Y, int Width, int Height)
	{
		Width--;
		Height--;
		float num = X;
		float num2 = Y;
		TransformedColoredTextured[] array = Renderer.GeoPool(5);
		array[0].Color = (array[1].Color = (array[2].Color = (array[3].Color = Renderer.GetQuadColor(Color))));
		array[0].X = num;
		array[0].Y = num2;
		array[1].X = num + (float)Width;
		array[1].Y = num2;
		array[2].X = num + (float)Width;
		array[2].Y = num2 + (float)Height;
		array[3].X = num;
		array[3].Y = num2 + (float)Height;
		array[4] = array[0];
		fixed (TransformedColoredTextured* pVertex = array)
		{
			Renderer.PushLineStrip(pVertex, 5);
		}
	}

	public static void TransparentRect(int Color, int X, int Y, int Width, int Height, Clipper c)
	{
		Renderer.SolidRect(Color, X, Y, 1, Height, c);
		Renderer.SolidRect(Color, X + Width - 1, Y, 1, Height, c);
		Renderer.SolidRect(Color, X + 1, Y, Width - 2, 1, c);
		Renderer.SolidRect(Color, X + 1, Y + Height - 1, Width - 2, 1, c);
	}

	public unsafe static void SolidRect(int Color, int X, int Y, int Width, int Height)
	{
		if (Width > 0 && Height > 0)
		{
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Start();
			}
			int quadColor = Renderer.GetQuadColor(Color);
			float num = (float)X - 0.5f;
			float num2 = (float)Y - 0.5f;
			float x = num + (float)Width;
			float y = num2 + (float)Height;
			float z;
			IVertexStorage vertexStorage = Renderer.AcquireQuadStorage(out z);
			ArraySegment<byte> arraySegment = vertexStorage.Store(4, 1);
			fixed (byte* array = arraySegment.Array)
			{
				TransformedColoredTextured* ptr = (TransformedColoredTextured*)(array + arraySegment.Offset);
				ptr->Color = quadColor;
				ptr->Rhw = 1f;
				ptr->X = x;
				ptr->Y = y;
				ptr->Z = z;
				ptr++;
				ptr->Color = quadColor;
				ptr->Rhw = 1f;
				ptr->X = x;
				ptr->Y = num2;
				ptr->Z = z;
				ptr++;
				ptr->Color = quadColor;
				ptr->Rhw = 1f;
				ptr->X = num;
				ptr->Y = y;
				ptr->Z = z;
				ptr++;
				ptr->Color = quadColor;
				ptr->Rhw = 1f;
				ptr->X = num;
				ptr->Y = num2;
				ptr->Z = z;
			}
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Stop();
			}
		}
	}

	public unsafe static void SolidRect(int Color, int X, int Y, int Width, int Height, Clipper c)
	{
		if (Width <= 0 || Height <= 0)
		{
			return;
		}
		float num = -0.5f + (float)X;
		float num2 = -0.5f + (float)Y;
		TransformedColoredTextured[] array = Renderer.GeoPool(4);
		array[0].Color = (array[1].Color = (array[2].Color = (array[3].Color = Renderer.GetQuadColor(Color))));
		ref TransformedColoredTextured reference = ref array[0];
		float x = (array[1].X = num + (float)Width);
		reference.X = x;
		ref TransformedColoredTextured reference2 = ref array[0];
		x = (array[2].Y = num2 + (float)Height);
		reference2.Y = x;
		ref TransformedColoredTextured reference3 = ref array[1];
		x = (array[3].Y = num2);
		reference3.Y = x;
		ref TransformedColoredTextured reference4 = ref array[2];
		x = (array[3].X = num);
		reference4.X = x;
		if (c.Clip(X, Y, Width, Height, array))
		{
			fixed (TransformedColoredTextured* pVertex = array)
			{
				Renderer.PushQuad(pVertex);
			}
		}
	}

	public unsafe static void SolidQuad(int Color, Point[] pts)
	{
		if (Renderer._profile != null)
		{
			Renderer._profile._composeTime.Start();
		}
		int quadColor = Renderer.GetQuadColor(Color);
		float z;
		IVertexStorage vertexStorage = Renderer.AcquireQuadStorage(out z);
		ArraySegment<byte> arraySegment = vertexStorage.Store(4, 1);
		fixed (byte* array = arraySegment.Array)
		{
			TransformedColoredTextured* ptr = (TransformedColoredTextured*)(array + arraySegment.Offset);
			ptr->Color = quadColor;
			ptr->Rhw = 1f;
			ptr->X = (float)pts[2].X - 0.5f;
			ptr->Y = (float)pts[2].Y - 0.5f;
			ptr->Z = z;
			ptr++;
			ptr->Color = quadColor;
			ptr->Rhw = 1f;
			ptr->X = (float)pts[1].X - 0.5f;
			ptr->Y = (float)pts[1].Y - 0.5f;
			ptr->Z = z;
			ptr++;
			ptr->Color = quadColor;
			ptr->Rhw = 1f;
			ptr->X = (float)pts[3].X - 0.5f;
			ptr->Y = (float)pts[3].Y - 0.5f;
			ptr->Z = z;
			ptr++;
			ptr->Color = quadColor;
			ptr->Rhw = 1f;
			ptr->X = (float)pts[3].X - 0.5f;
			ptr->Y = (float)pts[3].Y - 0.5f;
			ptr->Z = z;
		}
		if (Renderer._profile != null)
		{
			Renderer._profile._composeTime.Stop();
		}
	}

	public unsafe static void GradientRect4(int c00, int c10, int c11, int c01, int X, int Y, int Width, int Height)
	{
		if (Width > 0 && Height > 0)
		{
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Start();
			}
			Renderer.GetColors(ref c00, ref c10, ref c11, ref c01);
			float num = (float)X - 0.5f;
			float num2 = (float)Y - 0.5f;
			float x = num + (float)Width;
			float y = num2 + (float)Height;
			float z;
			IVertexStorage vertexStorage = Renderer.AcquireQuadStorage(out z);
			ArraySegment<byte> arraySegment = vertexStorage.Store(4, 1);
			fixed (byte* array = arraySegment.Array)
			{
				TransformedColoredTextured* ptr = (TransformedColoredTextured*)(array + arraySegment.Offset);
				ptr->Color = c11;
				ptr->Rhw = 1f;
				ptr->X = x;
				ptr->Y = y;
				ptr->Z = z;
				ptr++;
				ptr->Color = c10;
				ptr->Rhw = 1f;
				ptr->X = x;
				ptr->Y = num2;
				ptr->Z = z;
				ptr++;
				ptr->Color = c01;
				ptr->Rhw = 1f;
				ptr->X = num;
				ptr->Y = y;
				ptr->Z = z;
				ptr++;
				ptr->Color = c00;
				ptr->Rhw = 1f;
				ptr->X = num;
				ptr->Y = num2;
				ptr->Z = z;
			}
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Stop();
			}
		}
	}

	public unsafe static void GradientRectLR(int Color, int Color2, int X, int Y, int Width, int Height)
	{
		if (Width > 0 && Height > 0)
		{
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Start();
			}
			Renderer.GetColors(ref Color, ref Color2);
			float num = (float)X - 0.5f;
			float num2 = (float)Y - 0.5f;
			float x = num + (float)Width;
			float y = num2 + (float)Height;
			float z;
			IVertexStorage vertexStorage = Renderer.AcquireQuadStorage(out z);
			ArraySegment<byte> arraySegment = vertexStorage.Store(4, 1);
			fixed (byte* array = arraySegment.Array)
			{
				TransformedColoredTextured* ptr = (TransformedColoredTextured*)(array + arraySegment.Offset);
				ptr->Color = Color2;
				ptr->Rhw = 1f;
				ptr->X = x;
				ptr->Y = y;
				ptr->Z = z;
				ptr++;
				ptr->Color = Color2;
				ptr->Rhw = 1f;
				ptr->X = x;
				ptr->Y = num2;
				ptr->Z = z;
				ptr++;
				ptr->Color = Color;
				ptr->Rhw = 1f;
				ptr->X = num;
				ptr->Y = y;
				ptr->Z = z;
				ptr++;
				ptr->Color = Color;
				ptr->Rhw = 1f;
				ptr->X = num;
				ptr->Y = num2;
				ptr->Z = z;
			}
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Stop();
			}
		}
	}

	public unsafe static void GradientRect(int Color, int Color2, int X, int Y, int Width, int Height)
	{
		if (Width > 0 && Height > 0)
		{
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Start();
			}
			Renderer.GetColors(ref Color, ref Color2);
			float num = (float)X - 0.5f;
			float num2 = (float)Y - 0.5f;
			float x = num + (float)Width;
			float y = num2 + (float)Height;
			float z;
			IVertexStorage vertexStorage = Renderer.AcquireQuadStorage(out z);
			ArraySegment<byte> arraySegment = vertexStorage.Store(4, 1);
			fixed (byte* array = arraySegment.Array)
			{
				TransformedColoredTextured* ptr = (TransformedColoredTextured*)(array + arraySegment.Offset);
				ptr->Color = Color2;
				ptr->Rhw = 1f;
				ptr->X = x;
				ptr->Y = y;
				ptr->Z = z;
				ptr++;
				ptr->Color = Color;
				ptr->Rhw = 1f;
				ptr->X = x;
				ptr->Y = num2;
				ptr->Z = z;
				ptr++;
				ptr->Color = Color2;
				ptr->Rhw = 1f;
				ptr->X = num;
				ptr->Y = y;
				ptr->Z = z;
				ptr++;
				ptr->Color = Color;
				ptr->Rhw = 1f;
				ptr->X = num;
				ptr->Y = num2;
				ptr->Z = z;
			}
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Stop();
			}
		}
	}

	public unsafe static void DrawLine(TransformedColoredTextured v1, TransformedColoredTextured v2, int color)
	{
		TransformedColoredTextured* ptr = stackalloc TransformedColoredTextured[2];
		*ptr = v1;
		ptr[1] = v2;
		ptr->Color = (ptr[1].Color = Renderer.GetQuadColor(color));
		Renderer.PushLineStrip(ptr, 2);
	}

	private unsafe static void PushPointList(TransformedColoredTextured* pVertex, int count)
	{
	}

	public unsafe static void PushLineStrip(TransformedColoredTextured* pVertex, int count)
	{
		if (Renderer._profile != null)
		{
			Renderer._profile._composeTime.Start();
		}
		float z = (float)(8191 - Renderer.m_Count) / 8192f;
		if (Renderer._drawGroup == 0)
		{
			Renderer.m_Count++;
		}
		IVertexStorage vertexStorage = (Renderer._alphaEnable ? Renderer.AcquireAlphaStorage(0) : Renderer.AcquireSolidStorage(0));
		ArraySegment<byte> arraySegment = vertexStorage.Store((count - 1) * 2, count - 1);
		fixed (byte* array = arraySegment.Array)
		{
			TransformedColoredTextured* ptr = (TransformedColoredTextured*)(array + arraySegment.Offset);
			for (int i = 0; i < count - 1; i++)
			{
				ptr[i * 2] = pVertex[i];
				ptr[i * 2].Z = z;
				ptr[i * 2 + 1] = pVertex[i + 1];
				ptr[i * 2 + 1].Z = z;
			}
		}
		if (Renderer._profile != null)
		{
			Renderer._profile._composeTime.Stop();
		}
	}

	public unsafe static void PushVertices(TransformedColoredTextured* pVertex, int vertexCount, int type)
	{
		if (Renderer._profile != null)
		{
			Renderer._profile._composeTime.Start();
		}
		float z = (float)(8191 - Renderer.m_Count) / 8192f;
		if (Renderer._drawGroup == 0)
		{
			Renderer.m_Count++;
		}
		IVertexStorage vertexStorage = (Renderer._alphaEnable ? Renderer.AcquireAlphaStorage(type) : Renderer.AcquireSolidStorage(type));
		ArraySegment<byte> arraySegment = vertexStorage.Store(vertexCount, 1);
		fixed (byte* array = arraySegment.Array)
		{
			TransformedColoredTextured* ptr = (TransformedColoredTextured*)(array + arraySegment.Offset);
			TransformedColoredTextured* ptr2 = ptr + vertexCount;
			while (ptr < ptr2)
			{
				*ptr = *pVertex;
				ptr->Z = z;
				ptr++;
				pVertex++;
			}
		}
		if (Renderer._profile != null)
		{
			Renderer._profile._composeTime.Stop();
		}
	}

	public static IVertexStorage AcquireQuadStorage(out float z)
	{
		z = (float)(8191 - Renderer.m_Count) / 8192f;
		if (Renderer._drawGroup == 0)
		{
			Renderer.m_Count++;
		}
		return Renderer._alphaEnable ? Renderer.AcquireAlphaStorage(1) : Renderer.AcquireSolidStorage(1);
	}

	private unsafe static void PushQuad(TransformedColoredTextured* pVertex)
	{
		if (Renderer._profile != null)
		{
			Renderer._profile._composeTime.Start();
		}
		float z = (float)(8191 - Renderer.m_Count) / 8192f;
		if (Renderer._drawGroup == 0)
		{
			Renderer.m_Count++;
		}
		IVertexStorage vertexStorage = (Renderer._alphaEnable ? Renderer.AcquireAlphaStorage(1) : Renderer.AcquireSolidStorage(1));
		ArraySegment<byte> arraySegment = vertexStorage.Store(4, 1);
		fixed (byte* array = arraySegment.Array)
		{
			TransformedColoredTextured* ptr = (TransformedColoredTextured*)(array + arraySegment.Offset);
			for (int i = 0; i < 4; i++)
			{
				ptr[i] = pVertex[i];
				ptr[i].Z = z;
			}
		}
		if (Renderer._profile != null)
		{
			Renderer._profile._composeTime.Stop();
		}
	}

	public unsafe static void DrawLines(TransformedColoredTextured[] v)
	{
		v[0].Color = (v[1].Color = (v[2].Color = (v[3].Color = (v[4].Color = Renderer.GetQuadColor(v[0].Color)))));
		fixed (TransformedColoredTextured* pVertex = v)
		{
			Renderer.PushLineStrip(pVertex, v.Length);
		}
	}

	public unsafe static void DrawPoints(params Point[] points)
	{
		int num = points.Length;
		TransformedColoredTextured[] array = Renderer.GeoPool(num);
		for (int i = 0; i < num; i++)
		{
			array[i].X = 0.5f + (float)points[i].X;
			array[i].Y = 0.5f + (float)points[i].Y;
		}
		fixed (TransformedColoredTextured* pVertex = array)
		{
			Renderer.PushPointList(pVertex, num);
		}
	}

	public unsafe static void DrawLine(int X1, int Y1, int X2, int Y2)
	{
		TransformedColoredTextured[] array = Renderer.GeoPool(2);
		array[0].X = X1;
		array[0].Y = Y1;
		array[1].X = X2;
		array[1].Y = Y2;
		fixed (TransformedColoredTextured* pVertex = array)
		{
			Renderer.PushLineStrip(pVertex, 2);
		}
	}

	public unsafe static void DrawLine(int X1, int Y1, int X2, int Y2, int color)
	{
		TransformedColoredTextured[] array = Renderer.GeoPool(2);
		array[0].X = X1;
		array[0].Y = Y1;
		array[1].X = X2;
		array[1].Y = Y2;
		array[0].Color = (array[1].Color = Renderer.GetQuadColor(color));
		fixed (TransformedColoredTextured* pVertex = array)
		{
			Renderer.PushLineStrip(pVertex, 2);
		}
	}

	public unsafe static void DrawQuadPrecalc(TransformedColoredTextured[] v)
	{
		fixed (TransformedColoredTextured* pVertex = v)
		{
			Renderer.PushQuad(pVertex);
		}
	}

	public unsafe static void DrawQuadPrecalc(TransformedColoredTextured* pVertex)
	{
		Renderer.PushQuad(pVertex);
	}

	public static void GetColors(ref int c0, ref int c1)
	{
		if (!Renderer._alphaEnable)
		{
			c0 |= -16777216;
			c1 |= -16777216;
			return;
		}
		int alphaBits = Renderer._alphaBits;
		c0 &= 16777215;
		c0 |= alphaBits;
		c1 &= 16777215;
		c1 |= alphaBits;
	}

	public static void GetColors(ref int c0, ref int c1, ref int c2, ref int c3)
	{
		if (!Renderer._alphaEnable)
		{
			c0 |= -16777216;
			c1 |= -16777216;
			c2 |= -16777216;
			c3 |= -16777216;
			return;
		}
		int alphaBits = Renderer._alphaBits;
		c0 &= 16777215;
		c0 |= alphaBits;
		c1 &= 16777215;
		c1 |= alphaBits;
		c2 &= 16777215;
		c2 |= alphaBits;
		c3 &= 16777215;
		c3 |= alphaBits;
	}

	public static int GetQuadColor(int Color)
	{
		if (!Renderer._alphaEnable)
		{
			Color |= -16777216;
		}
		else
		{
			Color &= 0xFFFFFF;
			Color |= Renderer._alphaBits;
		}
		return Color;
	}

	public static void Init(Capabilities Caps)
	{
		if (Renderer.m_VertexStream != null)
		{
			Renderer.m_VertexStream.Unlock();
		}
		Renderer.m_VertexStream = null;
		Renderer._alphaTestEnable = false;
		Renderer._alphaEnable = false;
		Engine.m_Device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse | VertexFormat.Texture1;
		Engine.m_Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
		Engine.m_Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);
		Engine.m_Device.SetSamplerState(0, SamplerState.BorderColor, SharpDX.Color.Magenta.ToBgra());
		Engine.m_Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Point);
		Engine.m_Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Point);
		Engine.m_Device.SetSamplerState(1, SamplerState.AddressU, TextureAddress.Clamp);
		Engine.m_Device.SetSamplerState(1, SamplerState.AddressV, TextureAddress.Clamp);
		Engine.m_Device.SetSamplerState(1, SamplerState.BorderColor, SharpDX.Color.Magenta.ToBgra());
		Engine.m_Device.SetSamplerState(1, SamplerState.MinFilter, TextureFilter.Linear);
		Engine.m_Device.SetSamplerState(1, SamplerState.MagFilter, TextureFilter.Linear);
		Engine.m_Device.SetRenderState(RenderState.ZEnable, enable: true);
		Engine.m_Device.SetRenderState(RenderState.ZWriteEnable, enable: true);
	}

	public static void SetTexture(Texture texture)
	{
		if (Renderer.m_Texture != texture)
		{
			Renderer.m_Texture = texture;
			Renderer.UpdateAlphaSettings();
		}
	}

	public static void Invalidate()
	{
		Renderer.m_Invalidate = true;
	}

	public static void PushAlpha(float alpha)
	{
		alpha *= Renderer._alphaValue;
		Renderer._alphaStack.Push(alpha);
		Renderer.UpdateAlpha(alpha);
	}

	public static void SetAlpha(float alpha)
	{
		Renderer.PopAlpha();
		Renderer.PushAlpha(alpha);
	}

	public static void PopAlpha()
	{
		if (Renderer._alphaStack.Count > 0)
		{
			Renderer._alphaStack.Pop();
		}
		if (Renderer._alphaStack.Count > 0)
		{
			Renderer.UpdateAlpha(Renderer._alphaStack.Peek());
		}
		else
		{
			Renderer.UpdateAlpha(1f);
		}
	}

	public static void UpdateAlphaSettings()
	{
		if (Renderer._blendType == DrawBlendType.Normal)
		{
			switch ((Renderer.m_Texture ?? Texture.Empty).Transparency)
			{
			case TextureTransparency.None:
				Renderer._alphaEnable = Renderer._alphaValue < 1f;
				Renderer._alphaTestEnable = false;
				break;
			case TextureTransparency.Simple:
				Renderer._alphaEnable = Renderer._alphaValue < 1f;
				Renderer._alphaTestEnable = true;
				break;
			case TextureTransparency.Complex:
				Renderer._alphaEnable = true;
				Renderer._alphaTestEnable = true;
				break;
			}
		}
		else
		{
			Renderer._alphaEnable = true;
			Renderer._alphaTestEnable = false;
		}
	}

	private static void UpdateAlpha(float alpha)
	{
		if (Renderer._alphaValue != alpha)
		{
			Renderer._alphaValue = alpha;
			Renderer._alphaBits = (int)(alpha * 255f);
			if (Renderer._alphaBits < 0)
			{
				Renderer._alphaBits = 0;
			}
			else if (Renderer._alphaBits > 255)
			{
				Renderer._alphaBits = -16777216;
			}
			else
			{
				Renderer._alphaBits <<= 24;
			}
			Renderer.UpdateAlphaSettings();
		}
	}

	public static void SetText(string text)
	{
		text = Engine.Encode(text);
		SpeechFormat speechFormat = SpeechFormat.Find(text);
		int hueId = Preferences.Current.SpeechHues[speechFormat.SpeechType];
		text = speechFormat.Mutate(text, display: true);
		if (Renderer.m_vTextCache == null)
		{
			Renderer.m_vTextCache = new VertexCache();
		}
		else
		{
			Renderer.m_vTextCache.Invalidate();
		}
		Renderer.m_TextSurface = Engine.GetUniFont(3).GetString(text, Hues.Load(hueId));
	}

	public unsafe static void Grid(LandTile lt, int x, int y, int bx, int by)
	{
		if (!Renderer.m_DrawGrid || bx + 44 <= Engine.GameX || bx >= Engine.GameX + Engine.GameWidth)
		{
			return;
		}
		TransformedColoredTextured[] array = Renderer.GeoPool(5);
		array[0].Color = (array[1].Color = (array[2].Color = (array[3].Color = 4227327)));
		array[0].X = bx + 22;
		array[0].Y = by - (lt.m_Z << 2);
		array[1].Y = by + 22 - (lt.m_Z << 2);
		array[1].X = bx + 44;
		array[2].Y = by + 44 - (lt.m_Z << 2);
		array[2].X = bx + 22;
		array[3].Y = by + 22 - (lt.m_Z << 2);
		array[3].X = bx;
		array[4] = array[0];
		Renderer.SetTexture(null);
		Renderer.DrawLines(array);
		int num = x & 7;
		if ((y & 7) == 0)
		{
			fixed (TransformedColoredTextured* ptr = array)
			{
				ptr->Color = (ptr[1].Color = Renderer.GetQuadColor(16720000));
				Renderer.PushLineStrip(ptr, 2);
			}
		}
		if (num == 0)
		{
			fixed (TransformedColoredTextured* ptr2 = array)
			{
				ptr2[3].Color = (ptr2[4].Color = Renderer.GetQuadColor(16720000));
				Renderer.PushLineStrip(ptr2 + 3, 2);
			}
		}
	}

	public static void ScreenShot(string title)
	{
		if (!Engine.GMPrivs)
		{
			Renderer.screenshotContext = new ScreenshotContext(title);
			Map.Invalidate();
			Renderer.Draw();
			Renderer.screenshotContext = null;
			Map.Invalidate();
		}
	}

	private static void SaveStream(object state)
	{
		try
		{
			object[] array = (object[])state;
			MemoryStream memoryStream = (MemoryStream)array[0];
			FileStream fileStream = new FileStream((string)array[1], FileMode.Create, FileAccess.Write, FileShare.None);
			memoryStream.WriteTo(fileStream);
			fileStream.Close();
			memoryStream.Close();
		}
		catch
		{
		}
	}

	public static void DrawMapLine(LandTile[,] landTiles, int bx, int by, int x, int y, int x2, int y2)
	{
		Renderer.SetTexture(null);
		Renderer.DrawLine(bx + 22, by - (landTiles[x, y].m_Z << 2), bx + 22 + (x2 - y2) * 22, by + 22 - (landTiles[x + x2, y + y2].m_Z << 2), 4259648);
	}

	public static void Draw()
	{
		try
		{
			if (!Engine.m_Display.WindowState.HasFlag(FormWindowState.Minimized))
			{
				Renderer.DrawUnsafe();
			}
		}
		catch (SharpDXException ex)
		{
			int code = ex.ResultCode.Code;
			if (code == ResultCode.DeviceLost.Code)
			{
				Application.DoEvents();
				Thread.Sleep(10);
			}
			else if (code == ResultCode.DeviceNotReset.Code)
			{
				Engine.ResetDevice();
				GC.Collect();
			}
			else
			{
				Thread.Sleep(10);
				Debug.Error(ex);
			}
		}
		catch (Exception ex2)
		{
			Debug.Error(ex2);
		}
	}

	public static bool Validate()
	{
		Result result = Engine.m_Device.TestCooperativeLevel();
		if (result.Success)
		{
			return true;
		}
		int code = result.Code;
		if (code == ResultCode.DeviceLost.Code)
		{
			Application.DoEvents();
			Thread.Sleep(10);
		}
		else
		{
			if (code == ResultCode.DeviceNotReset.Code)
			{
				Engine.ResetDevice();
				GC.Collect();
				return true;
			}
			Application.DoEvents();
			Thread.Sleep(10);
		}
		return false;
	}

	public static void DrawPlayerIcon(int x, int y, Texture icon, int color)
	{
		y = ((icon != Engine.ImageCache.LastTargetIcon) ? (y - icon.Height * 2 / 3) : (y - icon.Height / 2));
		x -= icon.Width / 2;
		icon.DrawGame(x, y, color);
	}

	public static MapSubgroup[] GetMapSubgroups(int size)
	{
		if (Renderer._mapSubgroups == null)
		{
			Renderer._mapSubgroups = new MapSubgroup[size * size * 2];
			int num = 0;
			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					Renderer._mapSubgroups[num++] = new MapSubgroup(j, i, ground: true);
					Renderer._mapSubgroups[num++] = new MapSubgroup(j, i, ground: false);
				}
			}
			Array.Sort(Renderer._mapSubgroups, delegate(MapSubgroup a, MapSubgroup b)
			{
				int num2 = a.x + a.y;
				int num3 = b.x + b.y;
				int num4 = num2 - num3;
				if (num4 == 0)
				{
					num2 = a.x - a.y - (a.ground ? 3 : 0);
					num3 = b.x - b.y - (b.ground ? 3 : 0);
					num4 = num2 - num3;
				}
				return num4;
			});
		}
		return Renderer._mapSubgroups;
	}

	private static int GetLightHueByItemId(int itemId)
	{
		int result = 0;
		if ((itemId >= 15874 && itemId <= 15883) || (itemId >= 14612 && itemId <= 14633))
		{
			result = 2801;
		}
		else if ((itemId >= 14732 && itemId <= 14751) || (itemId >= 15911 && itemId <= 15930))
		{
			result = 2831;
		}
		else if (itemId >= 14662 && itemId <= 14692)
		{
			result = 2806;
		}
		else if (itemId >= 14695 && itemId <= 14730)
		{
			result = 2806;
		}
		else if ((itemId >= 3633 && itemId <= 3635) || itemId == 6587 || itemId == 7979)
		{
			result = 2840;
		}
		else if (itemId == 3948 || itemId == 8148)
		{
			result = 2802;
		}
		else if (itemId == 4017 || (itemId >= 6522 && itemId <= 6569))
		{
			result = 2860;
		}
		else if (itemId >= 6571 && itemId <= 6582)
		{
			result = 2860;
		}
		else if (itemId >= 3676 && itemId <= 3690)
		{
			result = 2806;
		}
		else if (itemId >= 3629 && itemId <= 3632)
		{
			result = 2862;
		}
		else if ((itemId >= 13639 && itemId <= 13644) || (itemId >= 13371 && itemId <= 13420) || (itemId >= 12934 && itemId <= 12955))
		{
			result = 2831;
		}
		else if (itemId >= 4846 && itemId <= 4941)
		{
			result = 2831;
		}
		else if (itemId >= 7885 && itemId <= 7887)
		{
			result = 2801;
		}
		else if (itemId >= 7888 && itemId <= 7890)
		{
			result = 2803;
		}
		else if ((itemId >= 6217 && itemId <= 6224) || (itemId >= 6227 && itemId <= 6234))
		{
			result = 2861;
		}
		else if (itemId >= 3553 && itemId <= 3562)
		{
			result = 2831;
		}
		else if (itemId == 4012 || (itemId >= 2555 && itemId <= 2580))
		{
			result = 2830;
		}
		else if (itemId == 5703)
		{
			result = 2861;
		}
		else if (itemId == 14239)
		{
			result = 2806;
		}
		else if (itemId >= 14000 && itemId <= 14035)
		{
			result = 2860;
		}
		else if (itemId >= 14036 && itemId <= 14051)
		{
			result = 2860;
		}
		else if (itemId >= 14052 && itemId <= 14067)
		{
			result = 2860;
		}
		return result;
	}

	private static float GetSample(int serial, int n)
	{
		float num = 0f;
		for (int i = 0; i < 2; i++)
		{
			uint num2 = (uint)(1664525 * (1664525 * (1664525 * (serial + 1013904223) + serial) + (n * 2 + i)));
			num2 *= num2 * (num2 * 15731 + 789221) + 1376312589;
			num += (float)num2 / 4.2949673E+09f;
		}
		return Math.Abs(num - 1f);
	}

	public static void RenderLight(int serial, int x, int y, int itemId, int lightId)
	{
		Renderer.RenderLight(serial, x, y, itemId, lightId, 0);
	}

	public static void RenderLight(int serial, int x, int y, int itemId, int lightId, int lightHueId)
	{
		float sample = Renderer.GetSample(serial, Renderer.m_Frames / 10);
		float sample2 = Renderer.GetSample(serial, Renderer.m_Frames / 10 + 1);
		float num = (float)(Renderer.m_Frames % 10) / 10f;
		float num2 = sample * (1f - num) + sample2 * num;
		num2 = 0.75f + num2 * 0.25f;
		Renderer.RenderLight(serial, x, y, itemId, lightId, lightHueId, num2);
	}

	public static void RenderLight(int serial, int x, int y, int itemId, int lightId, int lightHueId, float alpha)
	{
		if (lightHueId <= 0)
		{
			lightHueId = Renderer.GetLightHueByItemId(itemId);
		}
		if (lightId == 0)
		{
			lightId = Map.GetQuality(itemId);
		}
		if ((lightId == 26 || lightId == 27) && ((itemId >= 10678 && itemId <= 10687) || (itemId >= 59 && itemId <= 60)))
		{
			y -= ((lightId == 26) ? 10 : 8);
		}
		Texture light = Hues.Load((lightHueId != 0) ? (lightHueId + 1) : 0).GetLight(lightId);
		if (light != null)
		{
			Renderer.PushAlpha(alpha);
			light.Draw(x - light.Width / 2, y - light.Height / 2, 16777215);
			Renderer.PopAlpha();
		}
	}

	private static void RenderLights()
	{
		Mobile player = World.Player;
		if (player == null)
		{
			return;
		}
		int num = Engine.Effects.GlobalLight;
		if (player != null)
		{
			num -= player.LightLevel;
		}
		if (num < 0)
		{
			num = 0;
		}
		else if (num > 31)
		{
			num = 31;
		}
		if (num == 0)
		{
			return;
		}
		int x = player.X;
		int y = player.Y;
		int z = player.Z;
		MapPackage map = Map.GetMap((x >> 3) - (Renderer.blockWidth >> 1), (y >> 3) - (Renderer.blockHeight >> 1), Renderer.blockWidth, Renderer.blockHeight);
		int num2 = (x >> 3) - (Renderer.blockWidth >> 1) << 3;
		int num3 = (y >> 3) - (Renderer.blockHeight >> 1) << 3;
		List<ICell>[,] cells = map.cells;
		int num4 = x & 7;
		int num5 = y & 7;
		int num6 = Renderer.blockWidth / 2 * 8 + num4;
		int num7 = Renderer.blockHeight / 2 * 8 + num5;
		int num8 = 0;
		int num9 = 0;
		num8 = Engine.GameWidth >> 1;
		num8 -= 22;
		num8 += (4 - num4) * 22;
		num8 -= (4 - num5) * 22;
		num9 += (4 - num4) * 22;
		num9 += (4 - num5) * 22;
		num9 += z << 2;
		num9 += (Engine.GameHeight >> 1) - (num6 + num7) * 22 - (4 - num5) * 22 - (4 - num4) * 22 - 22;
		num8--;
		num9--;
		if (player != null && player.Walking.Count > 0)
		{
			WalkAnimation walkAnimation = player.Walking.Peek();
			int xOffset = 0;
			int yOffset = 0;
			int fOffset = 0;
			if (!walkAnimation.Snapshot(ref xOffset, ref yOffset, ref fOffset))
			{
				if (!walkAnimation.Advance)
				{
					xOffset = walkAnimation.xOffset;
					yOffset = walkAnimation.yOffset;
				}
				else
				{
					xOffset = 0;
					yOffset = 0;
				}
			}
			num8 -= xOffset;
			num9 -= yOffset;
			Renderer.m_xScroll = xOffset;
			Renderer.m_yScroll = yOffset;
		}
		int size = ((Renderer.cellWidth < Renderer.cellHeight) ? (Renderer.cellWidth - 1) : (Renderer.cellHeight - 1));
		Renderer.PushAll();
		if (Renderer.lightTexture == null)
		{
			Renderer.lightTexture = new Texture(Engine.GameWidth, Engine.GameHeight, Format.A8R8G8B8, Pool.Default, isReconstruct: false, TextureTransparency.Complex, Usage.RenderTarget);
		}
		if (Renderer.lightSurface == null)
		{
			Renderer.lightSurface = Renderer.lightTexture.m_Surface.GetSurfaceLevel(0);
		}
		MapSubgroup[] mapSubgroups = Renderer.GetMapSubgroups(size);
		int num10 = int.MaxValue;
		int num11 = int.MaxValue;
		int count = cells[num6 + 1, num7 + 1].Count;
		for (int i = 0; i < count; i++)
		{
			ICell cell = cells[num6 + 1, num7 + 1][i];
			Type cellType = cell.CellType;
			if (cellType == Renderer.tStaticItem || cellType == Renderer.tDynamicItem)
			{
				ITile tile = (ITile)cell;
				if (Map.m_ItemFlags[tile.ID][TileFlag.Roof] && tile.Z >= z + 15 && tile.Z < num10)
				{
					num10 = tile.Z;
				}
			}
		}
		count = cells[num6, num7].Count;
		for (int j = 0; j < count; j++)
		{
			ICell cell2 = cells[num6, num7][j];
			Type cellType2 = cell2.CellType;
			if (!(cellType2 == Renderer.tStaticItem) && !(cellType2 == Renderer.tDynamicItem) && !(cellType2 == Renderer.tLandTile))
			{
				continue;
			}
			ITile tile2 = (ITile)cell2;
			if (Map.GetTileFlags(tile2.ID)[TileFlag.Roof])
			{
				continue;
			}
			int num12 = ((cellType2 == Renderer.tLandTile) ? tile2.SortZ : tile2.Z);
			if (num12 < z + 15)
			{
				continue;
			}
			if (cellType2 == Renderer.tLandTile)
			{
				if (num12 < num10)
				{
					num10 = num12;
				}
				if (z + 16 < num11)
				{
					num11 = z + 16;
				}
			}
			else if (num12 < num10)
			{
				num10 = num12;
			}
		}
		Engine.m_Device.SetRenderTarget(0, Renderer.lightSurface);
		byte b = (byte)(255 - (num * 255 + 15) / 31);
		Engine.m_Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new ColorBGRA(b, b, b, 0), 1f, 0);
		Engine.m_Device.BeginScene();
		try
		{
			Renderer.SetAlpha(1f);
			Renderer.SetBlendType(DrawBlendType.Additive);
			for (int k = 0; k < mapSubgroups.Length; k++)
			{
				MapSubgroup mapSubgroup = mapSubgroups[k];
				int x2 = mapSubgroup.x;
				int y2 = mapSubgroup.y;
				bool flag = false;
				int num13 = (x2 - y2) * 22 + num8;
				int num14 = (x2 + y2) * 22 + num9;
				int count2 = cells[x2, y2].Count;
				for (int l = 0; l < count2; l++)
				{
					ICell cell3 = cells[x2, y2][l];
					Type cellType3 = cell3.CellType;
					if (mapSubgroup.ground == flag)
					{
						if (cellType3 == Renderer.tLandTile)
						{
							flag = true;
						}
					}
					else if (cellType3 == Renderer.tLandTile)
					{
						flag = true;
						int graphicId = ((LandTile)cell3).graphicId;
						if (graphicId >= 500 && graphicId <= 503)
						{
							Renderer.RenderLight(x2 * 4096 + y2, num13 + 22, num14 + 22 - cell3.Z * 4, 4846, 0);
						}
					}
					else if (cellType3 == Renderer.tMobileCell)
					{
						Mobile mobile = ((MobileCell)cell3).m_Mobile;
						if (mobile != null)
						{
							Item item = mobile.FindEquip(Layer.TwoHanded);
							int xOffset2 = 0;
							int yOffset2 = 0;
							int fOffset2 = 0;
							if (mobile.Walking.Count > 0)
							{
								WalkAnimation walkAnimation2 = mobile.Walking.Peek();
								walkAnimation2.Snapshot(ref xOffset2, ref yOffset2, ref fOffset2);
							}
							if (item != null && Map.m_ItemFlags[item.ID & 0x3FFF][TileFlag.LightSource])
							{
								Renderer.RenderLight(mobile.Serial, num13 + xOffset2 + 22, num14 + yOffset2 - cell3.Z * 4, item.ID & 0x3FFF, 29);
							}
						}
					}
					else
					{
						if (!(cellType3 == Renderer.tStaticItem) && !(cellType3 == Renderer.tDynamicItem))
						{
							continue;
						}
						bool flag2 = cellType3 == Renderer.tStaticItem;
						bool flag3 = !flag2;
						IItem item2 = (IItem)cell3;
						ushort iD;
						sbyte z2;
						TileFlags tileFlags;
						Item item3;
						if (flag2)
						{
							StaticItem staticItem = (StaticItem)item2;
							iD = staticItem.m_ID;
							if (iD == 16385 || iD == 22422 || iD == 24996 || iD == 24984 || iD == 25020 || iD == 24985)
							{
								continue;
							}
							z2 = staticItem.m_Z;
							tileFlags = Map.m_ItemFlags[iD];
							bool flag4 = num10 < int.MaxValue && (z2 >= num10 || tileFlags[TileFlag.Roof]);
							if (flag4)
							{
								continue;
							}
							if (x2 + 1 < Renderer.cellWidth && y2 + 1 < Renderer.cellHeight)
							{
								count = cells[x2 + 1, y2 + 1].Count;
								for (int m = 0; m < count; m++)
								{
									ICell cell4 = cells[x2 + 1, y2 + 1][m];
									Type cellType4 = cell4.CellType;
									if (cellType4 == Renderer.tStaticItem || cellType4 == Renderer.tDynamicItem)
									{
										ITile tile3 = (ITile)cell4;
										if ((num10 >= int.MaxValue || (tile3.Z < num10 && !Map.m_ItemFlags[tile3.ID][TileFlag.Roof])) && !Map.m_ItemFlags[tile3.ID][TileFlag.Roof] && tile3.Z >= cell3.Z + 1)
										{
											flag4 = true;
											break;
										}
									}
								}
								if (flag4)
								{
									continue;
								}
							}
							if (x2 + 2 < Renderer.cellWidth && y2 + 2 < Renderer.cellHeight)
							{
								count = cells[x2 + 2, y2 + 2].Count;
								for (int n = 0; n < count; n++)
								{
									ICell cell5 = cells[x2 + 2, y2 + 2][n];
									Type cellType5 = cell5.CellType;
									if (cellType5 == Renderer.tStaticItem || cellType5 == Renderer.tDynamicItem)
									{
										ITile tile4 = (ITile)cell5;
										if ((num10 >= int.MaxValue || (tile4.Z < num10 && !Map.m_ItemFlags[tile4.ID][TileFlag.Roof])) && Map.m_ItemFlags[tile4.ID][TileFlag.Roof] && tile4.Z >= cell3.Z + cell3.Height)
										{
											flag4 = true;
											break;
										}
									}
								}
								if (flag4)
								{
									continue;
								}
							}
							item3 = null;
						}
						else
						{
							DynamicItem dynamicItem = (DynamicItem)item2;
							iD = dynamicItem.m_ID;
							if (iD == 16385 || iD == 22422 || iD == 24996 || iD == 24984 || iD == 25020 || iD == 24985)
							{
								continue;
							}
							z2 = dynamicItem.m_Z;
							tileFlags = Map.m_ItemFlags[iD];
							if (num10 < int.MaxValue && (z2 >= num10 || tileFlags[TileFlag.Roof]))
							{
								continue;
							}
							item3 = dynamicItem.m_Item;
						}
						int serial = item3?.Serial ?? (x2 * 4096 + y2);
						if (tileFlags[TileFlag.LightSource])
						{
							Renderer.RenderLight(serial, num13 + 22, num14 + 22 - z2 * 4, iD & 0x3FFF, item3?.Direction ?? 0);
						}
					}
				}
			}
			Engine.Effects.RenderLights();
			Renderer.PushAll();
		}
		finally
		{
			Engine.m_Device.EndScene();
			Engine.m_Device.SetRenderTarget(0, Renderer.AcquireBackBuffer());
		}
		Renderer.SetAlpha(1f);
		Renderer.SetBlendType(DrawBlendType.Normal);
	}

	public unsafe static void DrawUnsafe()
	{
		if (Engine.m_Device == null || !Renderer.Validate())
		{
			return;
		}
		if (!Renderer._timeRefresh && Renderer._profile != null)
		{
			Renderer._profile.Reset();
		}
		Renderer.RenderLights();
		Stats.Reset();
		Engine.m_Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, SharpDX.Color.Black, 1f, 0);
		Queue<Mobile> queue = Renderer.m_ToUpdateQueue;
		if (queue == null)
		{
			queue = (Renderer.m_ToUpdateQueue = new Queue<Mobile>());
		}
		else if (queue.Count > 0)
		{
			queue.Clear();
		}
		Engine.m_Device.BeginScene();
		if (Renderer._profile != null)
		{
			Renderer._profile._drawTime.Start();
		}
		try
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			bool flag = false;
			bool flag2 = false;
			Renderer.m_xWorld = (Renderer.m_yWorld = (Renderer.m_zWorld = 0));
			Mobile player = World.Player;
			if (player != null)
			{
				flag = player.Ghost;
				flag2 = player.Flags[MobileFlag.Warmode];
				num = (Renderer.m_xWorld = player.X);
				num2 = (Renderer.m_yWorld = player.Y);
				num3 = (Renderer.m_zWorld = player.Z);
			}
			Renderer.m_Dead = flag;
			Renderer.m_xScroll = 0;
			Renderer.m_yScroll = 0;
			List<TextMessage> list = Renderer.m_TextToDrawList;
			if (list == null)
			{
				list = (Renderer.m_TextToDrawList = new List<TextMessage>());
			}
			else if (list.Count > 0)
			{
				list.Clear();
			}
			Renderer.m_TextToDraw = list;
			Queue<MiniHealthEntry> queue2 = Renderer.m_MiniHealthQueue;
			if (queue2 == null)
			{
				queue2 = (Renderer.m_MiniHealthQueue = new Queue<MiniHealthEntry>());
			}
			else if (queue2.Count > 0)
			{
				queue2.Clear();
			}
			Renderer.eOffsetX = 0;
			Renderer.eOffsetY = 0;
			if (Engine.m_Ingame)
			{
				if (GDesktopBorder.Instance != null)
				{
					GDesktopBorder.Instance.DoRender();
				}
				Renderer.SetViewport(Engine.GameX, Engine.GameY, Engine.GameWidth, Engine.GameHeight);
				if (Renderer._profile != null)
				{
					Renderer._profile._worldTime.Start();
				}
				Map.Lock();
				MapPackage map = Map.GetMap((num >> 3) - (Renderer.blockWidth >> 1), (num2 >> 3) - (Renderer.blockHeight >> 1), Renderer.blockWidth, Renderer.blockHeight);
				int num4 = (num >> 3) - (Renderer.blockWidth >> 1) << 3;
				int num5 = (num2 >> 3) - (Renderer.blockHeight >> 1) << 3;
				List<ICell>[,] cells = map.cells;
				int num6 = num & 7;
				int num7 = num2 & 7;
				int num8 = Renderer.blockWidth / 2 * 8 + num6;
				int num9 = Renderer.blockHeight / 2 * 8 + num7;
				int num10 = 0;
				int num11 = 0;
				num10 = Engine.GameWidth >> 1;
				num10 -= 22;
				num10 += (4 - num6) * 22;
				num10 -= (4 - num7) * 22;
				num11 += (4 - num6) * 22;
				num11 += (4 - num7) * 22;
				num11 += num3 << 2;
				num11 += (Engine.GameHeight >> 1) - (num8 + num9) * 22 - (4 - num7) * 22 - (4 - num6) * 22 - 22;
				int num12 = num10 - 1;
				num10 = num12;
				num12 = num11 - 1;
				num11 = num12;
				num10 += Engine.GameX;
				num11 += Engine.GameY;
				_ = World.Player;
				bool flag3 = false;
				Renderer.m_xScroll = (Renderer.m_yScroll = 0);
				if (player != null && player.Walking.Count > 0)
				{
					WalkAnimation walkAnimation = player.Walking.Peek();
					int xOffset = 0;
					int yOffset = 0;
					int fOffset = 0;
					if (!walkAnimation.Snapshot(ref xOffset, ref yOffset, ref fOffset))
					{
						if (!walkAnimation.Advance)
						{
							xOffset = walkAnimation.xOffset;
							yOffset = walkAnimation.yOffset;
						}
						else
						{
							xOffset = 0;
							yOffset = 0;
						}
					}
					num10 -= xOffset;
					num11 -= yOffset;
					Renderer.m_xScroll = xOffset;
					Renderer.m_yScroll = yOffset;
				}
				bool flag4 = !Renderer.m_Invalidate && Renderer.m_CharX == num && Renderer.m_CharY == num2 && Renderer.m_CharZ == num3 && Renderer.m_WasDead == flag && Renderer.m_xBaseLast == num10 && Renderer.m_yBaseLast == num11;
				Renderer.m_xBaseLast = num10;
				Renderer.m_yBaseLast = num11;
				Renderer.m_Invalidate = false;
				Renderer.m_WasDead = flag;
				Renderer.m_CharX = num;
				Renderer.m_CharY = num2;
				Renderer.m_CharZ = num3;
				bool notorietyHalos = Options.Current.NotorietyHalos;
				List<DrawQueueEntry> list2 = new List<DrawQueueEntry>();
				int num13 = int.MaxValue;
				int num14 = int.MaxValue;
				int count = cells[num8 + 1, num9 + 1].Count;
				for (int num15 = 0; num15 < count; num15 = num12)
				{
					ICell cell = cells[num8 + 1, num9 + 1][num15];
					Type cellType = cell.CellType;
					if (cellType == Renderer.tStaticItem || cellType == Renderer.tDynamicItem)
					{
						ITile tile = (ITile)cell;
						if (Map.m_ItemFlags[tile.ID][TileFlag.Roof] && tile.Z >= num3 + 15 && tile.Z < num13)
						{
							num13 = tile.Z;
						}
					}
					num12 = num15 + 1;
				}
				count = cells[num8, num9].Count;
				for (int num16 = 0; num16 < count; num16 = num12)
				{
					ICell cell2 = cells[num8, num9][num16];
					Type cellType2 = cell2.CellType;
					if (cellType2 == Renderer.tStaticItem || cellType2 == Renderer.tDynamicItem || cellType2 == Renderer.tLandTile)
					{
						ITile tile2 = (ITile)cell2;
						if (!Map.GetTileFlags(tile2.ID)[TileFlag.Roof])
						{
							int num17 = ((cellType2 == Renderer.tLandTile) ? tile2.SortZ : tile2.Z);
							if (num17 >= num3 + 15)
							{
								if (cellType2 == Renderer.tLandTile)
								{
									if (num17 < num13)
									{
										num13 = num17;
									}
									if (num3 + 16 < num14)
									{
										num14 = num3 + 16;
									}
								}
								else if (num17 < num13)
								{
									num13 = num17;
								}
							}
						}
					}
					num12 = num16 + 1;
				}
				Renderer.m_CullLand = num14 < int.MaxValue;
				IHue hue = (flag ? Hues.Grayscale : Hues.Default);
				Queue<TransparentDraw> queue3 = Renderer.m_TransDrawQueue;
				if (queue3 == null)
				{
					queue3 = (Renderer.m_TransDrawQueue = new Queue<TransparentDraw>());
				}
				else if (queue3.Count > 0)
				{
					queue3.Clear();
				}
				RenderSettings renderSettings = Preferences.Current.RenderSettings;
				bool itemShadows = renderSettings.ItemShadows;
				bool characterShadows = renderSettings.CharacterShadows;
				_ = renderSettings.SmoothCharacters;
				bool flag5 = Engine.m_MultiPreview;
				int num18 = 0;
				int num19 = 0;
				int num20 = 0;
				int num21 = 0;
				int num22 = 0;
				int num23 = 0;
				int num24 = 0;
				int num25 = 0;
				int num26 = 0;
				int num27 = 0;
				int num28 = 0;
				int num29 = 0;
				int num30 = 0;
				BaseTargetHandler active = TargetManager.Active;
				bool flag6 = flag2 || active != null;
				if (flag5)
				{
					if (Gumps.IsWorldAt(Engine.m_xMouse, Engine.m_yMouse, CheckDrag: true))
					{
						int TileX = 0;
						int TileY = 0;
						ICell cell3 = Renderer.FindTileFromXY(Engine.m_xMouse, Engine.m_yMouse, ref TileX, ref TileY, onlyMobs: false);
						if (cell3 == null)
						{
							flag5 = false;
						}
						else if (cell3.CellType == Renderer.tLandTile || cell3.CellType == Renderer.tStaticItem)
						{
							num18 = TileX - num4;
							num19 = TileY - num5;
							num20 = cell3.Z + (sbyte)((cell3.CellType == Renderer.tStaticItem) ? cell3.Height : 0);
							num21 = Engine.m_MultiList.Count;
							num18 -= Engine.m_xMultiOffset;
							num19 -= Engine.m_yMultiOffset;
							num20 -= Engine.m_zMultiOffset;
							num22 = num18 + Engine.m_MultiMinX;
							num23 = num19 + Engine.m_MultiMinY;
							num24 = num18 + Engine.m_MultiMaxX;
							num25 = num19 + Engine.m_MultiMaxY;
						}
						else
						{
							flag5 = false;
						}
					}
					else
					{
						flag5 = false;
					}
				}
				else if ((Control.ModifierKeys & (Keys.Shift | Keys.Control)) == (Keys.Shift | Keys.Control) && ((Gumps.LastOver is GSpellIcon && ((GSpellIcon)Gumps.LastOver).m_SpellID == 57) || (Gumps.LastOver is GContainerItem && ((GContainerItem)Gumps.LastOver).Item.ID == 8037)))
				{
					int num31 = 1 + (int)(Engine.Skills[SkillName.Magery].Value / 15f);
					num30 = 16737894;
					num26 = player.X - num4 - num31;
					num27 = player.Y - num5 - num31;
					num28 = player.X - num4 + num31;
					num29 = player.Y - num5 + num31;
				}
				else if (active is ServerTargetHandler)
				{
					ServerTargetHandler serverTargetHandler = active as ServerTargetHandler;
					int num32 = 0;
					int num33 = -1;
					bool flag7 = false;
					if (serverTargetHandler.Action == TargetAction.MeteorSwarm || serverTargetHandler.Action == TargetAction.ChainLightning)
					{
						num32 = 16737894;
						num33 = 2;
					}
					else if (serverTargetHandler.Action == TargetAction.MassCurse)
					{
						num32 = 16737894;
						num33 = 3;
					}
					else if (serverTargetHandler.Action == TargetAction.MassDispel)
					{
						num32 = 16737894;
						num33 = 8;
					}
					else if (serverTargetHandler.Action == TargetAction.Reveal)
					{
						num32 = 10079487;
						num33 = 1 + (int)(Engine.Skills[SkillName.Magery].Value / 20f);
					}
					else if (serverTargetHandler.Action == TargetAction.DetectHidden)
					{
						num32 = 10079487;
						num33 = (int)(Engine.Skills[SkillName.DetectingHidden].Value / 10f);
					}
					else if (serverTargetHandler.Action == TargetAction.ArchProtection)
					{
						num32 = 10079487;
						num33 = (Engine.ServerFeatures.AOS ? 2 : 3);
					}
					else if (serverTargetHandler.Action == TargetAction.ArchCure)
					{
						num32 = 10079487;
						num33 = 3;
					}
					else if (serverTargetHandler.Action == TargetAction.WallOfStone)
					{
						num32 = 16737894;
						num33 = 1;
						flag7 = true;
					}
					else if (serverTargetHandler.Action == TargetAction.EnergyField || serverTargetHandler.Action == TargetAction.FireField || serverTargetHandler.Action == TargetAction.ParalyzeField || serverTargetHandler.Action == TargetAction.PoisonField)
					{
						num32 = 16737894;
						num33 = 2;
						flag7 = true;
					}
					if (num32 != 0 && Gumps.IsWorldAt(Engine.m_xMouse, Engine.m_yMouse, CheckDrag: true))
					{
						int TileX2 = 0;
						int TileY2 = 0;
						ICell cell4 = Renderer.FindTileFromXY(Engine.m_xMouse, Engine.m_yMouse, ref TileX2, ref TileY2, onlyMobs: false);
						if (cell4 != null && (cell4.CellType == Renderer.tLandTile || cell4.CellType == Renderer.tStaticItem || cell4.CellType == Renderer.tMobileCell || cell4.CellType == Renderer.tDynamicItem))
						{
							num30 = num32;
							if (num33 >= 0)
							{
								if (flag7)
								{
									int num34 = player.X - TileX2;
									int num35 = player.Y - TileY2;
									int num36 = num34 - num35;
									int num37 = num34 + num35;
									if ((num36 < 0 || num37 < 0) && (num36 >= 0 || num37 >= 0))
									{
										num26 = TileX2 - num4 - num33;
										num28 = TileX2 - num4 + num33;
										num27 = TileY2 - num5;
										num29 = TileY2 - num5;
									}
									else
									{
										num26 = TileX2 - num4;
										num28 = TileX2 - num4;
										num27 = TileY2 - num5 - num33;
										num29 = TileY2 - num5 + num33;
									}
								}
								else
								{
									num26 = TileX2 - num4 - num33;
									num27 = TileY2 - num5 - num33;
									num28 = TileX2 - num4 + num33;
									num29 = TileY2 - num5 + num33;
								}
							}
						}
					}
				}
				DesignContext current = DesignContext.Current;
				bool flag8 = false;
				int x = 0;
				int y = 0;
				int x2 = 0;
				int y2 = 0;
				if (current != null)
				{
					DesignerEntry entry = current.Entry;
					if (entry != null && entry.GetMultiCursor() == null)
					{
						entry.FillCursor(Renderer.m_DesignerIDs);
						flag8 = true;
						int x3 = Engine.m_xMouse;
						int y3 = Engine.m_yMouse;
						bool flag9 = current.ComputeTilePosition(ref x3, ref y3);
						if (current.Dragging)
						{
							x = current.DragStartX;
							y = current.DragStartY;
						}
						else
						{
							x = x3;
							y = y3;
							flag8 = flag9;
						}
						x2 = x3;
						y2 = y3;
						if (x2 < x)
						{
							x3 = x2;
							x2 = x;
							x = x3;
						}
						if (y2 < y)
						{
							y3 = y2;
							y2 = y;
							y = y3;
						}
						current.NormalizeToCell(ref x, ref y, num4, num5);
						current.NormalizeToCell(ref x2, ref y2, num4, num5);
					}
				}
				StaticItem staticItem = null;
				Item item = null;
				bool xDouble = false;
				int size = ((Renderer.cellWidth < Renderer.cellHeight) ? (Renderer.cellWidth - 1) : (Renderer.cellHeight - 1));
				TerrainMeshProvider current2 = TerrainMeshProviders.Current;
				MapSubgroup[] mapSubgroups = Renderer.GetMapSubgroups(size);
				for (int num38 = 0; num38 < mapSubgroups.Length; num38 = num12)
				{
					MapSubgroup mapSubgroup = mapSubgroups[num38];
					int x4 = mapSubgroup.x;
					int y4 = mapSubgroup.y;
					bool flag10 = false;
					int num39 = (x4 - y4) * 22 + num10;
					int num40 = (x4 + y4) * 22 + num11;
					Stopwatch stopwatch = null;
					int count2 = cells[x4, y4].Count;
					for (int i = 0; i < count2; i++)
					{
						if (stopwatch != null)
						{
							stopwatch.Stop();
							stopwatch = null;
						}
						ICell cell5 = cells[x4, y4][i];
						Type cellType3 = cell5.CellType;
						if (mapSubgroup.ground == flag10)
						{
							if (cellType3 == Renderer.tLandTile)
							{
								flag10 = true;
							}
							continue;
						}
						if (cellType3 == Renderer.tLandTile)
						{
							flag10 = true;
						}
						if (cellType3 == Renderer.tStaticItem || cellType3 == Renderer.tDynamicItem)
						{
							bool flag11 = cellType3 == Renderer.tStaticItem;
							bool flag12 = !flag11;
							IItem item2 = (IItem)cell5;
							int z;
							TileFlags tileFlags;
							IHue hue2;
							int num41;
							if (flag11)
							{
								staticItem = (StaticItem)item2;
								num41 = staticItem.m_ID;
								z = staticItem.m_Z;
								tileFlags = Map.m_ItemFlags[num41];
								if (num13 < int.MaxValue && (z >= num13 || tileFlags[TileFlag.Roof]))
								{
									continue;
								}
								hue2 = staticItem.m_Hue;
								xDouble = false;
							}
							else
							{
								DynamicItem dynamicItem = (DynamicItem)item2;
								num41 = dynamicItem.m_ID;
								z = dynamicItem.m_Z;
								tileFlags = Map.m_ItemFlags[num41];
								if (num13 < int.MaxValue && (z >= num13 || tileFlags[TileFlag.Roof]))
								{
									continue;
								}
								hue2 = dynamicItem.m_Hue;
								item = dynamicItem.m_Item;
								num41 = Map.GetDispID(num41, (ushort)item.Amount, ref xDouble);
							}
							if (tileFlags[TileFlag.Internal])
							{
								continue;
							}
							bool flag13 = false;
							int color = 16777215;
							if (num30 != 0 && x4 >= num26 && y4 >= num27 && x4 <= num28 && y4 <= num29)
							{
								hue2 = Hues.Grayscale;
								color = num30;
								flag13 = true;
							}
							if (itemShadows && ShadowManager.HasShadow(num41))
							{
								Texture item3 = Hues.Shadow.GetItem(num41);
								if (item3 != null && !item3.IsEmpty())
								{
									int x5 = num39 + 22 - (item3.Width >> 1);
									int y5 = num40 - (z << 2) + 43 - item3.Height;
									Renderer.PushAlpha(0.5f);
									item3.DrawShadow(x5, y5, item3.Width / 2, item3.Height - 8);
									Renderer.PopAlpha();
								}
							}
							if (flag4 && flag11 && !flag13)
							{
								StaticItem staticItem2 = staticItem;
								if (staticItem2.m_bInit)
								{
									if (!staticItem2.m_bDraw)
									{
										continue;
									}
									Renderer.PushAlpha(staticItem2.m_fAlpha);
									Renderer.SetTexture(staticItem2.m_sDraw);
									fixed (TransformedColoredTextured* vPool = staticItem2.m_vPool)
									{
										Renderer.PushQuad(vPool);
									}
									Renderer.PopAlpha();
									continue;
								}
							}
							if (!flag11 && item != null && item.ID == 8198)
							{
								if (item.CorpseSerial != 0)
								{
									continue;
								}
								int num42 = item.Amount;
								GraphicTranslation graphicTranslation = GraphicTranslators.Corpse[num42];
								if (graphicTranslation != null)
								{
									num42 = graphicTranslation.FallbackId;
									hue2 = Hues.Load(graphicTranslation.FallbackData ^ 0x8000);
								}
								int animDirection = Engine.GetAnimDirection(item.Direction);
								int actionID = Engine.m_Animations.ConvertAction(num42, item.Serial, item.X, item.Y, animDirection, GenericAction.Die, null);
								int frameCount = Engine.m_Animations.GetFrameCount(num42, actionID, animDirection);
								int xCenter = num39 + 23;
								int yCenter = num40 - (z << 2) + 20;
								IHue h = ((!flag) ? hue2 : hue);
								int TextureX = 0;
								int TextureY = 0;
								Frame frame = Engine.m_Animations.GetFrame(item, num42, actionID, animDirection, frameCount - 1, xCenter, yCenter, h, ref TextureX, ref TextureY, flag);
								item.DrawGame(frame.Image, TextureX, TextureY, color);
								item.MessageX = TextureX + frame.Image.xMin + (frame.Image.xMax - frame.Image.xMin) / 2;
								item.MessageY = TextureY;
								item.BottomY = TextureY + frame.Image.yMax;
								item.MessageFrame = Renderer.m_ActFrames;
								List<Item> sortedCorpseItems = item.GetSortedCorpseItems();
								for (int num43 = 0; num43 < sortedCorpseItems.Count; num43 = num12)
								{
									Item item4 = sortedCorpseItems[num43];
									if (item4.Parent == item)
									{
										if (!flag)
										{
											h = Hues.GetItemHue(item4.ID, item4.Hue);
										}
										frame = Engine.m_Animations.GetFrame(item4, item4.AnimationId, actionID, animDirection, frameCount - 1, xCenter, yCenter, h, ref TextureX, ref TextureY, flag);
										item4.DrawGame(frame.Image, TextureX, TextureY, color);
									}
									num12 = num43 + 1;
								}
								continue;
							}
							float num44 = 1f;
							int itemID = num41;
							IHue hue3;
							if (flag)
							{
								hue3 = hue;
							}
							else if (flag11)
							{
								hue3 = hue2;
							}
							else if (item != null && item.Flags[ItemFlag.Hidden])
							{
								hue3 = Hues.Grayscale;
								num44 = 0.6f;
							}
							else
							{
								hue3 = hue2;
							}
							AnimData anim = Map.GetAnim(itemID);
							int num45 = num41;
							if (anim.frameCount != 0 && tileFlags[TileFlag.Animation])
							{
								num45 += anim[Renderer.m_Frames / (anim.frameInterval + 1) % anim.frameCount];
							}
							Texture item5 = item2.GetItem(hue3, (ushort)num45);
							if (item5 == null || item5.IsEmpty())
							{
								continue;
							}
							int num46 = num39 + 22 - (item5.Width >> 1);
							int num47 = num40 - (z << 2) + 43 - item5.Height;
							if (flag3 && tileFlags[TileFlag.Foliage])
							{
								if (new System.Drawing.Rectangle(num46 + item5.xMin, num47 + item5.yMin, item5.xMax, item5.yMax).IntersectsWith(Renderer.m_FoliageCheck))
								{
									num44 *= 0.4f;
								}
							}
							else if (tileFlags[TileFlag.Translucent])
							{
								num44 *= 0.9f;
							}
							if (flag12)
							{
								if (item != null)
								{
									Renderer.PushAlpha(num44);
									item.DrawGame(item5, num46, num47, color);
									if (xDouble)
									{
										item.DrawGame(item5, num46 + 5, num47 + 5, color);
									}
									Renderer.PopAlpha();
									if (Renderer.m_Transparency)
									{
										queue3.Enqueue(TransparentDraw.PoolInstance(item5, num46, num47, num44, xDouble));
									}
									item.MessageX = num46 + item5.xMin + (item5.xMax - item5.xMin) / 2;
									item.MessageY = num47;
									item.BottomY = num47 + item5.yMax;
									item.MessageFrame = Renderer.m_ActFrames;
								}
								else
								{
									Renderer.PushAlpha(num44);
									item5.DrawGame(num46, num47, color);
									if (xDouble)
									{
										item5.DrawGame(num46 + 5, num47 + 5, color);
									}
									Renderer.PopAlpha();
								}
							}
							else
							{
								Renderer.PushAlpha(num44);
								if (tileFlags[TileFlag.Animation])
								{
									item5.DrawGame(num46, num47, color, staticItem.m_vPool);
								}
								else
								{
									staticItem.m_bDraw = item5.DrawGame(num46, num47, color, staticItem.m_vPool);
									staticItem.m_fAlpha = num44;
									staticItem.m_bInit = !flag13;
									staticItem.m_sDraw = item5;
								}
								Renderer.PopAlpha();
							}
						}
						else if (cellType3 == Renderer.tLandTile)
						{
							LandTile landTile = (LandTile)cell5;
							int z2 = landTile.m_Z;
							if (landTile.m_ID == 2)
							{
								continue;
							}
							int num48 = num39;
							int y6 = num40;
							if (num48 < Engine.GameX + Engine.GameWidth && num48 + 44 > Engine.GameX && (num14 >= int.MaxValue || z2 < num14))
							{
								IHue hue4 = hue;
								int baseColor = 16777215;
								if (num30 != 0 && x4 >= num26 && y4 >= num27 && x4 <= num28 && y4 <= num29)
								{
									hue4 = Hues.Grayscale;
									baseColor = num30;
								}
								landTile.EnsureMesh(hue4, current2);
								if (landTile._mesh != null)
								{
									Renderer.SetTexture(landTile.m_sDraw);
									landTile._mesh.Update(landTile, baseColor);
									landTile._mesh.Render(num48, y6);
								}
							}
						}
						else
						{
							if (!(cellType3 == Renderer.tMobileCell) || (num13 < int.MaxValue && cell5.Z >= num13))
							{
								continue;
							}
							IAnimatedCell animatedCell = (IAnimatedCell)cell5;
							int Body = 0;
							int Direction = 0;
							int Hue = 0;
							int Action = 0;
							int Frame = 0;
							Mobile mobile = ((MobileCell)cell5).m_Mobile;
							int fOffset2 = 0;
							int xOffset2 = 0;
							int yOffset2 = 0;
							if (mobile.Player)
							{
								flag3 = true;
							}
							if (mobile.Walking.Count > 0)
							{
								WalkAnimation walkAnimation2 = mobile.Walking.Peek();
								if (!walkAnimation2.Snapshot(ref xOffset2, ref yOffset2, ref fOffset2))
								{
									if (!walkAnimation2.Advance)
									{
										xOffset2 = walkAnimation2.xOffset;
										yOffset2 = walkAnimation2.yOffset;
									}
									else
									{
										xOffset2 = 0;
										yOffset2 = 0;
									}
									fOffset2 = walkAnimation2.Frames;
									mobile.SetLocation(walkAnimation2.NewX, walkAnimation2.NewY, walkAnimation2.NewZ);
									mobile.Walking.Dequeue().Dispose();
									if (mobile.Player)
									{
										if (Engine.amMoving)
										{
											Engine.DoWalk(Engine.movingDir, fromRenderer: true);
										}
										Renderer.eOffsetX += xOffset2;
										Renderer.eOffsetY += yOffset2;
									}
									if (mobile.Walking.Count == 0)
									{
										mobile.Direction = (byte)walkAnimation2.NewDir;
										mobile.IsMoving = false;
										mobile.MovedTiles = 0;
										mobile.HorseFootsteps = 0;
									}
									else
									{
										mobile.Walking.Peek().Start();
									}
									queue.Enqueue(mobile);
								}
							}
							List<Item> sortedItems = mobile.GetSortedItems();
							animatedCell.GetPackage(ref Body, ref Action, ref Direction, ref Frame, ref Hue);
							int num49 = Frame;
							int frameCount2 = Engine.m_Animations.GetFrameCount(Body, Action, Direction);
							if (frameCount2 == 0)
							{
								continue;
							}
							num49 %= frameCount2;
							int num50 = num39 + 22;
							int num51 = num40 - (animatedCell.Z << 2) + 22;
							num12 = num50 + 1;
							num50 = num12;
							num51 -= 2;
							num50 += xOffset2;
							num51 += yOffset2;
							if (fOffset2 != 0)
							{
								num49 += fOffset2;
								num49 %= frameCount2;
								Frame += fOffset2;
								Frame %= frameCount2;
							}
							if (mobile.Human && mobile.IsMoving && mobile.LastFrame != Frame)
							{
								int? num52 = null;
								if (mobile.IsMounted && (Direction & 0x80) != 0)
								{
									switch (Frame)
									{
									case 1:
										num52 = 297;
										break;
									case 3:
										num52 = 298;
										break;
									}
								}
								else
								{
									switch (Frame)
									{
									case 1:
										num52 = 299;
										break;
									case 6:
										num52 = 300;
										break;
									}
								}
								if (num52.HasValue && !Preferences.Current.Footsteps.Volume.IsMuted)
								{
									float volume = 0.67499995f;
									float frequency = (float)((Engine.Random.NextDouble() * 2.0 - 1.0) / 14.0);
									if (mobile.Player)
									{
										Engine.Sounds.PlaySound(num52.Value, -1, -1, -1, volume, frequency);
									}
									else
									{
										Engine.Sounds.PlaySound(num52.Value, mobile.X, mobile.Y, mobile.Z, volume, frequency);
									}
								}
								mobile.LastFrame = Frame;
							}
							bool flag14 = false;
							bool flag15 = false;
							IHue hue5 = null;
							float num53 = 1f;
							int color2 = 16777215;
							IHue hue6;
							if (flag)
							{
								hue6 = hue;
								flag15 = true;
								hue5 = hue6;
							}
							else if (num30 != 0 && x4 >= num26 && y4 >= num27 && x4 <= num28 && y4 <= num29)
							{
								hue6 = Hues.Grayscale;
								color2 = num30;
								flag15 = true;
								hue5 = hue6;
								flag14 = false;
							}
							else if ((mobile.Flags.Value & -224) != 0)
							{
								hue6 = Hues.Load(33925);
								flag15 = true;
								hue5 = hue6;
								flag14 = false;
							}
							else if (mobile.Flags[MobileFlag.Hidden])
							{
								hue6 = Hues.Grayscale;
								flag15 = true;
								hue5 = hue6;
							}
							else if ((Engine.m_Highlight == mobile || Renderer.m_AlwaysHighlight == mobile.Serial) && !mobile.Player)
							{
								hue6 = Hues.GetNotoriety(mobile.Notoriety);
								flag15 = true;
								hue5 = hue6;
								flag14 = true;
							}
							else if (mobile.IsDeadPet)
							{
								hue6 = Hues.Grayscale;
								flag15 = true;
								hue5 = hue6;
								flag14 = true;
							}
							else
							{
								hue6 = Hues.Load(Hue);
							}
							int TextureX2 = 0;
							int TextureY2 = 0;
							Frame frame2;
							try
							{
								frame2 = Engine.m_Animations.GetFrame(mobile, Body, Action, Direction, num49, num50, num51, hue6, ref TextureX2, ref TextureY2, flag15);
							}
							catch
							{
								frame2 = Engine.m_Animations.GetFrame(mobile, Body, Action, Direction, num49, num50, num51, hue6, ref TextureX2, ref TextureY2, flag15);
							}
							bool flag16 = false;
							float alpha = 1f;
							int x6 = -1;
							int y7 = -1;
							Texture t = null;
							if (frame2.Image != null && !frame2.Image.IsEmpty())
							{
								mobile.MessageFrame = Renderer.m_ActFrames;
								int screenX = (mobile.MessageX = num50);
								mobile.ScreenX = screenX;
								mobile.ScreenY = num51;
								mobile.MessageY = TextureY2;
								if (mobile.Player)
								{
									Renderer.m_FoliageCheck = new System.Drawing.Rectangle(TextureX2, TextureY2, frame2.Image.Width, frame2.Image.Height);
								}
								if (flag2 && !mobile.Player && (int)mobile.Notoriety >= 1 && (int)mobile.Notoriety <= 7 && notorietyHalos)
								{
									Texture playerHalo = Engine.ImageCache.PlayerHalo;
									playerHalo.DrawGame(num50 - (playerHalo.Width >> 1), num51 - (playerHalo.Height >> 1), Engine.C16232(Hues.GetNotorietyData(mobile.Notoriety).colors[47]));
								}
								if (characterShadows && !mobile.IsDead)
								{
									int TextureX3 = 0;
									int TextureY3 = 0;
									Renderer.PushAlpha(0.5f);
									Frame frame3 = Engine.m_Animations.GetFrame(mobile, Body, Action, Direction, num49, num50, num51, Hues.Shadow, ref TextureX3, ref TextureY3, preserveHue: false);
									if (frame3.Image != null && !frame3.Image.IsEmpty())
									{
										frame3.Image.DrawShadow(TextureX3, TextureY3, num50 - TextureX3, num51 - TextureY3);
									}
									if (mobile.HumanOrGhost)
									{
										bool isMounted = mobile.IsMounted;
										for (int num55 = 0; num55 < sortedItems.Count; num12 = num55 + 1, num55 = num12)
										{
											Item item6 = sortedItems[num55];
											if (item6.Layer != Layer.Mount && item6.Layer != Layer.OneHanded && item6.Layer != Layer.TwoHanded)
											{
												continue;
											}
											int animationId = item6.AnimationId;
											int num56 = Action;
											int num57 = Frame;
											if (item6.Layer == Layer.Mount)
											{
												if (mobile.IsMoving)
												{
													num56 = (((Direction & 0x80) != 0) ? 1 : 0);
												}
												else if (mobile.Animation == null)
												{
													num56 = 2;
												}
												else if (num56 == 23)
												{
													num56 = 0;
												}
												else if (num56 == 24)
												{
													num56 = 1;
												}
												else
												{
													if (num56 < 25 || num56 > 29)
													{
														continue;
													}
													num56 = 2;
												}
											}
											else if (isMounted)
											{
												if (mobile.IsMoving)
												{
													num56 = 23 + ((Direction & 0x80) >> 7);
												}
												else if (mobile.Animation == null)
												{
													num56 = 25;
												}
											}
											int hue7 = item6.Hue;
											if (item6.Layer == Layer.Mount)
											{
												int bodyID = animationId;
												Engine.m_Animations.Translate(ref bodyID, ref hue7);
											}
											int frameCount3 = Engine.m_Animations.GetFrameCount(animationId, num56, Direction);
											num57 = ((frameCount3 != 0) ? (num57 % frameCount3) : 0);
											frame3 = Engine.m_Animations.GetFrame(item6, animationId, num56, Direction, num57, num50, num51, Hues.Shadow, ref TextureX3, ref TextureY3, preserveHue: false);
											if (frame3.Image != null && !frame3.Image.IsEmpty())
											{
												frame3.Image.DrawShadow(TextureX3, TextureY3, num50 - TextureX3, num51 - TextureY3);
											}
										}
									}
									Renderer.PopAlpha();
								}
								if (mobile.Flags[MobileFlag.Hidden] || Body == 970 || mobile.IsDeadPet || mobile.Ghost)
								{
									num53 = 0.5f;
								}
								Renderer.PushAlpha(num53);
								if (mobile.HumanOrGhost && mobile.IsMounted)
								{
									alpha = Renderer._alphaValue;
									t = frame2.Image;
									x6 = TextureX2;
									y7 = TextureY2;
									flag16 = true;
								}
								else
								{
									flag16 = false;
								}
								if (!mobile.Ghost && !flag16)
								{
									mobile.DrawGame(frame2.Image, TextureX2, TextureY2, color2);
								}
								Renderer.PopAlpha();
							}
							int height;
							int num58 = num40 - (cell5.Z << 2) + 18 - (height = Engine.m_Animations.GetHeight(Body, Action, Direction));
							int num59 = num39 + 22;
							num59 += xOffset2;
							num58 += yOffset2;
							if (Options.Current.MiniHealth && mobile.OpenedStatus && !mobile.Ghost && mobile.MaximumHitPoints > 0)
							{
								queue2.Enqueue(MiniHealthEntry.PoolInstance(num59, num58 + 4 + height, mobile));
							}
							if (mobile.HumanOrGhost)
							{
								bool isMounted2 = mobile.IsMounted;
								for (int num60 = 0; num60 < sortedItems.Count; num12 = num60 + 1, num60 = num12)
								{
									Item item7 = sortedItems[num60];
									if (mobile.Ghost && item7.Layer != Layer.OuterTorso)
									{
										continue;
									}
									if (mobile.HasEquip(mobile.FindEquip(new ItemIDValidator(9860, 9863))))
									{
										Layer layer = item7.Layer;
										if ((int)layer <= 11)
										{
											if (layer == Layer.Helm || (int)(layer - 10) <= 1)
											{
												goto IL_1f3f;
											}
										}
										else if (layer == Layer.InnerTorso || layer == Layer.MiddleTorso || layer == Layer.Arms)
										{
											goto IL_1f3f;
										}
									}
									else if (mobile.HasEquip(mobile.FindEquip(new ItemIDValidator(7939))))
									{
										switch (item7.Layer)
										{
										case Layer.Neck:
										case Layer.InnerTorso:
										case Layer.MiddleTorso:
										case Layer.Arms:
											if (item7 == mobile.FindEquip(new ItemIDValidator(item7.ID)))
											{
												continue;
											}
											break;
										}
									}
									goto IL_1fd7;
									IL_1f3f:
									if (item7 == mobile.FindEquip(new ItemIDValidator(item7.ID)))
									{
										continue;
									}
									goto IL_1fd7;
									IL_20a9:
									if (item7.Layer == Layer.Mount && flag16)
									{
										Renderer.PushAlpha(alpha);
										mobile.DrawGame(t, x6, y7, color2);
										Renderer.PopAlpha();
									}
									continue;
									IL_1fd7:
									int num61 = item7.AnimationId;
									int num62 = Action;
									int num63 = Frame;
									if (mobile.Ghost)
									{
										num61 = 970;
									}
									if (mobile.IsSitting)
									{
										continue;
									}
									if (item7.Layer == Layer.Mount)
									{
										if (mobile.IsMoving)
										{
											num62 = (((Direction & 0x80) != 0) ? 1 : 0);
										}
										else if (mobile.Animation == null)
										{
											num62 = 2;
										}
										else if (num62 == 23)
										{
											num62 = 0;
										}
										else if (num62 == 24)
										{
											num62 = 1;
										}
										else
										{
											if (num62 < 25 || num62 > 29)
											{
												goto IL_20a9;
											}
											num62 = 2;
										}
									}
									else if (isMounted2)
									{
										if (mobile.IsMoving)
										{
											num62 = 23 + ((Direction & 0x80) >> 7);
										}
										else if (mobile.Animation == null)
										{
											num62 = 25;
										}
									}
									float num64 = num53;
									int hue8 = item7.Hue;
									if (item7.Layer == Layer.Mount)
									{
										int bodyID2 = num61;
										Engine.m_Animations.Translate(ref bodyID2, ref hue8);
									}
									bool preserveHue;
									if (!flag15 || (item7.Layer == Layer.Mount && flag14))
									{
										hue6 = Hues.GetItemHue(item7.ID, item7.Hue);
										preserveHue = false;
									}
									else
									{
										hue6 = hue5;
										preserveHue = flag15;
									}
									int frameCount4 = Engine.m_Animations.GetFrameCount(num61, num62, Direction);
									num63 = ((frameCount4 != 0) ? (num63 % frameCount4) : 0);
									frame2 = Engine.m_Animations.GetFrame(item7, num61, num62, Direction, num63, num50, num51, hue6, ref TextureX2, ref TextureY2, preserveHue);
									if (frame2.Image != null && !frame2.Image.IsEmpty())
									{
										Renderer.PushAlpha(mobile.Ghost ? 0.5f : num64);
										item7.DrawGame(frame2.Image, TextureX2, TextureY2, color2);
										Renderer.PopAlpha();
									}
									goto IL_20a9;
								}
							}
							if (flag6)
							{
								int num65 = -1;
								if (mobile == TargetManager.LastOffensiveTarget)
								{
									num65 = 16720384;
								}
								else if (mobile == TargetManager.LastDefensiveTarget)
								{
									num65 = 2285055;
								}
								else if (mobile == TargetManager.LastTarget)
								{
									num65 = 13421772;
								}
								if (num65 != -1)
								{
									Renderer.DrawPlayerIcon(num50, num51, Engine.ImageCache.LastTargetIcon, num65);
								}
							}
							if (notorietyHalos)
							{
								if (mobile.IsFriend)
								{
									Renderer.DrawPlayerIcon(num50, TextureY2 - 10, Engine.ImageCache.PlayerAlly, 16777215);
								}
								else if (!mobile.Player && player.Warmode && mobile.Visible && mobile.Human && !mobile.IsDead && TargetManager.IsAcquirable(player, mobile))
								{
									Renderer.DrawPlayerIcon(num50, TextureY2 - 10, Engine.ImageCache.PlayerEnemy, 16777215);
								}
							}
						}
					}
					stopwatch?.Stop();
					if (flag8)
					{
						int num66 = num39 + 22;
						int num67 = num40 + 43;
						if (Renderer.m_vMultiPool == null)
						{
							Renderer.m_vMultiPool = VertexConstructor.Create();
						}
						if (x4 >= x && y4 >= y && x4 <= x2 && y4 <= y2)
						{
							int num68 = x2 - x + 1;
							int num69 = y2 - y + 1;
							int num72;
							if (num68 >= 2 && num69 >= 2)
							{
								int num70 = x4 - x;
								int num71 = y4 - y;
								bool flag17 = num71 == 0;
								bool flag18 = num70 == 0;
								bool flag19 = num71 == num69 - 1;
								bool flag20 = num70 == num68 - 1;
								num72 = ((flag17 && flag18) ? 1 : ((flag17 && flag20) ? 3 : ((flag19 && flag20) ? 5 : ((flag19 && flag18) ? 7 : (flag17 ? 2 : (flag20 ? 4 : (flag19 ? 6 : (flag18 ? 8 : 0))))))));
							}
							else
							{
								num72 = 0;
							}
							int displayID = Renderer.m_DesignerIDs[num72].DisplayID;
							Texture item8 = Hues.Default.GetItem(displayID);
							if (item8 != null && !item8.IsEmpty())
							{
								item8.DrawGame(num66 - (item8.Width >> 1), num67 - (player.Z << 2) - item8.Height);
							}
						}
					}
					if (flag5 && x4 >= num22 && x4 <= num24 && y4 >= num23 && y4 <= num25)
					{
						int num73 = num39 + 22;
						int num74 = num40 + 43;
						if (Renderer.m_vMultiPool == null)
						{
							Renderer.m_vMultiPool = VertexConstructor.Create();
						}
						for (int j = 0; j < num21; j++)
						{
							MultiItem multiItem = Engine.m_MultiList[j];
							if (multiItem.X == x4 - num18 && multiItem.Y == y4 - num19 && multiItem.Z == 0)
							{
								int itemID2 = multiItem.ItemID;
								AnimData anim2 = Map.GetAnim(itemID2);
								Texture texture = ((anim2.frameCount != 0 && Map.m_ItemFlags[itemID2][TileFlag.Animation]) ? hue.GetItem(multiItem.ItemID + (ushort)anim2[Renderer.m_Frames / (anim2.frameInterval + 1) % anim2.frameCount]) : hue.GetItem(multiItem.ItemID));
								if (texture != null && !texture.IsEmpty())
								{
									texture.DrawGame(num73 - (texture.Width >> 1), num74 - (num20 + multiItem.Z << 2) - texture.Height);
								}
							}
							else if (multiItem.X + num18 > x4 || (multiItem.X + num18 == x4 && multiItem.Y + num19 >= y4))
							{
								break;
							}
						}
					}
					for (int num75 = 0; num75 < list2.Count; num75 = num12)
					{
						DrawQueueEntry drawQueueEntry = list2[num75];
						if (drawQueueEntry.m_TileX == x4 && drawQueueEntry.m_TileY == y4)
						{
							list2.RemoveAt(num75);
							num12 = num75 - 1;
							num75 = num12;
							drawQueueEntry.m_Texture.Flip = drawQueueEntry.m_Flip;
							Clipper clipper = new Clipper(Engine.GameX, num40 - 46, Engine.GameWidth, Engine.GameHeight - num40 + 46);
							Renderer.PushAlpha(drawQueueEntry.m_fAlpha);
							drawQueueEntry.m_Texture.DrawClipped(drawQueueEntry.m_DrawX, drawQueueEntry.m_DrawY, clipper);
							Renderer.PopAlpha();
						}
						num12 = num75 + 1;
					}
					num12 = num38 + 1;
				}
				if (Renderer.m_Transparency)
				{
					if (Renderer.m_vTransDrawPool == null)
					{
						Renderer.m_vTransDrawPool = VertexConstructor.Create();
					}
					while (queue3.Count > 0)
					{
						TransparentDraw transparentDraw = queue3.Dequeue();
						Renderer.PushAlpha(transparentDraw.m_fAlpha * 0.5f);
						transparentDraw.m_Texture.DrawGame(transparentDraw.m_X, transparentDraw.m_Y);
						if (transparentDraw.m_Double)
						{
							transparentDraw.m_Texture.DrawGame(transparentDraw.m_X + 5, transparentDraw.m_Y + 5);
						}
						Renderer.PopAlpha();
						transparentDraw.Dispose();
					}
				}
				Renderer.SetTexture(null);
				while (queue2.Count > 0)
				{
					MiniHealthEntry miniHealthEntry = queue2.Dequeue();
					Mobile mobile2 = miniHealthEntry.m_Mobile;
					Renderer.TransparentRect(0, miniHealthEntry.m_X - 16, miniHealthEntry.m_Y + 8, 32, 7);
					double num76 = (double)mobile2.CurrentHitPoints / (double)mobile2.MaximumHitPoints;
					if (num76 == double.NaN)
					{
						num76 = 0.0;
					}
					else if (num76 < 0.0)
					{
						num76 = 0.0;
					}
					else if (num76 > 1.0)
					{
						num76 = 1.0;
					}
					int num77 = (int)(30.0 * num76 + 0.5);
					MobileFlags flags = mobile2.Flags;
					int color3;
					int color4;
					if (mobile2.IsPoisoned)
					{
						color3 = 65280;
						color4 = 32768;
					}
					else if (flags[MobileFlag.YellowHits])
					{
						color3 = 16760832;
						color4 = 8413184;
					}
					else
					{
						color3 = 2146559;
						color4 = 1073280;
					}
					Renderer.PushAlpha(0.6f);
					Renderer.GradientRect(color3, color4, miniHealthEntry.m_X - 15, miniHealthEntry.m_Y + 9, num77, 5);
					Renderer.GradientRect(6553600, 13107200, miniHealthEntry.m_X - 15 + num77, miniHealthEntry.m_Y + 9, 30 - num77, 5);
					Renderer.PopAlpha();
					miniHealthEntry.Dispose();
				}
				if (Engine.m_Ingame)
				{
					Engine.Effects.Draw();
					if (Renderer.eOffsetX != 0 || Renderer.eOffsetY != 0)
					{
						Engine.Effects.Offset(Renderer.eOffsetX, Renderer.eOffsetY);
					}
				}
				Map.Unlock();
				if (Renderer._profile != null)
				{
					Renderer._profile._worldTime.Stop();
				}
				Renderer.SetViewport(0, 0, Engine.ScreenWidth, Engine.ScreenHeight);
				if (Renderer.lightTexture != null)
				{
					int num78 = Engine.Effects.GlobalLight;
					if (player != null)
					{
						num78 -= player.LightLevel;
					}
					if (num78 < 0)
					{
						num78 = 0;
					}
					else if (num78 > 31)
					{
						num78 = 31;
					}
					if (num78 != 0)
					{
						Renderer.PushAlpha((float)num78 / 31f);
						Renderer.SetBlendType(DrawBlendType.LightSource);
						Renderer.lightTexture.Draw(Engine.GameX, Engine.GameY);
						Renderer.SetBlendType(DrawBlendType.Normal);
						Renderer.PopAlpha();
					}
				}
			}
			if (!Engine.m_Ingame)
			{
				Texture veritasLogo = Engine.ImageCache.VeritasLogo;
				veritasLogo?.Draw((Engine.ScreenWidth - veritasLogo.Width) / 2, (Engine.ScreenHeight - veritasLogo.Height) / 2);
			}
			if (!Engine.m_Loading)
			{
				MessageManager.BeginRender();
				if (Engine.m_Ingame && Renderer.m_TextSurface != null)
				{
					Renderer.SetTexture(null);
					if (player != null && player.OpenedStatus && player.StatusBar == null)
					{
						Renderer.PushAlpha(0.5f);
						Renderer.SolidRect(0, Engine.GameX + 2, Engine.GameY + Engine.GameHeight - 21, Engine.GameWidth - 46, 19);
						Renderer.PopAlpha();
						int num79 = Engine.GameX + Engine.GameWidth - 44;
						int num80 = Engine.GameY + Engine.GameHeight - 21;
						Renderer.SolidRect(0, num79, num80, 42, 19);
						int num12 = num79 + 1;
						num79 = num12;
						num12 = num80 + 1;
						num80 = num12;
						if (player.Ghost)
						{
							Renderer.GradientRect(12632256, 6316128, num79, num80, 40, 5);
							num80 += 6;
							Renderer.GradientRect(12632256, 6316128, num79, num80, 40, 5);
							num80 += 6;
							Renderer.GradientRect(12632256, 6316128, num79, num80, 40, 5);
						}
						else
						{
							int num81 = (int)((double)player.CurrentHitPoints / (double)player.MaximumHitPoints * 40.0);
							if (num81 > 40)
							{
								num81 = 40;
							}
							else if (num81 < 0)
							{
								num81 = 0;
							}
							MobileFlags flags2 = player.Flags;
							int color5;
							int color6;
							if (player.IsPoisoned)
							{
								color5 = 65280;
								color6 = 32768;
							}
							else if (flags2[MobileFlag.YellowHits])
							{
								color5 = 16760832;
								color6 = 8413184;
							}
							else
							{
								color5 = 2146559;
								color6 = 1073280;
							}
							Renderer.GradientRect(color5, color6, num79, num80, num81, 5);
							Renderer.GradientRect(16711680, 8388608, num79 + num81, num80, 40 - num81, 5);
							num80 += 6;
							num81 = (int)((double)player.CurrentMana / (double)player.MaximumMana * 40.0);
							if (num81 > 40)
							{
								num81 = 40;
							}
							else if (num81 < 0)
							{
								num81 = 0;
							}
							Renderer.GradientRect(2146559, 1073280, num79, num80, 40, 5);
							Renderer.GradientRect(16711680, 8388608, num79 + num81, num80, 40 - num81, 5);
							num80 += 6;
							num81 = (int)((double)player.CurrentStamina / (double)player.MaximumStamina * 40.0);
							if (num81 > 40)
							{
								num81 = 40;
							}
							else if (num81 < 0)
							{
								num81 = 0;
							}
							Renderer.GradientRect(2146559, 1073280, num79, num80, 40, 5);
							Renderer.GradientRect(16711680, 8388608, num79 + num81, num80, 40 - num81, 5);
						}
					}
					else
					{
						Renderer.PushAlpha(0.5f);
						Renderer.SolidRect(0, Engine.GameX + 2, Engine.GameY + Engine.GameHeight - 21, Engine.GameWidth - 4, 19);
						Renderer.PopAlpha();
					}
					Renderer.m_vTextCache.Draw(Renderer.m_TextSurface, Engine.GameX + 2, Engine.GameY + Engine.GameHeight - 2 - Renderer.m_TextSurface.Height);
				}
				if (Renderer._profile != null)
				{
					Renderer._profile._gumpTime.Start();
				}
				Gumps.Draw();
				if (Renderer._profile != null)
				{
					Renderer._profile._gumpTime.Stop();
				}
			}
			if (Engine.m_Ingame)
			{
				_ = Engine.GameY;
				_ = Engine.GameHeight;
				if (Renderer.m_TextSurface != null)
				{
					_ = Renderer.m_TextSurface.Height;
				}
				List<System.Drawing.Rectangle> list3 = Renderer.m_RectsList;
				if (list3 == null)
				{
					list3 = (Renderer.m_RectsList = new List<System.Drawing.Rectangle>());
				}
				else if (list3.Count > 0)
				{
					list3.Clear();
				}
				World.DrawAllMessages();
				list.Sort();
				int count3 = list.Count;
				int num82 = 0;
				while (num82 < count3)
				{
					TextMessage textMessage = list[num82];
					int num83 = textMessage.X + Engine.GameX;
					int num84 = textMessage.Y + Engine.GameY;
					if (num83 < Engine.GameX + 2)
					{
						num83 = Engine.GameX + 2;
					}
					else if (num83 + textMessage.Image.Width >= Engine.GameX + Engine.GameWidth - 2)
					{
						num83 = Engine.GameX + Engine.GameWidth - textMessage.Image.Width - 2;
					}
					if (num84 < Engine.GameY + 2)
					{
						num84 = Engine.GameY + 2;
					}
					else if (num84 + textMessage.Image.Height >= Engine.GameY + Engine.GameHeight - 2)
					{
						num84 = Engine.GameY + Engine.GameHeight - textMessage.Image.Height - 2;
					}
					list3.Add(new System.Drawing.Rectangle(num83, num84, textMessage.Image.Width, textMessage.Image.Height));
					int num12 = num82 + 1;
					num82 = num12;
				}
				int num85 = 0;
				while (num85 < count3)
				{
					TextMessage textMessage2 = list[num85];
					System.Drawing.Rectangle rect = list3[num85];
					float num86 = 1f;
					int count4 = list3.Count;
					for (int k = num85 + 1; k < count4; k++)
					{
						if (list3[k].IntersectsWith(rect))
						{
							num86 += list[k].Alpha;
						}
					}
					num86 = 1f / num86;
					if (textMessage2.Disposing)
					{
						num86 *= textMessage2.Alpha;
					}
					Renderer.PushAlpha(num86);
					textMessage2.Draw(rect.X, rect.Y);
					Renderer.PopAlpha();
					int num12 = num85 + 1;
					num85 = num12;
				}
				if (Renderer.eOffsetX != 0 || Renderer.eOffsetY != 0)
				{
					World.Offset(Renderer.eOffsetX, Renderer.eOffsetY);
				}
			}
			if (!Engine.m_Loading)
			{
				Cursor.Draw();
			}
			Renderer.PushAll();
		}
		catch (SharpDXException ex)
		{
			int code = ex.ResultCode.Code;
			if (code == ResultCode.DeviceLost.Code)
			{
				Application.DoEvents();
				Thread.Sleep(10);
			}
			else if (code == ResultCode.DeviceNotReset.Code)
			{
				Engine.ResetDevice();
				GC.Collect();
			}
			else
			{
				Thread.Sleep(10);
				Debug.Error(ex);
			}
		}
		catch (Exception ex2)
		{
			Debug.Trace("Draw Exception:");
			Debug.Error(ex2);
		}
		finally
		{
			Engine.m_Device.EndScene();
		}
		if (Renderer._profile != null)
		{
			Renderer._profile._drawTime.Stop();
		}
		Engine.m_Device.Present();
		Renderer.m_Count = 0;
		if (Renderer.screenshotContext != null)
		{
			Renderer.screenshotContext.Save();
			Renderer.screenshotContext = null;
		}
		Renderer.m_ActFrames++;
		while (queue.Count > 0)
		{
			Mobile mobile3 = queue.Dequeue();
			mobile3.MovedTiles++;
			mobile3.Update();
		}
		Map.Unlock();
	}

	public static void BeginGroup()
	{
		Renderer._drawGroup++;
	}

	public static void EndGroup()
	{
		Renderer._drawGroup--;
		Renderer.m_Count++;
	}

	public static void SetBlendType(DrawBlendType type)
	{
		if (Renderer._blendType != type)
		{
			Renderer._blendType = type;
			Renderer.UpdateAlphaSettings();
		}
	}

	public static void PushAll()
	{
		if (Renderer._profile != null)
		{
			Renderer._profile._pushes++;
			Renderer._profile._pushTime.Start();
		}
		Renderer.PushAll(0, alphaTest: false, filter: false);
		Renderer.PushAll(1, alphaTest: true, filter: false);
		Renderer.PushAll(1, alphaTest: true, filter: true);
		Renderer.PushAll(1, alphaTest: false, filter: false);
		Renderer.PushAll(1, alphaTest: false, filter: true);
		Renderer.PushAll(2, alphaTest: false, filter: false);
		Renderer.PushAll(2, alphaTest: false, filter: true);
		Renderer.PushAll(3, alphaTest: false, filter: false);
		Renderer.PushAll(3, alphaTest: false, filter: true);
		Renderer.PushAlphaStates();
		if (Renderer._profile != null)
		{
			Renderer._profile._pushTime.Stop();
		}
		Renderer._renderCount++;
	}

	private static void EnsureFilterState(bool filterState)
	{
		if (Renderer.m_CurFilter != filterState)
		{
			Renderer.m_CurFilter = filterState;
			TextureFilter textureFilter = ((!filterState) ? TextureFilter.Point : TextureFilter.Linear);
			Engine.m_Device.SetSamplerState(0, SamplerState.MinFilter, textureFilter);
			Engine.m_Device.SetSamplerState(0, SamplerState.MagFilter, textureFilter);
			Engine.m_Device.SetRenderState(RenderState.ShadeMode, ShadeMode.Gouraud);
			Engine.m_Device.SetRenderState(RenderState.MultisampleAntialias, filterState && Preferences.Current.RenderSettings.SmoothingMode != 0);
		}
	}

	private static void PushAlphaStates()
	{
		if (Renderer.m_AlphaStateCount == 0)
		{
			return;
		}
		if (Renderer.m_VertexStream == null)
		{
			Renderer.m_VertexStream = new BufferedVertexStream(Engine.m_VertexBuffer, 32768, TransformedColoredTextured.StrideSize);
		}
		Device device = Engine.m_Device;
		device.SetRenderState(RenderState.ZWriteEnable, enable: false);
		device.SetRenderState(RenderState.AlphaBlendEnable, enable: true);
		for (int i = 0; i < Renderer.m_AlphaStateCount; i++)
		{
			AlphaState alphaState = Renderer.m_AlphaStates[i];
			Texture texture = alphaState.m_Texture;
			TextureVB textureVB = alphaState.m_TextureVB;
			if (textureVB.m_Count <= 0 || textureVB.m_Frame != Renderer._renderCount)
			{
				continue;
			}
			if (Renderer.m_CurAlphaTest)
			{
				Renderer.m_CurAlphaTest = false;
				device.SetRenderState(RenderState.AlphaTestEnable, enable: false);
			}
			Renderer.EnsureFilterState(alphaState.m_Filter);
			if (alphaState.m_BlendType != Renderer.m_CurBlendType)
			{
				Renderer.m_CurBlendType = alphaState.m_BlendType;
				switch (Renderer.m_CurBlendType)
				{
				case DrawBlendType.Normal:
					device.SetRenderState(RenderState.SeparateAlphaBlendEnable, enable: false);
					device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
					device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
					device.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
					break;
				case DrawBlendType.Additive:
					device.SetRenderState(RenderState.SeparateAlphaBlendEnable, enable: false);
					device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
					device.SetRenderState(RenderState.DestinationBlend, Blend.One);
					device.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
					break;
				case DrawBlendType.Subtractive:
					device.SetRenderState(RenderState.SeparateAlphaBlendEnable, enable: true);
					device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
					device.SetRenderState(RenderState.DestinationBlend, Blend.One);
					device.SetRenderState(RenderState.BlendOperation, BlendOperation.ReverseSubtract);
					device.SetRenderState(RenderState.SourceBlendAlpha, Blend.SourceAlpha);
					device.SetRenderState(RenderState.DestinationBlendAlpha, Blend.DestinationAlpha);
					device.SetRenderState(RenderState.BlendOperationAlpha, BlendOperation.Maximum);
					break;
				case DrawBlendType.LightSource:
					device.SetRenderState(RenderState.SeparateAlphaBlendEnable, enable: false);
					device.SetRenderState(RenderState.SourceBlend, Blend.Zero);
					device.SetRenderState(RenderState.DestinationBlend, Blend.SourceColor);
					device.SetRenderState(RenderState.BlendOperation, BlendOperation.ReverseSubtract);
					break;
				case DrawBlendType.BlackTransparency:
					device.SetRenderState(RenderState.SeparateAlphaBlendEnable, enable: false);
					device.SetRenderState(RenderState.SourceBlend, Blend.Zero);
					device.SetRenderState(RenderState.DestinationBlend, Blend.SourceColor);
					device.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
					break;
				}
			}
			int type = alphaState._type;
			ObjectFormat objectFormat = Renderer._formats[type];
			int num = textureVB.m_Count * objectFormat.VertexCount;
			int num2 = Renderer.m_VertexStream.Push(textureVB._buffer, 0, num, unlock: true);
			if (num2 < 0)
			{
				continue;
			}
			Renderer.EnsureTexture(texture);
			IndexBuffer indexBuffer = objectFormat.IndexBuffer;
			if (indexBuffer != null)
			{
				if (Renderer._currentIndexBuffer != indexBuffer)
				{
					Renderer._currentIndexBuffer = indexBuffer;
					device.Indices = indexBuffer;
				}
				device.DrawIndexedPrimitive(objectFormat.Type, num2, 0, num, 0, objectFormat.PrimitiveCount * textureVB.m_Count);
			}
			else
			{
				device.DrawPrimitives(objectFormat.Type, num2, textureVB.m_Count * objectFormat.PrimitiveCount);
			}
			if (Renderer._profile != null)
			{
				Renderer._profile._draws++;
			}
		}
		device.SetRenderState(RenderState.AlphaBlendEnable, enable: false);
		device.SetRenderState(RenderState.ZWriteEnable, enable: true);
		Renderer.m_AlphaStateCount = 0;
	}

	private static void EnsureTexture(Texture tex)
	{
		BaseTexture baseTexture = null;
		BaseTexture baseTexture2 = null;
		PixelShader pixelShader = null;
		if (tex != null)
		{
			baseTexture = tex.m_Surface;
			if (tex._shaderData != null)
			{
				if (tex._shaderData.DataSurface != null)
				{
					baseTexture2 = tex._shaderData.DataSurface.m_Surface;
				}
				pixelShader = tex._shaderData.PixelShader;
			}
		}
		if (baseTexture != Renderer._tex0)
		{
			Renderer._tex0 = baseTexture;
			Engine.m_Device.SetTexture(0, baseTexture);
			if (Renderer._profile != null)
			{
				Renderer._profile._tex0++;
			}
		}
		if (baseTexture2 != null && baseTexture2 != Renderer._tex1)
		{
			Renderer._tex1 = baseTexture2;
			Engine.m_Device.SetTexture(1, baseTexture2);
			if (Renderer._profile != null)
			{
				Renderer._profile._tex1++;
			}
		}
		if (pixelShader != Renderer._psh)
		{
			Renderer._psh = pixelShader;
			Engine.m_Device.PixelShader = pixelShader;
			if (Renderer._profile != null)
			{
				Renderer._profile._psh++;
			}
		}
		if (tex != null && tex._shaderData != null && pixelShader != null && tex._shaderData.RenderCallback != null)
		{
			tex._shaderData.RenderCallback();
		}
	}

	public static IVertexStorage AcquireSolidStorage(int type)
	{
		if (Renderer._profile != null)
		{
			Renderer._profile._acquireTime.Start();
		}
		int num = 0;
		if (Renderer._alphaTestEnable)
		{
			num |= 1;
		}
		if (Renderer._filterEnable)
		{
			num |= 2;
		}
		num |= type << 2;
		Texture texture = Renderer.m_Texture ?? Texture.Empty;
		TextureVB vB = texture.GetVB(type, Renderer._alphaTestEnable, Renderer._filterEnable);
		if (vB.m_Frame != Renderer._renderCount)
		{
			List<Texture> list = Renderer.m_Lists[num];
			if (list == null)
			{
				list = (Renderer.m_Lists[num] = new List<Texture>());
			}
			list.Add(texture);
		}
		if (Renderer._profile != null)
		{
			Renderer._profile._acquireTime.Stop();
		}
		return vB;
	}

	private static IndexBuffer CreateIndexBuffer(int primitiveCount, int vertexCount, int[] indices)
	{
		short[] array = new short[primitiveCount * indices.Length];
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (num3 < primitiveCount)
		{
			int num4 = 0;
			while (num4 < indices.Length)
			{
				array[num] = (short)(num2 + indices[num4]);
				num4++;
				num++;
			}
			num3++;
			num2 += vertexCount;
		}
		IndexBuffer indexBuffer = new IndexBuffer(Engine.m_Device, array.Length * 2, Usage.WriteOnly, Pool.Managed, sixteenBit: true);
		indexBuffer.Lock(0, 0, SharpDX.Direct3D9.LockFlags.None).WriteRange(array);
		indexBuffer.Unlock();
		return indexBuffer;
	}

	private static Surface AcquireBackBuffer()
	{
		return Renderer._backBuffer ?? (Renderer._backBuffer = Renderer.GetBackBuffer());
	}

	private static Surface GetBackBuffer()
	{
		return Engine.m_Device.GetBackBuffer(0, 0);
	}

	public static void Reset()
	{
		Renderer.SetupFormats();
		Renderer._currentIndexBuffer = null;
		Renderer._tex0 = null;
		Renderer._tex1 = null;
		Renderer._psh = null;
		Renderer._backBuffer = null;
		Renderer.lightSurface = null;
		Renderer.m_CurBlendType = (DrawBlendType)(-1);
		Renderer.m_CurAlphaTest = false;
		Renderer.m_CurFilter = false;
	}

	public static void SetupFormats()
	{
		Renderer._formats = new ObjectFormat[4]
		{
			new ObjectFormat(PrimitiveType.LineList, 1, 2, null),
			new ObjectFormat(PrimitiveType.TriangleList, 2, 4, Renderer.CreateIndexBuffer(16384, 4, new int[6] { 3, 1, 2, 2, 1, 0 })),
			null,
			null
		};
		Renderer.SetupTerrainFormats();
	}

	public static void SetupTerrainFormats()
	{
		TerrainMeshProvider current = TerrainMeshProviders.Current;
		Renderer._formats[2] = new ObjectFormat(PrimitiveType.TriangleList, current.Divisions * current.Divisions * 2, current.Size, Renderer.CreateIndexBuffer(4096, current.Size, current.GetIndices(leftRight: true)));
		if (current.Divisions == 1)
		{
			Renderer._formats[3] = new ObjectFormat(PrimitiveType.TriangleList, current.Divisions * current.Divisions * 2, current.Size, Renderer.CreateIndexBuffer(4096, current.Size, current.GetIndices(leftRight: false)));
		}
		else
		{
			Renderer._formats[3] = Renderer._formats[2];
		}
	}

	public static IVertexStorage AcquireAlphaStorage(int type)
	{
		if (Renderer._profile != null)
		{
			Renderer._profile._acquireTime.Start();
		}
		DrawBlendType blendType = Renderer._blendType;
		Texture texture = Renderer.m_Texture ?? Texture.Empty;
		AlphaState alphaState = ((Renderer.m_AlphaStateCount > 0) ? Renderer.m_AlphaStates[Renderer.m_AlphaStateCount - 1] : null);
		if (alphaState == null || alphaState._type != type || alphaState.m_BlendType != blendType || alphaState.m_Texture != texture || alphaState.m_Filter != Renderer._filterEnable)
		{
			if (Renderer.m_AlphaStateCount < Renderer.m_AlphaStates.Count)
			{
				alphaState = Renderer.m_AlphaStates[Renderer.m_AlphaStateCount];
			}
			else
			{
				Renderer.m_AlphaStates.Add(alphaState = new AlphaState());
			}
			alphaState._type = type;
			alphaState.m_BlendType = blendType;
			alphaState.m_Texture = texture;
			alphaState.m_Filter = Renderer._filterEnable;
			Renderer.m_AlphaStateCount++;
		}
		if (Renderer._profile != null)
		{
			Renderer._profile._acquireTime.Stop();
		}
		return alphaState.m_TextureVB;
	}

	private static void PushAll(int type, bool alphaTest, bool filter)
	{
		int num = 0;
		if (alphaTest)
		{
			num |= 1;
		}
		if (filter)
		{
			num |= 2;
		}
		num |= type << 2;
		List<Texture> list = Renderer.m_Lists[num];
		if (list == null || list.Count == 0)
		{
			return;
		}
		Device device = Engine.m_Device;
		if (alphaTest != Renderer.m_CurAlphaTest)
		{
			Renderer.m_CurAlphaTest = alphaTest;
			device.SetRenderState(RenderState.AlphaTestEnable, alphaTest);
		}
		Renderer.EnsureFilterState(filter);
		ObjectFormat objectFormat = Renderer._formats[type];
		if (Renderer.m_VertexStream == null)
		{
			Renderer.m_VertexStream = new BufferedVertexStream(Engine.m_VertexBuffer, 32768, TransformedColoredTextured.StrideSize);
		}
		for (int i = 0; i < list.Count; i++)
		{
			Texture texture = list[i];
			TextureVB vB = texture.GetVB(type, alphaTest, filter);
			if (vB.m_Count <= 0 || vB.m_Frame != Renderer._renderCount)
			{
				continue;
			}
			int num2 = vB.m_Count;
			int num3 = 0;
			while (num2 > 0)
			{
				int num4 = Math.Min(num2, Renderer.m_VertexStream.Length / objectFormat.VertexCount);
				int num5 = Renderer.m_VertexStream.Push(vB._buffer, num3, num4 * objectFormat.VertexCount, unlock: true);
				if (num5 < 0)
				{
					break;
				}
				Renderer.EnsureTexture(texture);
				IndexBuffer indexBuffer = objectFormat.IndexBuffer;
				if (indexBuffer != null)
				{
					if (Renderer._currentIndexBuffer != indexBuffer)
					{
						Renderer._currentIndexBuffer = indexBuffer;
						device.Indices = indexBuffer;
					}
					device.DrawIndexedPrimitive(objectFormat.Type, num5, 0, num4 * objectFormat.VertexCount, 0, num4 * objectFormat.PrimitiveCount);
				}
				else
				{
					device.DrawPrimitives(objectFormat.Type, num5, num4 * objectFormat.PrimitiveCount);
				}
				if (Renderer._profile != null)
				{
					Renderer._profile._draws++;
				}
				vB.m_Frame = -1;
				num3 += num4 * objectFormat.VertexCount;
				num2 -= num4;
			}
		}
		list.Clear();
	}

	static Renderer()
	{
		Renderer.m_Version = 0;
		Renderer.blockWidth = 7;
		Renderer.blockHeight = 7;
		Renderer.cellWidth = Renderer.blockWidth << 3;
		Renderer.cellHeight = Renderer.blockHeight << 3;
		Renderer.m_PointPool = new System.Drawing.Point[4];
		Renderer.m_MousePoint = System.Drawing.Point.Empty;
		Renderer.m_FoliageCheck = new System.Drawing.Rectangle(Engine.ScreenWidth / 2 - 22, Engine.ScreenHeight / 2 - 60, 44, 82);
		Renderer._alphaValue = 1f;
		Renderer._alphaBits = -16777216;
		Renderer._alphaStack = new Stack<float>();
		Renderer.m_CharX = -1024;
		Renderer.m_CharY = -1024;
		Renderer.m_CharZ = -1024;
		Renderer.tLandTile = typeof(LandTile);
		Renderer.tDynamicItem = typeof(DynamicItem);
		Renderer.tStaticItem = typeof(StaticItem);
		Renderer.tMobileCell = typeof(MobileCell);
		Renderer.m_DesignerIDs = new DesignerID[9];
		Renderer.m_Lists = new List<Texture>[16];
		Renderer.m_AlphaStates = new List<AlphaState>();
	}
}
