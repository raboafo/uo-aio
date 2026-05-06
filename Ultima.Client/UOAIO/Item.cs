using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Ultima.Data;
using UOAIO.Profiles;
using UOAIO.Targeting;

namespace UOAIO;

public class Item : PhysicalAgent, IComparable, IMessageOwner, IPoint2D, IAnimationOwner
{
	private Layer _layer;

	private int m_Props;

	private DateTime m_NextQueryProps;

	private ushort m_Hue;

	private byte m_Direction;

	private List<Item> m_CorpseItems;

	private int m_CorpseSerial;

	private ItemFlags m_Flags;

	private bool m_TradeCheck1;

	private bool m_TradeCheck2;

	private bool m_ShouldOverrideHue;

	private int m_RealHue;

	private IHue m_LastTextHue;

	public string m_LastText;

	private Multi m_Multi;

	private int m_MessageFrame;

	private int m_MessageX;

	private int m_MessageY;

	private int m_BottomY;

	private bool m_QueueOpenSB;

	private Mobile m_LastLooked;

	private Texture m_tPool;

	private VertexCache m_vCache;

	private IHue m_hAnimationPool;

	private int m_iAnimationPool;

	private Frames m_fAnimationPool;

	private RestoreInfo m_RestoreInfo;

	private int m_SpellbookOffset;

	private int m_SpellbookGraphic;

	private long m_SpellContained;

	private int m_Revision = -1;

	private int m_Circle;

	private int m_LastSpell = -1;

	private int m_BookIconX = 25;

	private int m_BookIconY = 25;

	private bool m_OpenSB;

	private static List<Item> _emptyItems;

	private List<Item> corpseSortedItems;

	private LayerComparer corpseSortComparer;

	private static Queue<Item> m_FindItem_Queue;

	private ushort amount;

	private ushort itemId;

	private ushort visage;

	private GPaperdollItem _paperdollItem;

	public ItemId ItemId => (ItemId)this.itemId;

	public unsafe ItemData* ItemDataPointer => Map.GetItemDataPointer(this.ItemId);

	public unsafe int Height => this.ItemDataPointer->Height;

	public unsafe int AnimationId
	{
		get
		{
			if (this.Layer == Layer.Mount)
			{
				return Engine.m_Animations.ConvertMountItemToBody(this.ID);
			}
			return this.ItemDataPointer->AnimationId;
		}
	}

	public unsafe Layer Layer
	{
		get
		{
			if (this._layer == Layer.Invalid)
			{
				return (Layer)this.ItemDataPointer->quality_layer_light;
			}
			return this._layer;
		}
		set
		{
			if (this._layer != value)
			{
				this._layer = value;
				base.RaiseUpdateEvents();
			}
		}
	}

	public int PropertyID
	{
		get
		{
			return this.m_Props;
		}
		set
		{
			if (this.m_Props != value)
			{
				this.m_NextQueryProps = DateTime.Now;
			}
			this.m_Props = value;
		}
	}

	public ObjectPropertyList PropertyList => ObjectPropertyList.Find(base.Serial, this.m_Props);

	public bool HasContainerContent { get; set; }

	public long SpellContained
	{
		get
		{
			return this.m_SpellContained;
		}
		set
		{
			this.m_SpellContained = value;
		}
	}

	public int SpellbookOffset
	{
		get
		{
			return this.m_SpellbookOffset;
		}
		set
		{
			this.m_SpellbookOffset = value;
		}
	}

	public int SpellbookGraphic
	{
		get
		{
			return this.m_SpellbookGraphic;
		}
		set
		{
			this.m_SpellbookGraphic = value;
		}
	}

	public int Revision
	{
		get
		{
			return this.m_Revision;
		}
		set
		{
			this.m_Revision = value;
		}
	}

	public int BookIconX
	{
		get
		{
			return this.m_BookIconX;
		}
		set
		{
			this.m_BookIconX = value;
		}
	}

	public int BookIconY
	{
		get
		{
			return this.m_BookIconY;
		}
		set
		{
			this.m_BookIconY = value;
		}
	}

	public int Circle
	{
		get
		{
			return this.m_Circle;
		}
		set
		{
			this.m_Circle = value;
		}
	}

	public int LastSpell
	{
		get
		{
			return this.m_LastSpell;
		}
		set
		{
			this.m_LastSpell = value;
		}
	}

	public bool IsDoor
	{
		get
		{
			int num = this.itemId;
			return (this.TileFlags & TileFlag.Door) != TileFlag.None || num == 1682 || num == 2118 || num == 2163 || (num >= 1781 && num <= 1782);
		}
	}

	public RestoreInfo RestoreInfo
	{
		get
		{
			return this.m_RestoreInfo;
		}
		set
		{
			this.m_RestoreInfo = value;
		}
	}

	public Mobile LastLooked
	{
		get
		{
			return this.m_LastLooked;
		}
		set
		{
			this.m_LastLooked = value;
		}
	}

	public bool QueueOpenSB
	{
		get
		{
			return this.m_QueueOpenSB;
		}
		set
		{
			this.m_QueueOpenSB = value;
		}
	}

	public int MessageFrame
	{
		get
		{
			return this.m_MessageFrame;
		}
		set
		{
			this.m_MessageFrame = value;
		}
	}

	public int MessageX
	{
		get
		{
			return this.m_MessageX;
		}
		set
		{
			this.m_MessageX = value;
		}
	}

	public int MessageY
	{
		get
		{
			return this.m_MessageY;
		}
		set
		{
			this.m_MessageY = value;
		}
	}

	public int BottomY
	{
		get
		{
			return this.m_BottomY;
		}
		set
		{
			this.m_BottomY = value;
		}
	}

	public Multi Multi
	{
		get
		{
			return this.m_Multi;
		}
		set
		{
			this.m_Multi = value;
		}
	}

	public IHue LastTextHue
	{
		get
		{
			return this.m_LastTextHue;
		}
		set
		{
			this.m_LastTextHue = value;
		}
	}

	public bool IsCorpse => this.itemId == 8198;

	public bool IsBones => this.itemId >= 3786 && this.itemId <= 3794;

	public bool TradeCheck1
	{
		get
		{
			return this.m_TradeCheck1;
		}
		set
		{
			this.m_TradeCheck1 = value;
		}
	}

	public bool TradeCheck2
	{
		get
		{
			return this.m_TradeCheck2;
		}
		set
		{
			this.m_TradeCheck2 = value;
		}
	}

	public bool OpenSB
	{
		get
		{
			return this.m_OpenSB;
		}
		set
		{
			this.m_OpenSB = value;
		}
	}

	public bool IsMulti => this.Multi != null;

	public bool InTradeWindow
	{
		get
		{
			for (Item item = this; item != null; item = item.Parent as Item)
			{
				if (item.Container != null && item.Container != null && item.Container.m_TradeContainer)
				{
					return true;
				}
			}
			return false;
		}
	}

	public GContainer Container => (GContainer)base.ContainerView;

	public ItemFlags Flags
	{
		get
		{
			return this.m_Flags;
		}
		set
		{
			this.m_Flags = value;
			if ((this.m_Flags.Value & -161) != 0)
			{
				string message = $"Unknown item flags: 0x{this.m_Flags.Value:X2}";
				Debug.Trace(message);
				Engine.AddTextMessage(message);
			}
		}
	}

	public int CorpseSerial
	{
		get
		{
			return this.m_CorpseSerial;
		}
		set
		{
			this.m_CorpseSerial = value;
		}
	}

	public List<Item> CorpseItems => this.m_CorpseItems ?? Item._emptyItems;

	public unsafe int Weight => this.ItemDataPointer->Weight;

	public unsafe TileFlag TileFlags => this.ItemDataPointer->Flags;

	public bool IsStacked => this.IsStackable && this.Amount > 1;

	public bool IsPilable => false;

	public bool IsMovable => this.Flags[ItemFlag.CanMove] || this.Weight <= 90;

	public bool IsWearable => (this.TileFlags & TileFlag.Wearable) != 0;

	public bool IsContainer => (this.TileFlags & TileFlag.Container) != 0;

	public bool IsStackable => (this.TileFlags & TileFlag.Generic) != 0;

	public ushort Hue
	{
		get
		{
			return this.m_Hue;
		}
		set
		{
			if (this.m_ShouldOverrideHue)
			{
				this.m_RealHue = value;
			}
			else if (this.m_Hue != value)
			{
				this.m_Hue = value;
				base.RaiseUpdateEvents();
			}
		}
	}

	public byte Direction
	{
		get
		{
			return this.m_Direction;
		}
		set
		{
			this.m_Direction = value;
		}
	}

	public int Amount
	{
		get
		{
			return this.amount;
		}
		set
		{
			ushort num = checked((ushort)value);
			if (this.amount != num)
			{
				this.amount = num;
				base.RaiseUpdateEvents();
			}
		}
	}

	public int ID
	{
		get
		{
			return this.itemId;
		}
		set
		{
			ushort num = checked((ushort)value);
			if (this.itemId != num)
			{
				this.itemId = num;
				base.RaiseUpdateEvents();
			}
		}
	}

	public GPaperdollItem PaperdollItem
	{
		get
		{
			return this._paperdollItem;
		}
		set
		{
			this._paperdollItem = value;
		}
	}

	public void QueryProperties()
	{
		if (Engine.ServerFeatures.AOS && !(DateTime.Now < this.m_NextQueryProps))
		{
			this.m_NextQueryProps = DateTime.Now + TimeSpan.FromSeconds(1.0);
			Network.Send(new PQueryProperties(base.Serial));
		}
	}

	public void SetSpellContained(int index, bool value)
	{
		long num = 1L << index;
		if (value)
		{
			this.m_SpellContained |= num;
		}
		else
		{
			this.m_SpellContained &= ~num;
		}
	}

	protected override void OnDeleted()
	{
		base.OnDeleted();
		if (this.Multi != null)
		{
			Engine.Multis.Unregister(this);
			this.Multi = null;
		}
		this.HasContainerContent = false;
	}

	public bool GetSpellContained(int index)
	{
		long num = 1L << index;
		return (this.m_SpellContained & num) != 0;
	}

	public void Query()
	{
	}

	public bool IsChair(out Chairs.ChairData data)
	{
		int itemID = this.itemId;
		if (Chairs.CheckItemAsChair(itemID, out data))
		{
			Engine.AddTextMessage("Item is a fucking chair!");
			return true;
		}
		return false;
	}

	Frames IAnimationOwner.GetOwnedFrames(IHue hue, int realID)
	{
		if (this.m_iAnimationPool == realID && this.m_hAnimationPool == hue && !this.m_fAnimationPool.Disposed)
		{
			return this.m_fAnimationPool;
		}
		this.m_fAnimationPool = hue.GetAnimation(realID);
		this.m_hAnimationPool = hue;
		this.m_iAnimationPool = realID;
		return this.m_fAnimationPool;
	}

	public void Draw(Texture t, int x, int y)
	{
		if (this.m_vCache == null)
		{
			this.m_vCache = new VertexCache();
		}
		if (this.m_tPool != t)
		{
			this.m_tPool = t;
			this.m_vCache.Invalidate();
		}
		this.m_vCache.Draw(t, x, y);
	}

	public void DrawGame(Texture t, int x, int y, int color)
	{
		if (this.m_vCache == null)
		{
			this.m_vCache = new VertexCache();
		}
		if (this.m_tPool != t)
		{
			this.m_tPool = t;
			this.m_vCache.Invalidate();
		}
		this.m_vCache.DrawGame(t, x, y, color);
	}

	public void OnSingleClick()
	{
		this.Look();
	}

	public void OnDoubleClick()
	{
		this.Use(isManual: true);
		PUseRequest.Last = this;
	}

	public void OnTarget()
	{
		TargetManager.Target(this);
	}

	public void OverrideHue(int hue)
	{
		bool shouldOverrideHue = this.m_ShouldOverrideHue;
		this.m_ShouldOverrideHue = hue >= 0;
		if (this.m_ShouldOverrideHue)
		{
			if (!shouldOverrideHue)
			{
				this.m_RealHue = this.m_Hue;
				this.m_Hue = (ushort)hue;
			}
		}
		else if (shouldOverrideHue)
		{
			this.m_Hue = (ushort)this.m_RealHue;
		}
		base.RaiseUpdateEvents();
	}

	public void Update()
	{
	}

	public bool Use()
	{
		return this.Use(isManual: false);
	}

	public bool Use(bool isManual)
	{
		new UseContext(this, isManual).Enqueue();
		return true;
	}

	public bool SendUse()
	{
		return Network.Send(new PUseRequest(this));
	}

	public bool Look()
	{
		return Network.Send(new PLookRequest(this));
	}

	public int CompareTo(object x)
	{
		if (x == null)
		{
			return 1;
		}
		Item item = (Item)x;
		int num = base.Y - item.Y;
		if (num == 0)
		{
			num = base.X - item.X;
			if (num == 0)
			{
				num = base.Z - item.Z;
			}
		}
		return num;
	}

	public GContainer OpenContainer(int gumpId, IHue hue)
	{
		if (this.Container == null)
		{
			base.SetContainerView(new GContainer(this, gumpId, hue));
		}
		else
		{
			this.Container.GumpID = gumpId;
			this.Container.Hue = hue;
		}
		return this.Container;
	}

	protected override void OnChildAdded(Agent child)
	{
		base.OnChildAdded(child);
		if (child is Item)
		{
			Item item = (Item)child;
			if (item.InTradeWindow)
			{
				item.QueryProperties();
			}
			int iD = item.ID;
			if (iD >= 3570 && iD < 3574 && item.IsChildOf(World.Player))
			{
				new LookContext(item).Enqueue();
			}
		}
	}

	protected override void OnChildRemoved(Agent child)
	{
		base.OnChildRemoved(child);
	}

	public void ClearCorpseItems()
	{
		if (this.m_CorpseItems != null)
		{
			this.m_CorpseItems.Clear();
		}
		if (this.corpseSortedItems != null)
		{
			this.corpseSortedItems.Clear();
			this.corpseSortedItems = null;
			this.corpseSortComparer = null;
		}
	}

	public void AddCorpseItem(Item item)
	{
		if (this.m_CorpseItems == null)
		{
			this.m_CorpseItems = new List<Item>();
		}
		this.m_CorpseItems.Add(item);
		if (this.corpseSortedItems != null)
		{
			int num = this.corpseSortedItems.BinarySearch(item, this.corpseSortComparer);
			if (num < 0)
			{
				num = ~num;
			}
			this.corpseSortedItems.Insert(num, item);
		}
	}

	public List<Item> GetSortedCorpseItems()
	{
		return this.GetSortedCorpseItems(this.m_Direction);
	}

	public List<Item> GetSortedCorpseItems(int direction)
	{
		LayerComparer layerComparer = LayerComparer.FromDirection(direction);
		if (this.corpseSortedItems == null)
		{
			this.corpseSortedItems = new List<Item>(this.CorpseItems);
		}
		if (layerComparer != this.corpseSortComparer)
		{
			this.corpseSortComparer = layerComparer;
			this.corpseSortedItems.Sort(this.corpseSortComparer);
		}
		return this.corpseSortedItems;
	}

	protected override void OnLocationChanged()
	{
		base.OnLocationChanged();
		if (this.Multi != null)
		{
			Map.Invalidate();
			GRadar.Invalidate();
		}
	}

	public Gump OnBeginDrag()
	{
		if (this.IsStackable && this.amount > 1 && (Control.ModifierKeys & Keys.Shift) == 0)
		{
			GDragAmount gDragAmount = new GDragAmount(this);
			Gumps.Desktop.Children.Add(gDragAmount);
			return gDragAmount;
		}
		Player.Current?.EquipAgent.Dress.Remove(this);
		Network.Send(new PPickupItem(this, this.amount));
		GDraggedItem gDraggedItem = new GDraggedItem(this);
		Gumps.Desktop.Children.Add(gDraggedItem);
		return gDraggedItem;
	}

	public Item FindItem(IItemValidator iv)
	{
		return this.FindItem(iv.IsValid);
	}

	public Item FindItem(Predicate<Item> filter)
	{
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		Queue<Item> queue = Item.m_FindItem_Queue;
		if (queue == null)
		{
			queue = (Item.m_FindItem_Queue = new Queue<Item>());
		}
		else if (queue.Count > 0)
		{
			queue.Clear();
		}
		if (filter(this))
		{
			return this;
		}
		if (base.HasItems)
		{
			queue.Enqueue(this);
			while (queue.Count > 0)
			{
				Item item = queue.Dequeue();
				List<Item> items = item.Items;
				for (int i = 0; i < items.Count; i++)
				{
					Item item2 = items[i];
					if (filter(item2))
					{
						return item2;
					}
					if (item2.HasItems)
					{
						queue.Enqueue(item2);
					}
				}
			}
		}
		return null;
	}

	public IEnumerable<Item> GetItems(IItemValidator validator)
	{
		if (validator == null)
		{
			throw new ArgumentNullException("validator");
		}
		return this.GetItems(validator.IsValid);
	}

	public IEnumerable<Item> GetItems(Predicate<Item> filter)
	{
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		if (!base.HasItems)
		{
			yield break;
		}
		using ScratchQueue<Item> queueScratch = new ScratchQueue<Item>();
		Queue<Item> queue = queueScratch.Value;
		queue.Enqueue(this);
		while (queue.Count > 0)
		{
			Item item = queue.Dequeue();
			List<Item> items = item.Items;
			int i = 0;
			while (i < items.Count)
			{
				Item child = items[i];
				if (filter(child))
				{
					yield return child;
				}
				if (child.HasItems)
				{
					queue.Enqueue(child);
				}
				int num = i + 1;
				i = num;
			}
		}
	}

	public Item[] FindItems(IItemValidator validator)
	{
		return ScratchList<Item>.ToArray(this.GetItems(validator));
	}

	public Item(int serial)
		: base(serial)
	{
		this.m_Flags = new ItemFlags();
	}

	public void AddTextMessage(string Name, string Message, IFont Font, IHue Hue, bool unremovable)
	{
		this.m_LastTextHue = Hue;
		this.m_LastText = Message;
		string text = null;
		text = ((Name.Length <= 0) ? Message : (Name + ": " + Message));
		if (Message.Length > 0)
		{
			Engine.AddToJournal(new JournalEntry(text, Hue, base.Serial));
			Message = Engine.WrapText(Message, 200, Font).TrimEnd();
			if (Message.Length > 0)
			{
				MessageManager.AddMessage(new GDynamicMessage(unremovable, this, Message, Font, Hue));
			}
		}
	}

	public GPaperdollItem GetPaperdollItem(Mobile mob, bool canLift)
	{
		if (this._paperdollItem == null)
		{
			this._paperdollItem = new GPaperdollItem(mob, this, canLift);
		}
		else
		{
			this._paperdollItem.Mobile = mob;
			this._paperdollItem.CanLift = canLift;
		}
		return this._paperdollItem;
	}

	protected override IAgentCell CreateViewportCell()
	{
		return new DynamicItem(this);
	}

	protected override IEnumerable<IAgentView> GetAgentViews()
	{
		List<IAgentView> list = new List<IAgentView>(base.GetAgentViews());
		if (this._paperdollItem != null)
		{
			list.Add(this._paperdollItem);
		}
		return list;
	}

	static Item()
	{
		Item._emptyItems = new List<Item>();
	}
}
