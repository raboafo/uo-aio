namespace UOAIO.Prompts;

public class RenameRequestPrompt : IPrompt
{
	private Mobile m_Mobile;

	public RenameRequestPrompt(Mobile m)
	{
		this.m_Mobile = m;
		string message = $"Please enter a new name for \"{m.Name}\"";
		Engine.AddTextMessage(message, 12.5f);
	}

	public void OnReturn(string message)
	{
		Network.Send(new PRenameMobile(this.m_Mobile.Serial, message));
		Engine.AddTextMessage("Rename request sent.", 12.5f);
	}

	public void OnCancel(PromptCancelType type)
	{
		if (type == PromptCancelType.UserCancel)
		{
			Engine.AddTextMessage("Rename request canceled.", 12.5f);
		}
	}
}
