namespace UOAIO;

public struct AnimData
{
	public unsafe sbyte* pvFrames;

	public byte unknown;

	public byte frameCount;

	public byte frameInterval;

	public byte frameStartInterval;

	public unsafe sbyte this[int index] => this.pvFrames[index & 0x3F];
}
