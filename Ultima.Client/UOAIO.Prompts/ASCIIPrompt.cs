namespace UOAIO.Prompts;

public class ASCIIPrompt : IPrompt
{
	private int m_Serial;

	private int m_Prompt;

	private string m_Text;

	public int Serial => this.m_Serial;

	public int Prompt => this.m_Prompt;

	public string Text => this.m_Text;

	public ASCIIPrompt(int serial, int prompt, string text)
	{
		this.m_Serial = serial;
		this.m_Prompt = prompt;
		if ((this.m_Text = this.Text) != null && (this.m_Text = this.m_Text.Trim()).Length > 0)
		{
			Engine.AddTextMessage(this.m_Text, 12.5f);
		}
		else
		{
			this.m_Text = "";
		}
	}

	public void OnReturn(string message)
	{
		Network.Send(new PPrompt_Reply_ASCII(this.m_Serial, this.m_Prompt, message));
	}

	public void OnCancel(PromptCancelType type)
	{
		if (type == PromptCancelType.UserCancel)
		{
			Network.Send(new PPrompt_Cancel_ASCII(this.m_Serial, this.m_Prompt));
		}
	}
}
