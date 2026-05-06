using UOAIO.Profiles;

namespace UOAIO.Targeting;

internal class ScavengerTargetHandler : ClientTargetHandler
{
	private bool m_IsAdd;

	private bool m_ByType;

	public ScavengerTargetHandler(bool isAdd, bool byType)
	{
		this.m_IsAdd = isAdd;
		this.m_ByType = byType;
	}

	protected override bool OnTarget(Item item)
	{
		ScavengerAgent scavenger = Preferences.Current.Scavenger;
		ItemRef itemRef = scavenger[item];
		if (itemRef == null)
		{
			foreach (ItemRef item2 in scavenger.Items)
			{
				if (item2.Serial == 0 && item2.ItemID == item.ID)
				{
					itemRef = item2;
					break;
				}
			}
		}
		if (this.m_IsAdd)
		{
			if (itemRef == null)
			{
				if (this.m_ByType)
				{
					scavenger.Items.Add(new ItemRef(item.ID));
					Engine.AddTextMessage("Item type added to the scavenger list.");
				}
				else
				{
					scavenger.Items.Add(new ItemRef(item));
					Engine.AddTextMessage("Item instance added to the scavenger list.");
				}
			}
			else if (itemRef.Serial == 0)
			{
				Engine.AddTextMessage("Item type is already in the scavenger list.");
			}
			else if (this.m_ByType)
			{
				scavenger.Items.Remove(itemRef);
				scavenger.Items.Add(new ItemRef(item.ID));
				Engine.AddTextMessage("Scavenger entry changed to by-type.");
			}
			else
			{
				Engine.AddTextMessage("Item instance is already in the scavenger list.");
			}
		}
		else if (itemRef != null)
		{
			scavenger.Items.Remove(itemRef);
			if (itemRef.Serial == 0)
			{
				Engine.AddTextMessage("Item type removed from the scavenger list.");
			}
			else
			{
				Engine.AddTextMessage("Item instance removed from the scavenger list.");
			}
		}
		else
		{
			Engine.AddTextMessage("That's not even in the scavenger list.");
		}
		return true;
	}
}
