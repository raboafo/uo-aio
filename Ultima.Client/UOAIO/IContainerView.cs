namespace UOAIO;

public interface IContainerView : IAgentView
{
	void OnChildAdded(Item added);

	void OnChildRemoved(Item removed);

	void OnChildUpdated(Item refreshed);
}
