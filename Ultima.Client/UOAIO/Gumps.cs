using System;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using UOAIO.Assets;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public sealed class Gumps
{
	private class GumpFactory : TextureFactory
	{
		private int m_GumpID;

		private IHue m_Hue;

		private Gumps m_Gumps;

		private byte[] data;

		public override TextureTransparency Transparency => TextureTransparency.Simple;

		public GumpFactory(Gumps gumps)
		{
			this.m_Gumps = gumps;
		}

		public bool Exists(int gumpId)
		{
			return AssetSourceManager.Gumps.Exists(gumpId);
		}

		public Texture Load(int gumpID, IHue hue)
		{
			if (gumpID == 2624)
			{
				hue = Hues.Load(32769);
			}
			this.m_GumpID = gumpID;
			this.m_Hue = hue;
			return base.Construct(isReconstruct: false);
		}

		public override Texture Reconstruct(object[] args)
		{
			this.m_GumpID = (int)args[0];
			this.m_Hue = (IHue)args[1];
			return base.Construct(isReconstruct: true);
		}

		protected override void CoreAssignArgs(Texture tex)
		{
			tex.m_Factory = this;
			tex.m_FactoryArgs = new object[2] { this.m_GumpID, this.m_Hue };
			tex._shaderData = this.m_Hue.ShaderData;
		}

		private static string GetFilePath(int gumpId)
		{
			return $"build/gumpartlegacymul/{gumpId:00000000}.tga";
		}

		protected override bool CoreLookup()
		{
			if (this.m_GumpID == 0)
			{
				this.data = null;
			}
			else
			{
				AssetSourceManager.Gumps.TryRead(this.m_GumpID, out this.data);
			}
			return this.data != null && this.data.Length > 8;
		}

		protected override void CoreGetDimensions(out int width, out int height)
		{
			if (this.data == null)
			{
				throw new InvalidOperationException();
			}
			width = BitConverter.ToInt32(this.data, 0);
			height = BitConverter.ToInt32(this.data, 4);
		}

		protected unsafe override void CoreProcessImage(int width, int height, int stride, ushort* pLine, ushort* pLineEnd, ushort* pImageEnd, int lineDelta, int lineEndDelta)
		{
			byte[] array = this.data;
			fixed (byte* ptr = array)
			{
				int* ptr2 = (int*)ptr + 2;
				int* ptr3 = ptr2;
				int* ptr4 = ptr3 + height;
				ushort* ptr5 = pLine;
				while (pLine < pImageEnd)
				{
					int num = ((ptr3 + 1 < ptr4) ? ptr3[1] : (array.Length / 4 - 2));
					this.m_Hue.CopyEncodedLine((ushort*)(ptr2 + *ptr3), (ushort*)(ptr2 + num), pLine, pLineEnd);
					pLine += lineEndDelta;
					pLineEnd += lineEndDelta;
					ptr3++;
				}
			}
		}
	}

	private static Gump m_Desktop;

	private static Gump m_Drag;

	private static Gump m_Capture;

	private static Gump m_Focus;

	private static Gump m_Modal;

	private static Gump m_LastDragOver;

	private static Gump m_StartDrag;

	private static Point m_StartDragPoint;

	private static Gump m_LastOver;

	private static GTextBox m_TextFocus;

	private static TimeDelay m_TipDelay;

	private Hashtable m_Objects;

	private static Hashtable m_ToRestore;

	private static bool m_Invalidated;

	private const short Opaque = short.MinValue;

	private GumpFactory m_Factory;

	public static bool Invalidated
	{
		get
		{
			return Gumps.m_Invalidated;
		}
		set
		{
			Gumps.m_Invalidated = value;
		}
	}

	public static Gump Modal
	{
		get
		{
			return Gumps.m_Modal;
		}
		set
		{
			Gumps.m_Modal = value;
			if (Gumps.m_Modal != null && Gumps.m_TextFocus != null && !Gumps.m_TextFocus.IsChildOf(Gumps.m_Modal))
			{
				Gumps.m_TextFocus.Unfocus();
			}
		}
	}

	public static Gump Focus
	{
		get
		{
			return Gumps.m_Focus;
		}
		set
		{
			if (Gumps.m_Focus != value)
			{
				Gumps.RecurseFocusChanged(Gumps.m_Desktop, value);
			}
			Gumps.m_Focus = value;
		}
	}

	public string Name => "Gumps";

	public static Gump Capture
	{
		get
		{
			return Gumps.m_Capture;
		}
		set
		{
			Gumps.m_Capture = value;
		}
	}

	public static Gump Drag
	{
		get
		{
			return Gumps.m_Drag;
		}
		set
		{
			if (value == null && Gumps.m_Drag != null)
			{
				Gumps.m_Drag.m_IsDragging = false;
			}
			Gumps.m_Drag = value;
		}
	}

	public static Gump StartDrag
	{
		get
		{
			return Gumps.m_StartDrag;
		}
		set
		{
			Gumps.m_StartDrag = value;
		}
	}

	public static GTextBox TextFocus
	{
		get
		{
			return Gumps.m_TextFocus;
		}
		set
		{
			Gumps.m_TextFocus = value;
		}
	}

	public static Gump LastDragOver => Gumps.m_LastDragOver;

	public static Gump LastOver
	{
		get
		{
			return Gumps.m_LastOver;
		}
		set
		{
			if (Gumps.m_LastOver != value)
			{
				if (Gumps.m_LastOver != null)
				{
					Gumps.m_LastOver.OnMouseLeave();
				}
				Gumps.m_LastOver = value;
			}
		}
	}

	public static Gump Desktop => Gumps.m_Desktop;

	public static void Invalidate()
	{
		Gumps.m_Invalidated = true;
	}

	public static void Destroy(Gump g)
	{
		if (g == null)
		{
			return;
		}
		Gumps.m_Invalidated = true;
		g.Children.Clear();
		if (g == Gumps.m_Drag)
		{
			Gumps.m_Drag = null;
		}
		if (g == Gumps.m_Capture)
		{
			Gumps.m_Capture = null;
		}
		if (g == Gumps.m_Focus)
		{
			Gumps.m_Focus = null;
		}
		if (g == Gumps.m_Modal)
		{
			Gumps.m_Modal = null;
		}
		if (g == Gumps.m_LastDragOver)
		{
			Gumps.m_LastDragOver = null;
		}
		if (g == Gumps.m_StartDrag)
		{
			Gumps.m_StartDrag = null;
		}
		if (g == Gumps.m_LastOver)
		{
			Gumps.m_LastOver = null;
		}
		if (g == Gumps.m_TextFocus)
		{
			Gumps.m_TextFocus = null;
		}
		if (g.m_Restore && g.GUID != null && g.GUID.Length > 0)
		{
			Gumps.m_ToRestore[g.GUID] = new Point(g.X, g.Y);
		}
		if (g.HasTag("Dispose"))
		{
			string text = (string)g.GetTag("Dispose");
			string text2 = text;
			string text3 = text2;
			if (!(text3 == "Spellbook"))
			{
				if (text3 == "Modal")
				{
					Gumps.m_Modal = null;
				}
			}
			else
			{
				Item item = (Item)g.GetTag("Container");
				if (item != null)
				{
					item.OpenSB = false;
				}
			}
		}
		g.m_Disposed = true;
		g.OnDispose();
		if (g.Parent != null)
		{
			g.Parent.Children.Remove(g);
		}
	}

	public static void Restore(Gump ToRestore)
	{
		if (Gumps.m_ToRestore != null && ToRestore.GUID != null && ToRestore.GUID.Length > 0 && Gumps.m_ToRestore.Contains(ToRestore.GUID))
		{
			Point point = (Point)Gumps.m_ToRestore[ToRestore.GUID];
			ToRestore.X = point.X;
			ToRestore.Y = point.Y;
		}
	}

	public static void TextBoxTab(Gump Start)
	{
		GumpList children;
		int num;
		if (Start.Parent is GWindowsTextBox)
		{
			children = Start.Parent.Parent.Children;
			num = children.IndexOf(Start.Parent);
		}
		else
		{
			children = Start.Parent.Children;
			num = children.IndexOf(Start);
		}
		Gump[] array = children.ToArray();
		int num2 = array.Length;
		if ((Control.ModifierKeys & Keys.Shift) == 0)
		{
			num++;
			for (int i = 0; i < num2; i++)
			{
				Gump gump = array[(i + num) % num2];
				GTextBox gTextBox = ((!(gump is GWindowsTextBox)) ? (gump as GTextBox) : ((GWindowsTextBox)gump).TextBox);
				if (gTextBox != null)
				{
					if (Gumps.m_TextFocus != null)
					{
						Gumps.m_TextFocus.Unfocus();
					}
					gTextBox.Focus();
					break;
				}
			}
			return;
		}
		num--;
		for (int j = 0; j < num2; j++)
		{
			Gump gump2 = array[(num2 + num - j) % num2];
			GTextBox gTextBox = ((!(gump2 is GWindowsTextBox)) ? (gump2 as GTextBox) : ((GWindowsTextBox)gump2).TextBox);
			if (gTextBox != null)
			{
				if (Gumps.m_TextFocus != null)
				{
					Gumps.m_TextFocus.Unfocus();
				}
				gTextBox.Focus();
				break;
			}
		}
	}

	public static Gump FindGumpByGUID(string GUID)
	{
		Stack stack = new Stack();
		stack.Push(Gumps.m_Desktop);
		while (stack.Count > 0)
		{
			Gump gump = (Gump)stack.Pop();
			if (gump.GUID == GUID)
			{
				return gump;
			}
			Gump[] array = gump.Children.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				stack.Push(array[i]);
			}
		}
		return null;
	}

	public static bool KeyDown(char c)
	{
		if (Gumps.m_Modal != null)
		{
			if (Gumps.m_TextFocus != null && Gumps.m_TextFocus.IsChildOf(Gumps.m_Modal) && Gumps.m_TextFocus.OnKeyDown(c))
			{
				return true;
			}
			if (Gumps.m_Focus != null && Gumps.m_Focus.IsChildOf(Gumps.m_Modal))
			{
				if (!Gumps.RecurseKeyDown(Gumps.m_Focus, c))
				{
					Gumps.RecurseKeyDown(Gumps.m_Focus.Parent, c);
				}
			}
			else
			{
				Gumps.RecurseKeyDown(Gumps.m_Modal, c);
			}
			return true;
		}
		if (Gumps.m_TextFocus != null && Gumps.m_TextFocus.OnKeyDown(c))
		{
			return true;
		}
		Gump gump = Gumps.m_Focus;
		while (gump != null)
		{
			if (!Gumps.RecurseKeyDown(gump, c))
			{
				gump = gump.Parent;
				continue;
			}
			return true;
		}
		return false;
	}

	private static bool RecurseKeyDown(Gump g, char c)
	{
		if (!g.Visible)
		{
			return false;
		}
		Gump[] array = g.Children.ToArray();
		for (int num = array.Length - 1; num >= 0; num--)
		{
			if (Gumps.RecurseKeyDown(array[num], c))
			{
				return true;
			}
		}
		if (g.GetType() != typeof(GTextBox))
		{
			return g.OnKeyDown(c);
		}
		return false;
	}

	public static bool MouseUp(int X, int Y, MouseButtons mb)
	{
		Gumps.m_StartDrag = null;
		if (Gumps.m_Capture != null)
		{
			Point point = Gumps.m_Capture.PointToClient(new Point(X, Y));
			Gumps.m_Capture.OnMouseUp(point.X, point.Y, mb);
			return true;
		}
		if (Gumps.m_Desktop == null || Gumps.m_Desktop.Children.Count == 0)
		{
			return false;
		}
		if (Gumps.m_Drag != null && (mb & MouseButtons.Left) == MouseButtons.Left)
		{
			bool result = !Gumps.IsWorldAt(X, Y, CheckDrag: false);
			if (Gumps.m_Drag.m_IsDragging && Gumps.m_LastDragOver != null)
			{
				Gumps.m_LastDragOver.OnDragDrop(Gumps.m_Drag);
				Engine.CancelClick();
			}
			if (Gumps.m_Drag != null)
			{
				Gumps.m_Drag.m_IsDragging = false;
			}
			Gumps.Drag = null;
			Gumps.m_LastDragOver = null;
			return result;
		}
		if (Gumps.m_Drag != null)
		{
			return !Gumps.IsWorldAt(X, Y, CheckDrag: false);
		}
		return Gumps.RecurseMouseUp(0, 0, Gumps.m_Desktop, X, Y, mb) || Gumps.m_Modal != null;
	}

	private static bool RecurseMouseUp(int X, int Y, Gump g, int mX, int mY, MouseButtons mb)
	{
		if (!g.Visible)
		{
			return false;
		}
		if (g.m_NonRestrictivePicking || (mX >= X && mX < X + g.Width && mY >= Y && mY < Y + g.Height))
		{
			Gump[] array = g.Children.ToArray();
			for (int num = array.Length - 1; num >= 0; num--)
			{
				Gump gump = array[num];
				if (Gumps.RecurseMouseUp(X + gump.X, Y + gump.Y, gump, mX, mY, mb))
				{
					return true;
				}
			}
			if (!g.m_NonRestrictivePicking || (mX >= X && mX < X + g.Width && mY >= Y && mY < Y + g.Height))
			{
				if (Gumps.m_Modal == null && g.HitTest(mX - X, mY - Y))
				{
					g.OnMouseUp(mX - X, mY - Y, mb);
					return true;
				}
				if (Gumps.m_Modal != null && g.IsChildOf(Gumps.m_Modal) && g.HitTest(mX - X, mY - Y))
				{
					g.OnMouseUp(mX - X, mY - Y, mb);
					return true;
				}
			}
		}
		return false;
	}

	public static bool MouseDown(int X, int Y, MouseButtons mb)
	{
		if (Gumps.m_Capture != null)
		{
			Point point = Gumps.m_Capture.PointToClient(new Point(X, Y));
			Gumps.m_Capture.OnMouseDown(point.X, point.Y, mb);
			Gumps.Focus = Gumps.m_Capture;
			return true;
		}
		if (Gumps.m_Desktop == null || Gumps.m_Desktop.Children.Count == 0)
		{
			return false;
		}
		if (Gumps.RecurseMouseDown(0, 0, Gumps.m_Desktop, X, Y, mb))
		{
			return true;
		}
		if (Gumps.m_Modal != null)
		{
			Gumps.Focus = Gumps.m_Modal;
			return true;
		}
		if (Gumps.m_Drag != null)
		{
			Gumps.Focus = Gumps.m_Drag;
			return !Gumps.IsWorldAt(X, Y, CheckDrag: false);
		}
		Gumps.Focus = null;
		return false;
	}

	private static bool RecurseMouseDown(int X, int Y, Gump g, int mX, int mY, MouseButtons mb)
	{
		if (!g.Visible)
		{
			return false;
		}
		if (g.m_NonRestrictivePicking || (mX >= X && mX < X + g.Width && mY >= Y && mY < Y + g.Height))
		{
			Gump[] array = g.Children.ToArray();
			for (int num = array.Length - 1; num >= 0; num--)
			{
				Gump gump = array[num];
				if (Gumps.RecurseMouseDown(X + gump.X, Y + gump.Y, gump, mX, mY, mb))
				{
					return true;
				}
			}
			if (!g.m_NonRestrictivePicking || (mX >= X && mX < X + g.Width && mY >= Y && mY < Y + g.Height))
			{
				if (Gumps.m_Modal == null && g.HitTest(mX - X, mY - Y))
				{
					if (Gumps.m_TextFocus != null)
					{
						Gumps.m_TextFocus.Unfocus();
						Gumps.m_TextFocus = null;
					}
					if (Gumps.m_Drag == null && g.m_CanDrag && mb == MouseButtons.Left)
					{
						Gumps.m_StartDrag = g;
						Gumps.m_StartDragPoint = new Point(mX, mY);
						g.m_OffsetX = mX - X;
						g.m_OffsetY = mY - Y;
						if (g.m_QuickDrag)
						{
							g.m_IsDragging = true;
							Gumps.m_Drag = g;
							g.OnDragStart();
						}
					}
					g.OnMouseDown(mX - X, mY - Y, mb);
					Gumps.Focus = g;
					if (g == Gumps.m_Drag)
					{
						return !Gumps.IsWorldAt(mX, mY, CheckDrag: false);
					}
					return true;
				}
				if (Gumps.m_Modal != null && g.IsChildOf(Gumps.m_Modal) && g.HitTest(mX - X, mY - Y))
				{
					if (Gumps.m_TextFocus != null)
					{
						Gumps.m_TextFocus.Unfocus();
						Gumps.m_TextFocus = null;
					}
					if (Gumps.m_Drag == null && g.m_CanDrag && mb == MouseButtons.Left)
					{
						Gumps.m_StartDrag = g;
						Gumps.m_StartDragPoint = new Point(mX, mY);
						g.m_OffsetX = mX - X;
						g.m_OffsetY = mY - Y;
						if (g.m_QuickDrag)
						{
							g.m_IsDragging = true;
							g.OnDragStart();
							Gumps.m_Drag = g;
						}
					}
					g.OnMouseDown(mX - X, mY - Y, mb);
					Gumps.Focus = g;
					if (g == Gumps.m_Drag)
					{
						return !Gumps.IsWorldAt(mX, mY, CheckDrag: false);
					}
					return true;
				}
			}
		}
		return false;
	}

	public static object[] FindListForSingleClick(int x, int y)
	{
		if (Gumps.m_Capture != null)
		{
			Point point = Gumps.m_Capture.PointToClient(new Point(x, y));
			return new object[2]
			{
				Gumps.m_Capture,
				point
			};
		}
		if (Gumps.m_Desktop == null || Gumps.m_Desktop.Children.Count == 0)
		{
			return null;
		}
		return Gumps.RecurseFindListForSingleClick(0, 0, Gumps.m_Desktop, x, y);
	}

	private static object[] RecurseFindListForSingleClick(int x, int y, Gump g, int mx, int my)
	{
		if (!g.Visible)
		{
			return null;
		}
		if (g.m_NonRestrictivePicking || (mx >= x && mx < x + g.Width && my >= y && my < y + g.Height))
		{
			Gump[] array = g.Children.ToArray();
			for (int num = array.Length - 1; num >= 0; num--)
			{
				Gump gump = array[num];
				object[] array2 = Gumps.RecurseFindListForSingleClick(x + gump.X, y + gump.Y, gump, mx, my);
				if (array2 != null)
				{
					return array2;
				}
			}
			if (!g.m_NonRestrictivePicking || (mx >= x && mx < x + g.Width && my >= y && my < y + g.Height))
			{
				if (Gumps.m_Modal == null && g.HitTest(mx - x, my - y))
				{
					return new object[2]
					{
						g,
						new Point(mx - x, my - y)
					};
				}
				if (Gumps.m_Modal != null && g.IsChildOf(Gumps.m_Modal) && g.HitTest(mx - x, my - y))
				{
					return new object[2]
					{
						g,
						new Point(mx - x, my - y)
					};
				}
			}
		}
		return null;
	}

	public static bool DoubleClick(int X, int Y)
	{
		if (Gumps.m_Capture != null)
		{
			Point point = Gumps.m_Capture.PointToClient(new Point(X, Y));
			Gumps.m_Capture.OnDoubleClick(point.X, point.Y);
			return true;
		}
		if (Gumps.m_Desktop == null || Gumps.m_Desktop.Children.Count == 0)
		{
			return false;
		}
		return Gumps.RecurseDoubleClick(0, 0, Gumps.m_Desktop, X, Y) || Gumps.m_Modal != null;
	}

	private static void RecurseFocusChanged(Gump g, Gump focus)
	{
		g.OnFocusChanged(focus);
		Gump[] array = g.Children.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			Gumps.RecurseFocusChanged(array[i], focus);
		}
	}

	private static bool RecurseDoubleClick(int X, int Y, Gump g, int mX, int mY)
	{
		if (!g.Visible)
		{
			return false;
		}
		if (g.m_NonRestrictivePicking || (mX >= X && mX < X + g.Width && mY >= Y && mY < Y + g.Height))
		{
			Gump[] array = g.Children.ToArray();
			for (int num = array.Length - 1; num >= 0; num--)
			{
				Gump gump = array[num];
				if (Gumps.RecurseDoubleClick(X + gump.X, Y + gump.Y, gump, mX, mY))
				{
					return true;
				}
			}
			if (!g.m_NonRestrictivePicking || (mX >= X && mX < X + g.Width && mY >= Y && mY < Y + g.Height))
			{
				if (Gumps.m_Modal == null && g.HitTest(mX - X, mY - Y))
				{
					if (Gumps.m_TextFocus != null)
					{
						Gumps.m_TextFocus.Unfocus();
						Gumps.m_TextFocus = null;
					}
					g.OnDoubleClick(mX - X, mY - Y);
					return true;
				}
				if (Gumps.m_Modal != null && g.IsChildOf(Gumps.m_Modal) && g.HitTest(mX - X, mY - Y))
				{
					if (Gumps.m_TextFocus != null)
					{
						Gumps.m_TextFocus.Unfocus();
						Gumps.m_TextFocus = null;
					}
					g.OnDoubleClick(mX - X, mY - Y);
					return true;
				}
			}
		}
		return false;
	}

	public static void MouseWheel(int X, int Y, int Delta)
	{
		if (Gumps.m_Capture != null)
		{
			Gumps.m_Capture.OnMouseWheel(Delta);
		}
		else if (Gumps.m_Desktop != null && Gumps.m_Desktop.Children.Count != 0 && !Gumps.RecurseMouseWheel(0, 0, Gumps.m_Desktop, X, Y, Delta) && Gumps.m_Focus != null)
		{
			Gumps.m_Focus.OnMouseWheel(Delta);
		}
	}

	public static bool RecurseMouseWheel(int X, int Y, Gump g, int mX, int mY, int Delta)
	{
		if (!g.Visible)
		{
			return false;
		}
		if (g.m_NonRestrictivePicking || (mX >= X && mX < X + g.Width && mY >= Y && mY < Y + g.Height))
		{
			Gump[] array = g.Children.ToArray();
			for (int num = array.Length - 1; num >= 0; num--)
			{
				Gump gump = array[num];
				if (Gumps.RecurseMouseWheel(X + gump.X, Y + gump.Y, gump, mX, mY, Delta))
				{
					return true;
				}
			}
			if (!g.m_NonRestrictivePicking || (mX >= X && mX < X + g.Width && mY >= Y && mY < Y + g.Height))
			{
				if (Gumps.m_Modal == null && g.HitTest(mX - X, mY - Y))
				{
					g.OnMouseWheel(Delta);
					return true;
				}
				if (Gumps.m_Modal != null && g.IsChildOf(Gumps.m_Modal) && g.HitTest(mX - X, mY - Y))
				{
					g.OnMouseWheel(Delta);
					return true;
				}
			}
		}
		return false;
	}

	public static bool MouseMove(int X, int Y, MouseButtons mb)
	{
		if (Gumps.m_Capture != null)
		{
			Point point = Gumps.m_Capture.PointToClient(new Point(X, Y));
			if (Gumps.m_LastOver != Gumps.m_Capture)
			{
				if (Gumps.m_LastOver != null)
				{
					Gumps.m_LastOver.OnMouseLeave();
				}
				Gumps.m_Capture.OnMouseEnter(point.X, point.Y, mb);
				Gumps.m_LastOver = Gumps.m_Capture;
			}
			Gumps.m_Capture.OnMouseMove(point.X, point.Y, mb);
			return true;
		}
		if (Gumps.m_Desktop == null || Gumps.m_Desktop.Children.Count == 0)
		{
			return false;
		}
		if (Gumps.m_Drag != null && Gumps.m_Drag.m_IsDragging)
		{
			int num = X - Gumps.m_Drag.m_OffsetX;
			int num2 = Y - Gumps.m_Drag.m_OffsetY;
			if (num + Gumps.m_Drag.Width < Gumps.m_Drag.m_DragClipX)
			{
				num = Gumps.m_Drag.m_DragClipX - Gumps.m_Drag.Width;
			}
			else if (num > Engine.ScreenWidth - Gumps.m_Drag.m_DragClipX)
			{
				num = Engine.ScreenWidth - Gumps.m_Drag.m_DragClipX;
			}
			if (num2 + Gumps.m_Drag.Height < Gumps.m_Drag.m_DragClipY)
			{
				num2 = Gumps.m_Drag.m_DragClipY - Gumps.m_Drag.Height;
			}
			else if (num2 > Engine.ScreenHeight - Gumps.m_Drag.m_DragClipY)
			{
				num2 = Engine.ScreenHeight - Gumps.m_Drag.m_DragClipY;
			}
			Point point2 = Gumps.m_Drag.Parent.PointToClient(new Point(num, num2));
			Gumps.m_Drag.X = point2.X;
			Gumps.m_Drag.Y = point2.Y;
			Gumps.m_Drag.OnDragMove();
			Gump target = null;
			Gumps.RecurseFindDrop(0, 0, Gumps.m_Desktop, X, Y, mb, ref target);
			if (target != null)
			{
				if (Gumps.m_LastDragOver != target)
				{
					if (Gumps.m_LastDragOver != null)
					{
						Gumps.m_LastDragOver.OnDragLeave(Gumps.m_Drag);
					}
					target.OnDragEnter(Gumps.m_Drag);
				}
			}
			else if (Gumps.m_LastDragOver != null)
			{
				Gumps.m_LastDragOver.OnDragLeave(Gumps.m_Drag);
			}
			Gumps.m_LastDragOver = target;
			if (Gumps.m_LastOver != Gumps.m_Drag)
			{
				if (Gumps.m_LastOver != null)
				{
					Gumps.m_LastOver.OnMouseLeave();
				}
				Gumps.m_LastOver = Gumps.m_Drag;
				if (Gumps.m_LastOver != null)
				{
					point2 = Gumps.m_LastOver.PointToClient(new Point(X, Y));
					Gumps.m_LastOver.OnMouseEnter(point2.X, point2.Y, mb);
				}
			}
			return !Gumps.IsWorldAt(X, Y, CheckDrag: false);
		}
		Gump startDrag = Gumps.m_StartDrag;
		if (!Gumps.RecurseMouseMove(0, 0, Gumps.m_Desktop, X, Y, mb))
		{
			if (startDrag != null && startDrag.m_CanDrag && mb == MouseButtons.Left)
			{
				Gumps.m_Drag = startDrag;
				startDrag.m_IsDragging = true;
				startDrag.OnDragStart();
			}
			else if (Gumps.m_LastOver != null)
			{
				Gumps.m_LastOver.OnMouseLeave();
				Gumps.m_LastOver = null;
			}
			return Gumps.m_Modal != null;
		}
		if (startDrag != Gumps.m_LastOver && startDrag != null && startDrag.m_CanDrag && !startDrag.m_IsDragging && !startDrag.m_QuickDrag && mb == MouseButtons.Left)
		{
			Gumps.m_Drag = startDrag;
			if (Gumps.m_LastOver != Gumps.m_Drag)
			{
				if (Gumps.m_LastOver != null)
				{
					Gumps.m_LastOver.OnMouseLeave();
				}
				Gumps.m_LastOver = Gumps.m_Drag;
				if (Gumps.m_LastOver != null)
				{
					Point p = new Point(X, Y);
					p = Gumps.m_LastOver.PointToClient(p);
					Gumps.m_LastOver.OnMouseEnter(p.X, p.Y, mb);
				}
			}
			startDrag.m_IsDragging = true;
			startDrag.OnDragStart();
			if (Gumps.m_Drag != null && Gumps.m_Drag.m_IsDragging)
			{
				Gumps.MouseMove(X, Y, mb);
			}
			if (Gumps.m_LastOver != Gumps.m_Drag)
			{
				if (Gumps.m_LastOver != null)
				{
					Gumps.m_LastOver.OnMouseLeave();
				}
				Gumps.m_LastOver = Gumps.m_Drag;
				if (Gumps.m_LastOver != null)
				{
					Point p2 = new Point(X, Y);
					p2 = Gumps.m_LastOver.PointToClient(p2);
					Gumps.m_LastOver.OnMouseEnter(p2.X, p2.Y, mb);
				}
			}
		}
		else if (startDrag == Gumps.m_LastOver && startDrag != null && startDrag.m_CanDrag && !startDrag.m_IsDragging && !startDrag.m_QuickDrag && mb == MouseButtons.Left && (Gumps.m_StartDragPoint ^ new Point(X, Y)) >= 2)
		{
			Gumps.m_Drag = startDrag;
			if (Gumps.m_LastOver != Gumps.m_Drag)
			{
				if (Gumps.m_LastOver != null)
				{
					Gumps.m_LastOver.OnMouseLeave();
				}
				Gumps.m_LastOver = Gumps.m_Drag;
				if (Gumps.m_LastOver != null)
				{
					Point p3 = new Point(X, Y);
					p3 = Gumps.m_LastOver.PointToClient(p3);
					Gumps.m_LastOver.OnMouseEnter(p3.X, p3.Y, mb);
				}
			}
			startDrag.m_IsDragging = true;
			startDrag.OnDragStart();
			if (Gumps.m_Drag != null && Gumps.m_Drag.m_IsDragging)
			{
				Gumps.MouseMove(X, Y, mb);
			}
			if (Gumps.m_LastOver != Gumps.m_Drag)
			{
				if (Gumps.m_LastOver != null)
				{
					Gumps.m_LastOver.OnMouseLeave();
				}
				Gumps.m_LastOver = Gumps.m_Drag;
				if (Gumps.m_LastOver != null)
				{
					Point p4 = new Point(X, Y);
					p4 = Gumps.m_LastOver.PointToClient(p4);
					Gumps.m_LastOver.OnMouseEnter(p4.X, p4.Y, mb);
				}
			}
		}
		return true;
	}

	private static bool RecurseFindDrop(int X, int Y, Gump g, int mX, int mY, MouseButtons mb, ref Gump target)
	{
		if (!g.Visible)
		{
			return false;
		}
		if (g == Gumps.m_Drag)
		{
			return false;
		}
		if (g.m_NonRestrictivePicking || (mX >= X && mX < X + g.Width && mY >= Y && mY < Y + g.Height))
		{
			Gump[] array = g.Children.ToArray();
			for (int num = array.Length - 1; num >= 0; num--)
			{
				Gump gump = array[num];
				if (Gumps.RecurseFindDrop(X + gump.X, Y + gump.Y, gump, mX, mY, mb, ref target))
				{
					return true;
				}
			}
			if ((!g.m_NonRestrictivePicking || (mX >= X && mX < X + g.Width && mY >= Y && mY < Y + g.Height)) && g.m_CanDrop)
			{
				if (Gumps.m_Modal == null && g.HitTest(mX - X, mY - Y))
				{
					target = g;
					return true;
				}
				if (Gumps.m_Modal != null && g.IsChildOf(Gumps.m_Modal) && g.HitTest(mX - X, mY - Y))
				{
					target = g;
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsWorldAt(int X, int Y)
	{
		if (Gumps.m_Modal != null)
		{
			return true;
		}
		return !Gumps.RecurseIsWorldAt(0, 0, Gumps.m_Desktop, X, Y, CheckDrag: false);
	}

	public static bool IsWorldAt(int X, int Y, bool CheckDrag)
	{
		if (Gumps.m_Modal != null)
		{
			return true;
		}
		return !Gumps.RecurseIsWorldAt(0, 0, Gumps.m_Desktop, X, Y, CheckDrag);
	}

	private static bool RecurseIsWorldAt(int X, int Y, Gump g, int mX, int mY, bool CheckDrag)
	{
		if (!g.Visible)
		{
			return false;
		}
		if (!CheckDrag && g == Gumps.m_Drag)
		{
			return false;
		}
		if (g.m_NonRestrictivePicking || (mX >= X && mX < X + g.Width && mY >= Y && mY < Y + g.Height))
		{
			Gump[] array = g.Children.ToArray();
			for (int num = array.Length - 1; num >= 0; num--)
			{
				Gump gump = array[num];
				if (Gumps.RecurseIsWorldAt(X + gump.X, Y + gump.Y, gump, mX, mY, CheckDrag))
				{
					return true;
				}
			}
			if ((!g.m_NonRestrictivePicking || (mX >= X && mX < X + g.Width && mY >= Y && mY < Y + g.Height)) && g.HitTest(mX - X, mY - Y))
			{
				return true;
			}
		}
		return false;
	}

	private static bool RecurseMouseMove(int X, int Y, Gump g, int mX, int mY, MouseButtons mb)
	{
		if (!g.Visible)
		{
			return false;
		}
		if (g.m_NonRestrictivePicking || (mX >= X && mX < X + g.Width && mY >= Y && mY < Y + g.Height))
		{
			Gump[] array = g.Children.ToArray();
			for (int num = array.Length - 1; num >= 0; num--)
			{
				Gump gump = array[num];
				if (Gumps.RecurseMouseMove(X + gump.X, Y + gump.Y, gump, mX, mY, mb))
				{
					return true;
				}
			}
			if (!g.m_NonRestrictivePicking || (mX >= X && mX < X + g.Width && mY >= Y && mY < Y + g.Height))
			{
				if (Gumps.m_Modal == null && g.HitTest(mX - X, mY - Y))
				{
					if (Gumps.m_LastOver == g)
					{
						g.OnMouseMove(mX - X, mY - Y, mb);
					}
					else
					{
						if (Gumps.m_LastOver != null)
						{
							Gumps.m_LastOver.OnMouseLeave();
						}
						g.OnMouseEnter(mX - X, mY - Y, mb);
						if (g.Tooltip != null)
						{
							Gumps.m_TipDelay = new TimeDelay(g.Tooltip.Delay);
						}
						else
						{
							Gumps.m_TipDelay = null;
						}
						Gumps.m_LastOver = g;
					}
					return true;
				}
				if (Gumps.m_Modal != null && g.IsChildOf(Gumps.m_Modal) && g.HitTest(mX - X, mY - Y))
				{
					if (Gumps.m_LastOver == g)
					{
						g.OnMouseMove(mX - X, mY - Y, mb);
					}
					else
					{
						if (Gumps.m_LastOver != null)
						{
							Gumps.m_LastOver.OnMouseLeave();
						}
						g.OnMouseEnter(mX - X, mY - Y, mb);
						if (g.Tooltip != null)
						{
							Gumps.m_TipDelay = new TimeDelay(g.Tooltip.Delay);
						}
						else
						{
							Gumps.m_TipDelay = null;
						}
						Gumps.m_LastOver = g;
					}
					return true;
				}
			}
		}
		return false;
	}

	public static void MessageBoxOk(string Prompt, bool Modal, OnClick ClickHandler)
	{
		GBackground gBackground = new GBackground(2604, 356, 212, 142, 134, HasBorder: true);
		GButton gButton = new GButton(1153, 1154, 1155, 164, 170, MessageBoxOk_OnClick);
		gButton.SetTag("Dialog", gBackground);
		gButton.SetTag("ClickHandler", ClickHandler);
		GWrappedLabel gWrappedLabel = new GWrappedLabel(Prompt, Engine.GetFont(1), Hues.Load(1899), gBackground.OffsetX, gBackground.OffsetY, Engine.ScreenWidth / 2 - gBackground.OffsetX * 2);
		gBackground.Width = gWrappedLabel.Width + gBackground.OffsetX * 2;
		gBackground.Height = gWrappedLabel.Height + 10 + gBackground.OffsetY * 2;
		if (gBackground.Width < 150)
		{
			gBackground.Width = 150;
		}
		gBackground.Center();
		gButton.X = (gBackground.Width - gButton.Width) / 2;
		gButton.Y = gBackground.Height - gBackground.OffsetY;
		gBackground.Children.Add(gWrappedLabel);
		gBackground.Children.Add(gButton);
		if (Modal)
		{
			gBackground.Modal = true;
		}
		gBackground.m_CanDrag = true;
		gBackground.m_QuickDrag = true;
		Gumps.m_Desktop.Children.Add(gBackground);
	}

	public static void MessageBoxOk_OnClick(Gump Sender)
	{
		Gump gump = (Gump)Sender.GetTag("Dialog");
		((OnClick)Sender.GetTag("ClickHandler"))?.Invoke(Sender);
		if (gump != null)
		{
			Gumps.Destroy(gump);
		}
	}

	public static bool Check(ref int gumpID, ref int hue)
	{
		Gumps gumps = Engine.m_Gumps;
		if (gumps.m_Factory.Exists(gumpID))
		{
			return true;
		}
		GraphicTranslation graphicTranslation = GraphicTranslators.Gumps[gumpID];
		if (graphicTranslation != null)
		{
			gumpID = graphicTranslation.FallbackId;
			if (hue == 0)
			{
				hue = graphicTranslation.FallbackData;
			}
			return true;
		}
		return false;
	}

	public static int GetEquipGumpID(int itemID, int gender, ref int hue)
	{
		int animation = Map.GetAnimation(itemID);
		if (gender == 0)
		{
			int gumpID = animation + 50000;
			if (Gumps.Check(ref gumpID, ref hue))
			{
				return gumpID;
			}
			gumpID += 10000;
			if (Gumps.Check(ref gumpID, ref hue))
			{
				return gumpID;
			}
		}
		else
		{
			int gumpID = animation + 60000;
			if (Gumps.Check(ref gumpID, ref hue))
			{
				return gumpID;
			}
			gumpID -= 10000;
			if (Gumps.Check(ref gumpID, ref hue))
			{
				return gumpID;
			}
		}
		return 0;
	}

	public static void OpenPaperdoll(Mobile m, string Name, bool canDrag)
	{
		if (m == null)
		{
			return;
		}
		bool flag = m.Paperdoll != null;
		bool flag2 = flag && Gumps.m_LastOver == m.Paperdoll;
		bool flag3 = flag && Gumps.m_Drag == m.Paperdoll;
		int offsetX = (flag3 ? Gumps.m_Drag.m_OffsetX : 0);
		int offsetY = (flag3 ? Gumps.m_Drag.m_OffsetY : 0);
		int num = (flag ? m.Paperdoll.Parent.Children.IndexOf(m.Paperdoll) : (-1));
		int num2 = int.MaxValue;
		int y = 5;
		if (flag)
		{
			num2 = m.Paperdoll.X;
			y = m.Paperdoll.Y;
			Gumps.Destroy(m.Paperdoll);
		}
		else if (m.PaperdollX < int.MaxValue && m.PaperdollY < int.MaxValue)
		{
			num2 = m.PaperdollX;
			y = m.PaperdollY;
			m.PaperdollX = int.MaxValue;
			m.PaperdollY = int.MaxValue;
		}
		OnClick[] array = new OnClick[8]
		{
			Engine.Help_OnClick,
			Engine.Options_OnClick,
			Engine.LogOut_OnClick,
			Engine.Journal_OnClick,
			Engine.Skills_OnClick,
			Engine.Guild_OnClick,
			Engine.AttackModeToggle_OnClick,
			Engine.Status_OnClick
		};
		int[] array2 = new int[8] { 44, 71, 98, 124, 151, 179, 205, 233 };
		int[] obj = new int[8] { 2031, 2006, 2009, 2012, 2015, 22450, 0, 2027 };
		obj[6] = (World.Player.Flags[MobileFlag.Warmode] ? 2024 : 2021);
		int[] array3 = obj;
		GPaperdoll gPaperdoll;
		if (m.Player)
		{
			gPaperdoll = new GPaperdoll(m, 2000, num2, y, canDrag);
			if (!flag && num2 >= int.MaxValue)
			{
				gPaperdoll.X = Engine.ScreenWidth - gPaperdoll.Width - 5;
			}
			GButton[] array4 = new GButton[7];
			for (int i = 0; i < 7; i++)
			{
				if (array3[i] == 22450)
				{
					array4[i] = new GButton(array3[i], array3[i] + 1, array3[i] + 2, 185, array2[i], array[i]);
				}
				else
				{
					array4[i] = new GButton(array3[i], array3[i] + 2, array3[i] + 1, 185, array2[i], array[i]);
				}
				array4[i].Enabled = array[i] != null;
				gPaperdoll.Children.Add(array4[i]);
			}
		}
		else
		{
			gPaperdoll = new GPaperdoll(m, 2001, num2, y, canDrag);
			if (!flag && num2 >= int.MaxValue)
			{
				gPaperdoll.X = Engine.ScreenWidth - gPaperdoll.Width - 5;
			}
		}
		gPaperdoll.Title = Name;
		GButton gButton = new GButton(array3[7], array3[7] + 2, array3[7] + 1, 185, array2[7], array[7]);
		gButton.SetTag("Serial", m.Serial);
		gPaperdoll.Children.Add(gButton);
		int hue = m.Hue;
		bool flag4 = false;
		foreach (Item item in m.Items)
		{
			if (!flag4 || item.Layer == Layer.OuterTorso)
			{
				gPaperdoll.OnChildAdded(item);
			}
		}
		gPaperdoll.SetTag("Dispose", "Paperdoll");
		gPaperdoll.SetTag("Serial", m.Serial);
		if (flag2)
		{
			Gumps.m_LastOver = gPaperdoll;
		}
		if (flag3)
		{
			gPaperdoll.m_IsDragging = true;
			gPaperdoll.OffsetX = offsetX;
			gPaperdoll.OffsetY = offsetY;
			Gumps.m_Drag = gPaperdoll;
		}
		if (gPaperdoll.X + gPaperdoll.Width - gPaperdoll.m_DragClipX < 0)
		{
			gPaperdoll.X = gPaperdoll.m_DragClipX - gPaperdoll.Width;
		}
		else if (gPaperdoll.X + gPaperdoll.m_DragClipX >= Engine.ScreenWidth)
		{
			gPaperdoll.X = Engine.ScreenWidth - gPaperdoll.m_DragClipX;
		}
		if (gPaperdoll.Y + gPaperdoll.Height - gPaperdoll.m_DragClipY < 0)
		{
			gPaperdoll.Y = gPaperdoll.m_DragClipY - gPaperdoll.Height;
		}
		else if (gPaperdoll.Y + gPaperdoll.m_DragClipY >= Engine.ScreenHeight)
		{
			gPaperdoll.Y = Engine.ScreenHeight - gPaperdoll.m_DragClipY;
		}
		if (num != -1)
		{
			Gumps.Desktop.Children.Insert(num, gPaperdoll);
		}
		else
		{
			Gumps.Desktop.Children.Add(gPaperdoll);
		}
		m.SetContainerView(gPaperdoll);
	}

	public Gumps()
	{
		Gumps.m_Desktop = new Gump(0, 0);
		Gumps.m_Desktop.GUID = "Desktop";
		Gumps.m_ToRestore = new Hashtable();
		this.m_Factory = new GumpFactory(this);
	}

	public void DisplayObject(string Name)
	{
		Gumps.m_Desktop.Children.Add((Gump)this.m_Objects[Name]);
	}

	public void Dispose()
	{
		Stack stack = new Stack();
		stack.Push(Gumps.m_Desktop);
		while (stack.Count > 0)
		{
			Gump gump = (Gump)stack.Pop();
			if (gump == null)
			{
				continue;
			}
			GumpList children = gump.Children;
			if (children != null)
			{
				Gump[] array = children.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					stack.Push(array[i]);
				}
			}
			try
			{
				gump.OnDispose();
			}
			catch (Exception ex)
			{
				Debug.Trace("Exception in {0}.OnDispose()", gump);
				Debug.Error(ex);
			}
		}
		Gumps.m_Desktop = null;
		Gumps.m_Drag = null;
		Gumps.m_Capture = null;
		Gumps.m_Focus = null;
		Gumps.m_Modal = null;
		Gumps.m_LastDragOver = null;
		Gumps.m_StartDrag = null;
		Gumps.m_LastOver = null;
		Gumps.m_TextFocus = null;
		Gumps.m_TipDelay = null;
		if (this.m_Objects != null)
		{
			this.m_Objects.Clear();
			this.m_Objects = null;
		}
		if (Gumps.m_ToRestore != null)
		{
			Gumps.m_ToRestore.Clear();
			Gumps.m_ToRestore = null;
		}
	}

	public static void Draw()
	{
		if (Gumps.m_Desktop == null)
		{
			return;
		}
		Gumps.m_Desktop.Render(0, 0);
		if (Gumps.m_LastOver == null || Gumps.m_LastOver.Tooltip == null || Gumps.m_TipDelay == null || !Gumps.m_TipDelay.Elapsed || !Cursor.Visible)
		{
			return;
		}
		Gump gump = Gumps.m_LastOver.Tooltip.GetGump();
		if (gump != null)
		{
			bool flag = Engine.m_xMouse < Engine.ScreenWidth / 2;
			bool flag2 = Engine.m_yMouse < Engine.ScreenHeight / 2;
			int num = Engine.m_xMouse - gump.Width - 2;
			int num2 = Engine.m_yMouse - gump.Height - 2;
			if (flag)
			{
				num = ((!flag2) ? Engine.m_xMouse : (Engine.m_xMouse + Cursor.Width + 2));
			}
			if (flag2)
			{
				num2 = ((!flag) ? Engine.m_yMouse : (Engine.m_yMouse + Cursor.Height + 2));
			}
			if (num < 2)
			{
				num = 2;
			}
			else if (num + gump.Width + 2 > Engine.ScreenWidth)
			{
				num = Engine.ScreenWidth - gump.Width - 2;
			}
			if (num2 < 2)
			{
				num2 = 2;
			}
			else if (num2 + gump.Height + 2 > Engine.ScreenHeight)
			{
				num2 = Engine.ScreenHeight - gump.Height - 2;
			}
			gump.Render(num, num2);
		}
	}

	[Obsolete("please don't ever use this", false)]
	public Size Measure(int gumpID)
	{
		Texture gump = Hues.Default.GetGump(gumpID);
		if (gump != null)
		{
			return new Size(gump.Width, gump.Height);
		}
		return Size.Empty;
	}

	public Texture ReadFromDisk(int GumpID, IHue Hue)
	{
		if (this.m_Factory == null)
		{
			this.m_Factory = new GumpFactory(this);
		}
		return this.m_Factory.Load(GumpID, Hue);
	}
}
