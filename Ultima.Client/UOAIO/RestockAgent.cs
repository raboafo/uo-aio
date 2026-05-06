using System;
using System.Collections.Generic;
using System.Reflection;
using Ultima.Client;
using UOAIO.Profiles;
using UOAIO.Targeting;
using Veritas;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class RestockAgent : PersistableObject
{
	public static readonly PersistableType TypeCode;

	public override PersistableType TypeID => RestockAgent.TypeCode;

	[Optionable("Reagents", "Restocking", Default = 100)]
	public int ReagentCount { get; set; }

	[Optionable("Heal Potions", "Restocking", Default = 15)]
	public int HealPotionCount { get; set; }

	[Optionable("Cure Potions", "Restocking", Default = 15)]
	public int CurePotionCount { get; set; }

	[Optionable("Refresh Potions", "Restocking", Default = 15)]
	public int RefreshPotionCount { get; set; }

	[Optionable("Strength Potions", "Restocking", Default = 10)]
	public int StrengthPotionCount { get; set; }

	[Optionable("Agility Potions", "Restocking", Default = 10)]
	public int AgilityPotionCount { get; set; }

	[Optionable("Explosion Potions", "Restocking", Default = 0)]
	public int ExplosionPotionCount { get; set; }

	[Optionable("Bandages", "Restocking", Default = 0)]
	public int BandageCount { get; set; }

	public ItemRef TargetContainer { get; set; }

	public ItemRef SourceContainer { get; set; }

	private static PersistableObject Construct()
	{
		return new RestockAgent();
	}

	public RestockAgent()
	{
		this.ReagentCount = 100;
		this.HealPotionCount = 15;
		this.CurePotionCount = 15;
		this.RefreshPotionCount = 15;
		this.StrengthPotionCount = 10;
		this.AgilityPotionCount = 10;
		this.ExplosionPotionCount = 0;
		this.BandageCount = 0;
		this.TargetContainer = new ItemRef(0);
		this.SourceContainer = new ItemRef(0);
	}

	private static void Transfer(Item sourceContainer, Item targetContainer, int amountDesired, int itemId, IItemValidator predicate)
	{
		Mobile player = World.Player;
		if (player == null)
		{
			return;
		}
		Item backpack = player.Backpack;
		if (backpack == null)
		{
			return;
		}
		Item item = targetContainer.FindItem(predicate);
		Point value;
		bool flag = Player.Current.OrganizeAgent.GetTemplate().TryGetValue(itemId, out value) && value.X != 0 && value.Y != 0;
		int num = 0;
		Item[] array = backpack.FindItems(predicate);
		foreach (Item item2 in array)
		{
			Item dropTo = ((item != null && item.IsStackable) ? item : targetContainer);
			MoveContext moveContext = new MoveContext(item2, item2.Amount, dropTo, clickFirst: false);
			if (flag)
			{
				moveContext.Locate(value.X, value.Y);
			}
			moveContext.TryEnqueue();
			num += item2.Amount;
			if (item == null)
			{
				item = item2;
			}
		}
		if (num > amountDesired)
		{
			new MoveContext(item, num - amountDesired, sourceContainer, clickFirst: false).Enqueue();
		}
		else
		{
			if (num >= amountDesired)
			{
				return;
			}
			Item agent = player.FindEquip(Layer.Bank);
			Item[] array2 = sourceContainer.FindItems(predicate);
			foreach (Item item3 in array2)
			{
				if (!item3.IsChildOf(player) || item3.IsChildOf(agent))
				{
					int num2 = Math.Min(item3.Amount, amountDesired - num);
					MoveContext moveContext2 = new MoveContext(item3, num2, targetContainer, clickFirst: false);
					if (flag)
					{
						moveContext2.Locate(value.X, value.Y);
					}
					moveContext2.Enqueue();
					num += num2;
					if (num == amountDesired)
					{
						break;
					}
				}
			}
			if (num < amountDesired)
			{
				Engine.AddTextMessage($"Unable to find sufficient quantity of {Localization.GetString(1020000 + itemId)}.", Engine.DefaultFont, Hues.Load(38));
			}
		}
	}

	public void Invoke()
	{
		Mobile me = World.Player;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		if (me == null)
		{
			return;
		}
		Item item = this.SourceContainer.Find();
		if (item != null)
		{
			if (item.HasContainerContent)
			{
				Item item2 = this.TargetContainer.FindOnPlayer();
				if (item2 != null)
				{
					if (!item2.HasContainerContent || this.ReagentCount <= 0)
					{
						return;
					}
					int[] list = ReagentValidator.Validator.List;
					foreach (int key in list)
					{
						dictionary.Add(key, this.ReagentCount);
					}
					dictionary.Add(3847, this.CurePotionCount);
					dictionary.Add(3848, this.AgilityPotionCount);
					dictionary.Add(3849, this.StrengthPotionCount);
					dictionary.Add(3851, this.RefreshPotionCount);
					dictionary.Add(3852, this.HealPotionCount);
					dictionary.Add(3853, this.ExplosionPotionCount);
					dictionary.Add(3617, this.BandageCount);
					{
						foreach (KeyValuePair<int, int> item3 in dictionary)
						{
							IItemValidator predicate = new PredicateValidator(new PlayerDistanceValidator(new PickupValidator(new ItemIDValidator(item3.Key)), 3), (Item item3) => item3.Parent != null && (item3.WorldRoot is Item || item3.WorldRoot == me));
							RestockAgent.Transfer(item, item2, item3.Value, item3.Key, predicate);
						}
						return;
					}
				}
				TargetManager.Client = new SetRestockTargetTargetHandler(invoking: true);
				Engine.AddTextMessage("Target your lootbag.");
			}
			else
			{
				new OpenRestockContainerContext(item).Enqueue();
			}
		}
		else
		{
			TargetManager.Client = new SetRestockSourceTargetHandler(invoking: true);
			Engine.AddTextMessage("Target your restock source container.");
		}
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		op.SetInt32("reagent-count", this.ReagentCount);
		op.SetInt32("heal-potion-count", this.HealPotionCount);
		op.SetInt32("cure-potion-count", this.CurePotionCount);
		op.SetInt32("refresh-potion-count", this.RefreshPotionCount);
		op.SetInt32("strength-potion-count", this.StrengthPotionCount);
		op.SetInt32("agility-potion-count", this.AgilityPotionCount);
		op.SetInt32("explosion-potion-count", this.ExplosionPotionCount);
		op.SetInt32("bandags-count", this.BandageCount);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.ReagentCount = ip.GetInt32("reagent-count");
		this.HealPotionCount = ip.GetInt32("heal-potion-count");
		this.CurePotionCount = ip.GetInt32("cure-potion-count");
		this.RefreshPotionCount = ip.GetInt32("refresh-potion-count");
		this.StrengthPotionCount = ip.GetInt32("strength-potion-count");
		this.AgilityPotionCount = ip.GetInt32("agility-potion-count");
		this.ExplosionPotionCount = ip.GetInt32("explosion-potion-count");
		this.BandageCount = ip.GetInt32("bandage-count");
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		this.TargetContainer.Serialize(op);
		this.SourceContainer.Serialize(op);
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		this.TargetContainer = (ip.HasChild ? (ip.GetChild() as ItemRef) : null) ?? new ItemRef(0);
		this.SourceContainer = (ip.HasChild ? (ip.GetChild() as ItemRef) : null) ?? new ItemRef(0);
	}

	static RestockAgent()
	{
		RestockAgent.TypeCode = new PersistableType("restock", Construct, ItemRef.TypeCode);
	}
}
