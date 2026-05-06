using System.Collections.Generic;

namespace UOAIO;

public class VertexCachePool
{
	private Queue<VertexCache> m_Queue;

	public VertexCachePool()
	{
		this.m_Queue = new Queue<VertexCache>();
	}

	public VertexCache GetInstance()
	{
		VertexCache vertexCache;
		if (this.m_Queue.Count > 0)
		{
			vertexCache = this.m_Queue.Dequeue();
			vertexCache.Invalidate();
		}
		else
		{
			vertexCache = new VertexCache();
		}
		return vertexCache;
	}

	public void ReleaseInstance(VertexCache vc)
	{
		if (vc != null)
		{
			this.m_Queue.Enqueue(vc);
		}
	}
}
