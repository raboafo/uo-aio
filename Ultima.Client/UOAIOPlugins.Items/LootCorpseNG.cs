using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using UOAIO;
using UOAIO.Events;

namespace UOAIOPlugins.Items;

public class LootCorpseNG
{
	private enum CorpseState
	{
		Discovered,
		Opening,
		Opened,
		Looting,
		Done,
		Failed
	}

	private sealed class CorpseInfo
	{
		public int Serial { get; }

		public int X { get; set; }

		public int Y { get; set; }

		public CorpseState State { get; set; }

		public long LastAction { get; set; }

		public CorpseInfo(int serial, int x, int y)
		{
			this.Serial = serial;
			this.X = x;
			this.Y = y;
		}

		public bool InRange(int range)
		{
			return World.Player.InRange(this.X, this.Y, range);
		}
	}
	
	private static List<int> ToLoot;

	private static string path;

	public static readonly ActionCallback Macro_Callback;

	private static readonly Dictionary<int, CorpseInfo> _corpses;

	private static void EnableLootCorpseNG()
	{
		PacketHandlers.EventBus.Subscribe<ContainerItemsEvent>(handleContainerItemsEvent);
		Engine.EventBus.Subscribe<EngineTickEvent>(handleEngineTickEvent);
		Console.Print("Autoloot enabled", Console.MessageType.Information);
	}

	private static void DisableLootCorpseNG()
	{
		PacketHandlers.EventBus.Unsubscribe<ContainerItemsEvent>(handleContainerItemsEvent);
		Engine.EventBus.Unsubscribe<EngineTickEvent>(handleEngineTickEvent);
		Console.Print("Autoloot disabled", Console.MessageType.Information);
	}

	public static void Loot()
	{
	}

	public static void Load()
	{
		if (!File.Exists(LootCorpseNG.path))
		{
			return;
		}
		string[] array = File.ReadAllLines(LootCorpseNG.path);
		foreach (string text in array)
		{
			if (!text.StartsWith("#") && int.TryParse(text, out var result) && !LootCorpseNG.ToLoot.Contains(result))
			{
				LootCorpseNG.ToLoot.Add(result);
			}
		}
	}

	public static void AddItemID(int itemID)
	{
		if (LootCorpseNG.ToLoot.Contains(itemID))
		{
			return;
		}
		try
		{
			File.AppendAllText(LootCorpseNG.path, itemID + Environment.NewLine);
			LootCorpseNG.ToLoot.Add(itemID);
		}
		catch (Exception ex)
		{
			UOAIO.Debug.Error(ex);
			Console.Print("[LootCorpseNG] " + ex.Message, Console.MessageType.Error);
		}
	}

	static LootCorpseNG()
	{
		LootCorpseNG._corpses = new Dictionary<int, CorpseInfo>();
		LootCorpseNG.ToLoot = new List<int>();
		LootCorpseNG.path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AutoLootNG.txt");
		LootCorpseNG.Macro_Callback = OnMacro;
		LootCorpseNG.Load();
	}

	public static bool OnMacro(string args)
	{
		switch (args)
		{
		case "On":
			Options.AutoLootCorpseNG = true;
			LootCorpseNG.EnableLootCorpseNG();
			return true;
		case "Off":
			Options.AutoLootCorpseNG = false;
			LootCorpseNG.DisableLootCorpseNG();
			return true;
		default:
		{
			Options.AutoLootCorpseNG = !Options.AutoLootCorpseNG;
			bool autoLootCorpseNG = Options.AutoLootCorpseNG;
			if (autoLootCorpseNG)
			{
				if (autoLootCorpseNG)
				{
					LootCorpseNG.EnableLootCorpseNG();
				}
			}
			else
			{
				LootCorpseNG.DisableLootCorpseNG();
			}
			return true;
		}
		}
	}

	private static void handleWorldObjectUpdateEvent(WorldObjectUpdateEvent e)
	{
		Item item = e.Item;
		if (item != null && (item.IsCorpse || item.IsBones) && item.Items.Count > 0)
		{
			if (!LootCorpseNG._corpses.TryGetValue(item.Serial, out var value))
			{
				value = new CorpseInfo(item.Serial, item.X, item.Y);
				LootCorpseNG._corpses[item.Serial] = value;
			}
			if (item.InRange(World.Player, 2))
			{
				value.State = CorpseState.Opening;
				value.LastAction = Stopwatch.GetTimestamp();
				item.SendUse();
			}
		}
	}

	private static void handleContainerItemsEvent(ContainerItemsEvent e)
	{
		if ((!e.Container.IsCorpse && !e.Container.IsBones) || !World.Player.InRange(e.Container, 2))
		{
			return;
		}
		int num = 0;
		foreach (Item item in e.Items)
		{
			if (LootCorpseNG.ToLoot.Contains(item.ID))
			{
				if (item.InRange(World.Player, 2) && new MoveContext(item, item.Amount, World.Player, clickFirst: false).Enqueue())
				{
					int num2 = Math.Max(item.Amount, 1);
					Messenger.Info("Looting {0:N0} {1}", num2, Map.ReplaceAmount(Map.GetTileName(item.ID + 16384), num2));
				}
				num++;
			}
		}
		Item container = e.Container;
		if (!LootCorpseNG._corpses.TryGetValue(container.Serial, out var value))
		{
			value = new CorpseInfo(container.Serial, container.X, container.Y);
			LootCorpseNG._corpses[container.Serial] = value;
		}
		value.State = CorpseState.Looting;
		if (container.Items.Count <= 0 || num == 0)
		{
			value.State = CorpseState.Done;
		}
		value.LastAction = Stopwatch.GetTimestamp();
	}

	private static void handleMovementAcceptedEvent(MovementAcceptedEvent e)
	{
		foreach (CorpseInfo item2 in LootCorpseNG._corpses.Values.ToList())
		{
			double num = (double)(Stopwatch.GetTimestamp() - item2.LastAction) / (double)Stopwatch.Frequency * 1000.0;
			if ((item2.State != CorpseState.Opening || num >= 500.0) && item2.InRange(2))
			{
				Item item = World.FindItem(item2.Serial);
				if (item != null && item.Items.Count > 0)
				{
					item2.State = CorpseState.Opening;
					item2.LastAction = Stopwatch.GetTimestamp();
					item.SendUse();
					break;
				}
				LootCorpseNG._corpses.Remove(item2.Serial);
			}
		}
	}

	private static void handleEngineTickEvent(EngineTickEvent e)
	{
		if (World.Player.IsDead)
		{
			return;
		}
		foreach (Item value2 in World.Items.Values)
		{
			if ((value2.IsCorpse || value2.IsBones) && World.Player.InRange(value2, 2))
			{
				if (!LootCorpseNG._corpses.TryGetValue(value2.Serial, out var value))
				{
					value = new CorpseInfo(value2.Serial, value2.X, value2.Y);
					LootCorpseNG._corpses[value2.Serial] = value;
				}
				if (value2.Items.Count <= 0)
				{
					value.State = CorpseState.Done;
				}
				if (value.State == CorpseState.Discovered)
				{
					value.State = CorpseState.Opening;
					value2.Use();
					break;
				}
			}
		}
	}
}
