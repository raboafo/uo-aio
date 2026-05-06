using System.Collections.Generic;

namespace UOAIO;

public class GBook : GBackground
{
	public BookPageInfo[] m_Pages = null;

	public GWrappedLabel m_Title;

	public GWrappedLabel m_Author;

	public GTextBox m_FirstPage;

	public List<GTextBox> m_LeftPages = new List<GTextBox>();

	public List<GTextBox> m_RightPages = new List<GTextBox>();

	private int m_LastLeftPage = -1;

	private int m_LastRightPage = -1;

	private int m_CurrentPage = 0;

	private string guid;

	protected internal override void OnDispose()
	{
		if (this.m_FirstPage != null)
		{
			Engine.Sounds.PlaySound(88);
		}
	}

	public GBook(int serial, string author, int pageCount, string title)
		: base(510, 420, 250, 25, 25, HasBorder: false)
	{
		this.guid = $"Book-{serial}";
		this.m_Title = this.CreateLabel(title);
		this.m_Author = this.CreateLabel(author);
		Gump gump = Gumps.FindGumpByGUID(this.guid);
		if (gump != null)
		{
			base.m_IsDragging = gump.m_IsDragging;
			base.m_OffsetX = gump.m_OffsetX;
			base.m_OffsetY = gump.m_OffsetY;
			if (Gumps.Drag == gump)
			{
				Gumps.Drag = this;
			}
			if (Gumps.LastOver == gump)
			{
				Gumps.LastOver = this;
			}
			if (Gumps.Focus == gump)
			{
				Gumps.Focus = this;
			}
			base.m_X = gump.X;
			base.m_Y = gump.Y;
			Gumps.Destroy(gump);
		}
		base.m_GUID = this.guid;
		base.m_CanDrag = true;
		base.m_QuickDrag = true;
		base.CanClose = true;
		this.m_Author.X = 40;
		this.m_Author.Y = 40;
		this.m_Title.X = this.m_Author.X;
		this.m_Title.Y = this.m_Author.Y + (this.m_Author.Height + 50);
		if (this.m_Title.Width > this.Width)
		{
			this.Width = this.m_Title.Width;
		}
		this.Width += this.Width - base.UseWidth;
		base.m_Children.Add(this.m_Author);
		base.m_Children.Add(this.m_Title);
		base.m_Children.Add(new NextPageBtn(this));
		base.m_Children.Add(new PrevPageBtn(this));
	}

	public GBook(int serial, string author, int pageCount, string title, BookPageInfo[] pages)
		: base(510, 420, 250, 25, 25, HasBorder: false)
	{
		this.guid = $"Book-{serial}";
		this.m_Title = this.CreateLabel(title);
		this.m_Author = this.CreateLabel(author);
		Gump gump = Gumps.FindGumpByGUID(this.guid);
		if (gump != null)
		{
			base.m_IsDragging = gump.m_IsDragging;
			base.m_OffsetX = gump.m_OffsetX;
			base.m_OffsetY = gump.m_OffsetY;
			if (Gumps.Drag == gump)
			{
				Gumps.Drag = this;
			}
			if (Gumps.LastOver == gump)
			{
				Gumps.LastOver = this;
			}
			if (Gumps.Focus == gump)
			{
				Gumps.Focus = this;
			}
			base.m_X = gump.X;
			base.m_Y = gump.Y;
			Gumps.Destroy(gump);
		}
		base.m_GUID = this.guid;
		base.m_CanDrag = true;
		base.m_QuickDrag = true;
		base.CanClose = true;
		this.m_Author.X = 40;
		this.m_Author.Y = 40;
		this.m_Title.X = this.m_Author.X;
		this.m_Title.Y = this.m_Author.Y + (this.m_Author.Height + 50);
		if (this.m_Title.Width > this.Width)
		{
			this.Width = this.m_Title.Width;
		}
		this.Width += this.Width - base.UseWidth;
		base.m_Children.Add(this.m_Author);
		base.m_Children.Add(this.m_Title);
		base.m_Children.Add(new GWrappedLabel((++this.m_CurrentPage).ToString(), Engine.DefaultFont, Hues.Load(1109), 300, 200, 25));
		base.m_Children.Add(new NextPageBtn(this));
		if (pages == null)
		{
			return;
		}
		for (int i = 0; i < pages.Length; i++)
		{
			if (i == 0)
			{
				this.m_FirstPage = new GTextBox(0, HasBorder: false, 230, 30, 400, 160, pages[i].GetAllLines(), Engine.GetFont(1), Hues.Load(1109), Hues.Load(1109), Hues.Load(1109));
			}
			else if (i % 2 == 0)
			{
				this.m_RightPages.Add(new GTextBox(0, HasBorder: false, 230, 30, 400, 160, pages[i].GetAllLines(), Engine.GetFont(1), Hues.Load(1109), Hues.Load(1109), Hues.Load(1109)));
			}
			else
			{
				this.m_LeftPages.Add(new GTextBox(0, HasBorder: false, 40, 30, 400, 160, pages[i].GetAllLines(), Engine.GetFont(1), Hues.Load(1109), Hues.Load(1109), Hues.Load(1109)));
			}
		}
		base.m_Children.Add(this.m_FirstPage);
	}

	public void NextPage_OnClick()
	{
		if (this.m_LastLeftPage + 1 > this.m_LeftPages.Count - 1)
		{
			return;
		}
		Engine.Sounds.PlaySound(85);
		Gump[] array = base.Children.ToArray();
		foreach (Gump gump in array)
		{
			if (gump is GTextBox || gump is GClickable || gump is GLabel)
			{
				base.Children.Remove(gump);
			}
		}
		base.Children.Add(new PrevPageBtn(this));
		GumpList children = base.Children;
		int num = ++this.m_CurrentPage;
		children.Add(new GWrappedLabel(num.ToString(), Engine.DefaultFont, Hues.Load(1109), 120, 200, 25));
		base.Children.Add(this.m_LeftPages[++this.m_LastLeftPage]);
		if (this.m_LastRightPage + 1 <= this.m_RightPages.Count - 1)
		{
			GumpList children2 = base.Children;
			num = ++this.m_CurrentPage;
			children2.Add(new GWrappedLabel(num.ToString(), Engine.DefaultFont, Hues.Load(1109), 300, 200, 25));
			base.Children.Add(this.m_RightPages[++this.m_LastRightPage]);
			base.Children.Add(new NextPageBtn(this));
		}
		else
		{
			this.m_LastRightPage++;
		}
	}

	public void PrevPage_OnClick()
	{
		if (this.m_LastLeftPage != 0)
		{
			Engine.Sounds.PlaySound(85);
			Gump[] array = base.Children.ToArray();
			foreach (Gump gump in array)
			{
				if (gump is GTextBox || gump is GClickable || gump is GLabel)
				{
					base.Children.Remove(gump);
				}
			}
			base.Children.Add(new NextPageBtn(this));
			GumpList children = base.Children;
			int num = --this.m_CurrentPage;
			children.Add(new GWrappedLabel(num.ToString(), Engine.DefaultFont, Hues.Load(1109), 300, 200, 25));
			base.Children.Add(this.m_LeftPages[--this.m_LastLeftPage]);
			Engine.AddTextMessage(this.m_LastLeftPage.ToString());
			if (this.m_LastRightPage != 0)
			{
				GumpList children2 = base.Children;
				num = --this.m_CurrentPage;
				children2.Add(new GWrappedLabel(num.ToString(), Engine.DefaultFont, Hues.Load(1109), 120, 200, 25));
				base.Children.Add(this.m_RightPages[--this.m_LastRightPage]);
				Engine.AddTextMessage(this.m_LastRightPage.ToString());
				base.Children.Add(new PrevPageBtn(this));
			}
			return;
		}
		Gump[] array2 = base.Children.ToArray();
		foreach (Gump gump2 in array2)
		{
			if (gump2 is GTextBox || gump2 is GClickable || gump2 is GLabel)
			{
				base.Children.Remove(gump2);
			}
		}
		base.Children.Add(this.m_Title);
		base.Children.Add(this.m_Author);
		base.Children.Add(this.m_FirstPage);
		base.Children.Add(new NextPageBtn(this));
		if (this.m_LastLeftPage != -1)
		{
			Engine.Sounds.PlaySound(85);
		}
		this.m_LastLeftPage = -1;
		this.m_LastRightPage = -1;
		this.m_CurrentPage = 1;
		base.Children.Add(new GWrappedLabel(this.m_CurrentPage.ToString(), Engine.DefaultFont, Hues.Load(1109), 300, 200, 25));
	}

	private GWrappedLabel CreateLabel(string text)
	{
		text = text.Replace('\r', '\n');
		GWrappedLabel gWrappedLabel = new GWrappedLabel(text, Engine.GetFont(1), Hues.Load(1109), 0, 20, 160);
		gWrappedLabel.Height = 20;
		base.Children.Add(gWrappedLabel);
		gWrappedLabel.Center();
		return gWrappedLabel;
	}
}
