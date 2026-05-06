namespace UOAIO;

public class ParamNode
{
	private string m_Name;

	private string m_Param;

	private ParamNode[] m_Nodes;

	public string Name
	{
		get
		{
			return this.m_Name;
		}
		set
		{
			this.m_Name = value;
		}
	}

	public string Param
	{
		get
		{
			return this.m_Param;
		}
		set
		{
			this.m_Param = value;
		}
	}

	public ParamNode[] Nodes
	{
		get
		{
			return this.m_Nodes;
		}
		set
		{
			this.m_Nodes = value;
		}
	}

	public static ParamNode[] Toggle => new ParamNode[3]
	{
		new ParamNode("Toggle", ""),
		new ParamNode("On", "On"),
		new ParamNode("Off", "Off")
	};

	public static ParamNode[] Empty => new ParamNode[0];

	public static ParamNode[] Count(int start, int count, string format)
	{
		ParamNode[] array = new ParamNode[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = new ParamNode(string.Format(format, 1 + i), i.ToString());
		}
		return array;
	}

	public ParamNode(string name, string param)
		: this(name, param, null)
	{
	}

	public ParamNode(string name, ParamNode[] nodes)
		: this(name, null, nodes)
	{
	}

	public ParamNode(string name, string[] nodes)
		: this(name, null, new ParamNode[nodes.Length])
	{
		for (int i = 0; i < this.m_Nodes.Length; i++)
		{
			this.m_Nodes[i] = new ParamNode(nodes[i], nodes[i]);
		}
	}

	private ParamNode(string name, string param, ParamNode[] nodes)
	{
		this.m_Name = name;
		this.m_Param = param;
		this.m_Nodes = nodes;
	}
}
