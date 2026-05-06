using System;
using System.Collections;
using System.Collections.Generic;

namespace UOAIO;

public class MessageManager
{
	private static List<IMessage> m_Messages;

	private static Queue m_ToRemove;

	private static int m_yStack;

	public static int yStack
	{
		get
		{
			return MessageManager.m_yStack;
		}
		set
		{
			MessageManager.m_yStack = value;
		}
	}

	static MessageManager()
	{
		MessageManager.m_Messages = new List<IMessage>();
		MessageManager.m_ToRemove = new Queue();
	}

	public static void Remove(IMessage m)
	{
		MessageManager.m_ToRemove.Enqueue(m);
		Gumps.Invalidated = true;
	}

	public static void AddMessage(IMessage m)
	{
		Gumps.Desktop.Children.Add((Gump)m);
		MessageManager.m_Messages.Insert(0, m);
		Gumps.Invalidated = true;
		if (m is GDynamicMessage)
		{
			int num = 0;
			IMessageOwner owner = ((GDynamicMessage)m).Owner;
			int count = MessageManager.m_Messages.Count;
			for (int i = 0; i < count; i++)
			{
				if (MessageManager.m_Messages[i] is GDynamicMessage && ((GDynamicMessage)MessageManager.m_Messages[i]).Owner == owner)
				{
					if (num >= 3 && !((GDynamicMessage)MessageManager.m_Messages[i]).Unremovable)
					{
						MessageManager.Remove(MessageManager.m_Messages[i]);
					}
					num++;
				}
			}
		}
		else
		{
			if (!(m is GSystemMessage))
			{
				return;
			}
			GSystemMessage gSystemMessage = (GSystemMessage)m;
			DateTime dateTime = DateTime.Now - TimeSpan.FromSeconds(1.0);
			int count2 = MessageManager.m_Messages.Count;
			for (int j = 1; j < count2; j++)
			{
				if (MessageManager.m_Messages[j] is GSystemMessage)
				{
					GSystemMessage gSystemMessage2 = (GSystemMessage)MessageManager.m_Messages[j];
					if (gSystemMessage2.OrigText == gSystemMessage.Text && (j == 1 || gSystemMessage2.UpdateTime >= dateTime))
					{
						gSystemMessage.DupeCount = gSystemMessage2.DupeCount + 1;
						MessageManager.Remove(gSystemMessage2);
						break;
					}
				}
			}
		}
	}

	public static void ClearMessages(IMessageOwner owner)
	{
		int count = MessageManager.m_Messages.Count;
		for (int i = 0; i < count; i++)
		{
			IMessage message = MessageManager.m_Messages[i];
			if (message is GDynamicMessage && ((GDynamicMessage)message).Owner == owner)
			{
				MessageManager.Remove(message);
			}
		}
	}

	private static void RecurseProcessItemGumps(Gump g, int x, int y, bool isItemGump)
	{
		if (isItemGump)
		{
			IItemGump itemGump = (IItemGump)g;
			Item item = itemGump.Item;
			item.MessageX = x + itemGump.xOffset;
			item.MessageY = y + itemGump.yOffset;
			item.BottomY = y + itemGump.yBottom;
			item.MessageFrame = Renderer.m_ActFrames;
			Gump desktop = Gumps.Desktop;
			GumpList children = desktop.Children;
			Gump gump = g;
			while (gump.Parent != desktop)
			{
				gump = gump.Parent;
			}
			int num = children.IndexOf(gump);
			for (int i = 0; i < MessageManager.m_Messages.Count; i++)
			{
				if (MessageManager.m_Messages[i] is GDynamicMessage && ((GDynamicMessage)MessageManager.m_Messages[i]).Owner == item)
				{
					int num2 = children.IndexOf((Gump)MessageManager.m_Messages[i]);
					if (num2 < num && num2 >= 0)
					{
						children.RemoveAt(num2);
						num = children.IndexOf(gump);
						children.Insert(num + 1, (Gump)MessageManager.m_Messages[i]);
					}
				}
			}
			return;
		}
		Gump[] array = g.Children.ToArray();
		foreach (Gump gump2 in array)
		{
			if (gump2 is IItemGump)
			{
				MessageManager.RecurseProcessItemGumps(gump2, x + gump2.X, y + gump2.Y, isItemGump: true);
			}
			else if (gump2.Children.Count > 0)
			{
				MessageManager.RecurseProcessItemGumps(gump2, x + gump2.X, y + gump2.Y, isItemGump: false);
			}
		}
	}

	public static void BeginRender()
	{
		while (MessageManager.m_ToRemove.Count > 0)
		{
			object obj = MessageManager.m_ToRemove.Dequeue();
			MessageManager.m_Messages.Remove((IMessage)obj);
			Gumps.Destroy((Gump)obj);
		}
		MessageManager.m_yStack = Engine.GameY + Engine.GameHeight - 22;
		MessageManager.RecurseProcessItemGumps(Gumps.Desktop, 0, 0, isItemGump: false);
		for (int i = 0; i < MessageManager.m_Messages.Count; i++)
		{
			MessageManager.m_Messages[i].OnBeginRender();
		}
		while (MessageManager.m_ToRemove.Count > 0)
		{
			object obj2 = MessageManager.m_ToRemove.Dequeue();
			MessageManager.m_Messages.Remove((IMessage)obj2);
			Gumps.Destroy((Gump)obj2);
		}
		if (Gumps.Invalidated)
		{
			if (Engine.m_LastMouseArgs != null)
			{
				Engine.MouseMove(Engine.m_Display, Engine.m_LastMouseArgs);
				Engine.MouseMoveQueue();
			}
			Gumps.Invalidated = false;
		}
	}
}
