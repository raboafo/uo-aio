namespace UOAIO.Prompts;

public interface IPrompt
{
	void OnReturn(string message);

	void OnCancel(PromptCancelType type);
}
