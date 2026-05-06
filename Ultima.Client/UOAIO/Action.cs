namespace UOAIO;

public class Action
{
	private ActionData m_Data;

	private ActionHandler m_Action;

	public ActionData Data => this.m_Data;

	public ActionHandler Handler
	{
		get
		{
			return this.m_Action;
		}
		set
		{
			this.m_Action = value;
		}
	}

	public string Line
	{
		get
		{
			if (this.m_Data.Param != null && this.m_Data.Param.Length > 0)
			{
				return this.m_Data.Command + " " + this.m_Data.Param;
			}
			return this.m_Data.Command;
		}
	}

	public string Command
	{
		get
		{
			return this.m_Data.Command;
		}
		set
		{
			this.m_Data.Command = value;
		}
	}

	public string Param
	{
		get
		{
			return this.m_Data.Param;
		}
		set
		{
			this.m_Data.Param = value;
		}
	}

	public Action(string command, string param)
		: this(new ActionData(command, param))
	{
	}

	public Action(ActionData data)
	{
		this.m_Data = data;
		this.m_Action = ActionHandler.Find(this.m_Data.Command);
	}
}
