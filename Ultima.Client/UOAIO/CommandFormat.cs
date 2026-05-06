using System;
using System.Collections.Generic;
using UOAIO.Profiles;

namespace UOAIO;

public abstract class CommandFormat : SpeechFormat
{
	private Dictionary<string, CommandHandler> m_Commands;

	public CommandFormat(string prepend, string prefix, string format, byte messageType, SpeechType speechType)
		: base(prepend, prefix, format, messageType, speechType)
	{
		this.m_Commands = new Dictionary<string, CommandHandler>(StringComparer.OrdinalIgnoreCase);
	}

	public void Register(string name, CommandCallback callback)
	{
		this.Register(name, solitary: false, callback);
	}

	protected void Register(string name, bool solitary, CommandCallback callback)
	{
		this.m_Commands[name] = new CommandHandler(name, solitary, callback);
	}

	protected virtual void OnDefault(CommandArgs args)
	{
	}

	public override void Invoke(string text)
	{
		Mobile player = World.Player;
		if (player == null)
		{
			return;
		}
		CommandArgs commandArgs = new CommandArgs(player, text);
		CommandHandler value = null;
		if (this.m_Commands.TryGetValue(commandArgs.GetString(0), out value))
		{
			commandArgs.Step++;
			if (!value.Solitary || commandArgs.Length == 0)
			{
				value.Callback(commandArgs);
				if (!commandArgs.GoDefault)
				{
					return;
				}
			}
			commandArgs.Step--;
		}
		this.OnDefault(commandArgs);
	}
}
