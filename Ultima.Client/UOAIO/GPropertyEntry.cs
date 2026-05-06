using System;
using System.Reflection;
using System.Windows.Forms;
using UOAIO.Prompts;
using UOAIO.Targeting;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class GPropertyEntry : GEmpty
{
	private object m_Object;

	private ObjectEditorEntry m_Entry;

	private GAlphaBackground m_NameBack;

	private GAlphaBackground m_ValueBack;

	private GLabel m_Name;

	private GLabel m_Value;

	private GAlphaBackground m_Hue;

	private GPropertyHuePicker m_Picker;

	private Clipper m_Clipper;

	public ObjectEditorEntry Entry => this.m_Entry;

	public object Object => this.m_Object;

	public void Reset()
	{
		if (this.m_Picker != null)
		{
			Gumps.Destroy(this.m_Picker);
		}
		this.m_Picker = null;
	}

	public void SetClipper(Clipper c)
	{
		this.m_Clipper = c;
		this.m_Name.Clipper = c;
		if (this.m_Value != null)
		{
			this.m_Value.Clipper = c;
		}
		this.m_NameBack.Clipper = c;
		this.m_ValueBack.Clipper = c;
		if (this.m_Hue != null)
		{
			this.m_Hue.Clipper = c;
		}
	}

	protected internal override bool HitTest(int X, int Y)
	{
		Point p = base.PointToScreen(new Point(X, Y));
		return this.m_Clipper == null || this.m_Clipper.Evaluate(p);
	}

	protected internal override void OnMouseEnter(int X, int Y, MouseButtons mb)
	{
		this.m_NameBack.FillColor = GumpPaint.Blend(GumpColors.Window, GumpColors.Highlight, 0.9f);
		this.m_ValueBack.FillColor = this.m_NameBack.FillColor;
	}

	protected internal override void OnMouseLeave()
	{
		this.m_NameBack.FillColor = GumpColors.Window;
		this.m_ValueBack.FillColor = this.m_NameBack.FillColor;
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if (mb != MouseButtons.Left)
		{
			return;
		}
		if (base.Parent.Parent is GEditorPanel)
		{
			((GEditorPanel)base.Parent.Parent).Reset();
		}
		if (this.m_Entry.Property.PropertyType == typeof(Volume))
		{
			return;
		}
		object value = this.m_Entry.Property.GetValue(this.m_Object, null);
		if (value is bool)
		{
			this.SetValue(!(bool)value);
		}
		else if (value is int)
		{
			Engine.Prompt = new ConfigurationPrompt(this);
		}
		else if (value is Item || this.m_Entry.Property.PropertyType == typeof(Item))
		{
			TargetManager.Client = new SetItemPropertyTarget(this);
		}
		else if (value is Enum)
		{
			Array values = Enum.GetValues(value.GetType());
			for (int i = 0; i < values.Length; i++)
			{
				if (values.GetValue(i).Equals(value))
				{
					this.SetValue(values.GetValue((i + 1) % values.Length));
					break;
				}
			}
		}
		else if (this.m_Entry.Hue != null)
		{
			if (this.m_Picker == null)
			{
				GPropertyHuePicker gPropertyHuePicker = (this.m_Picker = new GPropertyHuePicker(this));
				gPropertyHuePicker.X = this.Width - 1;
				gPropertyHuePicker.Y = 0;
				base.m_Children.Add(gPropertyHuePicker);
			}
		}
		else if (this.m_Entry.Property.IsDefined(typeof(MacroEditorAttribute), inherit: true))
		{
			Gumps.Destroy(base.Parent.Parent.Parent.Parent);
			GMacroEditorForm.Open();
		}
		else if (this.m_Entry.Property.IsDefined(typeof(RenderSettingEditor), inherit: true))
		{
			Gumps.Destroy(base.Parent.Parent.Parent.Parent);
			GRenderSettingEditorForm.Open();
		}
	}

	public void SetValue(object val)
	{
		this.m_Entry.Property.SetValue(this.m_Object, val, null);
		if (this.m_Value != null)
		{
			IFont font = ((!((val is ValueType) ? val.Equals(this.m_Entry.Optionable.Default) : (val == this.m_Entry.Optionable.Default))) ? Engine.GetUniFont(1) : Engine.GetUniFont(2));
			if (this.m_Hue == null)
			{
				this.m_Value.Text = this.GetValString(val);
			}
			else
			{
				this.m_Hue.FillColor = Engine.C16232(Hues.Load((int)val).Pixel(ushort.MaxValue));
			}
			this.m_Value.Font = font;
		}
	}

	private string GetValString(object val)
	{
		if (val == null)
		{
			return "null";
		}
		if (val is bool)
		{
			return ((bool)val) ? "On" : "Off";
		}
		if (val is Item)
		{
			return Localization.GetString(1020000 + ((Item)val).ID);
		}
		return val.ToString();
	}

	public GPropertyEntry(object obj, ObjectEditorEntry entry)
		: base(0, 0, 279, 22)
	{
		this.m_Object = obj;
		this.m_Entry = entry;
		base.m_NonRestrictivePicking = true;
		bool flag = entry.Property.PropertyType == typeof(Volume);
		this.m_NameBack = new GAlphaBackground(0, 0, 140, 22);
		this.m_NameBack.FillColor = GumpColors.Window;
		this.m_NameBack.FillAlpha = 1f;
		this.m_NameBack.DrawBorder = false;
		this.m_NameBack.ShouldHitTest = false;
		base.m_Children.Add(this.m_NameBack);
		this.m_ValueBack = new GAlphaBackground(139, 0, 140, 22);
		this.m_ValueBack.FillColor = GumpColors.Window;
		this.m_ValueBack.FillAlpha = 1f;
		this.m_ValueBack.BorderColor = GumpColors.Control;
		this.m_ValueBack.ShouldHitTest = false;
		this.m_ValueBack.DrawBorder = false;
		base.m_Children.Add(this.m_ValueBack);
		this.m_Name = new GLabel(entry.Optionable.Name, Engine.GetUniFont(2), GumpHues.WindowText, 0, 0);
		this.m_Name.X = 5 - this.m_Name.Image.xMin;
		this.m_Name.Y = (22 - (this.m_Name.Image.yMax - this.m_Name.Image.yMin + 1)) / 2 - this.m_Name.Image.yMin;
		this.m_NameBack.Children.Add(this.m_Name);
		object value = entry.Property.GetValue(obj, null);
		string valString = this.GetValString(value);
		if (!flag)
		{
			this.m_Value = new GLabel(valString, (!((value is ValueType) ? value.Equals(entry.Optionable.Default) : (value == entry.Optionable.Default))) ? Engine.GetUniFont(1) : Engine.GetUniFont(2), GumpHues.WindowText, 0, 0);
			if (entry.Hue != null)
			{
				GAlphaBackground gAlphaBackground = new GAlphaBackground(4, 4, 22, 14)
				{
					FillColor = Engine.C16232(Hues.Load((int)value).Pixel(ushort.MaxValue)),
					FillAlpha = 1f,
					ShouldHitTest = false
				};
				this.m_ValueBack.Children.Add(gAlphaBackground);
				this.m_Value.Text = "Hue";
				this.m_Value.X = 30 - this.m_Value.Image.xMin;
				this.m_Value.Y = (22 - (this.m_Value.Image.yMax - this.m_Value.Image.yMin + 1)) / 2 - this.m_Value.Image.yMin;
				this.m_Hue = gAlphaBackground;
			}
			else
			{
				this.m_Value.X = 5 - this.m_Value.Image.xMin;
				this.m_Value.Y = (22 - (this.m_Value.Image.yMax - this.m_Value.Image.yMin + 1)) / 2 - this.m_Value.Image.yMin;
			}
			this.m_ValueBack.Children.Add(this.m_Value);
		}
	}
}
