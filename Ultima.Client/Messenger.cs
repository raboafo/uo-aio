using UOAIO;

public static class Messenger
{
	public enum MessageType
	{
		Generic,
		Error,
		Warning,
		Success,
		Information
	}

	public static void Info(string fmt, params object[] args)
	{
		Messenger.Print(MessageType.Information, fmt, args);
	}

	public static void Warn(string fmt, params object[] args)
	{
		Messenger.Print(MessageType.Warning, fmt, args);
	}

	public static void Error(string fmt, params object[] args)
	{
		Messenger.Print(MessageType.Error, fmt, args);
	}

	public static void Success(string fmt, params object[] args)
	{
		Messenger.Print(MessageType.Success, fmt, args);
	}

	public static void Generic(string fmt, params object[] args)
	{
		Messenger.Print(MessageType.Information, fmt, args);
	}

	private static void Print(MessageType messageType, string fmt, params object[] args)
	{
		fmt = "[VeritasUO] " + fmt;
		switch (messageType)
		{
		case MessageType.Generic:
			Engine.AddTextMessage(string.Format(fmt, args));
			break;
		case MessageType.Error:
			Engine.AddTextMessage(string.Format(fmt, args), Engine.DefaultFont, Hues.Load(34));
			break;
		case MessageType.Warning:
			Engine.AddTextMessage(string.Format(fmt, args), Engine.DefaultFont, Hues.Load(53));
			break;
		case MessageType.Success:
			Engine.AddTextMessage(string.Format(fmt, args), Engine.DefaultFont, Hues.Load(68));
			break;
		case MessageType.Information:
			Engine.AddTextMessage(string.Format(fmt, args), Engine.DefaultFont, Hues.Load(53));
			break;
		}
	}
}
