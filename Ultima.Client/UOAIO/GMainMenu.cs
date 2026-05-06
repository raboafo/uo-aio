namespace UOAIO;

public class GMainMenu : Gump
{
	private bool m_LeftToRight;

	public bool LeftToRight
	{
		get
		{
			return this.m_LeftToRight;
		}
		set
		{
			this.m_LeftToRight = value;
			this.Layout();
		}
	}

	public override int Width
	{
		get
		{
			return this.m_LeftToRight ? (1 + base.m_Children.Count * 23) : 120;
		}
		set
		{
		}
	}

	public override int Height
	{
		get
		{
			return this.m_LeftToRight ? 24 : (1 + base.m_Children.Count * 119);
		}
		set
		{
		}
	}

	public GMainMenu(int x, int y)
		: base(x, y)
	{
		base.m_NonRestrictivePicking = true;
	}

	public void Add(GMenuItem child)
	{
		base.m_Children.Remove(child);
		if (this.m_LeftToRight)
		{
			child.X = base.m_Children.Count * 119;
			child.Y = 0;
		}
		else
		{
			child.X = 0;
			child.Y = base.m_Children.Count * 23;
		}
		base.m_Children.Add(child);
	}

	public void Remove(GMenuItem child)
	{
		base.m_Children.Remove(child);
		this.Layout();
	}

	public bool Contains(GMenuItem child)
	{
		return base.m_Children.IndexOf(child) >= 0;
	}

	public void Layout()
	{
		int num = 0;
		Gump[] array = base.m_Children.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is GMenuItem gMenuItem)
			{
				if (this.m_LeftToRight)
				{
					gMenuItem.X = num++ * 119;
					gMenuItem.Y = 0;
				}
				else
				{
					gMenuItem.X = 0;
					gMenuItem.Y = num++ * 23;
				}
			}
		}
	}
}
