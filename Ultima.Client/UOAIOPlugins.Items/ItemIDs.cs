using UOAIO;
using UOAIO.Targeting;

namespace UOAIOPlugins.Items;

public class ItemIDs : ClientTargetHandler
{
	public static readonly CommandCallback Command_Callback;

	public static void Callback(CommandArgs args)
	{
		TargetManager.Client = new ItemIDs();
	}

	protected override bool OnTarget(Item item)
	{
		Console.Print("(Item) ID: " + item.ID, Console.MessageType.Information);
		LootCorpseNG.AddItemID(item.ID);
		return true;
	}

	public static string GetLocationFormat(int x, int y, int z)
	{
		return $"({x}, {y}, {z})";
	}

	static ItemIDs()
	{
		ItemIDs.Command_Callback = Callback;
	}
}
