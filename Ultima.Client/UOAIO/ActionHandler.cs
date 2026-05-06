using System;
using System.Collections.Generic;

namespace UOAIO;

public class ActionHandler : IComparable
{
	private string m_Action;

	private string m_Name;

	private ParamNode[] m_Params;

	private ActionCallback m_Callback;

	private static Dictionary<string, ActionHandler> m_Table;

	private static List<ActionHandler> m_List;

	private static ActionNode m_RootNode;

	public string Action => this.m_Action;

	public string Name => this.m_Name;

	public ParamNode[] Params => this.m_Params;

	public ActionCallback Callback => this.m_Callback;

	public static Dictionary<string, ActionHandler> Table
	{
		get
		{
			if (ActionHandler.m_Table == null)
			{
				ActionHandler.m_Table = new Dictionary<string, ActionHandler>();
			}
			return ActionHandler.m_Table;
		}
	}

	public static List<ActionHandler> List
	{
		get
		{
			if (ActionHandler.m_List == null)
			{
				ActionHandler.m_List = new List<ActionHandler>();
			}
			return ActionHandler.m_List;
		}
	}

	public static ActionNode Root
	{
		get
		{
			if (ActionHandler.m_RootNode == null)
			{
				ActionHandler.m_RootNode = new ActionNode("-root-");
			}
			return ActionHandler.m_RootNode;
		}
	}

	private ActionHandler(string action, string name, ParamNode[] parms, ActionCallback callback)
	{
		this.m_Action = action;
		this.m_Name = name;
		this.m_Params = parms;
		this.m_Callback = callback;
	}

	public static void Register(string action, ParamNode[] parms, ActionCallback callback)
	{
		if (ActionHandler.m_Table == null)
		{
			ActionHandler.m_Table = new Dictionary<string, ActionHandler>();
		}
		if (ActionHandler.m_List == null)
		{
			ActionHandler.m_List = new List<ActionHandler>();
		}
		if (ActionHandler.m_RootNode == null)
		{
			ActionHandler.m_RootNode = new ActionNode("-root-");
		}
		string[] array = action.Split('|');
		ActionNode actionNode = ActionHandler.m_RootNode;
		for (int i = 0; i < array.Length - 1; i++)
		{
			ActionNode actionNode2 = actionNode.GetNode(array[i]);
			if (actionNode2 == null)
			{
				actionNode.Nodes.Add(actionNode2 = new ActionNode(array[i]));
				actionNode.Nodes.Sort();
			}
			actionNode = actionNode2;
		}
		action = array[array.Length - 1];
		int num = action.IndexOf('@');
		string name;
		if (num >= 0)
		{
			name = action.Substring(num + 1);
			action = action.Substring(0, num);
		}
		else
		{
			name = action;
		}
		ActionHandler actionHandler = new ActionHandler(action, name, parms, callback);
		actionNode.Handlers.Add(actionHandler);
		actionNode.Handlers.Sort();
		ActionHandler.m_Table[action] = actionHandler;
		ActionHandler.m_List.Add(actionHandler);
	}

	public static ActionHandler Find(string action)
	{
		if (ActionHandler.m_Table == null)
		{
			return null;
		}
		ActionHandler value = null;
		ActionHandler.m_Table.TryGetValue(action, out value);
		return value;
	}

	public int CompareTo(object obj)
	{
		return this.m_Name.CompareTo(((ActionHandler)obj).m_Name);
	}
}
