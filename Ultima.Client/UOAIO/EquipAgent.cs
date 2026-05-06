using System.Collections.Generic;
using UOAIO.Profiles;
using Veritas;

namespace UOAIO;

public class EquipAgent : PersistableObject
{
	public class ArmingAgent : PersistableObject
	{
		private class Slot : ItemRef
		{
			public new static readonly PersistableType TypeCode;

			private int index;

			public override PersistableType TypeID => Slot.TypeCode;

			public int Index => this.index;

			private static PersistableObject Construct()
			{
				return new Slot();
			}

			public Slot()
			{
			}

			public Slot(int index, Item item)
				: base(item)
			{
				this.index = index;
			}

			protected override void SerializeAttributes(PersistanceWriter op)
			{
				op.SetInt32("index", this.index);
				base.SerializeAttributes(op);
			}

			protected override void DeserializeAttributes(PersistanceReader ip)
			{
				this.index = ip.GetInt32("index");
				base.DeserializeAttributes(ip);
			}

			static Slot()
			{
				Slot.TypeCode = new PersistableType("slot", Construct);
			}
		}

		public static readonly PersistableType TypeCode;

		private Slot[] slots;

		public override PersistableType TypeID => ArmingAgent.TypeCode;

		private static PersistableObject Construct()
		{
			return new ArmingAgent();
		}

		public ArmingAgent()
		{
			this.slots = new Slot[10];
		}

		public void Dequip()
		{
			this.Dequip(message: true);
		}

		public void Dequip(bool message)
		{
			Mobile player = World.Player;
			if (player == null)
			{
				return;
			}
			if (player.Ghost)
			{
				if (message)
				{
					Engine.AddTextMessage("You are dead.");
				}
				return;
			}
			if (Gumps.Drag != null && Gumps.Drag.GetType() == typeof(GDraggedItem))
			{
				if (message)
				{
					Engine.AddTextMessage("You are already dragging an item.");
				}
				return;
			}
			Item item = player.FindEquip(Layer.TwoHanded);
			if (item == null)
			{
				item = player.FindEquip(Layer.OneHanded);
			}
			if (item == null)
			{
				if (message)
				{
					Engine.AddTextMessage("You are not holding anything.");
				}
			}
			else
			{
				new MoveContext(item, item.Amount, player, clickFirst: false).Enqueue();
			}
		}

		public void Equip(int index)
		{
			Mobile player = World.Player;
			if (player == null)
			{
				return;
			}
			if (player.Ghost)
			{
				Engine.AddTextMessage("You are dead.");
				return;
			}
			if (Gumps.Drag != null && Gumps.Drag.GetType() == typeof(GDraggedItem))
			{
				Engine.AddTextMessage("You are already dragging an item.");
				return;
			}
			Slot slot = this.slots[index];
			if (slot != null)
			{
				Item item = slot.FindOnPlayer();
				if (item == null)
				{
					Engine.AddTextMessage("Equipment not found.");
				}
				else if (item.Parent != player)
				{
					new EquipContext(item, item.Amount, player, clickFirst: false).Enqueue();
				}
			}
		}

		public void Assign(int index, Item item)
		{
			this.slots[index] = new Slot(index, item);
		}

		protected override void SerializeChildren(PersistanceWriter op)
		{
			for (int i = 0; i < this.slots.Length; i++)
			{
				if (this.slots[i] != null)
				{
					this.slots[i].Serialize(op);
				}
			}
		}

		protected override void DeserializeChildren(PersistanceReader ip)
		{
			while (ip.HasChild)
			{
				Slot slot = ip.GetChild() as Slot;
				this.slots[slot.Index] = slot;
			}
		}

		static ArmingAgent()
		{
			ArmingAgent.TypeCode = new PersistableType("arms", Construct, Slot.TypeCode);
		}
	}

	public class DressAgent : PersistableObject
	{
		private class Slot : ItemRef
		{
			public new static readonly PersistableType TypeCode;

			private Layer layer;

			public override PersistableType TypeID => Slot.TypeCode;

			public Layer Layer => this.layer;

			private static PersistableObject Construct()
			{
				return new Slot();
			}

			public Slot()
			{
			}

			public Slot(Layer layer, Item item)
				: base(item)
			{
				this.layer = layer;
			}

			protected override void SerializeAttributes(PersistanceWriter op)
			{
				op.SetInt32("layer", (int)this.layer);
				base.SerializeAttributes(op);
			}

			protected override void DeserializeAttributes(PersistanceReader ip)
			{
				this.layer = (Layer)ip.GetInt32("layer");
				base.DeserializeAttributes(ip);
			}

			static Slot()
			{
				Slot.TypeCode = new PersistableType("slot", Construct);
			}
		}

		public static readonly PersistableType TypeCode;

		private List<Slot> slots;

		public override PersistableType TypeID => DressAgent.TypeCode;

		private static PersistableObject Construct()
		{
			return new DressAgent();
		}

		public DressAgent()
		{
			this.slots = new List<Slot>();
		}

		public void Remove(Item item)
		{
			foreach (Slot slot in this.slots)
			{
				if (slot.Serial == item.Serial)
				{
					this.slots.Remove(slot);
					break;
				}
			}
		}

		public void EnsureDressed()
		{
			Mobile player = World.Player;
			if (player == null)
			{
				return;
			}
			if (player.Ghost)
			{
				Engine.AddTextMessage("You are dead.");
				return;
			}
			foreach (Slot slot in this.slots)
			{
				Item item = slot.FindOnPlayer();
				if (item != null && !player.HasEquip(item))
				{
					new EquipContext(item, item.Amount, player, clickFirst: false).Enqueue();
				}
			}
		}

		public void Populate(Layer layer, Item item)
		{
			bool flag = false;
			foreach (Slot slot in this.slots)
			{
				if (slot.Layer == layer)
				{
					slot.Serial = item.Serial;
					slot.ItemID = item.ID;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				this.slots.Add(new Slot(layer, item));
			}
		}

		protected override void SerializeChildren(PersistanceWriter op)
		{
			foreach (Slot slot in this.slots)
			{
				slot.Serialize(op);
			}
		}

		protected override void DeserializeChildren(PersistanceReader ip)
		{
			while (ip.HasChild)
			{
				this.slots.Add(ip.GetChild() as Slot);
			}
		}

		static DressAgent()
		{
			DressAgent.TypeCode = new PersistableType("dress", Construct, Slot.TypeCode);
		}
	}

	public static readonly PersistableType TypeCode;

	private ArmingAgent arms;

	private DressAgent dress;

	private ItemRef mount;

	public override PersistableType TypeID => EquipAgent.TypeCode;

	public ArmingAgent Arms => this.arms;

	public DressAgent Dress => this.dress;

	public ItemRef Mount => this.mount;

	private static PersistableObject Construct()
	{
		return new EquipAgent();
	}

	public EquipAgent()
	{
		this.arms = new ArmingAgent();
		this.dress = new DressAgent();
		this.mount = new ItemRef(0);
	}

	public void UpdateEquipment()
	{
		Mobile player = World.Player;
		if (player == null || player.Ghost)
		{
			return;
		}
		foreach (Item item in player.Items)
		{
			Layer layer = item.Layer;
			if ((int)layer >= 3 && (int)layer <= 24 && layer != Layer.Backpack && layer != Layer.FacialHair && layer != Layer.Hair)
			{
				this.dress.Populate(layer, item);
			}
			else if (layer == Layer.Mount)
			{
				this.mount.Serial = item.Serial;
			}
		}
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		this.arms.Serialize(op);
		this.dress.Serialize(op);
		if (!this.mount.IsNull)
		{
			this.mount.Serialize(op);
		}
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		this.arms = ip.GetChild() as ArmingAgent;
		this.dress = ip.GetChild() as DressAgent;
		if (ip.HasChild)
		{
			this.mount = ip.GetChild() as ItemRef;
		}
		else
		{
			this.mount = new ItemRef(0);
		}
	}

	static EquipAgent()
	{
		EquipAgent.TypeCode = new PersistableType("equip", Construct, ArmingAgent.TypeCode, DressAgent.TypeCode, ItemRef.TypeCode);
	}
}
