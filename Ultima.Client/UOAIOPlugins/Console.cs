using UOAIO;

namespace UOAIOPlugins;

public static class Console
{
	public enum MessageType
	{
		Generic,
		Error,
		Warning,
		Success,
		Information
	}

	public static void Print(string text, MessageType messageType)
	{
		switch (messageType)
		{
		case MessageType.Generic:
			Engine.AddTextMessage($"[VeritasUO] {text}");
			break;
		case MessageType.Error:
			Engine.AddTextMessage($"[VeritasUO] {text}", Engine.DefaultFont, Hues.Load(34));
			break;
		case MessageType.Warning:
			Engine.AddTextMessage($"[VeritasUO] {text}", Engine.DefaultFont, Hues.Load(53));
			break;
		case MessageType.Success:
			Engine.AddTextMessage($"[VeritasUO] {text}", Engine.DefaultFont, Hues.Load(68));
			break;
		case MessageType.Information:
			Engine.AddTextMessage($"[VeritasUO] {text}", Engine.DefaultFont, Hues.Load(53));
			break;
		}
	}
}
