namespace UOAIO;

public interface IParticle
{
	bool Slice();

	void Destroy();

	void Invalidate();

	bool Offset(int xDelta, int yDelta);
}
