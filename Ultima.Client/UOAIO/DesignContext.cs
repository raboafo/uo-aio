using UOAIO.Targeting;

namespace UOAIO;

public class DesignContext
{
	private static DesignContext m_Current;

	private Item m_House;

	private DesignerGump m_Designer;

	private DesignerEntry m_Entry;

	private bool m_Dragging;

	private int m_DragStartX;

	private int m_DragStartY;

	private DesignerID[] m_DesignerIDs = new DesignerID[9];

	public static DesignContext Current
	{
		get
		{
			return DesignContext.m_Current;
		}
		set
		{
			if (DesignContext.m_Current != value)
			{
				if (DesignContext.m_Current != null)
				{
					Gumps.Destroy(DesignContext.m_Current.Designer);
				}
				if (TargetManager.Client is DesignerDeleteTarget)
				{
					TargetManager.Client = null;
				}
				DesignContext.m_Current = value;
				Map.Invalidate();
			}
		}
	}

	public bool Dragging => this.m_Dragging;

	public int DragStartX => this.m_DragStartX;

	public int DragStartY => this.m_DragStartY;

	public DesignerEntry Entry
	{
		get
		{
			return this.m_Entry;
		}
		set
		{
			this.m_Entry = value;
		}
	}

	public Multi Multi
	{
		get
		{
			CustomMultiEntry customMulti = CustomMultiLoader.GetCustomMulti(this.m_House.Serial, this.m_House.Revision);
			if (customMulti != null)
			{
				return customMulti.Multi;
			}
			return this.m_House.Multi;
		}
	}

	public Item House => this.m_House;

	public DesignerGump Designer => this.m_Designer;

	public DesignContext(Item house)
	{
		this.m_House = house;
		this.m_Designer = new DesignerGump(this);
		Gumps.Desktop.Children.Add(this.m_Designer);
	}

	public bool ComputeTilePosition(ref int x, ref int y)
	{
		Mobile player = World.Player;
		int num = Engine.GameX + Engine.GameWidth / 2;
		int num2 = Engine.GameY + Engine.GameHeight / 2;
		x -= num;
		y -= num2;
		y += 26;
		int num3 = x + y;
		int num4 = y - x;
		if (num3 < 0)
		{
			num3 -= 44;
		}
		if (num4 < 0)
		{
			num4 -= 44;
		}
		num3 /= 44;
		num4 /= 44;
		x = num3;
		y = num4;
		x += player.X;
		y += player.Y;
		x -= this.m_House.X;
		y -= this.m_House.Y;
		Multi multi = this.Multi;
		multi.GetBounds(out var xMin, out var yMin, out var xMax, out var yMax);
		bool result = x >= xMin && y >= yMin && x <= xMax && y <= yMax;
		if (x < xMin)
		{
			x = xMin;
		}
		else if (x > xMax)
		{
			x = xMax;
		}
		if (y < yMin)
		{
			y = yMin;
		}
		else if (y > yMax)
		{
			y = yMax;
		}
		return result;
	}

	public void BeginDrag(int x, int y)
	{
		this.m_Dragging = true;
		this.m_DragStartX = x;
		this.m_DragStartY = y;
	}

	public void FinishDrag(int x, int y)
	{
		int num = this.m_DragStartX;
		int num2 = this.m_DragStartY;
		int num3 = x;
		int num4 = y;
		if (num3 < num)
		{
			x = num3;
			num3 = num;
			num = x;
		}
		if (num4 < num2)
		{
			y = num4;
			num4 = num2;
			num2 = y;
		}
		this.EndDrag();
		if (this.m_Entry == null || this.m_Entry.GetMultiCursor() != null)
		{
			return;
		}
		this.m_Entry.FillCursor(this.m_DesignerIDs);
		Multi multi = this.Multi;
		int num5 = num3 - num + 1;
		int num6 = num4 - num2 + 1;
		int z = 7 + (this.GetCurrentLevel() - 1) * 20;
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				int num9;
				if (num5 >= 2 && num6 >= 2)
				{
					int num7 = i - num;
					int num8 = j - num2;
					bool flag = num8 == 0;
					bool flag2 = num7 == 0;
					bool flag3 = num8 == num6 - 1;
					bool flag4 = num7 == num5 - 1;
					num9 = ((flag && flag2) ? 1 : ((!(flag && flag4)) ? ((!(flag3 && flag4)) ? ((!(flag3 && flag2)) ? ((!flag) ? ((!flag4) ? ((!flag3) ? (flag2 ? 8 : 0) : 6) : 4) : 2) : 7) : 5) : 3));
				}
				else
				{
					num9 = 0;
				}
				int itemID = this.m_DesignerIDs[num9];
				multi.Add(itemID, i, j, z);
				Network.Send(new PDesigner_Build(this.m_House, i, j, itemID));
			}
		}
		Map.Invalidate();
	}

	public void EndDrag()
	{
		this.m_Dragging = false;
	}

	public void NormalizeToCell(ref int x, ref int y, int baseCellX, int baseCellY)
	{
		x += this.m_House.X - baseCellX;
		y += this.m_House.Y - baseCellY;
	}

	public int GetCurrentLevel()
	{
		Mobile player = World.Player;
		if (player == null)
		{
			return 0;
		}
		int num = 1 + (player.Z - this.m_House.Z - 7) / 20;
		if (num < 1)
		{
			num = 1;
		}
		else if (num > 4)
		{
			num = 4;
		}
		return num;
	}
}
