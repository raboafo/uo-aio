using System;
using System.Collections.Generic;

namespace UOAIO;

public class ActionNode : IComparable
{
	private string m_Name;

	private List<ActionNode> m_Nodes;

	private List<ActionHandler> m_Handlers;

	public string Name => this.m_Name;

	public List<ActionNode> Nodes => this.m_Nodes;

	public List<ActionHandler> Handlers => this.m_Handlers;

	public ActionNode GetNode(string name)
	{
		for (int i = 0; i < this.m_Nodes.Count; i++)
		{
			ActionNode actionNode = this.m_Nodes[i];
			if (actionNode.m_Name == name)
			{
				return actionNode;
			}
		}
		return null;
	}

	public ActionHandler GetHandler(string action)
	{
		for (int i = 0; i < this.m_Handlers.Count; i++)
		{
			ActionHandler actionHandler = this.m_Handlers[i];
			if (actionHandler.Action == action)
			{
				return actionHandler;
			}
		}
		return null;
	}

	public ActionNode(string name)
	{
		this.m_Name = name;
		this.m_Nodes = new List<ActionNode>();
		this.m_Handlers = new List<ActionHandler>();
	}

	public int CompareTo(object obj)
	{
		return this.m_Name.CompareTo(((ActionNode)obj).m_Name);
	}
}
