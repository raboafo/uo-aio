namespace UOAIO.Prompts;

public class ConfigurationPrompt : IPrompt
{
	private GPropertyEntry m_PropertyEntry;

	public ConfigurationPrompt(GPropertyEntry e)
	{
		this.m_PropertyEntry = e;
		string message = $"Please enter a new value for \"{e.Entry.Optionable.Name}\"";
		Engine.AddTextMessage(message, 12.5f);
	}

	public void OnReturn(string message)
	{
		if (!int.TryParse(message, out var result))
		{
			Engine.AddTextMessage("Configuration change could not be parsed.", 12.5f);
			return;
		}
		this.m_PropertyEntry.SetValue(result);
		Engine.AddTextMessage("Configuration changed.", 12.5f);
	}

	public void OnCancel(PromptCancelType type)
	{
		if (type == PromptCancelType.UserCancel)
		{
			Engine.AddTextMessage("Configuration unchanged.", 12.5f);
		}
	}
}
