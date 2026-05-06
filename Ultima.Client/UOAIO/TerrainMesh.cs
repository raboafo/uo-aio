namespace UOAIO;

public abstract class TerrainMesh
{
	protected LandTile tile;

	protected TransformedColoredTextured[] _mesh;

	protected int _x;

	protected int _y;

	protected int _color;

	protected int _xMin;

	protected int _xMax;

	protected int _yMin;

	protected int _yMax;

	public TerrainMesh()
	{
	}

	public void Load(LandTile tile, Texture tex)
	{
		this.tile = tile;
		this._mesh = this.GetMesh(tile, tex);
		if (this._mesh.Length != 0)
		{
			this._yMax = int.MinValue;
			this._yMin = int.MaxValue;
			this._xMax = int.MinValue;
			this._xMin = int.MaxValue;
			for (int i = 0; i < this._mesh.Length; i++)
			{
				int num = (int)this._mesh[i].X;
				int num2 = (int)this._mesh[i].Y;
				if (num2 < this._yMin)
				{
					this._yMin = num2;
				}
				if (num2 > this._yMax)
				{
					this._yMax = num2;
				}
				if (num < this._xMin)
				{
					this._xMin = num;
				}
				if (num > this._xMax)
				{
					this._xMax = num;
				}
			}
		}
		this._color = 0;
	}

	protected abstract TransformedColoredTextured[] GetMesh(LandTile tile, Texture tex);

	protected abstract void UpdateMesh(LandTile tile, int baseColor);

	public void Update(LandTile tile, int baseColor)
	{
		if (this._color != baseColor)
		{
			this._color = baseColor;
			this.UpdateMesh(tile, baseColor);
		}
	}

	public unsafe void Render(int x, int y)
	{
		if (y + this._yMax < Engine.GameY || y + this._yMin > Engine.GameY + Engine.GameHeight || x + this._xMax < Engine.GameX || x + this._xMin > Engine.GameX + Engine.GameWidth)
		{
			return;
		}
		int num = this._mesh.Length;
		fixed (TransformedColoredTextured* mesh = this._mesh)
		{
			if (this._x != x || this._y != y)
			{
				int num2 = x - this._x;
				int num3 = y - this._y;
				for (int i = 0; i < num; i++)
				{
					mesh[i].X += num2;
					mesh[i].Y += num3;
				}
				this._x = x;
				this._y = y;
			}
			this.RenderAux(mesh);
		}
		if (Renderer.DrawGrid)
		{
			Renderer.Grid(this.tile, this.tile.x, this.tile.y, x, y);
		}
	}

	protected unsafe abstract void RenderAux(TransformedColoredTextured* pMesh);
}
