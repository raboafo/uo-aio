using System;

namespace UOAIO.Targeting;

internal class TargetActions
{
	private static TargetAction m_Lookahead;

	private static DateTime m_Creation;

	public static TimeSpan MarginOfError;

	public static TargetAction Lookahead
	{
		get
		{
			return TargetActions.m_Lookahead;
		}
		set
		{
			TargetActions.m_Lookahead = value;
			TargetActions.m_Creation = DateTime.Now;
		}
	}

	public static void Identify()
	{
		ServerTargetHandler server = TargetManager.Server;
		if (TargetActions.m_Lookahead != TargetAction.Unknown)
		{
			if (TargetActions.m_Creation + TargetActions.MarginOfError > DateTime.Now && server != null && server.Aggression == TargetActions.GetFlags(TargetActions.m_Lookahead))
			{
				server.Action = TargetActions.m_Lookahead;
			}
			else
			{
				server.Action = TargetAction.Unknown;
			}
			TargetActions.m_Lookahead = TargetAction.Unknown;
		}
		else if (server != null && server.Aggression == AggressionType.Defensive)
		{
			server.Action = TargetAction.GreaterHeal;
		}
		else if (server != null)
		{
			server.Action = TargetAction.Unknown;
		}
	}

	public static string GetName(TargetAction action)
	{
		if (action != TargetAction.Unknown)
		{
			switch (action)
			{
			case TargetAction.Bola:
				return "a bola";
			case TargetAction.Bandage:
				return "a bandage";
			case TargetAction.PurplePotion:
				return "an explosion potion";
			default:
			{
				string text = action.ToString();
				for (int i = 0; i < text.Length; i++)
				{
					if (i > 0 && text[i] >= 'A' && text[i] <= 'Z')
					{
						text = text.Insert(i++, " ");
					}
				}
				return text.ToLowerInvariant();
			}
			}
		}
		return null;
	}

	public static void Identify(TargetAction action)
	{
		ServerTargetHandler server = TargetManager.Server;
		if (server != null && server.StartTime + TargetActions.MarginOfError > DateTime.Now && server.Aggression == TargetActions.GetFlags(action))
		{
			server.Action = action;
		}
		else if (server != null)
		{
			server.Action = TargetAction.Unknown;
		}
		TargetActions.m_Lookahead = TargetAction.Unknown;
	}

	public static AggressionType GetFlags(TargetAction action)
	{
		if (action >= TargetAction.DetectHidden)
		{
			return AggressionType.Neutral;
		}
		if (action >= TargetAction.Bola)
		{
			return AggressionType.Offensive;
		}
		if (action >= TargetAction.Bandage)
		{
			return AggressionType.Defensive;
		}
		switch (action)
		{
		case TargetAction.Clumsy:
		case TargetAction.Feeblemind:
		case TargetAction.MagicArrow:
		case TargetAction.Weaken:
		case TargetAction.Harm:
		case TargetAction.Fireball:
		case TargetAction.Poison:
		case TargetAction.Curse:
		case TargetAction.Lightning:
		case TargetAction.ManaDrain:
		case TargetAction.MindBlast:
		case TargetAction.Paralyze:
		case TargetAction.Dispel:
		case TargetAction.EnergyBolt:
		case TargetAction.Explosion:
		case TargetAction.FlameStrike:
		case TargetAction.ManaVampire:
			return AggressionType.Offensive;
		case TargetAction.Heal:
		case TargetAction.Agility:
		case TargetAction.Cunning:
		case TargetAction.Cure:
		case TargetAction.Strength:
		case TargetAction.Bless:
		case TargetAction.ArchCure:
		case TargetAction.ArchProtection:
		case TargetAction.GreaterHeal:
		case TargetAction.Invisibility:
		case TargetAction.Resurrection:
			return AggressionType.Defensive;
		default:
			return AggressionType.Neutral;
		}
	}

	static TargetActions()
	{
		TargetActions.MarginOfError = TimeSpan.FromSeconds(2.5);
	}
}
