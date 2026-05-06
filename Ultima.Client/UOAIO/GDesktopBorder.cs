using System.Windows.Forms;
using UOAIO.Profiles;

namespace UOAIO;

public class GDesktopBorder : GBackground
{
	public static GDesktopBorder Instance;

	public GDesktopBorder()
		: base(Engine.GameX - 4, Engine.GameY - 4, Engine.GameWidth + 8, Engine.GameHeight + 8, 2700, 2700, 2700, 2701, 0, 2701, 2700, 2700, 2700)
	{
		GDesktopBorder.Instance = this;
		base.m_CanDrag = true;
		base.m_QuickDrag = true;
		base.m_DragClipX = Engine.GameWidth + 4;
		base.m_DragClipY = Engine.GameHeight + 4;
		base.CanClose = false;
	}

	protected internal override void OnDragStart()
	{
		if ((Control.ModifierKeys & Keys.Shift) == 0)
		{
			base.m_IsDragging = false;
			Gumps.Drag = null;
		}
	}

	protected internal override bool HitTest(int X, int Y)
	{
		if (Engine.amMoving)
		{
			return false;
		}
		return X < 4 || Y < 4 || X >= Engine.GameWidth + 4 || Y >= Engine.GameHeight + 4;
	}

	public void DoRender()
	{
		int num = this.X + 4;
		int num2 = this.Y + 4;
		if (num != Engine.GameX || num2 != Engine.GameY)
		{
			Engine.GameX = num;
			Engine.GameY = num2;
			Preferences.Current.Layout.Update();
		}
		base.Render(Engine.GameX - 4 - this.X, Engine.GameY - 4 - this.Y);
	}

	protected internal override void Render(int X, int Y)
	{
	}
}
