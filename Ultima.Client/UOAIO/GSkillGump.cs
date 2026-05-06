using System.Windows.Forms;

namespace UOAIO;

public class GSkillGump : Gump
{
	private class GUsableSkillName : GTextButton
	{
		private Skill m_Skill;

		public GUsableSkillName(Skill skill)
			: base(skill.Name, Engine.GetUniFont(1), Hues.Bright, Hues.Load(53), 20, 0, null)
		{
			this.m_Skill = skill;
			base.m_CanDrag = true;
			base.m_QuickDrag = false;
		}

		protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
		{
			base.m_Parent.BringToTop();
		}

		protected internal override void OnMouseUp(int x, int y, MouseButtons mb)
		{
			base.m_Parent.BringToTop();
			if ((mb & MouseButtons.Left) != MouseButtons.None)
			{
				this.m_Skill.Use();
			}
		}

		protected internal override void OnDragStart()
		{
			base.m_IsDragging = false;
			Gumps.Drag = null;
			GSkillIcon gSkillIcon = new GSkillIcon(this.m_Skill);
			gSkillIcon.m_IsDragging = true;
			gSkillIcon.m_OffsetX = gSkillIcon.Width / 2;
			gSkillIcon.m_OffsetY = gSkillIcon.Height / 2;
			gSkillIcon.X = Engine.m_xMouse - gSkillIcon.m_OffsetX;
			gSkillIcon.Y = Engine.m_yMouse - gSkillIcon.m_OffsetY;
			Gumps.Desktop.Children.Add(gSkillIcon);
			Gumps.Drag = gSkillIcon;
		}
	}

	private Skill m_Skill;

	private GLabel m_Name;

	private GLabel m_Value;

	private GThreeToggle m_Lock;

	private int m_Width;

	private int m_Height;

	private int m_yBase;

	public int yBase => this.m_yBase;

	public override int Width
	{
		get
		{
			return this.m_Width;
		}
		set
		{
			this.m_Width = value;
			this.m_Value.X = this.m_Width - 24 - this.m_Value.Image.xMax;
			this.m_Lock.X = this.m_Width - 4 - 8 - this.m_Lock.Width / 2;
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

	public void Scissor(Clipper c)
	{
		this.m_Name.Scissor(c);
		this.m_Value.Scissor(c);
		this.m_Lock.Scissor(c);
	}

	public GSkillGump(Skill skill, int y, int width, bool showReal)
		: base(4, y)
	{
		this.m_yBase = y;
		this.m_Skill = skill;
		if (skill.Action)
		{
			this.m_Name = new GUsableSkillName(skill);
		}
		else
		{
			this.m_Name = new GLabel(skill.Name, Engine.GetUniFont(1), Hues.Bright, 20, 0);
		}
		this.m_Name.X -= this.m_Name.Image.xMin;
		this.m_Name.Y -= this.m_Name.Image.yMin;
		this.m_Width = width;
		this.m_Height = this.m_Name.Image.yMax - this.m_Name.Image.yMin;
		base.m_Children.Add(this.m_Name);
		this.m_Value = new GLabel((showReal ? this.m_Skill.Real : this.m_Skill.Value).ToString("F1"), Engine.GetUniFont(1), Hues.Bright, 0, 0);
		this.m_Value.X = this.m_Width - 24 - this.m_Value.Image.xMax;
		this.m_Value.Y -= this.m_Value.Image.yMin;
		if (this.m_Value.Image.yMax - this.m_Value.Image.yMin > this.m_Height)
		{
			this.m_Height = this.m_Value.Image.yMax - this.m_Value.Image.yMin;
		}
		base.m_Children.Add(this.m_Value);
		this.m_Lock = new GThreeToggle(Engine.m_SkillUp, Engine.m_SkillDown, Engine.m_SkillLocked, (int)this.m_Skill.Lock, this.m_Width - 4, 0);
		this.m_Lock.OnStateChange = Lock_OnStateChange;
		this.UpdateLock();
		base.m_Children.Add(this.m_Lock);
	}

	private void Lock_OnStateChange(int state, Gump g)
	{
		if (this.m_Skill.Lock != (SkillLock)state)
		{
			this.m_Skill.Lock = (SkillLock)state;
			Network.Send(new PUpdateSkillLock(this.m_Skill));
		}
	}

	private void UpdateLock()
	{
		this.m_Lock.X = this.m_Width - 4 - 8 - this.m_Lock.Width / 2;
		this.m_Lock.Y = (this.m_Height - this.m_Lock.Height) / 2;
	}

	public void OnSkillChange(double newValue, SkillLock newLock)
	{
		this.m_Value.Text = newValue.ToString("F1");
		this.m_Value.X = this.m_Width - 24 - this.m_Value.Image.xMax;
		this.m_Value.Y = -this.m_Value.Image.yMin;
		this.m_Lock.State = (int)newLock;
		this.UpdateLock();
	}
}
