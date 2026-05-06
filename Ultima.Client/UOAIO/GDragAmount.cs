using System;

namespace UOAIO;

public class GDragAmount : GDragable
{
	private Item m_Item;

	private int m_Amount;

	private GButtonNew m_Okay;

	private GSlider m_Slider;

	private GTextBox m_TextBox;

	private object m_ToDestroy;

	private bool m_First = true;

	public object ToDestroy
	{
		get
		{
			return this.m_ToDestroy;
		}
		set
		{
			this.m_ToDestroy = value;
		}
	}

	public Item Item => this.m_Item;

	public GDragAmount(Item item)
		: base(2140, 0, 0)
	{
		this.m_Item = item;
		int num = (this.m_Amount = (ushort)this.m_Item.Amount);
		this.m_Okay = new GButtonNew(2074, 2076, 2075, 102, 37);
		this.m_Okay.CanEnter = true;
		this.m_Okay.Clicked += Okay_Clicked;
		base.m_Children.Add(this.m_Okay);
		GSlider gSlider = new GSlider(2117, 35, 16, 95, 15, num, 0.0, num, 1.0)
		{
			OnValueChange = Slider_OnValueChange
		};
		base.m_Children.Add(gSlider);
		this.m_Slider = gSlider;
		GHotspot toAdd = new GHotspot(28, 16, 109, 15, gSlider);
		base.m_Children.Add(toAdd);
		GTextBox gTextBox = new GTextBox(0, HasBorder: false, 26, 43, 66, 15, num.ToString(), Engine.GetFont(1), Hues.Load(1109), Hues.Load(1109), Hues.Load(1109));
		gTextBox.OnTextChange = (OnTextChange)Delegate.Combine(gTextBox.OnTextChange, new OnTextChange(TextBox_OnTextChange));
		gTextBox.OnBeforeTextChange = (OnBeforeTextChange)Delegate.Combine(gTextBox.OnBeforeTextChange, new OnBeforeTextChange(TextBox_OnBeforeTextChange));
		gTextBox.EnterButton = this.m_Okay;
		base.m_Children.Add(gTextBox);
		this.m_TextBox = gTextBox;
		gTextBox.Focus();
		base.m_IsDragging = true;
		base.m_OffsetX = this.Width / 2;
		base.m_OffsetY = this.Height / 2;
		Gumps.LastOver = this;
		Gumps.Drag = this;
		Gumps.Focus = this;
		base.m_X = Engine.m_xMouse - base.m_OffsetX;
		base.m_Y = Engine.m_yMouse - base.m_OffsetY;
	}

	private void Okay_Clicked(object sender, EventArgs e)
	{
		try
		{
			int num = Convert.ToInt32(this.m_TextBox.String);
			if (num <= 0)
			{
				Gumps.Destroy(this);
				return;
			}
			if (num > this.m_Amount)
			{
				num = this.m_Amount;
			}
			base.m_IsDragging = false;
			this.m_Item.Amount = num;
			Network.Send(new PPickupItem(this.m_Item, this.m_Item.Amount));
			Gumps.Desktop.Children.Add(new GDraggedItem(this.m_Item));
			if (this.m_ToDestroy is Gump)
			{
				if (((Gump)this.m_ToDestroy).Parent is GContainer)
				{
					((GContainer)((Gump)this.m_ToDestroy).Parent).m_Hash[this.m_Item] = null;
				}
				Gumps.Destroy((Gump)this.m_ToDestroy);
			}
			else if (this.m_ToDestroy is Item)
			{
				Item item = (Item)this.m_ToDestroy;
				item.RestoreInfo = new RestoreInfo(item);
				World.Remove(item);
			}
			Gumps.Destroy(this);
		}
		catch
		{
		}
	}

	private void Slider_OnValueChange(double v, double old, Gump g)
	{
		this.m_TextBox.String = ((int)v).ToString();
	}

	private void TextBox_OnBeforeTextChange(Gump g)
	{
		if (this.m_First)
		{
			this.m_First = false;
			((GTextBox)g).String = "";
		}
	}

	private void TextBox_OnTextChange(string text, Gump g)
	{
		try
		{
			int num = Convert.ToInt32(text);
			if (num < 0)
			{
				this.m_Slider.SetValue(0.0, CallOnChange: true);
			}
			else if (num > this.m_Amount)
			{
				this.m_Slider.SetValue(this.m_Amount, CallOnChange: true);
			}
			else
			{
				this.m_Slider.SetValue(num, CallOnChange: false);
			}
		}
		catch
		{
		}
	}
}
