namespace UOAIO;

public class GComboBox : GBackground
{
	protected IFont m_Font;

	protected IHue m_HRegular;

	protected IHue m_HOver;

	protected int m_Index = -1;

	protected int m_Count;

	protected int m_BackID;

	protected GLabel m_Text;

	protected string[] m_List;

	private GBackground m_Dropdown;

	private GButton m_Button;

	public string[] List
	{
		get
		{
			return this.m_List;
		}
		set
		{
			this.m_List = value;
			this.m_Count = this.m_List.Length;
		}
	}

	public int Index
	{
		get
		{
			return this.m_Index;
		}
		set
		{
			if (value < 0 || value >= this.m_Count)
			{
				this.m_Index = -1;
				if (this.m_Text != null)
				{
					this.m_Text.Text = "";
				}
				return;
			}
			this.m_Index = value;
			if (this.m_Text != null)
			{
				this.m_Text.Text = this.m_List[this.m_Index];
				this.m_Text.Y = base.OffsetY + base.UseHeight - this.m_Text.Image.yMax - 2;
				return;
			}
			this.m_Text = new GLabel(this.m_List[this.m_Index], this.m_Font, this.m_HRegular, base.OffsetX, base.OffsetY);
			this.m_Text.Scissor(0, 0, this.m_Button.X - base.OffsetX - 2, this.m_Text.Height);
			this.m_Text.Y = base.OffsetY + base.UseHeight - this.m_Text.Image.yMax - 2;
			base.m_Children.Add(this.m_Text);
		}
	}

	private void SetIndex_OnClick(Gump g)
	{
		this.Index = (int)g.GetTag("Index");
		Gumps.Destroy(this.m_Dropdown);
	}

	private void OpenList_OnClick(Gump g)
	{
		if (this.m_Dropdown != null)
		{
			Gumps.Destroy(this.m_Dropdown);
		}
		Point point = base.PointToScreen(new Point(0, 0));
		this.m_Dropdown = new GBackground(this.m_BackID, this.Width, this.m_Count * 20 + (this.Height - base.UseHeight), point.X, point.Y, HasBorder: true);
		this.m_Dropdown.DestroyOnUnfocus = true;
		int num = this.m_Dropdown.OffsetY;
		int num2 = 0;
		for (int i = 0; i < this.m_Count; i++)
		{
			GTextButton gTextButton = new GTextButton(this.m_List[i], this.m_Font, this.m_HRegular, this.m_HOver, this.m_Dropdown.OffsetX, num, SetIndex_OnClick);
			gTextButton.SetTag("Index", i);
			this.m_Dropdown.Children.Add(gTextButton);
			num += gTextButton.Height;
			if (gTextButton.Width + 3 > num2)
			{
				num2 = gTextButton.Width + 3;
			}
		}
		this.m_Dropdown.Height = num + (this.m_Dropdown.Height - (this.m_Dropdown.OffsetY + this.m_Dropdown.UseHeight));
		num2 += this.m_Dropdown.Width - this.m_Dropdown.UseWidth;
		if (num2 > this.m_Dropdown.Width)
		{
			this.m_Dropdown.Width = num2;
		}
		Gumps.Desktop.Children.Add(this.m_Dropdown);
	}

	public GComboBox(string[] List, int Index, int BackID, int NormalID, int OverID, int PressedID, int X, int Y, int Width, int Height, IFont Font, IHue HRegular, IHue HOver)
		: base(BackID, Width, Height, X, Y, HasBorder: true)
	{
		this.m_BackID = BackID;
		this.m_Font = Font;
		this.m_HRegular = HRegular;
		this.m_HOver = HOver;
		this.m_Button = new GButton(NormalID, OverID, PressedID, 0, 0, OpenList_OnClick);
		base.m_Children.Add(this.m_Button);
		this.m_Button.Center();
		this.m_Button.X = base.OffsetX + base.UseWidth - this.m_Button.Width - 1;
		GButton button = this.m_Button;
		int y = button.Y + 1;
		button.Y = y;
		this.List = List;
		this.Index = Index;
	}
}
