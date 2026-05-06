using System.Collections.Generic;
using UOAIO.Profiles;
using UOAIO.Targeting;
using Veritas;

namespace UOAIO;

public class OrganizeAgent : PersistableObject
{
	public static readonly PersistableType TypeCode;

	public override PersistableType TypeID => OrganizeAgent.TypeCode;

	public bool TemplateSet { get; set; }

	public ItemRef TargetContainer { get; set; }

	public Point BlackPearlPos { get; set; }

	public Point BloodMossPos { get; set; }

	public Point GarlicPos { get; set; }

	public Point GinsengPos { get; set; }

	public Point MandrakeRootPos { get; set; }

	public Point NightshadePos { get; set; }

	public Point SpidersSilkPos { get; set; }

	public Point SulfurousAshPos { get; set; }

	public Point HealPotionPos { get; set; }

	public Point CurePotionPos { get; set; }

	public Point RefreshPotionPos { get; set; }

	public Point StrengthPotionPos { get; set; }

	public Point AgilityPotionPos { get; set; }

	public Point ExplosionPotionPos { get; set; }

	private static PersistableObject Construct()
	{
		return new OrganizeAgent();
	}

	public OrganizeAgent()
	{
		this.TemplateSet = false;
		this.TargetContainer = new ItemRef(0);
		this.BlackPearlPos = new Point(0, 0);
		this.BloodMossPos = new Point(0, 0);
		this.GarlicPos = new Point(0, 0);
		this.GinsengPos = new Point(0, 0);
		this.MandrakeRootPos = new Point(0, 0);
		this.NightshadePos = new Point(0, 0);
		this.SpidersSilkPos = new Point(0, 0);
		this.SulfurousAshPos = new Point(0, 0);
		this.HealPotionPos = new Point(0, 0);
		this.CurePotionPos = new Point(0, 0);
		this.RefreshPotionPos = new Point(0, 0);
		this.StrengthPotionPos = new Point(0, 0);
		this.AgilityPotionPos = new Point(0, 0);
		this.ExplosionPotionPos = new Point(0, 0);
	}

	private void OrganizeStackableItem(int itemID, Point destination, Item targetContainer)
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
		ItemIDValidator validator = new ItemIDValidator(itemID);
		Item item = null;
		Item[] array = backpack.FindItems(validator);
		Item[] array2 = targetContainer.FindItems(validator);
		if (array == null || array.Length == 0)
		{
			return;
		}
		if (array2 != null)
		{
			Item[] array3 = array2;
			foreach (Item item2 in array3)
			{
				if (item2.Parent == targetContainer && item2.X == destination.X && item2.Y == destination.Y)
				{
					item = item2;
					break;
				}
			}
		}
		if (item == null)
		{
			item = array[0];
			MoveContext moveContext = new MoveContext(item, item.Amount, targetContainer, clickFirst: false);
			moveContext.Locate(destination.X, destination.Y);
			moveContext.Enqueue();
		}
		Item[] array4 = array;
		foreach (Item item3 in array4)
		{
			if (item3 != item)
			{
				new MoveContext(item3, item3.Amount, item, clickFirst: false).Enqueue();
			}
		}
	}

	private void OrganizeNonstackableItem(int itemID, Point destination, Item targetContainer)
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
		Item[] array = backpack.FindItems(new ItemIDValidator(itemID));
		if (array == null || array.Length == 0)
		{
			return;
		}
		Item[] array2 = array;
		foreach (Item item in array2)
		{
			if (item.Parent != targetContainer || item.X != destination.X || item.Y != destination.Y)
			{
				MoveContext moveContext = new MoveContext(item, 1, targetContainer, clickFirst: false);
				moveContext.Locate(destination.X, destination.Y);
				moveContext.Enqueue();
			}
		}
	}

	public Dictionary<int, Point> GetTemplate()
	{
		Dictionary<int, Point> dictionary = new Dictionary<int, Point>();
		dictionary[3962] = this.BlackPearlPos;
		dictionary[3963] = this.BloodMossPos;
		dictionary[3972] = this.GarlicPos;
		dictionary[3973] = this.GinsengPos;
		dictionary[3974] = this.MandrakeRootPos;
		dictionary[3976] = this.NightshadePos;
		dictionary[3981] = this.SpidersSilkPos;
		dictionary[3980] = this.SulfurousAshPos;
		dictionary[3852] = this.HealPotionPos;
		dictionary[3847] = this.CurePotionPos;
		dictionary[3851] = this.RefreshPotionPos;
		dictionary[3849] = this.StrengthPotionPos;
		dictionary[3848] = this.AgilityPotionPos;
		dictionary[3853] = this.ExplosionPotionPos;
		return dictionary;
	}

	public void Invoke()
	{
		if (this.TemplateSet)
		{
			Item item = this.TargetContainer.FindOnPlayer();
			if (item != null)
			{
				Point point = new Point(0, 0);
				{
					foreach (KeyValuePair<int, Point> item2 in this.GetTemplate())
					{
						bool flag = item2.Key != 3853;
						if (item2.Key != 3853)
						{
							this.OrganizeStackableItem(item2.Key, item2.Value, item);
						}
						else
						{
							this.OrganizeNonstackableItem(item2.Key, item2.Value, item);
						}
					}
					return;
				}
			}
			TargetManager.Client = new SetOrganizeTargetTargetHandler(invoking: true);
			Engine.AddTextMessage("Target your lootbag.");
		}
		else
		{
			TargetManager.Client = new SetOrganizeTemplateTargetHandler(invoking: true);
			Engine.AddTextMessage("Target your template lootbag.");
		}
	}

	public void SetTemplate(Item item)
	{
		Item item2 = item.FindItem(new ItemIDValidator(3962));
		Item item3 = item.FindItem(new ItemIDValidator(3963));
		Item item4 = item.FindItem(new ItemIDValidator(3972));
		Item item5 = item.FindItem(new ItemIDValidator(3973));
		Item item6 = item.FindItem(new ItemIDValidator(3974));
		Item item7 = item.FindItem(new ItemIDValidator(3976));
		Item item8 = item.FindItem(new ItemIDValidator(3981));
		Item item9 = item.FindItem(new ItemIDValidator(3980));
		Item item10 = item.FindItem(new ItemIDValidator(3852));
		Item item11 = item.FindItem(new ItemIDValidator(3847));
		Item item12 = item.FindItem(new ItemIDValidator(3851));
		Item item13 = item.FindItem(new ItemIDValidator(3849));
		Item item14 = item.FindItem(new ItemIDValidator(3848));
		Item item15 = item.FindItem(new ItemIDValidator(3853));
		if (item2 == null)
		{
			this.BlackPearlPos = new Point(0, 0);
		}
		else
		{
			this.BlackPearlPos = new Point(item2.X, item2.Y);
		}
		if (item3 == null)
		{
			this.BloodMossPos = new Point(0, 0);
		}
		else
		{
			this.BloodMossPos = new Point(item3.X, item3.Y);
		}
		if (item4 == null)
		{
			this.GarlicPos = new Point(0, 0);
		}
		else
		{
			this.GarlicPos = new Point(item4.X, item4.Y);
		}
		if (item5 == null)
		{
			this.GinsengPos = new Point(0, 0);
		}
		else
		{
			this.GinsengPos = new Point(item5.X, item5.Y);
		}
		if (item6 == null)
		{
			this.MandrakeRootPos = new Point(0, 0);
		}
		else
		{
			this.MandrakeRootPos = new Point(item6.X, item6.Y);
		}
		if (item7 == null)
		{
			this.NightshadePos = new Point(0, 0);
		}
		else
		{
			this.NightshadePos = new Point(item7.X, item7.Y);
		}
		if (item8 == null)
		{
			this.SpidersSilkPos = new Point(0, 0);
		}
		else
		{
			this.SpidersSilkPos = new Point(item8.X, item8.Y);
		}
		if (item9 == null)
		{
			this.SulfurousAshPos = new Point(0, 0);
		}
		else
		{
			this.SulfurousAshPos = new Point(item9.X, item9.Y);
		}
		if (item10 == null)
		{
			this.HealPotionPos = new Point(0, 0);
		}
		else
		{
			this.HealPotionPos = new Point(item10.X, item10.Y);
		}
		if (item11 == null)
		{
			this.CurePotionPos = new Point(0, 0);
		}
		else
		{
			this.CurePotionPos = new Point(item11.X, item11.Y);
		}
		if (item12 == null)
		{
			this.RefreshPotionPos = new Point(0, 0);
		}
		else
		{
			this.RefreshPotionPos = new Point(item12.X, item12.Y);
		}
		if (item13 == null)
		{
			this.StrengthPotionPos = new Point(0, 0);
		}
		else
		{
			this.StrengthPotionPos = new Point(item13.X, item13.Y);
		}
		if (item14 == null)
		{
			this.AgilityPotionPos = new Point(0, 0);
		}
		else
		{
			this.AgilityPotionPos = new Point(item14.X, item14.Y);
		}
		if (item15 == null)
		{
			this.ExplosionPotionPos = new Point(0, 0);
		}
		else
		{
			this.ExplosionPotionPos = new Point(item15.X, item15.Y);
		}
		this.TemplateSet = true;
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		op.SetBoolean("template-set", this.TemplateSet);
		op.SetPoint("black-pearl", this.BlackPearlPos);
		op.SetPoint("blood-moss", this.BloodMossPos);
		op.SetPoint("garlic", this.GarlicPos);
		op.SetPoint("ginseng", this.GinsengPos);
		op.SetPoint("mandrake-root", this.MandrakeRootPos);
		op.SetPoint("nightshade", this.NightshadePos);
		op.SetPoint("spiders-silk", this.SpidersSilkPos);
		op.SetPoint("sulfurous-ash", this.SulfurousAshPos);
		op.SetPoint("heal-potion", this.HealPotionPos);
		op.SetPoint("cure-potion", this.CurePotionPos);
		op.SetPoint("refresh-potion", this.RefreshPotionPos);
		op.SetPoint("strength-potion", this.StrengthPotionPos);
		op.SetPoint("agility-potion", this.AgilityPotionPos);
		op.SetPoint("explosion-potion", this.ExplosionPotionPos);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.TemplateSet = ip.GetBoolean("template-set");
		this.BlackPearlPos = ip.GetPoint("black-pearl");
		this.BloodMossPos = ip.GetPoint("blood-moss");
		this.GarlicPos = ip.GetPoint("garlic");
		this.GinsengPos = ip.GetPoint("ginseng");
		this.MandrakeRootPos = ip.GetPoint("mandrake-root");
		this.NightshadePos = ip.GetPoint("nightshade");
		this.SpidersSilkPos = ip.GetPoint("spiders-silk");
		this.SulfurousAshPos = ip.GetPoint("sulfurous-ash");
		this.HealPotionPos = ip.GetPoint("heal-potion");
		this.CurePotionPos = ip.GetPoint("cure-potion");
		this.RefreshPotionPos = ip.GetPoint("refresh-potion");
		this.StrengthPotionPos = ip.GetPoint("strength-potion");
		this.AgilityPotionPos = ip.GetPoint("agility-potion");
		this.ExplosionPotionPos = ip.GetPoint("explosion-potion");
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		this.TargetContainer.Serialize(op);
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		this.TargetContainer = (ip.HasChild ? (ip.GetChild() as ItemRef) : null) ?? new ItemRef(0);
	}

	static OrganizeAgent()
	{
		OrganizeAgent.TypeCode = new PersistableType("organize", Construct, ItemRef.TypeCode);
	}
}
