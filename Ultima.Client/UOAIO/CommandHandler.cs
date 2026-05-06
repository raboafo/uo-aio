namespace UOAIO;

public sealed class CommandHandler
{
	private string m_Name;

	private bool m_Solitary;

	private CommandCallback m_Callback;

	public string Name => this.m_Name;

	public bool Solitary => this.m_Solitary;

	public CommandCallback Callback => this.m_Callback;

	public CommandHandler(string name, bool solitary, CommandCallback callback)
	{
		this.m_Name = name;
		this.m_Solitary = solitary;
		this.m_Callback = callback;
	}
}
