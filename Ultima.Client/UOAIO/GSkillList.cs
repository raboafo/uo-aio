using System;

namespace UOAIO;

public class GSkillList : GSingleBorder
{
	private GAlphaVSlider m_Slider;

	private GSingleBorder m_SliderBorder;

	private GHotspot m_Hotspot;

	private int m_xLast;

	private int m_yLast;

	private int m_xWidthLast;

	private int m_yHeightLast;

	private GSkills m_Owner;

	private GSkillGump[] m_SkillGumps;

	private static Type tGLabel;

	private static Type tGSkillGump;

	public bool ShowReal
	{
		set
		{
			Skills skills = Engine.Skills;
			if (!value)
			{
				for (int i = 0; i < this.m_SkillGumps.Length && this.m_SkillGumps[i] != null; i++)
				{
					Skill skill = skills[i];
					this.m_SkillGumps[i].OnSkillChange(skill.Value, skill.Lock);
				}
			}
			else
			{
				for (int j = 0; j < this.m_SkillGumps.Length && this.m_SkillGumps[j] != null; j++)
				{
					Skill skill2 = skills[j];
					this.m_SkillGumps[j].OnSkillChange(skill2.Real, skill2.Lock);
				}
			}
		}
	}

	public override int Width
	{
		get
		{
			return base.m_Width;
		}
		set
		{
			base.m_Width = value;
			this.m_Slider.X = base.m_Width - 15;
			this.m_SliderBorder.X = base.m_Width - 16;
			this.m_Hotspot.X = base.m_Width - 16;
			for (int i = 0; i < this.m_SkillGumps.Length && this.m_SkillGumps[i] != null; i++)
			{
				this.m_SkillGumps[i].Width = base.m_Width - 20;
			}
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
			double value2 = this.m_Slider.GetValue();
			this.m_Slider.Height = base.m_Height - 11;
			this.m_Slider.SetValue(value2, CallOnChange: true);
			this.m_SliderBorder.Height = base.m_Height;
			this.m_Hotspot.Height = base.m_Height;
		}
	}

	public GSkillList(GSkills owner)
		: base(4, 4, 250, 50)
	{
		this.m_Owner = owner;
		base.m_CanDrag = false;
		Skills skills = Engine.Skills;
		this.m_SkillGumps = new GSkillGump[256];
		int num = 4;
		for (int i = 0; i < skills.Groups.Length; i++)
		{
			SkillGroup skillGroup = skills.Groups[i];
			GLabel gLabel = new GLabel(skillGroup.Name, Engine.GetUniFont(1), Hues.Bright, 4, num);
			gLabel.X -= gLabel.Image.xMin;
			gLabel.Y -= gLabel.Image.yMin;
			gLabel.SetTag("yBase", gLabel.Y);
			base.m_Children.Add(gLabel);
			num += 4 + (gLabel.Image.yMax - gLabel.Image.yMin);
			for (int j = 0; j < skillGroup.Skills.Count; j++)
			{
				Skill skill = skillGroup.Skills[j];
				GSkillGump gSkillGump = new GSkillGump(skill, num, base.m_Width - 20, this.m_Owner.ShowReal);
				this.m_SkillGumps[skill.ID] = gSkillGump;
				base.m_Children.Add(gSkillGump);
				num += 4 + gSkillGump.Height;
			}
		}
		this.m_SliderBorder = new GSingleBorder(0, 0, 16, 100);
		base.m_Children.Add(this.m_SliderBorder);
		this.m_Slider = new GAlphaVSlider(0, 6, 16, 100, 0.0, 0.0, num + 1, 1.0);
		this.m_Slider.SetTag("Max", num + 1);
		GAlphaVSlider slider = this.m_Slider;
		slider.OnValueChange = (OnValueChange)Delegate.Combine(slider.OnValueChange, new OnValueChange(Slider_OnValueChange));
		base.m_Children.Add(this.m_Slider);
		this.m_Hotspot = new GHotspot(0, 0, 16, 100, this.m_Slider);
		base.m_Children.Add(this.m_Hotspot);
	}

	private void Slider_OnValueChange(double vNew, double vOld, Gump sender)
	{
		int num = (int)vNew;
		double num2 = (int)sender.GetTag("Max");
		num2 = (double)num / num2;
		num = (int)(0.0 - (double)((int)sender.GetTag("Max") - base.m_Height) * num2);
		Gump[] array = base.m_Children.ToArray();
		foreach (Gump gump in array)
		{
			Type type = gump.GetType();
			if (type == GSkillList.tGLabel)
			{
				gump.Y = num + (int)gump.GetTag("yBase");
			}
			else if (type == GSkillList.tGSkillGump)
			{
				gump.Y = num + ((GSkillGump)gump).yBase;
			}
		}
	}

	public void OnSkillChange(Skill skill, bool showReal)
	{
		this.m_SkillGumps[skill.ID]?.OnSkillChange(showReal ? skill.Real : skill.Value, skill.Lock);
	}

	protected internal override void Draw(int x, int y)
	{
		if (this.m_xLast != x || this.m_yLast != y || this.m_xWidthLast != base.m_Width || this.m_yHeightLast != base.m_Height)
		{
			Clipper clipper = new Clipper(x + 1, y + 1, base.m_Width - 17, base.m_Height - 2);
			Gump[] array = base.m_Children.ToArray();
			foreach (Gump gump in array)
			{
				Type type = gump.GetType();
				if (type == GSkillList.tGLabel)
				{
					((GLabel)gump).Scissor(clipper);
				}
				else if (type == GSkillList.tGSkillGump)
				{
					((GSkillGump)gump).Scissor(clipper);
				}
			}
			this.m_xLast = x;
			this.m_yLast = y;
			this.m_xWidthLast = base.m_Width;
			this.m_yHeightLast = base.m_Height;
		}
		base.Draw(x, y);
	}

	static GSkillList()
	{
		GSkillList.tGLabel = typeof(GLabel);
		GSkillList.tGSkillGump = typeof(GSkillGump);
	}
}
