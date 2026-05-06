using System;
using System.Collections.Generic;
using System.Linq;

namespace UOAIO;

public class WorldEx
{
	public static IEnumerable<Item> GetItems(IItemValidator validator)
	{
		if (validator == null)
		{
			throw new ArgumentNullException("validator");
		}
		return WorldEx.GetItems(validator.IsValid);
	}

	public static IEnumerable<Item> GetItems(Predicate<Item> validator)
	{
		if (validator == null)
		{
			throw new ArgumentNullException("validator");
		}
		foreach (Item item in World.Items.Values.ToList())
		{
			if (item.Visible && item.InWorld && !item.IsMulti && World.InRange(item) && validator(item))
			{
				yield return item;
			}
		}
	}

	public static bool IsGuildMember(string guild)
	{
		if (guild != null)
		{
			return guild == World.Player.Guild;
		}
		return false;
	}

	public static bool IsGuildMember(Mobile mobile)
	{
		if (mobile.Guild != null)
		{
			return mobile.Guild == World.Player.Guild;
		}
		return false;
	}
}
