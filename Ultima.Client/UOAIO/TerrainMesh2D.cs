namespace UOAIO;

public sealed class TerrainMesh2D : TerrainMesh
{
	protected unsafe override TransformedColoredTextured[] GetMesh(LandTile tile, Texture tex)
	{
		TransformedColoredTextured[] array = VertexConstructor.Create(4);
		fixed (TransformedColoredTextured* ptr = array)
		{
			float num = -0.5f;
			float num2 = (float)(tile.Z * -4) - 0.5f;
			float x = (ptr[1].X = num + (float)tex.Width);
			ptr->X = x;
			x = (ptr[2].Y = num2 + (float)tex.Height);
			ptr->Y = x;
			TransformedColoredTextured* num5 = ptr + 1;
			x = (ptr[3].Y = num2);
			num5->Y = x;
			TransformedColoredTextured* num7 = ptr + 2;
			x = (ptr[3].X = num);
			num7->X = x;
			float maxTU = tex.MaxTU;
			float tv = (ptr->Tv = tex.MaxTV);
			ptr->Tu = maxTU;
			ptr[1].Tu = maxTU;
			ptr[2].Tv = tv;
		}
		return array;
	}

	protected unsafe override void RenderAux(TransformedColoredTextured* pMesh)
	{
		Renderer.DrawQuadPrecalc(pMesh);
	}

	protected unsafe override void UpdateMesh(LandTile tile, int baseColor)
	{
		fixed (TransformedColoredTextured* mesh = base._mesh)
		{
			mesh[3].Color = (mesh[2].Color = (mesh[1].Color = (mesh->Color = Renderer.GetQuadColor(baseColor))));
		}
	}
}
