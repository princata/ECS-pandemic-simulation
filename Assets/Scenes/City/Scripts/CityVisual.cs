using TileMapEnum;
using UnityEngine;


//
public class CityVisual : MonoBehaviour
{
    private Grid<GridNode> grid;
    private Mesh mesh;
   

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void SetGrid(Grid<GridNode> grid)
    {
        this.grid = grid;
        UpdateVisual();

    }

    
    //QUESTA FUNZIONE SERVE SOLTANTO PER COSTRUIRE LA MESH
    private void UpdateVisual()
    {
        //create mesh for the whole map
        MeshUtils.CreateEmptyMeshArrays(grid.GetWidth() * grid.GetHeight(), out Vector3[] vertices, out Vector2[] uv, out int[] triangles);
        //create proper color for each cell
        for (int i = 0; i < grid.GetWidth(); i++)
        {
            for (int j = 0; j < grid.GetHeight(); j++)
            {
                int index = i * grid.GetHeight() + j;
                Vector3 quadsize = new Vector3(1, 1) * grid.GetCellSize();
                //inserire controllo se il gridobject presenta più piani
                int value = grid.GetGridObject(i, j).GetFirstFloors();

                Vector2 gridValueUV00, gridValueUV11;

                
                //based on type of cell, decide color
                if (value == (int)TileMapSprite.RoadHorizontal)
                {
                    gridValueUV00 = new Vector2(1f / 3f, 3f / 5f);
                    gridValueUV11 = new Vector2(2f / 3f, 4f / 5f);
                    //gridValueUV00 = new Vector2(1f / 3f, 1f / 2f);
                    //gridValueUV11 = new Vector2(2f / 3f, 3f / 4f);
                }
                else if (value == (int)TileMapSprite.RoadVertical)
                {
                    gridValueUV00 = new Vector2(0, 3f / 5f);
                    gridValueUV11 = new Vector2(1f / 3f, 4f / 5f);
                    //gridValueUV00 = new Vector2(0, 1f / 2f);
                    //gridValueUV11 = new Vector2(1f / 3f, 3f / 4f);
                }
                else if (value == (int)TileMapSprite.RoadCrossing)
                {
                    gridValueUV00 = new Vector2(0, 2f / 5f);
                    gridValueUV11 = new Vector2(1f / 3f, 3f / 5f);
                    //gridValueUV00 = new Vector2(0, 1f / 4f);
                    //gridValueUV11 = new Vector2(1f / 3f, 1f / 2f);
                }
                else if (value == (int)TileMapSprite.Park)
                {
                    gridValueUV00 = new Vector2(1f / 3f, 4f / 5f);
                    gridValueUV11 = new Vector2(2f / 3f, 1);
                    //gridValueUV00 = new Vector2(1f / 3f, 3f / 4f);
                    //gridValueUV11 = new Vector2(2f / 3f, 1);
                }
                else if (value == (int)TileMapSprite.Home)
                {
                    gridValueUV00 = new Vector2(2f / 3f, 2f / 5f);
                    gridValueUV11 = new Vector2(1, 3f / 5f);
                    //gridValueUV00 = new Vector2(2f / 3f, 1f / 4f);
                    //gridValueUV11 = new Vector2(1, 1f / 2f);
                }
                else if (value == (int)TileMapSprite.OAhome)
                {
                    gridValueUV00 = new Vector2(2f / 3f, 3f / 5f);
                    gridValueUV11 = new Vector2(1, 4f / 5f);
                    //gridValueUV00 = new Vector2(2f / 3f, 1f / 2f);
                    //gridValueUV11 = new Vector2(1, 3f / 4f);
                }
                else if (value == (int)TileMapSprite.Pub)
                {
                    gridValueUV00 = new Vector2(0, 4f / 5f);
                    gridValueUV11 = new Vector2(1f / 3f, 1);
                    //gridValueUV00 = new Vector2(0, 3f / 4f);
                    //gridValueUV11 = new Vector2(1f / 3f, 1);
                }
                else if (value == (int)TileMapSprite.Supermarket)
                {
                    gridValueUV00 = new Vector2(1f / 3f, 2f / 5f);
                    gridValueUV11 = new Vector2(2f / 3f, 3f / 5f);
                    //gridValueUV00 = new Vector2(1f / 3f, 1f / 4f);
                    //gridValueUV11 = new Vector2(2f / 3f, 1f / 2f);
                }
                else if (value == (int)TileMapSprite.Office)
                {
                    gridValueUV00 = new Vector2(2f / 3f, 4f / 5f);
                    gridValueUV11 = new Vector2(1, 1);
                    //gridValueUV00 = new Vector2(2f / 3f, 3f / 4f);
                    //gridValueUV11 = new Vector2(1, 1);
                }
                else if (value == (int)TileMapSprite.Hospital)
                {
                    gridValueUV00 = new Vector2(0, 1f / 5f);
                    gridValueUV11 = new Vector2(1f / 3f, 2f / 5f);
                    //gridValueUV00 = new Vector2(0, 0);
                    //gridValueUV11 = new Vector2(1f / 3f, 1f / 4f);
                }
                else if (value == (int)TileMapSprite.Gym)
                {
                    gridValueUV00 = new Vector2(1f / 3f, 1f / 5f);
                    gridValueUV11 = new Vector2(2f / 3f, 2f / 5f);
                    //gridValueUV00 = new Vector2(1f / 3f, 0);
                    //gridValueUV11 = new Vector2(2f / 3f, 1f / 4f);
                }
                else if (value == (int)TileMapSprite.School)
                {
                    gridValueUV00 = new Vector2(2f / 3f, 1f / 5f);
                    gridValueUV11 = new Vector2(1, 2f / 5f);
                    //gridValueUV00 = new Vector2(2f / 3f, 0);
                    //gridValueUV11 = new Vector2(1, 1f / 4f);
                }
                else if (value == (int)TileMapSprite.Impassable)
                {
                    gridValueUV00 = new Vector2(0f, 0f);
                    gridValueUV11 = new Vector2(1 / 3f, 1f / 5f);
                    //gridValueUV00 = new Vector2(2f / 3f, 0);
                    //gridValueUV11 = new Vector2(1, 1f / 4f);
                }
                else if (value == (int)TileMapSprite.Walkable)
                {
                    gridValueUV00 = new Vector2(1f / 3f, 0f);
                    gridValueUV11 = new Vector2(2f / 3f, 1f / 5f);
                    //gridValueUV00 = new Vector2(2f / 3f, 0);
                    //gridValueUV11 = new Vector2(1, 1f / 4f);
                }
                else 
                {//Park by default
                    gridValueUV00 = new Vector2(1f / 3f, 4f / 5f);
                    gridValueUV11 = new Vector2(2f / 3f, 1);
                    //gridValueUV00 = new Vector2(1f / 3f, 3f / 4f);
                    //gridValueUV11 = new Vector2(2f / 3f, 1);
                }
                //set mesh created and position offset (center of the cell)
                MeshUtils.AddToMeshArrays(vertices, uv, triangles, index, grid.GetWorldPosition(i, j) + quadsize * 0.5f, 0f, quadsize, gridValueUV00, gridValueUV11);

            }
        }
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //per le mappe grandi con più di 100x100 tiles
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

    }


}
