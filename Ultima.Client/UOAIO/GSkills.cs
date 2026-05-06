using System.Windows.Forms;

namespace UOAIO;

public class GSkills : GAlphaBackground, IResizable
{
	private GLabel m_Total;

	private GTextButton m_ValueType;

	private GSkillList m_SkillList;

	private bool m_ShowReal;

	private bool m_ShouldClose;

	public bool ShowReal => this.m_ShowReal;

	public int MinWidth => 225;

	public int MaxWidth => Engine.ScreenWidth;

	public int MinHeight => 43;

	public int MaxHeight => Engine.ScreenHeight;

	public override int Width
	{
		get
		{
			return base.m_Width;
		}
		set
		{
			base.m_Width = value;
			this.m_Total.X = base.m_Width - 5 - this.m_Total.Image.xMax;
			this.m_SkillList.Width = base.m_Width - 8;
		}
	}

	public override int Height
	{
		get
		{
			return base.m_Height;
		}
		set
		{
			base.m_Height = value;
			this.m_Total.Y = base.m_Height - 5 - this.m_Total.Image.yMax;
			this.m_ValueType.Y = base.m_Height - 5 - this.m_ValueType.Image.yMax;
			int num = this.m_Total.Y + this.m_Total.Image.yMin;
			if (this.m_ValueType.Y + this.m_ValueType.Image.yMin < num)
			{
				num = this.m_ValueType.Y + this.m_ValueType.Image.yMin;
			}
			this.m_SkillList.Height = num - 7;
		}
	}

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		base.BringToTop();
		if ((mb & MouseButtons.Right) != MouseButtons.None)
		{
			this.m_ShouldClose = true;
		}
	}

	protected internal override void OnMouseUp(int x, int y, MouseButtons mb)
	{
		if (this.m_ShouldClose && (mb & MouseButtons.Right) != MouseButtons.None)
		{
			base.ManualClose();
		}
		else
		{
			this.m_ShouldClose = false;
		}
	}

	protected internal override void OnMouseEnter(int x, int y, MouseButtons mb)
	{
		if (this.m_ShouldClose && (mb & MouseButtons.Right) == 0)
		{
			this.m_ShouldClose = false;
		}
	}

	protected internal override void OnDispose()
	{
		Engine.m_SkillsOpen = false;
		Engine.m_SkillsGump = null;
	}

	protected internal override void Render(int x, int y)
	{
		base.Render(x, y);
	}

	public GSkills()
		: base(50, 50, 250, 125)
	{
		base.m_Children.Add(new GVResizer(this));
		base.m_Children.Add(new GHResizer(this));
		base.m_Children.Add(new GLResizer(this));
		base.m_Children.Add(new GTResizer(this));
		base.m_Children.Add(new GHVResizer(this));
		base.m_Children.Add(new GLTResizer(this));
		base.m_Children.Add(new GHTResizer(this));
		base.m_Children.Add(new GLVResizer(this));
		this.m_ShowReal = false;
		this.m_Total = new GLabel($"Total: {this.GetTotalSkillCount():F1}", Engine.GetUniFont(1), Hues.Bright, 0, 0);
		base.m_Children.Add(this.m_Total);
		this.m_ValueType = new GTextButton("Used Values", Engine.GetUniFont(1), Hues.Bright, Hues.Load(53), 0, 0, ValueType_OnClick);
		this.m_ValueType.X = 4 - this.m_ValueType.Image.xMin;
		base.m_Children.Add(this.m_ValueType);
		this.m_SkillList = new GSkillList(this);
		base.m_Children.Add(this.m_SkillList);
		this.Width = 250;
		this.Height = 125;
	}

	private float GetTotalSkillCount()
	{
		float num = 0f;
		Skills skills = Engine.Skills;
		for (int i = 0; i < 256; i++)
		{
			Skill skill = skills[i];
			if (skill != null)
			{
				num += skill.Real;
				continue;
			}
			break;
		}
		return num;
	}

	private void ValueType_OnClick(Gump g)
	{
		this.m_ShowReal = !this.m_ShowReal;
		this.m_ValueType.Text = (this.m_ShowReal ? "Real Values" : "Used Values");
		this.m_ValueType.X = 4 - this.m_ValueType.Image.xMin;
		this.m_ValueType.Y = base.m_Height - 5 - this.m_ValueType.Image.yMax;
		this.m_SkillList.ShowReal = this.m_ShowReal;
	}

	public void OnSkillChange(Skill skill)
	{
		this.m_SkillList.OnSkillChange(skill, this.m_ShowReal);
		this.UpdateTotal();
	}

	private void UpdateTotal()
	{
		this.m_Total.Text = $"Total: {this.GetTotalSkillCount():F1}";
		this.m_Total.X = base.m_Width - 5 - this.m_Total.Image.xMax;
		this.m_Total.Y = base.m_Height - 5 - this.m_Total.Image.yMax;
		int num = this.m_Total.Y + this.m_Total.Image.yMin;
		if (this.m_ValueType.Y + this.m_ValueType.Image.yMin < num)
		{
			num = this.m_ValueType.Y + this.m_ValueType.Image.yMin;
		}
		this.m_SkillList.Height = num - 7;
	}
}
