using System.Collections;
using System.Reflection;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class GEditorPanel : GEmpty
{
	private GCategoryPanel[] m_Panels;

	private GEditorScroller m_Scroller;

	private int m_xLast;

	private int m_yLast;

	private Clipper m_Clipper;

	public GCategoryPanel[] Panels => this.m_Panels;

	public void Layout()
	{
		int num = 5;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		if (this.m_Scroller != null)
		{
			num2 = -this.m_Scroller.Value;
		}
		for (int i = 0; i < this.m_Panels.Length; i++)
		{
			GCategoryPanel gCategoryPanel = this.m_Panels[i];
			gCategoryPanel.X = 5;
			gCategoryPanel.Y = num + num2;
			if (this.m_Scroller == null)
			{
				base.m_Children.Add(gCategoryPanel);
			}
			if (gCategoryPanel.Width > num3)
			{
				num3 = gCategoryPanel.Width;
			}
			if (num + gCategoryPanel.Height > num4)
			{
				num4 = num + gCategoryPanel.Height;
			}
			num += gCategoryPanel.Height - 1;
		}
		num3 = (this.Width = num3 + 26);
		if (this.m_Scroller == null)
		{
			this.m_Scroller = new GEditorScroller(this);
			this.m_Scroller.X = num3 - 16;
			this.m_Scroller.Y = 0;
			this.m_Scroller.Height = this.Height;
			this.m_Scroller.Width = 16;
			this.m_Scroller.Maximum = num4 - this.Height + 5;
			base.m_Children.Insert(0, this.m_Scroller);
		}
	}

	public GEditorPanel(ArrayList panels, int height)
		: base(0, 0, 0, height)
	{
		this.m_Panels = (GCategoryPanel[])panels.ToArray(typeof(GCategoryPanel));
		base.m_NonRestrictivePicking = true;
		this.Layout();
	}

	public void Reset()
	{
		for (int i = 0; i < this.m_Panels.Length; i++)
		{
			this.m_Panels[i].Reset();
		}
	}

	protected internal override void Draw(int X, int Y)
	{
		if (this.m_xLast != X || this.m_yLast != Y)
		{
			this.m_xLast = X;
			this.m_yLast = Y;
			this.m_Clipper = new Clipper(this.m_xLast + 5, this.m_yLast, this.Width - 10, this.Height);
			for (int i = 0; i < this.m_Panels.Length; i++)
			{
				this.m_Panels[i].SetClipper(this.m_Clipper);
			}
		}
		Renderer.SetTexture(null);
		GumpPaint.DrawSunken3D(X - 2, Y - 2, this.Width + 4, this.Height + 4);
	}
}
