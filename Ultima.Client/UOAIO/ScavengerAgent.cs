using System;
using System.Collections;
using System.Reflection;
using UOAIO.Profiles;
using Veritas;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class ScavengerAgent : PersistableObject, IItemValidator, IComparer
{
	public static readonly PersistableType TypeCode;

	private ItemRefCollection m_Items;

	private ScavengerOptions m_Options;

	public override PersistableType TypeID => ScavengerAgent.TypeCode;

	[Optionable("Reagents", "Scavenger", Default = true)]
	public bool Reagents
	{
		get
		{
			return this[ScavengerOptions.Reagents];
		}
		set
		{
			this[ScavengerOptions.Reagents] = value;
		}
	}

	[Optionable("Arrows & Bolts", "Scavenger", Default = false)]
	public bool Munitions
	{
		get
		{
			return this[ScavengerOptions.Munitions];
		}
		set
		{
			this[ScavengerOptions.Munitions] = value;
		}
	}

	[Optionable("Bolas", "Scavenger", Default = true)]
	public bool Bolas
	{
		get
		{
			return this[ScavengerOptions.Bolas];
		}
		set
		{
			this[ScavengerOptions.Bolas] = value;
		}
	}

	[Optionable("Artifacts", "Scavenger", Default = true, OnlyAOS = true)]
	public bool Artifacts
	{
		get
		{
			return this[ScavengerOptions.Artifacts];
		}
		set
		{
			this[ScavengerOptions.Artifacts] = value;
		}
	}

	public ItemRefCollection Items => this.m_Items;

	internal int OptionsValue => (int)this.m_Options;

	public ItemRef this[Item item]
	{
		get
		{
			if (item != null)
			{
				foreach (ItemRef item2 in this.m_Items)
				{
					if (item2.Serial == item.Serial)
					{
						return item2;
					}
				}
			}
			return null;
		}
	}

	public bool this[ScavengerOptions option]
	{
		get
		{
			return (this.m_Options & option) == option;
		}
		set
		{
			if (value)
			{
				this.m_Options |= option;
			}
			else
			{
				this.m_Options &= ~option;
			}
		}
	}

	private static PersistableObject Construct()
	{
		return new ScavengerAgent();
	}

	public ScavengerAgent()
	{
		this.m_Items = new ItemRefCollection();
		this.m_Options = ScavengerOptions.Default;
	}

	internal void ApplyState(int options, ItemRef[] items)
	{
		this.m_Options = (ScavengerOptions)options;
		this.m_Items.Clear();
		if (items == null)
		{
			return;
		}

		for (int i = 0; i < items.Length; i++)
		{
			this.m_Items.Add(items[i]);
		}
	}

	public void Scavenge(bool isManual)
	{
		Mobile player = World.Player;
		if (player == null)
		{
			return;
		}
		IItemValidator validator = new PlayerDistanceValidator(new PickupValidator(this), 2);
		Item[] array = World.FindItems(validator);
		if (array.Length == 0)
		{
			return;
		}
		Array.Sort(array, this);
		bool clickFirst = false;
		foreach (Item item in array)
		{
			if (!Engine.Multis.RunUO_IsInside(item.X, item.Y, item.Z) && new MoveContext(item, item.Amount, player, clickFirst).Enqueue())
			{
				int num = Math.Max(item.Amount, 1);
				Engine.AddTextMessage($"Scavenging {num:N0} {Map.ReplaceAmount(Map.GetTileName(item.ID + 16384), num)}", Engine.DefaultFont, Hues.Load(53));
			}
		}
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		op.SetInt32("options", (int)this.m_Options);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.m_Options = (ScavengerOptions)ip.GetInt32("options");
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		for (int i = 0; i < this.m_Items.Count; i++)
		{
			this.m_Items[i].Serialize(op);
		}
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			this.m_Items.Add(ip.GetChild() as ItemRef);
		}
	}

	public bool IsValid(Item check)
	{
		if (this.Reagents && ReagentValidator.Validator.IsValid(check))
		{
			return true;
		}
		if (this.Bolas && check.ID == 9900)
		{
			return true;
		}
		if (this.Munitions && (check.ID == 3903 || check.ID == 7163))
		{
			return true;
		}
		if (this.Artifacts && ArtifactValidator.Default.IsValid(check))
		{
			return true;
		}
		foreach (ItemRef item in this.m_Items)
		{
			if (item.ItemID != check.ID || (item.Serial != 0 && item.Serial != check.Serial))
			{
				continue;
			}
			return true;
		}
		return false;
	}

	private int GetWorth(Item item)
	{
		if (ReagentValidator.Validator.IsValid(item))
		{
			return 4;
		}
		return 10;
	}

	public int Compare(object x, object y)
	{
		Item item = x as Item;
		Item item2 = y as Item;
		return this.GetWorth(item2) - this.GetWorth(item);
	}

	static ScavengerAgent()
	{
		ScavengerAgent.TypeCode = new PersistableType("scavenger", Construct, ItemRef.TypeCode);
	}
}
