using System;

namespace UOAIO;

public interface IVertexStorage
{
	ArraySegment<byte> Store(int vertexCount, int primitiveCount);
}
