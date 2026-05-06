using System.Collections.Generic;

namespace UOAIO;

public abstract class PhysicalAgent : Agent, IEntity, IPoint3D, IPoint2D
{
	private IContainerView _containerView;

	private IAgentCell _viewportCell;

	public IContainerView ContainerView
	{
		get
		{
			return this._containerView;
		}
		set
		{
			this._containerView = null;
		}
	}

	public IAgentCell ViewportCell
	{
		get
		{
			return this._viewportCell;
		}
		set
		{
			this._viewportCell = value;
		}
	}

	public bool Visible
	{
		get
		{
			Agent worldRoot = base.WorldRoot;
			return worldRoot != null && World.InUpdateRange(worldRoot);
		}
	}

	public PhysicalAgent(int serial)
		: base(serial)
	{
	}

	protected override void OnParentChanged(Agent parent)
	{
		base.OnParentChanged(parent);
		this.RaiseUpdateEvents();
	}

	protected override void OnLocationChanged()
	{
		base.OnLocationChanged();
		this.RaiseUpdateEvents();
	}

	public IAgentCell AcquireViewportCell()
	{
		if (this._viewportCell == null)
		{
			this._viewportCell = this.CreateViewportCell();
		}
		return this._viewportCell;
	}

	protected abstract IAgentCell CreateViewportCell();

	public void SetContainerView(IContainerView containerView)
	{
		this._containerView = containerView;
	}

	protected override void OnChildAdded(Agent child)
	{
		base.OnChildAdded(child);
		if (!(child is Item))
		{
			return;
		}
		foreach (IAgentView agentView in this.GetAgentViews())
		{
			if (agentView is IContainerView containerView)
			{
				containerView.OnChildAdded((Item)child);
			}
		}
	}

	protected override void OnChildRemoved(Agent child)
	{
		base.OnChildRemoved(child);
		if (!(child is Item))
		{
			return;
		}
		foreach (IAgentView agentView in this.GetAgentViews())
		{
			if (agentView is IContainerView containerView)
			{
				containerView.OnChildRemoved((Item)child);
			}
		}
	}

	protected override void OnDeleted()
	{
		base.OnDeleted();
		foreach (IAgentView agentView in this.GetAgentViews())
		{
			agentView.OnAgentDeleted();
		}
	}

	protected virtual IEnumerable<IAgentView> GetAgentViews()
	{
		if (this._containerView != null)
		{
			yield return this._containerView;
		}
		if (this._viewportCell != null)
		{
			yield return this._viewportCell;
		}
	}

	protected void RaiseUpdateEvents()
	{
		foreach (IAgentView agentView in this.GetAgentViews())
		{
			agentView.OnAgentUpdated();
		}
		if (!(base.Parent is PhysicalAgent physicalAgent) || !(this is Item))
		{
			return;
		}
		foreach (IAgentView agentView2 in physicalAgent.GetAgentViews())
		{
			if (agentView2 is IContainerView containerView)
			{
				containerView.OnChildUpdated((Item)this);
			}
		}
	}
}
