using System.Collections.Generic;
using System.Windows.Forms;
using Ultima.Data;

namespace UOAIO;

public class GPaperdoll : GDragable, IContainerView, IAgentView
{
	private Mobile m_Mobile;

	private Item _dragPreview;

	private PaperdollBody _body;

	private List<GImage> _bodyImages;

	private bool _allowItemLift;

	private PaperdollMode _mode = (PaperdollMode)(-1);

	private GLabel _titleLabel;

	public PaperdollBody Body
	{
		get
		{
			return this._body;
		}
		set
		{
			if (this._body != value)
			{
				foreach (GImage bodyImage in this._bodyImages)
				{
					Gumps.Destroy(bodyImage);
				}
				this._bodyImages.Clear();
				this._body = value;
				if (this._body != null)
				{
					for (int i = 0; i < this._body.Images.Length; i++)
					{
						PaperdollImage paperdollImage = this._body.Images[i];
						GImage gImage = new GImage(8, 19);
						gImage.GumpID = paperdollImage.GumpId;
						gImage.Hue = Hues.GetMobileHue(paperdollImage.HueId ?? this.m_Mobile.Hue);
						gImage.Alpha = paperdollImage.Alpha;
						this._bodyImages.Add(gImage);
						base.Children.Insert(i, gImage);
					}
				}
			}
			else if (this._body != null)
			{
				for (int j = 0; j < this._body.Images.Length; j++)
				{
					PaperdollImage paperdollImage2 = this._body.Images[j];
					GImage gImage2 = this._bodyImages[j];
					gImage2.Hue = Hues.GetMobileHue(paperdollImage2.HueId ?? this.m_Mobile.Hue);
					this._bodyImages.Add(gImage2);
				}
			}
		}
	}

	public PaperdollMode Mode
	{
		get
		{
			return this._mode;
		}
		set
		{
			if (this._mode == value)
			{
			}
		}
	}

	public string Title
	{
		get
		{
			return this._titleLabel.Text;
		}
		set
		{
			this._titleLabel.Text = value;
		}
	}

	protected internal override void OnDispose()
	{
		if (this.m_Mobile.ContainerView == this)
		{
			this.m_Mobile.SetContainerView(null);
		}
	}

	public GPaperdoll(Mobile m, int ID, int X, int Y, bool allowItemLift)
		: base(m.Player ? 2000 : 2001, X, Y)
	{
		this.m_Mobile = m;
		base.m_CanDrop = true;
		this._allowItemLift = allowItemLift;
		this._bodyImages = new List<GImage>();
		base.Children.Add(new GVirtueTrigger(m));
		if (m == World.Player)
		{
			base.Children.Add(new GPartyScroll(m));
		}
		base.Children.Add(new GProfileScroll(m));
		base.Children.Add(this._titleLabel = new GWrappedLabel("", Engine.GetFont(1), Hues.Load(1897), 39, 264, 184));
		this.OnAgentUpdated();
	}

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		base.BringToTop();
	}

	private Texture GetPreviewTexture()
	{
		Item dragPreview = this._dragPreview;
		if (dragPreview != null)
		{
			int iD = dragPreview.ID;
			int hue = dragPreview.Hue;
			int equipGumpID = Gumps.GetEquipGumpID(iD, this.m_Mobile.BodyGender, ref hue);
			Texture gump = Hues.GetItemHue(iD, hue).GetGump(equipGumpID);
			if (gump != null && !gump.IsEmpty())
			{
				return gump;
			}
		}
		return null;
	}

	protected internal override void Render(int X, int Y)
	{
		if (!base.m_Visible)
		{
			return;
		}
		int num = X + this.X;
		int num2 = Y + this.Y;
		this.Draw(num, num2);
		Texture texture = this.GetPreviewTexture();
		Gump[] array = base.m_Children.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			Gump gump = array[i];
			if (gump is GPaperdollItem)
			{
				GPaperdollItem gPaperdollItem = (GPaperdollItem)gump;
				if (gPaperdollItem.GumpID == 50000)
				{
					gPaperdollItem.Visible = false;
				}
			}
			if (texture != null && gump is GPaperdollItem)
			{
				GPaperdollItem gPaperdollItem2 = (GPaperdollItem)gump;
				if (this.Compare(this._dragPreview, gPaperdollItem2.Item) < 0)
				{
					Renderer.PushAlpha(0.5f);
					texture.Draw(num + 8, num2 + 19);
					Renderer.PopAlpha();
					texture = null;
				}
			}
			array[i].Render(num, num2);
		}
		if (texture != null)
		{
			Renderer.PushAlpha(0.5f);
			texture.Draw(num + 8, num2 + 19);
			Renderer.PopAlpha();
		}
	}

	protected internal override void OnDragEnter(Gump g)
	{
		if (g != null && g.GetType() == typeof(GDraggedItem))
		{
			GDraggedItem gDraggedItem = (GDraggedItem)g;
			this._dragPreview = gDraggedItem.Item;
		}
	}

	protected internal override void OnDragDrop(Gump g)
	{
		if (g != null && g.GetType() == typeof(GDraggedItem))
		{
			GDraggedItem gDraggedItem = (GDraggedItem)g;
			Item item = gDraggedItem.Item;
			Item item2 = null;
			Gump[] array = base.m_Children.ToArray();
			Point point = base.PointToClient(new Point(Engine.m_xMouse, Engine.m_yMouse));
			for (int num = array.Length - 1; num >= 0; num--)
			{
				if (array[num] is GPaperdollItem && array[num].HitTest(point.X - array[num].X, point.Y - array[num].Y))
				{
					item2 = ((GPaperdollItem)array[num]).Item;
					break;
				}
			}
			if (item2 != null && item2.Layer == Layer.Bank)
			{
				Network.Send(new PEquipItem(item, this.m_Mobile));
			}
			else if (item2 != null && Map.m_ItemFlags[item2.ID][TileFlag.Container])
			{
				Network.Send(new PDropItem(item.Serial, -1, -1, 0, item2.Serial));
			}
			else if (Map.m_ItemFlags[item.ID][TileFlag.Wearable])
			{
				Network.Send(new PEquipItem(item, this.m_Mobile));
			}
			else
			{
				Network.Send(new PDropItem(item.Serial, -1, -1, 0, World.Serial));
			}
			Gumps.Destroy(gDraggedItem);
		}
		this._dragPreview = null;
	}

	protected internal override void OnDragLeave(Gump g)
	{
		this._dragPreview = null;
	}

	public void OnChildAdded(Item added)
	{
		base.Children.Add(added.GetPaperdollItem(this.m_Mobile, this._allowItemLift));
		this.OnChildUpdated(added);
	}

	public void OnChildRemoved(Item removed)
	{
		if (removed.PaperdollItem.Parent == this)
		{
			Gumps.Destroy(removed.PaperdollItem);
		}
	}

	public void OnChildUpdated(Item refreshed)
	{
		int i = base.Children.IndexOf(refreshed.PaperdollItem);
		if (i >= 0)
		{
			while (i > 0 && base.Children[i - 1] is GPaperdollItem && this.Compare(i, i - 1) < 0)
			{
				base.Children.Swap(i, i - 1);
				i--;
			}
			for (; i + 1 < base.Children.Count && base.Children[i + 1] is GPaperdollItem && this.Compare(i, i + 1) > 0; i++)
			{
				base.Children.Swap(i, i + 1);
			}
		}
	}

	private int Compare(int left, int right)
	{
		GPaperdollItem gPaperdollItem = (GPaperdollItem)base.Children[left];
		GPaperdollItem gPaperdollItem2 = (GPaperdollItem)base.Children[right];
		return this.Compare(gPaperdollItem.Item, gPaperdollItem2.Item);
	}

	private int Compare(Item left, Item right)
	{
		LayerComparer paperdoll = LayerComparer.Paperdoll;
		return paperdoll.Compare(left, right);
	}

	public void OnAgentUpdated()
	{
		this.Mode = (this.m_Mobile.Player ? PaperdollMode.Extended : PaperdollMode.Compact);
		this.Body = PaperdollBody.FromMobile(this.m_Mobile);
	}

	public void OnAgentDeleted()
	{
		Gumps.Destroy(this);
	}
}
