using System;
using System.Collections;
using Ultima.Client;
using UOAIO.Profiles;
using UOAIO.Targeting;

namespace UOAIO;

public class ClientFormat : CommandFormat
{
	private class LeaveHouseAction : ActionContext
	{
		protected override bool CheckDispatch()
		{
			foreach (ActionContext item in ActionContext.Pending)
			{
				if (item is LeaveHouseAction)
				{
					return false;
				}
			}
			return base.CheckDispatch();
		}

		public override void OnDispatch()
		{
			Mobile player = World.Player;
			if (player != null)
			{
				Network.Send(new PPopupRequest(player));
				player.AddTextMessage(player.Name, "- leaving -", Engine.DefaultFont, Hues.Load(38), unremovable: true);
			}
			base.OnDispatch();
		}

		public override bool OnContextItem(object owner, int entryID, int stringID)
		{
			return stringID == 6207;
		}

		public override bool OnContextEnd(object owner, bool selected)
		{
			return false;
		}
	}

	private Hashtable m_GiveEntries;

	private void Recall_OnCommand(CommandArgs args)
	{
		string argument = args.GetArgument(0);
		TravelAgent travelAgent = Player.Current.TravelAgent;
		foreach (RunebookInfo runebook in travelAgent.Runebooks)
		{
			if (!runebook.IsValid)
			{
				continue;
			}
			foreach (RuneInfo rune in runebook.Runes)
			{
				if (string.Equals(rune.Name, argument, StringComparison.CurrentCultureIgnoreCase))
				{
					new TravelContext(runebook, rune, recall: true).Enqueue();
					return;
				}
			}
		}
		Engine.AddTextMessage("No rune with that name was found.");
	}

	private void Gate_OnCommand(CommandArgs args)
	{
		string argument = args.GetArgument(0);
		TravelAgent travelAgent = Player.Current.TravelAgent;
		foreach (RunebookInfo runebook in travelAgent.Runebooks)
		{
			if (!runebook.IsValid)
			{
				continue;
			}
			foreach (RuneInfo rune in runebook.Runes)
			{
				if (string.Equals(rune.Name, argument, StringComparison.CurrentCultureIgnoreCase))
				{
					new TravelContext(runebook, rune, recall: false).Enqueue();
					return;
				}
			}
		}
		Engine.AddTextMessage("No rune with that name was found.");
	}

	private void LeaveHouse_OnCommand(CommandArgs args)
	{
		new LeaveHouseAction().Dispatch();
	}

	private void OpenRunebooks(CommandArgs args)
	{
		foreach (Item item in World.Items.Values)
		{
			if (item.ID == 8901 || item.ID == 3643 || item.ID == 3834)
			{
				new GenericContext(delegate
				{
					Network.Send(new PPE_QueryRunebookContent(item));
				}).Enqueue();
			}
		}
	}

	private void Target_OnCommand(CommandArgs args)
	{
		TargetManager.Client = new AcquireTargetHandler();
	}

	private void Acquire_OnCommand(CommandArgs args)
	{
		TargetManager.Reacquire();
	}

	private void AutoPickup_OnCommand(CommandArgs args)
	{
		bool scavenger = !Options.Current.Scavenger;
		if (args.Length > 0)
		{
			scavenger = args.GetBoolean(0);
		}
		Options.Current.Scavenger = scavenger;
	}

	private void AlwaysRun_OnCommand(CommandArgs args)
	{
		bool alwaysRun = !Options.Current.AlwaysRun;
		if (args.Length > 0)
		{
			alwaysRun = args.GetBoolean(0);
		}
		Options.Current.AlwaysRun = alwaysRun;
	}

	private void SmoothWalk_OnCommand(CommandArgs args)
	{
		bool smoothWalk = !Options.Current.SmoothWalk;
		if (args.Length > 0)
		{
			smoothWalk = args.GetBoolean(0);
		}
		Options.Current.SmoothWalk = smoothWalk;
	}

	private void QueueTargets_OnCommand(CommandArgs args)
	{
		bool queueTargets = !Options.Current.QueueTargets;
		if (args.Length > 0)
		{
			queueTargets = args.GetBoolean(0);
		}
		Options.Current.QueueTargets = queueTargets;
	}

	private void Footsteps_OnCommand(CommandArgs args)
	{
		bool flag = !Preferences.Current.Footsteps.Volume.Mute;
		if (args.Length > 0)
		{
			flag = !args.GetBoolean(0);
		}
		Preferences.Current.Footsteps.Volume.Mute = flag;
		Engine.AddTextMessage(string.Format("Footsteps are {0}", flag ? "muted" : "unmuted"));
	}

	private void Sound_OnCommand(CommandArgs args)
	{
		bool flag = !Preferences.Current.Sound.Volume.Mute;
		if (args.Length > 0)
		{
			flag = !args.GetBoolean(0);
		}
		Preferences.Current.Sound.Volume.Mute = flag;
		Engine.AddTextMessage(string.Format("Sound is {0}", flag ? "muted" : "unmuted"));
	}

	private void Music_OnCommand(CommandArgs args)
	{
		string text = ((args.Length > 0) ? args.GetString(0) : null);
		if (string.Equals(text, "stop", StringComparison.OrdinalIgnoreCase))
		{
			Music.Stop();
			return;
		}
		bool? flag = null;
		if (string.Equals(text, "on", StringComparison.OrdinalIgnoreCase))
		{
			flag = false;
		}
		else if (string.Equals(text, "off", StringComparison.OrdinalIgnoreCase))
		{
			flag = true;
		}
		else if (text == null || string.Equals(text, "toggle", StringComparison.OrdinalIgnoreCase))
		{
			flag = !Preferences.Current.Music.Volume.Mute;
		}
		if (flag.HasValue)
		{
			Preferences.Current.Music.Volume.Mute = flag.Value;
			if (flag.Value)
			{
				Engine.AddTextMessage("Music disabled.");
				Music.Stop();
			}
			else
			{
				Engine.AddTextMessage("Music enabled.");
			}
		}
		else
		{
			Engine.AddTextMessage("Use 'on', 'off', 'toggle', or 'stop'.");
		}
	}

	private void UseGate_OnCommand(CommandArgs args)
	{
		Engine.UseMoongate();
	}

	private void DragToBag_OnCommand(CommandArgs args)
	{
		TargetManager.Client = new DragToBagTargetHandler(clickFirst: false);
		Engine.AddTextMessage("Target an item to move.");
	}

	private void Move_OnCommand(CommandArgs args)
	{
		if (args.Length == 0)
		{
			TargetManager.Client = new MoveTargetHandler(null);
		}
		else
		{
			TargetManager.Client = new MoveTargetHandler(args.GetInt32(0));
		}
		Engine.AddTextMessage("Target one of the items to move.");
	}

	private void Stack_OnCommand(CommandArgs args)
	{
		TargetManager.Client = new StackTargetHandler();
		Engine.AddTextMessage("Target the destination item.");
	}

	private void BringTo_OnCommand(CommandArgs args)
	{
		if (args.Length == 0)
		{
			TargetManager.Client = new BringToTargetHandler(0, 0);
			Engine.AddTextMessage("Target the destination item.");
		}
		else if (args.Length == 2)
		{
			TargetManager.Client = new BringToTargetHandler(args.GetInt32(0), args.GetInt32(1));
			Engine.AddTextMessage("Target the destination item.");
		}
		else
		{
			Engine.AddTextMessage("Use 'bringto' or 'bringto x y'.");
		}
	}

	private void RegDrop_OnCommand(CommandArgs args)
	{
		TargetManager.Client = new RegDropTargetHandler();
		Engine.AddTextMessage("Target the destination container.");
	}

	private void PotDrop_OnCommand(CommandArgs args)
	{
		TargetManager.Client = new PotDropTargetHandler();
		Engine.AddTextMessage("Target the destination container.");
	}

	private void Disturb_OnCommand(CommandArgs args)
	{
		Engine.Disturb();
	}

	private void Friend_OnCommand(CommandArgs args)
	{
		TargetManager.Client = new FriendTargetHandler();
		Engine.AddTextMessage("Target a player to toggle their friendship status.", Engine.DefaultFont, Hues.Load(89));
	}

	private void Ignore_OnCommand(CommandArgs args)
	{
		TargetManager.Client = new IgnoreTargetHandler();
		Engine.AddTextMessage("Target a player to toggle their ignored status.", Engine.DefaultFont, Hues.Load(89));
	}

	private void TurnTo_OnCommand(CommandArgs args)
	{
		TargetManager.Client = new TurnTargetHandler();
		Engine.AddTextMessage("Turn to where?");
	}

	private void Disarm_OnCommand(CommandArgs args)
	{
		Network.Send(new PWrestleDisarm());
	}

	private void Stun_OnCommand(CommandArgs args)
	{
		Network.Send(new PWrestleStun());
	}

	private void Remove_OnCommand(CommandArgs args)
	{
		TargetManager.Client = new RemoveTargetHandler();
		Engine.AddTextMessage("Remove what?");
	}

	private void Scavenge_OnCommand(CommandArgs args)
	{
		if (args.Length > 0)
		{
			switch (args.GetString(0).ToLower())
			{
			case "all":
				TargetManager.Client = new ScavengerTargetHandler(isAdd: true, byType: true);
				break;
			case "only":
				TargetManager.Client = new ScavengerTargetHandler(isAdd: true, byType: false);
				break;
			case "remove":
				TargetManager.Client = new ScavengerTargetHandler(isAdd: false, byType: true);
				break;
			default:
				Engine.AddTextMessage("Use 'all', 'only', or 'remove'.");
				break;
			}
		}
		else
		{
			Preferences.Current.Scavenger.Scavenge(isManual: true);
		}
	}

	private void NotoQuery_OnCommand(CommandArgs args)
	{
		NotoQueryType notoQueryType = (NotoQueryType)((int)(1 + Options.Current.NotorietyQuery) % 3);
		if (args.Length > 0)
		{
			if (args.GetString(0).ToLower() == "smart")
			{
				notoQueryType = NotoQueryType.Smart;
			}
			else
			{
				notoQueryType = (args.GetBoolean(0) ? NotoQueryType.On : NotoQueryType.Off);
			}
		}
		Preferences.Current.Scavenger.Scavenge(isManual: true);
	}

	private void HouseLevel_OnCommand(CommandArgs args)
	{
		if (args.Length == 1)
		{
			int houseLevel = Options.Current.HouseLevel;
			houseLevel = args.GetString(0).ToLower() switch
			{
				"up" => houseLevel + 1, 
				"down" => houseLevel - 1, 
				"on" => 1, 
				"off" => 5, 
				_ => args.GetInt32(0), 
			};
			Options.Current.HouseLevel = houseLevel;
		}
	}

	private void GMFollow_OnCommand(CommandArgs args)
	{
		if (!Engine.GMPrivs)
		{
			Engine.AddTextMessage("You do not have access to this command.");
			return;
		}
		if (TargetManager.LastTarget == null)
		{
			Engine.AddTextMessage("You do not have a last target set.");
			return;
		}
		Engine.m_GMFollow = true;
		if (args.GetString(0) == "stop")
		{
			Engine.m_GMFollow = false;
		}
		else
		{
			Engine.AddTextMessage("Type \". follow stop\" in order to stop the macro.");
		}
	}

	private void Speed_OnCommand(CommandArgs args)
	{
		if (!Engine.GMPrivs)
		{
			Engine.AddTextMessage("You do not have access to this command.");
			return;
		}
		float num = 0.4f;
		if (args.Length > 0)
		{
			int num2 = args.GetInt32(0) - 1;
			if (num2 < 0)
			{
				num2 = 0;
			}
			num -= (float)num2 * 0.1f;
			if (num < 0f)
			{
				num = 0f;
			}
		}
		Walking.Speed = num;
		Engine.AddTextMessage($"Speed set to {(float)World.Player.Speed / Walking.RunSpeed:N0} tiles/sec.");
	}

	private void RunSpeed_OnCommand(CommandArgs args)
	{
		if (!Engine.GMPrivs)
		{
			Engine.AddTextMessage("You do not have access to this command.");
			return;
		}
		int num = 200;
		if (args.Length > 0)
		{
			num = args.GetInt32(0);
		}
		Walking.RunSpeed = (float)num * 0.001f;
		Engine.AddTextMessage($"Running speed set to {(float)World.Player.Speed / Walking.RunSpeed:N0} tiles/sec.");
	}

	private void WalkSpeed_OnCommand(CommandArgs args)
	{
		if (!Engine.GMPrivs)
		{
			Engine.AddTextMessage("You do not have access to this command.");
			return;
		}
		int num = 400;
		if (args.Length > 0)
		{
			num = args.GetInt32(0);
		}
		Walking.WalkSpeed = (float)num * 0.001f;
		Engine.AddTextMessage($"Walking speed set to {(float)World.Player.Speed / Walking.WalkSpeed:N0} tiles/sec.");
	}

	private void DropTarg_OnCommand(CommandArgs args)
	{
		if (TargetManager.IsActive)
		{
			TargetManager.Active.Cancel();
		}
	}

	private void CancelStealth_OnCommand(CommandArgs args)
	{
		if (Engine.m_Stealth)
		{
			Engine.m_Stealth = false;
			Engine.m_StealthSteps = 0;
			Engine.AddTextMessage("You have deactivated stealth.");
		}
		else
		{
			Engine.AddTextMessage("Stealth has not yet been activated.");
		}
	}

	private void RegisterGive(GiveEntry entry)
	{
		this.m_GiveEntries[entry.Name] = entry;
	}

	private void Give_OnCommand(CommandArgs args)
	{
		if (this.m_GiveEntries == null)
		{
			this.m_GiveEntries = new Hashtable(StringComparer.OrdinalIgnoreCase);
			this.RegisterGive(new GiveFixedEntry("Recalls", 2, new ItemIDValidator(3974), new ItemIDValidator(3962), new ItemIDValidator(3963)));
			this.RegisterGive(new GiveFixedEntry("Gates", 2, new ItemIDValidator(3974), new ItemIDValidator(3962), new ItemIDValidator(3980)));
			this.RegisterGive(new GiveRatioEntry("Regs", 25, new ItemIDValidator(3962), new ItemIDValidator(3963), new ItemIDValidator(3972), new ItemIDValidator(3973), new ItemIDValidator(3974), new ItemIDValidator(3976), new ItemIDValidator(3980), new ItemIDValidator(3981)));
			this.RegisterGive(new GiveRatioEntry("Drake", 25, new ItemIDValidator(3974)));
			this.RegisterGive(new GiveRatioEntry("Pearl", 25, new ItemIDValidator(3962)));
			this.RegisterGive(new GiveRatioEntry("Moss", 25, new ItemIDValidator(3963)));
			this.RegisterGive(new GiveRatioEntry("Garlic", 25, new ItemIDValidator(3972)));
			this.RegisterGive(new GiveRatioEntry("Shade", 25, new ItemIDValidator(3976)));
			this.RegisterGive(new GiveRatioEntry("Ginseng", 25, new ItemIDValidator(3973)));
			this.RegisterGive(new GiveRatioEntry("Silk", 25, new ItemIDValidator(3981)));
			this.RegisterGive(new GiveRatioEntry("Ash", 25, new ItemIDValidator(3980)));
		}
		if (args.Length > 0)
		{
			if (this.m_GiveEntries[args.GetString(0)] is GiveEntry giveEntry)
			{
				int num = -1;
				if (args.Length > 1)
				{
					num = args.GetInt32(1);
				}
				if (num > 0 || num == -1)
				{
					TargetManager.Client = new GiveTargetHandler(giveEntry, num);
					Engine.AddTextMessage(string.Format("Who do you wish to give {0} to?", (giveEntry.Validators.Length == 1) ? "this item" : "these items"));
				}
				else
				{
					Engine.AddTextMessage("You have specified an invalid amount.");
				}
			}
			else
			{
				Engine.AddTextMessage("You have specified an unknown item name.");
			}
		}
		else
		{
			Engine.AddTextMessage("You must specify an item name.");
		}
	}

	protected override void OnDefault(CommandArgs args)
	{
		Engine.AddTextMessage("You have entered an unknown command.");
	}

	public void FPS_OnCommand(CommandArgs args)
	{
		int num = 100;
		if (args.Length > 0)
		{
			num = args.GetInt32(0);
		}
		Timer timer = new Timer(Engine.TimeRefresh_OnTick, 1, 1);
		timer.SetTag("Frames", num);
		timer.Start(Now: false);
	}

	public ClientFormat(string prepend, string prefix, string format, byte messageType, SpeechType speechType)
		: base(prepend, prefix, format, messageType, speechType)
	{
		base.Register("Disturb", Disturb_OnCommand);
		base.Register("LeaveHouse", LeaveHouse_OnCommand);
		base.Register("OpenRunebooks", OpenRunebooksEx);
		base.Register("Dress", delegate
		{
			Engine.Dress();
		});
		base.Register("Recall", Recall_OnCommandEx);
		base.Register("Gate", Gate_OnCommandEx);
		base.Register("Target", Target_OnCommand);
		base.Register("Acquire", Acquire_OnCommand);
		base.Register("AlwaysRun", AlwaysRun_OnCommand);
		base.Register("SmoothWalk", SmoothWalk_OnCommand);
		base.Register("Footsteps", Footsteps_OnCommand);
		base.Register("Sound", Sound_OnCommand);
		base.Register("Music", Music_OnCommand);
		base.Register("QueueTargets", QueueTargets_OnCommand);
		base.Register("Remove", Remove_OnCommand);
		base.Register("UseGate", UseGate_OnCommand);
		base.Register("Friend", Friend_OnCommand);
		base.Register("FriendLoc", ClientFormatEx.FriendLoc_OnCommand);
		base.Register("Ignore", Ignore_OnCommand);
		base.Register("Scavenge", Scavenge_OnCommand);
		base.Register("RegDrop", RegDrop_OnCommand);
		base.Register("PotDrop", PotDrop_OnCommand);
		base.Register("DragToBag", DragToBag_OnCommand);
		base.Register("Move", Move_OnCommand);
		base.Register("Stack", Stack_OnCommand);
		base.Register("BringTo", BringTo_OnCommand);
		base.Register("TurnTo", TurnTo_OnCommand);
		base.Register("Disarm", Disarm_OnCommand);
		base.Register("Stun", Stun_OnCommand);
		base.Register("Noto", NotoQuery_OnCommand);
		base.Register("NotoQuery", NotoQuery_OnCommand);
		base.Register("HouseLevel", HouseLevel_OnCommand);
		base.Register("AutoPickup", AutoPickup_OnCommand);
		base.Register("Speed", Speed_OnCommand);
		base.Register("RunSpeed", RunSpeed_OnCommand);
		base.Register("WalkSpeed", WalkSpeed_OnCommand);
		base.Register("Follow", GMFollow_OnCommand);
		base.Register("DropTarg", DropTarg_OnCommand);
		base.Register("CancelStealth", CancelStealth_OnCommand);
		base.Register("Give", Give_OnCommand);
		base.Register("FPS", FPS_OnCommand);
		base.Register("Restock", delegate
		{
			Player.Current.RestockAgent.Invoke();
		});
		base.Register("Lootbag", delegate
		{
			TargetManager.Client = new SetRestockTargetTargetHandler(invoking: false);
			Engine.AddTextMessage("Target your lootbag.");
		});
		base.Register("ClearMoves", delegate
		{
			if (ActionContext.Queued.Count > 0)
			{
				ActionContext.Clear();
				Engine.AddTextMessage("Action queue cleared.");
			}
			else
			{
				Engine.AddTextMessage("Action queue is already empty.");
			}
		});
		base.Register("Organize", delegate
		{
			Player.Current.OrganizeAgent.Invoke();
		});
		base.Register("SetOrganize", delegate
		{
			TargetManager.Client = new SetOrganizeTemplateTargetHandler(invoking: false);
			Engine.AddTextMessage("Target your template lootbag.");
		});
		base.Register("LeapFrog", delegate
		{
			Gump drag = Gumps.Drag;
			if (drag is GDraggedItem)
			{
				Engine.m_LeapFrog = ((GDraggedItem)drag).Item;
				Engine.AddTextMessage("Leapfrog item set.");
			}
			else
			{
				Engine.m_LeapFrog = null;
				Engine.AddTextMessage("You are not holding an item. Leapfrog item cleared.");
			}
		});
		base.Register("AssembleShaders", delegate
		{
			ShaderData.AssembleAndDumpShaders();
		});
		base.Register("RenamePet", delegate
		{
			TargetManager.Client = new RenameTargetHandler();
			Engine.AddTextMessage("Which creature do you wish to rename?");
		});
	}

	private void Recall_OnCommandEx(CommandArgs args)
	{
		ClientFormatEx.Recall(args.GetArgument(0), isGate: false);
	}

	private void OpenRunebooksEx(CommandArgs args)
	{
		ClientFormatEx.OpenRunebooks();
	}

	private void Gate_OnCommandEx(CommandArgs args)
	{
		ClientFormatEx.Recall(args.GetArgument(0), isGate: true);
	}
}
