using System;
using System.Collections.Generic;
using Ultima.Client;
using UOAIO.Profiles;
using UOAIO.Targeting;

namespace UOAIO;

public class MacroHandlers
{
	private static Dictionary<string, WandEffect> _wandEffects;

	public static void Register(ActionCallback callback, string action)
	{
		MacroHandlers.Register(callback, action, (ParamNode[])null);
	}

	public static void Register(ActionCallback callback, string action, params string[] list)
	{
		ParamNode[] array = new ParamNode[list.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new ParamNode(list[i], list[i]);
		}
		MacroHandlers.Register(callback, action, array);
	}

	public static void Register(ActionCallback callback, string action, string[,] list)
	{
		ParamNode[] array = new ParamNode[list.GetLength(0)];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new ParamNode(list[i, 0], list[i, 1]);
		}
		MacroHandlers.Register(callback, action, array);
	}

	private static bool Toggle(bool old, string val)
	{
		if (val != null && val.Length > 0)
		{
			switch (val.ToLower())
			{
			case "yes":
			case "on":
			case "1":
				return true;
			case "off":
			case "no":
			case "0":
				return false;
			}
		}
		return !old;
	}

	public static void Register(ActionCallback callback, string action, params ParamNode[] options)
	{
		ActionHandler.Register(action, options, callback);
	}

	public static void Setup()
	{
		MacroHandlers.Register(Screenshots_OnAction, "Options|Screenshots@Death Screenshots", ParamNode.Toggle);
		MacroHandlers.Register(RegCounter_OnAction, "Interface|RegCounter@Reg Counter", ParamNode.Toggle);
		MacroHandlers.Register(Warmode_OnAction, "Actions|Warmode", ParamNode.Toggle);
		MacroHandlers.Register(Halos_OnAction, "Options|Halos@Halos", ParamNode.Toggle);
		MacroHandlers.Register(ParticleCount_OnAction, "Options|ParticleCount@Particle Counter", ParamNode.Toggle);
		MacroHandlers.Register(Temperature_OnAction, "Options|Temperature@Temperature", ParamNode.Toggle);
		MacroHandlers.Register(Ping_OnAction, "Options|Ping@Ping Display", ParamNode.Toggle);
		MacroHandlers.Register(Transparency_OnAction, "Options|Transparency", ParamNode.Toggle);
		MacroHandlers.Register(ContainerGrid_OnAction, "Options|ContainerGrid@Container Grid", ParamNode.Toggle);
		MacroHandlers.Register(Grid_OnAction, "Options|Grid@Terrain Grid", ParamNode.Toggle);
		MacroHandlers.Register(PumpFPS_OnAction, "Options|PumpFPS@Pump FPS", ParamNode.Toggle);
		MacroHandlers.Register(FPS_OnAction, "Options|FPS@Display FPS", ParamNode.Toggle);
		MacroHandlers.Register(MiniHealth_OnAction, "Options|MiniHealth@Mini Health", ParamNode.Toggle);
		MacroHandlers.Register(ToggleHotkeys_OnAction, "Options|ToggleHotkeys@Toggle Hotkeys", ParamNode.Toggle);
		ParamNode[] options = new ParamNode[3]
		{
			new ParamNode("Mage", new ParamNode[8]
			{
				new ParamNode("First", new string[8] { "Clumsy", "Create Food", "Feeblemind", "Heal", "Magic Arrow", "Night Sight", "Reactive Armor", "Weaken" }),
				new ParamNode("Second", new string[8] { "Agility", "Cunning", "Cure", "Harm", "Magic Trap", "Magic Untrap", "Protection", "Strength" }),
				new ParamNode("Third", new string[8] { "Bless", "Fireball", "Magic Lock", "Poison", "Telekinesis", "Teleport", "Unlock", "Wall of Stone" }),
				new ParamNode("Fourth", new string[8] { "Arch Cure", "Arch Protection", "Curse", "Fire Field", "Greater Heal", "Lightning", "Mana Drain", "Recall" }),
				new ParamNode("Fifth", new string[8] { "Blade Spirits", "Dispel Field", "Incognito", "Magic Reflection", "Mind Blast", "Paralyze", "Poison Field", "Summ. Creature" }),
				new ParamNode("Sixth", new string[8] { "Dispel", "Energy Bolt", "Explosion", "Invisibility", "Mark", "Mass Curse", "Paralyze Field", "Reveal" }),
				new ParamNode("Seventh", new string[8] { "Chain Lightning", "Energy Field", "Flame Strike", "Gate Travel", "Mana Vampire", "Mass Dispel", "Meteor Swarm", "Polymorph" }),
				new ParamNode("Eighth", new string[8] { "Earthquake", "Energy Vortex", "Resurrection", "Air Elemental", "Summon Daemon", "Earth Elemental", "Fire Elemental", "Water Elemental" })
			}),
			new ParamNode("Paladin", new string[10] { "Cleanse by Fire", "Close Wounds", "Consecrate Weapon", "Dispel Evil", "Divine Fury", "Enemy of One", "Holy Light", "Noble Sacrifice", "Remove Curse", "Sacred Journey" }),
			new ParamNode("Necromancer", new ParamNode[4]
			{
				new ParamNode("Curses", new string[5] { "Blood Oath", "Corpse Skin", "Curse Weapon", "Evil Omen", "Mind Rot" }),
				new ParamNode("Damaging", new string[4] { "Pain Spike", "Poison Strike", "Strangle", "Wither" }),
				new ParamNode("Transorming", new string[4] { "Horrific Beast", "Lich Form", "Vampiric Embrace", "Wraith Form" }),
				new ParamNode("Summoning", new string[3] { "Animate Dead", "Summon Familiar", "Vengeful Spirit" })
			})
		};
		MacroHandlers.Register(Cast_OnAction, "Actions|Cast@Cast Spell", options);
		string[,] list = new string[9, 2]
		{
			{ "Smart", "Smart" },
			{ "Cure", "Orange" },
			{ "Heal", "Yellow" },
			{ "Poison", "Green" },
			{ "Agility", "Blue" },
			{ "Refresh", "Red" },
			{ "Strength", "White" },
			{ "Explosion", "Purple" },
			{ "Night Sight", "Black" }
		};
		MacroHandlers.Register(UsePotion_OnAction, "Items|UsePotion@Use Potion", list);
		options = new ParamNode[14]
		{
			new ParamNode("Spellbook", new ParamNode[3]
			{
				new ParamNode("Mage", "Spellbook"),
				new ParamNode("Paladin", "PaladinSpellbook"),
				new ParamNode("Necromancer", "NecroSpellbook")
			}),
			new ParamNode("Combat Book", "Abilities"),
			new ParamNode("Backpack", "Backpack"),
			new ParamNode("Help", "Help"),
			new ParamNode("Journal", "Journal"),
			new ParamNode("Network Stats", "NetStats"),
			new ParamNode("Configuration", "Options"),
			new ParamNode("Paperdoll", "Paperdoll"),
			new ParamNode("Overview", "Radar"),
			new ParamNode("Skills", "Skills"),
			new ParamNode("Status", "Status"),
			new ParamNode("Radar", "radar"),
			new ParamNode("Macro Editor", "Macros"),
			new ParamNode("Info Browser", "InfoBrowser")
		};
		MacroHandlers.Register(Open_OnAction, "Interface|Open", options);
		options = new ParamNode[4]
		{
			new ParamNode("Actions", new string[11]
			{
				"Animal Taming", "Begging", "Detecting Hidden", "Hiding", "Meditation", "Poisoning", "Remove Trap", "Spirit Speak", "Stealing", "Stealth",
				"Tracking"
			}),
			new ParamNode("Lore & Knowledge", new string[7] { "Anatomy", "Animal Lore", "Arms Lore", "Evaluating Intelligence", "Forensic Evaluation", "Taste Identification", "Item Identification" }),
			new ParamNode("Crafting", new string[2] { "Inscription", "Cartography" }),
			new ParamNode("Bardic", new string[3] { "Discordance", "Peacemaking", "Provocation" })
		};
		MacroHandlers.Register(UseSkill_OnAction, "Actions|Use@Use Skill", options);
		options = new ParamNode[4]
		{
			new ParamNode("Self", "Self"),
			new ParamNode("Last", "Last"),
			new ParamNode("Acquire", "Acquire"),
			new ParamNode("Find", "Find")
		};
		MacroHandlers.Register(Target_OnAction, "Actions|Target", options);
		options = new ParamNode[3]
		{
			new ParamNode("Screen", "Screen"),
			new ParamNode("Target Queue", "TargetQueue"),
			new ParamNode("Target Cursor", "Target")
		};
		MacroHandlers.Register(Clear_OnAction, "Other|Clear", options);
		MacroHandlers.Register(PropertiesTargetHandler.OnMacro, "Other|Target Info@Target Info", ParamNode.Empty);
		MacroHandlers.Register(DelayMacro_OnAction, "Other|DelayMacro@Delay Macro");
		MacroHandlers.Register(Repeat_OnAction, "Other|Repeat", "Speech", "Macro");
		MacroHandlers.Register(UseOnce_OnAction, "Items|UseOnce@Use Once", "Use", "Set");
		MacroHandlers.Register(UseObjectByID_OnAction, "Items|Use Object ByI D@Use Object By ID");
		MacroHandlers.Register(UseItemByType_OnAction, "Items|UseItemByType@Use By Type", "Bola", "Bandage", "Dagger", "Candle", "Moongate", "Purple Petal", "Orange Petal");
		MacroHandlers.Register(UseItemInHand_OnAction, "Items|UseItemInHand@Use In Hand");
		MacroHandlers.Register(UseWand_OnAction, "Items|UseWand@Use Wand", "Identification", "Clumsy", "Feeblemind", "Harm", "Magic Arrow", "Weaken", "Fireball", "Heal", "Greater Heal", "Lightning", "Mana Drain");
		MacroHandlers.Register(SetEquip_OnAction, "Equipment|SetEquip@Set Equipment", ParamNode.Count(0, 10, "Slot {0}"));
		MacroHandlers.Register(Equip_OnAction, "Equipment|Arm", ParamNode.Count(0, 10, "Slot {0}"));
		MacroHandlers.Register(Dequip_OnAction, "Equipment|Disarm", ParamNode.Empty);
		MacroHandlers.Register(Dress_OnAction, "Equipment|Dress", ParamNode.Empty);
		MacroHandlers.Register(Bow_OnAction, "Other|Animations|Bow", ParamNode.Empty);
		MacroHandlers.Register(Salute_OnAction, "Other|Animations|Salute", ParamNode.Empty);
		MacroHandlers.Register(Wrestle_OnAction, "Other|Wrestle@Wrestle Move", "Disarm", "Stun");
		MacroHandlers.Register(Resync_OnAction, "Other|Resync@Resynchronize", ParamNode.Empty);
		MacroHandlers.Register(BandageSelf_OnAction, "Actions|BandageSelf@Bandage Self", ParamNode.Empty);
		MacroHandlers.Register(Paste_OnAction, "Interface|Paste");
		MacroHandlers.Register(Dismount_OnAction, "Actions|Dismount", ParamNode.Empty);
		MacroHandlers.Register(Remount_OnAction, "Actions|Remount", ParamNode.Empty);
		MacroHandlers.Register(StopMacros_OnAction, "Other|StopMacros@Stop Macros", ParamNode.Empty);
		MacroHandlers.Register(WaitForTargetLast_OnAction, "Other|WaitForTargetLast", ParamNode.Empty);
		MacroHandlers.Register(WaitForTarget_OnAction, "Other|WaitForTarget", ParamNode.Empty);
		MacroHandlers.Register(Quit_OnAction, "Interface|Quit", ParamNode.Empty);
		MacroHandlers.Register(AllNames_OnAction, "Interface|AllNames@All Names", ParamNode.Empty);
		MacroHandlers.Register(SetAbility_OnAction, "Other|Ability@Set Ability", "Primary", "Secondary", "None");
		MacroHandlers.Register(Count_OnAction, "Interface|Count", "Regs", "Ammo");
		MacroHandlers.Register(Attack_OnAction, "Other|Attack", "Last", "Red");
		MacroHandlers.Register(Last_OnAction, "Actions|Last", "Object", "Skill", "Spell");
		MacroHandlers.Register(Disrupt_OnAction, "Actions|Disrupt", ParamNode.Empty);
		MacroHandlers.Register(Say_OnAction, "Interface|Say");
		MacroHandlers.Register(RestoreSpeech_OnAction, "Interface|Restore@Restore Speech", ParamNode.Empty);
	}

	private static bool RestoreSpeech_OnAction(string args)
	{
		Engine.RestoreSpeech();
		return true;
	}

	private static bool Clear_OnAction(string args)
	{
		string[] array = args.ToLower().Split(' ');
		if (array[0] == "screen")
		{
			Engine.ClearScreen();
		}
		else if (array[0] == "targetqueue")
		{
			TargetManager.ClearQueue();
		}
		else if (array[0] == "target" && TargetManager.IsActive)
		{
			TargetManager.Active.Cancel();
		}
		return true;
	}

	private static bool Disrupt_OnAction(string args)
	{
		Engine.Disturb();
		return true;
	}

	private static bool Target_OnAction(string args)
	{
		if (args != null && args.Length > 0)
		{
			switch (args.ToLowerInvariant())
			{
			case "find":
				TargetManager.Reacquire();
				break;
			case "aquire":
			case "acquire":
				TargetManager.TargetAcquire();
				break;
			case "smart":
			case "last":
				TargetManager.TargetSmart();
				break;
			case "self":
				TargetManager.TargetSelf();
				break;
			}
		}
		return true;
	}

	private static bool Say_OnAction(string args)
	{
		Engine.m_SayMacro = true;
		Engine.commandEntered(Engine.Encode(args));
		Engine.m_SayMacro = false;
		return true;
	}

	public static bool UseWand_OnAction(string args)
	{
		if (MacroHandlers._wandEffects == null)
		{
			MacroHandlers._wandEffects = MacroHandlers.CreateWandEffectTable();
		}
		if (MacroHandlers._wandEffects.TryGetValue(args, out var value))
		{
			Mobile player = World.Player;
			if (player != null)
			{
				Item item = WandRepository.Find(value);
				if (item != null)
				{
					if (item.Parent == player)
					{
						item.Use();
					}
					else
					{
						Item item2 = player.FindEquip(Layer.OneHanded);
						if (item2 != null)
						{
							Item backpack = player.Backpack;
							if (backpack != null)
							{
								new MoveContext(item2, item2.Amount, backpack, clickFirst: false).Enqueue();
							}
						}
						item2 = player.FindEquip(Layer.TwoHanded);
						if (item2 != null)
						{
							Item backpack2 = player.Backpack;
							if (backpack2 != null)
							{
								new MoveContext(item2, item2.Amount, backpack2, clickFirst: false).Enqueue();
							}
						}
						new EquipContext(item, item.Amount, player, clickFirst: false).Enqueue();
						new UseContext(item, isManual: false).Enqueue();
					}
				}
				else
				{
					Engine.AddTextMessage("Wand not found.", Engine.DefaultFont, Hues.Load(38));
				}
			}
		}
		return true;
	}

	private static Dictionary<string, WandEffect> CreateWandEffectTable()
	{
		Dictionary<string, WandEffect> dictionary = new Dictionary<string, WandEffect>(StringComparer.OrdinalIgnoreCase);
		dictionary["Identification"] = WandEffect.Identification;
		dictionary["Clumsy"] = WandEffect.Clumsiness;
		dictionary["Feeblemind"] = WandEffect.Feeblemindedness;
		dictionary["Harm"] = WandEffect.Harming;
		dictionary["Magic Arrow"] = WandEffect.MagicArrow;
		dictionary["Weaken"] = WandEffect.Weakness;
		dictionary["Fireball"] = WandEffect.Fireball;
		dictionary["Heal"] = WandEffect.Healing;
		dictionary["Greater Heal"] = WandEffect.GreaterHealing;
		dictionary["Lightning"] = WandEffect.Lightning;
		dictionary["Mana Drain"] = WandEffect.ManaDraining;
		return dictionary;
	}

	public static bool UseItemInHand_OnAction(string args)
	{
		Mobile player = World.Player;
		if (player != null)
		{
			(player.FindEquip(Layer.OneHanded) ?? player.FindEquip(Layer.TwoHanded))?.Use();
		}
		return true;
	}

	private static bool UseItemByType_OnAction(string args)
	{
		if (args != null && args.Length > 0)
		{
			int[] array = null;
			switch (args.ToLower())
			{
			case "bola":
				array = new int[1] { 9900 };
				break;
			case "bandage":
				array = new int[2] { 3617, 3817 };
				break;
			case "dagger":
				array = new int[2] { 3921, 3922 };
				break;
			case "candle":
				array = new int[2] { 2575, 2600 };
				break;
			case "moongate":
				Engine.UseMoongate();
				break;
			case "trinsic petal":
			case "strength petal":
			case "purple petal":
				Engine.UseItemByTypeAndHue(new int[1] { 4129 }, 14);
				return true;
			case "orange petal":
				Engine.UseItemByTypeAndHue(new int[1] { 4129 }, 43);
				return true;
			default:
				try
				{
					int num = int.Parse(args);
					array = new int[1] { num };
				}
				catch
				{
				}
				break;
			}
			if (array != null)
			{
				Engine.UseItemByType(array);
			}
		}
		return true;
	}

	private static bool LastObject_OnAction(string args)
	{
		PUseRequest.SendLast();
		return true;
	}

	private static bool UseObjectByID_OnAction(string args)
	{
		if (args != null && args.Length > 0)
		{
			foreach (Item value in World.Items.Values)
			{
				if (value.ID.ToString() == args)
				{
					Network.Send(new PUseRequest(value));
					return true;
				}
			}
			Engine.AddTextMessage("No item with the id of " + args + " was found.");
		}
		return true;
	}

	private static bool Last_OnAction(string args)
	{
		if (args != null && args.Length > 0)
		{
			switch (args.ToLower())
			{
			case "object":
				PUseRequest.SendLast();
				break;
			case "spell":
				PCastSpell.SendLast();
				break;
			case "skill":
				PUseSkill.SendLast();
				break;
			}
		}
		return true;
	}

	private static bool Attack_OnAction(string args)
	{
		if (args != null && args.Length > 0)
		{
			string text = args.ToLower();
			string text2 = text;
			if (text2 == "last")
			{
				Engine.AttackLast();
			}
		}
		return true;
	}

	private static bool Count_OnAction(string args)
	{
		if (args != null && args.Length > 0)
		{
			string text = args.ToLower();
			string text2 = text;
			if (!(text2 == "regs"))
			{
				if (text2 == "ammo")
				{
					Engine.CountAmmo();
				}
			}
			else
			{
				Engine.CountReagents();
			}
		}
		return true;
	}

	private static bool UseSkill_OnAction(string args)
	{
		if (args != null && args.ToLower() == "smartpotion")
		{
			Engine.SmartPotion();
		}
		else
		{
			Skill skill = Engine.Skills[Engine.Skills.GetSkill(args)];
			if (skill != null)
			{
				skill.Use();
			}
			else
			{
				Engine.AddTextMessage($"Unknown skill '{args}'");
			}
		}
		return true;
	}

	private static bool SetAbility_OnAction(string args)
	{
		if (args != null && args.Length > 0)
		{
			switch (args.ToLower())
			{
			case "primary":
				AbilityInfo.Active = AbilityInfo.Primary;
				break;
			case "secondary":
				AbilityInfo.Active = AbilityInfo.Secondary;
				break;
			case "none":
				AbilityInfo.Active = null;
				break;
			}
		}
		return true;
	}

	private static bool AllNames_OnAction(string args)
	{
		Engine.AllNames();
		return true;
	}

	private static bool Quit_OnAction(string args)
	{
		Engine.Quit();
		return true;
	}

	private static bool StopMacros_OnAction(string args)
	{
		Macros.StopAll();
		return true;
	}

	private static bool WaitForTarget_OnAction(string args)
	{
		return TargetManager.IsActive;
	}

	private static bool WaitForTargetLast_OnAction(string args)
	{
		return TargetManager.IsActive && TargetManager.LastTarget != null && TargetManager.TargetIsInRange();
	}

	private static bool Dismount_OnAction(string args)
	{
		Engine.Dismount();
		return true;
	}

	private static bool Remount_OnAction(string args)
	{
		Engine.Remount();
		return true;
	}

	private static bool Paste_OnAction(string args)
	{
		if (args != null && args.Length > 0)
		{
			Engine.Paste(args);
		}
		else
		{
			Engine.Paste();
		}
		return true;
	}

	private static bool BandageSelf_OnAction(string args)
	{
		Engine.BandageSelf();
		return true;
	}

	private static bool Resync_OnAction(string args)
	{
		Engine.Resync();
		return true;
	}

	private static bool Wrestle_OnAction(string args)
	{
		try
		{
			switch ((WrestleType)Enum.Parse(typeof(WrestleType), args, ignoreCase: true))
			{
			case WrestleType.Stun:
				return Network.Send(new PWrestleStun());
			case WrestleType.Disarm:
				return Network.Send(new PWrestleDisarm());
			}
		}
		catch
		{
			Engine.AddTextMessage($"Unknown wrestle type: {args}");
		}
		return true;
	}

	private static bool Open_OnAction(string args)
	{
		if (args != null && args.Length > 0)
		{
			switch (args.ToLower())
			{
			case "help":
				Engine.OpenHelp();
				break;
			case "options":
				Engine.OpenOptions();
				break;
			case "journal":
				Engine.OpenJournal();
				break;
			case "skills":
				Engine.OpenSkills();
				break;
			case "status":
				Engine.OpenStatus();
				break;
			case "spellbook":
				Engine.OpenSpellbook(1);
				break;
			case "necrospellbook":
				Engine.OpenSpellbook(2);
				break;
			case "paladinspellbook":
				Engine.OpenSpellbook(3);
				break;
			case "paperdoll":
				Engine.OpenPaperdoll();
				break;
			case "backpack":
				Engine.OpenBackpack();
				break;
			case "radar":
				GRadar.Open();
				break;
			case "abilities":
				GCombatGump.Open();
				break;
			case "macros":
				GMacroEditorForm.Open();
				break;
			}
		}
		return true;
	}

	private static bool Bow_OnAction(string args)
	{
		return Network.Send(new PAction("bow"));
	}

	private static bool Salute_OnAction(string args)
	{
		return Network.Send(new PAction("salute"));
	}

	private static bool SetEquip_OnAction(string args)
	{
		try
		{
			int equip = Convert.ToInt32(args);
			Engine.SetEquip(equip);
		}
		catch
		{
		}
		return true;
	}

	private static bool Equip_OnAction(string args)
	{
		try
		{
			int index = Convert.ToInt32(args);
			Engine.Equip(index);
		}
		catch
		{
		}
		return true;
	}

	private static bool Dequip_OnAction(string args)
	{
		Engine.Dequip();
		return true;
	}

	private static bool Dress_OnAction(string args)
	{
		Engine.Dress();
		return true;
	}

	private static bool UsePotion_OnAction(string args)
	{
		if (args.ToLower() == "smart")
		{
			Engine.SmartPotion();
			return true;
		}
		try
		{
			PotionType type = (PotionType)Enum.Parse(typeof(PotionType), args, ignoreCase: true);
			if (!Engine.UsePotion(type))
			{
				Engine.AddTextMessage($"You do not have any {type.ToString().ToLower()} potions!", Engine.DefaultFont, Hues.Load(34));
			}
		}
		catch
		{
			Engine.AddTextMessage($"Unknown potion type: {args}");
		}
		return true;
	}

	private static bool UseOnce_OnAction(string args)
	{
		if (args != null && args.Length > 0)
		{
			string text = args.ToLower();
			string text2 = text;
			if (!(text2 == "set"))
			{
				if (text2 == "use")
				{
					Engine.AutoUse();
				}
			}
			else
			{
				Engine.SetAutoUse();
			}
		}
		return true;
	}

	private static bool Cast_OnAction(string args)
	{
		Spells.GetSpellByName(args)?.Cast();
		return true;
	}

	private static bool DelayMacro_OnAction(string args)
	{
		try
		{
			int ms = int.Parse(args);
			return Macro.Delay(ms);
		}
		catch
		{
		}
		return true;
	}

	private static bool Repeat_OnAction(string args)
	{
		if (args != null && args.Length > 0)
		{
			string text = args.ToLower();
			string text2 = text;
			if (!(text2 == "speech"))
			{
				if (text2 == "macro")
				{
					Macro.Repeat();
				}
			}
			else
			{
				Engine.Repeat();
			}
		}
		return true;
	}

	private static bool Screenshots_OnAction(string args)
	{
		Options.Current.Screenshots = MacroHandlers.Toggle(Options.Current.Screenshots, args);
		return true;
	}

	private static bool RegCounter_OnAction(string args)
	{
		GItemCounters.Active = MacroHandlers.Toggle(GItemCounters.Active, args);
		return true;
	}

	private static bool Warmode_OnAction(string args)
	{
		Engine.Warmode = MacroHandlers.Toggle(Engine.Warmode, args);
		return true;
	}

	private static bool Halos_OnAction(string args)
	{
		Options.Current.NotorietyHalos = MacroHandlers.Toggle(Options.Current.NotorietyHalos, args);
		return true;
	}

	private static bool ParticleCount_OnAction(string args)
	{
		Renderer.DrawPCount = MacroHandlers.Toggle(Renderer.DrawPCount, args);
		return true;
	}

	private static bool Temperature_OnAction(string args)
	{
		Engine.Effects.DrawTemperature = MacroHandlers.Toggle(Engine.Effects.DrawTemperature, args);
		return true;
	}

	private static bool Ping_OnAction(string args)
	{
		Renderer.DrawPing = MacroHandlers.Toggle(Renderer.DrawPing, args);
		return true;
	}

	private static bool Transparency_OnAction(string args)
	{
		Renderer.Transparency = MacroHandlers.Toggle(Renderer.Transparency, args);
		return true;
	}

	private static bool ContainerGrid_OnAction(string args)
	{
		Options.Current.ContainerGrid = MacroHandlers.Toggle(Options.Current.ContainerGrid, args);
		return true;
	}

	private static bool Grid_OnAction(string args)
	{
		Engine.Grid = MacroHandlers.Toggle(Engine.Grid, args);
		return true;
	}

	private static bool PumpFPS_OnAction(string args)
	{
		Engine.m_PumpFPS = MacroHandlers.Toggle(Engine.m_PumpFPS, args);
		return true;
	}

	private static bool FPS_OnAction(string args)
	{
		Engine.FPS = MacroHandlers.Toggle(Engine.FPS, args);
		return true;
	}

	private static bool MiniHealth_OnAction(string args)
	{
		Options.Current.MiniHealth = MacroHandlers.Toggle(Options.Current.MiniHealth, args);
		return true;
	}

	private static bool ToggleHotkeys_OnAction(string args)
	{
		Options.Current.HotkeysEnabled = MacroHandlers.Toggle(Options.Current.HotkeysEnabled, args);
		return true;
	}
}
