using UOAIO.Profiles;

namespace UOAIO;

internal class PartyFormat : CommandFormat
{
	public PartyFormat(string prepend, string prefix, string format, byte messageType, SpeechType speechType)
		: base(prepend, prefix, format, messageType, speechType)
	{
		base.Register("accept", solitary: true, Accept_OnCommand);
		base.Register("reject", solitary: true, Reject_OnCommand);
		base.Register("decline", solitary: true, Reject_OnCommand);
		base.Register("add", solitary: true, Add_OnCommand);
		base.Register("rem", solitary: true, Remove_OnCommand);
		base.Register("remove", solitary: true, Remove_OnCommand);
		base.Register("quit", solitary: true, Quit_OnCommand);
		base.Register("loot", solitary: false, Loot_OnCommand);
	}

	protected override void OnDefault(CommandArgs args)
	{
		switch (UOAIO.Party.State)
		{
		case PartyState.Alone:
			Engine.AddTextMessage($"Note to self: {args.GetArgument(0)}", Engine.DefaultFont, Hues.Load(946));
			break;
		case PartyState.Joining:
			Engine.AddTextMessage("Use '/accept' or '/decline'.", Engine.DefaultFont, Hues.Load(946));
			break;
		case PartyState.Joined:
		{
			Mobile mob;
			string text = this.GetText(args.GetArgument(0), out mob);
			if (mob == null)
			{
				Network.Send(new PParty_PublicMessage(text));
				break;
			}
			Network.Send(new PParty_PrivateMessage(mob, text));
			string name = args.Mobile.Name;
			if (name == null || (name = name.Trim()).Length == 0)
			{
				name = "You";
			}
			Engine.AddTextMessage($"<{name}> {text}", Engine.DefaultFont, Hues.Load(Preferences.Current.SpeechHues.Whisper));
			break;
		}
		}
	}

	private void Accept_OnCommand(CommandArgs args)
	{
		if (UOAIO.Party.State == PartyState.Joining)
		{
			Network.Send(new PParty_Accept(UOAIO.Party.Leader));
		}
		else
		{
			args.GoDefault = true;
		}
	}

	private void Reject_OnCommand(CommandArgs args)
	{
		if (UOAIO.Party.State == PartyState.Joining)
		{
			Network.Send(new PParty_Decline(UOAIO.Party.Leader));
		}
		else
		{
			args.GoDefault = true;
		}
	}

	private void Add_OnCommand(CommandArgs args)
	{
		if (UOAIO.Party.State == PartyState.Alone)
		{
			Network.Send(new PParty_AddMember());
		}
		else if (UOAIO.Party.State == PartyState.Joined && UOAIO.Party.IsLeader)
		{
			Network.Send(new PParty_AddMember());
		}
		else
		{
			args.GoDefault = true;
		}
	}

	private void Remove_OnCommand(CommandArgs args)
	{
		if (UOAIO.Party.State == PartyState.Joined && UOAIO.Party.IsLeader)
		{
			Network.Send(new PParty_RemoveMember());
		}
		else
		{
			args.GoDefault = true;
		}
	}

	private void Quit_OnCommand(CommandArgs args)
	{
		if (UOAIO.Party.State == PartyState.Joined)
		{
			Network.Send(new PParty_Quit());
		}
		else
		{
			args.GoDefault = true;
		}
	}

	private void Loot_OnCommand(CommandArgs args)
	{
		if (UOAIO.Party.State == PartyState.Joined)
		{
			if (args.Length > 0)
			{
				Network.Send(new PParty_SetCanLoot(args.GetBoolean(0)));
			}
			else
			{
				Engine.AddTextMessage("Use '/loot on' or '/loot off'.", Engine.DefaultFont, Hues.Load(946));
			}
		}
		else
		{
			args.GoDefault = true;
		}
	}

	protected string GetText(string text, out Mobile mob)
	{
		if (text.Length > 0 && char.IsDigit(text, 0) && UOAIO.Party.State == PartyState.Joined)
		{
			int num = text[0] - 49;
			if (num >= 0 && num < UOAIO.Party.Members.Length)
			{
				mob = UOAIO.Party.Members[num];
				if (mob != null && !mob.Player)
				{
					return text.Substring(1);
				}
			}
		}
		mob = null;
		return text;
	}

	public override string Mutate(string text, bool display)
	{
		text = base.Mutate(text, display: false);
		if (display)
		{
			text = this.GetText(text, out var mob);
			string text2 = "Party";
			if (mob != null)
			{
				text2 = mob.Name;
				if (text2 == null || (text2 = text2.Trim()).Length == 0)
				{
					text2 = "Someone";
				}
			}
			if (!display && base.m_Format != null)
			{
				text = string.Format(base.m_Format, text);
			}
			else if (display)
			{
				text = text2 + ": " + text + "_";
			}
		}
		return text;
	}
}
